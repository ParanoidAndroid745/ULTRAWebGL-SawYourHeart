using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class BossBarManager : MonoSingleton<BossBarManager>
{
	[Serializable]
	public class SliderLayer
	{
		public Color color;

		public Color afterImageColor;
	}

	[Serializable]
	public class HealthLayer
	{
		public float health;
	}

	[SerializeField]
	private BossHealthBarTemplate template;

	[SerializeField]
	private SliderLayer[] layers;

	public void CreateBossBar(string bossName, HealthLayer[] healthLayers, ref BossHealthBarTemplate createdBossBar, ref Slider[] hpSliders, ref Slider[] hpAfterImages, ref GameObject bossBar)
	{
		Debug.Log("Creating boss bar for " + bossName);
		List<Slider> list = new List<Slider>();
		List<Slider> list2 = new List<Slider>();
		createdBossBar = UnityEngine.Object.Instantiate(template, template.transform.parent, worldPositionStays: true);
		bossBar = createdBossBar.gameObject;
		bossBar.SetActive(value: true);
		createdBossBar.bossNameText.text = bossName;
		float num = 0f;
		for (int i = 0; i < healthLayers.Length; i++)
		{
			BossHealthSliderTemplate bossHealthSliderTemplate = UnityEngine.Object.Instantiate(createdBossBar.sliderTemplate, createdBossBar.sliderTemplate.transform.parent);
			list2.Add(bossHealthSliderTemplate.slider);
			bossHealthSliderTemplate.slider.minValue = num;
			bossHealthSliderTemplate.slider.maxValue = num + healthLayers[i].health;
			bossHealthSliderTemplate.gameObject.SetActive(value: true);
			bossHealthSliderTemplate.background.SetActive(i == 0);
			bossHealthSliderTemplate.fill.color = layers[i].afterImageColor;
			BossHealthSliderTemplate bossHealthSliderTemplate2 = UnityEngine.Object.Instantiate(createdBossBar.sliderTemplate, createdBossBar.sliderTemplate.transform.parent);
			list.Add(bossHealthSliderTemplate2.slider);
			bossHealthSliderTemplate2.slider.minValue = num;
			bossHealthSliderTemplate2.slider.maxValue = num + healthLayers[i].health;
			bossHealthSliderTemplate2.gameObject.SetActive(value: true);
			bossHealthSliderTemplate2.background.SetActive(value: false);
			bossHealthSliderTemplate2.fill.color = layers[i].color;
			num += healthLayers[i].health;
		}
		hpSliders = list.ToArray();
		hpAfterImages = list2.ToArray();
		template.sliderTemplate.gameObject.SetActive(value: false);
		template.gameObject.SetActive(value: false);
		bossBar.SetActive(value: false);
		StartCoroutine(FitAsync(template.transform.parent.GetComponent<RectTransform>()));
	}

	private IEnumerator FitAsync(RectTransform layoutRoot)
	{
		yield return new WaitForFixedUpdate();
		LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
	}

	public void CreateThinBar(BossHealthSliderTemplate thinTarget, ref Slider slider, ref GameObject thinBar)
	{
		BossHealthSliderTemplate bossHealthSliderTemplate = UnityEngine.Object.Instantiate(thinTarget, thinTarget.transform.parent);
		thinBar = bossHealthSliderTemplate.gameObject;
		slider = bossHealthSliderTemplate.slider;
	}
}
