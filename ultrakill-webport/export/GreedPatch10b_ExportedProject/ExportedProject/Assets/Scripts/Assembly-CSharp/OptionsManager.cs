using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class OptionsManager : MonoSingleton<OptionsManager>
{
	public bool mainMenu;

	[HideInInspector]
	public bool paused;

	public bool inIntro;

	public bool frozen;

	[HideInInspector]
	public GameObject pauseMenu;

	[HideInInspector]
	public GameObject optionsMenu;

	[HideInInspector]
	public GameObject progressChecker;

	private NewMovement nm;

	private GunControl gc;

	private FistControl fc;

	[HideInInspector]
	public float mouseSensitivity;

	[HideInInspector]
	public float simplifiedDistance;

	[HideInInspector]
	public bool simplifyEnemies;

	[HideInInspector]
	public bool outlinesOnly;

	private int screenWidth;

	private int screenHeight;

	[HideInInspector]
	public Toggle fullScreen;

	[HideInInspector]
	public float bloodstainChance;

	[HideInInspector]
	public float maxGore;

	[HideInInspector]
	public GameObject playerPosInfo;

	[HideInInspector]
	public bool dontUnpause;

	public string nextAct;

	public bool previousWeaponState;

	public bool dontLockMouseOnUnpause;

	protected override void Awake()
	{
		base.Awake();
		if (GameObject.FindWithTag("OptionsManager") == null)
		{
			Object.Instantiate(progressChecker);
		}
		base.transform.SetParent(null);
	}

	private void Update()
	{
		if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.Return))
		{
			if (!Screen.fullScreen)
			{
				Screen.SetResolution(Screen.resolutions.Last().width, Screen.resolutions.Last().height, fullscreen: true);
			}
			else
			{
				Screen.fullScreen = false;
			}
		}
		if (frozen)
		{
			return;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame && !inIntro && !mainMenu)
		{
			if (!paused)
			{
				Pause();
			}
			else if (!dontUnpause)
			{
				if (SandboxHud.SavesMenuOpen)
				{
					Debug.Log("Closing sandbox saves menu first");
					MonoSingleton<SandboxHud>.Instance.HideSavesMenu();
					return;
				}
				CloseOptions();
				UnPause();
			}
		}
		if (mainMenu && !paused)
		{
			Pause();
		}
		if (paused)
		{
			if (mainMenu)
			{
				Time.timeScale = 1f;
			}
			else
			{
				Time.timeScale = 0f;
			}
		}
	}

	public void Pause()
	{
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
			gc = nm.GetComponentInChildren<GunControl>();
			fc = nm.GetComponentInChildren<FistControl>();
		}
		if (!mainMenu)
		{
			nm.enabled = false;
			MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allPitch", 0f);
			MonoSingleton<AudioMixerController>.Instance.doorSound.SetFloat("allPitch", 0f);
			if ((bool)MonoSingleton<MusicManager>.Instance)
			{
				MonoSingleton<MusicManager>.Instance.FilterMusic();
			}
		}
		MonoSingleton<CameraController>.Instance.activated = false;
		gc.activated = false;
		if (fc != null)
		{
			fc.activated = false;
		}
		paused = true;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		if ((bool)pauseMenu)
		{
			pauseMenu.SetActive(value: true);
		}
		VideoPlayer[] array = Object.FindObjectsOfType<VideoPlayer>();
		foreach (VideoPlayer videoPlayer in array)
		{
			if (videoPlayer.isPlaying)
			{
				videoPlayer.Pause();
			}
		}
	}

	public void UnPause()
	{
		CloseOptions();
		if (!dontLockMouseOnUnpause)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		paused = false;
		Time.timeScale = MonoSingleton<TimeController>.Instance.timeScale * MonoSingleton<TimeController>.Instance.timeScaleModifier;
		MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allPitch", 1f);
		MonoSingleton<AudioMixerController>.Instance.doorSound.SetFloat("allPitch", 1f);
		if ((bool)MonoSingleton<MusicManager>.Instance)
		{
			MonoSingleton<MusicManager>.Instance.UnfilterMusic();
		}
		if (!nm.dead)
		{
			nm.enabled = true;
			MonoSingleton<CameraController>.Instance.activated = true;
			if (!gc.stayUnarmed)
			{
				gc.activated = true;
			}
			if (fc != null)
			{
				fc.activated = true;
			}
		}
		if ((bool)pauseMenu)
		{
			pauseMenu.SetActive(value: false);
		}
		VideoPlayer[] array = Object.FindObjectsOfType<VideoPlayer>();
		foreach (VideoPlayer videoPlayer in array)
		{
			if (videoPlayer.isPaused)
			{
				videoPlayer.Play();
			}
		}
	}

	public void Freeze()
	{
		frozen = true;
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
			gc = nm.GetComponentInChildren<GunControl>();
			fc = nm.GetComponentInChildren<FistControl>();
		}
		MonoSingleton<CameraController>.Instance.activated = false;
		previousWeaponState = !gc.noWeapons;
		gc.NoWeapon();
		gc.enabled = false;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void UnFreeze()
	{
		frozen = false;
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
			gc = nm.GetComponentInChildren<GunControl>();
			fc = nm.GetComponentInChildren<FistControl>();
		}
		MonoSingleton<CameraController>.Instance.activated = true;
		if (previousWeaponState)
		{
			gc.YesWeapon();
		}
		gc.enabled = true;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void RestartCheckpoint()
	{
		UnPause();
		StatsManager statsManager = MonoSingleton<StatsManager>.Instance;
		if (!statsManager.infoSent)
		{
			statsManager.Restart();
		}
	}

	public void RestartMission()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		Object.Destroy(base.gameObject);
	}

	public void OpenOptions()
	{
		pauseMenu.SetActive(value: false);
		optionsMenu.SetActive(value: true);
	}

	public void CloseOptions()
	{
		optionsMenu.SetActive(value: false);
		if ((bool)MonoSingleton<CheatsManager>.Instance)
		{
			MonoSingleton<CheatsManager>.Instance.HideMenu();
		}
		pauseMenu.SetActive(value: true);
	}

	public void QuitMission()
	{
		Time.timeScale = 1f;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		if (Application.CanStreamedLevelBeLoaded("Main Menu"))
		{
			SceneManager.LoadScene("Main Menu");
		}
		else if (Application.CanStreamedLevelBeLoaded("Custom Content"))
		{
			SceneManager.LoadScene("Custom Content");
		}
		else
		{
			Application.Quit();
		}
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void ChangeLevel(string levelname)
	{
		if (Application.CanStreamedLevelBeLoaded(levelname))
		{
			if (nm == null)
			{
				nm = MonoSingleton<NewMovement>.Instance;
			}
			PlayerPosInfo component = Object.Instantiate(playerPosInfo).GetComponent<PlayerPosInfo>();
			component.velocity = nm.GetComponent<Rigidbody>().velocity;
			component.wooshTime = nm.GetComponentInChildren<WallCheck>().GetComponent<AudioSource>().time;
			component.noPosition = true;
			if ((bool)MonoSingleton<LoadingScreen>.Instance)
			{
				MonoSingleton<LoadingScreen>.Instance.gameObject.SetActive(value: true);
			}
			SceneManager.LoadScene(levelname);
		}
		else
		{
			SceneManager.LoadScene("Main Menu");
		}
	}

	public void ChangeLevelAbrupt(string levelname)
	{
		if ((bool)MonoSingleton<LoadingScreen>.Instance)
		{
			MonoSingleton<LoadingScreen>.Instance.gameObject.SetActive(value: true);
		}
		if (!string.IsNullOrEmpty(nextAct))
		{
			Debug.Log("Loading " + nextAct);
			MonoSingleton<MapLoader>.Instance.LoadAct(nextAct, (AssetBundle bundle) =>
			{
				SceneManager.LoadScene(levelname);
			}, () =>
			{
				SceneManager.LoadScene("Main Menu");
			});
		}
		else if (Application.CanStreamedLevelBeLoaded(levelname))
		{
			SceneManager.LoadScene(levelname);
		}
		else
		{
			SceneManager.LoadScene("Main Menu");
		}
	}

	public void ChangeLevelWithPosition(string levelname)
	{
		if (Application.CanStreamedLevelBeLoaded(levelname))
		{
			if (nm == null)
			{
				nm = MonoSingleton<NewMovement>.Instance;
			}
			PlayerPosInfo component = Object.Instantiate(playerPosInfo).GetComponent<PlayerPosInfo>();
			component.velocity = nm.GetComponent<Rigidbody>().velocity;
			component.wooshTime = nm.GetComponentInChildren<WallCheck>().GetComponent<AudioSource>().time;
			component.noPosition = false;
			if ((bool)MonoSingleton<LoadingScreen>.Instance)
			{
				MonoSingleton<LoadingScreen>.Instance.gameObject.SetActive(value: true);
			}
			SceneManager.LoadScene(levelname);
		}
		else
		{
			SceneManager.LoadScene("Main Menu");
		}
	}
}
