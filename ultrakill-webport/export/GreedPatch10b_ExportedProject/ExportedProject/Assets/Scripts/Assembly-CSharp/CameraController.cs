using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CameraController : MonoSingleton<CameraController>
{
	public bool invert;

	public float minimumX = -89f;

	public float maximumX = 89f;

	public float minimumY = -360f;

	public float maximumY = 360f;

	public OptionsManager opm;

	public float scroll;

	public Vector3 originalPos;

	public Vector3 defaultPos;

	private Vector3 targetPos;

	public GameObject player;

	public NewMovement pm;

	[HideInInspector]
	public Camera cam;

	public bool activated;

	public int gamepadFreezeCount;

	public float rotationY;

	public float rotationX;

	public bool reverseX;

	public bool reverseY;

	public float cameraShaking;

	public float movementHor;

	public float movementVer;

	public int dodgeDirection;

	public float defaultFov;

	private AudioMixer[] audmix;

	private bool mouseUnlocked;

	public bool slide;

	private AssistController asscon;

	[SerializeField]
	private GameObject parryLight;

	[SerializeField]
	private GameObject parryFlash;

	[SerializeField]
	private Camera hudCamera;

	private float aspectRatio;

	private bool pixeled;

	private bool tilt;

	private float currentStop;

	private bool zooming;

	private float zoomTarget;

	private LayerMask environmentMask;

	public bool platformerCamera;

	protected override void Awake()
	{
		audmix = new AudioMixer[5]
		{
			MonoSingleton<AudioMixerController>.Instance.allSound,
			MonoSingleton<AudioMixerController>.Instance.goreSound,
			MonoSingleton<AudioMixerController>.Instance.musicSound,
			MonoSingleton<AudioMixerController>.Instance.doorSound,
			MonoSingleton<AudioMixerController>.Instance.unfreezeableSound
		};
		base.Awake();
		pm = MonoSingleton<NewMovement>.Instance;
		player = pm.gameObject;
	}

	private void Start()
	{
		if (!MonoSingleton<CheatsManager>.Instance || !MonoSingleton<CheatsManager>.Instance.MenuOpen)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		cam = GetComponent<Camera>();
		if ((bool)MonoSingleton<StatsManager>.Instance)
		{
			asscon = MonoSingleton<AssistController>.Instance;
		}
		originalPos = base.transform.localPosition;
		defaultPos = base.transform.localPosition;
		targetPos = new Vector3(defaultPos.x, defaultPos.y - 0.2f, defaultPos.z);
		float fieldOfView = MonoSingleton<PrefsManager>.Instance.GetFloat("fieldOfView");
		if (platformerCamera)
		{
			fieldOfView = 105f;
		}
		cam.fieldOfView = fieldOfView;
		defaultFov = cam.fieldOfView;
		if (opm == null && (bool)MonoSingleton<StatsManager>.Instance && (bool)MonoSingleton<OptionsManager>.Instance)
		{
			opm = MonoSingleton<OptionsManager>.Instance;
		}
		AudioMixer[] array = audmix;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetFloat("allPitch", 1f);
		}
		CheckAspectRatio();
		CheckTilt();
		CheckMouseReverse();
		environmentMask = (int)environmentMask | 0x100;
		environmentMask = (int)environmentMask | 0x1000000;
	}

	protected override void OnEnable()
	{
		if (MonoSingleton<OptionsManager>.Instance.frozen || MonoSingleton<OptionsManager>.Instance.paused)
		{
			MonoSingleton<CameraController>.Instance.activated = true;
			activated = false;
		}
		base.OnEnable();
		CheckAspectRatio();
		CheckTilt();
		CheckMouseReverse();
		float fieldOfView = MonoSingleton<PrefsManager>.Instance.GetFloat("fieldOfView");
		if (platformerCamera)
		{
			fieldOfView = 105f;
		}
		cam.fieldOfView = fieldOfView;
		defaultFov = cam.fieldOfView;
	}

	private void Update()
	{
		CheckAspectRatio();
		if (Input.GetKeyDown(KeyCode.F1) && Debug.isDebugBuild)
		{
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
		if (cameraShaking > 0f)
		{
			_ = defaultPos;
			Vector3 position = base.transform.parent.position;
			if (cameraShaking > 1f)
			{
				targetPos = new Vector3(defaultPos.x + (float)Random.Range(-1, 2), defaultPos.y + (float)Random.Range(-1, 2), defaultPos.z);
			}
			else
			{
				targetPos = new Vector3(defaultPos.x + cameraShaking * Random.Range(-1f, 1f), defaultPos.y + cameraShaking * Random.Range(-1f, 1f), defaultPos.z);
			}
			if (Physics.Raycast(position + defaultPos, targetPos - defaultPos, out var hitInfo, Vector3.Distance(targetPos, defaultPos), environmentMask))
			{
				base.transform.position = hitInfo.point - (targetPos - defaultPos).normalized * 0.5f;
			}
			else
			{
				base.transform.localPosition = targetPos;
			}
			cameraShaking -= Time.unscaledDeltaTime * 3f;
		}
		if (platformerCamera)
		{
			return;
		}
		if (player == null)
		{
			player = pm.gameObject;
		}
		scroll = Input.GetAxis("Mouse ScrollWheel");
#if UNITY_WEBGL
		// Si el navegador bloquea el pointer lock, permitimos mirar con las
		// flechas y reintentamos capturar el raton en cada click. Solo mientras
		// se esta jugando: con un menu abierto el juego suelta el raton a
		// proposito y volver a bloquearlo dejaria el menu sin responder.
		if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked && !IsMenuOpen())
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		if (activated)
		{
			float lookSpeed = 140f * Time.unscaledDeltaTime;
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				rotationY -= lookSpeed;
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				rotationY += lookSpeed;
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				rotationX -= lookSpeed * 0.7f;
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				rotationX += lookSpeed * 0.7f;
			}
		}
