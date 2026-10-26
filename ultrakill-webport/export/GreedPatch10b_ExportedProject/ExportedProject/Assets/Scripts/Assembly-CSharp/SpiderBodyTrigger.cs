using UnityEngine;

public class SpiderBodyTrigger : MonoBehaviour
{
	private SpiderBody spbody;

	private bool dead;

	private void Start()
	{
		spbody = base.transform.parent.GetComponentInChildren<SpiderBody>(includeInactive: true);
	}

	private void Update()
	{
		if (spbody != null)
		{
			base.transform.position = spbody.transform.position;
			base.transform.rotation = spbody.transform.rotation;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 12)
		{
			if (!spbody)
			{
				spbody = base.transform.parent.GetComponentInChildren<SpiderBody>();
			}
			if ((bool)spbody)
			{
				spbody.TriggerHit(other);
			}
		}
	}
}
