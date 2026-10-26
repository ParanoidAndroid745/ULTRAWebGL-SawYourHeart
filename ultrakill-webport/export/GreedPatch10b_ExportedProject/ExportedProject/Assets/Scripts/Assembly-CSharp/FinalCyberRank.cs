using System.Collections;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalCyberRank : MonoBehaviour
{
	public Text waveText;

	public Text killsText;

	public Text styleText;

	public Text timeText;

	public Text bestWaveText;

	public Text bestKillsText;

	public Text bestStyleText;

	public Text bestTimeText;

	public Text pointsText;

	public int totalPoints;

	public GameObject[] toAppear;

	private bool skipping;

	private float timeBetween = 0.25f;

	private bool countTime;

	public float savedTime;

	private float checkedSeconds;

	private float seconds;

	private float minutes;

	private bool countWaves;

	public float savedWaves;

	private float checkedWaves;

	private bool countKills;

	public int savedKills;

	private float checkedKills;

	private bool countStyle;

	public int savedStyle;

	private float checkedStyle;

	private bool flashFade;

	private UnityEngine.Color flashColor;

	private UnityEngine.UI.Image flashPanel;

	private int i;

	private bool gameOver;

	private bool complete;

	private CyberRankData previousBest;

	private bool newBest;

	private TimeController timeController;

	private OptionsManager opm;

	private bool wasPaused;

	private StatsManager sman;

	private bool highScoresDisplayed;

	[SerializeField]
	private GameObject[] previousElements;

	[SerializeField]
	private GameObject highScoreElement;

	[SerializeField]
	private GameObject friendContainer;

	[SerializeField]
	private GameObject globalContainer;

	[SerializeField]
	private GameObject friendPlaceholder;

	[SerializeField]
	private GameObject globalPlaceholder;

	[SerializeField]
	private GameObject template;

	[SerializeField]
	private Text tRank;

	[SerializeField]
	private Text tUsername;

	[SerializeField]
	private Text tScore;

	[SerializeField]
	private Text tPercent;

	private void Start()
	{
		sman = MonoSingleton<StatsManager>.Instance;
		GameObject[] array = toAppear;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		MonoSingleton<NewMovement>.Instance.endlessMode = true;
	}

	public void GameOver()
	{
		if (gameOver)
		{
			return;
		}
		if (sman == null)
		{
			sman = MonoSingleton<StatsManager>.Instance;
		}
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		gameOver = true;
		sman.StopTimer();
		sman.HideShit();
		MonoSingleton<TimeController>.Instance.controlTimeScale = false;
		savedTime = sman.seconds;
		savedKills = sman.kills;
		savedStyle = sman.stylePoints;
		if (savedStyle < 0)
		{
			savedStyle = 0;
		}
		ActivateNextWave activateNextWave = Object.FindObjectOfType<ActivateNextWave>();
		savedWaves = (float)MonoSingleton<EndlessGrid>.Instance.currentWave + (float)activateNextWave.deadEnemies / (float)MonoSingleton<EndlessGrid>.Instance.enemyAmount;
		previousBest = GameProgressSaver.GetBestCyber();
		bestWaveText.text = Mathf.FloorToInt(previousBest.preciseWavesByDifficulty[num]) + $"\n<color=#616161><size=20>{CalculatePerc(previousBest.preciseWavesByDifficulty[num])}%</size></color>";
		bestKillsText.text = string.Concat(previousBest.kills[num]);
		bestStyleText.text = string.Concat(previousBest.style[num]);
		int num2 = 0;
		float num3;
		for (num3 = previousBest.time[num]; num3 >= 60f; num3 -= 60f)
		{
			num2++;
		}
		bestTimeText.text = num2 + ":" + num3.ToString("00.000");
		if (sman.majorUsed || MonoSingleton<AssistController>.Instance.cheatsEnabled || MonoSingleton<EndlessGrid>.Instance.customPatternMode)
		{
			return;
		}
		if ((bool)SteamController.Instance)
		{
			SteamController.Instance.SubmitCyberGrindScore(num, savedWaves, savedKills, savedStyle, sman.seconds);
		}
		if (savedWaves > previousBest.preciseWavesByDifficulty[num])
		{
			NewBest();
		}
		else
		{
			if (savedWaves < previousBest.preciseWavesByDifficulty[num])
			{
				return;
			}
			if (savedKills > previousBest.kills[num])
			{
				NewBest();
			}
			else if (savedKills >= previousBest.kills[num])
			{
				if (savedStyle > previousBest.style[num])
				{
					NewBest();
				}
				else if (savedStyle >= previousBest.style[num] && savedTime > previousBest.time[num])
				{
					NewBest();
				}
			}
		}
	}

	private void NewBest()
	{
		GameProgressSaver.SetBestCyber(this);
		newBest = true;
	}

	private void Update()
	{
		if (gameOver)
		{
			if (timeController == null)
			{
				timeController = MonoSingleton<TimeController>.Instance;
			}
			if (timeController.timeScale > 0f)
			{
				timeController.timeScale = Mathf.MoveTowards(timeController.timeScale, 0f, Time.unscaledDeltaTime * (timeController.timeScale + 0.01f));
				Time.timeScale = timeController.timeScale * timeController.timeScaleModifier;
				MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allPitch", timeController.timeScale);
				MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allVolume", 0.5f + timeController.timeScale / 2f);
				MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("allPitch", timeController.timeScale);
				MonoSingleton<MusicManager>.Instance.volume = 0.5f + timeController.timeScale / 2f;
				if (timeController.timeScale <= 0f)
				{
					Appear();
					MonoSingleton<MusicManager>.Instance.forcedOff = true;
					MonoSingleton<MusicManager>.Instance.StopMusic();
				}
			}
		}
		if (countTime)
		{
			if (savedTime >= checkedSeconds)
			{
				if (savedTime > checkedSeconds)
				{
					float num = savedTime - checkedSeconds;
					checkedSeconds += Time.unscaledDeltaTime * 20f + Time.unscaledDeltaTime * num * 1.5f;
					seconds += Time.unscaledDeltaTime * 20f + Time.unscaledDeltaTime * num * 1.5f;
				}
				if (checkedSeconds >= savedTime || skipping)
				{
					checkedSeconds = savedTime;
					seconds = savedTime;
					minutes = 0f;
					while (seconds >= 60f)
					{
						seconds -= 60f;
						minutes++;
					}
					countTime = false;
					timeText.GetComponent<AudioSource>().Stop();
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
				}
				if (seconds >= 60f)
				{
					seconds -= 60f;
					minutes++;
				}
				timeText.text = minutes + ":" + seconds.ToString("00.000");
			}
		}
		else if (countWaves)
		{
			if (savedWaves >= checkedWaves)
			{
				if (savedWaves > checkedWaves)
				{
					checkedWaves += Time.unscaledDeltaTime * 20f + Time.unscaledDeltaTime * (savedWaves - checkedWaves) * 1.5f;
				}
				if (checkedWaves >= savedWaves || skipping)
				{
					checkedWaves = savedWaves;
					countWaves = false;
					waveText.GetComponent<AudioSource>().Stop();
					totalPoints += Mathf.FloorToInt(savedWaves) * 100;
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
				}
				else
				{
					int num2 = totalPoints + Mathf.RoundToInt(checkedWaves) * 100;
					int num3 = 0;
					while (num2 >= 1000)
					{
						num3++;
						num2 -= 1000;
					}
					if (num3 > 0)
					{
						if (num2 < 10)
						{
							pointsText.text = "+" + num3 + ",00" + num2 + "<color=orange>P</color>";
						}
						else if (num2 < 100)
						{
							pointsText.text = "+" + num3 + ",0" + num2 + "<color=orange>P</color>";
						}
						else
						{
							pointsText.text = "+" + num3 + "," + num2 + "<color=orange>P</color>";
						}
					}
					else
					{
						pointsText.text = "+" + num2 + "<color=orange>P</color>";
					}
				}
				waveText.text = Mathf.FloorToInt(checkedWaves) + $"\n<color=#616161><size=20>{CalculatePerc(savedWaves)}%</size></color>";
			}
		}
		else if (countKills)
		{
			if ((float)savedKills >= checkedKills)
			{
				if ((float)savedKills > checkedKills)
				{
					checkedKills += Time.unscaledDeltaTime * 20f + Time.unscaledDeltaTime * ((float)savedKills - checkedKills) * 1.5f;
				}
				if (checkedKills >= (float)savedKills || skipping)
				{
					checkedKills = savedKills;
					countKills = false;
					killsText.GetComponent<AudioSource>().Stop();
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
				}
				killsText.text = checkedKills.ToString("0");
			}
		}
		else if (countStyle && (float)savedStyle >= checkedStyle)
		{
			_ = checkedStyle;
			if ((float)savedStyle > checkedStyle)
			{
				checkedStyle += Time.unscaledDeltaTime * 2500f + Time.unscaledDeltaTime * ((float)savedStyle - checkedStyle) * 1.5f;
			}
			if (checkedStyle >= (float)savedStyle || skipping)
			{
				checkedStyle = savedStyle;
				countStyle = false;
				styleText.GetComponent<AudioSource>().Stop();
				StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
				totalPoints += savedStyle;
				PointsShow();
			}
			else
			{
				int num4 = totalPoints + Mathf.RoundToInt(checkedStyle);
				int num5 = 0;
				while (num4 >= 1000)
				{
					num5++;
					num4 -= 1000;
				}
				if (num5 > 0)
				{
					if (num4 < 10)
					{
						pointsText.text = "+" + num5 + ",00" + num4 + "<color=orange>P</color>";
					}
					else if (num4 < 100)
					{
						pointsText.text = "+" + num5 + ",0" + num4 + "<color=orange>P</color>";
					}
					else
					{
						pointsText.text = "+" + num5 + "," + num4 + "<color=orange>P</color>";
					}
				}
				else
				{
					pointsText.text = "+" + num4 + "<color=orange>P</color>";
				}
			}
			styleText.text = checkedStyle.ToString("0");
		}
		if (flashFade)
		{
			flashColor.a = Mathf.MoveTowards(flashColor.a, 0f, Time.unscaledDeltaTime * 0.5f);
			flashPanel.color = flashColor;
			if (flashColor.a <= 0f)
			{
				flashFade = false;
			}
		}
		if (!gameOver)
		{
			return;
		}
		if (timeController == null)
		{
			timeController = MonoSingleton<TimeController>.Instance;
		}
		if (opm == null)
		{
			opm = MonoSingleton<OptionsManager>.Instance;
		}
		if (opm.paused && !wasPaused)
		{
			wasPaused = true;
		}
		else if (!opm.paused && wasPaused)
		{
			wasPaused = false;
			MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allPitch", 0f);
			MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allVolume", 0.5f);
			MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("allPitch", 0f);
			MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("allVolume", 0.5f);
		}
		if (sman.majorUsed || MonoSingleton<AssistController>.Instance.cheatsEnabled || MonoSingleton<EndlessGrid>.Instance.customPatternMode)
		{
			highScoresDisplayed = true;
		}
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame) && complete && !opm.paused)
		{
			if (highScoresDisplayed)
			{
				SceneManager.LoadScene("Endless");
				return;
			}
			highScoresDisplayed = true;
			GameObject[] array = previousElements;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			highScoreElement.SetActive(value: true);
			FetchTheScores();
		}
		else if (timeController.timeScale <= 0f && !MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame) && !complete && !opm.paused)
		{
			skipping = true;
			timeBetween = 0.01f;
		}
	}

	private int CalculatePerc(float value)
	{
		return Mathf.FloorToInt((value - (float)Mathf.FloorToInt(value)) * 100f);
	}

	private async void FetchTheScores()
	{
		int difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		LeaderboardEntry[] array = await SteamController.Instance.GetCyberGrindScores(difficulty, friendsOnly: true);
		if (!template)
		{
			return;
		}
		int num = 1;
		LeaderboardEntry[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			LeaderboardEntry leaderboardEntry = array2[i];
			Text text = tUsername;
			Friend user = leaderboardEntry.User;
			text.text = TruncateUsername(user.Name, 18);
			tScore.text = Mathf.FloorToInt((float)leaderboardEntry.Score / 1000f).ToString();
			tPercent.text = $"<color=#616161>{CalculatePerc((float)leaderboardEntry.Score / 1000f)}%</color>";
			tRank.text = num.ToString();
			GameObject gameObject = Object.Instantiate(template, friendContainer.transform);
			FetchAvatar(gameObject.GetComponentInChildren<RawImage>(), leaderboardEntry.User);
			gameObject.SetActive(value: true);
			num++;
		}
		friendPlaceholder.SetActive(value: false);
		friendContainer.SetActive(value: true);
		array = await SteamController.Instance.GetCyberGrindScores(difficulty, friendsOnly: false);
		if ((bool)template)
		{
			array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				LeaderboardEntry leaderboardEntry2 = array2[i];
				Text text2 = tUsername;
				Friend user = leaderboardEntry2.User;
				text2.text = TruncateUsername(user.Name, 18);
				tScore.text = Mathf.FloorToInt((float)leaderboardEntry2.Score / 1000f).ToString();
				tPercent.text = $"<color=#616161>{CalculatePerc((float)leaderboardEntry2.Score / 1000f)}%</color>";
				Text text3 = tRank;
				int globalRank = leaderboardEntry2.GlobalRank;
				text3.text = globalRank.ToString();
				GameObject gameObject2 = Object.Instantiate(template, globalContainer.transform);
				FetchAvatar(gameObject2.GetComponentInChildren<RawImage>(), leaderboardEntry2.User);
				gameObject2.SetActive(value: true);
			}
			globalPlaceholder.SetActive(value: false);
			globalContainer.SetActive(value: true);
		}
	}

	private static string TruncateUsername(string value, int maxChars)
	{
		if (value.Length > maxChars)
		{
			return value.Substring(0, maxChars);
		}
		return value;
	}

	private async void FetchAvatar(RawImage target, Friend user)
	{
		Steamworks.Data.Image? image = await user.GetMediumAvatarAsync();
		if ((bool)template && image.HasValue)
		{
			Texture2D texture2D = new Texture2D((int)image.Value.Width, (int)image.Value.Height, TextureFormat.RGBA32, mipChain: false);
			texture2D.LoadRawTextureData(image.Value.Data);
			texture2D.Apply();
			target.texture = texture2D;
		}
	}

	public void Appear()
	{
		if (i < toAppear.Length)
		{
			if (skipping)
			{
				HudOpenEffect component = toAppear[i].GetComponent<HudOpenEffect>();
				if (component != null)
				{
					component.skip = true;
				}
			}
			if (toAppear[i] == timeText.gameObject)
			{
				if (skipping)
				{
					checkedSeconds = savedTime;
					seconds = savedTime;
					minutes = 0f;
					while (seconds >= 60f)
					{
						seconds -= 60f;
						minutes++;
					}
					timeText.GetComponent<AudioSource>().playOnAwake = false;
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
					timeText.text = minutes + ":" + seconds.ToString("00.000");
				}
				else
				{
					countTime = true;
				}
			}
			else if (toAppear[i] == killsText.gameObject)
			{
				if (skipping)
				{
					checkedKills = savedKills;
					killsText.GetComponent<AudioSource>().playOnAwake = false;
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
					killsText.text = checkedKills.ToString("0");
				}
				else
				{
					countKills = true;
				}
			}
			else if (toAppear[i] == waveText.gameObject)
			{
				if (skipping)
				{
					checkedWaves = savedWaves;
					waveText.GetComponent<AudioSource>().playOnAwake = false;
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
					waveText.text = Mathf.FloorToInt(savedWaves) + $"\n<color=#616161><size=20>{CalculatePerc(savedWaves)}%</size></color>";
				}
				else
				{
					countWaves = true;
				}
			}
			else if (toAppear[i] == styleText.gameObject)
			{
				if (skipping)
				{
					checkedStyle = savedStyle;
					styleText.text = checkedStyle.ToString("0");
					styleText.GetComponent<AudioSource>().playOnAwake = false;
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
					totalPoints += savedStyle;
					PointsShow();
				}
				else
				{
					countStyle = true;
				}
			}
			else
			{
				StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween));
			}
			toAppear[i].gameObject.SetActive(value: true);
			i++;
		}
		else
		{
			if (newBest)
			{
				GameObject gameObject = bestWaveText.transform.parent.parent.parent.GetChild(1).gameObject;
				FlashPanel(gameObject);
				gameObject.GetComponent<AudioSource>().Play();
				bestWaveText.text = waveText.text;
				bestKillsText.text = killsText.text;
				bestStyleText.text = styleText.text;
				bestTimeText.text = timeText.text;
			}
			if (!complete)
			{
				complete = true;
				GameProgressSaver.AddMoney(totalPoints);
			}
		}
	}

	public void FlashPanel(GameObject panel)
	{
		if (flashFade)
		{
			flashColor.a = 0f;
			flashPanel.color = flashColor;
		}
		flashPanel = panel.GetComponent<UnityEngine.UI.Image>();
		flashColor = flashPanel.color;
		flashColor.a = 1f;
		flashPanel.color = flashColor;
		flashFade = true;
	}

	private void PointsShow()
	{
		int num = totalPoints;
		int num2 = 0;
		while (num >= 1000)
		{
			num2++;
			num -= 1000;
		}
		if (num2 > 0)
		{
			if (num < 10)
			{
				pointsText.text = "+" + num2 + ",00" + num + "<color=orange>P</color>";
			}
			else if (num < 100)
			{
				pointsText.text = "+" + num2 + ",0" + num + "<color=orange>P</color>";
			}
			else
			{
				pointsText.text = "+" + num2 + "," + num + "<color=orange>P</color>";
			}
		}
		else
		{
			pointsText.text = "+" + num + "<color=orange>P</color>";
		}
	}

	private IEnumerator InvokeRealtimeCoroutine(UnityAction action, float seconds)
	{
		yield return new WaitForSecondsRealtime(seconds);
		action?.Invoke();
	}
}
