using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SubtitledAudioSource : MonoBehaviour
{
	[Serializable]
	public class SubtitleData
	{
		public SubtitleDataLine[] lines;
	}

	[Serializable]
	public class SubtitleDataLine
	{
		public string subtitle;

		public float time;
	}

	[SerializeField]
	private SubtitleData subtitles;

	private AudioSource audioSource;

	private int currentVoiceLine;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
	}

	private void OnEnable()
	{
		if (audioSource.playOnAwake)
		{
			currentVoiceLine = 0;
		}
	}

	private void Update()
	{
		if (audioSource.isPlaying && subtitles.lines.Length > currentVoiceLine && audioSource.time >= subtitles.lines[currentVoiceLine].time)
		{
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(subtitles.lines[currentVoiceLine].subtitle);
			currentVoiceLine++;
		}
	}
}
