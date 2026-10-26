using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class MapLoader : MonoSingleton<MapLoader>
{
	public delegate void LoadCallback(AssetBundle actBundle);

	public delegate void WorkshopLoadingState(bool loaded, bool error);

	public delegate void WorkshopUpdateInterrupt(PublishedFileId id);

	public delegate void LoadErrorCallback();

	[SerializeField]
	private string agonyVersion;

	[SerializeField]
	private GameObject helperCanvas;

	[SerializeField]
	private AudioMixerGroup masterMixer;

	[SerializeField]
	private AudioMixerGroup musicMixer;

	[SerializeField]
	private GameObject requiredMapObject;

	[SerializeField]
	private GameObject finalRoomPit;

	[Space]
	[SerializeField]
	private GameObject notificationPanel;

	[HideInInspector]
	public string currentCustomId;

	[HideInInspector]
	public ulong currentWorkshopId;

	public bool isCustomLoaded;

	public bool isCommonLoaded;

	private FileSystemWatcher watcher;

	private float mapUpdateCounter;

	private bool mapChanged;

	private string loadedBundlePath;

	private AssetBundle loadedBundle;

	private AssetBundleCreateRequest loadRequest;

	private bool customSecret;

	private List<string> debugLoadedBundles;

	private string queuedBundle;

	private PublishedFileId queuedWorkshopItem;

	private WorkshopLoadingState cachedWorkshopLoadingState;

	private LoadCallback queuedCallback;

#if UNITY_WEBGL
	// Cache-busting para que el navegador no reutilice un bundle viejo.
	private const string WebGLBundleQuery = "?v=10b1";
#endif

	public static bool CustomMapPlaying
	{
		get
		{
			if ((bool)MonoSingleton<MapLoader>.Instance)
			{
				return MonoSingleton<MapLoader>.Instance.isCustomLoaded;
			}
			return false;
		}
	}

	public static string CustomMapsPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Mods", "Maps");

	protected override void Awake()
	{
		if (MonoSingleton<MapLoader>.Instance != this && MonoSingleton<MapLoader>.Instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		base.Awake();
		debugLoadedBundles = new List<string>();
	}

	private void Start()
	{
		base.transform.SetParent(null);
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void SpawnFinalPitAndFinish()
	{
		FinalRoom finalRoom = Object.FindObjectOfType<FinalRoom>();
		if (finalRoom != null)
		{
			if ((bool)finalRoom.doorOpener)
			{
				finalRoom.doorOpener.SetActive(value: true);
			}
			MonoSingleton<NewMovement>.Instance.transform.position = finalRoom.dropPoint.position;
		}
		else
		{
			GameObject obj = Object.Instantiate(finalRoomPit);
			finalRoom = obj.GetComponent<FinalRoom>();
			obj.transform.position = new Vector3(50000f, -1000f, 50000f);
			MonoSingleton<NewMovement>.Instance.transform.position = finalRoom.dropPoint.position;
		}
	}

	public void DropLoadedBundle()
	{
		if ((bool)loadedBundle)
		{
			if (debugLoadedBundles.Contains(loadedBundle.name))
			{
				debugLoadedBundles.Remove(loadedBundle.name);
			}
			loadedBundle.Unload(unloadAllLoadedObjects: true);
		}
		isCustomLoaded = false;
		currentCustomId = string.Empty;
		currentWorkshopId = 0uL;
	}

	private void SubscribeToMapFile(string path)
	{
		watcher?.Dispose();
		watcher = new FileSystemWatcher(Path.GetDirectoryName(path))
		{
			NotifyFilter = (NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size),
			Filter = Path.GetFileName(Path.GetFileName(path))
		};
		Debug.Log("Subscribing to " + path);
		watcher.Changed += (object sender, FileSystemEventArgs args) =>
		{
			Debug.Log("Map Updated");
			mapUpdateCounter = 0f;
			mapChanged = true;
		};
		watcher.EnableRaisingEvents = true;
	}

	private void ShowMapChangeNotif()
	{
		notificationPanel.SetActive(value: true);
	}

	private void HideMapChangeNotif()
	{
		Debug.Log("Notification Hide");
		mapChanged = false;
		notificationPanel.SetActive(value: false);
	}

	private void Update()
	{
		if (isCustomLoaded && mapChanged)
		{
			mapUpdateCounter += Time.deltaTime;
			ShowMapChangeNotif();
			if (Input.GetKeyDown(KeyCode.Return))
			{
				LoadAuto(loadedBundlePath);
				HideMapChangeNotif();
			}
			if (mapUpdateCounter > 7f)
			{
				HideMapChangeNotif();
			}
		}
	}

	private void OnGUI()
	{
		if (!Debug.isDebugBuild)
		{
			return;
		}
		int num = 0;
		if (isCustomLoaded)
		{
			GUI.Label(new Rect(0f, num * 20, 500f, 50f), "Agony [" + agonyVersion + "]");
			num++;
		}
		if (currentWorkshopId != 0L)
		{
			GUI.Label(new Rect(0f, num * 20, 500f, 50f), $"WorkshopId: {currentWorkshopId}");
			num++;
		}
		if (debugLoadedBundles != null && debugLoadedBundles.Count != 0)
		{
			GUI.Label(new Rect(0f, num * 20, 500f, 50f), "external: " + isCustomLoaded);
		}
		num++;
		foreach (string debugLoadedBundle in debugLoadedBundles)
		{
			GUI.Label(new Rect(0f, num * 20, 500f, 50f), debugLoadedBundle);
			num++;
		}
	}

	private new void OnEnable()
	{
		SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
	}

	public static bool CheckForAct(string act)
	{
#if UNITY_WEBGL
		// En WebGL los bundles se sirven por HTTP; File.Exists no aplica.
		return true;
#else
		return File.Exists(Path.Combine(Application.streamingAssetsPath, "acts", act));
#endif
	}

	private void LoadWorkshopMapFromPath(string path)
	{
		mapChanged = false;
		EnsureCommonIsLoaded();
		MapDataRebuild mapDataRebuild = LoadMapData(path, withBundle: true);
		if (loadedBundle != null)
		{
			if (debugLoadedBundles.Contains(loadedBundle.name))
			{
				debugLoadedBundles.Remove(loadedBundle.name);
			}
			loadedBundle.Unload(unloadAllLoadedObjects: false);
		}
		isCustomLoaded = true;
		currentCustomId = mapDataRebuild.uniqueId;
		AssetBundle assetBundle = AssetBundle.LoadFromMemory(mapDataRebuild.bundle);
		debugLoadedBundles.Add(assetBundle.name);
		loadedBundle = assetBundle;
		SceneManager.LoadScene(Path.GetFileNameWithoutExtension(assetBundle.GetAllScenePaths()[0]));
		helperCanvas.SetActive(value: true);
	}

	public async Task<Item?> FetchWorkshopItemInfo(PublishedFileId id)
	{
		Item? result = await SteamUGC.QueryFileAsync(id);
		if (result.HasValue)
		{
			return result;
		}
		Debug.LogError("Invalid item Id");
		return null;
	}

	public async void AutoLoadWorkshopMap(PublishedFileId id, WorkshopLoadingState stateChanged, WorkshopUpdateInterrupt workshopInterrupt, bool overrideWorkshopUpdate = false)
	{
		mapChanged = false;
		Item? item = await Item.GetAsync(id);
		if (!item.HasValue)
		{
			stateChanged(loaded: false, error: true);
			Debug.LogError("Invalid item Id");
			return;
		}
		Debug.Log("Title: " + item?.Title);
		Debug.Log($"IsInstalled: {item?.IsInstalled}");
		Debug.Log($"IsDownloading: {item?.IsDownloading}");
		Debug.Log($"IsDownloadPending: {item?.IsDownloadPending}");
		Debug.Log($"IsSubscribed: {item?.IsSubscribed}");
		Debug.Log($"NeedsUpdate: {item?.NeedsUpdate}");
		Debug.Log("Description: " + item?.Description);
		if ((item?.NeedsUpdate).Value && !overrideWorkshopUpdate && workshopInterrupt != null)
		{
			workshopInterrupt(id);
			return;
		}
		bool flag = item.Value.IsInstalled;
		if ((item?.NeedsUpdate).Value && !overrideWorkshopUpdate)
		{
			flag = false;
		}
		if (flag)
		{
			Debug.Log(item.Value.Directory);
			currentWorkshopId = id.Value;
			string[] files = Directory.GetFiles(item.Value.Directory);
			Debug.Log(files[0]);
			LoadWorkshopMapFromPath(files[0]);
			stateChanged(loaded: true, error: false);
		}
		else
		{
			Debug.Log("Updating workshop map: " + item?.Title);
			stateChanged(loaded: false, error: false);
			queuedWorkshopItem = id;
			cachedWorkshopLoadingState = stateChanged;
			SteamUGC.OnDownloadItemResult += SteamUGCOnOnDownloadItemResult;
			SteamUGC.Download(id);
		}
	}

	private void SteamUGCOnOnDownloadItemResult(Result obj)
	{
		if (obj == Result.OK)
		{
			SteamUGC.OnDownloadItemResult -= SteamUGCOnOnDownloadItemResult;
			AutoLoadWorkshopMap(queuedWorkshopItem, cachedWorkshopLoadingState, null, overrideWorkshopUpdate: true);
		}
	}

	/// <summary>
	/// Carga directa de un nivel (menu F3 de la pagina WebGL). Elige el bundle
	/// del acto segun el nombre del nivel.
	/// </summary>
	public void DebugLoadLevel(string levelName)
	{
		if (string.IsNullOrEmpty(levelName))
		{
			return;
		}
		// Escenas que viven en el player, no en los bundles.
		if (levelName == "Endless" || levelName == "Tutorial")
		{
			if (Application.CanStreamedLevelBeLoaded(levelName))
			{
				Debug.Log("[DebugLoadLevel] " + levelName + " -> escena del player");
				SceneManager.LoadScene(levelName);
			}
			else
			{
				Debug.LogError("[DebugLoadLevel] " + levelName + " no esta en el build del player");
			}
			return;
		}
		string act = "act-0";
		if (levelName.StartsWith("Level 1-") || levelName.StartsWith("Level 2-") || levelName.StartsWith("Level 3-"))
		{
			act = "act-1";
		}
		else if (levelName.StartsWith("Level 4-") || levelName == "EarlyAccessEnd")
		{
			act = "act-2";
		}
		else if (levelName.StartsWith("Level P-"))
		{
			act = "prime";
		}
		Debug.Log("[DebugLoadLevel] " + levelName + " -> bundle " + act);
		LoadAct(act, (AssetBundle bundle) =>
		{
			string[] scenes = (from p in bundle.GetAllScenePaths() select Path.GetFileNameWithoutExtension(p)).ToArray();
			if (!scenes.Contains(levelName))
			{
				Debug.LogError("[DebugLoadLevel] el bundle " + act + " no contiene " + levelName);
				return;
			}
			SceneManager.LoadSceneAsync(levelName);
		}, () =>
		{
			Debug.LogError("[DebugLoadLevel] no se pudo cargar el bundle " + act);
		});
	}

	/// <summary>
	/// Estado del menu F3 (checkboxes). Se envia a la pagina WebGL.
	/// </summary>
	public void DebugPushState()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		Application.ExternalCall("UKDebugReceiveState", GameProgressSaver.DebugGetState());
#endif
	}

	public void DebugSetLayerCompleted(string payload)
	{
		string[] array = payload.Split('|');
		if (array.Length == 2 && int.TryParse(array[0], out int layer))
		{
			GameProgressSaver.DebugSetLayerCompleted(layer, array[1] == "1");
			Debug.Log("[F3] Capa " + layer + ": " + (array[1] == "1" ? "completada" : "desmarcada"));
			DebugPushState();
		}
	}

	public void DebugSetPrimeCompleted(string payload)
	{
		GameProgressSaver.DebugSetPrimeCompleted(payload == "1");
		Debug.Log("[F3] Prime: " + (payload == "1" ? "completado" : "desmarcado"));
		DebugPushState();
	}

	public void DebugSetAllLevelsCompleted(string payload)
	{
		GameProgressSaver.DebugSetAllLevelsCompleted(payload == "1");
		Debug.Log("[F3] Todos los niveles: " + (payload == "1" ? "completados" : "reseteados"));
		DebugPushState();
	}

	public void DebugSetGearFamily(string payload)
	{
		string[] array = payload.Split('|');
		if (array.Length == 2)
		{
			GameProgressSaver.DebugSetGearFamily(array[0], array[1] == "1");
			Debug.Log("[F3] Arsenal " + array[0] + ": " + (array[1] == "1" ? "desbloqueado" : "bloqueado"));
			DebugPushState();
		}
	}

	public void DebugSetAllGear(string payload)
	{
		GameProgressSaver.DebugSetAllGear(payload == "1");
		Debug.Log("[F3] Todo el arsenal: " + (payload == "1" ? "desbloqueado" : "bloqueado"));
		DebugPushState();
	}

	public void LoadAct(string act, LoadCallback callback, LoadErrorCallback errorCallback)
	{
		mapChanged = false;
		EnsureCommonIsLoaded();
#if UNITY_WEBGL
		if ((bool)loadedBundle && loadedBundle.name == "acts/" + act)
		{
			Debug.Log("Act " + act + " already loaded");
			callback(loadedBundle);
			return;
		}
		if ((bool)loadedBundle)
		{
			if (debugLoadedBundles.Contains(loadedBundle.name))
			{
				debugLoadedBundles.Remove(loadedBundle.name);
			}
			loadedBundle.Unload(unloadAllLoadedObjects: false);
		}
		isCustomLoaded = false;
		currentCustomId = string.Empty;
		SmartLoadBundle(Application.streamingAssetsPath + "/acts/" + act + WebGLBundleQuery, callback, errorCallback);
#else
		if (!File.Exists(Path.Combine(Application.streamingAssetsPath, "acts", act)))
		{
			Debug.LogError(act + " is missing from streaming assets");
			errorCallback();
			return;
		}
		if (loadRequest != null && loadRequest.assetBundle.name != "acts/" + act)
		{
			Debug.Log("Interrupting " + loadRequest.assetBundle.name + " load to load acts/" + act);
			queuedBundle = Path.Combine(Application.streamingAssetsPath, "acts", act);
			return;
		}
		if ((bool)loadedBundle && loadedBundle.name == "acts/" + act)
		{
			Debug.Log("Act " + act + " already loaded");
			callback(loadedBundle);
			return;
		}
		if ((bool)loadedBundle)
		{
			if (debugLoadedBundles.Contains(loadedBundle.name))
			{
				debugLoadedBundles.Remove(loadedBundle.name);
			}
			loadedBundle.Unload(unloadAllLoadedObjects: false);
		}
		isCustomLoaded = false;
		currentCustomId = string.Empty;
		SmartLoadBundle(Path.Combine(Application.streamingAssetsPath, "acts", act), callback);
#endif
	}

	private void SmartLoadBundle(string bundle, LoadCallback callback, LoadErrorCallback errorCallback = null)
	{
#if UNITY_WEBGL
		StartCoroutine(LoadBundleWebGL(bundle, callback, errorCallback));
#else
		mapChanged = false;
		loadRequest = AssetBundle.LoadFromFileAsync(bundle);
		loadRequest.completed += (AsyncOperation operation) =>
		{
			debugLoadedBundles.Add(loadRequest.assetBundle.name);
			loadedBundle = loadRequest.assetBundle;
			if (!string.IsNullOrEmpty(queuedBundle))
			{
				if (debugLoadedBundles.Contains(loadedBundle.name))
				{
					debugLoadedBundles.Remove(loadedBundle.name);
				}
				loadedBundle.Unload(unloadAllLoadedObjects: false);
				SmartLoadBundle(queuedBundle, queuedCallback);
				queuedBundle = string.Empty;
				queuedCallback = null;
			}
			else
			{
				callback(loadRequest.assetBundle);
				loadRequest = null;
			}
		};
#endif
	}

#if UNITY_WEBGL
	private IEnumerator LoadBundleWebGL(string url, LoadCallback callback, LoadErrorCallback errorCallback)
	{
		Debug.Log("[WebGL] Loading bundle " + url);
		using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url))
		{
			yield return request.SendWebRequest();
			if (request.isNetworkError || request.isHttpError)
			{
				Debug.LogError("[WebGL] Error loading " + url + ": " + request.error);
				if (errorCallback != null)
				{
					errorCallback();
				}
				yield break;
			}
			AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
			if (bundle == null)
			{
				Debug.LogError("[WebGL] Null bundle " + url);
				if (errorCallback != null)
				{
					errorCallback();
				}
				yield break;
			}
			debugLoadedBundles.Add(bundle.name);
			loadedBundle = bundle;
			loadRequest = null;
			callback(bundle);
		}
	}
