using UnityEngine;

public class RevolverAnimationReceiver : MonoBehaviour
{
	private Revolver rev;

	public GameObject click;

	private void Start()
	{
		rev = GetComponentInParent<Revolver>();
	}

	public void ReadyGun()
	{
		rev.ReadyGun();
	}

	public void Click()
	{
		if ((bool)click)
		{
			Object.Instantiate(click);
		}
	}
}
