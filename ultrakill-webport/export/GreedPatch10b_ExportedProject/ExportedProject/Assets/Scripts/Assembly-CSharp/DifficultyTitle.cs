using UnityEngine;
using UnityEngine.UI;

public class DifficultyTitle : MonoBehaviour
{
	public bool lines;

	private Text txt;

	private void Start()
	{
		Check();
	}

	private void OnEnable()
	{
		Check();
	}

	private void Check()
	{
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		if (txt == null)
		{
			txt = GetComponent<Text>();
		}
		txt.text = "";
		if (lines)
		{
			txt.text += "-- ";
		}
		switch (num)
		{
		case 0:
			txt.text += "HARMLESS";
			break;
		case 1:
			txt.text += "LENIENT";
			break;
		case 2:
			txt.text += "STANDARD";
			break;
		case 3:
			txt.text += "VIOLENT";
			break;
		case 4:
			txt.text += "BRUTAL";
			break;
		case 5:
			txt.text += "ULTRAKILL MUST DIE";
			break;
		}
		if (lines)
		{
			txt.text += " --";
		}
	}
}
