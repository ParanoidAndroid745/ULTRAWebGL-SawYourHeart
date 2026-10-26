using UnityEngine;

public class Breakable : MonoBehaviour
{
	public bool weak;

	public bool precisionOnly;

	public bool interrupt;

	public bool playerOnly;

	public bool accurateExplosionsOnly;

	[Space(10f)]
	public GameObject breakParticle;

	public bool particleAtBoundsCenter;

	[Space(10f)]
	public bool crate;

	public int bounceHealth;

	[HideInInspector]
	public int originalBounceHealth;

	public GameObject crateCoin;

	public int coinAmount;

	private float defaultHeight;

	public bool protectorCrate;

	[Space(10f)]
	public GameObject[] activateOnBreak;

	public GameObject[] destroyOnBreak;

	public UltrakillEvent destroyEvent;

	private bool broken;

	private Collider col;

	private void Start()
	{
		defaultHeight = base.transform.localScale.y;
		originalBounceHealth = bounceHealth;
	}

	public void Bounce()
	{
		if (originalBounceHealth > 0 && (bool)crateCoin && ((bool)col || TryGetComponent<Collider>(out col)))
		{
			Object.Instantiate(crateCoin, col.bounds.center, Quaternion.identity, GoreZone.ResolveGoreZone(base.transform).transform);
		}
		if (bounceHealth > 1)
		{
			base.transform.localScale = new Vector3(base.transform.localScale.x, defaultHeight / 4f, base.transform.localScale.z);
			bounceHealth--;
		}
		else
		{
			Break();
		}
	}

	private void Update()
	{
		if (crate && base.transform.localScale.y != defaultHeight)
		{
			base.transform.localScale = new Vector3(base.transform.localScale.x, Mathf.MoveTowards(base.transform.localScale.y, defaultHeight, Time.deltaTime * 10f), base.transform.localScale.z);
		}
	}

	public void Break()
	{
		if (broken)
		{
			return;
		}
		if ((bool)GetComponent<SandboxProp>() && TryGetComponent<Rigidbody>(out var component) && component.isKinematic && (bool)MonoSingleton<SandboxNavmesh>.Instance)
		{
			MonoSingleton<SandboxNavmesh>.Instance.MarkAsDirty();
		}
		broken = true;
		if (breakParticle != null)
		{
			Vector3 position = base.transform.position;
			if (particleAtBoundsCenter && ((bool)col || TryGetComponent<Collider>(out col)))
			{
				position = col.bounds.center;
			}
			Object.Instantiate(breakParticle, position, base.transform.rotation);
		}
		if (crate)
		{
			MonoSingleton<CrateCounter>.Instance.AddCrate();
			if ((bool)crateCoin && coinAmount > 0 && ((bool)col || TryGetComponent<Collider>(out col)))
			{
				for (int i = 0; i < coinAmount; i++)
				{
					Object.Instantiate(crateCoin, col.bounds.center + new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f)), Quaternion.identity, GoreZone.ResolveGoreZone(base.transform).transform);
				}
			}
			if (protectorCrate && MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
			{
				MonoSingleton<PlatformerMovement>.Instance.AddExtraHit();
			}
		}
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		if (componentsInChildren.Length != 0)
		{
			Rigidbody[] array = componentsInChildren;
			foreach (Rigidbody obj in array)
			{
				obj.transform.SetParent(base.transform.parent, worldPositionStays: true);
				obj.isKinematic = false;
				obj.useGravity = true;
			}
		}
		if (activateOnBreak.Length != 0)
		{
			GameObject[] array2 = activateOnBreak;
			foreach (GameObject gameObject in array2)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(value: true);
				}
			}
		}
		if (destroyOnBreak.Length != 0)
		{
			GameObject[] array2 = destroyOnBreak;
			foreach (GameObject gameObject2 in array2)
			{
				if (gameObject2 != null)
				{
					Object.Destroy(gameObject2);
				}
			}
		}
		destroyEvent.Invoke();
		Object.Destroy(base.gameObject);
	}
}
