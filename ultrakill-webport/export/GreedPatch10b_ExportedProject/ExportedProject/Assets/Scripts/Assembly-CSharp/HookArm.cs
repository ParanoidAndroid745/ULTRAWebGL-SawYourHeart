using System;
using System.Collections.Generic;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class HookArm : MonoSingleton<HookArm>
{
	public bool equipped;

	private LineRenderer lr;

	private Animator anim;

	private Vector3 hookPoint;

	[HideInInspector]
	public HookState state;

	private bool returning;

	[SerializeField]
	private GameObject model;

	private CapsuleCollider playerCollider;

	public Transform hand;

	public Transform hook;

	public GameObject hookModel;

	private Vector3 throwDirection;

	private float returnDistance;

	private LayerMask throwMask;

	private LayerMask enviroMask;

	private float throwWarp;

	private Transform caughtTransform;

	private Vector3 caughtPoint;

	private Collider caughtCollider;

	private EnemyIdentifier caughtEid;

	private List<EnemyType> deadIgnoreTypes = new List<EnemyType>();

	private List<EnemyType> lightEnemies = new List<EnemyType>();

	private GroundCheckEnemy enemyGroundCheck;

	private Rigidbody enemyRigidbody;

	private HookPoint caughtHook;

	private bool lightTarget;

	[SerializeField]
	private LineRenderer inspectLr;

	private bool forcingGroundCheck;

	private bool forcingFistControl;

	[Header("Sounds")]
	private AudioSource aud;

	public GameObject throwSound;

	public GameObject hitSound;

	public GameObject pullSound;

	public GameObject pullDoneSound;

	public GameObject catchSound;

	public GameObject errorSound;

	public AudioClip throwLoop;

	public AudioClip pullLoop;

	public GameObject wooshSound;

	private GameObject currentWoosh;

	public GameObject clinkSparks;

	private float cooldown;

	private CameraFrustumTargeter targeter;

	[HideInInspector]
	public bool beingPulled;

	private void Start()
	{
		targeter = MonoSingleton<CameraFrustumTargeter>.Instance;
		lr = GetComponent<LineRenderer>();
		lr.enabled = false;
		anim = GetComponent<Animator>();
		playerCollider = MonoSingleton<NewMovement>.Instance.GetComponent<CapsuleCollider>();
		aud = GetComponent<AudioSource>();
		throwMask = (int)throwMask | 0x800;
		throwMask = (int)throwMask | 0x1000;
		throwMask = (int)throwMask | 0x4000;
		throwMask = (int)throwMask | 0x10000;
		throwMask = (int)throwMask | 0x400000;
		throwMask = (int)throwMask | 0x4000000;
		enviroMask = (int)enviroMask | 0x100;
		enviroMask = (int)enviroMask | 0x10000;
		enviroMask = (int)enviroMask | 0x40000;
		enviroMask = (int)enviroMask | 0x1000000;
		deadIgnoreTypes.Add(EnemyType.Drone);
		deadIgnoreTypes.Add(EnemyType.MaliciousFace);
		deadIgnoreTypes.Add(EnemyType.Virtue);
		lightEnemies.Add(EnemyType.Drone);
		lightEnemies.Add(EnemyType.Filth);
		lightEnemies.Add(EnemyType.Schism);
		lightEnemies.Add(EnemyType.Soldier);
		lightEnemies.Add(EnemyType.Stray);
		lightEnemies.Add(EnemyType.Streetcleaner);
		model.SetActive(value: false);
	}

	public void Inspect()
	{
		model.SetActive(value: true);
		inspectLr.enabled = true;
		anim.Play("Inspect", -1, 0f);
	}

	private void Update()
	{
		if (MonoSingleton<OptionsManager>.Instance.paused)
		{
			return;
		}
		if (!equipped || MonoSingleton<FistControl>.Instance.shopping || !MonoSingleton<FistControl>.Instance.activated)
		{
			if (state != HookState.Ready || returning)
			{
				Cancel();
			}
			model.SetActive(value: false);
			return;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Hook.WasPerformedThisFrame)
		{
			if (state == HookState.Pulling)
			{
				StopThrow();
			}
			else if (cooldown <= 0f)
			{
				cooldown = 0.5f;
				model.SetActive(value: true);
				if (!forcingFistControl)
				{
					if ((bool)MonoSingleton<FistControl>.Instance.currentPunch)
					{
						MonoSingleton<FistControl>.Instance.currentPunch.CancelAttack();
					}
					MonoSingleton<FistControl>.Instance.forceNoHold++;
					forcingFistControl = true;
					MonoSingleton<FistControl>.Instance.transform.localRotation = Quaternion.identity;
				}
				lr.enabled = true;
				hookPoint = base.transform.position;
				if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
				{
					throwDirection = (targeter.CurrentTarget.bounds.center - base.transform.position).normalized;
				}
				else
				{
					throwDirection = base.transform.forward;
				}
				returning = false;
				state = HookState.Throwing;
				lightTarget = false;
				throwWarp = 1f;
				anim.Play("Throw", -1, 0f);
				inspectLr.enabled = false;
				hand.transform.localPosition = new Vector3(0.09f, -0.051f, 0.045f);
				if (MonoSingleton<CameraController>.Instance.defaultFov > 105f)
				{
					hand.transform.localPosition += new Vector3(0.225f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f), -0.25f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f), 0.05f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f));
				}
				caughtPoint = Vector3.zero;
				caughtTransform = null;
				caughtCollider = null;
				caughtEid = null;
				UnityEngine.Object.Instantiate(throwSound);
				aud.clip = throwLoop;
				aud.panStereo = 0f;
				aud.Play();
				aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
			}
		}
		if (cooldown != 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		}
		if (lr.enabled)
		{
			throwWarp = Mathf.MoveTowards(throwWarp, 0f, Time.deltaTime * 6.5f);
			lr.SetPosition(0, hand.position);
			for (int i = 1; i < lr.positionCount - 1; i++)
			{
				float num = 3f;
				if (i % 2 == 0)
				{
					num = -3f;
				}
				lr.SetPosition(i, Vector3.Lerp(hand.position, hookPoint, (float)i / (float)lr.positionCount) + base.transform.up * num * throwWarp * (1f / (float)i));
			}
			lr.SetPosition(lr.positionCount - 1, hookPoint);
		}
		if (state == HookState.Pulling && !lightTarget && MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame)
		{
			if (MonoSingleton<NewMovement>.Instance.rb.velocity.y < 1f)
			{
				MonoSingleton<NewMovement>.Instance.rb.velocity = new Vector3(MonoSingleton<NewMovement>.Instance.rb.velocity.x, 1f, MonoSingleton<NewMovement>.Instance.rb.velocity.z);
			}
			MonoSingleton<NewMovement>.Instance.rb.velocity = Vector3.ClampMagnitude(MonoSingleton<NewMovement>.Instance.rb.velocity, 30f);
			if (!MonoSingleton<NewMovement>.Instance.gc.touchingGround && !Physics.Raycast(MonoSingleton<NewMovement>.Instance.gc.transform.position, Vector3.down, 1.5f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				MonoSingleton<NewMovement>.Instance.rb.AddForce(Vector3.up * 15f, ForceMode.VelocityChange);
			}
			else if (!MonoSingleton<NewMovement>.Instance.jumping)
			{
				MonoSingleton<NewMovement>.Instance.Jump();
			}
			StopThrow(1f);
		}
		if (!MonoSingleton<FistControl>.Instance.currentPunch || !MonoSingleton<FistControl>.Instance.currentPunch.holding || !forcingFistControl)
		{
			return;
		}
		MonoSingleton<FistControl>.Instance.currentPunch.heldItem.transform.position = hook.position + hook.up * 0.2f;
		if (state != HookState.Ready || returning)
		{
			MonoSingleton<FistControl>.Instance.heldObject.hooked = true;
			if (MonoSingleton<FistControl>.Instance.heldObject.gameObject.layer != 22)
			{
				Transform[] componentsInChildren = MonoSingleton<FistControl>.Instance.heldObject.GetComponentsInChildren<Transform>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].gameObject.layer = 22;
				}
			}
			return;
		}
		MonoSingleton<FistControl>.Instance.heldObject.hooked = false;
		if (MonoSingleton<FistControl>.Instance.heldObject.gameObject.layer != 13)
		{
			Transform[] componentsInChildren = MonoSingleton<FistControl>.Instance.heldObject.GetComponentsInChildren<Transform>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].gameObject.layer = 13;
			}
		}
	}

	private void LateUpdate()
	{
		if (state != HookState.Ready || returning)
		{
			hook.position = hookPoint;
			hook.up = throwDirection;
			hookModel.layer = 2;
		}
		else
		{
			hookModel.layer = 13;
		}
	}

	private void FixedUpdate()
	{
		if (state == HookState.Ready && returning)
		{
			hookPoint = Vector3.MoveTowards(hookPoint, hand.position, Time.fixedDeltaTime * (100f + returnDistance / 2f));
			if (hookPoint == hand.position)
			{
				lr.enabled = false;
				returning = false;
				anim.Play("Catch", -1, 0f);
				UnityEngine.Object.Instantiate(catchSound);
				aud.Stop();
			}
		}
		if (state == HookState.Throwing)
		{
			if (!MonoSingleton<InputManager>.Instance.InputSource.Hook.IsPressed && cooldown <= 0.1f)
			{
				StopThrow();
			}
			else
			{
				float num = 250f * Time.fixedDeltaTime;
				bool flag = false;
				if (Physics.Raycast(hookPoint, throwDirection, out var hitInfo, num, enviroMask, QueryTriggerInteraction.Ignore))
				{
					flag = true;
					num = hitInfo.distance;
				}
				RaycastHit[] array = Physics.SphereCastAll(hookPoint, Mathf.Min(Vector3.Distance(base.transform.position, hookPoint) / 15f, 5f), throwDirection, num, throwMask, QueryTriggerInteraction.Collide);
				Array.Sort(array, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
				bool flag2 = false;
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					RaycastHit rhit = array[num2];
					bool flag3 = false;
					switch (rhit.transform.gameObject.layer)
					{
					case 26:
						if (!rhit.collider.isTrigger)
						{
							StopThrow();
							UnityEngine.Object.Instantiate(clinkSparks, rhit.point, Quaternion.LookRotation(rhit.normal));
							flag2 = true;
							flag3 = true;
						}
						goto default;
					case 16:
					{
						if (rhit.collider.isTrigger && rhit.transform.TryGetComponent<BulletCheck>(out var component4))
						{
							component4.ForceDodge();
						}
						flag3 = true;
						goto default;
					}
					case 11:
					case 12:
					{
						if (Physics.Raycast(hookPoint, rhit.collider.bounds.center - hookPoint, Vector3.Distance(hookPoint, rhit.collider.bounds.center), enviroMask, QueryTriggerInteraction.Ignore))
						{
							continue;
						}
						caughtEid = rhit.transform.GetComponentInParent<EnemyIdentifier>();
						if ((bool)caughtEid && caughtEid.enemyType == EnemyType.MaliciousFace && caughtEid.dead && !rhit.collider.Raycast(new Ray(hookPoint, throwDirection), out var _, num))
						{
							caughtEid = null;
							continue;
						}
						if (caughtEid == null && rhit.transform.TryGetComponent<EnemyIdentifierIdentifier>(out var component3))
						{
							caughtEid = component3.eid;
						}
						if (!caughtEid)
						{
							goto IL_053e;
						}
						if (caughtEid.hookIgnore)
						{
							caughtEid = null;
						}
						else
						{
							if (!caughtEid.blessed)
							{
								caughtEid.hitter = "hook";
								caughtEid.hooked = true;
								if (((caughtEid.enemyType != EnemyType.Drone && caughtEid.enemyType != EnemyType.Virtue) || !caughtEid.dead) && caughtEid.enemyType != EnemyType.Stalker)
								{
									caughtEid.DeliverDamage(caughtEid.gameObject, Vector3.zero, rhit.point, 0.2f, tryForExplode: false);
								}
								if (caughtEid == null)
								{
									return;
								}
								if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
								{
									GameObject gameObject = rhit.transform.gameObject;
									if (rhit.transform.gameObject.layer == 12)
									{
										EnemyIdentifierIdentifier componentInChildren = gameObject.GetComponentInChildren<EnemyIdentifierIdentifier>();
										if ((bool)componentInChildren)
										{
											gameObject = componentInChildren.gameObject;
										}
									}
									MonoSingleton<FistControl>.Instance.heldObject.SendMessage("HitWith", gameObject, SendMessageOptions.DontRequireReceiver);
								}
								if (caughtEid.dead)
								{
									if (!deadIgnoreTypes.Contains(caughtEid.enemyType))
									{
										goto default;
									}
									if (caughtEid.enemyType == EnemyType.Virtue || lightEnemies.Contains(caughtEid.enemyType))
									{
										lightTarget = true;
									}
									caughtEid = null;
								}
								else if (lightEnemies.Contains(caughtEid.enemyType))
								{
									lightTarget = true;
								}
								goto IL_053e;
							}
							flag3 = true;
							caughtEid.hitter = "hook";
							caughtEid.DeliverDamage(caughtEid.gameObject, Vector3.zero, rhit.point, 1f, tryForExplode: false);
							StopThrow();
						}
						goto default;
					}
					case 22:
					{
						if (rhit.transform.TryGetComponent<HookPoint>(out var component))
						{
							if (component.active && Vector3.Distance(base.transform.position, rhit.transform.position) > 5f)
							{
								flag2 = true;
								flag3 = true;
								caughtTransform = rhit.transform;
								hookPoint = rhit.transform.position;
								caughtPoint = Vector3.zero;
								state = HookState.Caught;
								caughtCollider = rhit.collider;
								aud.Stop();
								caughtHook = component;
								component.Hooked();
								goto default;
							}
						}
						else if ((bool)MonoSingleton<FistControl>.Instance.currentPunch && !MonoSingleton<FistControl>.Instance.currentPunch.holding)
						{
							if (rhit.transform.TryGetComponent<ItemIdentifier>(out var component2))
							{
								if (Physics.Raycast(hookPoint, rhit.transform.position - hookPoint, Vector3.Distance(hookPoint, rhit.transform.position), enviroMask, QueryTriggerInteraction.Ignore))
								{
									continue;
								}
								flag2 = true;
								if (component2.ipz == null || component2.ipz.CheckDoorBounds())
								{
									MonoSingleton<FistControl>.Instance.currentPunch.ForceHold(component2);
								}
								else
								{
									ItemGrabError(rhit);
								}
							}
						}
						else
						{
							ItemPlaceZone[] components = rhit.transform.GetComponents<ItemPlaceZone>();
							bool flag4 = false;
							ItemPlaceZone[] array2 = components;
							foreach (ItemPlaceZone itemPlaceZone in array2)
							{
								if (itemPlaceZone.acceptedItemType == MonoSingleton<FistControl>.Instance.heldObject.itemType && !itemPlaceZone.CheckReverseDoorBounds())
								{
									flag4 = true;
								}
							}
							if (components.Length != 0)
							{
								if (Physics.Raycast(hookPoint, rhit.transform.position - hookPoint, Vector3.Distance(hookPoint, rhit.transform.position), enviroMask, QueryTriggerInteraction.Ignore))
								{
									continue;
								}
								flag2 = true;
								if (!flag4)
								{
									MonoSingleton<FistControl>.Instance.currentPunch.PlaceHeldObject(components, rhit.transform);
								}
								else
								{
									ItemGrabError(rhit);
								}
							}
						}
						if (flag2)
						{
							flag3 = true;
							StopThrow();
						}
						else if (!Physics.Raycast(hookPoint, rhit.transform.position - hookPoint, Vector3.Distance(hookPoint, rhit.transform.position), enviroMask, QueryTriggerInteraction.Ignore))
						{
							flag3 = true;
						}
						goto default;
					}
					default:
						{
							if (flag3 && (bool)MonoSingleton<FistControl>.Instance.heldObject)
							{
								MonoSingleton<FistControl>.Instance.heldObject.SendMessage("HitWith", rhit.transform.gameObject, SendMessageOptions.DontRequireReceiver);
							}
							if (!flag2)
							{
								continue;
							}
							break;
						}
						IL_053e:
						flag2 = true;
						flag3 = true;
						caughtTransform = rhit.transform;
						hookPoint = rhit.collider.bounds.center;
						caughtPoint = hookPoint - caughtTransform.position;
						state = HookState.Caught;
						caughtCollider = rhit.collider;
						aud.Stop();
						UnityEngine.Object.Instantiate(hitSound, rhit.point, Quaternion.identity);
						goto default;
					}
					break;
				}
				if (flag && !flag2)
				{
					if (hitInfo.transform.TryGetComponent<Breakable>(out var component5) && component5.weak && !component5.precisionOnly)
					{
						component5.Break();
					}
					if (hitInfo.transform.gameObject.TryGetComponent<SandboxProp>(out var component6))
					{
						Debug.Log("Prop");
						component6.rigidbody.AddForceAtPosition(base.transform.forward * -100f, hitInfo.point, ForceMode.VelocityChange);
					}
					else
					{
						UnityEngine.Object.Instantiate(clinkSparks, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
					}
					StopThrow();
					flag2 = true;
				}
				if (!flag2 && Vector3.Distance(base.transform.position, hookPoint) > 300f)
				{
					StopThrow();
				}
				else if (!flag2)
				{
					hookPoint += throwDirection * num;
				}
			}
		}
		else if (state == HookState.Caught)
		{
			if (caughtEid != null && (caughtEid.dead || caughtEid.hookIgnore || caughtEid.blessed))
			{
				if (!caughtEid.dead || !deadIgnoreTypes.Contains(caughtEid.enemyType))
				{
					StopThrow();
					return;
				}
				SolveDeadIgnore();
			}
			else if (!caughtTransform || Physics.Raycast(hand.position, caughtTransform.position + caughtPoint - hand.position, Vector3.Distance(hand.position, caughtTransform.position + caughtPoint), enviroMask, QueryTriggerInteraction.Ignore))
			{
				StopThrow();
				return;
			}
			hookPoint = caughtTransform.position + caughtPoint;
			if (!MonoSingleton<InputManager>.Instance.InputSource.Hook.IsPressed)
			{
				anim.Play("Pull", -1, 0f);
				hand.transform.localPosition = new Vector3(-0.015f, 0.071f, 0.04f);
				state = HookState.Pulling;
				currentWoosh = UnityEngine.Object.Instantiate(wooshSound);
				UnityEngine.Object.Instantiate(pullSound);
				aud.clip = pullLoop;
				aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
				aud.panStereo = -0.5f;
				aud.Play();
				if (!forcingGroundCheck && !lightTarget)
				{
					ForceGroundCheck();
				}
				else if (lightTarget)
				{
					Rigidbody component7;
					if ((bool)caughtEid)
					{
						if (enemyGroundCheck != null)
						{
							enemyGroundCheck.StopForceOff();
						}
						enemyGroundCheck = caughtEid.gce;
						if ((bool)enemyGroundCheck)
						{
							enemyGroundCheck.ForceOff();
						}
						enemyRigidbody = caughtEid.GetComponent<Rigidbody>();
					}
					else if (caughtTransform.TryGetComponent<Rigidbody>(out component7))
					{
						enemyRigidbody = component7;
					}
					else
					{
						StopThrow();
					}
					if (!MonoSingleton<NewMovement>.Instance.gc.touchingGround)
					{
						if (!MonoSingleton<UnderwaterController>.Instance.inWater)
						{
							MonoSingleton<NewMovement>.Instance.rb.velocity = new Vector3(0f, 15f, 0f);
						}
						else
						{
							MonoSingleton<NewMovement>.Instance.rb.velocity = new Vector3(0f, 5f, 0f);
						}
					}
				}
			}
		}
		if (state == HookState.Pulling)
		{
			if (!caughtTransform || !caughtCollider || Physics.Raycast(hand.position, caughtTransform.position + caughtPoint - hand.position, Vector3.Distance(hand.position, caughtTransform.position + caughtPoint), enviroMask, QueryTriggerInteraction.Ignore))
			{
				StopThrow(1f);
				return;
			}
			if (caughtEid != null && (caughtEid.dead || caughtEid.hookIgnore || caughtEid.blessed))
			{
				if (!caughtEid.dead || !deadIgnoreTypes.Contains(caughtEid.enemyType))
				{
					StopThrow(1f);
					return;
				}
				SolveDeadIgnore();
			}
			Vector3 vector = playerCollider.ClosestPoint(hookPoint);
			if (Vector3.Distance(vector, caughtCollider.ClosestPoint(vector)) < 0.25f)
			{
				if ((bool)enemyRigidbody)
				{
					if (enemyGroundCheck == null || enemyGroundCheck.touchingGround || ((bool)caughtEid && caughtEid.underwater))
					{
						enemyRigidbody.velocity = Vector3.zero;
					}
					else
					{
						enemyRigidbody.velocity = new Vector3(0f, 15f, 0f);
					}
				}
				StopThrow(1f);
				if (!MonoSingleton<NewMovement>.Instance.gc.touchingGround)
				{
					if (MonoSingleton<UnderwaterController>.Instance.inWater)
					{
						MonoSingleton<NewMovement>.Instance.rb.velocity = Vector3.zero;
					}
					else if (base.transform.position.y < hookPoint.y)
					{
						MonoSingleton<NewMovement>.Instance.rb.velocity = new Vector3(0f, 15f + (hookPoint.y - base.transform.position.y) * 3f, 0f);
					}
					else
					{
						MonoSingleton<NewMovement>.Instance.rb.velocity = new Vector3(0f, 15f, 0f);
					}
				}
				return;
			}
			if ((bool)caughtEid && (bool)enemyRigidbody && caughtEid.enemyType == EnemyType.Drone)
			{
				if (enemyRigidbody.isKinematic)
				{
					lightTarget = false;
				}
				else
				{
					lightTarget = true;
				}
			}
			if (lightTarget && forcingGroundCheck)
			{
				StopForceGroundCheck();
			}
			else if (!lightTarget && !forcingGroundCheck)
			{
				ForceGroundCheck();
			}
			if (lightTarget)
			{
				if (!enemyRigidbody)
				{
					StopThrow(1f);
					return;
				}
				hookPoint = caughtTransform.position + caughtPoint;
				if (enemyGroundCheck != null)
				{
					enemyRigidbody.velocity = (MonoSingleton<NewMovement>.Instance.transform.position - hookPoint).normalized * 60f;
					caughtEid.transform.LookAt(new Vector3(MonoSingleton<CameraController>.Instance.transform.position.x, caughtEid.transform.position.y, MonoSingleton<CameraController>.Instance.transform.position.z));
					return;
				}
				enemyRigidbody.velocity = (MonoSingleton<CameraController>.Instance.transform.position - hookPoint).normalized * 60f;
				if ((bool)caughtEid)
				{
					caughtEid.transform.LookAt(MonoSingleton<CameraController>.Instance.transform.position);
				}
				else
				{
					caughtTransform.LookAt(MonoSingleton<CameraController>.Instance.transform.position);
				}
			}
			else
			{
				hookPoint = caughtTransform.position + caughtPoint;
				beingPulled = true;
				if (!MonoSingleton<NewMovement>.Instance.boost || MonoSingleton<NewMovement>.Instance.sliding)
				{
					MonoSingleton<NewMovement>.Instance.rb.velocity = (hookPoint - MonoSingleton<NewMovement>.Instance.transform.position).normalized * 60f;
				}
			}
		}
		else
		{
			beingPulled = false;
		}
	}

	private void SolveDeadIgnore()
	{
		if (!caughtEid)
		{
			return;
		}
		switch (caughtEid.enemyType)
		{
		case EnemyType.Virtue:
			lightTarget = true;
			enemyRigidbody = caughtEid.GetComponent<Rigidbody>();
			break;
		case EnemyType.MaliciousFace:
		{
			EnemyIdentifierIdentifier[] componentsInChildren = caughtEid.GetComponentsInChildren<EnemyIdentifierIdentifier>();
			if (componentsInChildren.Length == 0)
			{
				break;
			}
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].gameObject.layer == 11)
				{
					caughtTransform = componentsInChildren[i].transform;
					break;
				}
			}
			break;
		}
		}
		caughtEid = null;
	}

	private void ItemGrabError(RaycastHit rhit)
	{
		UnityEngine.Object.Instantiate(errorSound);
		MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
		MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=red>ERROR: BLOCKING DOOR WOULD CLOSE</color>", "", "", 0, silent: true);
	}

	public void StopThrow(float animationTime = 0f, bool sparks = false)
	{
		if (animationTime == 0f)
		{
			UnityEngine.Object.Instantiate(pullSound);
			aud.clip = pullLoop;
			aud.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
			aud.panStereo = -0.5f;
			aud.Play();
		}
		else
		{
			UnityEngine.Object.Instantiate(pullDoneSound);
		}
		if (forcingGroundCheck)
		{
			StopForceGroundCheck();
		}
		if (lightTarget)
		{
			if ((bool)enemyGroundCheck)
			{
				enemyGroundCheck.StopForceOff();
			}
			lightTarget = false;
			enemyGroundCheck = null;
			enemyRigidbody = null;
		}
		if ((bool)caughtEid)
		{
			caughtEid.hooked = false;
			caughtEid = null;
		}
		if ((bool)caughtHook)
		{
			caughtHook.Unhooked();
			caughtHook = null;
		}
		if (sparks)
		{
			UnityEngine.Object.Instantiate(clinkSparks, hookPoint, Quaternion.LookRotation(base.transform.position - hookPoint));
		}
		state = HookState.Ready;
		anim.Play("Pull", -1, animationTime);
		hand.transform.localPosition = new Vector3(-0.015f, 0.071f, 0.04f);
		if (MonoSingleton<CameraController>.Instance.defaultFov > 105f)
		{
			hand.transform.localPosition += new Vector3(0.25f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f), 0f, 0.05f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 60f));
		}
		else if (MonoSingleton<CameraController>.Instance.defaultFov < 105f)
		{
			hand.transform.localPosition -= new Vector3(0.05f * ((105f - MonoSingleton<CameraController>.Instance.defaultFov) / 60f), 0.075f * ((105f - MonoSingleton<CameraController>.Instance.defaultFov) / 60f), 0.125f * ((105f - MonoSingleton<CameraController>.Instance.defaultFov) / 60f));
		}
		returnDistance = Mathf.Max(Vector3.Distance(base.transform.position, hookPoint), 25f);
		returning = true;
		throwWarp = 0f;
		if ((bool)currentWoosh)
		{
			UnityEngine.Object.Destroy(currentWoosh);
		}
	}

	public void Cancel()
	{
		if (forcingGroundCheck)
		{
			StopForceGroundCheck();
		}
		if (forcingFistControl)
		{
			MonoSingleton<FistControl>.Instance.forceNoHold--;
			forcingFistControl = false;
			if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
			{
				MonoSingleton<FistControl>.Instance.heldObject.gameObject.layer = 13;
				MonoSingleton<FistControl>.Instance.heldObject.hooked = false;
			}
		}
		if (lightTarget)
		{
			if ((bool)enemyGroundCheck)
			{
				enemyGroundCheck.StopForceOff();
			}
			lightTarget = false;
			enemyGroundCheck = null;
			enemyRigidbody = null;
		}
		if ((bool)caughtEid)
		{
			caughtEid.hooked = false;
			caughtEid = null;
		}
		if ((bool)caughtHook)
		{
			caughtHook.Unhooked();
			caughtHook = null;
		}
		state = HookState.Ready;
		anim.Play("Idle", -1, 0f);
		returning = false;
		throwWarp = 0f;
		lr.enabled = false;
		hookPoint = hand.position;
		aud.Stop();
		if ((bool)MonoSingleton<FistControl>.Instance.currentPunch && MonoSingleton<FistControl>.Instance.currentPunch.holding)
		{
			MonoSingleton<FistControl>.Instance.ResetHeldItemPosition();
		}
		if ((bool)currentWoosh)
		{
			UnityEngine.Object.Destroy(currentWoosh);
		}
		model.SetActive(value: false);
	}

	public void CatchOver()
	{
		if (state != HookState.Ready || returning)
		{
			return;
		}
		if (forcingFistControl)
		{
			MonoSingleton<FistControl>.Instance.forceNoHold--;
			forcingFistControl = false;
			if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
			{
				MonoSingleton<FistControl>.Instance.heldObject.hooked = false;
			}
		}
		if ((bool)MonoSingleton<FistControl>.Instance.currentPunch && MonoSingleton<FistControl>.Instance.currentPunch.holding)
		{
			MonoSingleton<FistControl>.Instance.ResetHeldItemPosition();
		}
		model.SetActive(value: false);
	}

	private void ForceGroundCheck()
	{
		if (MonoSingleton<NewMovement>.Instance.sliding)
		{
			MonoSingleton<NewMovement>.Instance.StopSlide();
		}
		forcingGroundCheck = true;
		MonoSingleton<NewMovement>.Instance.gc.ForceOff();
		MonoSingleton<NewMovement>.Instance.slopeCheck.ForceOff();
	}

	private void StopForceGroundCheck()
	{
		forcingGroundCheck = false;
		MonoSingleton<NewMovement>.Instance.gc.StopForceOff();
		MonoSingleton<NewMovement>.Instance.slopeCheck.StopForceOff();
	}
}
