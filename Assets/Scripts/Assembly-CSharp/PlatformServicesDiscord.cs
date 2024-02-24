using System.Collections;
using System.IO;
using Discord;
using UnityEngine;

public class PlatformServicesDiscord : PlatformServices
{
	public sealed class CallbackHost : MonoBehaviour
	{
		private global::Discord.Discord discord;

		public void Initialize(global::Discord.Discord _discord)
		{
			discord = _discord;
		}

		private void Update()
		{
			discord.RunCallbacks();
		}
	}

	private readonly long applicationId = 363413811518242816L;

	private global::Discord.Discord discord;

	private GameObject callbackHostObject;

	private ActivityManager activityManager;

	private UserManager userManager;

	private UserStorage userStorage;

	private string userId;

	private string userName;

	private const string defaultNewsUrl = "https://subnautica.unknownworlds.com/api/news/pc-new";

	private bool devToolsEnabled = true;

	public event PlatformServicesUtils.AchievementsChanged OnAchievementsChanged;

	public static bool IsPresent()
	{
		return PlatformServicesUtils.IsRuntimePluginDllPresent("discord_game_sdk");
	}

	public IEnumerator InitializeAsync()
	{
		discord = new global::Discord.Discord(applicationId, 0uL);
		discord.SetLogHook(LogLevel.Debug, delegate(LogLevel level, string message)
		{
			Debug.LogFormat("Discord: [{0}] {1}", level, message);
		});
		ApplicationManager applicationManager = discord.GetApplicationManager();
		activityManager = discord.GetActivityManager();
		userManager = discord.GetUserManager();
		userManager.OnCurrentUserUpdate += OnCurrentUserUpdate;
		string savePath = Path.Combine(Application.persistentDataPath, "Subnautica/SavedGames");
		userStorage = new UserStoragePC(savePath);
		StartCallbacks();
		while (userId == null)
		{
			yield return null;
		}
		Debug.LogFormat("Discord Locale: {0}", applicationManager.GetCurrentLocale());
		Debug.LogFormat("Discord Branch: {0}", applicationManager.GetCurrentBranch());
		Debug.LogFormat("Discord User: {0} ({1})", GetUserName(), GetUserId());
	}

	private void OnCurrentUserUpdate()
	{
		try
		{
			User currentUser = userManager.GetCurrentUser();
			userId = currentUser.Id.ToString();
			userName = currentUser.Username;
		}
		catch
		{
			userId = "0";
			userName = "DiscordUser";
		}
	}

	public string GetName()
	{
		return "Discord";
	}

	public bool GetSupportsDynamicLogOn()
	{
		return false;
	}

	public bool GetSupportsSharingScreenshots()
	{
		return false;
	}

	public string GetUserId()
	{
		return userId;
	}

	public string GetUserName()
	{
		return userName;
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

	public bool IsUserLoggedIn()
	{
		return true;
	}

	public void LogOffUser()
	{
	}

	public PlatformServicesUtils.AsyncOperation LogOnUserAsync(int gamepadIndex)
	{
		return null;
	}

	public void ResetAchievements()
	{
	}

	private static void OnActivityUpdated(Result result)
	{
	}

	public void SetRichPresence(string presenceKey)
	{
		string format = Language.main.GetFormat(presenceKey);
		if (!string.Equals(format, presenceKey))
		{
			Activity activity = default(Activity);
			activity.State = "Playing";
			activity.Details = format;
			activityManager.UpdateActivity(activity, OnActivityUpdated);
		}
	}

	public bool ShareScreenshot(string fileName)
	{
		return false;
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

	private void StartCallbacks()
	{
		callbackHostObject = new GameObject();
		callbackHostObject.hideFlags = HideFlags.HideInHierarchy;
		Object.DontDestroyOnLoad(callbackHostObject);
		callbackHostObject.AddComponent<SceneCleanerPreserve>();
		callbackHostObject.AddComponent<CallbackHost>().Initialize(discord);
	}

	private void StopCallbacks()
	{
		Object.Destroy(callbackHostObject);
	}

	public void Shutdown()
	{
		discord.Dispose();
	}

	public void RegisterOnQuitBehaviour(IOnQuitBehaviour behaviour)
	{
	}

	public void DeregisterOnQuitBehaviour(IOnQuitBehaviour behaviour)
	{
	}

	public void UnlockAchievement(GameAchievements.Id id)
	{
	}

	public bool GetAchievementUnlocked(GameAchievements.Id id)
	{
		return false;
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
