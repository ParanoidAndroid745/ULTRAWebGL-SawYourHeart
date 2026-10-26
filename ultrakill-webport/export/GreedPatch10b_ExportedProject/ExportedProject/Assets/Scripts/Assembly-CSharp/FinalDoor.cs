using UnityEngine;

public class FinalDoor : MonoBehaviour
{
	public Door[] doors;

	public GameObject doorLight;

	public bool startOpen;

	public Material[] offMaterials;

	public Material[] onMaterials;

	public bool levelNameOnOpen;

	private MeshRenderer[] allRenderers;

	private bool opened;

	[HideInInspector]
	public bool aboutToOpen;

	private void Start()
	{
		if (doorLight == null)
		{
			doorLight = GetComponentInChildren<Light>().gameObject;
		}
		if (!aboutToOpen)
		{
			doorLight.SetActive(value: false);
		}
		if (startOpen || (aboutToOpen && !opened))
		{
			Open();
		}
		allRenderers = GetComponentsInChildren<MeshRenderer>();
	}

	public void Open()
	{
		aboutToOpen = true;
		MonoSingleton<MusicManager>.Instance.ArenaMusicEnd();
		Invoke("OpenDoors", 1f);
		GetComponent<AudioSource>().Play();
		doorLight.SetActive(value: true);
		if (onMaterials.Length == 0)
		{
			return;
		}
		if (allRenderers == null || allRenderers.Length == 0)
		{
			allRenderers = GetComponentsInChildren<MeshRenderer>();
		}
		MeshRenderer[] array = allRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			int material = GetMaterial(meshRenderer);
			if (material >= 0)
			{
				meshRenderer.sharedMaterial = onMaterials[material];
			}
		}
	}

	private int GetMaterial(MeshRenderer mr)
	{
		for (int i = 0; i < offMaterials.Length; i++)
		{
			if (mr.sharedMaterial == offMaterials[i])
			{
				return i;
			}
		}
		return -1;
	}

	public void OpenDoors()
	{
		if (!opened)
		{
			opened = true;
			aboutToOpen = false;
			Door[] array = doors;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Open(enemy: false, skull: true);
			}
			if (levelNameOnOpen)
			{
				Invoke("LevelNameGo", 1f);
			}
			MonoSingleton<PlayerTracker>.Instance.CheckPlayerType();
		}
	}

	private void LevelNameGo()
	{
		MonoSingleton<LevelNamePopup>.Instance.NameAppear();
	}
}
