using System;

[Serializable]
public class RankData
{
	public int[] ranks;

	public int secretsAmount;

	public bool[] secretsFound;

	public bool challenge;

	public int levelNumber;

	public bool[] majorAssists;

	public RankData(StatsManager sman)
	{
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		levelNumber = sman.levelNumber;
		RankData rank = GameProgressSaver.GetRank(returnNull: true);
		if (rank != null)
		{
			ranks = rank.ranks;
			if (rank.majorAssists != null)
			{
				majorAssists = rank.majorAssists;
			}
			else
			{
				majorAssists = new bool[5];
			}
			if ((sman.rankScore >= rank.ranks[num] && (rank.majorAssists == null || (!sman.majorUsed && rank.majorAssists[num]))) || sman.rankScore > rank.ranks[num] || rank.levelNumber != levelNumber)
			{
				majorAssists[num] = sman.majorUsed;
				ranks[num] = sman.rankScore;
			}
			secretsAmount = sman.secretObjects.Length;
			secretsFound = new bool[secretsAmount];
			for (int i = 0; i < secretsAmount; i++)
			{
				if (sman.secretObjects[i] == null || rank.secretsFound[i])
				{
					secretsFound[i] = true;
				}
			}
			challenge = rank.challenge;
			return;
		}
		ranks = new int[6];
		majorAssists = new bool[6];
		for (int j = 0; j < ranks.Length; j++)
		{
			ranks[j] = -1;
		}
		ranks[num] = sman.rankScore;
		majorAssists[num] = sman.majorUsed;
		secretsAmount = sman.secretObjects.Length;
		secretsFound = new bool[secretsAmount];
		for (int k = 0; k < secretsAmount; k++)
		{
			if (sman.secretObjects[k] == null)
			{
				secretsFound[k] = true;
			}
		}
	}
}
