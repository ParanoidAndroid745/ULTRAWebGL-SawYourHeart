using System;
using UnityEngine;

public class SeasonalHats : MonoBehaviour
{
	private DateTime time;

	[SerializeField]
	private GameObject christmas;

	[SerializeField]
	private GameObject halloween;

	[SerializeField]
	private GameObject easter;

	private void Start()
	{
		if (!MonoSingleton<PrefsManager>.Instance.GetBool("seasonalEvents"))
		{
			return;
		}
		time = DateTime.Now;
		switch (time.Month)
		{
		case 12:
			if (time.Day >= 22 && time.Day <= 28)
			{
				christmas.SetActive(value: true);
			}
			break;
		case 10:
			if (time.Day >= 25 && time.Day <= 31)
			{
				halloween.SetActive(value: true);
			}
			break;
		case 4:
			if (time.Day >= 4 && time.Day <= 25)
			{
				easter.SetActive(value: true);
			}
			break;
		}
	}
}
