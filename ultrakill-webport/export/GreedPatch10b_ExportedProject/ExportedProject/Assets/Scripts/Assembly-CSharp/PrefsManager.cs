using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance | SingletonFlags.DestroyDuplicates)]
public class PrefsManager : MonoSingleton<PrefsManager>
{
	private enum PrefsCommitMode
	{
		Immediate = 0,
		OnQuit = 1,
		DirtySlowTick = 2
	}

	private FileStream prefsStream;

	private FileStream localPrefsStream;

	private static PrefsCommitMode CommitMode = ((!Application.isEditor) ? PrefsCommitMode.DirtySlowTick : PrefsCommitMode.Immediate);

	private TimeSince timeSinceLastTick;

	private const float SlowTickCommitInterval = 3f;

	private const bool DebugLogging = false;

	private bool isDirty;

	private bool isLocalDirty;

	private Dictionary<string, dynamic> prefMap;

	private Dictionary<string, dynamic> localPrefMap;

	private readonly Dictionary<string, dynamic> defaultValues = new Dictionary<string, object>
	{
		{ "difficulty", 2 },
		{ "scrollEnabled", true },
		{ "scrollWeapons", true },
		{ "scrollVariations", false },
		{ "scrollReversed", false },
		{ "mouseSensitivity", 50f },
		{ "discordIntegration", true },
		{ "subtitlesEnabled", false },
		{ "seasonalEvents", true },
		{ "majorAssist", false },
		{ "gameSpeed", 1f },
		{ "damageTaken", 1f },
		{ "infiniteStamina", false },
		{ "disableHardDamage", false },
		{ "autoAim", false },
		{ "autoAimAmount", 0.2f },
		{ "bossDifficultyOverride", 0 },
		{ "hideMajorAssistPopup", false },
		{ "frameRateLimit", 1 },
		{ "fullscreen", true },
		{ "fieldOfView", 105f },
		{ "vSync", true },
		{ "musicVolume", 0.6f },
		{ "allVolume", 1f },
		{ "screenShake", 1f },
		{ "cameraTilt", true },
		{ "dithering", 0.2f },
		{ "colorCompression", 2 },
		{ "vertexWarping", 0 },
		{ "textureWarping", 0f },
		{ "pixelization", 0 },
		{ "crossHair", 1 },
		{ "crossHairColor", 1 },
		{ "crossHairHud", 2 },
		{ "hudType", 1 },
		{ "hudBackgroundOpacity", 50f },
		{ "weaponHoldPosition", 0 },
		{ "variationMemory", false },
		{ "bloodEnabled", true },
		{ "bloodStainChance", 0.5f },
		{ "maxGore", 3000f },
		{ "weaponIcons", true },
		{ "armIcons", true },
		{ "styleMeter", true },
		{ "styleInfo", true },
		{ "crossHairHudFade", true },
		{ "powerUpMeter", true },
		{ "railcannonMeter", true },
		{ "simpleNailPhysics", true },
		{ "selectedSaveSlot", 0 }
	};

#if UNITY_WEBGL
	// En WebGL Application.dataPath es una URL y no existe un sistema de
	// ficheros escribible. Los prefs se quedan en memoria durante la sesion.
	private static string PrefsPath => "Preferences";
#else
	private static string PrefsPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Preferences");
#endif

	public bool HasKey(string key)
	{
		EnsureInitialized();
		if (!prefMap.ContainsKey(key))
		{
			return localPrefMap.ContainsKey(key);
		}
		return true;
	}

	public void DeleteKey(string key)
	{
		EnsureInitialized();
		if (prefMap.ContainsKey(key))
		{
			prefMap.Remove(key);
			if (CommitMode == PrefsCommitMode.DirtySlowTick)
			{
				isDirty = true;
			}
			if (CommitMode == PrefsCommitMode.Immediate)
			{
				CommitPrefs(local: false);
			}
		}
		if (localPrefMap.ContainsKey(key))
		{
			localPrefMap.Remove(key);
			if (CommitMode == PrefsCommitMode.DirtySlowTick)
			{
				isLocalDirty = true;
			}
			if (CommitMode == PrefsCommitMode.Immediate)
			{
				CommitPrefs(local: true);
			}
		}
	}

