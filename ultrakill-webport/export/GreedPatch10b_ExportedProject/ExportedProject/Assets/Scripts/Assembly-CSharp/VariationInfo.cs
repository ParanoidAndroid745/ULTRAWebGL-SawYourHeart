using UnityEngine;
using UnityEngine.UI;

public class VariationInfo : MonoBehaviour
{
	public GameObject varPage;

	private int money;

	public Text moneyText;

	public int cost;

	public Text costText;

	public ShopButton buyButton;

	private Text buttonText;

	public GameObject buySound;

	public Button equipButton;

	private Image equipImage;

	public Sprite[] equipSprites;

	private int equipStatus;

	public bool alreadyOwned;

	public string weaponName;

	private GunSetter gs;

	private FistControl fc;

	private GameObject player;

	public GameObject orderButtons;

	private void Start()
	{
		player = MonoSingleton<NewMovement>.Instance.gameObject;
		buttonText = buyButton.GetComponentInChildren<Text>();
		buyButton.variationInfo = this;
		equipImage = equipButton.transform.GetChild(0).GetComponent<Image>();
		if (GameProgressSaver.CheckGear(weaponName) > 0)
		{
			alreadyOwned = true;
		}
		UpdateMoney();
	}

	private void OnEnable()
	{
		UpdateMoney();
	}

	public void UpdateMoney()
	{
		money = GameProgressSaver.GetMoney();
		moneyText.text = DivideMoney(money) + "<color=orange>P</color>";
		if (!alreadyOwned && cost < 0 && GameProgressSaver.CheckGear(weaponName) > 0)
		{
			alreadyOwned = true;
		}
		if (!alreadyOwned)
		{
			if (cost < 0)
			{
				costText.text = "<color=red>UNAVAILABLE</color>";
				if (buttonText == null)
				{
					buttonText = buyButton.GetComponentInChildren<Text>();
				}
				buttonText.text = costText.text;
				buyButton.failure = true;
				buyButton.GetComponent<Button>().interactable = false;
				buyButton.GetComponent<Image>().color = Color.red;
				if (TryGetComponent<ShopButton>(out var component))
				{
					component.failure = true;
				}
			}
			else if (cost > money)
			{
				costText.text = "<color=red>" + DivideMoney(cost) + "P</color>";
				if (buttonText == null)
				{
					buttonText = buyButton.GetComponentInChildren<Text>();
				}
				buttonText.text = costText.text;
				buyButton.failure = true;
				buyButton.GetComponent<Button>().interactable = false;
				buyButton.GetComponent<Image>().color = Color.red;
			}
			else
			{
				costText.text = "<color=white>" + DivideMoney(cost) + "</color><color=orange>P</color>";
				if (buttonText == null)
				{
					buttonText = buyButton.GetComponentInChildren<Text>();
				}
				buttonText.text = costText.text;
				buyButton.failure = false;
				buyButton.GetComponent<Button>().interactable = true;
				buyButton.GetComponent<Image>().color = Color.white;
			}
			equipButton.gameObject.SetActive(value: false);
			return;
		}
		costText.text = "ALREADY OWNED";
		if (buttonText == null)
		{
			buttonText = buyButton.GetComponentInChildren<Text>();
		}
		buttonText.text = costText.text;
		buyButton.failure = true;
		buyButton.GetComponent<Button>().interactable = false;
		buyButton.GetComponent<Image>().color = Color.white;
		equipButton.gameObject.SetActive(value: true);
		equipButton.interactable = true;
		if (equipImage == null)
		{
			equipImage = equipButton.transform.GetChild(0).GetComponent<Image>();
		}
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("weapon." + weaponName, 1);
		if (num == 2 && GameProgressSaver.CheckGear(weaponName.Substring(0, weaponName.Length - 1) + "alt") > 0)
		{
			equipStatus = 2;
		}
		else if (num > 0)
		{
			equipStatus = 1;
		}
		else
		{
			equipStatus = 0;
		}
		if ((bool)orderButtons)
		{
			if (equipStatus != 0)
			{
				orderButtons.SetActive(value: true);
			}
			else
			{
				orderButtons.SetActive(value: false);
			}
		}
		equipImage.sprite = equipSprites[equipStatus];
		if (cost < 0 && TryGetComponent<ShopButton>(out var component2))
		{
			component2.failure = false;
		}
	}

	public void WeaponBought()
	{
		alreadyOwned = true;
		Object.Instantiate(buySound);
		GameProgressSaver.AddMoney(cost * -1);
		GameProgressSaver.AddGear(weaponName);
		MonoSingleton<PrefsManager>.Instance.SetInt("weapon." + weaponName, 1);
		base.gameObject.GetComponentInParent<ShopZone>().UpdateMoney();
		if (PlayerPrefs.GetInt("FirVar", 1) == 1)
		{
			GetComponentInParent<ShopZone>().firstVariationBuy = true;
		}
		if (gs == null)
		{
			gs = player.GetComponentInChildren<GunSetter>();
		}
		gs.ResetWeapons();
		gs.ForceWeapon(weaponName);
		gs.gunc.NoWeapon();
		if (fc == null)
		{
			fc = player.GetComponentInChildren<FistControl>();
		}
		fc.ResetFists();
	}

	public void ChangeEquipment(int value)
	{
		int num = equipStatus;
		num = ((value <= 0) ? (num - 1) : (num + 1));
		int num2 = num;
		if (num < 0)
		{
			num2 = ((GameProgressSaver.CheckGear(weaponName.Substring(0, weaponName.Length - 1) + "alt") <= 0) ? 1 : 2);
		}
		else if (num == 2)
		{
			num2 = ((GameProgressSaver.CheckGear(weaponName.Substring(0, weaponName.Length - 1) + "alt") > 0) ? 2 : 0);
		}
		else if (num > 2)
		{
			num2 = 0;
		}
		equipStatus = num2;
		equipImage.sprite = equipSprites[num2];
		MonoSingleton<PrefsManager>.Instance.SetInt("weapon." + weaponName, num2);
		if ((bool)orderButtons)
		{
			if (equipStatus == 0)
			{
				orderButtons.SetActive(value: false);
			}
			else
			{
				orderButtons.SetActive(value: true);
			}
		}
		if (gs == null)
		{
			gs = player.GetComponentInChildren<GunSetter>();
		}
		gs.ResetWeapons();
		if (fc == null)
		{
			fc = player.GetComponentInChildren<FistControl>();
		}
		fc.ResetFists();
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
