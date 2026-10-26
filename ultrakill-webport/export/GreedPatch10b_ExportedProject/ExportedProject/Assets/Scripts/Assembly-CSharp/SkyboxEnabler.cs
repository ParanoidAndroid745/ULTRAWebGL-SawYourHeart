using UnityEngine;

public class SkyboxEnabler : MonoBehaviour
{
	private void Start()
	{
		if (MonoSingleton<CameraController>.Instance.TryGetComponent<Camera>(out var component))
		{
			component.clearFlags = CameraClearFlags.Skybox;
		}
	}
}
