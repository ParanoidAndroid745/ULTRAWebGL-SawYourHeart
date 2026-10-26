using UnityEngine;

public class CheckLevelRank : MonoBehaviour
{
	public UltrakillEvent onSuccess;

	public UltrakillEvent onFail;

	private void Start()
	{
		if (CheckLevelStatus())
		{
			onSuccess?.Invoke();
		}
		else
		{
			onFail?.Invoke();
		}
	}

	public static bool CheckLevelStatus()
	{
		if (MonoSingleton<MapLoader>.Instance.isCustomLoaded)
		{
			return true;
		}
		if (MonoSingleton<StatsManager>.Instance.levelNumber == 0)
		{
			return true;
		}
		RankData rank = GameProgressSaver.GetRank(returnNull: true);
		if (rank != null && rank.levelNumber == MonoSingleton<StatsManager>.Instance.levelNumber)
		{
			return true;
		}
		return false;
	}
}
