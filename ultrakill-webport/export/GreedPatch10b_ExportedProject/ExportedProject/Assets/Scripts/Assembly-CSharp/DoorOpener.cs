using UnityEngine;

public class DoorOpener : MonoBehaviour
{
	public Door door;

	public bool oneTime;

	private void OnEnable()
	{
		door.Open(enemy: false, skull: true);
		if (oneTime)
		{
			base.enabled = false;
		}
	}
}
