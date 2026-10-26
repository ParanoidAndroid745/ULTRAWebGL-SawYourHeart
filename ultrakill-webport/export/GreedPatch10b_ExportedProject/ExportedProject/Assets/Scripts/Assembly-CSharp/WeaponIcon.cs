using UnityEngine;

public class WeaponIcon : MonoBehaviour
{
	public Sprite weaponIcon;

	public Sprite glowIcon;

	public int variationColor;

	[SerializeField]
	private Renderer[] variationColoredRenderers;

	[SerializeField]
	private Material[] variationColoredMaterials;

	private void OnEnable()
	{
		variationColoredMaterials = new Material[variationColoredRenderers.Length];
		if (variationColoredRenderers.Length != 0)
		{
			for (int i = 0; i < variationColoredRenderers.Length; i++)
			{
				variationColoredMaterials[i] = new Material(variationColoredRenderers[i].material);
				if (variationColoredMaterials[i].HasProperty("_EmissiveColor"))
				{
					variationColoredMaterials[i].SetColor("_EmissiveColor", MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor]);
				}
				else
				{
					variationColoredMaterials[i].color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor];
				}
				variationColoredRenderers[i].material = variationColoredMaterials[i];
			}
		}
		UpdateIcon();
	}

	public void UpdateIcon()
	{
		if ((bool)MonoSingleton<WeaponHUD>.Instance)
		{
			MonoSingleton<WeaponHUD>.Instance.UpdateImage(weaponIcon, glowIcon, variationColor);
		}
		Material[] array = variationColoredMaterials;
		foreach (Material material in array)
		{
			if (material.HasProperty("_EmissiveColor"))
			{
				material.SetColor("_EmissiveColor", MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor]);
			}
			else
			{
				material.color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor];
			}
		}
	}
}
