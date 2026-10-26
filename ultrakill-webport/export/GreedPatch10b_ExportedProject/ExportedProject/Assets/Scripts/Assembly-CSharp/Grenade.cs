using UnityEngine;

public class Grenade : MonoBehaviour
{
	public string hitterWeapon;

	public GameObject explosion;

	public GameObject harmlessExplosion;

	public GameObject superExplosion;

	public GameObject ineffectiveSound;

	private bool exploded;

	public bool enemy;

	public bool rocket;

	[HideInInspector]
	public Rigidbody rb;

	public float rocketSpeed;

	public bool frozen;

	private void Awake()
	{
		if (!enemy)
		{
			Physics.IgnoreCollision(GetComponent<Collider>(), MonoSingleton<NewMovement>.Instance.GetComponent<Collider>(), ignore: true);
		}
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (rocket && rocketSpeed != 0f && (bool)rb)
		{
			if (frozen)
			{
				rb.velocity = Vector3.zero;
			}
			else
			{
				rb.velocity = base.transform.forward * rocketSpeed;
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (exploded || (!enemy && !(collision.gameObject.tag != "Player")) || collision.transform.gameObject.layer == 14)
		{
			return;
		}
		bool flag = false;
		if (collision.gameObject.layer == 11)
		{
			EnemyIdentifier eid = collision.gameObject.GetComponent<EnemyIdentifierIdentifier>().eid;
			if (eid != null && !eid.dead && eid.enemyType == EnemyType.MaliciousFace)
			{
				flag = true;
			}
		}
		if (!flag && collision.gameObject.tag == "Armor")
		{
			flag = true;
		}
		if (flag)
		{
			LayerMask layerMask = 256;
			layerMask = (int)layerMask | 0x1000000;
			if (Physics.Raycast(base.transform.position - rb.velocity.normalized, base.transform.forward, out var hitInfo, float.PositiveInfinity, layerMask))
			{
				Vector3 velocity = rb.velocity;
				rb.velocity = Vector3.zero;
				rb.AddForce(Vector3.Reflect(velocity.normalized, hitInfo.normal).normalized * velocity.magnitude * 2f, ForceMode.VelocityChange);
				base.transform.forward = Vector3.Reflect(velocity.normalized, hitInfo.normal).normalized;
			}
			Object.Instantiate(ineffectiveSound, base.transform.position, Quaternion.identity).GetComponent<AudioSource>().volume = 0.75f;
			return;
		}
		bool harmless = false;
		bool big = false;
		bool super = false;
		if (rocket)
		{
			if (collision.gameObject.layer == 10 || collision.gameObject.layer == 11)
			{
				EnemyIdentifierIdentifier component = collision.gameObject.GetComponent<EnemyIdentifierIdentifier>();
				if ((bool)component && (bool)component.eid && !component.eid.dead && !component.eid.flying && (bool)component.eid.gce && !component.eid.gce.onGround)
				{
					super = true;
				}
				MonoSingleton<TimeController>.Instance.HitStop(0.05f);
			}
			else
			{
				harmless = true;
			}
		}
		else if (collision.gameObject.layer != 8)
		{
			MonoSingleton<TimeController>.Instance.HitStop(0.05f);
		}
		Explode(big, harmless, super);
	}

	public void Explode(bool big = false, bool harmless = false, bool super = false, float sizeMultiplier = 1f, bool ultrabooster = false)
	{
		exploded = true;
		GameObject gameObject = (harmless ? Object.Instantiate(harmlessExplosion, base.transform.position, Quaternion.identity) : ((!super) ? Object.Instantiate(this.explosion, base.transform.position, Quaternion.identity) : Object.Instantiate(superExplosion, base.transform.position, Quaternion.identity)));
		Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			explosion.hitterWeapon = hitterWeapon;
			if (enemy)
			{
				explosion.enemy = true;
			}
			if (rocket & super & big)
			{
				explosion.maxSize *= 2.5f;
				explosion.speed *= 2.5f;
			}
			else if (big)
			{
				explosion.maxSize *= 1.5f;
				explosion.speed *= 1.5f;
			}
			explosion.maxSize *= sizeMultiplier;
			explosion.speed *= sizeMultiplier;
			if (ultrabooster)
			{
				explosion.ultrabooster = true;
			}
		}
		gameObject.transform.localScale *= sizeMultiplier;
		Object.Destroy(base.gameObject);
	}
}
