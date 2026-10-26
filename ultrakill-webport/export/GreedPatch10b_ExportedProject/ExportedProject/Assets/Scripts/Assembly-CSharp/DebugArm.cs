using System.Collections;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugArm : MonoBehaviour
{
	private enum ArmState
	{
		SimplePlace = 0,
		PropSpawn = 1,
		BrushBlock = 2,
		Manipulate = 3,
		Point = 4
	}

	private class ManipulatedObject
	{
		public Transform target;

		public GameObject particles;

		public Rigidbody rigidbody;

		public Collider collider;

		public Vector3 positionOffset;

		public float distance;

		public Vector3 originalRotation;

		public float rotationOffset;

		public ManipulatedObject(RaycastHit hit, SandboxProp propOverwrite = null)
		{
			GameObject gameObject = (propOverwrite ? propOverwrite.gameObject : hit.collider.gameObject);
			target = gameObject.transform;
			rigidbody = gameObject.GetComponent<Rigidbody>();
			collider = gameObject.GetComponent<Collider>();
			positionOffset = target.position - hit.point;
			CameraController instance = MonoSingleton<CameraController>.Instance;
			distance = Vector3.Distance(instance.transform.position, hit.point);
			originalRotation = target.eulerAngles;
			rotationOffset = originalRotation.y - instance.rotationY;
			if ((bool)rigidbody)
			{
				rigidbody.isKinematic = true;
			}
		}
	}

	[HideInInspector]
	public CameraController cameraCtrl;

	[SerializeField]
	private LayerMask raycastLayers;

	[SerializeField]
	private GameObject axisPoint;

	[SerializeField]
	private GameObject spawnEffect;

	[SerializeField]
	private Material previewMaterial;

	[SerializeField]
	private Transform holder;

	[SerializeField]
	private Animator armAnimator;

	[Space]
	[SerializeField]
	private AudioSource tickSound;

	private float tickDelay;

	[SerializeField]
	private AudioSource jabSound;

	[SerializeField]
	private AudioSource selectSound;

	[SerializeField]
	private AudioSource freezeSound;

	[SerializeField]
	private AudioSource unfreezeSound;

	[SerializeField]
	private AudioSource destroySound;

	[SerializeField]
	private GameObject genericBreakParticles;

	[SerializeField]
	private GameObject manipulateEffect;

	private SpawnMenu menu;

	private SpawnableObject currentObject;

	private GoreZone goreZone;

	private ArmState currentState;

	private bool debugStarted;

	private TimeSince timeSinceDebug;

	private ManipulatedObject manipulatedObject;

	private bool firstBrushPositionSet;

	private Vector3 brushOffset = Vector3.one;

	private Vector3 firstBlockPos;

	private Vector3 secondBlockPos;

	private Vector3 previousSecondBlockPos;

	public static GoreZone debugZone;

	private GameObject handPreview;

	private GameObject worldPreviewObject;

	private static readonly int Holding = Animator.StringToHash("Holding");

	private static readonly int Punch = Animator.StringToHash("Punch");

	private static readonly int Manipulating = Animator.StringToHash("Manipulating");

	private static readonly int Pinched = Animator.StringToHash("Pinched");

	private static readonly int Crush = Animator.StringToHash("Crush");

	private static readonly int PushZ = Animator.StringToHash("PushZ");

	private static readonly int Point = Animator.StringToHash("Point");

	private static readonly int Tap = Animator.StringToHash("Tap");

	private bool snapping => ULTRAKILL.Cheats.Snapping.SnappingEnabled;

	private GameObject WorldPreviewObject
	{
		get
		{
			return worldPreviewObject;
		}
		set
		{
			if (value != worldPreviewObject && (bool)worldPreviewObject)
			{
				Object.Destroy(worldPreviewObject);
			}
			worldPreviewObject = value;
		}
	}

	private void Awake()
	{
		menu = MonoSingleton<HUDOptions>.Instance.GetComponentInChildren<SpawnMenu>(includeInactive: true);
		if ((bool)menu.arm)
		{
			Object.Destroy(base.gameObject);
		}
		menu.arm = this;
	}

	private GoreZone GetGoreZone()
	{
		if (!goreZone)
		{
			goreZone = new GameObject("Debug Gore Zone").AddComponent<GoreZone>();
			debugZone = goreZone;
		}
		return goreZone;
	}

	public void SelectObject(SpawnableObject obj)
	{
		if (obj.spawnableType == SpawnableType.ToolHand)
		{
			menu.gameObject.SetActive(value: false);
			currentObject = null;
			WorldPreviewObject = null;
			if ((bool)handPreview)
			{
				Object.Destroy(handPreview);
			}
			MonoSingleton<OptionsManager>.Instance.UnFreeze();
			currentState = ArmState.Manipulate;
			armAnimator.SetBool(Holding, value: false);
			armAnimator.SetBool(Point, value: false);
			armAnimator.SetBool(Manipulating, value: true);
		}
		else if (obj.spawnableType == SpawnableType.HandPointer)
		{
			menu.gameObject.SetActive(value: false);
			currentObject = null;
			WorldPreviewObject = null;
			if ((bool)handPreview)
			{
				Object.Destroy(handPreview);
			}
			MonoSingleton<OptionsManager>.Instance.UnFreeze();
			currentState = ArmState.Point;
			armAnimator.SetBool(Holding, value: false);
			armAnimator.SetBool(Manipulating, value: false);
			armAnimator.SetBool(Point, value: true);
		}
		else if (obj.spawnableType == SpawnableType.BlockCreator)
		{
			currentState = ArmState.BrushBlock;
			CreateHandPreview(obj);
			SetupBlockCreator(obj);
		}
		else
		{
			currentState = ((obj.spawnableType != SpawnableType.SimpleSpawn) ? ArmState.PropSpawn : ArmState.SimplePlace);
			CreateHandPreview(obj);
			CreateWorldPreview(obj);
		}
	}

	public void SetupBlockCreator(SpawnableObject template)
	{
		firstBrushPositionSet = false;
		selectSound.Play();
		currentObject = template;
		menu.gameObject.SetActive(value: false);
		MonoSingleton<OptionsManager>.Instance.UnFreeze();
		WorldPreviewObject = Object.Instantiate(axisPoint);
	}

	public void CreateHandPreview(SpawnableObject obj)
	{
		selectSound.Play();
		currentObject = obj;
		if ((bool)handPreview)
		{
			Object.Destroy(handPreview);
		}
		if ((bool)obj.preview)
		{
			GameObject gameObject = Object.Instantiate(obj.preview, holder, worldPositionStays: false);
			gameObject.transform.localPosition += obj.armOffset;
			gameObject.transform.Rotate(obj.armRotationOffset);
			handPreview = gameObject;
		}
		else
		{
			GameObject gameObject2 = Object.Instantiate(obj.gameObject, holder, worldPositionStays: false);
			SandboxUtils.StripForPreview(gameObject2.transform);
			SandboxUtils.SetLayerDeep(gameObject2.transform, holder.gameObject.layer);
			gameObject2.SetActive(value: true);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localScale *= 0.25f;
			handPreview = gameObject2;
		}
		menu.gameObject.SetActive(value: false);
		MonoSingleton<OptionsManager>.Instance.UnFreeze();
	}

	public void CreateWorldPreview(SpawnableObject obj)
	{
		if (obj.gameObject.TryGetComponent<MeshFilter>(out var component) && obj.gameObject.TryGetComponent<SandboxProp>(out var component2) && !component2.forceFullWorldPreview)
		{
			Mesh sharedMesh = component.sharedMesh;
			CreateWorldPreview();
			WorldPreviewObject.transform.localRotation = obj.gameObject.transform.localRotation;
			WorldPreviewObject.GetComponent<MeshFilter>().mesh = sharedMesh;
			WorldPreviewObject.GetComponent<MeshRenderer>().material = previewMaterial;
		}
		else
		{
			WorldPreviewObject = Object.Instantiate(obj.gameObject);
			SandboxUtils.StripForPreview(WorldPreviewObject.transform, previewMaterial);
		}
	}

	private void CreateWorldPreview()
	{
		WorldPreviewObject = new GameObject("World Preview");
		WorldPreviewObject.AddComponent<MeshFilter>();
		WorldPreviewObject.AddComponent<MeshRenderer>().material = previewMaterial;
	}

	private Vector3 CalculatePropPosition(RaycastHit hit)
	{
		if (!snapping)
		{
			brushOffset = Vector3.zero;
			if ((bool)currentObject && currentObject.spawnOffset != 0f)
			{
				return hit.point + hit.normal * currentObject.spawnOffset;
			}
			return hit.point;
		}
		Vector3 offset = Vector3.zero;
		if ((bool)hit.transform && (bool)SandboxUtils.GetProp(hit.collider.gameObject))
		{
			offset = -new Vector3(0f, SandboxUtils.SnapPos(hit.transform.position).y - hit.transform.position.y, 0f);
		}
		Vector3 vector = SandboxUtils.SnapPos(hit.point, offset);
		float distanceToPoint = new Plane(hit.normal, vector).GetDistanceToPoint(hit.point);
		vector += hit.normal * distanceToPoint;
		if ((bool)hit.transform && !SandboxUtils.GetProp(hit.collider.gameObject))
		{
			offset = hit.normal * distanceToPoint;
		}
		if (currentState == ArmState.BrushBlock && !firstBrushPositionSet)
		{
			brushOffset = offset;
		}
		if ((bool)currentObject && currentObject.spawnOffset != 0f)
		{
			vector += hit.normal * currentObject.spawnOffset;
		}
		return vector;
	}

	private Quaternion CalculatePropRotation(Vector3 normal, Quaternion baseRotation)
	{
		if (currentObject.spawnableType != SpawnableType.Prop)
		{
			return baseRotation;
		}
		float num = MonoSingleton<CameraController>.Instance.rotationY;
		if (snapping)
		{
			num /= 90f;
			num = Mathf.Round(num);
			num *= 90f;
		}
		return Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.AngleAxis(num, Vector3.up) * baseRotation;
	}

	private void OnEnable()
	{
		armAnimator.SetBool((currentState == ArmState.Manipulate) ? Manipulating : ((currentState == ArmState.Point) ? Point : Holding), value: true);
		holder.gameObject.SetActive(value: true);
		if ((bool)WorldPreviewObject)
		{
			WorldPreviewObject.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		if ((bool)WorldPreviewObject)
		{
			WorldPreviewObject.SetActive(value: false);
		}
		if (manipulatedObject != null)
		{
			ReleaseManipulatedObject(Vector3.zero);
		}
	}

	private void OnDestroy()
	{
		if ((bool)MonoSingleton<CheatsController>.Instance)
		{
			MonoSingleton<CheatsController>.Instance.arm = null;
			WorldPreviewObject = null;
		}
	}

	private IEnumerator HandClosedAnimationThing()
	{
		holder.gameObject.SetActive(value: false);
		yield return new WaitForSeconds(0.85f);
		holder.gameObject.SetActive(value: true);
	}

	private void ReleaseManipulatedObject(Vector3 velocity)
	{
		Object.Destroy(manipulatedObject.particles);
		MonoSingleton<GunControl>.Instance.activated = true;
		armAnimator.SetBool(Pinched, value: false);
		if ((bool)manipulatedObject.rigidbody)
		{
			if ((bool)manipulatedObject.rigidbody)
			{
				manipulatedObject.rigidbody.isKinematic = false;
			}
			manipulatedObject.rigidbody.velocity = velocity;
		}
		manipulatedObject = null;
	}

	private void ManipulationIntegrityCheck()
	{
		if (manipulatedObject != null && !(manipulatedObject.target != null))
		{
			ReleaseManipulatedObject(Vector3.zero);
		}
	}

	private void Update()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		if (tickDelay > 0f)
		{
			tickDelay = Mathf.MoveTowards(tickDelay, 0f, Time.deltaTime);
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && (currentState != ArmState.Manipulate || manipulatedObject == null))
		{
			menu.gameObject.SetActive(value: true);
			MonoSingleton<OptionsManager>.Instance.Freeze();
			return;
		}
		bool flag = UnityEngine.Physics.Raycast(cameraCtrl.transform.position, cameraCtrl.transform.forward, out var hitInfo, 75f, raycastLayers);
		if (currentState == ArmState.BrushBlock)
		{
			if (!firstBrushPositionSet)
			{
				WorldPreviewObject.SetActive(flag);
				WorldPreviewObject.transform.position = CalculatePropPosition(hitInfo);
			}
			else
			{
				bool num = UnityEngine.Physics.Raycast(cameraCtrl.transform.position, cameraCtrl.transform.forward, out var hitInfo2, 5f, raycastLayers);
				Vector3 zero = Vector3.zero;
				zero = ((!num) ? (cameraCtrl.transform.position + cameraCtrl.transform.forward * 4.5f) : (hitInfo2.point + new Vector3(0f, 0f, 0f)));
				zero = SandboxUtils.SnapPos(zero, brushOffset, snapping ? 0.5f : 7.5f);
				if (Mathf.Abs(firstBlockPos.x - zero.x) >= 1f)
				{
					secondBlockPos.x = zero.x;
				}
				if (Mathf.Abs(firstBlockPos.y - zero.y) >= 1f)
				{
					secondBlockPos.y = zero.y;
				}
				if (Mathf.Abs(firstBlockPos.z - zero.z) >= 1f)
				{
					secondBlockPos.z = zero.z;
				}
				if (secondBlockPos != previousSecondBlockPos)
				{
					if (tickDelay == 0f)
					{
						tickSound.pitch = Random.Range(0.74f, 0.76f);
						tickSound.Play();
						tickDelay = 0.05f;
					}
					previousSecondBlockPos = secondBlockPos;
					SandboxUtils.SmallerBigger(firstBlockPos, secondBlockPos, out var smaller, out var bigger);
					Vector3 size = bigger - smaller;
					WorldPreviewObject.GetComponent<MeshFilter>().mesh = SandboxUtils.GenerateProceduralMesh(size, simple: true);
					WorldPreviewObject.transform.position = smaller;
				}
				WorldPreviewObject.SetActive(value: true);
			}
		}
		else if ((bool)WorldPreviewObject)
		{
			WorldPreviewObject.SetActive(flag);
			if (flag)
			{
				WorldPreviewObject.transform.position = CalculatePropPosition(hitInfo);
				WorldPreviewObject.transform.rotation = CalculatePropRotation(hitInfo.normal, currentObject.gameObject.transform.rotation);
			}
		}
		ManipulationIntegrityCheck();
		if (currentState == ArmState.Manipulate && manipulatedObject != null)
		{
			Vector3 vector = new Vector3(manipulatedObject.originalRotation.x, cameraCtrl.rotationY + manipulatedObject.rotationOffset, manipulatedObject.originalRotation.z);
			vector = (snapping ? SandboxUtils.SnapRotation(vector) : vector);
			manipulatedObject.target.eulerAngles = vector;
			Vector3 vector2 = Quaternion.Euler(0f, 0f - (manipulatedObject.originalRotation.y - vector.y), 0f) * (snapping ? SandboxUtils.SnapPos(manipulatedObject.positionOffset) : manipulatedObject.positionOffset);
			Vector3 vector3 = manipulatedObject.target.position - vector2;
			Vector3 vector4 = cameraCtrl.transform.position + cameraCtrl.transform.forward * manipulatedObject.distance;
			if (snapping)
			{
				vector4 = SandboxUtils.SnapPos(vector4);
			}
			Vector3 vector5 = vector4 - vector3;
			manipulatedObject.particles.transform.position = manipulatedObject.collider.bounds.center;
			manipulatedObject.target.position += vector5 * 15.5f * Time.deltaTime;
			float y = Mouse.current.scroll.ReadValue().y;
			armAnimator.SetFloat(PushZ, y);
			manipulatedObject.distance += Mathf.Clamp(manipulatedObject.distance, 1f, 10f) / 10f * y * 0.05f;
			if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasCanceledThisFrame)
			{
				ReleaseManipulatedObject(vector5 * 6.5f);
				return;
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame)
			{
				Object.Destroy(manipulatedObject.particles);
				MonoSingleton<GunControl>.Instance.activated = true;
				armAnimator.SetBool(Pinched, value: false);
				armAnimator.SetTrigger(Crush);
				if ((bool)manipulatedObject.rigidbody)
				{
					if ((bool)manipulatedObject.rigidbody)
					{
						manipulatedObject.rigidbody.isKinematic = true;
					}
					manipulatedObject.rigidbody.velocity = Vector3.zero;
				}
				if (manipulatedObject.target.CompareTag("Untagged"))
				{
					manipulatedObject.target.tag = "Floor";
				}
				MonoSingleton<SandboxNavmesh>.Instance.MarkAsDirty();
				freezeSound.pitch = Random.Range(1f, 1.05f);
				freezeSound.Play();
				GameObject obj = new GameObject("Ghost Effect Wrapper");
				obj.transform.position = manipulatedObject.collider.bounds.center;
				GameObject gameObject = obj;
				SandboxUtils.StripForPreview(Object.Instantiate(manipulatedObject.target, gameObject.transform, worldPositionStays: true), previewMaterial);
				gameObject.gameObject.AddComponent<SandboxGhostEffect>();
				manipulatedObject = null;
				return;
			}
		}
		if (currentState == ArmState.Point && !MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame)
		{
			if (hitInfo.collider == null)
			{
				return;
			}
			EnemyIdentifierIdentifier component = hitInfo.collider.GetComponent<EnemyIdentifierIdentifier>();
			GameObject obj2;
			if ((bool)component && (bool)component.eid)
			{
				SandboxEnemy componentInParent = component.eid.GetComponentInParent<SandboxEnemy>();
				obj2 = (componentInParent ? componentInParent.gameObject : component.eid.gameObject);
			}
			else
			{
				SandboxProp prop = SandboxUtils.GetProp(hitInfo.collider.gameObject);
				if (prop == null)
				{
					DietProp dietProp = hitInfo.collider.GetComponent<DietProp>();
					if (!dietProp)
					{
						return;
					}
					if (dietProp.parent != null)
					{
						dietProp = dietProp.parent;
					}
					obj2 = dietProp.gameObject;
				}
				else
				{
					obj2 = prop.gameObject;
				}
			}
			if (hitInfo.collider.TryGetComponent<Rigidbody>(out var component2) && component2.isKinematic && (bool)MonoSingleton<SandboxNavmesh>.Instance && (!component || !component.eid))
			{
				MonoSingleton<SandboxNavmesh>.Instance.MarkAsDirty();
			}
			Object.Instantiate(genericBreakParticles).transform.position = hitInfo.collider.bounds.center;
			Object.Destroy(obj2);
			destroySound.Play();
			StopAllCoroutines();
			armAnimator.SetTrigger(Tap);
		}
		else
		{
			if (MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() || !MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame || ((currentState == ArmState.SimplePlace || currentState == ArmState.PropSpawn) && currentObject == null) || menu.gameObject.activeSelf || (!flag && (currentState != ArmState.BrushBlock || !firstBrushPositionSet)))
			{
				return;
			}
			StopAllCoroutines();
			if (currentState == ArmState.Manipulate)
			{
				if (flag)
				{
					SandboxProp prop2 = SandboxUtils.GetProp(hitInfo.collider.gameObject);
					if ((bool)prop2)
					{
						MonoSingleton<GunControl>.Instance.activated = false;
						timeSinceDebug = 0f;
						manipulatedObject = new ManipulatedObject(hitInfo, prop2)
						{
							particles = Object.Instantiate(manipulateEffect)
						};
						armAnimator.SetBool(Pinched, value: true);
					}
				}
				return;
			}
			StartCoroutine(HandClosedAnimationThing());
			jabSound.Play();
			armAnimator.SetTrigger(Punch);
			if (currentState == ArmState.BrushBlock)
			{
				if (!firstBrushPositionSet & flag)
				{
					firstBlockPos = CalculatePropPosition(hitInfo);
					secondBlockPos = firstBlockPos + Vector3.one / 0.5f;
					previousSecondBlockPos = secondBlockPos;
					firstBrushPositionSet = true;
					CreateWorldPreview();
					return;
				}
				tickSound.Play();
				SandboxUtils.SmallerBigger(firstBlockPos, secondBlockPos, out var smaller2, out var bigger2);
				Vector3 size2 = bigger2 - smaller2;
				if (SandboxUtils.CreateFinalBlock(currentObject, smaller2, size2).TryGetComponent<Rigidbody>(out var component3))
				{
					component3.isKinematic = !ULTRAKILL.Cheats.Physics.PhysicsDynamic;
					if (!ULTRAKILL.Cheats.Physics.PhysicsDynamic)
					{
						MonoSingleton<SandboxNavmesh>.Instance.MarkAsDirty();
					}
				}
				firstBrushPositionSet = false;
				WorldPreviewObject = null;
				SetupBlockCreator(currentObject);
				return;
			}
			GameObject gameObject2 = Object.Instantiate(currentObject.gameObject, CalculatePropPosition(hitInfo), CalculatePropRotation(hitInfo.normal, currentObject.gameObject.transform.localRotation), GetGoreZone().transform);
			if (gameObject2.TryGetComponent<SandboxProp>(out var component4))
			{
				component4.sourceObject = currentObject;
			}
			if (currentObject.spawnableObjectType != SpawnableObject.SpawnableObjectDataType.Enemy && gameObject2.TryGetComponent<Rigidbody>(out var component5))
			{
				component5.isKinematic = !ULTRAKILL.Cheats.Physics.PhysicsDynamic;
				if ((currentObject.spawnableType == SpawnableType.Prop || currentObject.spawnableType == SpawnableType.BlockCreator) && !ULTRAKILL.Cheats.Physics.PhysicsDynamic)
				{
					MonoSingleton<SandboxNavmesh>.Instance.MarkAsDirty();
				}
			}
			if (currentObject.spawnableObjectType == SpawnableObject.SpawnableObjectDataType.Enemy)
			{
				gameObject2.AddComponent<SandboxEnemy>().spawnSource = currentObject;
			}
			gameObject2.SetActive(value: true);
		}
	}
}
