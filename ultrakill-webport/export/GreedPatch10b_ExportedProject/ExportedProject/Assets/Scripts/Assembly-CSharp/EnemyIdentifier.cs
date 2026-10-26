using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.Events;

public class EnemyIdentifier : MonoBehaviour
{
	[HideInInspector]
	public Zombie zombie;

	[HideInInspector]
	public SpiderBody spider;

	[HideInInspector]
	public Machine machine;

	[HideInInspector]
	public Statue statue;

	[HideInInspector]
	public Wicked wicked;

	[HideInInspector]
	public Drone drone;

	public EnemyClass enemyClass;

	public EnemyType enemyType;

	public bool spawnIn;

	public GameObject spawnEffect;

	public float health;

	[HideInInspector]
	public string hitter;

	[HideInInspector]
	public List<HitterAttribute> hitterAttributes = new List<HitterAttribute>();

	[HideInInspector]
	public List<string> hitterWeapons = new List<string>();

	public string[] weaknesses;

	public float[] weaknessMultipliers;

	public float totalDamageMultiplier = 1f;

	public GameObject weakPoint;

	[HideInInspector]
	public bool exploded;

	public bool dead;

	[HideInInspector]
	public DoorController usingDoor;

	public bool ignoredByEnemies;

	public GameObject ineffectiveSound;

	private EnemyIdentifierIdentifier[] limbs;

	[HideInInspector]
	public int nailsAmount;

	[HideInInspector]
	public List<Nail> nails = new List<Nail>();

	public bool useBrakes;

	public bool bigEnemy;

	public bool unbounceable;

	public bool poise;

	private bool beingZapped;

	[HideInInspector]
	public bool pulledByMagnet;

	[HideInInspector]
	public List<Magnet> stuckMagnets = new List<Magnet>();

	[HideInInspector]
	public List<Harpoon> drillers = new List<Harpoon>();

	[HideInInspector]
	public bool underwater;

	public bool sandified;

	[HideInInspector]
	public bool checkingSpawnStatus = true;

	public bool flying;

	public bool dontCountAsKills;

	public bool dontUnlockBestiary;

	public bool specialOob;

	public GameObject[] activateOnDeath;

	public UnityEvent onDeath;

	private BloodsplatterManager bsm;

	[HideInInspector]
	public GroundCheckEnemy gce;

	private GoreZone gz;

	private List<GameObject> sandifiedParticles = new List<GameObject>();

	private List<GameObject> blessingGlows = new List<GameObject>();

	[HideInInspector]
	public EnemyIdentifier buffTargeter;

	public int difficultyOverride = -1;

	public bool hooked;

	public bool hookIgnore;

	[HideInInspector]
	public List<Flammable> burners;

	[HideInInspector]
	public bool harpooned;

	public bool blessed;

	private void Awake()
	{
		health = 999f;
		ForceGetHealth();
	}

	private GoreZone GetGoreZone()
	{
		if ((bool)gz)
		{
			return gz;
		}
		gz = GoreZone.ResolveGoreZone(base.transform);
		return gz;
	}

	public void ForceGetHealth()
	{
		if (enemyType == EnemyType.Drone || enemyType == EnemyType.Virtue)
		{
			if (!drone)
			{
				drone = GetComponent<Drone>();
			}
			if ((bool)drone)
			{
				health = drone.health;
			}
			return;
		}
		if (enemyType == EnemyType.MaliciousFace)
		{
			if (!spider)
			{
				spider = GetComponent<SpiderBody>();
			}
			if ((bool)spider)
			{
				health = spider.health;
			}
			return;
		}
		switch (enemyClass)
		{
		case EnemyClass.Husk:
			if (!zombie)
			{
				zombie = GetComponent<Zombie>();
			}
			if ((bool)zombie)
			{
				health = zombie.health;
			}
			break;
		case EnemyClass.Demon:
			if (!statue)
			{
				statue = GetComponent<Statue>();
			}
			if ((bool)statue)
			{
				health = statue.health;
			}
			break;
		case EnemyClass.Machine:
			if (!machine)
			{
				machine = GetComponent<Machine>();
			}
			if ((bool)machine)
			{
				health = machine.health;
			}
			break;
		}
	}

