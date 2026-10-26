using UnityEngine;

public class WeaponIdentifier : MonoBehaviour
{
	public float delay;

	public float speedMultiplier;

	private void Start()
	{
		if (speedMultiplier == 0f)
		{
			speedMultiplier = 1f;
		}
	}
}
