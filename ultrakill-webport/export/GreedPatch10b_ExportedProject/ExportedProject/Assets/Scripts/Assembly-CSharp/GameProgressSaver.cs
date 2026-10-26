using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public static class GameProgressSaver
{
	public static int currentSlot = 0;

	private static bool initialized;

	private static readonly string[] SlotIgnoreFiles = new string[1] { "prefs" };

#if UNITY_WEBGL
	// Ruta virtual: en WebGL solo se usa como clave de la cache en memoria.
	public static string BaseSavePath => "Saves";
#else
	public static string BaseSavePath => Path.Combine((SystemInfo.deviceType == DeviceType.Desktop) ? Directory.GetParent(Application.dataPath).FullName : Application.persistentDataPath, "Saves");
#endif

	// Persistencia WebGL: no hay disco, asi que ReadFile/WriteFile trabajan
	// contra este diccionario. La clave es la ruta logica del guardado.
#if UNITY_WEBGL
	private static readonly Dictionary<string, object> webglFileCache = new Dictionary<string, object>();
#endif

	private static string savePath => Path.Combine(BaseSavePath, $"Slot{currentSlot + 1}");

	private static string currentDifficultyPath => DifficultySavePath(MonoSingleton<PrefsManager>.Instance.GetInt("difficulty"));

	private static string customSavesDir => Path.Combine(savePath, "Custom");

	private static string generalProgressPath => Path.Combine(savePath, "generalprogress.bepis");

	private static string cyberGrindHighScorePath => Path.Combine(savePath, "cybergrindhighscore.bepis");

	private static string customLevelProgressPath => Path.Combine(customSavesDir, MonoSingleton<MapLoader>.Instance.currentCustomId + ".bepis");

	private static string resolveCurrentLevelPath
	{
		get
		{
			if (!MapLoader.CustomMapPlaying)
			{
				return LevelProgressPath(MonoSingleton<StatsManager>.Instance.levelNumber);
			}
			return customLevelProgressPath;
		}
	}

	private static string DifficultySavePath(int diff)
	{
		return Path.Combine(savePath, $"difficulty{diff}progress.bepis");
	}

	private static string LevelProgressPath(int lvl)
	{
		return Path.Combine(savePath, $"lvl{lvl}progress.bepis");
	}

	public static void SetSlot(int slot)
	{
		currentSlot = slot;
		MonoSingleton<PrefsManager>.Instance.SetInt("selectedSaveSlot", slot);
		MonoSingleton<BestiaryData>.Instance.CheckSave();
	}

	private static void CreateSaveDirs(bool forceCustom = false)
	{
#if UNITY_WEBGL
		// Sin disco: las "carpetas" son solo prefijos de claves en la cache.
#else
		if (!Directory.Exists(savePath))
		{
			Directory.CreateDirectory(savePath);
		}
		if ((MapLoader.CustomMapPlaying | forceCustom) && !Directory.Exists(customSavesDir))
		{
			Directory.CreateDirectory(customSavesDir);
		}
#endif
	}

	public static void WipeSlot(int slot)
	{
		int num = currentSlot;
		currentSlot = slot;
#if UNITY_WEBGL
		List<string> list = new List<string>();
		foreach (string key in webglFileCache.Keys)
		{
			if (key.StartsWith(savePath + Path.DirectorySeparatorChar))
			{
				list.Add(key);
			}
		}
		foreach (string item in list)
		{
			webglFileCache.Remove(item);
		}
#else
		try
		{
			if (Directory.Exists(savePath))
			{
				string[] files = Directory.GetFiles(savePath, "*.bepis", SearchOption.TopDirectoryOnly);
				for (int i = 0; i < files.Length; i++)
				{
					File.Delete(files[i]);
				}
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
#endif
		currentSlot = num;
	}

	private static SaveSlotMenu.SlotData GetDirectorySlotData(string path)
	{
		Debug.Log("Generating SlotData for " + path);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 6; i++)
		{
			if (ReadFile(Path.Combine(path, $"difficulty{i}progress.bepis")) is GameProgressData gameProgressData && (gameProgressData.levelNum > num || (gameProgressData.levelNum == num && gameProgressData.difficulty > num2)))
			{
				num = gameProgressData.levelNum;
				num2 = gameProgressData.difficulty;
			}
		}
		return new SaveSlotMenu.SlotData
		{
			exists = true,
			highestDifficulty = num2,
			highestLvlNumber = num
		};
	}

	public static SaveSlotMenu.SlotData[] GetSlots()
	{
		PrepareFs();
		int num = currentSlot;
		List<SaveSlotMenu.SlotData> list = new List<SaveSlotMenu.SlotData>();
		for (int i = 0; i < 5; i++)
		{
			currentSlot = i;
#if UNITY_WEBGL
			bool flag = false;
			foreach (string key in webglFileCache.Keys)
			{
				if (key.StartsWith(savePath + Path.DirectorySeparatorChar))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
#else
			if (!Directory.Exists(savePath))
#endif
			{
				list.Add(new SaveSlotMenu.SlotData
				{
					exists = false
				});
			}
			else if (!(ReadFile(generalProgressPath) is GameProgressMoneyAndGear))
			{
				list.Add(new SaveSlotMenu.SlotData
				{
					exists = false
				});
			}
			else
			{
				SaveSlotMenu.SlotData directorySlotData = GetDirectorySlotData(savePath);
				list.Add(directorySlotData);
			}
		}
		currentSlot = num;
		return list.ToArray();
	}

	private static void PrepareFs()
	{
		CreateSaveDirs();
		if (initialized)
		{
			return;
		}
		initialized = true;
#if UNITY_WEBGL
		// No hay guardados antiguos en disco; solo restauramos el slot elegido.
		currentSlot = MonoSingleton<PrefsManager>.Instance.GetInt("selectedSaveSlot");
		return;
#else
		if ((from a in Directory.GetFiles(BaseSavePath, "*.bepis", SearchOption.TopDirectoryOnly)
			where !SlotIgnoreFiles.Contains(Path.GetFileNameWithoutExtension(a))
			select a).ToArray().Length != 0)
		{
			Debug.Log("Old saves found");
			int num = currentSlot;
			currentSlot = 0;
			try
			{
				CreateSaveDirs();
				if (Directory.GetFiles(savePath, "*.bepis").Length != 0)
				{
					Debug.Log("Slot 1 is populated while old saves also exist. Showing consent screen.");
					SaveSlotMenu.SlotData directorySlotData = GetDirectorySlotData(BaseSavePath);
					SaveSlotMenu.SlotData directorySlotData2 = GetDirectorySlotData(savePath);
					SaveLoadFailMessage.DisplayMergeConsent(directorySlotData, directorySlotData2);
				}
				else
				{
					MergeRootWithSlotOne(keepRoot: true);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			currentSlot = num;
		}
		if (Directory.Exists(Path.Combine(BaseSavePath, "Custom")))
		{
			string[] files = Directory.GetFiles(Path.Combine(BaseSavePath, "Custom"), "*.bepis", SearchOption.TopDirectoryOnly);
			if (files.Length != 0)
			{
				int num2 = currentSlot;
				currentSlot = 0;
				try
				{
					CreateSaveDirs(forceCustom: true);
					string[] array = files;
					foreach (string text in array)
					{
						SafeMove(text, Path.Combine(customSavesDir, Path.GetFileName(text)));
					}
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
				currentSlot = num2;
			}
		}
		currentSlot = MonoSingleton<PrefsManager>.Instance.GetInt("selectedSaveSlot");
#endif
	}

	public static void MergeRootWithSlotOne(bool keepRoot)
	{
#if UNITY_WEBGL
		// En WebGL no existen guardados antiguos que migrar.
#else
		string[] array = (from a in Directory.GetFiles(BaseSavePath, "*.bepis", SearchOption.TopDirectoryOnly)
			where !SlotIgnoreFiles.Contains(Path.GetFileNameWithoutExtension(a))
			select a).ToArray();
		foreach (string text in array)
		{
			Debug.Log(text);
			if (!keepRoot)
			{
				File.Delete(text);
			}
			else
			{
				SafeMove(text, Path.Combine(savePath, Path.GetFileName(text)));
			}
		}
#endif
	}

	private static void SafeMove(string source, string target)
	{
		if (File.Exists(target))
		{
			File.Delete(target);
		}
		File.Move(source, target);
	}

	private static object ReadFile(string path)
	{
		PrepareFs();
#if UNITY_WEBGL
		object result;
		if (webglFileCache.TryGetValue(path, out result))
		{
			return result;
		}
		return null;
#else
		if (!File.Exists(path))
		{
			return null;
		}
		using (FileStream fileStream = new FileStream(path, FileMode.Open))
		{
			if (fileStream.Length == 0L)
			{
				Debug.LogError("Stream Length 0");
				return null;
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			try
			{
				object result = binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				return result;
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				return null;
			}
		}
#endif
	}

	private static void WriteFile(string path, object data)
	{
		PrepareFs();
		Debug.Log("[FS] Writing To " + path);
#if UNITY_WEBGL
		webglFileCache[path] = data;
#else
		using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			try
			{
				binaryFormatter.Serialize(fileStream, data);
				fileStream.Close();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
#endif
	}

	private static RankData GetRankData(bool returnNull)
	{
		string path;
		return GetRankData(out path, -1, returnNull);
	}

	private static RankData GetRankData(int lvl = -1)
	{
		string path;
		return GetRankData(out path, lvl);
	}

	private static RankData GetRankData(out string path, int lvl = -1, bool returnNull = false)
	{
		path = ((lvl < 0) ? resolveCurrentLevelPath : LevelProgressPath(lvl));
		object obj = ReadFile(path);
		Debug.Log(obj);
		RankData result;
		if ((result = obj as RankData) == null)
		{
			result = (returnNull ? null : new RankData(MonoSingleton<StatsManager>.Instance));
		}
		return result;
	}

	private static GameProgressData GetGameProgress(int difficulty = -1)
	{
		string path;
		return GetGameProgress(out path, difficulty);
	}

	private static GameProgressData GetGameProgress(out string path, int difficulty = -1)
	{
		path = ((difficulty < 0) ? currentDifficultyPath : DifficultySavePath(difficulty));
		GameProgressData gameProgressData;
		if ((gameProgressData = ReadFile(path) as GameProgressData) == null)
		{
			gameProgressData = new GameProgressData();
		}
		if (gameProgressData.primeLevels == null || gameProgressData.primeLevels.Length == 0)
		{
			gameProgressData.primeLevels = new int[3];
		}
		return gameProgressData;
	}

	public static void ChallengeComplete()
	{
		RankData rankData = GetRankData();
		if (!rankData.challenge && (rankData.levelNumber == MonoSingleton<StatsManager>.Instance.levelNumber || ((bool)MonoSingleton<MapLoader>.Instance && MonoSingleton<MapLoader>.Instance.isCustomLoaded)))
		{
			rankData.challenge = true;
			WriteFile(resolveCurrentLevelPath, rankData);
		}
	}

	public static void SaveProgress(int levelNum)
	{
		GameProgressData gameProgress = GetGameProgress(out var path);
		if (gameProgress.levelNum < levelNum || gameProgress.difficulty != MonoSingleton<PrefsManager>.Instance.GetInt("difficulty"))
		{
			gameProgress.levelNum = levelNum;
			WriteFile(path, gameProgress);
		}
	}

	public static void SaveRank()
	{
		WriteFile(resolveCurrentLevelPath, new RankData(MonoSingleton<StatsManager>.Instance));
	}

	public static RankData GetRank(bool returnNull)
	{
		return GetRankData(returnNull);
	}

	public static void SecretFound(int secretNum)
	{
		RankData rankData = GetRankData(out var path);
		if ((rankData.levelNumber == MonoSingleton<StatsManager>.Instance.levelNumber || MapLoader.CustomMapPlaying) && !rankData.secretsFound[secretNum])
		{
			rankData.secretsFound[secretNum] = true;
			WriteFile(path, rankData);
		}
	}

	public static int GetProgress(int difficulty)
	{
		if (MapLoader.CustomMapPlaying)
		{
			return 1;
		}
		int num = 1;
		for (int i = difficulty; i <= 5; i++)
		{
			GameProgressData gameProgress = GetGameProgress(i);
			if (gameProgress != null && gameProgress.difficulty == i && gameProgress.levelNum > num)
			{
				num = gameProgress.levelNum;
			}
		}
		return num;
	}

	public static RankData GetRank(int levelNumber)
	{
		return GetRankData(levelNumber);
	}

	public static void SetPrime(int level, int state)
	{
		if (MapLoader.CustomMapPlaying)
		{
			return;
		}
		level--;
		GameProgressData gameProgress = GetGameProgress(out var path);
		if (level < gameProgress.primeLevels.Length)
		{
			if (state <= gameProgress.primeLevels[level])
			{
				return;
			}
			gameProgress.primeLevels[level] = state;
		}
		else
		{
			int[] array = new int[3];
			for (int i = 0; i < gameProgress.primeLevels.Length; i++)
			{
				array[i] = gameProgress.primeLevels[i];
			}
			for (int j = gameProgress.primeLevels.Length; j < array.Length; j++)
			{
				if (j == level)
				{
					array[j] = state;
				}
				else
				{
					array[j] = 0;
				}
			}
			gameProgress.primeLevels = array;
		}
		WriteFile(path, gameProgress);
	}

	public static int GetPrime(int difficulty, int level)
	{
		if (MapLoader.CustomMapPlaying)
		{
			return 0;
		}
		level--;
		int num = 0;
		for (int i = difficulty; i <= 5; i++)
		{
			GameProgressData gameProgress = GetGameProgress(i);
			if (gameProgress != null && gameProgress.difficulty == i && gameProgress.primeLevels != null && gameProgress.primeLevels.Length > level && gameProgress.primeLevels[level] > num)
			{
				Debug.Log("Highest: . Data: " + gameProgress.primeLevels[level]);
				if (gameProgress.primeLevels[level] >= 2)
				{
					return 2;
				}
				num = gameProgress.primeLevels[level];
			}
		}
		return num;
	}

	public static GameProgressMoneyAndGear GetGeneralProgress()
	{
		PrepareFs();
		GameProgressMoneyAndGear gameProgressMoneyAndGear;
		if ((gameProgressMoneyAndGear = ReadFile(generalProgressPath) as GameProgressMoneyAndGear) == null)
		{
			gameProgressMoneyAndGear = new GameProgressMoneyAndGear();
		}
		if (gameProgressMoneyAndGear.secretMissions == null || gameProgressMoneyAndGear.secretMissions.Length == 0)
		{
			gameProgressMoneyAndGear.secretMissions = new int[10];
		}
		if (gameProgressMoneyAndGear.limboSwitches == null || gameProgressMoneyAndGear.limboSwitches.Length == 0)
		{
			gameProgressMoneyAndGear.limboSwitches = new bool[4];
		}
		if (gameProgressMoneyAndGear.newEnemiesFound == null)
		{
			gameProgressMoneyAndGear.newEnemiesFound = new int[Enum.GetValues(typeof(EnemyType)).Length];
		}
		return gameProgressMoneyAndGear;
	}

	public static void AddGear(string gear)
	{
		if (!MapLoader.CustomMapPlaying)
		{
			GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
			FieldInfo field = typeof(GameProgressMoneyAndGear).GetField(gear, BindingFlags.Instance | BindingFlags.Public);
			if (!(field == null))
			{
				field.SetValue(generalProgress, 1);
				WriteFile(generalProgressPath, generalProgress);
			}
		}
	}

	public static int CheckGear(string gear)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		FieldInfo field = typeof(GameProgressMoneyAndGear).GetField(gear, BindingFlags.Instance | BindingFlags.Public);
		if (field == null)
		{
			return 0;
		}
		object value = field.GetValue(generalProgress);
		if (value == null)
		{
			return 0;
		}
		return (int)value;
	}

	public static void AddMoney(int money)
	{
		if (!MapLoader.CustomMapPlaying)
		{
			GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
			if (generalProgress.money + money >= 0)
			{
				generalProgress.money += money;
			}
			else
			{
				generalProgress.money = 0;
			}
			WriteFile(generalProgressPath, generalProgress);
		}
	}

	public static int GetMoney()
	{
		return GetGeneralProgress().money;
	}

	public static void SetTutorial(bool beat)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		generalProgress.tutorialBeat = beat;
		WriteFile(generalProgressPath, generalProgress);
	}

	public static bool GetTutorial()
	{
		if (MapLoader.CustomMapPlaying)
		{
			return true;
		}
		return GetGeneralProgress().tutorialBeat;
	}

	public static void SetIntro(bool seen)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		generalProgress.introSeen = seen;
		WriteFile(generalProgressPath, generalProgress);
	}

	public static bool GetIntro()
	{
		if (MapLoader.CustomMapPlaying)
		{
			return true;
		}
		return GetGeneralProgress().introSeen;
	}

	public static void SetClashModeUnlocked(bool unlocked)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		generalProgress.clashModeUnlocked = unlocked;
		WriteFile(generalProgressPath, generalProgress);
	}

	public static bool GetClashModeUnlocked()
	{
		if (MapLoader.CustomMapPlaying)
		{
			return true;
		}
		return GetGeneralProgress().clashModeUnlocked;
	}

	public static void SetBestiary(EnemyType enemy, int state)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		int[] array = new int[Enum.GetValues(typeof(EnemyType)).Length];
		for (int i = 0; i < generalProgress.newEnemiesFound.Length; i++)
		{
			array[i] = generalProgress.newEnemiesFound[i];
		}
		for (int j = generalProgress.newEnemiesFound.Length; j < array.Length; j++)
		{
			array[j] = 0;
		}
		array[(int)enemy] = state;
		generalProgress.newEnemiesFound = array;
		WriteFile(generalProgressPath, generalProgress);
	}

	public static int[] GetBestiary()
	{
		return GetGeneralProgress().newEnemiesFound;
	}

	public static void SetLimboSwitch(int switchNum)
	{
		if (!MapLoader.CustomMapPlaying)
		{
			GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
			if (switchNum < generalProgress.limboSwitches.Length)
			{
				generalProgress.limboSwitches[switchNum] = true;
			}
			WriteFile(generalProgressPath, generalProgress);
		}
	}

	public static bool GetLimboSwitch(int switchNum)
	{
		return GetGeneralProgress().limboSwitches[switchNum];
	}

	public static int GetSecretMission(int missionNumber)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		if (generalProgress.secretMissions.Length > missionNumber)
		{
			return generalProgress.secretMissions[missionNumber];
		}
		return 0;
	}

	public static void FoundSecretMission(int missionNumber)
	{
		if (!MapLoader.CustomMapPlaying)
		{
			GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
			if (generalProgress.secretMissions[missionNumber] != 2)
			{
				generalProgress.secretMissions[missionNumber] = 1;
			}
			WriteFile(generalProgressPath, generalProgress);
		}
	}

	public static void SetSecretMission(int missionNumber)
	{
		if (!MapLoader.CustomMapPlaying)
		{
			GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
			generalProgress.secretMissions[missionNumber] = 2;
			WriteFile(generalProgressPath, generalProgress);
		}
	}

	private static CyberRankData GetCyberRankData()
	{
		CyberRankData cyberRankData;
		if ((cyberRankData = ReadFile(cyberGrindHighScorePath) as CyberRankData) == null)
		{
			cyberRankData = new CyberRankData();
		}
		if (cyberRankData.preciseWavesByDifficulty == null || cyberRankData.preciseWavesByDifficulty.Length != 6)
		{
			cyberRankData.preciseWavesByDifficulty = new float[6];
		}
		if (cyberRankData.style == null || cyberRankData.style.Length != 6)
		{
			cyberRankData.style = new int[6];
		}
		if (cyberRankData.kills == null || cyberRankData.kills.Length != 6)
		{
			cyberRankData.kills = new int[6];
		}
		if (cyberRankData.time == null || cyberRankData.time.Length != 6)
		{
			cyberRankData.time = new float[6];
		}
		return cyberRankData;
	}

	public static CyberRankData GetBestCyber()
	{
		return GetCyberRankData();
	}

	public static void SetBestCyber(FinalCyberRank fcr)
	{
		if (!MapLoader.CustomMapPlaying)
		{
			CyberRankData cyberRankData = GetCyberRankData();
			int num = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
			cyberRankData.preciseWavesByDifficulty[num] = fcr.savedWaves;
			cyberRankData.kills[num] = fcr.savedKills;
			cyberRankData.style[num] = fcr.savedStyle;
			cyberRankData.time[num] = fcr.savedTime;
			WriteFile(cyberGrindHighScorePath, cyberRankData);
		}
	}

	// -----------------------------------------------------------------------
	// Debug del menu F3 (WebGL): desbloqueo de armas y progreso de niveles.
	// WriteFile en WebGL solo actualiza la cache en memoria, asi que no se
	// toca el sistema de ficheros.
	// -----------------------------------------------------------------------

	private static readonly string[] DebugGearIds = new string[29]
	{
		"rev0", "rev1", "rev2", "rev3", "revalt",
		"sho0", "sho1", "sho2", "sho3",
		"nai0", "nai1", "nai2", "nai3", "naialt",
		"rai0", "rai1", "rai2", "rai3",
		"rock0", "rock1", "rock2", "rock3",
		"beam0", "beam1", "beam2", "beam3",
		"arm1", "arm2", "arm3"
	};

	// Capas 0..4. La capa 4 (GREED) va del nivel 16 (4-1) al 19 (4-4).
	private static readonly int[] DebugLayerFirstLevel = new int[5] { 1, 6, 10, 14, 16 };

	private static readonly int[] DebugLayerLastLevel = new int[5] { 5, 9, 13, 15, 19 };

	public static void DebugSetLayerCompleted(int layer, bool completed)
	{
		if (layer < 0 || layer >= DebugLayerFirstLevel.Length)
		{
			return;
		}
		int difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		int levelNum = (completed ? Mathf.Max(GetProgress(difficulty), DebugLayerLastLevel[layer] + 1) : DebugLayerFirstLevel[layer]);
		DebugWriteProgress(levelNum);
		if (completed)
		{
			SetTutorial(beat: true);
			SetIntro(seen: true);
		}
	}

	public static void DebugSetPrimeCompleted(bool completed)
	{
		GameProgressData gameProgress = GetGameProgress(out var path);
		if (gameProgress.primeLevels == null || gameProgress.primeLevels.Length < 3)
		{
			gameProgress.primeLevels = new int[3];
		}
		gameProgress.primeLevels[0] = (completed ? 2 : 0);
		WriteFile(path, gameProgress);
		if (completed)
		{
			SetTutorial(beat: true);
			SetIntro(seen: true);
		}
	}

	public static void DebugSetAllLevelsCompleted(bool completed)
	{
		if (completed)
		{
			// 20 = todo lo disponible en 10b (hasta 4-4) vencido.
			DebugWriteProgress(20);
			SetTutorial(beat: true);
			SetIntro(seen: true);
			DebugSetPrimeCompleted(completed: true);
		}
		else
		{
			DebugWriteProgress(1);
			DebugSetPrimeCompleted(completed: false);
		}
	}

	private static void DebugWriteProgress(int levelNum)
	{
		GameProgressData gameProgress = GetGameProgress(out var path);
		gameProgress.levelNum = levelNum;
		gameProgress.difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		WriteFile(path, gameProgress);
	}

	public static void DebugSetGearFamily(string family, bool unlocked)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		int num = (unlocked ? 1 : 0);
		switch (family)
		{
		case "revolver":
			generalProgress.rev0 = num;
			generalProgress.rev1 = num;
			generalProgress.rev2 = num;
			generalProgress.rev3 = num;
			generalProgress.revalt = num;
			break;
		case "shotgun":
			generalProgress.sho0 = num;
			generalProgress.sho1 = num;
			generalProgress.sho2 = num;
			generalProgress.sho3 = num;
			break;
		case "nailgun":
			generalProgress.nai0 = num;
			generalProgress.nai1 = num;
			generalProgress.nai2 = num;
			generalProgress.nai3 = num;
			generalProgress.naialt = num;
			break;
		case "railcannon":
			generalProgress.rai0 = num;
			generalProgress.rai1 = num;
			generalProgress.rai2 = num;
			generalProgress.rai3 = num;
			break;
		case "rocket":
			generalProgress.rock0 = num;
			generalProgress.rock1 = num;
			generalProgress.rock2 = num;
			generalProgress.rock3 = num;
			break;
		case "beamgun":
			generalProgress.beam0 = num;
			generalProgress.beam1 = num;
			generalProgress.beam2 = num;
			generalProgress.beam3 = num;
			break;
		case "arms":
			generalProgress.arm1 = num;
			generalProgress.arm2 = num;
			generalProgress.arm3 = num;
			break;
		default:
			return;
		}
		WriteFile(generalProgressPath, generalProgress);
	}

	public static void DebugSetAllGear(bool unlocked)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		int num = (unlocked ? 1 : 0);
		generalProgress.rev0 = num;
		generalProgress.rev1 = num;
		generalProgress.rev2 = num;
		generalProgress.rev3 = num;
		generalProgress.revalt = num;
		generalProgress.sho0 = num;
		generalProgress.sho1 = num;
		generalProgress.sho2 = num;
		generalProgress.sho3 = num;
		generalProgress.nai0 = num;
		generalProgress.nai1 = num;
		generalProgress.nai2 = num;
		generalProgress.nai3 = num;
		generalProgress.naialt = num;
		generalProgress.rai0 = num;
		generalProgress.rai1 = num;
		generalProgress.rai2 = num;
		generalProgress.rai3 = num;
		generalProgress.rock0 = num;
		generalProgress.rock1 = num;
		generalProgress.rock2 = num;
		generalProgress.rock3 = num;
		generalProgress.beam0 = num;
		generalProgress.beam1 = num;
		generalProgress.beam2 = num;
		generalProgress.beam3 = num;
		generalProgress.arm1 = num;
		generalProgress.arm2 = num;
		generalProgress.arm3 = num;
		WriteFile(generalProgressPath, generalProgress);
	}

	public static string DebugGetState()
	{
		// Formato compacto (sin comillas) para Application.ExternalCall:
		// progreso|dificultad|tutorial|intro|prime|rev0,rev1,...
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		int difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		StringBuilder stringBuilder = new StringBuilder(200);
		stringBuilder.Append(GetProgress(difficulty)).Append('|');
		stringBuilder.Append(difficulty).Append('|');
		stringBuilder.Append(GetTutorial() ? 1 : 0).Append('|');
		stringBuilder.Append(GetIntro() ? 1 : 0).Append('|');
		stringBuilder.Append(GetPrime(difficulty, 1)).Append('|');
		for (int i = 0; i < DebugGearIds.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append(DebugGetGearValue(generalProgress, DebugGearIds[i]));
		}
		return stringBuilder.ToString();
	}

	private static int DebugGetGearValue(GameProgressMoneyAndGear data, string gear)
	{
		if (data == null)
		{
			return 0;
		}
		FieldInfo field = typeof(GameProgressMoneyAndGear).GetField(gear, BindingFlags.Instance | BindingFlags.Public);
		if (field == null)
		{
			return 0;
		}
		object value = field.GetValue(data);
		if (value == null)
		{
			return 0;
		}
		return (int)value;
	}
}
