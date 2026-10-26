using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class PrefabReplacer : MonoBehaviour
{
	public static PrefabReplacer Instance;

	[SerializeField]
	private PlaceholderPrefabDatabase database;

	private Dictionary<string, PlaceholderPrefabTarget> placeholderPrefabs;

	private List<PlaceholderPrefab> delayedPrefabs = new List<PlaceholderPrefab>();

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
		}
		placeholderPrefabs = new Dictionary<string, PlaceholderPrefabTarget>();
		PlaceholderPrefabTarget[] array = database.placeholderPrefabs;
		foreach (PlaceholderPrefabTarget placeholderPrefabTarget in array)
		{
			placeholderPrefabs.Add(placeholderPrefabTarget.uniqueId, placeholderPrefabTarget);
		}
	}

	private void PerformSwap(PlaceholderPrefab placeholder)
	{
		Transform transform = placeholder.transform;
		GameObject gameObject = Object.Instantiate(placeholderPrefabs[placeholder.uniqueId].actualPrefab, transform.position, transform.rotation, transform.parent);
		IPlaceholdableComponent[] componentsInChildren = placeholder.GetComponentsInChildren<IPlaceholdableComponent>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].WillReplace(placeholder.gameObject, gameObject, isSelfBeingReplaced: true);
		}
		componentsInChildren = gameObject.GetComponentsInChildren<IPlaceholdableComponent>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].WillReplace(placeholder.gameObject, gameObject, isSelfBeingReplaced: false);
		}
		Object.Destroy(placeholder.gameObject);
	}

	public void ReplaceDelayed()
	{
		foreach (PlaceholderPrefab delayedPrefab in delayedPrefabs)
		{
			PerformSwap(delayedPrefab);
		}
		delayedPrefabs = new List<PlaceholderPrefab>();
	}

	public void ReplacePrefab(PlaceholderPrefab placeholder)
	{
		if (placeholderPrefabs.ContainsKey(placeholder.uniqueId))
		{
			Debug.Log("Key " + placeholder.uniqueId + " matched");
			if (placeholderPrefabs[placeholder.uniqueId].delayedSwap)
			{
				delayedPrefabs.Add(placeholder);
			}
			else
			{
				PerformSwap(placeholder);
			}
		}
		else
		{
			Debug.Log("Key " + placeholder.uniqueId + " not matched");
		}
	}
}
