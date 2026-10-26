using UnityEngine;
using UnityEngine.UI;

public class ShopZone : MonoBehaviour
{
	private GunControl gc;

	private bool inUse;

	private bool playerInZone;

	private Canvas shopCanvas;

	private FistControl pun;

	private AudioSource music;

	public Text moneyText;

	public bool firstVariationBuy;

	public GameObject variationSwitchTip;

	private ShopMother shom;

	private ShopCategory[] shopcats;

	public bool muteMusic;

	private float originalMusicVolume;

	private float musicTarget = 1f;

	private bool fading;

	public bool forcedOff;

	private void Start()
	{
		shopCanvas = GetComponentInChildren<Canvas>(includeInactive: true);
		if (shopCanvas != null)
		{
			shopCanvas.gameObject.SetActive(value: false);
		}
		music = base.transform.Find("Music").GetComponent<AudioSource>();
		if (moneyText != null)
		{
			moneyText.text = DivideMoney(GameProgressSaver.GetMoney()) + "<color=orange>P</color>";
		}
		originalMusicVolume = MonoSingleton<AudioMixerController>.Instance.optionsMusicVolume;
		musicTarget = originalMusicVolume;
		MonoSingleton<CheckPointsController>.Instance.AddShop(this);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			playerInZone = true;
			TurnOn();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			playerInZone = false;
			TurnOff();
		}
	}

	private void Update()
	{
		if (music != null)
		{
			if (inUse)
			{
				if (music.pitch < 1f)
				{
					music.pitch = Mathf.MoveTowards(music.pitch, 1f, Time.deltaTime);
				}
			}
			else if (music.pitch > 0f)
			{
				music.pitch = Mathf.MoveTowards(music.pitch, 0f, Time.deltaTime);
			}
		}
		if (muteMusic && fading && MonoSingleton<AudioMixerController>.Instance.musicVolume != musicTarget)
		{
			MonoSingleton<AudioMixerController>.Instance.SetMusicVolume(Mathf.MoveTowards(MonoSingleton<AudioMixerController>.Instance.musicVolume, Mathf.Min(musicTarget, MonoSingleton<AudioMixerController>.Instance.optionsMusicVolume), originalMusicVolume * Time.deltaTime));
			if (MonoSingleton<AudioMixerController>.Instance.musicVolume == musicTarget)
			{
				fading = false;
			}
		}
	}

	public void TurnOn()
	{
		if (!inUse && !forcedOff)
		{
			if (gc == null)
			{
				gc = MonoSingleton<GunControl>.Instance;
			}
			gc.stayUnarmed = true;
			gc.NoWeapon();
			if (pun == null)
			{
				pun = MonoSingleton<FistControl>.Instance;
			}
			pun.ShopMode();
			inUse = true;
			if (shopCanvas != null)
			{
				shopCanvas.gameObject.SetActive(value: true);
			}
			if (shopcats == null)
			{
				shopcats = GetComponentsInChildren<ShopCategory>();
			}
			ShopCategory[] array = shopcats;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CheckGear();
			}
			ControllerPointer.raycaster = shopCanvas.GetComponent<GraphicRaycaster>();
			if (muteMusic)
			{
				fading = true;
				musicTarget = 0f;
			}
			if ((bool)moneyText)
			{
				UpdateMoney();
			}
		}
	}

	public void TurnOff()
	{
		if (inUse)
		{
			if (shopCanvas != null)
			{
				shopCanvas.gameObject.SetActive(value: false);
			}
			if (gc == null)
			{
				gc = MonoSingleton<GunControl>.Instance;
			}
			gc.stayUnarmed = false;
			gc.YesWeapon();
			inUse = false;
			if (pun == null)
			{
				pun = MonoSingleton<FistControl>.Instance;
			}
			pun.StopShop();
			shopCanvas.gameObject.SetActive(value: false);
			if (firstVariationBuy)
			{
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Cycle through <color=orange>EQUIPPED</color> variations with '<color=orange>", "ChangeVariation", "</color>'.");
				firstVariationBuy = false;
				PlayerPrefs.SetInt("FirVar", 0);
			}
			if (muteMusic)
			{
				fading = true;
				musicTarget = originalMusicVolume;
			}
		}
	}

	public void ForceOff()
	{
		TurnOff();
		forcedOff = true;
	}

	public void StopForceOff()
	{
		forcedOff = false;
		if (playerInZone)
		{
			TurnOn();
		}
	}

	public void UpdateMoney()
	{
		moneyText.text = DivideMoney(GameProgressSaver.GetMoney()) + "<color=orange>P</color>";
		VariationInfo[] componentsInChildren = GetComponentsInChildren<VariationInfo>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateMoney();
		}
	}

	public string DivideMoney(int dosh)
	{
		int num = dosh;
		int num2 = 0;
		int num3 = 0;
		while (num >= 1000)
		{
			num2++;
			num -= 1000;
		}
		while (num2 >= 1000)
		{
			num3++;
			num2 -= 1000;
		}
		if (num3 > 0)
		{
			string text = num3 + ",";
			text = ((num2 < 10) ? (text + "00" + num2 + ",") : ((num2 >= 100) ? (text + num2 + ",") : (text + "0" + num2 + ",")));
			if (num < 10)
			{
				return text + "00" + num;
			}
			if (num < 100)
			{
				return text + "0" + num;
			}
			return text + num;
		}
		if (num2 > 0)
		{
			string text = num2 + ",";
			if (num < 10)
			{
				return text + "00" + num;
			}
			if (num < 100)
			{
				return text + "0" + num;
			}
			return text + num;
		}
		return string.Concat(num);
	}
}
