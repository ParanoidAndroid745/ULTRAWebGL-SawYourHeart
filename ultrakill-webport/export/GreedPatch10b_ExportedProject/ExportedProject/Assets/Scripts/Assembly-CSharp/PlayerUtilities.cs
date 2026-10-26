using UnityEngine;

public class PlayerUtilities : MonoBehaviour
{
	private static AudioSource detachedWhoosh;

	public void DisablePlayer()
	{
		MonoSingleton<NewMovement>.Instance.gameObject.SetActive(value: false);
	}

	public void EnablePlayer()
	{
		MonoSingleton<NewMovement>.Instance.gameObject.SetActive(value: true);
	}

	public void FreezePlayer()
	{
		MonoSingleton<NewMovement>.Instance.enabled = false;
		MonoSingleton<NewMovement>.Instance.rb.isKinematic = true;
		MonoSingleton<CameraController>.Instance.activated = false;
	}

	public void UnfreezePlayer()
	{
		MonoSingleton<NewMovement>.Instance.enabled = true;
		MonoSingleton<NewMovement>.Instance.rb.isKinematic = false;
		MonoSingleton<CameraController>.Instance.activated = true;
	}

	public void FadeOutFallingWhoosh()
	{
		if ((bool)detachedWhoosh)
		{
			Object.Destroy(detachedWhoosh.gameObject);
		}
		detachedWhoosh = MonoSingleton<NewMovement>.Instance.DuplicateDetachWhoosh();
		FadeOut fadeOut = detachedWhoosh.gameObject.AddComponent<FadeOut>();
		fadeOut.auds = new AudioSource[1] { detachedWhoosh };
		fadeOut.speed = 0.1f;
	}

	public void FadeOutFallingWhooshCustomSpeed(float speed)
	{
		if ((bool)detachedWhoosh)
		{
			Object.Destroy(detachedWhoosh.gameObject);
		}
		detachedWhoosh = MonoSingleton<NewMovement>.Instance.DuplicateDetachWhoosh();
		FadeOut fadeOut = detachedWhoosh.gameObject.AddComponent<FadeOut>();
		fadeOut.auds = new AudioSource[1] { detachedWhoosh };
		fadeOut.speed = speed;
	}

	public void RestoreFallingWhoosh()
	{
		if ((bool)detachedWhoosh)
		{
			AudioSource audioSource = MonoSingleton<NewMovement>.Instance.RestoreWhoosh();
			audioSource.time = detachedWhoosh.time;
			audioSource.Play();
			Object.Destroy(detachedWhoosh.gameObject);
		}
	}

	public void YesWeapon()
	{
		MonoSingleton<GunControl>.Instance.YesWeapon();
	}

	public void NoWeapon()
	{
		MonoSingleton<GunControl>.Instance.NoWeapon();
	}

	public void YesFist()
	{
		MonoSingleton<FistControl>.Instance.YesFist();
	}

	public void NoFist()
	{
		MonoSingleton<FistControl>.Instance.NoFist();
	}

	public void HealPlayer(int health)
	{
		MonoSingleton<NewMovement>.Instance.GetHealth(health, silent: false);
	}

	public void HealPlayerSilent(int health)
	{
		MonoSingleton<NewMovement>.Instance.GetHealth(health, silent: true);
	}

	public void EmptyStamina()
	{
		MonoSingleton<NewMovement>.Instance.EmptyStamina();
	}

	public void FullStamina()
	{
		MonoSingleton<NewMovement>.Instance.FullStamina();
	}

	public void ResetHardDamage()
	{
		MonoSingleton<NewMovement>.Instance.ResetHardDamage();
	}

	public void MaxCharges()
	{
		MonoSingleton<WeaponCharges>.Instance.MaxCharges();
	}

	public void DestroyHeldObject()
	{
		if ((bool)MonoSingleton<FistControl>.Instance && (bool)MonoSingleton<FistControl>.Instance.heldObject)
		{
			Object.Destroy(MonoSingleton<FistControl>.Instance.heldObject.gameObject);
			MonoSingleton<FistControl>.Instance.currentPunch.ResetHeldState();
		}
	}

	public void PlaceHeldObject(ItemPlaceZone target)
	{
		if ((bool)MonoSingleton<FistControl>.Instance && (bool)MonoSingleton<FistControl>.Instance.currentPunch && (bool)target)
		{
			MonoSingleton<FistControl>.Instance.currentPunch.PlaceHeldObject(new ItemPlaceZone[1] { target }, target.transform);
		}
	}

	public void ForceHoldObject(ItemIdentifier pickup)
	{
		DestroyHeldObject();
		if ((bool)MonoSingleton<FistControl>.Instance && (bool)MonoSingleton<FistControl>.Instance.currentPunch)
		{
			MonoSingleton<FistControl>.Instance.currentPunch.ForceHold(pickup);
		}
	}

	public void ParryFlash()
	{
		MonoSingleton<TimeController>.Instance.ParryFlash();
	}

	public void QuitMap()
	{
		MonoSingleton<OptionsManager>.Instance.QuitMission();
	}

	public void FinishMap()
	{
		MonoSingleton<MapLoader>.Instance.SpawnFinalPitAndFinish();
	}
}
