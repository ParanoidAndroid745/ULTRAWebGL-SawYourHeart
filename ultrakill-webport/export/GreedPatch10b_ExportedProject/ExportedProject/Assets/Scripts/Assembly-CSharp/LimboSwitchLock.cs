using UnityEngine;
using UnityEngine.Events;

public class LimboSwitchLock : MonoBehaviour
{
	public SwitchLockType type;

	public MeshRenderer[] locks;

	private MaterialPropertyBlock block;

	private float[] lockIntensities;

	private bool[] lockStates;

	private int openedLocks;

	public UnityEvent onAllLocksOpen;

	private int minimumOrderNumber;

	private void Start()
	{
		block = new MaterialPropertyBlock();
		lockIntensities = new float[locks.Length];
		lockStates = new bool[locks.Length];
		if (type != SwitchLockType.None)
		{
			if (type == SwitchLockType.Limbo)
			{
				for (int i = 0; i < locks.Length; i++)
				{
					if (GameProgressSaver.GetLimboSwitch(i + minimumOrderNumber))
					{
						lockIntensities[i] = 2f;
						lockStates[i] = true;
						locks[i].GetPropertyBlock(block);
						block.SetFloat(UKShaderProperties.EmissiveIntensity, 2f);
						locks[i].SetPropertyBlock(block);
						openedLocks++;
					}
				}
			}
			else if (type == SwitchLockType.PRank)
			{
				int num = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
				for (int j = 0; j < locks.Length; j++)
				{
					RankData rank = GameProgressSaver.GetRank(j + minimumOrderNumber + 1);
					if (rank != null && rank.ranks[num] == 12 && (rank.majorAssists == null || !rank.majorAssists[num]))
					{
						lockIntensities[j] = 2f;
						lockStates[j] = true;
						locks[j].GetPropertyBlock(block);
						block.SetFloat(UKShaderProperties.EmissiveIntensity, 2f);
						locks[j].SetPropertyBlock(block);
						openedLocks++;
					}
				}
			}
		}
		CheckLocks();
	}

	private void Update()
	{
		for (int i = 0; i < locks.Length; i++)
		{
			if (lockStates[i] && lockIntensities[i] != 2f)
			{
				lockIntensities[i] = Mathf.MoveTowards(lockIntensities[i], 2f, Time.deltaTime);
				locks[i].GetPropertyBlock(block);
				block.SetFloat(UKShaderProperties.EmissiveIntensity, lockIntensities[i]);
				locks[i].SetPropertyBlock(block);
				if (lockIntensities[i] == 2f)
				{
					openedLocks++;
					CheckLocks();
				}
			}
		}
	}

	private void CheckLocks()
	{
		if (openedLocks == locks.Length)
		{
			onAllLocksOpen?.Invoke();
		}
	}

	public void OpenLock(int num)
	{
		lockStates[num - 1] = true;
		locks[num - 1].GetComponent<AudioSource>()?.Play();
	}
}