#endif

	public void EnsureCommonIsLoaded()
	{
#if UNITY_WEBGL
		// El bundle common va incluido en el player (StreamingAssets se sirve
		// por HTTP); no hay AssetBundle.LoadFromFile disponible.
		isCommonLoaded = true;
#else
		if (!isCommonLoaded)
		{
			Debug.Log("Loading the common bundle");
			AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "common"));
			debugLoadedBundles.Add("common");
			isCommonLoaded = true;
		}
#endif
	}

	public MapDataRebuild[] GetAvailableMaps(LocalSortMode sortMode)
	{
		Directory.CreateDirectory(CustomMapsPath);
		FileInfo[] array = new DirectoryInfo(CustomMapsPath).GetFiles("*");
		if (sortMode == LocalSortMode.Date)
		{
			array = array.OrderBy((FileInfo x) => x.LastWriteTime).Reverse().ToArray();
		}
		Debug.Log($"Found {array.Length} files in {CustomMapsPath}");
		array = array.Where((FileInfo f) => f.Extension == ".blood" || f.Extension == ".cum").ToArray();
		return array.Select((FileInfo file) => LoadMapData(file.FullName, withBundle: false)).ToArray();
	}

	public static MapDataRebuild LoadMapData(string file, bool withBundle)
	{
		FileStream fileStream = new FileStream(file, FileMode.Open);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		MapHeader mapHeader = JsonUtility.FromJson<MapHeader>(binaryReader.ReadString());
		byte[] data = binaryReader.ReadBytes(mapHeader.ThumbSize);
		Texture2D texture2D = null;
		if (mapHeader.ThumbSize != 0)
		{
			texture2D = new Texture2D(640, 480);
			texture2D.LoadImage(data);
			texture2D.Apply();
		}
		MapDataRebuild mapDataRebuild = new MapDataRebuild
		{
			uniqueId = mapHeader.UniqueIdentifier,
			author = mapHeader.Author,
			description = mapHeader.Description,
			mapName = mapHeader.Name,
			path = file,
			thumbnail = texture2D
		};
		if (withBundle)
		{
			mapDataRebuild.bundle = binaryReader.ReadBytes(mapHeader.BundleSize);
		}
		binaryReader.Close();
		fileStream.Close();
		return mapDataRebuild;
	}

	public void LoadAuto(string relativePath)
	{
		watcher?.Dispose();
		mapChanged = false;
		if (File.Exists(Path.Combine(CustomMapsPath, relativePath)))
		{
			EnsureCommonIsLoaded();
			MapDataRebuild mapDataRebuild = LoadMapData(Path.Combine(CustomMapsPath, relativePath), withBundle: true);
			if (loadedBundle != null)
			{
				if (debugLoadedBundles.Contains(loadedBundle.name))
				{
					debugLoadedBundles.Remove(loadedBundle.name);
				}
				loadedBundle.Unload(unloadAllLoadedObjects: false);
			}
			isCustomLoaded = true;
			currentWorkshopId = 0uL;
			currentCustomId = mapDataRebuild.uniqueId;
			AssetBundle assetBundle = AssetBundle.LoadFromMemory(mapDataRebuild.bundle);
			debugLoadedBundles.Add(assetBundle.name);
			loadedBundlePath = Path.Combine(CustomMapsPath, relativePath);
			SubscribeToMapFile(loadedBundlePath);
			loadedBundle = assetBundle;
			SceneManager.LoadScene(Path.GetFileNameWithoutExtension(assetBundle.GetAllScenePaths()[0]));
			helperCanvas.SetActive(value: true);
		}
		else
		{
			Debug.LogError("Tried to load an invalid map, " + relativePath);
		}
	}

	/// <summary>
	/// El player y los bundles llevan COPIAS distintas de los mixers. El intro
	/// del menu baja el AllAudio del player a -80 dB y solo lo devuelve a 0 al
	/// terminar; si se carga otra escena antes (p. ej. Endless desde el menu
	/// F3), el mixer del player se queda mudo. Lo restauramos al cargar escena.
	/// </summary>
	private void RestoreAllAudioMixer(string sceneName)
	{
		if (sceneName == "Main Menu")
		{
			// En el menu el volumen lo controla IntroTextController.
			return;
		}
		AudioMixerController mixerController = MonoSingleton<AudioMixerController>.Instance;
		if (!mixerController || !mixerController.allSound)
		{
			return;
		}
		float volume = 0f;
		float pitch = 1f;
		mixerController.allSound.GetFloat("allVolume", out volume);
		mixerController.allSound.GetFloat("allPitch", out pitch);
		mixerController.allSound.SetFloat("allVolume", 0f);
		mixerController.allSound.SetFloat("allPitch", 1f);
		if (volume < -0.5f || Mathf.Abs(pitch - 1f) > 0.01f)
		{
			Debug.Log("[Audio] " + sceneName + ": AllAudio allVolume " + volume.ToString("0.0") + " -> 0 dB, allPitch " + pitch.ToString("0.00") + " -> 1");
		}
	}

	private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode mode)
	{
		RestoreAllAudioMixer(scene.name);
		if (scene.name == "Main Menu" || scene.name == "Custom Content" || scene.name == "Endless")
		{
			watcher?.Dispose();
			HideMapChangeNotif();
			isCustomLoaded = false;
			currentCustomId = string.Empty;
		}
		else
		{
			if (!isCustomLoaded)
			{
				return;
			}
			MapInfo mapInfo = Object.FindObjectOfType<MapInfo>();
			if ((bool)mapInfo && mapInfo.renderSkybox)
			{
				MonoSingleton<CameraController>.Instance.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
			}
			Object.Instantiate(requiredMapObject);
			AudioSource[] array = Object.FindObjectsOfType<AudioSource>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].outputAudioMixerGroup = masterMixer;
			}
			if ((bool)MonoSingleton<MusicManager>.Instance)
			{
				if ((bool)MonoSingleton<MusicManager>.Instance.battleTheme)
				{
					MonoSingleton<MusicManager>.Instance.battleTheme.outputAudioMixerGroup = musicMixer;
				}
				if ((bool)MonoSingleton<MusicManager>.Instance.cleanTheme)
				{
					MonoSingleton<MusicManager>.Instance.cleanTheme.outputAudioMixerGroup = musicMixer;
				}
				if ((bool)MonoSingleton<MusicManager>.Instance.bossTheme)
				{
					MonoSingleton<MusicManager>.Instance.bossTheme.outputAudioMixerGroup = musicMixer;
				}
			}
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			List<PlaceholderPrefab> list = new List<PlaceholderPrefab>();
			GameObject[] array2 = rootGameObjects;
			foreach (GameObject gameObject in array2)
			{
				list.AddRange(gameObject.GetComponentsInChildren<PlaceholderPrefab>());
			}
			StartCoroutine(ReplaceAfterTick(list.ToArray()));
		}
	}

	private IEnumerator ReplaceAfterTick(PlaceholderPrefab[] prefabs)
	{
		yield return new WaitForEndOfFrame();
		foreach (PlaceholderPrefab placeholderPrefab in prefabs)
		{
			if (!(placeholderPrefab == null))
			{
				PrefabReplacer.Instance.ReplacePrefab(placeholderPrefab);
			}
		}
		yield return new WaitForEndOfFrame();
		GetComponent<PrefabReplacer>().ReplaceDelayed();
	}
}
