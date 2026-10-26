using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class WeaponWheel : MonoSingleton<WeaponWheel>
{
	private List<UICircle> segments;

	private int segmentCount;

	private int selectedSegment;

	private Vector2 direction;

	private void Start()
	{
		base.gameObject.SetActive(value: false);
	}

	private new void OnEnable()
	{
		if (!(MonoSingleton<InputManager>.Instance == null))
		{
			Time.timeScale = 0.25f;
			selectedSegment = -1;
			direction = Vector2.zero;
			MonoSingleton<InputManager>.Instance.InputSource.Look.Action.Disable();
		}
	}

	private void OnDisable()
	{
		Time.timeScale = 1f;
		if ((bool)MonoSingleton<InputManager>.Instance && (bool)MonoSingleton<FistControl>.Instance)
		{
			MonoSingleton<InputManager>.Instance.InputSource.Look.Action.Enable();
			MonoSingleton<FistControl>.Instance.RefreshArm();
		}
	}

	private void Update()
	{
		if (!MonoSingleton<GunControl>.Instance || !MonoSingleton<GunControl>.Instance.activated || MonoSingleton<OptionsManager>.Instance.paused || MonoSingleton<NewMovement>.Instance.dead)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		direction = Vector2.ClampMagnitude(direction + MonoSingleton<InputManager>.Instance.InputSource.WheelLook.ReadValue<Vector2>(), 1f);
		float num = Mathf.Repeat(Mathf.Atan2(direction.x, direction.y) * 57.29578f + 90f, 360f);
		selectedSegment = ((direction.sqrMagnitude > 0f) ? ((int)(num / (360f / (float)segmentCount))) : selectedSegment);
		if (MonoSingleton<InputManager>.Instance.InputSource.NextWeapon.WasCanceledThisFrame || MonoSingleton<InputManager>.Instance.InputSource.PrevWeapon.WasCanceledThisFrame || MonoSingleton<InputManager>.Instance.InputSource.LastWeapon.WasCanceledThisFrame)
		{
			if (selectedSegment != -1)
			{
				MonoSingleton<GunControl>.Instance.SwitchWeapon(selectedSegment + 1);
			}
			base.gameObject.SetActive(value: false);
			return;
		}
		for (int i = 0; i < segments.Count; i++)
		{
			if (i == selectedSegment)
			{
				segments[i].color = Color.red;
			}
			else
			{
				segments[i].color = Color.black;
			}
		}
	}

	public void SetSegmentCount(int count)
	{
		if (count == segmentCount)
		{
			return;
		}
		segmentCount = count;
		if (segments == null)
		{
			segments = new List<UICircle>(segmentCount);
		}
		foreach (UICircle segment in segments)
		{
			Object.Destroy(segment);
		}
		segments.Clear();
		for (int i = 0; i < segmentCount; i++)
		{
			UICircle uICircle = new GameObject().AddComponent<UICircle>();
			uICircle.Arc = 1f / (float)segmentCount - 0.005f;
			uICircle.ArcRotation = (int)(360f * ((float)i / (float)segmentCount) + 1.8f);
			uICircle.Fill = false;
			uICircle.transform.SetParent(base.transform, worldPositionStays: false);
			uICircle.rectTransform.anchorMin = Vector2.zero;
			uICircle.rectTransform.anchorMax = Vector2.one;
			uICircle.rectTransform.anchoredPosition = Vector2.zero;
			uICircle.rectTransform.sizeDelta = Vector2.zero;
			Outline outline = uICircle.gameObject.AddComponent<Outline>();
			outline.effectDistance = new Vector2(2f, -2f);
			outline.effectColor = Color.white;
			segments.Add(uICircle);
		}
	}
}
