using UnityEngine;
using UnityEngine.SceneManagement;

[ConfigureSingleton(SingletonFlags.NoAutoInstance | SingletonFlags.DestroyDuplicates)]
public class PresenceController : MonoSingleton<PresenceController>
{
	public string[] diffNames;

	private void Start()
	{
		base.transform.SetParent(null);
		Object.DontDestroyOnLoad(base.gameObject);
		SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
		SceneManagerOnsceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
	}

	public static void UpdateCyberGrindWave(int wave)
	{
		DiscordController.UpdateWave(wave);
		SteamController.Instance.UpdateWave(wave);
	}

	private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (mode != LoadSceneMode.Additive)
		{
			DiscordController.Instance.FetchSceneActivity(scene.name);
			SteamController.Instance.FetchSceneActivity(scene.name);
		}
	}
}
