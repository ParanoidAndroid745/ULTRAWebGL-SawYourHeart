using UnityEngine;

public class CoinActivated : MonoBehaviour
{
	public UltrakillEvent events;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Coin"))
		{
			events.Invoke();
			GetComponent<Collider>().enabled = false;
		}
	}
}
