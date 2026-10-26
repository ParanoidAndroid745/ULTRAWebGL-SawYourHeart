using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	public static float globalSizeMulti = 1f;

	public bool enemy;

	public bool harmless;

	public bool lowQuality;

	private CameraController cc;

	private Light light;

	private MeshRenderer mr;

	private Color materialColor;

	private bool fading;

	public float speed;

	public float maxSize;

	private LayerMask lmask;

	public int damage;

	public float enemyDamageMultiplier;

	[HideInInspector]
	public int playerDamageOverride = -1;

	public GameObject explosionChunk;

	public bool ignite;

	public bool friendlyFire;

	private List<Collider> hitColliders = new List<Collider>();

	public string hitterWeapon;

	public bool halved;

	private SphereCollider scol;

	public AffectedSubjects canHit;

	private bool hasHitPlayer;

	public bool rocketExplosion;

	public List<EnemyType> toIgnore;

	private RaycastHit neoRhit;

	[HideInInspector]
	public bool ultrabooster;

	private void Start()
	{
		cc = MonoSingleton<CameraController>.Instance;
		float num = Vector3.Distance(base.transform.position, cc.transform.position);
		float num2 = 1f;
		if (damage == 0)
		{
			num2 = 0.25f;
		}
		if (num < 3f * maxSize)
		{
			cc.CameraShake(1.5f * num2);
		}
		else if (num < 85f)
		{
			cc.CameraShake((1.5f - (num - 20f) / 65f * 1.5f) / 6f * maxSize * num2);
		}
		scol = GetComponent<SphereCollider>();
		if ((bool)scol)
		{
			scol.enabled = true;
		}
		if (speed == 0f)
		{
			speed = 1f;
		}
		if (!lowQuality && MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleExplosions"))
		{
			lowQuality = true;
		}
		if ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Count > 0)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 1f, LayerMaskDefaults.Get(LMD.Environment));
			if (array.Length != 0)
			{
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if (MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(collider.transform) && collider.transform.TryGetComponent<ScrollingTexture>(out var component))
					{
						component.attachedObjects.Add(base.transform);
					}
				}
			}
		}
		if (!lowQuality)
		{
			light = GetComponentInChildren<Light>();
			light.enabled = true;
			if (explosionChunk != null)
			{
				for (int j = 0; j < Random.Range(24, 30); j++)
				{
					GameObject obj = Object.Instantiate(explosionChunk, base.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), Random.rotation);
					Vector3 vector = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 2f), Random.Range(-1f, 1f));
					obj.GetComponent<Rigidbody>().AddForce(vector * 100f, ForceMode.VelocityChange);
					Physics.IgnoreCollision(obj.GetComponent<Collider>(), scol);
				}
			}
		}
		lmask = (int)lmask | 0x100;
		lmask = (int)lmask | 0x800;
		lmask = (int)lmask | 0x1000000;
		lmask = (int)lmask | 0x4000000;
		speed *= globalSizeMulti;
		maxSize *= globalSizeMulti;
	}

	private void FixedUpdate()
	{
		base.transform.localScale += Vector3.one * 0.05f * speed;
		if (light != null)
		{
			light.range += 0.05f * speed;
		}
		if (!fading && base.transform.lossyScale.x * scol.radius > maxSize)
		{
			Fade();
		}
		if (!halved && base.transform.lossyScale.x * scol.radius > maxSize / 2f)
		{
			halved = true;
			damage = Mathf.RoundToInt((float)damage / 1.5f);
		}
		if (fading)
		{
			materialColor.a -= 0.02f;
			if (light != null)
			{
				light.intensity -= 0.65f;
			}
			mr.material.SetColor("_Color", materialColor);
			if (materialColor.a <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!harmless && !hitColliders.Contains(other))
		{
			bool flag = false;
			if (Physics.Raycast(base.transform.position + (other.transform.position - base.transform.position).normalized * 0.1f, other.transform.position - base.transform.position, out var hitInfo, Vector3.Distance(base.transform.position + (other.transform.position - base.transform.position).normalized * 0.1f, other.transform.position), lmask, QueryTriggerInteraction.Ignore))
			{
				RaycastHit[] rhits = Physics.RaycastAll(base.transform.position + (other.transform.position - base.transform.position).normalized * 0.1f, other.transform.position - base.transform.position, Vector3.Distance(base.transform.position + (other.transform.position - base.transform.position).normalized * 0.1f, other.transform.position), lmask, QueryTriggerInteraction.Ignore);
				if (CheckRaycasts(other, rhits))
				{
					hitInfo = neoRhit;
				}
				else
				{
					flag = false;
				}
			}
			Breakable component7;
			Glass component8;
			Flammable component9;
			if (!flag || other.isTrigger || hitInfo.transform.gameObject == other.gameObject || other.gameObject.layer == 11 || ((bool)hitInfo.collider.attachedRigidbody && (bool)other.attachedRigidbody && hitInfo.collider.attachedRigidbody == other.attachedRigidbody))
			{
				if (enemy && Physics.Raycast(other.transform.position, base.transform.position - other.transform.position, Vector3.Distance(base.transform.position, other.transform.position) - 0.1f, lmask, QueryTriggerInteraction.Ignore))
				{
					RaycastHit[] rhits2 = Physics.RaycastAll(other.transform.position, base.transform.position - other.transform.position, Vector3.Distance(base.transform.position, other.transform.position) - 0.1f, lmask, QueryTriggerInteraction.Ignore);
					if (CheckRaycasts(other, rhits2))
					{
						return;
					}
				}
				if (flag && other.gameObject.layer == 11)
				{
					Collider[] componentsInChildren = hitInfo.transform.GetComponentsInChildren<Collider>();
					bool flag2 = false;
					Collider[] array = componentsInChildren;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].transform == hitInfo.transform)
						{
							flag2 = true;
						}
					}
					if (!flag2)
					{
						return;
					}
				}
				Breakable component3;
				Bleeder component4;
				Glass component5;
				Flammable component6;
				if (other.gameObject.tag == "Player" && !hasHitPlayer)
				{
					hasHitPlayer = true;
					hitColliders.Add(other);
					if (canHit != AffectedSubjects.EnemiesOnly)
					{
						if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && !harmless && damage > 0)
						{
							MonoSingleton<PlatformerMovement>.Instance.Burn();
							return;
						}
						if (!MonoSingleton<NewMovement>.Instance.exploded)
						{
							int num = (rocketExplosion ? 100 : 200);
							if (Mathf.Abs(base.transform.position.x - other.transform.position.x) < 0.25f && Mathf.Abs(base.transform.position.z - other.transform.position.z) < 0.25f)
							{
								MonoSingleton<NewMovement>.Instance.Launch(other.transform.position, num, maxSize);
								if (ultrabooster && Vector3.Distance(base.transform.position, other.transform.position) < 12f)
								{
									MonoSingleton<NewMovement>.Instance.Launch(other.transform.position, num, maxSize);
								}
							}
							else
							{
								MonoSingleton<NewMovement>.Instance.Launch(base.transform.position, num, maxSize);
								if (ultrabooster && Vector3.Distance(base.transform.position, other.transform.position) < 12f)
								{
									MonoSingleton<NewMovement>.Instance.Launch(base.transform.position, num, maxSize);
								}
							}
						}
						if (damage > 0)
						{
							int num2 = damage;
							if (ultrabooster)
							{
								num2 = ((!(Vector3.Distance(base.transform.position, other.transform.position) < 3f)) ? 50 : 35);
							}
							else if (playerDamageOverride >= 0)
							{
								num2 = playerDamageOverride;
							}
							if (enemy)
							{
								MonoSingleton<NewMovement>.Instance.GetHurt(num2, invincible: true, 1f, explosion: true);
							}
							else
							{
								MonoSingleton<NewMovement>.Instance.GetHurt(num2, invincible: true, 0f, explosion: true);
							}
						}
					}
				}
				else if ((other.gameObject.layer == 10 || other.gameObject.layer == 11) && canHit != AffectedSubjects.PlayerOnly)
				{
					EnemyIdentifierIdentifier componentInParent = other.GetComponentInParent<EnemyIdentifierIdentifier>();
					if (componentInParent != null && componentInParent.eid != null)
					{
						Collider component = componentInParent.eid.GetComponent<Collider>();
						if (component != null && !hitColliders.Contains(component) && !componentInParent.eid.dead)
						{
							hitColliders.Add(component);
							if (componentInParent.eid.enemyType != EnemyType.MaliciousFace && (!enemy || (componentInParent.eid.enemyType != EnemyType.HideousMass && componentInParent.eid.enemyType != EnemyType.Sisyphus)) && !toIgnore.Contains(componentInParent.eid.enemyType))
							{
								if (friendlyFire)
								{
									componentInParent.eid.hitter = "ffexplosion";
								}
								else if (enemy)
								{
									componentInParent.eid.hitter = "enemy";
								}
								else
								{
									componentInParent.eid.hitter = "explosion";
								}
								if (!componentInParent.eid.hitterWeapons.Contains(hitterWeapon))
								{
									componentInParent.eid.hitterWeapons.Add(hitterWeapon);
								}
								Vector3 vector = (other.transform.position - base.transform.position).normalized;
								if (componentInParent.eid.enemyType == EnemyType.Drone && damage == 0)
								{
									vector = Vector3.zero;
								}
								else if (vector.y <= 0.5f)
								{
									vector = new Vector3(vector.x, vector.y + 0.5f, vector.z);
								}
								else if (vector.y < 1f)
								{
									vector = new Vector3(vector.x, 1f, vector.z);
								}
								componentInParent.eid.DeliverDamage(other.gameObject, vector * 50000f, other.transform.position, (float)damage / 10f * enemyDamageMultiplier, tryForExplode: false);
								if (ignite)
								{
									Flammable componentInChildren = componentInParent.eid.GetComponentInChildren<Flammable>();
									if (componentInChildren != null)
									{
										componentInChildren.Burn(damage / 10);
									}
								}
							}
						}
						else if (componentInParent.eid.dead)
						{
							hitColliders.Add(other);
							if (enemy)
							{
								componentInParent.eid.hitter = "enemy";
							}
							else
							{
								componentInParent.eid.hitter = "explosion";
							}
							componentInParent.eid.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 5000f, other.transform.position, (float)damage / 10f * enemyDamageMultiplier, tryForExplode: false);
							if (ignite && componentInParent.TryGetComponent<Flammable>(out var _))
							{
								Flammable componentInChildren2 = componentInParent.eid.GetComponentInChildren<Flammable>();
								if (componentInChildren2 != null)
								{
									componentInChildren2.Burn(damage / 10);
								}
							}
						}
					}
				}
				else if (other.TryGetComponent<Breakable>(out component3))
				{
					if (!component3.accurateExplosionsOnly)
					{
						component3.Break();
					}
					else
					{
						Vector3 vector2 = other.ClosestPoint(base.transform.position);
						if (!Physics.Raycast(vector2 + (vector2 - base.transform.position).normalized * 0.001f, base.transform.position - vector2, Vector3.Distance(base.transform.position, vector2), lmask, QueryTriggerInteraction.Ignore))
						{
							component3.Break();
						}
					}
				}
				else if (other.TryGetComponent<Bleeder>(out component4))
				{
					bool flag3 = false;
					if (toIgnore.Count > 0 && component4.ignoreTypes.Length != 0)
					{
						EnemyType[] ignoreTypes = component4.ignoreTypes;
						foreach (EnemyType enemyType in ignoreTypes)
						{
							for (int j = 0; j < toIgnore.Count; j++)
							{
								if (enemyType == toIgnore[j])
								{
									flag3 = true;
									break;
								}
							}
							if (flag3)
							{
								break;
							}
						}
					}
					if (!flag3)
					{
						component4.GetHit(other.transform.position, GoreType.Head);
					}
				}
				else if (other.TryGetComponent<Glass>(out component5))
				{
					component5.Shatter();
				}
				else if (ignite && other.TryGetComponent<Flammable>(out component6) && (!enemy || !component6.playerOnly))
				{
					component6.Burn(4f);
				}
			}
			else if (other.TryGetComponent<Breakable>(out component7) && !component7.accurateExplosionsOnly)
			{
				component7.Break();
			}
			else if (other.TryGetComponent<Glass>(out component8))
			{
				component8.Shatter();
			}
			else if (ignite && other.TryGetComponent<Flammable>(out component9) && (!enemy || !component9.playerOnly))
			{
				component9.Burn(4f);
			}
		}
		if (harmless || (!(other.gameObject.tag != "Player") && MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.Platformer))
		{
			return;
		}
		Rigidbody component10 = other.GetComponent<Rigidbody>();
		if ((!component10 || other.gameObject.layer != 14 || !other.gameObject.CompareTag("Metal") || !other.TryGetComponent<Nail>(out var component11) || component11.magnets.Count == 0) && (bool)component10 && (other.gameObject.layer != 14 || component10.useGravity) && other.gameObject.tag != "IgnorePushes")
		{
			if (!hitColliders.Contains(other))
			{
				hitColliders.Add(other);
			}
			Vector3 normalized = (other.transform.position - base.transform.position).normalized;
			normalized = new Vector3(normalized.x * Mathf.Max(5f - Vector3.Distance(other.transform.position, base.transform.position), 0f) * 7500f, normalized.y * Mathf.Max(5f - Vector3.Distance(other.transform.position, base.transform.position), 0f), normalized.z * Mathf.Max(5f - Vector3.Distance(other.transform.position, base.transform.position), 0f) * 7500f);
			if (component10.useGravity)
			{
				normalized = new Vector3(normalized.x, 18750f, normalized.z);
			}
			if (other.gameObject.layer == 27 || other.gameObject.layer == 9)
			{
				normalized = Vector3.ClampMagnitude(normalized, 5000f);
			}
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && other.gameObject == MonoSingleton<PlatformerMovement>.Instance.gameObject)
			{
				normalized *= 30f;
			}
			component10.AddForce(normalized);
		}
		if (other.gameObject.layer != 14)
		{
			return;
		}
		ThrownSword component12 = other.GetComponent<ThrownSword>();
		Projectile component13 = other.GetComponent<Projectile>();
		if (component12 != null)
		{
			component12.deflected = true;
		}
		if (component13 != null)
		{
			component13.homingType = HomingType.None;
			other.transform.LookAt(other.transform.position + (other.transform.position - base.transform.position));
			component13.friendly = true;
			component13.target = null;
			component13.turnSpeed = 0f;
			if (component13.speed < 65f)
			{
				component13.speed = 65f;
			}
		}
	}

	private void Fade()
	{
		harmless = true;
		mr = GetComponent<MeshRenderer>();
		materialColor = mr.material.GetColor("_Color");
		fading = true;
		speed /= 4f;
	}

	private void BecomeHarmless()
	{
		harmless = true;
	}

	private bool CheckRaycasts(Collider other, RaycastHit[] rhits)
	{
		bool flag = true;
		if (rhits.Length != 0)
		{
			for (int i = 0; i < rhits.Length; i++)
			{
				flag = true;
				if (rhits[i].collider.gameObject.layer == 11 && rhits[i].collider.TryGetComponent<EnemyIdentifierIdentifier>(out var component) && (bool)component.eid && toIgnore.Count > 0)
				{
					foreach (EnemyType item in toIgnore)
					{
						if (item == component.eid.enemyType)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					neoRhit = rhits[i];
					break;
				}
			}
			return flag;
		}
		return false;
	}
}
