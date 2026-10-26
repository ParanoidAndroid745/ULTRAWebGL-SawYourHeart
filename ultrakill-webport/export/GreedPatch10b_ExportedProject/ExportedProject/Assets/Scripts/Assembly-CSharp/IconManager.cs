using System.Linq;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class IconManager : MonoSingleton<IconManager>
{
	[SerializeField]
	private CheatAssetObject[] iconPacks;

	private int currentIconPack;

	private bool prefFetched;

	public int CurrentIconPackId
	{
		get
		{
			if (!prefFetched)
			{
				return FetchSavedPref();
			}
			return currentIconPack;
		}
	}

	public CheatAssetObject CurrentIcons => iconPacks[CurrentIconPackId];

	private int FetchSavedPref()
	{
		prefFetched = true;
		currentIconPack = MonoSingleton<PrefsManager>.Instance.GetInt("iconPack");
		return currentIconPack;
	}

	public string[] AvailableIconPacks()
	{
		return iconPacks.Select((CheatAssetObject ip) => ip.name).ToArray();
	}

	public void SetIconPack(int pack)
	{
		currentIconPack = pack;
		MonoSingleton<PrefsManager>.Instance.SetInt("iconPack", pack);
	}

	public void Reload()
	{
		MonoSingleton<CheatsManager>.Instance.RebuildIcons();
		MonoSingleton<CheatsManager>.Instance.RebuildMenu();
		if ((bool)MonoSingleton<SpawnMenu>.Instance)
		{
			MonoSingleton<SpawnMenu>.Instance.RebuildIcons();
			MonoSingleton<SpawnMenu>.Instance.RebuildMenu();
		}
	}
}
