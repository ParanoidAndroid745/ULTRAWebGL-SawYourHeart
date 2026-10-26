using UnityEngine;

public class SecretMissionChecker : MonoBehaviour
{
	public int secretMission;

	public UltrakillEvent onMissionGet;

	private void Start()
	{
		if (GameProgressSaver.GetSecretMission(secretMission) == 2)
		{
			onMissionGet.Invoke();
		}
	}
}
