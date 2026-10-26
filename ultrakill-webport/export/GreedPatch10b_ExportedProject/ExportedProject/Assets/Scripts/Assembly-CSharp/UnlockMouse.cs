using UnityEngine;

public class UnlockMouse : MonoBehaviour
{
	public bool lockOnDisable;

	private bool wasEnabled;

	private void OnEnable()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		wasEnabled = true;
		MonoSingleton<OptionsManager>.Instance.dontLockMouseOnUnpause = true;
	}

	private void OnDisable()
	{
		if (!lockOnDisable || !wasEnabled || !base.gameObject.scene.isLoaded)
		{
			return;
		}
		wasEnabled = false;
		if (MonoSingleton<OptionsManager>.Instance != null)
		{
			if (!MonoSingleton<OptionsManager>.Instance.paused)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			MonoSingleton<OptionsManager>.Instance.dontLockMouseOnUnpause = false;
		}
	}

	private void OnApplicationQuit()
	{
		lockOnDisable = false;
	}
}
