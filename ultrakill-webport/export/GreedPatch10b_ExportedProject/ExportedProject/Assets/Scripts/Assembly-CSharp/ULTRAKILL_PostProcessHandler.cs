using UnityEngine;

public class ULTRAKILL_PostProcessHandler : MonoBehaviour
{
	private Camera cam;

	private RenderTexture tempRT;

	public Material targetMat;

	private Vector2 Resolution;

	private OptionsManager opm;

	private bool simplifyEnemies;

	public Texture sandTex;

	private void Awake()
	{
		cam = GetComponent<Camera>();
		if (sandTex != null)
		{
			Shader.SetGlobalTexture("_SandTex", sandTex);
		}
	}

	private void Start()
	{
		opm = MonoSingleton<OptionsManager>.Instance;
		if (sandTex != null)
		{
			Shader.SetGlobalTexture("_SandTex", sandTex);
		}
	}

	private void OnPreRender()
	{
		getRT();
	}

	private void OnEnable()
	{
		targetMat.SetTexture("_MainTex", tempRT);
	}

	private void getRT()
	{
		if (cam == null)
		{
			cam = GetComponent<Camera>();
		}
		if (!opm)
		{
			opm = MonoSingleton<OptionsManager>.Instance;
			if (!opm)
			{
				opm = Object.FindObjectOfType<OptionsManager>();
			}
			if (!opm)
			{
				return;
			}
		}
		Vector2 vector = new Vector2(Screen.width, Screen.height);
		if (Resolution != vector || simplifyEnemies != opm.simplifyEnemies)
		{
			Resolution = vector;
			RenderTexture.ReleaseTemporary(tempRT);
			if (base.gameObject.name == "Main Camera" && opm.simplifyEnemies)
			{
				tempRT = RenderTexture.GetTemporary((int)Resolution.x, (int)Resolution.y, 24, RenderTextureFormat.ARGBHalf);
			}
			else
			{
				tempRT = RenderTexture.GetTemporary((int)Resolution.x, (int)Resolution.y, 24, RenderTextureFormat.ARGB32);
			}
			tempRT.antiAliasing = 1;
			tempRT.filterMode = FilterMode.Point;
			cam.targetTexture = tempRT;
			targetMat.SetTexture("_MainTex", tempRT);
			simplifyEnemies = opm.simplifyEnemies;
		}
		if (targetMat == null)
		{
			Debug.LogWarning("Cannot render Camera because it lacks a target Material!");
		}
	}
}
