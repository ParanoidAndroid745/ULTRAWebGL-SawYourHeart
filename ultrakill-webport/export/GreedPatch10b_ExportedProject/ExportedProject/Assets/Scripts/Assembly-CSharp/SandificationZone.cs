using UnityEngine;

public class SandificationZone : MonoBehaviour
{
	private int difficulty;

	private void Start()
	{
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
	}

	private void Enter(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			if (difficulty >= 3)
			{
				if (MonoSingleton<NewMovement>.Instance.hp > 10)
				{
					MonoSingleton<NewMovement>.Instance.ForceAntiHP(100 - MonoSingleton<NewMovement>.Instance.hp + 10);
				}
				else
				{
					MonoSingleton<NewMovement>.Instance.ForceAntiHP(99);
				}
			}
			else if (difficulty == 2)
			{
				if (MonoSingleton<NewMovement>.Instance.hp > 10)
				{
					MonoSingleton<NewMovement>.Instance.ForceAntiHP(Mathf.RoundToInt(MonoSingleton<NewMovement>.Instance.antiHp) + 10);
				}
				else
				{
					MonoSingleton<NewMovement>.Instance.ForceAntiHP(99);
				}
			}
		}
		else if (other.gameObject.layer == 10 || other.gameObject.layer == 11)
		{
			EnemyIdentifierIdentifier component = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
			if ((bool)component && (bool)component.eid && !component.eid.dead)
			{
				component.eid.Sandify();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Enter(other);
	}

	private void OnCollisionEnter(Collision collision)
	{
		Enter(collision.collider);
	}
}
