public static class GetMissionName
{
	public static string GetMission(int missionNum)
	{
		if ((bool)MonoSingleton<MapLoader>.Instance && MonoSingleton<MapLoader>.Instance.isCustomLoaded)
		{
			return MapInfoBase.InstanceAnyType.levelName;
		}
		switch (missionNum)
		{
		default:
			return "MISSION NAME NOT FOUND";
		case 0:
			return "MAIN MENU";
		case 1:
			return "0-1: INTO THE FIRE";
		case 2:
			return "0-2: THE MEATGRINDER";
		case 3:
			return "0-3: DOUBLE DOWN";
		case 4:
			return "0-4: A ONE-MACHINE ARMY";
		case 5:
			return "0-5: CERBERUS";
		case 6:
			return "1-1: HEART OF THE SUNRISE";
		case 7:
			return "1-2: THE BURNING WORLD";
		case 8:
			return "1-3: HALLS OF SACRED REMAINS";
		case 9:
			return "1-4: CLAIR DE LUNE";
		case 10:
			return "2-1: BRIDGEBURNER";
		case 11:
			return "2-2: DEATH AT 20,000 VOLTS";
		case 12:
			return "2-3: SHEER HEART ATTACK";
		case 13:
			return "2-4: COURT OF THE CORPSE KING";
		case 14:
			return "3-1: BELLY OF THE BEAST";
		case 15:
			return "3-2: IN THE FLESH";
		case 16:
			return "4-1: SLAVES TO POWER";
		case 17:
			return "4-2: GOD DAMN THE SUN";
		case 18:
			return "4-3: A SHOT IN THE DARK";
		case 19:
			return "4-4: CLAIR DE SOLEIL";
		case 20:
			return "5-1: ???";
		case 21:
			return "5-2: WAVES OF THE STARLESS SEA";
		case 22:
			return "5-3: ???";
		case 23:
			return "5-4: ???";
		case 24:
			return "6-1: ???";
		case 25:
			return "6-2: ???";
		case 26:
			return "7-1: ???";
		case 27:
			return "7-2: ???";
		case 28:
			return "7-3: ???";
		case 29:
			return "7-4: ???";
		case 30:
			return "8-1: ???";
		case 31:
			return "8-2: ???";
		case 32:
			return "8-3: ???";
		case 33:
			return "8-4: ???";
		case 34:
			return "9-1: ???";
		case 35:
			return "9-2: ???";
		case 666:
			return "P-1: SOUL SURVIVOR";
		case 667:
			return "P-2: ???";
		case 668:
			return "P-3: ???";
		}
	}

	public static string GetSceneName(int missionNum)
	{
		switch (missionNum)
		{
		default:
			return "Main Menu";
		case 1:
			return "Level 0-1";
		case 2:
			return "Level 0-2";
		case 3:
			return "Level 0-3";
		case 4:
			return "Level 0-4";
		case 5:
			return "Level 0-5";
		case 6:
			return "Level 1-1";
		case 7:
			return "Level 1-2";
		case 8:
			return "Level 1-3";
		case 9:
			return "Level 1-4";
		case 10:
			return "Level 2-1";
		case 11:
			return "Level 2-2";
		case 12:
			return "Level 2-3";
		case 13:
			return "Level 2-4";
		case 14:
			return "Level 3-1";
		case 15:
			return "Level 3-2";
		case 16:
			return "Level 4-1";
		case 17:
			return "Level 4-2";
		case 18:
			return "Level 4-3";
		case 19:
			return "Level 4-4";
		case 20:
			return "Level 5-1";
		case 21:
			return "Level 5-2";
		case 22:
			return "Level 5-3";
		case 23:
			return "Level 5-4";
		case 24:
			return "Level 6-1";
		case 25:
			return "Level 6-2";
		case 26:
			return "Level 7-1";
		case 27:
			return "Level 7-2";
		case 28:
			return "Level 7-3";
		case 29:
			return "Level 7-4";
		case 30:
			return "Level 8-1";
		case 31:
			return "Level 8-2";
		case 32:
			return "Level 8-3";
		case 33:
			return "Level 8-4";
		case 34:
			return "Level 9-1";
		case 35:
			return "Level 9-2";
		case 36:
			return "Level 10-1";
		case 37:
			return "Level 10-2";
		case 666:
			return "Level P-1";
		case 667:
			return "Level P-2";
		case 668:
			return "Level P-3";
		}
	}
}
