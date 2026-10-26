using UnityEngine;

public class Readable : MonoBehaviour
{
	[SerializeField]
	[TextArea(3, 12)]
	private string content;

	[SerializeField]
	private bool instantScan;

	private bool pickedUp;

	public void PickUp()
	{
		pickedUp = true;
		MonoSingleton<ScanningStuff>.Instance.oldWeaponState = !MonoSingleton<GunControl>.Instance.noWeapons;
		Invoke("StartScan", 0.5f);
	}

	public void PutDown()
	{
		pickedUp = false;
		CancelInvoke("StartScan");
		MonoSingleton<ScanningStuff>.Instance.ResetState();
	}

	private void StartScan()
	{
		MonoSingleton<ScanningStuff>.Instance.ScanBook(content, instantScan, base.gameObject.GetInstanceID());
	}
}