#endif
		bool flag = activated;
		if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad && gamepadFreezeCount > 0)
		{
			flag = false;
		}
		if (flag)
		{
			float num = 1f;
			Vector2 vector = MonoSingleton<InputManager>.Instance.InputSource.Look.ReadValue<Vector2>();
			if (zooming)
			{
				num = cam.fieldOfView / defaultFov;
			}
			if (!reverseY)
			{
				rotationX += vector.y * (opm.mouseSensitivity / 10f) * num;
			}
			else
			{
				rotationX -= vector.y * (opm.mouseSensitivity / 10f) * num;
			}
			if (!reverseX)
			{
				rotationY += vector.x * (opm.mouseSensitivity / 10f) * num;
			}
			else
			{
				rotationY -= vector.x * (opm.mouseSensitivity / 10f) * num;
			}
		}
		if (rotationY > 180f)
		{
			rotationY -= 360f;
		}
		else if (rotationY < -180f)
		{
			rotationY += 360f;
		}
		rotationX = Mathf.Clamp(rotationX, minimumX, maximumX);
		if (zooming)
		{
			cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, zoomTarget, Time.deltaTime * 300f);
		}
		else if (pm.boost)
		{
			if (dodgeDirection == 0)
			{
				cam.fieldOfView = defaultFov - defaultFov / 20f;
			}
			else if (dodgeDirection == 1)
			{
				cam.fieldOfView = defaultFov + defaultFov / 10f;
			}
		}
		else
		{
			cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, defaultFov, Time.deltaTime * 300f);
		}
		if ((bool)hudCamera)
		{
			if (zooming)
			{
				hudCamera.fieldOfView = Mathf.MoveTowards(hudCamera.fieldOfView, zoomTarget, Time.deltaTime * 300f);
			}
			else if (hudCamera.fieldOfView != 90f)
			{
				hudCamera.fieldOfView = Mathf.MoveTowards(hudCamera.fieldOfView, 90f, Time.deltaTime * 300f);
			}
		}
		if (flag)
		{
			player.transform.localEulerAngles = new Vector3(0f, rotationY, 0f);
		}
		float num2 = 0f;
		float num3 = movementHor * -1f;
		float num4 = base.transform.localEulerAngles.z;
		if (num4 > 180f)
		{
			num4 -= 360f;
		}
		num2 = ((!tilt) ? Mathf.MoveTowards(num4, 0f, Time.deltaTime * 25f * (Mathf.Abs(num4) + 0.01f)) : (pm.boost ? Mathf.MoveTowards(num4, num3 * 5f, Time.deltaTime * 100f * (Mathf.Abs(num4 - num3 * 5f) + 0.01f)) : Mathf.MoveTowards(num4, num3, Time.deltaTime * 25f * (Mathf.Abs(num4 - num3) + 0.01f))));
		if (flag)
		{
			base.transform.localEulerAngles = new Vector3(0f - rotationX, 0f, num2);
		}
		if (!pm.activated || !(cameraShaking <= 0f))
		{
			return;
		}
		if (pm.walking && pm.standing && !pm.rising)
		{
			base.transform.localPosition = new Vector3(Mathf.MoveTowards(base.transform.localPosition.x, targetPos.x, Time.deltaTime * 0.5f), Mathf.MoveTowards(base.transform.localPosition.y, targetPos.y, Time.deltaTime * 0.5f * (Mathf.Min(pm.rb.velocity.magnitude, 15f) / 15f)), Mathf.MoveTowards(base.transform.localPosition.z, targetPos.z, Time.deltaTime * 0.5f));
			if (base.transform.localPosition == targetPos && targetPos != defaultPos)
			{
				targetPos = defaultPos;
			}
			else if (base.transform.localPosition == targetPos && targetPos == defaultPos)
			{
				targetPos = new Vector3(defaultPos.x, defaultPos.y - 0.1f, defaultPos.z);
			}
		}
		else
		{
			base.transform.localPosition = defaultPos;
			targetPos = new Vector3(defaultPos.x, defaultPos.y - 0.1f, defaultPos.z);
		}
	}

	public void CameraShake(float shakeAmount)
	{
		float num = MonoSingleton<PrefsManager>.Instance.GetFloat("screenShake");
		if (num != 0f && cameraShaking < shakeAmount * num)
		{
			cameraShaking = shakeAmount * num;
		}
	}

	public void StopShake()
	{
		cameraShaking = 0f;
		base.transform.localPosition = defaultPos;
	}

	public void ResetCamera(float degreesY, float degreesX = 0f)
	{
		rotationY = degreesY;
		rotationX = degreesX;
	}

	public void Zoom(float amount)
	{
		zooming = true;
		zoomTarget = amount;
	}

	public void StopZoom()
	{
		zooming = false;
	}

	public void ResetToDefaultPos()
	{
		base.transform.localPosition = defaultPos;
		targetPos = new Vector3(defaultPos.x, defaultPos.y - 0.1f, defaultPos.z);
	}

	public Vector3 GetDefaultPos()
	{
		return base.transform.parent.position + defaultPos;
	}

	public void CheckAspectRatio()
	{
		if (!cam)
		{
			cam = GetComponent<Camera>();
		}
		if (aspectRatio != cam.aspect)
		{
			aspectRatio = cam.aspect;
			float x = Mathf.Min(aspectRatio / 1.778f, 1f);
			if ((bool)hudCamera)
			{
				hudCamera.transform.localScale = new Vector3(x, 1f, 1f);
			}
		}
	}

	public void CheckTilt()
	{
		tilt = MonoSingleton<PrefsManager>.Instance.GetBool("cameraTilt");
	}

	public void CheckMouseReverse()
	{
		reverseX = MonoSingleton<PrefsManager>.Instance.GetBool("mouseReverseX");
		reverseY = MonoSingleton<PrefsManager>.Instance.GetBool("mouseReverseY");
	}

#if UNITY_WEBGL
	/// <summary>
	/// True si hay un menu abierto (pausa, opciones, tienda, menu principal).
	/// En esos menus el juego suelta el raton a proposito para que se pueda
	/// pinchar en los botones; si se vuelve a bloquear, el navegador congela el
	/// puntero y el EventSystem recibe siempre la misma posicion (el menu deja
	/// de responder).
	/// </summary>
	private static bool IsMenuOpen()
	{
		OptionsManager om = MonoSingleton<OptionsManager>.Instance;
		if (om != null)
		{
			return om.paused || om.mainMenu;
		}
		return Cursor.visible;
	}
#endif
}
