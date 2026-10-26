using System.Collections.Generic;

[ConfigureSingleton(SingletonFlags.None)]
public class EnemyTracker : MonoSingleton<EnemyTracker>
{
	public List<EnemyIdentifier> enemies = new List<EnemyIdentifier>();

	public List<int> enemyRanks = new List<int>();

	public List<EnemyIdentifier> GetCurrentEnemies()
	{
		List<EnemyIdentifier> list = new List<EnemyIdentifier>();
		if (enemies != null && enemies.Count > 0)
		{
			for (int i = 0; i < enemies.Count; i++)
			{
				if (enemies[i].dead || enemies[i] == null || enemies[i].gameObject == null)
				{
					enemies.RemoveAt(i);
					enemyRanks.RemoveAt(i);
				}
				else if (enemies[i].gameObject.activeInHierarchy)
				{
					list.Add(enemies[i]);
				}
			}
		}
		return list;
	}

	public List<EnemyIdentifier> GetEnemiesOfType(EnemyType type)
	{
		List<EnemyIdentifier> currentEnemies = GetCurrentEnemies();
		if (currentEnemies.Count > 0)
		{
			for (int num = currentEnemies.Count - 1; num >= 0; num--)
			{
				if (currentEnemies[num].enemyType != type)
				{
					currentEnemies.RemoveAt(num);
				}
			}
		}
		return currentEnemies;
	}

	public void AddEnemy(EnemyIdentifier eid)
	{
		if (!enemies.Contains(eid))
		{
			enemies.Add(eid);
			enemyRanks.Add(GetEnemyRank(eid));
		}
	}

	public int GetEnemyRank(EnemyIdentifier eid)
	{
		switch (eid.enemyType)
		{
		case EnemyType.Cerberus:
			return 3;
		case EnemyType.Drone:
			return 1;
		case EnemyType.HideousMass:
			return 6;
		case EnemyType.Filth:
			return 0;
		case EnemyType.Gabriel:
			return 6;
		case EnemyType.Stray:
			return 0;
		case EnemyType.Schism:
			return 1;
		case EnemyType.Sisyphus:
			return 6;
		case EnemyType.Soldier:
			return 1;
		case EnemyType.MaliciousFace:
			return 3;
		case EnemyType.Mindflayer:
			return 5;
		case EnemyType.Minos:
			return 6;
		case EnemyType.Stalker:
			return 4;
		case EnemyType.Streetcleaner:
			return 2;
		case EnemyType.Swordsmachine:
			return 3;
		case EnemyType.Turret:
			return 3;
		case EnemyType.V2:
			return 6;
		case EnemyType.Virtue:
			return 3;
		case EnemyType.Wicked:
			return 6;
		default:
			return -1;
		}
	}
}
