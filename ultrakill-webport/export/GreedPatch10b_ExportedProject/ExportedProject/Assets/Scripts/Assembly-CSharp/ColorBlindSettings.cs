using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class ColorBlindSettings : MonoSingleton<ColorBlindSettings>
{
	public Color[] variationColors;

	[Header("HUD Colors")]
	public Color healthBarColor;

	public Color healthBarAfterImageColor;

	public Color antiHpColor;

	public Color overHealColor;

	public Color healthBarTextColor;

	public Color staminaColor;

	public Color staminaChargingColor;

	public Color staminaEmptyColor;

	public Color railcannonFullColor;

	public Color railcannonChargingColor;

	[Header("Enemy Colors")]
	public Color filthColor;

	public Color strayColor;

	public Color schismColor;

	public Color shotgunnerColor;

	public Color stalkerColor;

	public Color sisyphusColor;

	public Color droneColor;

	public Color streetcleanerColor;

	public Color swordsmachineColor;

	public Color mindflayerColor;

	public Color v2Color;

	public Color maliciousColor;

	public Color cerberusColor;

	public Color virtueColor;

	public Color enrageColor;

	public void UpdateEnemyColors()
	{
		EnemySimplifier[] array = Object.FindObjectsOfType<EnemySimplifier>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateColors();
		}
	}

	public void UpdateHudColors()
	{
		StaminaMeter[] array = Object.FindObjectsOfType<StaminaMeter>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateColors();
		}
		ColorBlindGet[] array2 = Object.FindObjectsOfType<ColorBlindGet>();
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].UpdateColor();
		}
	}

	public void UpdateWeaponColors()
	{
		WeaponIcon weaponIcon = Object.FindObjectOfType<WeaponIcon>();
		if ((bool)weaponIcon)
		{
			weaponIcon.UpdateIcon();
		}
		MonoSingleton<FistControl>.Instance.UpdateFistIcon();
	}

	public Color GetEnemyColor(EnemyType ect)
	{
		switch (ect)
		{
		case EnemyType.Cerberus:
			return cerberusColor;
		case EnemyType.Drone:
			return droneColor;
		case EnemyType.Filth:
			return filthColor;
		case EnemyType.MaliciousFace:
			return maliciousColor;
		case EnemyType.Mindflayer:
			return mindflayerColor;
		case EnemyType.Schism:
			return schismColor;
		case EnemyType.Sisyphus:
			return sisyphusColor;
		case EnemyType.Soldier:
			return shotgunnerColor;
		case EnemyType.Stalker:
			return stalkerColor;
		case EnemyType.Stray:
			return strayColor;
		case EnemyType.Streetcleaner:
			return streetcleanerColor;
		case EnemyType.Swordsmachine:
			return swordsmachineColor;
		case EnemyType.V2:
			return v2Color;
		case EnemyType.Virtue:
			return virtueColor;
		default:
			return Color.black;
		}
	}

	public Color GetHudColor(HudColorType hct)
	{
		switch (hct)
		{
		case HudColorType.antiHp:
			return antiHpColor;
		case HudColorType.health:
			return healthBarColor;
		case HudColorType.healthAfterImage:
			return healthBarAfterImageColor;
		case HudColorType.healthText:
			return healthBarTextColor;
		case HudColorType.overheal:
			return overHealColor;
		case HudColorType.stamina:
			return staminaColor;
		case HudColorType.staminaCharging:
			return staminaChargingColor;
		case HudColorType.staminaEmpty:
			return staminaEmptyColor;
		case HudColorType.railcannonFull:
			return railcannonFullColor;
		case HudColorType.railcannonCharging:
			return railcannonChargingColor;
		default:
			return Color.white;
		}
	}

	public void SetEnemyColor(EnemyType ect, Color color)
	{
		switch (ect)
		{
		case EnemyType.Cerberus:
			cerberusColor = color;
			break;
		case EnemyType.Drone:
			droneColor = color;
			break;
		case EnemyType.Filth:
			filthColor = color;
			break;
		case EnemyType.MaliciousFace:
			maliciousColor = color;
			break;
		case EnemyType.Mindflayer:
			mindflayerColor = color;
			break;
		case EnemyType.Schism:
			schismColor = color;
			break;
		case EnemyType.Sisyphus:
			sisyphusColor = color;
			break;
		case EnemyType.Soldier:
			shotgunnerColor = color;
			break;
		case EnemyType.Stalker:
			stalkerColor = color;
			break;
		case EnemyType.Stray:
			strayColor = color;
			break;
		case EnemyType.Streetcleaner:
			streetcleanerColor = color;
			break;
		case EnemyType.Swordsmachine:
			swordsmachineColor = color;
			break;
		case EnemyType.V2:
			v2Color = color;
			break;
		case EnemyType.Virtue:
			virtueColor = color;
			break;
		}
		UpdateEnemyColors();
	}

	public void SetHudColor(HudColorType hct, Color color)
	{
		switch (hct)
		{
		case HudColorType.antiHp:
			antiHpColor = color;
			break;
		case HudColorType.health:
			healthBarColor = color;
			break;
		case HudColorType.healthAfterImage:
			healthBarAfterImageColor = color;
			break;
		case HudColorType.healthText:
			healthBarTextColor = color;
			break;
		case HudColorType.overheal:
			overHealColor = color;
			break;
		case HudColorType.stamina:
			staminaColor = color;
			break;
		case HudColorType.staminaCharging:
			staminaChargingColor = color;
			break;
		case HudColorType.staminaEmpty:
			staminaEmptyColor = color;
			break;
		case HudColorType.railcannonFull:
			railcannonFullColor = color;
			break;
		case HudColorType.railcannonCharging:
			railcannonChargingColor = color;
			break;
		}
		UpdateHudColors();
	}
}
