using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamController : MonoBehaviour
{
	public static SteamController Instance;

	[SerializeField]
	private string[] cyberGrindDifLeaderboards;

	private Dictionary<string, Leaderboard> cyberGrindLeaderboardCache = new Dictionary<string, Leaderboard>();

	[SerializeField]
	private uint appId;

	private void Awake()
	{
		if ((bool)Instance)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		Instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		base.transform.SetParent(null);
#if UNITY_WEBGL
		Debug.Log("Steam disabled on WebGL");
#else
		try
		{
			SteamClient.Init(appId);
			Debug.Log("Steam initialized!");
		}
		catch (Exception)
		{
			Debug.Log("Couldn't initialize steam");
		}
#endif
	}

	public void UpdateWave(int wave)
	{
		if (SteamClient.IsValid)
		{
			SteamFriends.SetRichPresence("wave", wave.ToString());
		}
	}

	private async Task<Leaderboard?> FetchLeaderboard(int difficulty, bool precise)
	{
		string key = difficulty.ToString();
		if (precise)
		{
			key += "_precise";
		}
		if (cyberGrindDifLeaderboards.Length > difficulty && !string.IsNullOrEmpty(cyberGrindDifLeaderboards[difficulty]))
		{
			if (cyberGrindLeaderboardCache.ContainsKey(key))
			{
				return cyberGrindLeaderboardCache[key];
			}
			string text = cyberGrindDifLeaderboards[difficulty];
			if (precise)
			{
				text += " Precise";
			}
			Leaderboard? leaderboard = await SteamUserStats.FindLeaderboardAsync(text);
			if (!leaderboard.HasValue)
			{
				return null;
			}
			Leaderboard value = leaderboard.Value;
			cyberGrindLeaderboardCache.Add(key, value);
			return value;
		}
		return null;
	}

	public async void SubmitCyberGrindScore(int difficulty, float wave, int kills, int style, float seconds)
	{
		if (!SteamClient.IsValid)
		{
			return;
		}
		int majorVersion = -1;
		int minorVersion = -1;
		string[] array = Application.version.Split('.');
		if (int.TryParse(array[0], out var result))
		{
			majorVersion = result;
		}
		if (array.Length > 1 && int.TryParse(array[1], out var result2))
		{
			minorVersion = result2;
		}
		Leaderboard? leaderboard = await FetchLeaderboard(difficulty, precise: false);
		if (leaderboard.HasValue)
		{
			await leaderboard.Value.SubmitScoreAsync(Mathf.FloorToInt(wave), new int[6]
			{
				kills,
				style,
				Mathf.FloorToInt(seconds * 1000f),
				majorVersion,
				minorVersion,
				DateTimeOffset.UtcNow.Millisecond
			});
			Leaderboard? leaderboard2 = await FetchLeaderboard(difficulty, precise: true);
			if (leaderboard2.HasValue)
			{
				await leaderboard2.Value.SubmitScoreAsync(Mathf.FloorToInt(wave * 1000f), new int[6]
				{
					kills,
					style,
					Mathf.FloorToInt(seconds * 1000f),
					majorVersion,
					minorVersion,
					DateTimeOffset.UtcNow.Millisecond
				});
				Debug.Log($"Score {wave} submitted to Steamworks");
			}
		}
	}

	public async Task<LeaderboardEntry[]> GetCyberGrindScores(int difficulty, bool friendsOnly)
	{
		if (!SteamClient.IsValid)
		{
			return null;
		}
		Leaderboard? leaderboard = await FetchLeaderboard(difficulty, precise: true);
		if (!leaderboard.HasValue)
		{
			return null;
		}
		Leaderboard value = leaderboard.Value;
		if (friendsOnly)
		{
			LeaderboardEntry[] array = await value.GetScoresFromFriendsAsync();
			if (array == null)
			{
				return new LeaderboardEntry[0];
			}
			return array.Take(10).ToArray();
		}
		LeaderboardEntry[] array2 = await value.GetScoresAroundUserAsync(-4, 3);
		if (array2 == null)
		{
			return new LeaderboardEntry[0];
		}
		return array2;
	}

	public void FetchSceneActivity(string scene)
	{
		if (!SteamClient.IsValid)
		{
			return;
		}
		if ((bool)MonoSingleton<MapLoader>.Instance && MonoSingleton<MapLoader>.Instance.isCustomLoaded)
		{
			SteamFriends.SetRichPresence("steam_display", "#AtCustomLevel");
			return;
		}
		StockMapInfo instance = StockMapInfo.Instance;
		if (scene == "Main Menu")
		{
			SteamFriends.SetRichPresence("steam_display", "#AtMainMenu");
		}
		else if (scene == "Endless")
		{
			SteamFriends.SetRichPresence("steam_display", "#AtCyberGrind");
			SteamFriends.SetRichPresence("difficulty", MonoSingleton<PresenceController>.Instance.diffNames[MonoSingleton<PrefsManager>.Instance.GetInt("difficulty")]);
			SteamFriends.SetRichPresence("wave", "0");
		}
		else if (instance != null && !string.IsNullOrEmpty(instance.assets.Deserialize().LargeText))
		{
			SteamFriends.SetRichPresence("steam_display", "#AtStandardLevel");
			SteamFriends.SetRichPresence("difficulty", MonoSingleton<PresenceController>.Instance.diffNames[MonoSingleton<PrefsManager>.Instance.GetInt("difficulty")]);
			SteamFriends.SetRichPresence("level", instance.assets.Deserialize().LargeText);
		}
		else
		{
			SteamFriends.SetRichPresence("steam_display", "#UnknownLevel");
		}
	}

	private void OnApplicationQuit()
	{
		if (SteamClient.IsValid)
		{
			SteamClient.Shutdown();
		}
	}
}
