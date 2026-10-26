using UnityEngine;

public class Cannonball : MonoBehaviour
{
	public bool launchable = true;

	[SerializeField]
	public bool launched;

	private Rigidbody rb;

	private Collider col;

	[SerializeField]
	private GameObject breakEffect;

	private bool checkingForBreak;

	public float damage;

	public float speed;

	public bool parry;

	[HideInInspector]
	public Sisyphus sisy;

	public bool ghostCollider;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		col = GetComponent<Collider>();
	}

	private void FixedUpdate()
	{
		if (launched)
		{
			rb.velocity = base.transform.forward * speed;
		}
	}

	public void Launch()
	{
		if (launchable)
		{
			launched = true;
			rb.isKinematic = false;
			rb.useGravity = false;
			col.isTrigger = true;
			if ((bool)sisy)
			{
				sisy.GotParried();
			}
		}
	}

	public void Unlaunch(bool relaunchable = true)
	{
		launchable = relaunchable;
		launched = false;
		rb.isKinematic = true;
		rb.velocity = Vector3.zero;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!ghostCollider || (launched && (other.gameObject.layer == 10 || other.gameObject.layer == 11 || other.gameObject.layer == 12)))
		{
			Collide(other);
		}
	}

	public void Collide(Collider other)
	{
		if (!launched)
		{
			return;
		}
		if (other.gameObject.layer == 0 || other.gameObject.layer == 8 || other.gameObject.layer == 24 || other.gameObject.tag == "Player")
		{
			Break();
		}
		else
		{
			if ((other.gameObject.layer != 10 && other.gameObject.layer != 11 && other.gameObject.layer != 12) || checkingForBreak)
			{
				return;
			}
			checkingForBreak = true;
			EnemyIdentifierIdentifier component = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
			EnemyIdentifier enemyIdentifier = ((!component || !component.eid) ? other.gameObject.GetComponent<EnemyIdentifier>() : component.eid);
			if ((bool)enemyIdentifier)
			{
				bool flag = true;
				if (!enemyIdentifier.dead)
				{
					flag = false;
				}
				enemyIdentifier.hitter = "cannonball";
				enemyIdentifier.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 100f, base.transform.position, damage, tryForExplode: true);
				if (!enemyIdentifier || enemyIdentifier.dead)
				{
					if (!flag)
					{
						MonoSingleton<StyleHUD>.Instance.AddPoints(50, "CANNONBALLED");
					}
					if ((bool)enemyIdentifier)
					{
						enemyIdentifier.Explode();
					}
					checkingForBreak = false;
				}
				else
				{
					Break();
					if (enemyIdentifier.enemyType == EnemyType.Sisyphus && enemyIdentifier.TryGetComponent<Sisyphus>(out var component2))
					{
						component2.Knockdown(base.transform.position);
					}
				}
			}
			else
			{
				checkingForBreak = false;
			}
		}
	}

	private void Break()
	{
		if ((bool)sisy)
		{
			checkingForBreak = false;
			launched = false;
			launchable = false;
			rb.useGravity = true;
			rb.velocity = Vector3.up * 25f;
			MonoSingleton<CameraController>.Instance.CameraShake(1f);
			if ((bool)breakEffect)
			{
				Object.Instantiate(breakEffect, base.transform.position, base.transform.rotation);
			}
			sisy.SwingStop();
		}
		else
		{
			if ((bool)breakEffect)
			{
				Object.Instantiate(breakEffect, base.transform.position, base.transform.rotation);
			}
			Object.Destroy(base.gameObject);
		}
	}
}
