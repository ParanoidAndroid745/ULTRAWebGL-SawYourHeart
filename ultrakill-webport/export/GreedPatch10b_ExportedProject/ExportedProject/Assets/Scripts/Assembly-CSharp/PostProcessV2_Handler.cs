using System;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PostProcessV2_Handler : MonoSingleton<PostProcessV2_Handler>
{
	private bool mainCameraOnly;

	public Shader outlineJFA_Initialize;

	public Shader outlineJFA_Loop;

	[Space(10f)]
	public Material postProcessV2_VSRM;

	public Texture sandTex;

	public Texture ditherTexture;

	public int distance = 1;

	private bool distanceConfirm;

	private Camera mainCam;

	public Camera hudCam;

	public Camera virtualCam;

	private RenderTexture mainTex;

	private RenderTexture hudTex;

	private RenderTexture outlineTex_BufferA;

	private RenderTexture outlineTex_BufferB;

	private Material outlineJFA_Initialize_Mat;

	private Material outlineJFA_Loop_Mat;

	private int width;

	private int height;

	private int lastWidth;

	private int lastHeight;

	private bool resolutionChanged;

	[HideInInspector]
	public bool enableJFA;

	[HideInInspector]
	public float downscaleResolution;

	public Texture CurrentTexture;

	public Texture CurrentMapPaletteOverride;

	[SerializeField]
	private Material conversionMaterial;

	public void ColorPalette(bool stuff)
	{
		if (!(CurrentMapPaletteOverride != null))
		{
			MonoSingleton<PrefsManager>.Instance.SetBoolLocal("colorPalette", stuff);
			if (stuff && CurrentTexture != null)
			{
				Shader.SetGlobalFloat("_UsePalette", 1f);
				Shader.SetGlobalInt("_ColorPrecision", 2048);
				MonoSingleton<ConvertPaletteToLUT>.Instance.ConvertPalette((Texture2D)CurrentTexture, conversionMaterial);
			}
			else
			{
				Shader.SetGlobalFloat("_UsePalette", 0f);
				GraphicsOptions.ColorCompressionApply(MonoSingleton<PrefsManager>.Instance.GetInt("colorCompression"));
			}
		}
	}

	public void ApplyUserColorPalette(Texture tex)
	{
		CurrentTexture = tex;
		if (!(CurrentMapPaletteOverride != null))
		{
			MonoSingleton<ConvertPaletteToLUT>.Instance.ConvertPalette((Texture2D)tex, conversionMaterial);
		}
	}

	public void ApplyMapColorPalette(Texture tex)
	{
		if (tex == null)
		{
			CurrentMapPaletteOverride = null;
			ColorPalette(MonoSingleton<PrefsManager>.Instance.GetBoolLocal("colorPalette"));
			return;
		}
		MonoSingleton<ConvertPaletteToLUT>.Instance.ConvertPalette((Texture2D)tex, conversionMaterial);
		CurrentMapPaletteOverride = tex;
		Shader.SetGlobalTexture("_PaletteTex", tex);
		Shader.SetGlobalFloat("_UsePalette", 1f);
		Shader.SetGlobalInt("_ColorPrecision", 2048);
	}

	private void ReinitializeCameras()
	{
		if (Application.isPlaying)
		{
			Camera.onPreRender = null;
			Camera.onPreRender = null;
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderCallback));
		}
	}

	private void SolveResolution()
	{
		width = Screen.width;
		height = Screen.height;
		if (downscaleResolution != 0f)
		{
			float num = width;
			float num2 = height;
			float num3 = Mathf.Min(num, num2);
			Vector2 vector = new Vector2(num / num3, num2 / num3);
			vector *= downscaleResolution;
			width = (int)vector.x;
			height = (int)vector.y;
		}
		resolutionChanged = width != lastWidth || height != lastHeight;
		lastWidth = width;
		lastHeight = height;
		postProcessV2_VSRM.SetFloat("_VirtualX", width);
		postProcessV2_VSRM.SetFloat("_VirtualY", height);
	}

	private void SetupRTs()
	{
		if (resolutionChanged)
		{
			if ((bool)mainTex)
			{
				mainTex.Release();
				UnityEngine.Object.Destroy(mainTex);
			}
			if ((bool)hudTex)
			{
				hudTex.Release();
				UnityEngine.Object.Destroy(hudTex);
			}
			mainTex = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBHalf);
			if (mainTex == null)
			{
				Debug.LogWarning(mainTex);
			}
			mainTex.antiAliasing = 1;
			mainTex.filterMode = FilterMode.Point;
			hudTex = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
			if (hudTex == null)
			{
				Debug.LogWarning(mainTex);
			}
			hudTex.antiAliasing = 1;
			hudTex.filterMode = FilterMode.Point;
		}
		mainCam.targetTexture = mainTex;
		outlineJFA_Initialize_Mat.SetTexture("_MainTex", mainTex);
		postProcessV2_VSRM.SetTexture("_MainTex", mainTex);
		hudCam.targetTexture = hudTex;
		postProcessV2_VSRM.SetTexture("_HudTex", hudTex);
	}

	private void SetupJFA()
	{
		Vector4 vector = new Vector4((float)Screen.width / (float)width, (float)Screen.height / (float)height, 0f, 0f);
		outlineJFA_Loop_Mat.SetVector("_ResolutionDifference", vector);
		Vector2 vector2 = new Vector2(1f, 1f) / new Vector2(Screen.width, Screen.height);
		Vector2 vector3 = new Vector2(distance, distance) / vector * vector2;
		distanceConfirm = vector3.x < vector2.x && vector3.y < vector2.y;
		postProcessV2_VSRM.SetFloat("_EnableJFA", Convert.ToSingle(enableJFA));
		postProcessV2_VSRM.SetFloat("_JFADistance", distance);
		if (resolutionChanged || !enableJFA || distance < 1 || distanceConfirm)
		{
			if ((bool)outlineTex_BufferA)
			{
				outlineTex_BufferA.Release();
				UnityEngine.Object.Destroy(outlineTex_BufferA);
			}
			if ((bool)outlineTex_BufferB)
			{
				outlineTex_BufferB.Release();
				UnityEngine.Object.Destroy(outlineTex_BufferB);
			}
		}
		if (enableJFA && distance > 1 && !distanceConfirm && (resolutionChanged || !outlineTex_BufferA || !outlineTex_BufferB))
		{
			outlineTex_BufferA = new RenderTexture(width, height, 0, RenderTextureFormat.RGHalf);
			outlineTex_BufferA.antiAliasing = 1;
			outlineTex_BufferA.filterMode = FilterMode.Point;
			outlineTex_BufferB = new RenderTexture(width, height, 0, RenderTextureFormat.RGHalf);
			outlineTex_BufferB.antiAliasing = 1;
			outlineTex_BufferB.filterMode = FilterMode.Point;
		}
	}

	private void JFA()
	{
		if (distance > 1 && !distanceConfirm)
		{
			Graphics.Blit(null, outlineTex_BufferA, outlineJFA_Initialize_Mat);
			for (float num = ((float)distance + 0.5f) * 0.5f; num >= 0.5f; num *= 0.5f)
			{
				outlineJFA_Loop_Mat.SetFloat("_Distance", num);
				outlineJFA_Loop_Mat.SetTexture("_MainTex", outlineTex_BufferA);
				Graphics.Blit(null, outlineTex_BufferB, outlineJFA_Loop_Mat);
				RenderTexture renderTexture = outlineTex_BufferA;
				outlineTex_BufferA = outlineTex_BufferB;
				outlineTex_BufferB = renderTexture;
			}
			postProcessV2_VSRM.SetTexture("_OutlineTex", outlineTex_BufferA);
		}
	}

	private void Start()
	{
		mainCam = MonoSingleton<CameraController>.Instance.cam;
		hudCam = mainCam.transform.Find("HUD Camera").GetComponent<Camera>();
		virtualCam = mainCam.transform.Find("Virtual Camera").GetComponent<Camera>();
		ReinitializeCameras();
		postProcessV2_VSRM.SetTexture("_Dither", ditherTexture);
		if (sandTex != null)
		{
			Shader.SetGlobalTexture("_SandTex", sandTex);
		}
		outlineJFA_Initialize_Mat = new Material(outlineJFA_Initialize);
		outlineJFA_Loop_Mat = new Material(outlineJFA_Loop);
	}

	public void ChangeCamera(bool hudless)
	{
		mainCameraOnly = hudless;
		mainCam.targetTexture = null;
		MonoSingleton<CameraController>.Instance.cam.clearFlags = mainCam.clearFlags;
		mainCam = MonoSingleton<CameraController>.Instance.cam;
		virtualCam = mainCam.transform.Find("Virtual Camera").GetComponent<Camera>();
		resolutionChanged = true;
		SetupRTs();
		Debug.Log("MainCamera: " + mainCam.name, mainCam);
	}

	public void OnPreRenderCallback(Camera cam)
	{
		if (cam == virtualCam)
		{
			SolveResolution();
			SetupRTs();
			SetupJFA();
		}
	}

	private void OnPostRenderCallback(Camera cam)
	{
		if (cam == virtualCam && enableJFA && distance > 1 && !distanceConfirm)
		{
			JFA();
		}
	}

	public void SetOutlineThickness(int newAmount)
	{
		distance = newAmount;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderCallback));
	}
}
