using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Subtitle : MonoBehaviour
{
	[HideInInspector]
	public Subtitle nextInChain;

	[SerializeField]
	private float fadeInSpeed = 0.001f;

	[SerializeField]
	private float holdForBase = 2f;

	[SerializeField]
	private float holdForPerChar = 0.1f;

	[SerializeField]
	private float fadeOutSpeed = 0.0001f;

	[SerializeField]
	private float paddingHorizontal;

	[SerializeField]
	private Text uiText;

	private Image image;

	private CanvasGroup group;

	private float currentAlpha;

	private bool isFadingIn;

	private bool chainContinue;

	private float holdFor;

	private bool isFadingOut;

	private TimeSince holdingSince;

	private RectTransform rectTransform;

	private void Awake()
	{
		image = GetComponent<Image>();
		group = GetComponent<CanvasGroup>();
		rectTransform = GetComponent<RectTransform>();
	}

	private void OnEnable()
	{
		Fit();
		string text = new Regex("<[^>]*>").Replace(uiText.text, "");
		holdFor = holdForBase + (float)text.Length * holdForPerChar;
		currentAlpha = 0f;
		isFadingIn = true;
	}

	public void ContinueChain()
	{
		chainContinue = true;
	}

	private void Update()
	{
		if (isFadingIn)
		{
			currentAlpha += fadeInSpeed * Time.deltaTime;
			if (currentAlpha >= 1f)
			{
				currentAlpha = 1f;
				isFadingIn = false;
				holdingSince = 0f;
			}
			group.alpha = currentAlpha;
		}
		else if (isFadingOut)
		{
			currentAlpha -= fadeOutSpeed * Time.deltaTime;
			if (currentAlpha <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
			group.alpha = currentAlpha;
		}
		else if ((float)holdingSince > holdFor && chainContinue)
		{
			isFadingOut = true;
			MonoSingleton<SubtitleController>.Instance.NotifyHoldEnd(this);
			if ((bool)nextInChain)
			{
				nextInChain.ContinueChain();
			}
		}
	}

	private void Fit()
	{
		StartCoroutine(FitAsync());
	}

	private IEnumerator FitAsync()
	{
		yield return new WaitForFixedUpdate();
		float preferredSize = LayoutUtility.GetPreferredSize(uiText.rectTransform, 0);
		uiText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize + paddingHorizontal * 2f);
	}
}
