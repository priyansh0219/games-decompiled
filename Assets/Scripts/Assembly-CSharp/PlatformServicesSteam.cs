using System;
using System.Collections;
using System.IO;
using System.Text;
using Steamworks;
using UnityEngine;

public class PlatformServicesSteam : PlatformServices
{
	public sealed class CallbackHost : MonoBehaviour
	{
		private void Update()
		{
			SteamAPI.RunCallbacks();
		}
	}

	private readonly AppId_t appId = new AppId_t(264710u);

	private UserStoragePC userStoragePC;

	private IEconomyItems economyItems;

	private GameObject callbackHost;

	private CGameID gameID;

	private SteamAPIWarningMessageHook_t warningMessageHook;

	private Callback<UserStatsReceived_t> userStatsReceived;

	private Callback<UserStatsStored_t> userStatsStored;

	private Callback<UserAchievementStored_t> userAchievementStored;

	public string RichPresenceStr = string.Empty;

	private const string defaultNewsUrl = "https://subnautica.unknownworlds.com/api/news/pc-new";

	protected PlatformServicesUtils.VirtualKeyboardFinished onVirtualKeyboardDismissed;

	protected Callback<GamepadTextInputDismissed_t> gamepadTextInputDismissed;

	private bool devToolsEnabled = true;

	protected static bool IsSteamDeck => SteamUtils.IsSteamRunningOnSteamDeck();

	protected static bool IsBigPictureMode => SteamUtils.IsSteamInBigPictureMode();

	public event PlatformServicesUtils.AchievementsChanged OnAchievementsChanged;

