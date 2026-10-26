using System;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PhysicsSounds : MonoSingleton<PhysicsSounds>
{
	[Serializable]
	public struct PhysSounds
	{
		public AudioClip plastic;

		public AudioClip wood;

		public AudioClip stone;

		public AudioClip metal;

		public AudioClip fleshy;

		public AudioClip glass;

		public AudioClip grass;
	}

	public enum PhysMaterial
	{
		Plastic = 0,
		Wood = 1,
		Stone = 2,
		Metal = 3,
		Fleshy = 4,
		Glass = 5,
		Grass = 6
	}

	[SerializeField]
	private PhysSounds sounds;

	[SerializeField]
	private AudioSource template;

	public AudioClip ResolveSound(PhysMaterial material)
	{
		switch (material)
		{
		case PhysMaterial.Plastic:
			return sounds.plastic;
		case PhysMaterial.Wood:
			return sounds.wood;
		case PhysMaterial.Stone:
			return sounds.stone;
		case PhysMaterial.Metal:
			return sounds.metal;
		case PhysMaterial.Fleshy:
			return sounds.fleshy;
		case PhysMaterial.Grass:
			return sounds.grass;
		case PhysMaterial.Glass:
			return sounds.glass;
		default:
			return sounds.plastic;
		}
	}

	public void ImpactAt(Vector3 point, float magnitude, PhysMaterial material)
	{
		if (!(magnitude < 3.5f))
		{
			AudioSource audioSource = UnityEngine.Object.Instantiate(template);
			audioSource.transform.position = point;
			audioSource.clip = ResolveSound(material);
			audioSource.volume = Mathf.Lerp(0.2f, 1f, magnitude / 60f);
			audioSource.pitch = Mathf.Lerp(0.65f, 2.2f, (60f - magnitude) / 60f);
			audioSource.gameObject.SetActive(value: true);
			audioSource.Play();
		}
	}
}
