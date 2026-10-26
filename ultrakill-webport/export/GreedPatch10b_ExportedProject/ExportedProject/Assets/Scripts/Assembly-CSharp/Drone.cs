using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;

public class Drone : MonoBehaviour
{
	public bool friendly;

	public bool dontStartAware;

	public bool stationary;

	public float health;

	public bool crashing;

	private Vector3 crashTarget;

	private Rigidbody rb;

	private bool canInterruptCrash;

	private Transform modelTransform;

	public bool playerSpotted;

	public bool toLastKnownPos;

	private Vector3 lastKnownPos;

	private Vector3 nextRandomPos;

	public float checkCooldown;

	public float blockCooldown;

	public float preferredDistanceToTarget = 15f;

	private GameObject camObj;

	private Transform target;

	private BloodsplatterManager bsm;

	public GameObject explosion;

	public GameObject gib;

	private StyleCalculator scalc;

	private EnemyIdentifier eid;

	private EnemyType type;

	private AudioSource aud;

	public AudioClip hurtSound;

	public AudioClip deathSound;

	public AudioClip windUpSound;

	public AudioClip spotSound;

	public AudioClip loseSound;

	private float dodgeCooldown;

	private float attackCooldown;

	public GameObject projectile;

	private Material origMaterial;

	public Material shootMaterial;

	private MeshRenderer[] mrs;

	public ParticleSystem chargeParticle;

	private bool killedByPlayer;

	private bool parried;

	private bool exploded;

	private bool parryable;

	private Vector3 viewTarget;

	[HideInInspector]
	public bool musicRequested;

	private GoreZone gz;

	private int difficulty;

	private Animator anim;

	public bool enraged;

	public GameObject enrageEffect;

	private int usedAttacks;

	public Material[] originalMaterials;

	public Material[] enrageMaterials;

	[HideInInspector]
	public List<VirtueInsignia> childVi = new List<VirtueInsignia>();

	private VirtueController vc;

	private KeepInBounds kib;

	private bool checkingForCrash;

	[HideInInspector]
	public bool lockRotation;

	[HideInInspector]
	public bool lockPosition;

	private bool hooked;

	private bool homeRunnable;

	public bool cantInstaExplode;

	[HideInInspector]
	public bool fleshDrone;

	private void Start()
	{
		bsm = MonoSingleton<BloodsplatterManager>.Instance;
		eid = GetComponent<EnemyIdentifier>();
		type = eid.enemyType;
		camObj = MonoSingleton<PlayerTracker>.Instance.GetTarget().gameObject;
		rb = GetComponent<Rigidbody>();
		kib = GetComponent<KeepInBounds>();
		if (type == EnemyType.Virtue)
		{
			vc = MonoSingleton<VirtueController>.Instance;
			if (!vc)
			{
				VirtueController virtueController = new GameObject().AddComponent<VirtueController>();
				vc = virtueController;
			}
			if (!crashing)
			{
				vc.currentVirtues++;
			}
		}
		if (!chargeParticle)
		{
			chargeParticle = GetComponentInChildren<ParticleSystem>();
		}
		if (type == EnemyType.Virtue)
		{
			anim = GetComponent<Animator>();
		}
		if (!friendly)
		{
			target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
		}
		dodgeCooldown = Random.Range(0.5f, 3f);
		if (type == EnemyType.Drone)
		{
			attackCooldown = Random.Range(1f, 3f);
		}
		else
		{
			attackCooldown = 1.5f;
		}
		if (!dontStartAware)
		{
			playerSpotted = true;
		}
		if (type == EnemyType.Drone)
		{
			modelTransform = base.transform.Find("drone");
			if ((bool)modelTransform)
			{
				mrs = modelTransform.GetComponentsInChildren<MeshRenderer>();
				origMaterial = mrs[0].material;
			}
			rb.solverIterations *= 3;
			rb.solverVelocityIterations *= 3;
		}
		SlowUpdate();
		if (!musicRequested)
		{
			musicRequested = true;
			MonoSingleton<MusicManager>.Instance.PlayBattleMusic();
		}
		gz = GoreZone.ResolveGoreZone(base.transform);
		if (enraged)
		{
			Enrage();
		}
		if (eid.difficultyOverride >= 0)
		{
			difficulty = eid.difficultyOverride;
		}
		else
		{
			difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		}
	}

