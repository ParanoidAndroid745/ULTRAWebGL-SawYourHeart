using UnityEngine;

public class RemoveOnTime : MonoBehaviour
{
	public bool useAudioLength;

	public float time;

	public float randomizer;

	private void Start()
	{
		if (useAudioLength)
		{
			AudioSource component = GetComponent<AudioSource>();
			if (!component)
			{
				Debug.LogError("useAudioLength is enabled, but an AudioSource was not found");
				Object.Destroy(this);
			}
			else if (!component.clip)
			{
				Debug.LogError("useAudioLength is enabled without a clip");
				Object.Destroy(this);
			}
			else
			{
				Invoke("Remove", component.clip.length);
			}
		}
		else
		{
			Invoke("Remove", time + Random.Range(0f - randomizer, randomizer));
		}
	}

	private void Remove()
	{
		Object.Destroy(base.gameObject);
	}
}
