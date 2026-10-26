using UnityEngine;

public class PlayerActivatorRelay : MonoSingleton<PlayerActivatorRelay>
{
	[SerializeField]
	private GameObject[] toActivate;

	[SerializeField]
	private GameObject gunPanel;

	[SerializeField]
	private float delay = 0.2f;

	private int index;

	public void Activate()
	{
		if (index >= toActivate.Length)
		{
			return;
		}
		if (toActivate[index] == gunPanel)
		{
			if (!MonoSingleton<GunControl>.Instance.noWeapons && MonoSingleton<PrefsManager>.Instance.GetBool("weaponIcons"))
			{
				gunPanel.SetActive(value: true);
			}
			index++;
			if (index < toActivate.Length)
			{
				Invoke("Activate", delay);
			}
		}
		else
		{
			toActivate[index].SetActive(value: true);
			index++;
			if (index < toActivate.Length)
			{
				Invoke("Activate", delay);
			}
		}
	}
}
