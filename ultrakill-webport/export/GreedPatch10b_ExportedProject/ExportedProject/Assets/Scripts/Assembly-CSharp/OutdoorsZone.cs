using UnityEngine;

public class OutdoorsZone : MonoBehaviour
{
	private OutdoorLightMaster olm;

	private int hasRequested;

	private void Start()
	{
		if (TryGetComponent<Collider>(out var component) && !MonoSingleton<OutdoorLightMaster>.Instance.outdoorsZones.Contains(component))
		{
			MonoSingleton<OutdoorLightMaster>.Instance.outdoorsZones.Add(component);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!olm)
		{
			olm = GetComponentInParent<OutdoorLightMaster>();
		}
		if ((bool)olm && other.gameObject.tag == "Player")
		{
			if (hasRequested == 0)
			{
				olm.AddRequest();
			}
			hasRequested++;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!olm)
		{
			olm = GetComponentInParent<OutdoorLightMaster>();
		}
		if ((bool)olm && other.gameObject.tag == "Player")
		{
			if (hasRequested == 1)
			{
				olm.RemoveRequest();
			}
			hasRequested--;
		}
	}
}