	public bool GetBoolLocal(string key, bool fallback = false)
	{
		EnsureInitialized();
		object obj;
		if (localPrefMap.ContainsKey(key) && (obj = localPrefMap[key]) is bool)
		{
			return (bool)obj;
		}
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is bool)
		{
			return (bool)obj;
		}
		return fallback;
	}

	public bool GetBool(string key, bool fallback = false)
	{
		EnsureInitialized();
		object obj;
		if (prefMap.ContainsKey(key) && (obj = prefMap[key]) is bool)
		{
			return (bool)obj;
		}
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is bool)
		{
			return (bool)obj;
		}
		return fallback;
	}

	public void SetBoolLocal(string key, bool content)
	{
		EnsureInitialized();
		if (localPrefMap.ContainsKey(key))
		{
			localPrefMap[key] = content;
		}
		else
		{
			localPrefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: true);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isLocalDirty = true;
		}
	}

	public void SetBool(string key, bool content)
	{
		EnsureInitialized();
		if (prefMap.ContainsKey(key))
		{
			prefMap[key] = content;
		}
		else
		{
			prefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: false);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isDirty = true;
		}
	}

	public int GetIntLocal(string key, int fallback = 0)
	{
		EnsureInitialized();
		object obj;
		if (localPrefMap.ContainsKey(key))
		{
			if ((obj = localPrefMap[key]) is int)
			{
				return (int)obj;
			}
			if (localPrefMap[key] is long num)
			{
				return (int)num;
			}
			if (localPrefMap[key] is float num2)
			{
				return (int)num2;
			}
			if (localPrefMap[key] is double num3)
			{
				return (int)num3;
			}
		}
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is int)
		{
			return (int)obj;
		}
		return fallback;
	}

	public int GetInt(string key, int fallback = 0)
	{
		EnsureInitialized();
		object obj;
		if (prefMap.ContainsKey(key))
		{
			if ((obj = prefMap[key]) is int)
			{
				return (int)obj;
			}
			if (prefMap[key] is long num)
			{
				return (int)num;
			}
			if (prefMap[key] is float num2)
			{
				return (int)num2;
			}
			if (prefMap[key] is double num3)
			{
				return (int)num3;
			}
		}
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is int)
		{
			return (int)obj;
		}
		return fallback;
	}

	public void SetIntLocal(string key, int content)
	{
		EnsureInitialized();
		if (localPrefMap.ContainsKey(key))
		{
			localPrefMap[key] = content;
		}
		else
		{
			localPrefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: true);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isLocalDirty = true;
		}
	}

	public void SetInt(string key, int content)
	{
		EnsureInitialized();
		if (prefMap.ContainsKey(key))
		{
			prefMap[key] = content;
		}
		else
		{
			prefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: false);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isDirty = true;
		}
	}

	public float GetFloatLocal(string key, float fallback = 0f)
	{
		EnsureInitialized();
		object obj;
		if (localPrefMap.ContainsKey(key))
		{
			if ((obj = localPrefMap[key]) is float)
			{
				return (float)obj;
			}
			if (localPrefMap[key] is int num)
			{
				return num;
			}
			if (localPrefMap[key] is long num2)
			{
				return num2;
			}
			if (localPrefMap[key] is double num3)
			{
				return (float)num3;
			}
		}
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is float)
		{
			return (float)obj;
		}
		return fallback;
	}

	public float GetFloat(string key, float fallback = 0f)
	{
		EnsureInitialized();
		object obj;
		if (prefMap.ContainsKey(key))
		{
			if ((obj = prefMap[key]) is float)
			{
				return (float)obj;
			}
			if (prefMap[key] is int num)
			{
				return num;
			}
			if (prefMap[key] is long num2)
			{
				return num2;
			}
			if (prefMap[key] is double num3)
			{
				return (float)num3;
			}
		}
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is float)
		{
			return (float)obj;
		}
		return fallback;
	}

	public void SetFloatLocal(string key, float content)
	{
		EnsureInitialized();
		if (localPrefMap.ContainsKey(key))
		{
			localPrefMap[key] = content;
		}
		else
		{
			localPrefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: true);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isLocalDirty = true;
		}
	}

	public void SetFloat(string key, float content)
	{
		EnsureInitialized();
		if (prefMap.ContainsKey(key))
		{
			prefMap[key] = content;
		}
		else
		{
			prefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: false);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isDirty = true;
		}
	}

	public string GetStringLocal(string key, string fallback = null)
	{
		EnsureInitialized();
		if (localPrefMap.ContainsKey(key) && localPrefMap[key] is string result)
		{
			return result;
		}
		if (defaultValues.ContainsKey(key) && defaultValues[key] is string result2)
		{
			return result2;
		}
		return fallback;
	}

	public string GetString(string key, string fallback = null)
	{
		EnsureInitialized();
		if (prefMap.ContainsKey(key) && prefMap[key] is string result)
		{
			return result;
		}
		if (defaultValues.ContainsKey(key) && defaultValues[key] is string result2)
		{
			return result2;
		}
		return fallback;
	}

	public void SetStringLocal(string key, string content)
	{
		EnsureInitialized();
		if (localPrefMap.ContainsKey(key))
		{
			localPrefMap[key] = content;
		}
		else
		{
			localPrefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: true);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isLocalDirty = true;
		}
	}

	public void SetString(string key, string content)
	{
		EnsureInitialized();
		if (prefMap.ContainsKey(key))
		{
			prefMap[key] = content;
		}
		else
		{
			prefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: false);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isDirty = true;
		}
	}

	private void CommitPrefs(bool local)
	{
#if UNITY_WEBGL
		// Sin disco: prefMap/localPrefMap ya contienen los valores actuales.
#else
		if (local)
		{
			string value = JsonConvert.SerializeObject(localPrefMap, Formatting.Indented);
			localPrefsStream.SetLength(0L);
			StreamWriter streamWriter = new StreamWriter(localPrefsStream);
			streamWriter.Write(value);
			streamWriter.Flush();
		}
		else
		{
			string value2 = JsonConvert.SerializeObject(prefMap, Formatting.Indented);
			prefsStream.SetLength(0L);
			StreamWriter streamWriter2 = new StreamWriter(prefsStream);
			streamWriter2.Write(value2);
			streamWriter2.Flush();
		}
#endif
	}

	private void EnsureInitialized()
	{
		if (prefMap == null || localPrefMap == null)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
#if UNITY_WEBGL
		// Sin acceso a disco: arrancamos con mapas vacios y los valores por
		// defecto de defaultValues hacen el resto.
		timeSinceLastTick = 0f;
		prefMap = new Dictionary<string, dynamic>();
		localPrefMap = new Dictionary<string, dynamic>();
		Debug.Log("PrefsManager initialized (WebGL, en memoria)");
#else
		if (!Directory.Exists(PrefsPath))
		{
			Directory.CreateDirectory(PrefsPath);
		}
		timeSinceLastTick = 0f;
		if (prefsStream == null)
		{
			prefsStream = new FileStream(Path.Combine(PrefsPath, "Prefs.json"), FileMode.OpenOrCreate);
		}
		if (localPrefsStream == null)
		{
			localPrefsStream = new FileStream(Path.Combine(PrefsPath, "LocalPrefs.json"), FileMode.OpenOrCreate);
		}
		prefMap = LoadPrefs(prefsStream);
		localPrefMap = LoadPrefs(localPrefsStream);
		Debug.Log("PrefsManager initialized");
#endif
	}

	private Dictionary<string, dynamic> LoadPrefs(FileStream stream)
	{
		return JsonConvert.DeserializeObject<Dictionary<string, object>>(new StreamReader(stream).ReadToEnd()) ?? new Dictionary<string, object>();
	}

	protected override void Awake()
	{
		base.Awake();
		if (!(MonoSingleton<PrefsManager>.Instance != this))
		{
			Initialize();
		}
	}

	private void FixedUpdate()
	{
		if ((isDirty || isLocalDirty) && CommitMode == PrefsCommitMode.DirtySlowTick && (float)timeSinceLastTick >= 3f)
		{
			timeSinceLastTick = 0f;
			if (isLocalDirty)
			{
				CommitPrefs(local: true);
			}
			if (isDirty)
			{
				CommitPrefs(local: false);
			}
			isLocalDirty = false;
			isDirty = false;
		}
	}

	private void OnApplicationQuit()
	{
		if (CommitMode == PrefsCommitMode.OnQuit || CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			CommitPrefs(local: false);
			CommitPrefs(local: true);
		}
		prefsStream?.Close();
		localPrefsStream?.Close();
	}
}