	private void Start()
	{
		if (spawnIn && (bool)spawnEffect)
		{
			Collider component = GetComponent<Collider>();
			if ((bool)component)
			{
				Object.Instantiate(spawnEffect, component.bounds.center, base.transform.rotation);
			}
			else
			{
				Object.Instantiate(spawnEffect, base.transform.position + Vector3.up * 1.5f, base.transform.rotation);
			}
			spawnIn = false;
		}
		if (!dontUnlockBestiary)
		{
			MonoSingleton<BestiaryData>.Instance.SetEnemy(enemyType, 1);
		}
		bsm = MonoSingleton<BloodsplatterManager>.Instance;
		if (checkingSpawnStatus)
		{
			if (sandified && enemyType != EnemyType.Stalker && !dead)
			{
				Sandify(ignorePrevious: true);
			}
			if (blessed && !dead)
			{
				Bless(ignorePrevious: true);
			}
			checkingSpawnStatus = false;
		}
		if (!MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies().Contains(this))
		{
			MonoSingleton<EnemyTracker>.Instance.AddEnemy(this);
		}
		gce = GetComponentInChildren<GroundCheckEnemy>(includeInactive: true);
	}

	private void Update()
	{
		ForceGetHealth();
		CheckBurners();
	}

	private void CheckBurners()
	{
		if (burners == null || burners.Count <= 0)
		{
			return;
		}
		for (int num = burners.Count - 1; num >= 0; num--)
		{
			if (burners[num] == null || burners[num].secondary || !burners[num].burning)
			{
				burners.RemoveAt(num);
			}
		}
	}

	public void DeliverDamage(GameObject target, Vector3 force, Vector3 hitPoint, float multiplier, bool tryForExplode, float critMultiplier = 0f)
	{
		if (target == base.gameObject)
		{
			EnemyIdentifierIdentifier componentInChildren = GetComponentInChildren<EnemyIdentifierIdentifier>();
			if (componentInChildren != null)
			{
				target = componentInChildren.gameObject;
			}
		}
		multiplier *= totalDamageMultiplier;
		if (weaknesses.Length != 0)
		{
			for (int i = 0; i < weaknesses.Length; i++)
			{
				if (hitter == weaknesses[i])
				{
					multiplier *= weaknessMultipliers[i];
				}
			}
		}
		CheckBurners();
		if (burners.Count > 0 && hitter != "fire" && hitter != "explosion" && hitter != "ffexplosion")
		{
			multiplier *= 1.5f;
		}
		if (nails.Count > 10)
		{
			for (int j = 0; j < nails.Count - 10; j++)
			{
				if (nails[j] != null)
				{
					Object.Destroy(nails[j].gameObject);
				}
				nails.RemoveAt(j);
			}
		}
		if (!beingZapped && hitterAttributes.Contains(HitterAttribute.Electricity) && nailsAmount > 0)
		{
			beingZapped = true;
			foreach (Nail nail in nails)
			{
				if (nail != null)
				{
					nail.Zap();
				}
			}
			Invoke("AfterShock", 1f);
		}
		if (pulledByMagnet && hitter != "deathzone")
		{
			pulledByMagnet = false;
		}
		bool flag = false;
		switch (enemyType)
		{
		case EnemyType.MaliciousFace:
			if (spider == null)
			{
				spider = GetComponent<SpiderBody>();
			}
			if (spider == null)
			{
				return;
			}
			if (hitter != "explosion" && hitter != "ffexplosion")
			{
				spider.GetHurt(target, force, hitPoint, multiplier);
			}
			if (spider.health <= 0f)
			{
				Death();
			}
			health = spider.health;
			flag = true;
			break;
		case EnemyType.Wicked:
			if (wicked == null)
			{
				wicked = GetComponent<Wicked>();
			}
			if (wicked == null)
			{
				return;
			}
			wicked.GetHit();
			flag = true;
			break;
		case EnemyType.Drone:
		case EnemyType.Virtue:
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			if (drone == null)
			{
				return;
			}
			drone.GetHurt(force, multiplier);
			health = drone.health;
			if (health <= 0f)
			{
				Death();
			}
			flag = true;
			break;
		}
		if (!flag)
		{
			switch (enemyClass)
			{
			case EnemyClass.Husk:
				if (zombie == null)
				{
					zombie = GetComponent<Zombie>();
				}
				if (zombie == null)
				{
					return;
				}
				zombie.GetHurt(target, force, multiplier, critMultiplier);
				if (tryForExplode && zombie.health <= 0f && !exploded)
				{
					Explode();
				}
				if (zombie.health <= 0f)
				{
					Death();
				}
				health = zombie.health;
				break;
			case EnemyClass.Machine:
				if (machine == null)
				{
					machine = GetComponent<Machine>();
				}
				if (machine == null)
				{
					return;
				}
				machine.GetHurt(target, force, multiplier, critMultiplier);
				if (tryForExplode && machine.health <= 0f && (machine.symbiote == null || machine.symbiote.health <= 0f) && !machine.dontDie && !exploded)
				{
					Explode();
				}
				if (machine.health <= 0f && (machine.symbiote == null || machine.symbiote.health <= 0f))
				{
					Death();
				}
				health = machine.health;
				break;
			case EnemyClass.Demon:
				if (statue == null)
				{
					statue = GetComponent<Statue>();
				}
				if (statue == null)
				{
					return;
				}
				statue.GetHurt(target, force, multiplier, critMultiplier, hitPoint);
				if (tryForExplode && statue.health <= 0f && !exploded)
				{
					Explode();
				}
				if (statue.health <= 0f)
				{
					Death();
				}
				health = statue.health;
				break;
			}
		}
		hitterAttributes.Clear();
	}

