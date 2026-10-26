using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscChange : MonoBehaviour
{
	public string nextAct;

	public string targetLevelName;

	private AsyncOperation asyncLoad;

	private bool ready;

	private bool done;

	[SerializeField]
	private GameObject discSound;

	private void Start()
	{
		StartCoroutine(LoadNextLevel());
	}

	private void Update()
	{
		if (ready && !done && asyncLoad != null && asyncLoad.progress >= 0.9f)
		{
			done = true;
			PlayerPrefs.SetInt("DisCha", 1);
			discSound.transform.parent = null;
			Object.DontDestroyOnLoad(discSound);
			if (discSound.TryGetComponent<FadeOut>(out var component))
			{
				component.BeginFade();
			}
			asyncLoad.allowSceneActivation = true;
		}
	}

	private IEnumerator LoadNextLevel()
	{
		if ((bool)MonoSingleton<MapLoader>.Instance && MonoSingleton<MapLoader>.Instance.isCustomLoaded)
		{
			yield break;
		}
		string empty = string.Empty;
		StockMapInfo instance = StockMapInfo.Instance;
		if ((bool)instance)
		{
			empty = instance.nextAct;
		}
		if (targetLevelName != "null")
		{
			if (!string.IsNullOrEmpty(empty))
			{
				MonoSingleton<MapLoader>.Instance.LoadAct(empty, (AssetBundle bundle) =>
				{
					Debug.Log("Bundle loaded. Loading the scene");
					string[] array = (from p in bundle.GetAllScenePaths()
						select Path.GetFileName(p)).ToArray();
					Debug.Log(array[0]);
					Debug.Log(targetLevelName + ".unity");
					if (!array.Contains(targetLevelName + ".unity"))
					{
						targetLevelName = "Main Menu";
					}
					asyncLoad = SceneManager.LoadSceneAsync(targetLevelName);
					asyncLoad.allowSceneActivation = false;
				}, () =>
				{
					targetLevelName = "Main Menu";
					asyncLoad = SceneManager.LoadSceneAsync(targetLevelName);
					asyncLoad.allowSceneActivation = false;
				});
			}
			else
			{
				asyncLoad = SceneManager.LoadSceneAsync(targetLevelName);
				asyncLoad.allowSceneActivation = false;
			}
		}
		else
		{
			asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
		}
		if (asyncLoad != null)
		{
			asyncLoad.allowSceneActivation = false;
		}
		while (asyncLoad != null && !asyncLoad.isDone)
		{
			yield return null;
		}
	}

	public void ReadyToChangeLevel()
	{
		ready = true;
	}
}
