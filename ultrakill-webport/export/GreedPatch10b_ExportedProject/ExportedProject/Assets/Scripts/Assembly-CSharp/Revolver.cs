using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

public class Revolver : MonoBehaviour
{
	private InputManager inman;

	private WeaponIdentifier wid;

	public int gunVariation;

	public bool altVersion;

	public Transform kickBackPos;

	private AudioSource gunAud;

	private AudioSource superGunAud;

	public AudioClip[] gunShots;

	public AudioClip[] superGunShots;

	private int currentGunShot;

	public GameObject gunBarrel;

	private bool gunReady;

	private bool shootReady = true;

	private bool pierceReady = true;

	public float shootCharge;

	public float pierceCharge;

	private bool chargingPierce;

	public float pierceShotCharge;

	public Vector3 shotHitPoint;

	public GameObject revolverBeam;

	public GameObject revolverBeamSuper;

	public RaycastHit hit;

	public RaycastHit[] allHits;

	private int currentHit;

	private int currentHitMultiplier;

	public float recoilFOV;

	public GameObject chargeEffect;

	private AudioSource ceaud;

	private Light celight;

	private GameObject camObj;

	private Camera cam;

	private CameraController cc;

	private Vector3 tempCamPos;

	public Vector3 beamReflectPos;

	private GameObject beamDirectionSetter;

	public MeshRenderer screenMR;

	public Material batteryMat;

	public Texture2D batteryFull;

	public Texture2D batteryMid;

	public Texture2D batteryLow;

	public Texture2D[] batteryCharges;

	private AudioSource screenAud;

	public AudioClip chargedSound;

	public AudioClip chargingSound;

	private int bodiesPierced;

	private Enemy enemy;

	private CharacterJoint[] cjs;

	private CharacterJoint cj;

	private GameObject limb;

	private Transform firstChild;

	private int bulletForce;

	private bool slowMo;

	private bool timeStopped;

	private float untilTimeResume;

	private int enemiesLeftToHit;

	private int enemiesPierced;

	private RaycastHit subHit;

	private float damageMultiplier = 1f;

	private SecondaryRevolver secRev;

	private bool twirling;

	private AudioClip chargeEffectSound;

	public AudioClip twirlSound;

	public bool twirlRecovery;

	private GameObject currentDrip;

	public GameObject coin;

	private Turn cylinder;

	private SwitchMaterial rimLight;

	public GunControl gc;

	private Animator anim;

	private Punch punch;

	private NewMovement nmov;

	private WeaponPos wpos;

	public Image[] coinPanels;

	public bool[] coinPanelsCharged;

	private WeaponCharges wc;

	private CameraFrustumTargeter targeter;

	private float coinCharge = 400f;

	private void Start()
	{
		targeter = MonoSingleton<CameraFrustumTargeter>.Instance;
		inman = MonoSingleton<InputManager>.Instance;
		wid = GetComponent<WeaponIdentifier>();
		gunReady = false;
		cam = MonoSingleton<CameraController>.Instance.GetComponent<Camera>();
		camObj = cam.gameObject;
		cc = MonoSingleton<CameraController>.Instance;
		nmov = MonoSingleton<NewMovement>.Instance;
		shootCharge = 0f;
		pierceShotCharge = 0f;
		pierceCharge = 100f;
		pierceReady = false;
		shootReady = false;
		gunAud = GetComponent<AudioSource>();
		kickBackPos = base.transform.parent.GetChild(0);
		superGunAud = kickBackPos.GetComponent<AudioSource>();
		if (gunVariation == 0)
		{
			screenAud = screenMR.gameObject.GetComponent<AudioSource>();
		}
		else if (gunVariation == 1)
		{
			screenAud = GetComponentInChildren<Canvas>().GetComponent<AudioSource>();
		}
		secRev = base.transform.parent.GetComponentInChildren<SecondaryRevolver>();
		chargeEffect = gunBarrel.transform.GetChild(0).gameObject;
		ceaud = chargeEffect.GetComponent<AudioSource>();
		celight = chargeEffect.GetComponent<Light>();
		chargeEffectSound = ceaud.clip;
		if (gunVariation == 0)
		{
			screenAud.clip = chargingSound;
			screenAud.loop = true;
			if (gunVariation == 0)
			{
				screenAud.pitch = 1f;
			}
			else if (gunVariation == 1)
			{
				screenAud.pitch = 1.25f;
			}
			screenAud.volume = 0.25f;
			screenAud.Play();
		}
		if (gunVariation == 2)
		{
			secRev.gameObject.SetActive(value: true);
			secRev.enabled = true;
		}
		cylinder = GetComponentInChildren<Turn>();
		gc = GetComponentInParent<GunControl>();
		beamDirectionSetter = new GameObject();
		anim = GetComponentInChildren<Animator>();
		wc = MonoSingleton<WeaponCharges>.Instance;
		wpos = GetComponent<WeaponPos>();
		if (wid.delay != 0f && gunVariation == 0)
		{
			pierceCharge = wc.rev0charge;
		}
	}

