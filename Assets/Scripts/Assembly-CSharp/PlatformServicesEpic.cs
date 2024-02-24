using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class PlatformServicesEpic : PlatformServices
{
	private UserStorage userStorage;

	private string userId;

	private const string defaultNewsUrl = "https://subnautica.unknownworlds.com/api/news/pc-new";

	private bool devToolsEnabled = true;

	public event PlatformServicesUtils.AchievementsChanged OnAchievementsChanged;

	public PlatformServicesEpic()
	{
		string savePath = Path.Combine(Application.persistentDataPath, "Subnautica/SavedGames");
		userStorage = new UserStoragePC(savePath);
	}

	public static bool IsPresent()
	{
		return Array.IndexOf(Environment.GetCommandLineArgs(), "-EpicPortal") != -1;
	}

	public bool Initialize()
	{
		Match match = Regex.Match(Environment.CommandLine, "-epicuserid\\s*=\\s*(\\S+)");
		if (match.Success)
		{
			userId = match.Groups[1].Value;
			Debug.LogFormat("User id = {0}", userId);
		}
		return true;
	}

	public void Shutdown()
	{
	}

	public void RegisterOnQuitBehaviour(IOnQuitBehaviour behaviour)
	{
	}

	public void DeregisterOnQuitBehaviour(IOnQuitBehaviour behaviour)
	{
	}

	public string GetName()
	{
		return "Epic";
	}

	public UserStorage GetUserStorage()
	{
		return userStorage;
	}

	public IEconomyItems GetEconomyItems()
	{
		return null;
	}

	public string GetUserMusicPath()
	{
		return PlatformServicesUtils.GetDesktopUserMusicPath();
	}

	public void UnlockAchievement(GameAchievements.Id id)
	{
		Debug.LogFormat("Achievement Unlocked: {0}", id);
	}

	public bool GetAchievementUnlocked(GameAchievements.Id id)
	{
		return false;
	}

	public void ResetAchievements()
	{
	}

	public bool GetSupportsSharingScreenshots()
	{
		return false;
	}

	public bool ShareScreenshot(string fileName)
	{
		return false;
	}

	public bool IsUserLoggedIn()
	{
		return true;
	}

	public string GetUserId()
	{
		return userId;
	}

	public string GetUserName()
	{
		return null;
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
		return false;
	}

	public bool ShowVirtualKeyboard(string title, string defaultText, PlatformServicesUtils.VirtualKeyboardFinished callback, int characterLimit = -1)
	{
		return false;
	}

	public void SetRichPresence(string presenceKey)
	{
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
		return string.Empty;
	}

	public void OpenURL(string url, bool overlay = false)
	{
		PlatformServicesUtils.DefaultOpenURL(url);
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
