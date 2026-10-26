using UnityEngine;

public class SandboxProp : MonoBehaviour
{
	[SerializeField]
	private PhysicsSounds.PhysMaterial physicsMaterial;

	[SerializeField]
	private bool enableImpactDamage;

	public bool forceFullWorldPreview;

	public SpawnableObject sourceObject;

	private TimeSince timeSinceLastImpact;

	[HideInInspector]
	public Rigidbody rigidbody;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		timeSinceLastImpact = 0f;
	}

	private void OnCollisionEnter(Collision other)
	{
		if (!rigidbody.isKinematic && !(other.impulse.magnitude < 3f) && !((float)timeSinceLastImpact < 0.1f))
		{
			timeSinceLastImpact = 0f;
			MonoSingleton<PhysicsSounds>.Instance.ImpactAt(other.GetContact(0).point, other.impulse.magnitude, physicsMaterial);
		}
	}

	public SavedProp Save()
	{
		return new SavedProp
		{
			ObjectIdentifier = sourceObject.identifier,
			Position = base.transform.position,
			Rotation = base.transform.rotation,
			Scale = base.transform.localScale,
			Kinematic = (!rigidbody || rigidbody.isKinematic)
		};
	}

	private void OnCollisionStay(Collision other)
	{
		OnCollisionEnter(other);
	}
}