	private void OnDisable()
	{
		if (wc == null)
		{
			wc = MonoSingleton<WeaponCharges>.Instance;
		}
		if (gunVariation == 0)
		{
			if (altVersion)
			{
				wc.rev0alt = true;
			}
			wc.rev0charge = pierceCharge;
		}
		pierceShotCharge = 0f;
		gunReady = false;
	}

	private void OnEnable()
	{
		if (wc == null)
		{
			wc = MonoSingleton<WeaponCharges>.Instance;
		}
		shootCharge = 100f;
		if (gunVariation == 0)
		{
			pierceCharge = wc.rev0charge;
		}
		else if (gunVariation == 1)
		{
			pierceCharge = 100f;
			pierceReady = true;
			CheckCoinCharges();
		}
		if (altVersion)
		{
			if (!anim)
			{
				anim = GetComponentInChildren<Animator>();
			}
			if (wc.revaltpickupcharge > 0f)
			{
				anim.SetBool("SlowPickup", value: true);
			}
			else
			{
				anim.SetBool("SlowPickup", value: false);
			}
		}
		gunReady = false;
	}

	private void Update()
	{
		if (gunVariation == 2)
		{
			pierceCharge = shootCharge;
		}
		if (!shootReady)
		{
			if (gunVariation != 0)
			{
				if (shootCharge + 175f * Time.deltaTime < 100f)
				{
					shootCharge += 175f * Time.deltaTime;
				}
				else
				{
					shootCharge = 100f;
					shootReady = true;
				}
			}
			else if (shootCharge + 200f * Time.deltaTime < 100f)
			{
				shootCharge += 200f * Time.deltaTime;
			}
			else
			{
				shootCharge = 100f;
				shootReady = true;
			}
		}
		if (!pierceReady)
		{
			if (gunVariation == 0)
			{
				if (NoWeaponCooldown.NoCooldown)
				{
					pierceCharge = 100f;
				}
				float num = 1f;
				if (altVersion)
				{
					num = 0.5f;
				}
				if (pierceCharge + 40f * Time.deltaTime < 100f)
				{
					pierceCharge += 40f * Time.deltaTime * num;
				}
				else
				{
					pierceCharge = 100f;
					pierceReady = true;
					screenAud.clip = chargedSound;
					screenAud.loop = false;
					screenAud.volume = 0.35f;
					screenAud.pitch = Random.Range(1f, 1.1f);
					screenAud.Play();
				}
				if (pierceCharge < 50f)
				{
					screenMR.material.SetTexture("_MainTex", batteryLow);
					screenMR.material.color = Color.red;
				}
				else if (pierceCharge < 100f)
				{
					screenMR.material.SetTexture("_MainTex", batteryMid);
					screenMR.material.color = Color.yellow;
				}
				else
				{
					screenMR.material.SetTexture("_MainTex", batteryFull);
				}
			}
			else if (gunVariation == 1)
			{
				if (pierceCharge + 480f * Time.deltaTime < 100f)
				{
					pierceCharge += 480f * Time.deltaTime;
				}
				else
				{
					pierceCharge = 100f;
					pierceReady = true;
				}
			}
		}
		else if (pierceShotCharge != 0f)
		{
			if (pierceShotCharge < 50f)
			{
				screenMR.material.SetTexture("_MainTex", batteryCharges[0]);
			}
			else if (pierceShotCharge < 100f)
			{
				screenMR.material.SetTexture("_MainTex", batteryCharges[1]);
			}
			else
			{
				screenMR.material.SetTexture("_MainTex", batteryCharges[2]);
			}
			base.transform.localPosition = new Vector3(wpos.currentDefault.x + pierceShotCharge / 250f * Random.Range(-0.05f, 0.05f), wpos.currentDefault.y + pierceShotCharge / 250f * Random.Range(-0.05f, 0.05f), wpos.currentDefault.z + pierceShotCharge / 250f * Random.Range(-0.05f, 0.05f));
			cylinder.spinSpeed = pierceShotCharge;
		}
		else
		{
			if (gunVariation == 0 && screenMR.material.GetTexture("_MainTex") != batteryFull)
			{
				screenMR.material.SetTexture("_MainTex", batteryFull);
			}
			if (cylinder.spinSpeed != 0f)
			{
				cylinder.spinSpeed = 0f;
			}
		}
		if (gc.activated)
		{
			if (gunVariation == 0 && gunReady)
			{
				if ((MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasCanceledThisFrame || (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)) && shootReady && pierceShotCharge == 100f)
				{
					if (!wid || wid.delay == 0f)
					{
						Shoot(2);
					}
					else
					{
						shootReady = false;
						shootCharge = 0f;
						Invoke("DelayedShoot2", wid.delay);
					}
					pierceShotCharge = 0f;
				}
				else if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && shootReady && !chargingPierce)
				{
					if (!wid || wid.delay == 0f)
					{
						Shoot();
					}
					else
					{
						shootReady = false;
						shootCharge = 0f;
						Invoke("DelayedShoot", wid.delay);
					}
				}
				else if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && shootReady && pierceReady)
				{
					chargingPierce = true;
					if (pierceShotCharge + 175f * Time.deltaTime < 100f)
					{
						pierceShotCharge += 175f * Time.deltaTime;
					}
					else
					{
						pierceShotCharge = 100f;
					}
				}
				else
				{
					chargingPierce = false;
					if (pierceShotCharge - 175f * Time.deltaTime > 0f)
					{
						pierceShotCharge -= 175f * Time.deltaTime;
					}
					else
					{
						pierceShotCharge = 0f;
					}
				}
			}
			else if (gunVariation == 1)
			{
				if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && pierceReady && coinCharge >= 100f)
				{
					cc.StopShake();
					if (!wid || wid.delay == 0f)
					{
						wc.rev1charge -= 100f;
					}
					if (!wid || wid.delay == 0f)
					{
						ThrowCoin();
					}
					else
					{
						Invoke("ThrowCoin", wid.delay);
						pierceReady = false;
						pierceCharge = 0f;
					}
				}
				else if (gunReady && !MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && shootReady)
				{
					if (!wid || wid.delay == 0f)
					{
						Shoot();
					}
					else
					{
						shootReady = false;
						shootCharge = 0f;
						Invoke("DelayedShoot", wid.delay);
					}
					twirling = false;
					if (ceaud.volume != 0f)
					{
						ceaud.volume = 0f;
					}
					twirlRecovery = false;
				}
				else
				{
					twirling = false;
				}
			}
			else if (gunVariation == 2 && gunReady && MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && shootReady)
			{
				Shoot();
			}
		}
		if (pierceShotCharge == 0f && celight.enabled)
		{
			celight.enabled = false;
		}
		else if (pierceShotCharge != 0f)
		{
			celight.enabled = true;
			celight.range = pierceShotCharge * 0.01f;
		}
		chargeEffect.transform.localScale = Vector3.one * pierceShotCharge * 0.02f;
		if (gunVariation == 0)
		{
			ceaud.volume = 0.25f + pierceShotCharge * 0.005f;
			ceaud.pitch = pierceShotCharge * 0.005f;
		}
		if (gunVariation == 1)
		{
			CheckCoinCharges();
		}
		else if (gunVariation == 0 && pierceCharge == 100f)
		{
			screenMR.material.color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[gunVariation];
		}
	}

	private void Shoot(int shotType = 1)
	{
		cc.StopShake();
		shootReady = false;
		shootCharge = 0f;
		if (altVersion)
		{
			MonoSingleton<WeaponCharges>.Instance.revaltpickupcharge = 2f;
		}
		switch (shotType)
		{
		case 1:
		{
			GameObject gameObject2 = Object.Instantiate(revolverBeam, cc.transform.position, cc.transform.rotation);
			if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
			{
				gameObject2.transform.LookAt(targeter.CurrentTarget.bounds.center);
			}
			RevolverBeam component2 = gameObject2.GetComponent<RevolverBeam>();
			component2.alternateStartPoint = gunBarrel.transform.position;
			component2.gunVariation = gunVariation;
			if (anim.GetCurrentAnimatorStateInfo(0).IsName("PickUp"))
			{
				component2.quickDraw = true;
			}
			currentGunShot = Random.Range(0, gunShots.Length);
			gunAud.clip = gunShots[currentGunShot];
			gunAud.volume = 0.55f;
			gunAud.pitch = Random.Range(0.9f, 1.1f);
			gunAud.Play();
			cam.fieldOfView += cc.defaultFov / 40f;
			break;
		}
		case 2:
		{
			GameObject gameObject = Object.Instantiate(revolverBeamSuper, cc.transform.position, cc.transform.rotation);
			if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
			{
				gameObject.transform.LookAt(targeter.CurrentTarget.bounds.center);
			}
			RevolverBeam component = gameObject.GetComponent<RevolverBeam>();
			component.alternateStartPoint = gunBarrel.transform.position;
			component.gunVariation = gunVariation;
			if (anim.GetCurrentAnimatorStateInfo(0).IsName("PickUp"))
			{
				component.quickDraw = true;
			}
			pierceReady = false;
			pierceCharge = 0f;
			screenAud.clip = chargingSound;
			screenAud.loop = true;
			if (altVersion)
			{
				screenAud.pitch = 0.5f;
			}
			else
			{
				screenAud.pitch = 1f;
			}
			screenAud.volume = 0.55f;
			screenAud.Play();
			if ((bool)superGunAud)
			{
				currentGunShot = Random.Range(0, superGunShots.Length);
				superGunAud.clip = superGunShots[currentGunShot];
				superGunAud.volume = 0.5f;
				superGunAud.pitch = Random.Range(0.9f, 1.1f);
				superGunAud.Play();
			}
			cam.fieldOfView += cc.defaultFov / 20f;
			break;
		}
		}
		cylinder.DoTurn();
		anim.SetFloat("RandomChance", Random.Range(0f, 1f));
		if (shotType == 1)
		{
			anim.SetTrigger("Shoot");
		}
		else
		{
			anim.SetTrigger("ChargeShoot");
		}
		gunReady = false;
	}

	private void ThrowCoin()
	{
		if (punch == null || !punch.gameObject.activeInHierarchy)
		{
			punch = MonoSingleton<FistControl>.Instance.currentPunch;
		}
		if ((bool)punch)
		{
			punch.CoinFlip();
		}
		GameObject obj = Object.Instantiate(coin, camObj.transform.position + camObj.transform.up * -0.5f, camObj.transform.rotation);
		obj.GetComponent<Coin>();
		Vector3 zero = Vector3.zero;
		obj.GetComponent<Rigidbody>().AddForce(camObj.transform.forward * 20f + Vector3.up * 15f + cc.player.GetComponent<Rigidbody>().velocity + zero, ForceMode.VelocityChange);
		pierceCharge = 0f;
		pierceReady = false;
	}

	private void ReadyToShoot()
	{
		shootReady = true;
	}

	public void Punch()
	{
		gunReady = false;
		anim.SetTrigger("ChargeShoot");
	}

	public void ReadyGun()
	{
		if (altVersion)
		{
			MonoSingleton<WeaponCharges>.Instance.revaltpickupcharge = 0f;
		}
		gunReady = true;
	}

	public void MaxCharge()
	{
		if (gunVariation == 0)
		{
			pierceCharge = 100f;
		}
		else if (gunVariation == 1)
		{
			CheckCoinCharges();
		}
	}

	private void DelayedShoot()
	{
		Shoot();
	}

	private void DelayedShoot2()
	{
		Shoot(2);
	}

	private void CheckCoinCharges()
	{
		if (coinPanelsCharged == null || coinPanelsCharged.Length == 0)
		{
			coinPanelsCharged = new bool[coinPanels.Length];
		}
		coinCharge = wc.rev1charge;
		for (int i = 0; i < coinPanels.Length; i++)
		{
			coinPanels[i].fillAmount = wc.rev1charge / 100f - (float)i;
			if (coinPanels[i].fillAmount < 1f)
			{
				coinPanels[i].color = Color.red;
				coinPanelsCharged[i] = false;
				continue;
			}
			if (coinPanels[i].color != MonoSingleton<ColorBlindSettings>.Instance.variationColors[gunVariation])
			{
				coinPanels[i].color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[gunVariation];
			}
			if (!coinPanelsCharged[i] && (!wid || wid.delay == 0f))
			{
				if (!screenAud)
				{
					screenAud = GetComponentInChildren<Canvas>().GetComponent<AudioSource>();
				}
				screenAud.pitch = 1f + (float)i / 2f;
				screenAud.Play();
				coinPanelsCharged[i] = true;
			}
		}
	}
}
