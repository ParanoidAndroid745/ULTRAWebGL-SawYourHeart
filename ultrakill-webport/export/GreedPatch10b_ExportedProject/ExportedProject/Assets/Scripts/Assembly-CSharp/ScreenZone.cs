using UnityEngine;
using UnityEngine.UI;

public class ScreenZone : MonoBehaviour
{
	private bool inZone;

	private GunControl gc;

	private FistControl pun;

	public AudioSource music;

	private void OnDisable()
	{
		if (inZone)
		{
			gc?.YesWeapon();
			pun?.StopShop();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			if (gc == null)
			{
				gc = other.GetComponentInChildren<GunControl>();
			}
			gc?.NoWeapon();
			if (pun == null)
			{
				pun = other.GetComponentInChildren<FistControl>();
			}
			pun?.ShopMode();
			inZone = true;
			GraphicRaycaster componentInChildren = GetComponentInChildren<GraphicRaycaster>(includeInactive: true);
			if (componentInChildren == null)
			{
				componentInChildren = base.transform.parent.GetComponentInChildren<GraphicRaycaster>(includeInactive: true);
			}
			ControllerPointer.raycaster = componentInChildren;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			if (gc == null)
			{
				gc = other.GetComponentInChildren<GunControl>();
			}
			gc?.YesWeapon();
			pun?.StopShop();
			inZone = false;
		}
	}

	private void Update()
	{
		if (!(music != null))
		{
			return;
		}
		if (inZone)
		{
			if (music.pitch < 1f)
			{
				music.pitch = Mathf.MoveTowards(music.pitch, 1f, Time.deltaTime);
			}
		}
		else if (music.pitch > 0f)
		{
			music.pitch = Mathf.MoveTowards(music.pitch, 0f, Time.deltaTime);
		}
	}
}
