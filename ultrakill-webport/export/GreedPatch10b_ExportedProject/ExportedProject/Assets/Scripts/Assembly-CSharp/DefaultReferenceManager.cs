using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class DefaultReferenceManager : MonoSingleton<DefaultReferenceManager>
{
	public GameObject wetParticle;

	public GameObject sandDrip;

	public GameObject blessingGlow;

	public GameObject sandificationEffect;

	public GameObject enrageEffect;
}
