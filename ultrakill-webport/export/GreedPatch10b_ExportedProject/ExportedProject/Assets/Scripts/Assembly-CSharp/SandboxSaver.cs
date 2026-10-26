using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SandboxSaver : MonoSingleton<SandboxSaver>
{
	public const string SaveExtension = ".pitr";

	[SerializeField]
	private SpawnableObjectsDatabase objects;

	private Dictionary<string, SpawnableObject> registeredObject;

	public static string SavePath => Path.Combine(GameProgressSaver.BaseSavePath, "Sandbox");

#if UNITY_WEBGL
	// En WebGL no hay disco: los saves de sandbox viven en memoria.
	private static readonly Dictionary<string, string> webglSaves = new Dictionary<string, string>();
#endif

	private static void SetupDirs()
	{
#if !UNITY_WEBGL
		if (!Directory.Exists(SavePath))
		{
			Directory.CreateDirectory(SavePath);
		}
#endif
	}

	public string[] ListSaves()
	{
		SetupDirs();
#if UNITY_WEBGL
		return webglSaves.Keys.ToArray();
#else
		return (from f in new DirectoryInfo(SavePath).GetFileSystemInfos()
			orderby f.LastWriteTime descending
			select Path.GetFileNameWithoutExtension(f.Name)).ToArray();
#endif
	}

	public void QuickSave()
	{
		Save($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}");
	}

	public void QuickLoad()
	{
		string[] array = ListSaves();
		if (array.Length != 0)
		{
			Load(array[0]);
		}
	}

	public void Delete(string name)
	{
		SetupDirs();
		string path = Path.Combine(SavePath, name + ".pitr");
#if UNITY_WEBGL
		webglSaves.Remove(name);
#else
		if (File.Exists(path))
		{
			File.Delete(path);
		}
#endif
	}

	public void Save(string name)
	{
		SetupDirs();
		CreateSaveAndWrite(name);
	}

	public void Load(string name)
	{
		SetupDirs();
		Clear();
		RebuildObjectList();
#if UNITY_WEBGL
		string json;
		if (!webglSaves.TryGetValue(name, out json))
		{
			Debug.LogError("Sandbox save not found: " + name);
			return;
		}
		SandboxSaveData sandboxSaveData = JsonUtility.FromJson<SandboxSaveData>(json);
#else
		SandboxSaveData sandboxSaveData = JsonUtility.FromJson<SandboxSaveData>(File.ReadAllText(Path.Combine(SavePath, name + ".pitr")));
#endif
		Debug.Log($"Loaded {sandboxSaveData.Blocks.Length} blocks\nLoaded {sandboxSaveData.Props.Length} props");
		SavedProp[] props = sandboxSaveData.Props;
		foreach (SavedProp prop in props)
		{
			RecreateProp(prop);
		}
		SavedBlock[] blocks = sandboxSaveData.Blocks;
		foreach (SavedBlock block in blocks)
		{
			RecreateBlock(block);
		}
		MonoSingleton<SandboxNavmesh>.Instance.Rebake();
		SavedGeneric[] enemies = sandboxSaveData.Enemies;
		foreach (SavedGeneric genericObject in enemies)
		{
			RecreateEnemy(genericObject);
		}
	}

	private void RecreateEnemy(SavedGeneric genericObject)
	{
		if (!registeredObject.ContainsKey(genericObject.ObjectIdentifier))
		{
			Debug.LogError(genericObject.ObjectIdentifier + " missing from registered objects");
			return;
		}
		GameObject obj = UnityEngine.Object.Instantiate(registeredObject[genericObject.ObjectIdentifier].gameObject);
		obj.transform.position = genericObject.Position;
		obj.transform.localScale = genericObject.Scale;
		obj.AddComponent<SandboxEnemy>().spawnSource = registeredObject[genericObject.ObjectIdentifier];
	}

	private void RecreateProp(SavedProp prop)
	{
		if (!registeredObject.ContainsKey(prop.ObjectIdentifier))
		{
			Debug.LogError(prop.ObjectIdentifier + " missing from registered objects");
			return;
		}
		GameObject obj = UnityEngine.Object.Instantiate(registeredObject[prop.ObjectIdentifier].gameObject);
		obj.transform.position = prop.Position;
		obj.transform.rotation = prop.Rotation;
		obj.transform.localScale = prop.Scale;
		obj.GetComponent<SandboxProp>().sourceObject = registeredObject[prop.ObjectIdentifier];
		if (obj.TryGetComponent<Rigidbody>(out var component))
		{
			component.isKinematic = prop.Kinematic;
		}
	}

	private void RecreateBlock(SavedBlock block)
	{
		if (!registeredObject.ContainsKey(block.ObjectIdentifier))
		{
			Debug.LogError(block.ObjectIdentifier + " missing from registered objects");
			return;
		}
		GameObject obj = SandboxUtils.CreateFinalBlock(registeredObject[block.ObjectIdentifier], block.Position, block.BlockSize);
		obj.transform.rotation = block.Rotation;
		obj.GetComponent<Rigidbody>().isKinematic = block.Kinematic;
	}

	public void RebuildObjectList()
	{
		if (registeredObject == null)
		{
			registeredObject = new Dictionary<string, SpawnableObject>();
		}
		registeredObject.Clear();
		RegisterObjects(objects.objects);
		RegisterObjects(objects.enemies);
		RegisterObjects(objects.sandboxTools);
		RegisterObjects(objects.sandboxObjects);
	}

	private void RegisterObjects(SpawnableObject[] objs)
	{
		foreach (SpawnableObject spawnableObject in objs)
		{
			if (!string.IsNullOrEmpty(spawnableObject.identifier))
			{
				if (registeredObject.ContainsKey(spawnableObject.identifier))
				{
					Debug.LogError("Duplicate Object Identifier, pls fix. " + spawnableObject.identifier);
				}
				else
				{
					registeredObject.Add(spawnableObject.identifier, spawnableObject);
				}
			}
		}
	}

	public static void Clear()
	{
		SandboxProp[] array = UnityEngine.Object.FindObjectsOfType<SandboxProp>();
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.Destroy(array[i].gameObject);
		}
		BrushBlock[] array2 = UnityEngine.Object.FindObjectsOfType<BrushBlock>();
		for (int i = 0; i < array2.Length; i++)
		{
			UnityEngine.Object.Destroy(array2[i].gameObject);
		}
		SandboxEnemy[] array3 = UnityEngine.Object.FindObjectsOfType<SandboxEnemy>();
		for (int i = 0; i < array3.Length; i++)
		{
			UnityEngine.Object.Destroy(array3[i].gameObject);
		}
	}

	private static void CreateSaveAndWrite(string name)
	{
		SandboxProp[] array = UnityEngine.Object.FindObjectsOfType<SandboxProp>();
		Debug.Log($"{array.Length} props found");
		BrushBlock[] array2 = UnityEngine.Object.FindObjectsOfType<BrushBlock>();
		Debug.Log($"{array2.Length} procedural blocks found");
		SandboxEnemy[] array3 = UnityEngine.Object.FindObjectsOfType<SandboxEnemy>();
		Debug.Log($"{array2.Length} sandbox enemies found");
		List<SavedBlock> list = new List<SavedBlock>();
		BrushBlock[] array4 = array2;
		foreach (BrushBlock brushBlock in array4)
		{
			Debug.Log($"Position: {brushBlock.transform.position}\nRotation: {brushBlock.transform.rotation}\nSize: {brushBlock.DataSize}\nType: {brushBlock.Type}");
			list.Add(brushBlock.Save());
		}
		List<SavedProp> list2 = new List<SavedProp>();
		SandboxProp[] array5 = array;
		foreach (SandboxProp sandboxProp in array5)
		{
			if (!sandboxProp.GetComponent<BrushBlock>())
			{
				Debug.Log($"Position: {sandboxProp.transform.position}\nRotation: {sandboxProp.transform.rotation}");
				list2.Add(sandboxProp.Save());
			}
		}
		List<SavedGeneric> list3 = new List<SavedGeneric>();
		SandboxEnemy[] array6 = array3;
		for (int i = 0; i < array6.Length; i++)
		{
			SavedGeneric savedGeneric = array6[i].Save();
			if (savedGeneric != null)
			{
				list3.Add(savedGeneric);
			}
		}
		string contents = JsonUtility.ToJson(new SandboxSaveData
		{
			MapName = SceneManager.GetActiveScene().name,
			Blocks = list.ToArray(),
			Props = list2.ToArray(),
			Enemies = list3.ToArray()
		});
#if UNITY_WEBGL
		webglSaves[name] = contents;
#else
		File.WriteAllText(Path.Combine(SavePath, name + ".pitr"), contents);
#endif
	}
}
