using UnityEngine;

public class WeaponPickUp : MonoBehaviour
{
	public GameObject weapon;

	public int inventorySlot;

	private int tempSlot;

	public GunSetter gs;

	public string pPref;

	public bool arm;

	public GameObject activateOnPickup;

	private bool activated;

	private void Awake()
	{
		tempSlot = inventorySlot;
		if (!arm)
		{
			tempSlot--;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Player" && !activated)
		{
			GotActivated();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player" && !activated)
		{
			GotActivated();
		}
	}

	private void GotActivated()
	{
		activated = true;
		bool flag = GameProgressSaver.CheckGear(pPref) != 0 && MonoSingleton<PrefsManager>.Instance.GetInt("weapon." + pPref) != 0;
		if (!flag)
		{
			MonoSingleton<PrefsManager>.Instance.SetInt("weapon." + pPref, 1);
			GameProgressSaver.AddGear(pPref);
		}
		if (!arm)
		{
			MonoSingleton<GunControl>.Instance.noWeapons = false;
			if (gs != null)
			{
				gs.enabled = true;
				gs.ResetWeapons();
			}
			if (!flag)
			{
				if ((bool)MonoSingleton<MapLoader>.Instance && MonoSingleton<MapLoader>.Instance.isCustomLoaded)
				{
					flag = true;
				}
				else
				{
					for (int i = 0; i < MonoSingleton<GunControl>.Instance.slots[tempSlot].Count; i++)
					{
						if (MonoSingleton<GunControl>.Instance.slots[tempSlot][i].name == weapon.name + "(Clone)")
						{
							flag = true;
						}
					}
				}
			}
			if (!flag)
			{
				GameObject item = Object.Instantiate(weapon, MonoSingleton<GunControl>.Instance.transform);
				MonoSingleton<GunControl>.Instance.slots[tempSlot].Add(item);
				MonoSingleton<GunControl>.Instance.ForceWeapon(weapon);
				MonoSingleton<GunControl>.Instance.noWeapons = false;
				MonoSingleton<GunControl>.Instance.UpdateWeaponList();
			}
		}
		else
		{
			MonoSingleton<FistControl>.Instance.ResetFists();
			MonoSingleton<FistControl>.Instance.ForceArm(tempSlot, animation: true);
		}
		if (activateOnPickup != null)
		{
			activateOnPickup.SetActive(value: true);
		}
		base.gameObject.SetActive(value: false);
	}
}
