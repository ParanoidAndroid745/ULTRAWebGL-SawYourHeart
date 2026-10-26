using UnityEngine;

public class OutdoorsChecker : MonoBehaviour
{
	private bool inverse;

	public bool nonSolid = true;

	public bool oneTime;

	public GameObject[] targets;

	private BoxCollider boxCol;

	private void Start()
	{
		if (!MonoSingleton<OutdoorLightMaster>.Instance)
		{
			base.enabled = false;
			return;
		}
		if (MonoSingleton<OutdoorLightMaster>.Instance.inverse)
		{
			inverse = true;
		}
		if (targets.Length == 0)
		{
			targets = new GameObject[1];
			targets[0] = base.gameObject;
		}
		boxCol = GetComponent<BoxCollider>();
		SlowUpdate();
	}

	private void SlowUpdate()
	{
		if (!oneTime)
		{
			Invoke("SlowUpdate", 0.5f);
		}
		Collider[] array = Physics.OverlapSphere(boxCol ? boxCol.bounds.center : base.transform.position, 0.1f, 262144, QueryTriggerInteraction.Collide);
		GameObject[] array3;
		if (array != null && array.Length != 0)
		{
			Collider[] array2 = array;
			foreach (Collider item in array2)
			{
				if (!MonoSingleton<OutdoorLightMaster>.Instance.outdoorsZones.Contains(item))
				{
					continue;
				}
				array3 = targets;
				foreach (GameObject gameObject in array3)
				{
					if ((bool)gameObject)
					{
						if (!inverse)
						{
							gameObject.layer = (nonSolid ? 25 : 24);
						}
						else
						{
							gameObject.layer = ((!nonSolid) ? 8 : 0);
						}
					}
				}
				return;
			}
		}
		array3 = targets;
		foreach (GameObject gameObject2 in array3)
		{
			if ((bool)gameObject2)
			{
				if (!inverse)
				{
					gameObject2.layer = ((!nonSolid) ? 8 : 0);
				}
				else
				{
					gameObject2.layer = (nonSolid ? 25 : 24);
				}
			}
		}
	}
}
