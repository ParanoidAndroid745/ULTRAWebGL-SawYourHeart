using UnityEngine;

public class Projectile : MonoBehaviour
{
	private Rigidbody rb;

	public float speed;

	public float turnSpeed;

	public float speedRandomizer;

	private AudioSource aud;

	public GameObject explosionEffect;

	public float damage;

	public bool friendly;

	public bool playerBullet;

	public string bulletType;

	public string weaponType;

	public bool decorative;

	private Vector3 origScale;

	private bool active = true;

	public EnemyType safeEnemyType;

	public bool explosive;

	public bool bigExplosion;

	public HomingType homingType;

	public float turningSpeedMultiplier = 1f;

	public Transform target;

	private float maxSpeed;

	private Quaternion targetRotation;

	public bool hittingPlayer;

	private NewMovement nmov;

	public bool boosted;

	private Collider col;

	private float radius;

	public bool undeflectable;

	public LayerMask lmask;

	public bool keepTrail;

	public bool strong;

	public bool spreaded;

	private int difficulty;

	public bool precheckForCollisions;

	public bool canHitCoin;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		aud = GetComponent<AudioSource>();
		if ((bool)aud)
		{
			aud.pitch = Random.Range(1.8f, 2f);
			if (aud.enabled)
			{
				aud.Play();
			}
		}
		if (decorative)
		{
			origScale = base.transform.localScale;
			base.transform.localScale = Vector3.zero;
		}
		if (speed != 0f)
		{
			speed += Random.Range(0f - speedRandomizer, speedRandomizer);
		}
		if (col == null)
		{
			col = GetComponentInChildren<Collider>();
		}
		if (col != null && !decorative)
		{
			col.enabled = false;
			col.enabled = true;
		}
		maxSpeed = speed;
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
	}

	public static float GetProjectileSpeedMulti(int difficulty)
	{
		if (difficulty > 2)
		{
			return 1.35f;
		}
		switch (difficulty)
		{
		case 1:
			return 0.75f;
		case 0:
			return 0.5f;
		default:
			return 1f;
		}
	}

	private void Update()
	{
		if (homingType == HomingType.None || !(target != null) || hittingPlayer)
		{
			return;
		}
		switch (homingType)
		{
		case HomingType.Gradual:
		{
			if (difficulty == 1)
			{
				maxSpeed += Time.deltaTime * 17.5f;
			}
			else if (difficulty == 0)
			{
				maxSpeed += Time.deltaTime * 10f;
			}
			else
			{
				maxSpeed += Time.deltaTime * 25f;
			}
			Quaternion to = Quaternion.LookRotation(target.position - base.transform.position);
			if (difficulty == 0)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 100f * turningSpeedMultiplier);
			}
			else if (difficulty == 1)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 135f * turningSpeedMultiplier);
			}
			else if (difficulty == 2)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 185f * turningSpeedMultiplier);
			}
			else
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 200f * turningSpeedMultiplier);
			}
			rb.velocity = base.transform.forward * maxSpeed;
			break;
		}
		case HomingType.Instant:
		{
			Quaternion to = Quaternion.LookRotation(target.position - base.transform.position);
			if (difficulty == 0)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 100f * turningSpeedMultiplier);
			}
			else if (difficulty == 1)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 135f * turningSpeedMultiplier);
			}
			else if (difficulty == 2)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 185f * turningSpeedMultiplier);
			}
			else
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 200f * turningSpeedMultiplier);
			}
			rb.velocity = base.transform.forward * speed;
			break;
		}
		case HomingType.Loose:
		{
			maxSpeed += Time.deltaTime * 10f;
			base.transform.LookAt(base.transform.position + rb.velocity);
			Vector3 normalized = (target.position - base.transform.position).normalized;
			rb.AddForce(normalized * speed * Time.deltaTime * 200f, ForceMode.Acceleration);
			rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
			break;
		}
		case HomingType.HorizontalOnly:
		{
			base.transform.LookAt(target.position + rb.velocity);
			float num = Mathf.Clamp(target.position.x - base.transform.position.x, -25f, 25f);
			float num2 = Mathf.Clamp(target.position.z - base.transform.position.z, -25f, 25f);
			float num3 = 15f;
			if (difficulty == 1)
			{
				num3 = 10f;
			}
			else if (difficulty == 0)
			{
				num3 = 5f;
			}
			else if (difficulty >= 3)
			{
				num3 = 25f;
			}
			float x = Mathf.MoveTowards(rb.velocity.x, num, Time.deltaTime * num3);
			float z = Mathf.MoveTowards(rb.velocity.z, num2, Time.deltaTime * num3);
			rb.velocity = new Vector3(x, rb.velocity.y, z);
			break;
		}
		default:
			maxSpeed += Time.deltaTime * 10f;
			targetRotation = Quaternion.LookRotation(target.position - base.transform.position);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
			rb.velocity = base.transform.forward * maxSpeed;
			break;
		}
	}

	private void FixedUpdate()
	{
		if (!hittingPlayer && !undeflectable && !decorative && speed != 0f && homingType == HomingType.None)
		{
			rb.velocity = base.transform.forward * speed;
		}
		if (decorative && base.transform.localScale.x < origScale.x)
		{
			aud.pitch = base.transform.localScale.x / origScale.x * 2.8f;
			base.transform.localScale = Vector3.Slerp(base.transform.localScale, origScale, Time.deltaTime * speed);
		}
		if (precheckForCollisions)
		{
			LayerMask layerMask = LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment);
			layerMask = (int)layerMask | 4;
			if (Physics.SphereCast(base.transform.position, radius, rb.velocity.normalized, out var hitInfo, rb.velocity.magnitude * Time.fixedDeltaTime, layerMask))
			{
				base.transform.position = base.transform.position + rb.velocity.normalized * hitInfo.distance;
				Collided(hitInfo.collider);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Collided(other);
	}

	private void Collided(Collider other)
	{
		if (!active)
		{
			return;
		}
		if (!friendly && other.gameObject.tag == "Player" && !hittingPlayer)
		{
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
			{
				MonoSingleton<PlatformerMovement>.Instance.Explode();
				if (explosive)
				{
					Explode();
					return;
				}
				if (keepTrail)
				{
					KeepTrail();
				}
				Object.Instantiate(explosionEffect, base.transform.position, base.transform.rotation);
				Object.Destroy(base.gameObject);
				return;
			}
			if (spreaded)
			{
				ProjectileSpread componentInParent = GetComponentInParent<ProjectileSpread>();
				if (componentInParent != null && componentInParent.parried)
				{
					return;
				}
			}
			if (explosive)
			{
				Explode();
				return;
			}
			hittingPlayer = true;
			rb.velocity = Vector3.zero;
			if (keepTrail)
			{
				KeepTrail();
			}
			base.transform.position = new Vector3(other.transform.position.x, base.transform.position.y, other.transform.position.z);
			nmov = other.gameObject.GetComponentInParent<NewMovement>();
			Invoke("RecheckPlayerHit", 0.05f);
		}
		else if (canHitCoin && other.gameObject.tag == "Coin")
		{
			Coin component = other.gameObject.GetComponent<Coin>();
			if ((bool)component && !component.shot)
			{
				if (!friendly)
				{
					component.DelayedEnemyReflect();
				}
				else
				{
					component.DelayedReflectRevolver(component.transform.position);
				}
			}
			if (explosive)
			{
				Explode();
				return;
			}
			if (keepTrail)
			{
				KeepTrail();
			}
			active = false;
			Object.Instantiate(explosionEffect, base.transform.position, base.transform.rotation);
			Object.Destroy(base.gameObject);
		}
		else if (active && (other.gameObject.tag == "Head" || other.gameObject.tag == "Body" || other.gameObject.tag == "Limb" || other.gameObject.tag == "EndLimb") && other.gameObject.tag != "Armor")
		{
			EnemyIdentifierIdentifier componentInParent2 = other.gameObject.GetComponentInParent<EnemyIdentifierIdentifier>();
			EnemyIdentifier enemyIdentifier = null;
			if (componentInParent2 != null && componentInParent2.eid != null)
			{
				enemyIdentifier = componentInParent2.eid;
			}
			if (!(enemyIdentifier != null) || ((enemyIdentifier.enemyType == safeEnemyType || EnemyIdentifier.CheckHurtException(safeEnemyType, enemyIdentifier.enemyType)) && !friendly))
			{
				return;
			}
			if (explosive)
			{
				Explode();
			}
			active = false;
			bool tryForExplode = false;
			bool dead = enemyIdentifier.dead;
			if (playerBullet)
			{
				enemyIdentifier.hitter = bulletType;
				if (!enemyIdentifier.hitterWeapons.Contains(weaponType))
				{
					enemyIdentifier.hitterWeapons.Add(weaponType);
				}
			}
			else if (!friendly)
			{
				enemyIdentifier.hitter = "enemy";
			}
			else
			{
				enemyIdentifier.hitter = "projectile";
				tryForExplode = true;
			}
			if (boosted && !enemyIdentifier.dead)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(200, "<color=lime>PROJECTILE BOOST</color>");
			}
			bool flag = true;
			if (spreaded)
			{
				ProjectileSpread componentInParent3 = GetComponentInParent<ProjectileSpread>();
				if (componentInParent3 != null)
				{
					if (componentInParent3.hitEnemies.Contains(enemyIdentifier))
					{
						flag = false;
					}
					else
					{
						componentInParent3.hitEnemies.Add(enemyIdentifier);
					}
				}
			}
			if (!explosive)
			{
				if (flag)
				{
					if (playerBullet)
					{
						enemyIdentifier.DeliverDamage(other.gameObject, rb.velocity.normalized * 2500f, base.transform.position, damage / 4f, tryForExplode);
					}
					else if (friendly)
					{
						enemyIdentifier.DeliverDamage(other.gameObject, rb.velocity.normalized * 10000f, base.transform.position, damage / 4f, tryForExplode);
					}
					else
					{
						enemyIdentifier.DeliverDamage(other.gameObject, rb.velocity.normalized * 100f, base.transform.position, damage / 10f, tryForExplode);
					}
				}
				Object.Instantiate(explosionEffect, base.transform.position, base.transform.rotation);
			}
			if (keepTrail)
			{
				KeepTrail();
			}
			if (!dead)
			{
				MonoSingleton<TimeController>.Instance.HitStop(0.005f);
			}
			if (!dead || other.gameObject.layer == 11 || boosted)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				active = true;
			}
		}
		else if (other.gameObject.tag == "Armor")
		{
			Debug.Log("Hit Armor: " + other.gameObject.name);
			if (Physics.Raycast(base.transform.position - rb.velocity.normalized, base.transform.forward, out var hitInfo, float.PositiveInfinity, lmask))
			{
				base.transform.forward = Vector3.Reflect(base.transform.forward, hitInfo.normal);
				EnemyIdentifier componentInParent4 = other.GetComponentInParent<EnemyIdentifier>();
				if (componentInParent4 != null)
				{
					Object.Instantiate(componentInParent4.ineffectiveSound, base.transform.position, Quaternion.identity);
				}
			}
		}
		else if (!hittingPlayer && (other.gameObject.layer == 8 || other.gameObject.layer == 24) && active)
		{
			Breakable component2 = other.gameObject.GetComponent<Breakable>();
			if (component2 != null && !component2.precisionOnly && (component2.weak || strong))
			{
				component2.Break();
			}
			if (other.gameObject.TryGetComponent<Bleeder>(out var component3))
			{
				bool flag2 = false;
				if (!friendly && !playerBullet && component3.ignoreTypes.Length != 0)
				{
					EnemyType[] ignoreTypes = component3.ignoreTypes;
					for (int i = 0; i < ignoreTypes.Length; i++)
					{
						if (ignoreTypes[i] == safeEnemyType)
						{
							flag2 = true;
							break;
						}
					}
				}
				if (!flag2)
				{
					if (damage <= 10f)
					{
						component3.GetHit(base.transform.position, GoreType.Body);
					}
					else if (damage <= 30f)
					{
						component3.GetHit(base.transform.position, GoreType.Limb);
					}
					else
					{
						component3.GetHit(base.transform.position, GoreType.Head);
					}
				}
			}
			if (explosive)
			{
				Explode();
			}
			else
			{
				if (keepTrail)
				{
					KeepTrail();
				}
				Object.Instantiate(explosionEffect, base.transform.position, base.transform.rotation);
				Object.Destroy(base.gameObject);
			}
			active = false;
		}
		else if (other.gameObject.layer == 0)
		{
			Rigidbody componentInParent5 = other.GetComponentInParent<Rigidbody>();
			if (componentInParent5 != null)
			{
				componentInParent5.AddForce(base.transform.forward * 1000f);
			}
		}
	}

	public void Explode()
	{
		if (!active)
		{
			return;
		}
		active = false;
		if (keepTrail)
		{
			KeepTrail();
		}
		Explosion[] componentsInChildren = Object.Instantiate(explosionEffect, base.transform.position - rb.velocity * 0.02f, base.transform.rotation).GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			if (bigExplosion)
			{
				explosion.maxSize *= 1.5f;
			}
			if (explosion.damage != 0)
			{
				explosion.damage = Mathf.RoundToInt(damage);
			}
			explosion.enemy = true;
		}
		Object.Destroy(base.gameObject);
	}

	private void RecheckPlayerHit()
	{
		if (hittingPlayer)
		{
			hittingPlayer = false;
			col.enabled = false;
			undeflectable = true;
			Invoke("TimeToDie", 0.01f);
		}
	}

	private void TimeToDie()
	{
		bool flag = false;
		if (spreaded)
		{
			ProjectileSpread componentInParent = GetComponentInParent<ProjectileSpread>();
			if (componentInParent != null && componentInParent.parried)
			{
				flag = true;
			}
		}
		Object.Instantiate(explosionEffect, base.transform.position, base.transform.rotation);
		if (!flag)
		{
			nmov.GetHurt(Mathf.RoundToInt(damage), invincible: true);
		}
		Object.Destroy(base.gameObject);
	}

	private void KeepTrail()
	{
		TrailRenderer componentInChildren = GetComponentInChildren<TrailRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.transform.parent = null;
			componentInChildren.gameObject.AddComponent<RemoveOnTime>().time = 3f;
		}
	}
}
