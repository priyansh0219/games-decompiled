using System;
using System.Collections;
using System.Text;
using Gendarme;
using Platform.IO;
using UWE;
using UnityEngine;

public class AutomatedProfiler : MonoBehaviour
{
	private bool bSceneLoadedForSmokeTest;

	public static bool IsActive()
	{
		return CommandLine.GetFlag("-phototour");
	}

	private IEnumerator Start()
	{
		while (uGUI_MainMenu.main == null)
		{
			yield return null;
		}
		bool flag = false;
		bool bWantsStopwatch = false;
		bool bWantsScreenshots = false;
		string outDir = SNUtils.GetDevTempPath();
		string spec = "default";
		string tourFileName = "ssperf.tour";
		string saveName = "stopwatchSave";
		int runcount = 1;
		bool abtest = false;
		bool detailed = false;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		int num = ((commandLineArgs != null) ? commandLineArgs.Length : 0);
		for (int i = 0; i < num; i++)
		{
			string text = commandLineArgs[i];
			if (text.Equals("-phototour"))
			{
				if (i + 1 < num)
				{
					string text2 = commandLineArgs[i + 1];
					tourFileName = text2;
					if (!tourFileName.Contains(".tour"))
					{
						tourFileName += ".tour";
					}
					flag = true;
				}
			}
			else if (text.Equals("-stopwatch"))
			{
				bWantsStopwatch = true;
			}
			else if (text.Equals("-takescreens"))
			{
				bWantsScreenshots = true;
			}
			else if (text.Equals("-outdir"))
			{
				if (i + 1 < num)
				{
					outDir = commandLineArgs[i + 1];
				}
			}
			else if (text.Equals("-spec"))
			{
				if (i + 1 < num)
				{
					spec = commandLineArgs[i + 1];
				}
			}
			else if (text.Equals("-loadsave"))
			{
				if (i + 1 < num)
				{
					saveName = commandLineArgs[i + 1];
				}
			}
			else if (text.Equals("-runcount"))
			{
				if (i + 1 < num)
				{
					runcount = int.Parse(commandLineArgs[i + 1]);
				}
			}
			else if (text.Equals("-delaytime"))
			{
				if (i + 1 < num)
				{
					float.Parse(commandLineArgs[i + 1]);
				}
			}
			else if (text.Equals("-abtest"))
			{
				abtest = true;
			}
			else if (text.Equals("-detailed"))
			{
				detailed = true;
			}
		}
		if (!flag)
		{
			yield break;
		}
		bool flag2 = true;
		string tourFile = SNUtils.InsideUnmanaged("phototours/" + tourFileName);
		if (!File.Exists(tourFile))
		{
			string message = "AutoProfileManager can't find phototour:  " + tourFile;
			ErrorMessage.AddDebug(message);
			Debug.LogWarning(message);
			flag2 = false;
		}
		if (string.IsNullOrEmpty(saveName))
		{
			ErrorMessage.AddDebug("AutoProfileManager needs a save to load.  Specify with the -loadsave switch");
			Debug.LogWarning("AutoProfileManager needs a save to load.  Specify with the -loadsave switch");
			flag2 = false;
		}
		else if (SaveLoadManager.main.GetGameInfo(saveName) == null)
		{
			string message2 = $"AutoProfileManager can't find save: {saveName}";
			ErrorMessage.AddDebug(message2);
			Debug.LogWarning(message2);
			flag2 = false;
		}
		if (outDir == null || outDir == "")
		{
			outDir = ".";
		}
		else if (!Directory.Exists(outDir))
		{
			Directory.CreateDirectory(outDir);
		}
		if (!flag2)
		{
			yield break;
		}
		if (spec == "min")
		{
			GraphicsUtil.SetQualityLevel(0);
			Screen.SetResolution(1280, 720, fullscreen: true);
		}
		else if (spec == "high")
		{
			spec = "high";
			GraphicsUtil.SetQualityLevel(2);
			Screen.SetResolution(1920, 1080, fullscreen: true);
		}
		else
		{
			spec = "rec";
			GraphicsUtil.SetQualityLevel(1);
			Screen.SetResolution(1920, 1080, fullscreen: true);
		}
		Debug.Log("---About to load from save: " + saveName);
		yield return StartCoroutine(LoadSceneForSmokeTest(saveName));
		if (bSceneLoadedForSmokeTest)
		{
			Debug.Log("---Scene loaded.");
			string tourID = tourFileName;
			if (tourFileName.Contains("."))
			{
				int length = tourFileName.IndexOf(".");
				tourID = tourID.Substring(0, length);
			}
			tourID = "phototour_" + tourID + "_" + spec;
			PhotoTour.main.onPlaybackDone += delegate
			{
				int num2 = runcount;
				runcount = num2 - 1;
				if (runcount > 0)
				{
					StopwatchProfiler.Instance.StopRecording();
					PhotoTour.main.PlayFile(tourFile, "", outDir);
					if (bWantsStopwatch)
					{
						StopwatchProfiler.Instance.SetSettingsReportString(GetCurrentQualityOptionsCSV());
						StopwatchProfiler.Instance.StartRecording(outDir, tourID, 5f, saveName);
					}
				}
				else
				{
					StopwatchProfiler.Instance.StopRecordingAndCloseSession();
					Application.Quit();
				}
			};
			PhotoTour.main.bScreenShotsAllowed = bWantsScreenshots;
			PhotoTour.main.PlayFile(tourFile, "", outDir);
			GameModeUtils.ActivateCheat(GameModeOption.NoOxygen | GameModeOption.NoAggression);
			DamageSystem.damageMultiplier = 0f;
			if (bWantsStopwatch)
			{
				StopwatchProfiler.Instance.ABTestingEnabled = abtest;
				if (detailed)
				{
					StopwatchProfiler.Instance.SetCategoryDetailed();
				}
				else
				{
					StopwatchProfiler.Instance.SetCategoryMinimal();
				}
				StopwatchProfiler.Instance.SetSettingsReportString(GetCurrentQualityOptionsCSV());
				StopwatchProfiler.Instance.StartRecording(outDir, tourID, 5f);
			}
		}
		else
		{
			string message3 = "SmokeTest / Failed to load save named: " + saveName;
			Debug.LogWarning(message3);
			ErrorMessage.AddDebug(message3);
		}
	}

