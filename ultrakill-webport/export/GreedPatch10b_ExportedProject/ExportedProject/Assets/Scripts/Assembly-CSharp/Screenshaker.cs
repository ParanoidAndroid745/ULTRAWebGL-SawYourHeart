using UnityEngine;

public class Screenshaker : MonoBehaviour
{
	public float amount;

	public bool oneTime;

	public bool continuous;

	private void OnEnable()
	{
		MonoSingleton<CameraController>.Instance.CameraShake(amount);
		if (oneTime)
		{
			Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (continuous && base.gameObject.activeInHierarchy)
		{
			MonoSingleton<CameraController>.Instance.CameraShake(amount);
		}
	}
}
