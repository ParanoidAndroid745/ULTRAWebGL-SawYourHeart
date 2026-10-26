using UnityEngine;
using UnityEngine.SceneManagement;

public class AbruptLevelChanger : MonoBehaviour
{
	public bool saveMission;

	public void AbruptChangeLevel(string levelname)
	{
		if (saveMission)
		{
			MonoSingleton<PreviousMissionSaver>.Instance.previousMission = MonoSingleton<StatsManager>.Instance.levelNumber;
		}
		SceneManager.LoadScene(levelname);
	}

	public void NormalChangeLevel(string levelname)
	{
		MonoSingleton<OptionsManager>.Instance.ChangeLevel(levelname);
	}

	public void PositionChangeLevel(string levelname)
	{
		MonoSingleton<OptionsManager>.Instance.ChangeLevelWithPosition(levelname);
	}

	public void GoToLevel(int missionNumber)
	{
		SceneManager.LoadScene(GetMissionName.GetSceneName(missionNumber));
	}

	public void GoToSavedLevel()
	{
		PreviousMissionSaver instance = MonoSingleton<PreviousMissionSaver>.Instance;
		if (instance != null)
		{
			_ = instance.previousMission;
			Object.Destroy(instance.gameObject);
			GoToLevel(instance.previousMission);
		}
		else
		{
			GoToLevel(GameProgressSaver.GetProgress(MonoSingleton<PrefsManager>.Instance.GetInt("difficulty")));
		}
	}
}
