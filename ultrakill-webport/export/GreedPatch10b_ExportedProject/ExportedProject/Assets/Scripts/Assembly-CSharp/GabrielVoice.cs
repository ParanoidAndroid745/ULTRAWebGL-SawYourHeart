using System.Collections.Generic;
using UnityEngine;

public class GabrielVoice : MonoBehaviour
{
	private AudioSource aud;

	public AudioClip[] hurt;

	public AudioClip[] bigHurt;

	public AudioClip enough;

	public AudioClip[] taunt;

	[SerializeField]
	private string[] taunts;

	private List<int> usedTaunts = new List<int>();

	private int priority;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if ((bool)aud && !aud.isPlaying)
		{
			priority = 0;
		}
	}

	public void Hurt()
	{
		if (priority <= 0)
		{
			if (hurt.Length > 1)
			{
				aud.clip = hurt[Random.Range(0, hurt.Length)];
			}
			else
			{
				aud.clip = hurt[0];
			}
			aud.volume = 0.75f;
			aud.Play();
		}
	}

	public void BigHurt()
	{
		if (priority <= 2)
		{
			priority = 2;
			if (bigHurt.Length > 1)
			{
				aud.clip = bigHurt[Random.Range(0, bigHurt.Length)];
			}
			else
			{
				aud.clip = bigHurt[0];
			}
			aud.volume = 1f;
			aud.Play();
		}
	}

	public void Enough()
	{
		if (priority <= 3)
		{
			priority = 3;
			aud.clip = enough;
			aud.volume = 1f;
			aud.Play();
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Enough!");
		}
	}

	public void Taunt()
	{
		if (!aud)
		{
			aud = GetComponent<AudioSource>();
		}
		if (priority > 1)
		{
			return;
		}
		priority = 1;
		if (taunt.Length > 1)
		{
			int num = Random.Range(0, taunt.Length);
			if (usedTaunts.Contains(num))
			{
				for (int i = 0; i < taunt.Length; i++)
				{
					if (!usedTaunts.Contains(i))
					{
						num = i;
						break;
					}
				}
			}
			aud.clip = taunt[num];
			if (usedTaunts.Count == taunt.Length - 1)
			{
				usedTaunts.Clear();
			}
			usedTaunts.Add(num);
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(taunts[num]);
		}
		else
		{
			aud.clip = taunt[0];
		}
		aud.volume = 0.85f;
		aud.Play();
	}
}