	private IEnumerator LoadSceneForSmokeTest(string saveName)
	{
		while (SaveLoadManager.main == null)
		{
			yield return null;
		}
		Debug.Log("LoadSceneForSmokeTest / SaveLoadManager available - trying to load: " + saveName);
		SaveLoadManager.GameInfo gameInfo = SaveLoadManager.main.GetGameInfo(saveName);
		int changeSet = gameInfo.changeSet;
		GameMode gameMode = gameInfo.gameMode;
		Utils.SetContinueMode(mode: true);
		Utils.SetLegacyGameMode(gameMode);
		SaveLoadManager.main.SetCurrentSlot(Path.GetFileName(saveName));
		VRLoadingOverlay.Show();
		Debug.Log("LoadSceneForSmokeTest / About to call LoadAsync.");
		CoroutineTask<SaveLoadManager.LoadResult> task = SaveLoadManager.main.LoadAsync();
		yield return task;
		if (!task.GetResult().success)
		{
			Debug.Log("LoadSceneForSmokeTest / LoadAsync task failure.");
			bSceneLoadedForSmokeTest = false;
			yield break;
		}
		Debug.Log("LoadSceneForSmokeTest / LoadAsync success!");
		BatchUpgrade.UpgradeBatches(changeSet);
		Debug.Log("LoadSceneForSmokeTest / Batches upgraded.");
		FPSInputModule.SelectGroup(null);
		yield return MainSceneLoading.Launch();
		while (WaitScreen.IsWaiting)
		{
			yield return CoroutineUtils.waitForNextFrame;
		}
		bSceneLoadedForSmokeTest = true;
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public static string GetCurrentQualityOptionsCSV()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Resolution,{0}x{1}\n", Screen.currentResolution.width, Screen.currentResolution.height);
		stringBuilder.AppendFormat("QualitySettings,{0}\n", QualitySettings.GetQualityLevel());
		stringBuilder.AppendFormat("CameraFOV,{0}\n", MiscSettings.fieldOfView);
		stringBuilder.AppendFormat("AntiAliasingMode,{0}\n", UwePostProcessingManager.GetAaMode());
		stringBuilder.AppendFormat("AntiAliasingQuality,{0}\n", UwePostProcessingManager.GetAaQuality());
		stringBuilder.AppendFormat("AmbientOcclusion,{0}\n", UwePostProcessingManager.GetAoQuality());
		stringBuilder.AppendFormat("ColorGradingMode,{0}\n", UwePostProcessingManager.GetColorGradingMode());
		stringBuilder.AppendFormat("SSRQuality,{0}\n", UwePostProcessingManager.GetSsrQuality());
		stringBuilder.AppendFormat("Bloom,{0}\n", UwePostProcessingManager.GetBloomEnabled());
		stringBuilder.AppendFormat("BloomLensDirt,{0}\n", UwePostProcessingManager.GetBloomLensDirtEnabled());
		stringBuilder.AppendFormat("DepthOfField,{0}\n", UwePostProcessingManager.GetDofEnabled());
		stringBuilder.AppendFormat("MotionBlurQuality,{0}\n", UwePostProcessingManager.GetMotionBlurQuality());
		stringBuilder.AppendFormat("Dithering,{0}\n", UwePostProcessingManager.GetDitheringEnabled());
		return stringBuilder.ToString();
	}
}
