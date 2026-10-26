using UnityEngine;

public class SandboxEnemy : MonoBehaviour
{
	public EnemyIdentifier enemyId;

	public SpawnableObject spawnSource;

	private void OnEnable()
	{
		enemyId = GetComponent<EnemyIdentifier>();
		if (!enemyId)
		{
			enemyId = GetComponentInChildren<EnemyIdentifier>();
		}
	}

	public SavedGeneric Save()
	{
		if (!enemyId || enemyId.health < 0f || enemyId.dead)
		{
			return null;
		}
		return new SavedGeneric
		{
			ObjectIdentifier = spawnSource.identifier,
			Position = base.transform.position,
			Scale = base.transform.localScale
		};
	}
}