	private void OnDisable()
	{
		if (musicRequested)
		{
			musicRequested = false;
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			if ((bool)instance)
			{
				instance.PlayCleanMusic();
			}
		}
	}

	private void OnEnable()
	{
		if (!musicRequested)
		{
			musicRequested = true;
			MonoSingleton<MusicManager>.Instance.PlayBattleMusic();
		}
	}

	private void Update()
	{
		if (!crashing)
		{
			if (BlindEnemies.Blind)
			{
				return;
			}
			if (playerSpotted)
			{
				viewTarget = target.position;
				float num = difficulty / 2;
				if (num == 0f)
				{
					num = 0.25f;
				}
				if (dodgeCooldown > 0f)
				{
					dodgeCooldown = Mathf.MoveTowards(dodgeCooldown, 0f, Time.deltaTime * num);
				}
				else if (!stationary && !lockPosition)
				{
					dodgeCooldown = Random.Range(1f, 3f);
					RandomDodge();
				}
			}
			if ((type == EnemyType.Virtue && (Vector3.Distance(base.transform.position, target.position) < 150f || stationary)) || playerSpotted)
			{
				float num2 = difficulty / 2;
				if (difficulty == 1)
				{
					num2 = 0.75f;
				}
				if (difficulty == 0)
				{
					num2 = 0.5f;
				}
				if (attackCooldown > 0f)
				{
					attackCooldown = Mathf.MoveTowards(attackCooldown, 0f, Time.deltaTime * num2);
				}
				else if ((type != EnemyType.Virtue && projectile != null) || ((bool)vc && vc.cooldown == 0f))
				{
					if ((bool)vc)
					{
						vc.cooldown = 1f;
					}
					parryable = true;
					PlaySound(windUpSound);
					chargeParticle?.Play();
					if (mrs != null && mrs.Length != 0)
					{
						MeshRenderer[] array = mrs;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].material = shootMaterial;
						}
					}
					if (type == EnemyType.Drone)
					{
						attackCooldown = Random.Range(2f, 4f);
						Invoke("Shoot", 0.75f);
					}
					else
					{
						attackCooldown = Random.Range(4f, 6f);
						anim.SetTrigger("Attack");
					}
				}
			}
		}
		if ((bool)eid && eid.hooked && !hooked)
		{
			Hooked();
		}
		else if ((bool)eid && !eid.hooked && hooked)
		{
			Unhooked();
		}
	}

	private void SlowUpdate()
	{
		if (!crashing)
		{
			if (playerSpotted)
			{
				if (UnityEngine.Physics.Raycast(base.transform.position, target.transform.position - base.transform.position, Vector3.Distance(base.transform.position, target.transform.position) - 1f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
				{
					playerSpotted = false;
					PlaySound(loseSound);
					lastKnownPos = target.transform.position;
					blockCooldown = 0f;
					checkCooldown = 0f;
					toLastKnownPos = true;
				}
			}
			else if (!UnityEngine.Physics.Raycast(base.transform.position, target.transform.position - base.transform.position, Vector3.Distance(base.transform.position, target.transform.position) - 1f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				PlaySound(spotSound);
				playerSpotted = true;
			}
		}
		Invoke("SlowUpdate", 0.25f);
	}

	private void FixedUpdate()
	{
		if (rb.velocity.magnitude < 1f && rb.collisionDetectionMode != CollisionDetectionMode.Discrete)
		{
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}
		if (crashing)
		{
			if (type == EnemyType.Virtue)
			{
				if (parried)
				{
					rb.useGravity = false;
					rb.velocity = base.transform.forward * 120f;
				}
			}
			else if (!parried)
			{
				float num = 50f;
				if (difficulty == 1)
				{
					num = 40f;
				}
				else if (difficulty == 0)
				{
					num = 25f;
				}
				rb.AddForce(base.transform.forward * num, ForceMode.Acceleration);
				modelTransform?.Rotate(0f, 0f, 10f, Space.Self);
			}
			else
			{
				rb.velocity = base.transform.forward * 50f;
				modelTransform?.Rotate(0f, 0f, 50f, Space.Self);
			}
		}
		else if (playerSpotted)
		{
			if (type == EnemyType.Drone)
			{
				rb.velocity *= 0.95f;
				if (!stationary && !BlindEnemies.Blind && !lockPosition)
				{
					if (Vector3.Distance(base.transform.position, target.transform.position) > preferredDistanceToTarget)
					{
						rb.AddForce(base.transform.forward * 50f, ForceMode.Acceleration);
					}
					else if (Vector3.Distance(base.transform.position, target.transform.position) < 5f)
					{
						if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
						{
							rb.AddForce(base.transform.forward * -0.1f, ForceMode.Impulse);
						}
						else
						{
							rb.AddForce(base.transform.forward * -50f, ForceMode.Impulse);
						}
					}
				}
			}
			else
			{
				rb.velocity *= 0.975f;
				if (!stationary && !BlindEnemies.Blind && Vector3.Distance(base.transform.position, target.transform.position) > 15f)
				{
					rb.AddForce(base.transform.forward * 10f, ForceMode.Acceleration);
				}
			}
		}
		else if (toLastKnownPos && !stationary && !BlindEnemies.Blind && !lockPosition)
		{
			if (blockCooldown == 0f)
			{
				viewTarget = lastKnownPos;
			}
			else
			{
				blockCooldown = Mathf.MoveTowards(blockCooldown, 0f, 0.01f);
			}
			rb.AddForce(base.transform.forward * 10f, ForceMode.Acceleration);
			if (checkCooldown == 0f && Vector3.Distance(base.transform.position, lastKnownPos) > 5f)
			{
				checkCooldown = 0.1f;
				if (UnityEngine.Physics.BoxCast(base.transform.position - (viewTarget - base.transform.position).normalized, Vector3.one, viewTarget - base.transform.position, base.transform.rotation, 4f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
				{
					blockCooldown = Random.Range(1.5f, 3f);
					Vector3 vector = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
					viewTarget = base.transform.position + vector * 100f;
				}
			}
			else if (Vector3.Distance(base.transform.position, lastKnownPos) <= 3f)
			{
				UnityEngine.Physics.Raycast(base.transform.position, Random.onUnitSphere, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies));
				lastKnownPos = hitInfo.point;
			}
			if (checkCooldown != 0f)
			{
				checkCooldown = Mathf.MoveTowards(checkCooldown, 0f, 0.01f);
			}
		}
		if (!crashing)
		{
			if (!lockRotation)
			{
				Quaternion b = Quaternion.LookRotation(viewTarget - base.transform.position);
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, 0.075f + 0.00025f * Quaternion.Angle(base.transform.rotation, b));
			}
			rb.velocity = Vector3.ClampMagnitude(rb.velocity, 50f);
			if ((bool)kib)
			{
				kib.ValidateMove();
			}
		}
	}

	public void RandomDodge()
	{
		if ((difficulty != 1 || !(Random.Range(0f, 1f) > 0.75f)) && difficulty != 0)
		{
			float num = 50f;
			if (type == EnemyType.Virtue)
			{
				num = 150f;
			}
			Vector3 force = base.transform.up * Random.Range(-5f, 5f) + (base.transform.right * Random.Range(-5f, 5f)).normalized * num;
			rb.AddForce(force, ForceMode.Impulse);
		}
	}

	public void GetHurt(Vector3 force, float multiplier)
	{
		bool flag = false;
		if (!crashing)
		{
			if (eid.hitter == "shotgunzone" && !parryable && health > 4f)
			{
				return;
			}
			if ((eid.hitter == "shotgunzone" || eid.hitter == "punch") && parryable)
			{
				if (!InvincibleEnemies.Enabled && !eid.blessed)
				{
					multiplier = 4f;
				}
				MonoSingleton<FistControl>.Instance.currentPunch.Parry();
				parryable = false;
			}
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= 1f * multiplier;
			}
			if (eid == null)
			{
				eid = GetComponent<EnemyIdentifier>();
			}
			if (health <= 0f)
			{
				flag = true;
			}
			if (((homeRunnable && !fleshDrone) & flag) && (eid.hitter == "punch" || eid.hitter == "heavypunch"))
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(100, "HOMERUN");
				MonoSingleton<StyleCalculator>.Instance.AddToMultiKill();
			}
			else if (eid.hitter != "enemy" && multiplier != 0f)
			{
				if (scalc == null)
				{
					scalc = MonoSingleton<StyleCalculator>.Instance;
				}
				if ((bool)scalc)
				{
					scalc.HitCalculator(eid.hitter, "drone", "", flag, base.gameObject);
				}
			}
			if (health <= 0f && !crashing)
			{
				parryable = false;
				Death();
				if (eid.hitter != "punch" && eid.hitter != "heavypunch")
				{
					crashTarget = target.transform.position;
				}
				else
				{
					base.transform.position += force.normalized;
					crashTarget = base.transform.position + force;
					rb.velocity = force.normalized * 40f;
				}
				base.transform.LookAt(crashTarget);
				if (aud == null)
				{
					aud = GetComponent<AudioSource>();
				}
				if (type == EnemyType.Drone)
				{
					aud.clip = deathSound;
					aud.volume = 0.75f;
					aud.pitch = Random.Range(0.85f, 1.35f);
					aud.priority = 11;
					aud.Play();
				}
				else
				{
					PlaySound(deathSound);
				}
				Invoke("CanInterruptCrash", 0.5f);
				Invoke("Explode", 5f);
				return;
			}
			PlaySound(hurtSound);
			GameObject gameObject = null;
			Bloodsplatter bloodsplatter = null;
			if (multiplier != 0f)
			{
				gameObject = bsm.GetGore(GoreType.Body, eid.underwater, eid.sandified, eid.blessed);
				gameObject.transform.position = base.transform.position;
				gameObject.SetActive(value: true);
				gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
				if (eid.hitter == "drill")
				{
					gameObject.transform.localScale *= 2f;
				}
				bloodsplatter = gameObject.GetComponent<Bloodsplatter>();
			}
			if (health > 0f)
			{
				if ((bool)bloodsplatter)
				{
					bloodsplatter.GetReady();
				}
				rb.velocity /= 10f;
				rb.AddForce(force.normalized * (force.magnitude / 100f), ForceMode.Impulse);
				rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				if (rb.velocity.magnitude > 50f)
				{
					rb.velocity = Vector3.ClampMagnitude(rb.velocity, 50f);
				}
			}
			if (multiplier >= 1f)
			{
				if ((bool)bloodsplatter)
				{
					bloodsplatter.hpAmount = 30;
				}
				for (int i = 0; (float)i <= multiplier; i++)
				{
					Object.Instantiate(gib, base.transform.position, Random.rotation).transform.SetParent(gz.goreZone, worldPositionStays: true);
				}
			}
			if (OptionsMenuToManager.bloodEnabled && (bool)gameObject && gameObject.TryGetComponent<ParticleSystem>(out var component))
			{
				component.Play();
			}
		}
		else if (eid.hitter == "punch" && !parried)
		{
			parried = true;
			rb.velocity = Vector3.zero;
			base.transform.rotation = camObj.transform.rotation;
			Punch currentPunch = MonoSingleton<FistControl>.Instance.currentPunch;
			currentPunch.GetComponent<Animator>().Play("Hook", -1, 0.065f);
			currentPunch.Parry();
			if (type == EnemyType.Virtue && TryGetComponent<Collider>(out var component2))
			{
				component2.isTrigger = true;
			}
		}
		else if (multiplier >= 1f || canInterruptCrash)
		{
			Explode();
		}
	}

	public void PlaySound(AudioClip clippe)
	{
		if ((bool)clippe)
		{
			if (aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = clippe;
			if (type == EnemyType.Drone)
			{
				aud.volume = 0.5f;
				aud.pitch = Random.Range(0.85f, 1.35f);
			}
			aud.priority = 12;
			aud.Play();
		}
	}

	public void Explode()
	{
		if (exploded || !base.gameObject.activeInHierarchy || (cantInstaExplode && !canInterruptCrash))
		{
			return;
		}
		exploded = true;
		GameObject gameObject = Object.Instantiate(explosion, base.transform.position, Quaternion.identity);
		gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
		if (killedByPlayer)
		{
			Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].friendlyFire = true;
			}
		}
		if (!crashing)
		{
			Death();
		}
		else if (eid.drillers.Count > 0)
		{
			foreach (Harpoon driller in eid.drillers)
			{
				Object.Destroy(driller.gameObject);
			}
		}
		Object.Destroy(base.gameObject);
		if (musicRequested)
		{
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			if ((bool)instance)
			{
				instance.PlayCleanMusic();
			}
		}
	}

	private void Death()
	{
		if (crashing)
		{
			return;
		}
		crashing = true;
		if (rb.isKinematic)
		{
			rb.isKinematic = false;
		}
		if (type == EnemyType.Virtue)
		{
			rb.velocity = Vector3.zero;
			rb.AddForce(Vector3.up * 10f, ForceMode.VelocityChange);
			rb.useGravity = true;
			vc.currentVirtues--;
			if (childVi.Count > 0)
			{
				for (int i = 0; i < childVi.Count; i++)
				{
					if (childVi[i] != null && (bool)childVi[i].gameObject)
					{
						Object.Destroy(childVi[i].gameObject);
					}
				}
			}
		}
		if (eid.hitter != "enemy")
		{
			killedByPlayer = true;
		}
		if (!eid.dontCountAsKills)
		{
			if (gz != null && gz.checkpoint != null)
			{
				gz.AddDeath();
				gz.checkpoint.sm.kills++;
			}
			else
			{
				MonoSingleton<StatsManager>.Instance.kills++;
			}
		}
		GameObject gore = bsm.GetGore(GoreType.Head, eid.underwater, eid.sandified, eid.blessed);
		if ((bool)gore)
		{
			gore.transform.position = base.transform.position;
			gore.GetComponent<Bloodsplatter>()?.GetReady();
			if (OptionsMenuToManager.bloodEnabled)
			{
				gore.GetComponent<ParticleSystem>()?.Play();
			}
			gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
			gore.SetActive(value: true);
		}
		if (eid.hitter == "drill")
		{
			gore.transform.localScale *= 2f;
		}
		if (!eid.dontCountAsKills)
		{
			ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
			if (componentInParent != null)
			{
				componentInParent.AddDeadEnemy();
			}
		}
	}

	public void Shoot()
	{
		if (!crashing && (bool)projectile)
		{
			parryable = false;
			int num = 35;
			if (difficulty >= 3)
			{
				num = 45;
			}
			else if (difficulty == 1)
			{
				num = 25;
			}
			else if (difficulty == 0)
			{
				num = 15;
			}
			GameObject gameObject = Object.Instantiate(projectile, base.transform.position + base.transform.forward, base.transform.rotation);
			gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x, gameObject.transform.rotation.eulerAngles.y, Random.Range(0, 360));
			gameObject.transform.localScale *= 0.5f;
			Projectile component = gameObject.GetComponent<Projectile>();
			component.safeEnemyType = EnemyType.Drone;
			component.speed = num;
			GameObject gameObject2 = Object.Instantiate(projectile, gameObject.transform.position + gameObject.transform.up, gameObject.transform.rotation);
			if (difficulty > 2)
			{
				gameObject2.transform.rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x + 10f, gameObject.transform.rotation.eulerAngles.y, gameObject.transform.rotation.eulerAngles.z);
			}
			gameObject2.transform.localScale *= 0.5f;
			Projectile component2 = gameObject2.GetComponent<Projectile>();
			component2.safeEnemyType = EnemyType.Drone;
			component2.speed = num;
			gameObject2 = Object.Instantiate(projectile, gameObject.transform.position - gameObject.transform.up, gameObject.transform.rotation);
			if (difficulty > 2)
			{
				gameObject2.transform.rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x - 10f, gameObject.transform.rotation.eulerAngles.y, gameObject.transform.rotation.eulerAngles.z);
			}
			gameObject2.transform.localScale *= 0.5f;
			Projectile component3 = gameObject2.GetComponent<Projectile>();
			component3.safeEnemyType = EnemyType.Drone;
			component3.speed = num;
			MeshRenderer[] array = mrs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].material = origMaterial;
			}
		}
	}

	public void SpawnInsignia()
	{
		if (!crashing)
		{
			parryable = false;
			GameObject gameObject = Object.Instantiate(projectile, target.transform.position, Quaternion.identity);
			VirtueInsignia component = gameObject.GetComponent<VirtueInsignia>();
			component.target = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
			component.parentDrone = this;
			component.hadParent = true;
			chargeParticle.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmittingAndClear);
			if (enraged)
			{
				component.predictive = true;
			}
			if (difficulty == 1)
			{
				component.windUpSpeedMultiplier = 0.875f;
			}
			else if (difficulty == 0)
			{
				component.windUpSpeedMultiplier = 0.75f;
			}
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
			{
				gameObject.transform.localScale *= 0.75f;
				component.windUpSpeedMultiplier *= 0.875f;
			}
			usedAttacks++;
			if (((difficulty > 2 && usedAttacks > 2) || (difficulty == 2 && usedAttacks > 4)) && !enraged && vc.currentVirtues < 3)
			{
				Invoke("Enrage", 3f);
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (crashing && (collision.gameObject.layer == 0 || collision.gameObject.layer == 8 || collision.gameObject.layer == 24 || collision.gameObject.tag == "Player" || collision.gameObject.layer == 10 || collision.gameObject.layer == 11 || collision.gameObject.layer == 12))
		{
			Explode();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!crashing)
		{
			return;
		}
		if (other.gameObject.layer == 0 || other.gameObject.layer == 8 || other.gameObject.layer == 24 || other.gameObject.tag == "Player")
		{
			Explode();
		}
		else
		{
			if ((other.gameObject.layer != 10 && other.gameObject.layer != 11 && other.gameObject.layer != 12) || checkingForCrash)
			{
				return;
			}
			checkingForCrash = true;
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
				enemyIdentifier.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 100f, base.transform.position, 5f, tryForExplode: true);
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
					checkingForCrash = false;
				}
				else
				{
					Explode();
				}
			}
			else
			{
				checkingForCrash = false;
			}
		}
	}

	private void CanInterruptCrash()
	{
		canInterruptCrash = true;
	}

	private void Enrage()
	{
		if (type != EnemyType.Drone)
		{
			Object.Instantiate(enrageEffect, base.transform.position, base.transform.rotation).transform.SetParent(base.transform, worldPositionStays: true);
			enraged = true;
			EnemySimplifier[] componentsInChildren = GetComponentsInChildren<EnemySimplifier>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enraged = true;
			}
		}
	}

	public void Hooked()
	{
		hooked = true;
		lockPosition = true;
		homeRunnable = true;
		CancelInvoke("DelayedUnhooked");
	}

	public void Unhooked()
	{
		hooked = false;
		Invoke("DelayedUnhooked", 0.25f);
	}

	private void DelayedUnhooked()
	{
		if (!crashing)
		{
			Invoke("NoMoreHomeRun", 0.5f);
		}
		lockPosition = false;
	}

	private void NoMoreHomeRun()
	{
		if (!crashing)
		{
			homeRunnable = false;
		}
	}
}
