using UnityEngine;

public class MenuLoaderRelay : MonoBehaviour
{
	[SerializeField]
	private string act;

	private void Awake()
	{
		MenuActSelect component = GetComponent<MenuActSelect>();
		if ((bool)component && !string.IsNullOrEmpty(act))
		{
			component.forceOff = !MapLoader.CheckForAct(act);
		}
	}

	public void LoadAct()
	{
		TryToLoadCommon();
		MonoSingleton<MapLoader>.Instance.LoadAct(act, (AssetBundle bundle) =>
		{
		}, () =>
		{
		});
	}

	public void UnloadAct()
	{
		MonoSingleton<MapLoader>.Instance.DropLoadedBundle();
	}

	public void TryToLoadCommon()
	{
		MonoSingleton<MapLoader>.Instance.EnsureCommonIsLoaded();
	}
}
