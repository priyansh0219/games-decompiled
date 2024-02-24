using System.Collections;

public interface PlatformServices
{
	void Shutdown();

	void RegisterOnQuitBehaviour(IOnQuitBehaviour behaviour);

	void DeregisterOnQuitBehaviour(IOnQuitBehaviour behaviour);

	string GetName();

	UserStorage GetUserStorage();

	IEconomyItems GetEconomyItems();

	string GetUserMusicPath();

	void UnlockAchievement(GameAchievements.Id id);

	void ResetAchievements();

	bool GetSupportsSharingScreenshots();

	bool ShareScreenshot(string fileName);

	string GetUserId();

	string GetUserName();

	bool GetSupportsDynamicLogOn();

	bool IsUserLoggedIn();

	PlatformServicesUtils.AsyncOperation LogOnUserAsync(int gamepadIndex);

	void LogOffUser();

	void ShowHelp();

	bool GetSupportsVirtualKeyboard();

	bool ShowVirtualKeyboard(string title, string defaultText, PlatformServicesUtils.VirtualKeyboardFinished callback, int characterLimit = -1);

	void SetRichPresence(string presenceKey);

	void ShowUGCRestrictionMessageIfNecessary();

	IEnumerator TryEnsureServerAccessAsync(bool onUserInput = false);

	bool CanAccessServers();

	bool CanAccessUGC();

	string GetRichPresence();

	void OpenURL(string url, bool overlay = false);

	void Update();

	int GetActiveController();

	bool ReconnectController(int gamepadIndex);

	void SetUseFastLoadMode(bool useFastLoadMode);

	DisplayOperationMode GetCurrentDisplayOperationMode();

	bool GetDisplayOutOfSpaceMessage();

	float GetDefaultUiScale(DisplayOperationMode displayOperationMode);

	bool GetDevToolsEnabled();

	void SetDevToolsEnabled(bool enabled);

	string GetDefaultNewsUrl();
}
