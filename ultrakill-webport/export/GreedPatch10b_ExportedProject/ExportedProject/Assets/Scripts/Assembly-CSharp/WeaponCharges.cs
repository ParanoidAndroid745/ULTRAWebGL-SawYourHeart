using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class WeaponCharges : MonoSingleton<WeaponCharges>
{
	private GunControl gc;

	public float rev0charge = 100f;

	public bool rev0alt;

	public float rev1charge = 400f;

	[HideInInspector]
	public bool nai0set;

	public float naiHeatsinks = 2f;

	public float naiSawHeatsinks = 1f;

	public float naiheatUp;

	public float naiAmmo = 100f;

	public float naiSaws = 10f;

	public bool naiAmmoDontCharge;

	[HideInInspector]
	public List<Magnet> magnets = new List<Magnet>();

	public float naiMagnetCharge = 3f;

	public float raicharge = 5f;

	public GameObject railCannonFullChargeSound;

	public bool railChargePlayed;

	[HideInInspector]
	public bool rocketset;

	public float rocketcharge;

	public float revaltpickupcharge;

	private void Update()
	{
		if (NoWeaponCooldown.NoCooldown)
		{
			MaxCharges();
		}
		else
		{
			Charge(Time.deltaTime);
		}
	}

	public void Charge(float amount)
	{
		if (rev0charge < 100f)
		{
			if (rev0alt)
			{
				rev0charge = Mathf.MoveTowards(rev0charge, 100f, 20f * amount);
			}
			else
			{
				rev0charge = Mathf.MoveTowards(rev0charge, 100f, 40f * amount);
			}
		}
		if (rev1charge < 400f)
		{
			rev1charge = Mathf.MoveTowards(rev1charge, 400f, 25f * amount);
		}
		if (naiHeatsinks < 2f)
		{
			naiHeatsinks = Mathf.MoveTowards(naiHeatsinks, 2f, amount * 0.075f);
		}
		if (naiSawHeatsinks < 1f)
		{
			naiSawHeatsinks = Mathf.MoveTowards(naiSawHeatsinks, 1f, amount * 0.075f);
		}
		if (naiheatUp > 0f)
		{
			naiheatUp = Mathf.MoveTowards(naiheatUp, 0f, amount * 0.3f);
		}
		if (raicharge < 5f)
		{
			raicharge = Mathf.MoveTowards(raicharge, 5f, amount * 0.25f);
			if (raicharge >= 4f && (bool)railCannonFullChargeSound)
			{
				raicharge = 5f;
				PlayRailCharge();
			}
		}
		if (rocketcharge > 0f)
		{
			rocketcharge = Mathf.MoveTowards(rocketcharge, 0f, amount);
		}
		if (revaltpickupcharge > 0f)
		{
			revaltpickupcharge = Mathf.MoveTowards(revaltpickupcharge, 0f, amount);
		}
		if (!naiAmmoDontCharge)
		{
			naiAmmo = Mathf.MoveTowards(naiAmmo, 100f, amount * 3.5f);
			naiSaws = Mathf.MoveTowards(naiSaws, 10f, amount * 0.5f);
		}
		if (magnets.Count > 0)
		{
			for (int num = magnets.Count - 1; num >= 0; num--)
			{
				if (magnets[num] == null)
				{
					magnets.RemoveAt(num);
				}
			}
			if (magnets.Count < 3 && naiMagnetCharge < (float)(3 - magnets.Count))
			{
				naiMagnetCharge = Mathf.MoveTowards(naiMagnetCharge, 3 - magnets.Count, amount * 3f);
			}
		}
		else if (naiMagnetCharge < 3f)
		{
			naiMagnetCharge = Mathf.MoveTowards(naiMagnetCharge, 3f, amount * 3f);
		}
	}

	public void MaxCharges()
	{
		rev0charge = 100f;
		rev1charge = 400f;
		naiHeatsinks = 2f;
		naiSawHeatsinks = 1f;
		naiheatUp = 0f;
		rocketcharge = 0f;
		revaltpickupcharge = 0f;
		naiAmmo = 100f;
		naiSaws = 10f;
		magnets.Clear();
		naiMagnetCharge = 3f;
		if (raicharge < 5f)
		{
			raicharge = 5f;
			PlayRailCharge();
		}
		if (!gc)
		{
			gc = GetComponent<GunControl>();
		}
		if ((bool)gc && (bool)gc.currentWeapon)
		{
			gc.currentWeapon.SendMessage("MaxCharge", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void PlayRailCharge()
	{
		railChargePlayed = true;
		Object.Instantiate(railCannonFullChargeSound);
	}
}
