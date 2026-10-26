using UnityEngine;

public class ScaleNFade : MonoBehaviour
{
	public bool scale;

	public bool fade;

	public FadeType ft;

	public float scaleSpeed;

	public float fadeSpeed;

	private SpriteRenderer sr;

	private LineRenderer lr;

	private Light lght;

	private Renderer rend;

	public bool dontDestroyOnZero;

	private void Start()
	{
		if (fade && ft == FadeType.Sprite)
		{
			sr = GetComponent<SpriteRenderer>();
		}
		else if (fade && ft == FadeType.Line)
		{
			lr = GetComponent<LineRenderer>();
		}
		else if (fade && ft == FadeType.Light)
		{
			lght = GetComponent<Light>();
		}
		else if (fade && ft == FadeType.Renderer)
		{
			rend = GetComponent<Renderer>();
		}
	}

	private void Update()
	{
		if (scale)
		{
			base.transform.localScale += Vector3.one * Time.deltaTime * scaleSpeed;
		}
		if (!fade)
		{
			return;
		}
		if (ft == FadeType.Sprite && sr.color.a > 0f)
		{
			Color color = sr.color;
			color.a -= fadeSpeed * Time.deltaTime;
			sr.color = color;
			if (sr.color.a <= 0f && !dontDestroyOnZero)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else if (ft == FadeType.Light && lght.range > 0f)
		{
			lght.range -= Time.deltaTime * fadeSpeed;
			if (lght.range <= 0f && !dontDestroyOnZero)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			if (ft != FadeType.Renderer)
			{
				return;
			}
			float num = rend.material.GetFloat("_OpacScale");
			if (num > 0f)
			{
				num -= Time.deltaTime * fadeSpeed;
				rend.material.SetFloat("_OpacScale", num);
				if (num <= 0f && !dontDestroyOnZero)
				{
					Object.Destroy(base.gameObject);
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (ft == FadeType.Line)
		{
			Color startColor = lr.startColor;
			startColor.a -= fadeSpeed * Time.deltaTime;
			lr.startColor = startColor;
			startColor = lr.endColor;
			startColor.a -= fadeSpeed * Time.deltaTime;
			lr.endColor = startColor;
			if (lr.startColor.a <= 0f && lr.endColor.a <= 0f && !dontDestroyOnZero)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