	private void AfterShock()
	{
		float num = nailsAmount / 15;
		if (num > 6f)
		{
			num = 6f;
		}
		GoreZone goreZone = GetGoreZone();
		foreach (Nail nail in nails)
		{
			if (nail == null)
			{
				continue;
			}
			GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Small, underwater, sandified, blessed);
			if ((bool)gore && (bool)goreZone)
			{
				gore.transform.position = nail.transform.position;
				gore.SetActive(value: true);
				Bloodsplatter component = gore.GetComponent<Bloodsplatter>();
				gore.transform.SetParent(goreZone.goreZone, worldPositionStays: true);
				if (component != null && !dead)
				{
					component.GetReady();
				}
			}
			Object.Destroy(nail.gameObject);
		}
		nails.Clear();
		nailsAmount = 0;
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		if (!dead)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(Mathf.RoundToInt(num * 15f), "<color=cyan>CONDUCTOR</color>");
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].gameObject != base.gameObject)
			{
				hitter = "aftershock";
				hitterAttributes.Add(HitterAttribute.Electricity);
				DeliverDamage(componentsInChildren[i].gameObject, Vector3.zero, base.transform.position, num, tryForExplode: true);
				break;
			}
		}
		beingZapped = false;
		MonoSingleton<CameraController>.Instance.CameraShake(1f);
	}

	public void Death()
	{
		if (dead)
		{
			return;
		}
		dead = true;
		if (hitterWeapons.Count > 1)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(50, "<color=cyan>ARSENAL</color>");
		}
		GameObject[] array = activateOnDeath;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		onDeath?.Invoke();
		if (!dontUnlockBestiary)
		{
			MonoSingleton<BestiaryData>.Instance.SetEnemy(enemyType);
		}
		if (health > 0f)
		{
			health = 0f;
		}
		if (stuckMagnets.Count > 0)
		{
			for (int num = stuckMagnets.Count - 1; num >= 0; num--)
			{
				if (stuckMagnets[num] != null)
				{
					Object.Destroy(stuckMagnets[num].gameObject);
				}
			}
		}
		if (drillers.Count > 0 && enemyType != EnemyType.MaliciousFace)
		{
			foreach (Harpoon driller in drillers)
			{
				driller.DelayedDestroyIfOnCorpse();
			}
		}
		if (usingDoor != null)
		{
			usingDoor.Close();
			usingDoor = null;
		}
		Desandify(visualOnly: true);
		Unbless();
	}

	public void InstaKill()
	{
		if (dead)
		{
			return;
		}
		Death();
		if (pulledByMagnet)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(240, "<color=cyan>CATAPULTED</color>");
		}
		dead = true;
		bool flag = false;
		switch (enemyType)
		{
		case EnemyType.Drone:
		case EnemyType.Virtue:
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			drone.GetHurt(Vector3.zero, 999f);
			drone.Explode();
			flag = true;
			break;
		case EnemyType.MaliciousFace:
			if (spider == null)
			{
				spider = GetComponent<SpiderBody>();
			}
			if (!spider.dead)
			{
				spider.Die();
			}
			flag = true;
			break;
		}
		if (!flag)
		{
			switch (enemyClass)
			{
			case EnemyClass.Husk:
				if (zombie == null)
				{
					zombie = GetComponent<Zombie>();
				}
				if (!zombie.limp)
				{
					zombie.GoLimp();
				}
				break;
			case EnemyClass.Machine:
				if (machine == null)
				{
					machine = GetComponent<Machine>();
				}
				if (!machine.limp)
				{
					machine.GoLimp();
				}
				break;
			case EnemyClass.Demon:
				if (statue == null)
				{
					statue = GetComponent<Statue>();
				}
				if (!statue.limp)
				{
					statue.GoLimp();
				}
				break;
			}
		}
		if (usingDoor != null)
		{
			usingDoor.Close();
			usingDoor = null;
		}
	}

	public void Explode()
	{
		bool flag = dead;
		if (!dead)
		{
			Death();
		}
		if (enemyType == EnemyType.MaliciousFace)
		{
			if (spider == null)
			{
				spider = GetComponent<SpiderBody>();
			}
			if (!spider.dead)
			{
				hitter = "breaker";
				spider.Die();
			}
			else
			{
				spider.BreakCorpse();
			}
		}
		else if (enemyType == EnemyType.Drone || enemyType == EnemyType.Virtue)
		{
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			drone.Explode();
		}
		else if (enemyClass == EnemyClass.Husk)
		{
			if (zombie == null)
			{
				zombie = GetComponent<Zombie>();
			}
			if (exploded || !zombie || zombie.chestExploding)
			{
				return;
			}
			exploded = true;
			if (zombie.chestExploding)
			{
				zombie.ChestExplodeEnd();
			}
			if (!flag && pulledByMagnet)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(240, "<color=cyan>CATAPULTED</color>");
			}
			Transform[] componentsInChildren = zombie.GetComponentsInChildren<Transform>(includeInactive: true);
			GoreZone goreZone = GetGoreZone();
			bool flag2 = false;
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				if (transform.gameObject.tag == "Limb")
				{
					Object.Destroy(transform.GetComponent<CharacterJoint>());
					transform.transform.SetParent(goreZone.gibZone, worldPositionStays: true);
					if (!flag2)
					{
						zombie.GetHurt(transform.gameObject, (base.transform.position - transform.position).normalized * 1000f, 1E+09f, 1f);
					}
				}
				else if (transform.gameObject.tag == "Head" || transform.gameObject.tag == "EndLimb")
				{
					flag2 = true;
					zombie.GetHurt(transform.gameObject, (base.transform.position - transform.position).normalized * 1000f, 1E+09f, 1f);
				}
			}
			if (!flag2)
			{
				zombie.GoLimp();
			}
			health = zombie.health;
			if (usingDoor != null)
			{
				usingDoor.Close();
				usingDoor = null;
			}
		}
		else if (enemyClass == EnemyClass.Machine && enemyType != EnemyType.Drone)
		{
			if (machine == null)
			{
				machine = GetComponent<Machine>();
			}
			if (exploded || !machine)
			{
				return;
			}
			exploded = true;
			bool flag3 = false;
			if (machine.dismemberment)
			{
				Collider[] componentsInChildren2 = machine.GetComponentsInChildren<Collider>();
				List<EnemyIdentifierIdentifier> list = new List<EnemyIdentifierIdentifier>();
				Collider[] array2 = componentsInChildren2;
				for (int i = 0; i < array2.Length; i++)
				{
					EnemyIdentifierIdentifier component = array2[i].GetComponent<EnemyIdentifierIdentifier>();
					if (component != null)
					{
						list.Add(component);
					}
				}
				GoreZone goreZone2 = GetGoreZone();
				foreach (EnemyIdentifierIdentifier item in list)
				{
					if (item.gameObject.tag == "Limb")
					{
						CharacterJoint component2 = item.GetComponent<CharacterJoint>();
						if (component2 != null)
						{
							Object.Destroy(component2);
						}
						item.transform.SetParent(goreZone2.gibZone, worldPositionStays: true);
					}
					else if (item.gameObject.tag == "Head" || item.gameObject.tag == "EndLimb")
					{
						flag3 = true;
						machine.GetHurt(item.gameObject, (base.transform.position - item.transform.position).normalized * 1000f, 999f, 1f);
					}
				}
			}
			if (!flag3)
			{
				machine.GoLimp();
			}
			health = machine.health;
			if (usingDoor != null)
			{
				usingDoor.Close();
				usingDoor = null;
			}
		}
		else
		{
			if (enemyClass != EnemyClass.Demon)
			{
				return;
			}
			if (statue == null)
			{
				statue = GetComponent<Statue>();
			}
			if (!exploded)
			{
				exploded = true;
				if (!statue.limp)
				{
					statue.GoLimp();
				}
				health = statue.health;
			}
		}
	}

	public void Splatter()
	{
		if (InvincibleEnemies.Enabled || blessed)
		{
			return;
		}
		if (enemyType == EnemyType.MaliciousFace)
		{
			if (spider == null)
			{
				spider = GetComponent<SpiderBody>();
			}
			spider.Die();
			spider.BreakCorpse();
		}
		else if (enemyType == EnemyType.Drone || enemyType == EnemyType.Virtue)
		{
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			drone.GetHurt(Vector3.zero, 999f);
			if (enemyType == EnemyType.Virtue)
			{
				drone.Explode();
			}
		}
		else
		{
			switch (enemyClass)
			{
			case EnemyClass.Husk:
				if (zombie == null)
				{
					zombie = GetComponent<Zombie>();
				}
				break;
			case EnemyClass.Demon:
				if (statue == null)
				{
					statue = GetComponent<Statue>();
				}
				break;
			case EnemyClass.Machine:
				if (machine == null)
				{
					machine = GetComponent<Machine>();
				}
				break;
			}
		}
		bool flag = dead;
		if (enemyClass == EnemyClass.Machine && (bool)machine && !machine.dismemberment)
		{
			InstaKill();
		}
		else if (!exploded && (enemyClass != EnemyClass.Husk || !zombie.chestExploding))
		{
			exploded = true;
			limbs = GetComponentsInChildren<EnemyIdentifierIdentifier>();
			if (!flag)
			{
				SendMessage("GoLimp", SendMessageOptions.DontRequireReceiver);
				StyleHUD instance = MonoSingleton<StyleHUD>.Instance;
				if (pulledByMagnet)
				{
					instance.AddPoints(120, "<color=cyan>CATAPULTED</color>");
				}
				instance.AddPoints(100, "SPLATTERED");
				base.transform.Rotate(new Vector3(90f, 0f, 0f));
			}
			GameObject gore = bsm.GetGore(GoreType.Splatter, underwater, sandified, blessed);
			gore.transform.position = base.transform.position + Vector3.up;
			GoreZone goreZone = GetGoreZone();
			if (goreZone != null && goreZone.goreZone != null)
			{
				gore.transform.SetParent(goreZone.goreZone, worldPositionStays: true);
			}
			gore.GetComponent<Bloodsplatter>()?.GetReady();
			EnemyIdentifierIdentifier[] array = limbs;
			foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in array)
			{
				if (enemyIdentifierIdentifier.gameObject.tag == "Body" || enemyIdentifierIdentifier.gameObject.tag == "Limb" || enemyIdentifierIdentifier.gameObject.tag == "Head" || enemyIdentifierIdentifier.gameObject.tag == "EndLimb")
				{
					Object.Destroy(enemyIdentifierIdentifier.GetComponent<CharacterJoint>());
					enemyIdentifierIdentifier.transform.SetParent(GetGoreZone().gibZone, worldPositionStays: true);
					Rigidbody component = enemyIdentifierIdentifier.GetComponent<Rigidbody>();
					if (component != null)
					{
						component.velocity = Vector3.zero;
						enemyIdentifierIdentifier.transform.position = new Vector3(enemyIdentifierIdentifier.transform.position.x, base.transform.position.y + 0.1f, enemyIdentifierIdentifier.transform.position.z);
						Vector3 vector = new Vector3(base.transform.position.x - enemyIdentifierIdentifier.transform.position.x, 0f, base.transform.position.z - enemyIdentifierIdentifier.transform.position.z);
						component.AddForce(vector * 15f, ForceMode.VelocityChange);
						component.constraints = RigidbodyConstraints.FreezePositionY;
					}
				}
			}
			Invoke("StopSplatter", 1f);
			if (usingDoor != null)
			{
				usingDoor.Close();
				usingDoor = null;
			}
		}
		Death();
	}

	public void StopSplatter()
	{
		EnemyIdentifierIdentifier[] array = limbs;
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in array)
		{
			if (enemyIdentifierIdentifier != null)
			{
				Rigidbody component = enemyIdentifierIdentifier.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.constraints = RigidbodyConstraints.None;
				}
			}
		}
	}

	public void Sandify(bool ignorePrevious = false)
	{
		if (!ignorePrevious && sandified)
		{
			return;
		}
		sandified = true;
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
		{
			GameObject gameObject = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.sandDrip, enemyIdentifierIdentifier.transform.position, enemyIdentifierIdentifier.transform.rotation);
			Collider component = enemyIdentifierIdentifier.GetComponent<Collider>();
			if ((bool)component)
			{
				gameObject.transform.localScale = component.bounds.size;
			}
			gameObject.transform.SetParent(enemyIdentifierIdentifier.transform, worldPositionStays: true);
			sandifiedParticles.Add(gameObject);
		}
		Renderer[] componentsInChildren2 = GetComponentsInChildren<Renderer>();
		Collider component2 = GetComponent<Collider>();
		if ((bool)component2)
		{
			Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.sandificationEffect, component2.bounds.center, Quaternion.identity);
		}
		else
		{
			Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.sandificationEffect, base.transform.position, Quaternion.identity);
		}
		Renderer[] array = componentsInChildren2;
		for (int i = 0; i < array.Length; i++)
		{
			Material[] materials = array[i].materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j].SetFloat("_HasSandBuff", 1f);
			}
		}
	}

	public void Desandify(bool visualOnly = false)
	{
		if (!visualOnly)
		{
			sandified = false;
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j].SetFloat("_HasSandBuff", 0f);
			}
		}
		foreach (GameObject sandifiedParticle in sandifiedParticles)
		{
			Object.Destroy(sandifiedParticle);
		}
		sandifiedParticles.Clear();
	}

	public void Bless(bool ignorePrevious = false)
	{
		if (!ignorePrevious && blessed)
		{
			return;
		}
		blessed = true;
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
		{
			GameObject gameObject = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.blessingGlow, enemyIdentifierIdentifier.transform.position, enemyIdentifierIdentifier.transform.rotation);
			Collider component = enemyIdentifierIdentifier.GetComponent<Collider>();
			if ((bool)component)
			{
				gameObject.transform.localScale = component.bounds.size;
			}
			gameObject.transform.SetParent(enemyIdentifierIdentifier.transform, worldPositionStays: true);
			blessingGlows.Add(gameObject);
		}
	}

	public void Unbless(bool visualOnly = false)
	{
		if (!visualOnly)
		{
			blessed = false;
		}
		foreach (GameObject blessingGlow in blessingGlows)
		{
			Object.Destroy(blessingGlow);
		}
		blessingGlows.Clear();
	}

	public static bool CheckHurtException(EnemyType attacker, EnemyType receiver)
	{
		if ((attacker == EnemyType.Stalker && receiver != EnemyType.Swordsmachine) || (receiver == EnemyType.Stalker && attacker != EnemyType.Swordsmachine))
		{
			return true;
		}
		if ((attacker == EnemyType.Filth || attacker == EnemyType.Stray || attacker == EnemyType.Schism || attacker == EnemyType.Soldier) && (receiver == EnemyType.Filth || receiver == EnemyType.Stray || receiver == EnemyType.Schism || receiver == EnemyType.Soldier))
		{
			return true;
		}
		if ((attacker == EnemyType.Drone && receiver == EnemyType.FleshPrison) || (receiver == EnemyType.Drone && attacker == EnemyType.FleshPrison))
		{
			return true;
		}
		if (receiver == EnemyType.Sisyphus)
		{
			return true;
		}
		return false;
	}
}
