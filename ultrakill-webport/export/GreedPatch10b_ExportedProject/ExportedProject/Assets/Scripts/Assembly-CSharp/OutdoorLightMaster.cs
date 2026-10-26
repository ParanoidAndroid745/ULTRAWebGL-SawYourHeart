using System.Collections.Generic;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class OutdoorLightMaster : MonoSingleton<OutdoorLightMaster>
{
	public bool inverse;

	private List<Light> outdoorLights = new List<Light>();

	public Light[] extraLights;

	public GameObject[] activateWhenOutside;

	[HideInInspector]
	public LayerMask normalMask;

	[HideInInspector]
	public LayerMask playerMask;

	private int requests;

	public bool dontRotateSkybox;

	private float skyboxRotation;

	private bool firstDoorOpened;

	public bool waitForFirstDoorOpen;

	private Material skyboxMaterial;

	private Material tempSkybox;

	[HideInInspector]
	public List<Collider> outdoorsZones = new List<Collider>();

	private void Start()
	{
		Light[] componentsInChildren = GetComponentsInChildren<Light>(includeInactive: true);
		outdoorLights.AddRange(componentsInChildren);
		if (extraLights != null)
		{
			outdoorLights.AddRange(extraLights);
		}
		if (outdoorLights.Count != 0)
		{
			normalMask = 16777216;
			normalMask = (int)normalMask | 0x2000000;
			LayerMask layerMask = 8192;
			playerMask = (int)normalMask | (int)layerMask;
		}
		foreach (Light outdoorLight in outdoorLights)
		{
			if (inverse && (!waitForFirstDoorOpen || firstDoorOpened))
			{
				outdoorLight.cullingMask = playerMask;
			}
			else
			{
				outdoorLight.cullingMask = normalMask;
			}
		}
		if (activateWhenOutside != null)
		{
			GameObject[] array = activateWhenOutside;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(inverse && (!waitForFirstDoorOpen || firstDoorOpened));
			}
		}
	}

	private void Update()
	{
		if (dontRotateSkybox || !RenderSettings.skybox)
		{
			return;
		}
		if (!tempSkybox)
		{
			UpdateSkyboxMaterial();
			return;
		}
		skyboxRotation += Time.deltaTime;
		if (skyboxRotation >= 360f)
		{
			skyboxRotation -= 360f;
		}
		RenderSettings.skybox.SetFloat("_Rotation", skyboxRotation);
	}

	public void AddRequest()
	{
		requests++;
		if (requests != 1)
		{
			return;
		}
		GameObject[] array;
		if (inverse)
		{
			foreach (Light outdoorLight in outdoorLights)
			{
				outdoorLight.cullingMask = normalMask;
			}
			array = activateWhenOutside;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			return;
		}
		foreach (Light outdoorLight2 in outdoorLights)
		{
			outdoorLight2.cullingMask = playerMask;
		}
		array = activateWhenOutside;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
	}

	public void RemoveRequest()
	{
		requests--;
		if (requests != 0)
		{
			return;
		}
		GameObject[] array;
		if (inverse)
		{
			foreach (Light outdoorLight in outdoorLights)
			{
				outdoorLight.cullingMask = playerMask;
			}
			array = activateWhenOutside;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			return;
		}
		foreach (Light outdoorLight2 in outdoorLights)
		{
			outdoorLight2.cullingMask = normalMask;
		}
		array = activateWhenOutside;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}

	public void FirstDoorOpen()
	{
		if (firstDoorOpened)
		{
			return;
		}
		firstDoorOpened = true;
		if (!inverse || !waitForFirstDoorOpen)
		{
			return;
		}
		foreach (Light outdoorLight in outdoorLights)
		{
			outdoorLight.cullingMask = playerMask;
		}
		if (activateWhenOutside != null)
		{
			GameObject[] array = activateWhenOutside;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
	}

	public void UpdateSkyboxMaterial()
	{
		if (!skyboxMaterial)
		{
			skyboxMaterial = RenderSettings.skybox;
		}
		tempSkybox = new Material(skyboxMaterial);
		RenderSettings.skybox = tempSkybox;
	}
}