	public static bool IsPresent()
	{
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsPlayer:
			return File.Exists($"{Application.dataPath}/Plugins/x86_64/steam_api64.dll");
		case RuntimePlatform.OSXPlayer:
			return Directory.Exists($"{Application.dataPath}/Plugins/steam_api.bundle");
		default:
			Debug.LogWarningFormat("Unhandled platform {0} when checking for Steam library", Application.platform);
			return false;
		}
	}

	public IEnumerator InitializeAsync()
	{
		if (!Packsize.Test())
		{
			Debug.LogWarning("The wrong version of Steamworks.NET is being run in this platform.");
			Application.Quit();
			yield break;
		}
		if (!DllCheck.Test())
		{
			Debug.LogWarning("One or more of the Steamworks binaries seems to be the wrong version.");
			Application.Quit();
			yield break;
		}
		RestartInSteam();
		if (!SteamAPI.Init())
		{
			Debug.LogWarning("Couldn't initialize Steamworks");
			Application.Quit();
			yield break;
		}
		warningMessageHook = DebugTextHook;
		SteamClient.SetWarningMessageHook(warningMessageHook);
		gameID = new CGameID(SteamUtils.GetAppID());
		userStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
		userAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
		SteamUserStats.RequestCurrentStats();
		StartCallbacks();
		string fullName = Directory.GetParent(Application.dataPath).FullName;
		string savePath = Path.Combine(fullName, "SNAppData/SavedGames");
		userStoragePC = new UserStoragePC(savePath);
		yield return UpgradeSaveDataAsync(userStoragePC, Path.Combine(fullName, "SNAppData/SavedGamesBackup"));
		economyItems = new EconomyItemsSteam(GetUserId());
	}

	private IEnumerator UpgradeSaveDataAsync(UserStoragePC userStorage, string backupPath = null)
	{
		UserStorageUtils.UpgradeOperation upgradeOperation = userStorage.UpgradeSaveDataAsync(backupPath);
		if (upgradeOperation.itemsTotal > 0)
		{
			while (Language.main == null)
			{
				yield return null;
			}
			string migratePrompt = Language.main.Get("SaveDataUpdate");
			while (!uGUI.isInitialized)
			{
				yield return null;
			}
			uGUI.main.progressBar.Show(migratePrompt, upgradeProgress);
			HideStartScreenDisclaimer();
		}
		yield return upgradeOperation;
		if (!uGUI.isInitialized)
		{
			yield break;
		}
		uGUI.main.progressBar.Close();
		if (!upgradeOperation.GetSuccessful())
		{
			while (Language.main == null)
			{
				yield return null;
			}
			string format = Language.main.GetFormat("SaveDataUpdateErrorFormat", upgradeOperation.errorMessage);
			uGUI.main.confirmation.Show(format);
			while (uGUI.main.confirmation.focused)
			{
				yield return null;
			}
		}
		float upgradeProgress()
		{
			if (upgradeOperation.itemsTotal == 0)
			{
				return 0f;
			}
			return (float)(upgradeOperation.itemsPrecessed + 1) / (float)upgradeOperation.itemsTotal;
		}
	}

	private void HideStartScreenDisclaimer()
	{
		UnityEngine.Object.FindObjectOfType<StartScreen>().HideDisclaimer();
	}

	public void Shutdown()
	{
		StopCallbacks();
		SteamAPI.Shutdown();
	}

	public void RegisterOnQuitBehaviour(IOnQuitBehaviour behaviour)
	{
	}

	public void DeregisterOnQuitBehaviour(IOnQuitBehaviour behaviour)
	{
	}

	public string GetName()
	{
		return "Steam";
	}

	public UserStorage GetUserStorage()
	{
		return userStoragePC;
	}

	public IEconomyItems GetEconomyItems()
	{
		return economyItems;
	}

	public string GetUserMusicPath()
	{
		return PlatformServicesUtils.GetDesktopUserMusicPath();
	}

	private void StartCallbacks()
	{
		callbackHost = new GameObject();
		callbackHost.hideFlags = HideFlags.HideInHierarchy;
		UnityEngine.Object.DontDestroyOnLoad(callbackHost);
		callbackHost.AddComponent<CallbackHost>();
		callbackHost.AddComponent<SceneCleanerPreserve>();
	}

	private void StopCallbacks()
	{
		UnityEngine.Object.Destroy(callbackHost);
	}

	private void RestartInSteam()
	{
		try
		{
			if (SteamAPI.RestartAppIfNecessary(appId))
			{
				Debug.LogWarning("Missing Steam DRM");
				Application.Quit();
			}
		}
		catch (DllNotFoundException)
		{
			Debug.LogFormat("Could not load [lib]steam_api.dll/so/dylib");
			Application.Quit();
		}
	}

	public void UnlockAchievement(GameAchievements.Id id)
	{
		string platformId = GameAchievements.GetPlatformId(id);
		if (platformId != null)
		{
			SteamUserStats.SetAchievement(platformId);
			SteamUserStats.StoreStats();
		}
	}

	public bool GetAchievementUnlocked(GameAchievements.Id id)
	{
		string platformId = GameAchievements.GetPlatformId(id);
		bool pbAchieved = false;
		if (platformId != null)
		{
			SteamUserStats.GetAchievement(platformId, out pbAchieved);
		}
		return pbAchieved;
	}

	public void ResetAchievements()
	{
		SteamUserStats.ResetAllStats(bAchievementsToo: true);
		SteamUserStats.RequestCurrentStats();
	}

	public bool GetSupportsSharingScreenshots()
	{
		return true;
	}

	public bool ShareScreenshot(string fileName)
	{
		if (File.Exists(fileName))
		{
			SteamScreenshots.AddScreenshotToLibrary(fileName, "", Screen.width, Screen.height);
			return true;
		}
		return false;
	}

	public bool IsUserLoggedIn()
	{
		return true;
	}

	private CSteamID GetSteamUserId()
	{
		if (SteamUser.BLoggedOn())
		{
			CSteamID steamID = SteamUser.GetSteamID();
			if (steamID.IsValid())
			{
				return steamID;
			}
		}
		return CSteamID.Nil;
	}

	public string GetUserId()
	{
		return GetSteamUserId().ToString();
	}

	public string GetUserName()
	{
		if (!SteamUser.BLoggedOn())
		{
			return "UnityEditorPlayer";
		}
		if (!SteamUser.GetSteamID().IsValid())
		{
			return "UnityEditorPlayer";
		}
		return SteamFriends.GetPersonaName();
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if ((ulong)gameID != pCallback.m_nGameID)
		{
			return;
		}
		if (EResult.k_EResultOK == pCallback.m_eResult)
		{
			if (this.OnAchievementsChanged != null)
			{
				this.OnAchievementsChanged();
			}
		}
		else
		{
			Debug.Log("Steamworks: RequestStats - failed, " + pCallback.m_eResult);
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		if ((ulong)gameID == pCallback.m_nGameID)
		{
			if (pCallback.m_nMaxProgress == 0)
			{
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
				return;
			}
			Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
		}
	}

	private static void DebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	public bool GetSupportsDynamicLogOn()
	{
		return false;
	}

	public PlatformServicesUtils.AsyncOperation LogOnUserAsync(int gamepadIndex)
	{
		return null;
	}

	public void LogOffUser()
	{
	}

	public void ShowHelp()
	{
	}

	public bool GetSupportsVirtualKeyboard()
	{
		return IsBigPictureMode;
	}

	protected void OnGamepadTextInputDismissed(GamepadTextInputDismissed_t pCallback)
	{
		if (onVirtualKeyboardDismissed != null)
		{
			bool success = false;
			bool cancelled = true;
			string pchText = string.Empty;
			if (pCallback.m_bSubmitted)
			{
				success = SteamUtils.GetEnteredGamepadTextInput(out pchText, pCallback.m_unSubmittedText);
				cancelled = false;
			}
			try
			{
				onVirtualKeyboardDismissed(success, cancelled, pchText);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			onVirtualKeyboardDismissed = null;
		}
	}

	public bool ShowVirtualKeyboard(string title, string defaultText, PlatformServicesUtils.VirtualKeyboardFinished callback, int characterLimit = -1)
	{
		if (GetSupportsVirtualKeyboard())
		{
			if (gamepadTextInputDismissed == null)
			{
				gamepadTextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(OnGamepadTextInputDismissed);
			}
			onVirtualKeyboardDismissed = callback;
			uint unCharMax = ((characterLimit <= 0) ? 65536u : ((uint)characterLimit));
			return SteamUtils.ShowGamepadTextInput(EGamepadTextInputMode.k_EGamepadTextInputModeNormal, EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine, title, unCharMax, defaultText);
		}
		return false;
	}

	public void SetRichPresence(string presenceKey)
	{
		RichPresenceStr = Language.main.GetFormat(presenceKey);
	}

	public void ShowUGCRestrictionMessageIfNecessary()
	{
	}

	public IEnumerator TryEnsureServerAccessAsync(bool onUserInput = false)
	{
		yield break;
	}

	public bool CanAccessServers()
	{
		return true;
	}

	public bool CanAccessUGC()
	{
		return true;
	}

	public string GetRichPresence()
	{
		return RichPresenceStr;
	}

	public void OpenURL(string url, bool overlay = false)
	{
		if (overlay)
		{
			SteamFriends.ActivateGameOverlayToWebPage(url);
		}
		else
		{
			PlatformServicesUtils.DefaultOpenURL(url);
		}
	}

	public void Update()
	{
	}

	public int GetActiveController()
	{
		return -1;
	}

	public bool ReconnectController(int gamepadIndex)
	{
		return true;
	}

	public void SetUseFastLoadMode(bool useFastMode)
	{
	}

	public DisplayOperationMode GetCurrentDisplayOperationMode()
	{
		return DisplayOperationMode.Default;
	}

	public bool GetDisplayOutOfSpaceMessage()
	{
		return true;
	}

	public float GetDefaultUiScale(DisplayOperationMode displayOperationMode)
	{
		return 1f;
	}

	public bool GetDevToolsEnabled()
	{
		return devToolsEnabled;
	}

	public void SetDevToolsEnabled(bool enabled)
	{
		devToolsEnabled = enabled;
	}

	public string GetDefaultNewsUrl()
	{
		return "https://subnautica.unknownworlds.com/api/news/pc-new";
	}
}
