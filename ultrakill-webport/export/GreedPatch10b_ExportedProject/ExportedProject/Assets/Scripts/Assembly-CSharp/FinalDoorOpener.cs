using UnityEngine;

public class FinalDoorOpener : MonoBehaviour
{
	public bool startTimer;

	public bool startMusic;

	private bool opened;

	private void Awake()
	{
		FinalDoor componentInParent = GetComponentInParent<FinalDoor>();
		if (componentInParent != null)
		{
			componentInParent.Open();
		}
		if (componentInParent != null)
		{
			Invoke("GoTime", 1f);
		}
		else
		{
			GoTime();
		}
	}

	public void GoTime()
	{
		if (!opened)
		{
			opened = true;
			if (startTimer)
			{
				MonoSingleton<StatsManager>.Instance.StartTimer();
			}
			if (startMusic)
			{
				MonoSingleton<MusicManager>.Instance.StartMusic();
			}
			if ((bool)MonoSingleton<OutdoorLightMaster>.Instance)
			{
				MonoSingleton<OutdoorLightMaster>.Instance.FirstDoorOpen();
			}
		}
	}
}
