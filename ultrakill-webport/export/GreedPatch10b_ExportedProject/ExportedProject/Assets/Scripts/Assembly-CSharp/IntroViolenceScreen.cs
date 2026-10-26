using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroViolenceScreen : MonoBehaviour
{
	private Image img;

	private float fadeAmount;

	private bool fade;

	private float targetAlpha = 1f;

	private VideoPlayer vp;

	private bool videoOver;

	private void Start()
	{
		img = GetComponent<Image>();
		vp = GetComponent<VideoPlayer>();
		vp.SetDirectAudioVolume(0, MonoSingleton<PrefsManager>.Instance.GetFloat("allVolume") / 2f);
		Cursor.visible = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
		{
			Skip();
		}
		if (Gamepad.current != null && (Gamepad.current.startButton.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame))
		{
			Skip();
		}
		if (!videoOver && vp.isPaused)
		{
			videoOver = true;
			vp.Stop();
			Invoke("FadeOut", 1f);
		}
		if (!fade)
		{
			return;
		}
		fadeAmount = Mathf.MoveTowards(fadeAmount, targetAlpha, Time.deltaTime);
		Color color = img.color;
		color.a = fadeAmount;
		img.color = color;
		if (fadeAmount == targetAlpha)
		{
			if (fadeAmount == 1f)
			{
				fade = false;
				targetAlpha = 0f;
				Invoke("FadeOut", 3f);
			}
			else if (!GameProgressSaver.GetIntro() || !GameProgressSaver.GetTutorial())
			{
				SceneManager.LoadScene("Tutorial");
			}
			else
			{
				SceneManager.LoadScene("Main Menu");
			}
		}
	}

	private void Skip()
	{
		if (vp.isPlaying)
		{
			vp.Stop();
			Invoke("FadeOut", 1f);
		}
		else if (fade)
		{
			targetAlpha = 0f;
		}
		else
		{
			CancelInvoke("FadeOut");
			targetAlpha = 0f;
			fade = true;
		}
	}

	private void FadeOut()
	{
		fade = true;
	}
}
