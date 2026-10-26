using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SpawnMenu : MonoSingleton<SpawnMenu>
{
	[SerializeField]
	private SpawnMenuSectionReference sectionReference;

	[SerializeField]
	private SpawnableObjectsDatabase objects;

	[HideInInspector]
	public DebugArm arm;

	[SerializeField]
	private Sprite lockedIcon;

	private Dictionary<string, Sprite> spriteIcons;

	private new void Awake()
	{
		RebuildIcons();
		RebuildMenu();
	}

	public void RebuildIcons()
	{
		spriteIcons = new Dictionary<string, Sprite>();
		CheatAssetObject.KeyIcon[] sandboxToolIcons = MonoSingleton<IconManager>.Instance.CurrentIcons.sandboxToolIcons;
		for (int i = 0; i < sandboxToolIcons.Length; i++)
		{
			CheatAssetObject.KeyIcon keyIcon = sandboxToolIcons[i];
			spriteIcons.Add(keyIcon.key, keyIcon.sprite);
		}
	}

	public void ResetMenu()
	{
		for (int i = 1; i < sectionReference.transform.parent.childCount; i++)
		{
			Object.Destroy(sectionReference.transform.parent.GetChild(i).gameObject);
		}
	}

	public void RebuildMenu()
	{
		ResetMenu();
		if (MapInfoBase.InstanceAnyType.sandboxTools)
		{
			CreateButtons(objects.sandboxTools, "SANDBOX TOOLS :^)");
			CreateButtons(objects.sandboxObjects, "SANDBOX");
		}
		CreateButtons(objects.enemies, "ENEMIES");
		CreateButtons(objects.objects, "ITEMS");
		sectionReference.gameObject.SetActive(value: false);
	}

	private Sprite ResolveSprite(SpawnableObject target)
	{
		if (!string.IsNullOrEmpty(target.iconKey) && spriteIcons.ContainsKey(target.iconKey))
		{
			return spriteIcons[target.iconKey];
		}
		if (target.gridIcon != null)
		{
			return target.gridIcon;
		}
		return MonoSingleton<IconManager>.Instance.CurrentIcons.genericSandboxToolIcon;
	}

	private void CreateButtons(SpawnableObject[] list, string sectionName)
	{
		SpawnMenuSectionReference spawnMenuSectionReference = Object.Instantiate(sectionReference, sectionReference.transform.parent);
		spawnMenuSectionReference.gameObject.SetActive(value: true);
		spawnMenuSectionReference.sectionName.text = sectionName;
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i] == null)
			{
				continue;
			}
			int num;
			if (list == objects.enemies)
			{
				num = ((MonoSingleton<BestiaryData>.Instance.GetEnemy(list[i].enemyType) >= 1) ? 1 : 0);
				if (num == 0)
				{
					spawnMenuSectionReference.buttonBackgroundImage.color = Color.gray;
					spawnMenuSectionReference.buttonForegroundImage.sprite = lockedIcon;
					goto IL_00cc;
				}
			}
			else
			{
				num = 1;
			}
			spawnMenuSectionReference.buttonBackgroundImage.color = list[i].backgroundColor;
			spawnMenuSectionReference.buttonForegroundImage.sprite = ResolveSprite(list[i]);
			goto IL_00cc;
			IL_00cc:
			Button button = Object.Instantiate(spawnMenuSectionReference.button, spawnMenuSectionReference.grid.transform, worldPositionStays: false);
			SpawnableObject spawnableObj = list[i];
			if (num != 0)
			{
				button.onClick.AddListener(() =>
				{
					SelectObject(spawnableObj);
				});
			}
		}
		spawnMenuSectionReference.button.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		MonoSingleton<OptionsManager>.Instance.UnFreeze();
	}

	private void SelectObject(SpawnableObject obj)
	{
		arm.SelectObject(obj);
	}
}
