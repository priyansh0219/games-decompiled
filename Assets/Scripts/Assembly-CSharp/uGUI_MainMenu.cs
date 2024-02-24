using System;
using System.Collections;
using System.IO;
using UWE;
using UnityEngine;
using UnityEngine.SceneManagement;

public class uGUI_MainMenu : uGUI_InputGroup
{
	public static uGUI_MainMenu main;

	public MainMenuRightSide rightSide;

	public MainMenuPrimaryOptionsMenu primaryOptions;

	[AssertNotNull]
	public uGUI_CanvasScaler canvasScaler;

	[SerializeField]
	[AssertNotNull]
	private uGUI_GraphicRaycaster graphicRaycaster;

	[SerializeField]
	[AssertNotNull]
	private CanvasGroup canvasGroup;

	[Tooltip("Button that opens the Save Migration panel for moving local Windows Store save games into the cloud save space.")]
	public GameObject saveMigrationButton;

	private uGUI_INavigableIconGrid subMenu;

	private string lastGroup;

	private bool isStartingNewGame;

	private static bool hasQuickLaunched;

	[AssertLocalization]
	private const string createFailedOutOfSlotsMessage = "SaveFailedSlot";

	[AssertLocalization]
	private const string createFailedOutOfSpaceMessage = "CreateFailedSpace";

	[AssertLocalization(1)]
	private const string createFailedErrorFormat = "CreateFailedFormat";

	[AssertLocalization(1)]
	private const string loadFailedMessageFormat = "LoadFailed";

	[AssertLocalization]
	private const string loadFailedOutOfSpaceMessage = "LoadFailedSpace";

	protected override void Awake()
	{
		main = this;
		base.Awake();
		isStartingNewGame = false;
		subMenu = primaryOptions;
		uGUI_TabbedControlsPanel[] componentsInChildren = GetComponentsInChildren<uGUI_TabbedControlsPanel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].onClose = ClosePanel;
		}
	}

	private IEnumerator Start()
	{
		canvasGroup.SetVisible(visible: true);
		Select();
		PlatformServices services = PlatformUtils.main.GetServices();
		services.SetRichPresence("PresenceMainMenu");
		bool showSaveMigration = false;
		if (PlatformUtils.isWindowsStore && services is PlatformServicesWindows platformServicesWindows)
		{
			UserStorage localStorage = platformServicesWindows.GetLocalStorage();
			if (localStorage != null)
			{
				UserStorageUtils.QueryOperation task = localStorage.GetContainerNamesAsync();
				yield return task;
				if (task.GetSuccessful() && task.results.Count > 1)
				{
					showSaveMigration = true;
				}
			}
		}
		if ((bool)saveMigrationButton)
		{
			saveMigrationButton.SetActive(showSaveMigration);
		}
		yield return null;
	}

	private void StartMostRecentSaveOrNewGame()
	{
		if (!HasSavedGames())
		{
			OnButtonSurvival();
		}
		else
		{
			LoadMostRecentSavedGame();
		}
	}

	protected override void Update()
	{
	}

	public uGUI_INavigableIconGrid GetCurrentSubMenu()
	{
		return subMenu;
	}

	private bool HasSavedGames()
	{
		string[] activeSlotNames = SaveLoadManager.main.GetActiveSlotNames();
		int i = 0;
		for (int num = activeSlotNames.Length; i < num; i++)
		{
			if (SaveLoadManager.main.GetGameInfo(activeSlotNames[i]) != null)
			{
				return true;
			}
		}
		return false;
	}

	private void LoadMostRecentSavedGame()
	{
		string[] activeSlotNames = SaveLoadManager.main.GetActiveSlotNames();
		long num = 0L;
		SaveLoadManager.GameInfo gameInfo = null;
		string saveGame = string.Empty;
		int i = 0;
		for (int num2 = activeSlotNames.Length; i < num2; i++)
		{
			SaveLoadManager.GameInfo gameInfo2 = SaveLoadManager.main.GetGameInfo(activeSlotNames[i]);
			if (gameInfo2.dateTicks > num)
			{
				gameInfo = gameInfo2;
				num = gameInfo2.dateTicks;
				saveGame = activeSlotNames[i];
			}
		}
		if (gameInfo != null)
		{
			CoroutineHost.StartCoroutine(LoadGameAsync(saveGame, gameInfo.session, gameInfo.changeSet, gameInfo.gameMode));
		}
	}

	private IEnumerator StartNewGame(GameMode gameMode)
	{
		if (isStartingNewGame)
		{
			yield break;
		}
		isStartingNewGame = true;
		string value = SaveLoadManager.main.StartNewSession();
		using (GameAnalytics.EventData eventData = GameAnalytics.CustomEvent(GameAnalytics.Event.NewGameStarted))
		{
			eventData.Add("session_id", value);
			eventData.Add("game_mode", (int)gameMode);
		}
		PlatformUtils.main.GetServices().ShowUGCRestrictionMessageIfNecessary();
		Utils.SetContinueMode(mode: false);
		Utils.SetLegacyGameMode(gameMode);
		EnableInput(enable: false);
		CoroutineTask<SaveLoadManager.CreateResult> createSlotTask = SaveLoadManager.main.CreateSlotAsync();
		yield return createSlotTask;
		SaveLoadManager.CreateResult result = createSlotTask.GetResult();
		if (!result.success)
		{
			EnableInput(enable: true);
			SaveLoadManager.Error error = result.error;
			string descriptionText;
			switch (error)
			{
			case SaveLoadManager.Error.OutOfSpace:
				descriptionText = Language.main.Get("CreateFailedSpace");
				break;
			case SaveLoadManager.Error.OutOfSlots:
				descriptionText = Language.main.Get("SaveFailedSlot");
				break;
			default:
				descriptionText = Language.main.GetFormat("CreateFailedFormat", error);
				break;
			}
			uGUI.main.confirmation.Show(descriptionText, null, this);
			isStartingNewGame = false;
		}
		else
		{
			SaveLoadManager.main.SetCurrentSlot(result.slotName);
			VRLoadingOverlay.Show();
			UserStorageUtils.AsyncOperation clearSlotTask = SaveLoadManager.main.ClearSlotAsync(result.slotName);
			yield return clearSlotTask;
			if (!clearSlotTask.GetSuccessful())
			{
				Debug.LogError("Clearing save data failed. But we ignore it.");
			}
			Deselect();
			GamepadInputModule.current.SetCurrentGrid(null);
			yield return MainSceneLoading.Launch();
		}
	}

	public IEnumerator LoadGameAsync(string saveGame, string sessionId, int changeSet, GameMode gameMode)
	{
		if (isStartingNewGame)
		{
			yield break;
		}
		isStartingNewGame = true;
		GC.Collect();
		EnableInput(enable: false);
		FPSInputModule.SelectGroup(null);
		WaitScreen.ManualWaitItem waitDummy = WaitScreen.Add("FadeInDummy");
		WaitScreen.ManualWaitItem waitSaveFilesLoad = WaitScreen.Add("SaveFilesLoad");
		using (GameAnalytics.EventData eventData = GameAnalytics.CustomEvent(GameAnalytics.Event.GameLoaded))
		{
			eventData.Add("session_id", sessionId);
			eventData.Add("game_mode", (int)gameMode);
		}
		Utils.SetContinueMode(mode: true);
		Utils.SetLegacyGameMode(gameMode);
		SaveLoadManager.main.SetCurrentSlot(Path.GetFileName(saveGame));
		VRLoadingOverlay.Show();
		CoroutineTask<SaveLoadManager.LoadResult> task = SaveLoadManager.main.LoadAsync();
		yield return task;
		WaitScreen.Remove(waitSaveFilesLoad);
		SaveLoadManager.LoadResult result = task.GetResult();
		if (!result.success)
		{
			WaitScreen.Remove(waitDummy);
			EnableInput(enable: true);
			isStartingNewGame = false;
			string descriptionText = Language.main.GetFormat("LoadFailed", result.errorMessage);
			if (result.error == SaveLoadManager.Error.OutOfSpace)
			{
				descriptionText = Language.main.Get("LoadFailedSpace");
			}
			uGUI.main.confirmation.Show(descriptionText, delegate(bool confirmed)
			{
				OnErrorConfirmed(confirmed, saveGame, sessionId, changeSet, gameMode);
			}, this);
		}
		else
		{
			BatchUpgrade.UpgradeBatches(changeSet);
			FPSInputModule.SelectGroup(null);
			yield return MainSceneLoading.Launch(waitDummy);
		}
	}

	private void OnErrorConfirmed(bool confirmed, string sessionId, string saveGame, int changeSet, GameMode gameMode)
	{
		if (confirmed)
		{
			CoroutineHost.StartCoroutine(LoadGameAsync(saveGame, sessionId, changeSet, gameMode));
		}
	}

	public void EnableInput(bool enable)
	{
		graphicRaycaster.enabled = enable;
	}

	private void ClosePanel()
	{
		ShowPrimaryOptions(show: true);
		OnHome();
	}

	public void OnButtonLoad()
	{
		rightSide.OpenGroup("SavedGames");
	}

	public void OnButtonSaveMigration()
	{
		if (PlatformUtils.isWindowsStore)
		{
			rightSide.OpenGroup("SaveMigration");
		}
	}

	public void OnButtonNew()
	{
		rightSide.OpenGroup("NewGame");
	}

	public void OnButtonHelp()
	{
		PlatformUtils.main.GetServices().ShowHelp();
	}

	public void OnButtonOptions()
	{
		ShowPrimaryOptions(show: false);
		rightSide.OpenGroup("Options");
	}

	public void OnButtonSwitchUser()
	{
		canvasGroup.SetVisible(visible: false);
		PlatformUtils.AddTemporaryCamera();
		PlatformUtils.main.LogOffUser();
		Deselect();
		SceneCleaner.Open();
	}

	public void OnButtonFeedback()
	{
		uGUI_FeedbackCollector.main.Open();
	}

	public void OnButtonStore()
	{
		PlatformUtils.OpenURL("https://unknownworlds.com/sn_store");
	}

	public void OnButtonCredits()
	{
		MainMenuMusic.Stop();
		StartCoroutine(LoadCreditsScene());
	}

	private IEnumerator LoadCreditsScene()
	{
		yield return AddressablesUtility.LoadSceneAsync("EndCreditsSceneCleaner", LoadSceneMode.Single);
	}

	public void OnButtonQuit()
	{
		Application.Quit();
	}

	public void OnButtonFreedom()
	{
		CoroutineHost.StartCoroutine(StartNewGame(GameMode.Freedom));
	}

	public void OnButtonSurvival()
	{
		CoroutineHost.StartCoroutine(StartNewGame(GameMode.Survival));
	}

	public void OnButtonHardcore()
	{
		CoroutineHost.StartCoroutine(StartNewGame(GameMode.Hardcore));
	}

	public void OnButtonCreative()
	{
		CoroutineHost.StartCoroutine(StartNewGame(GameMode.Creative));
	}

	public void OnHome()
	{
		rightSide.OpenGroup("Home");
	}

	public void ShowPrimaryOptions(bool show)
	{
		primaryOptions.gameObject.SetActive(show);
	}

	public void OnRightSideOpened(GameObject root)
	{
		lastGroup = root.name;
		subMenu = root.GetComponentInChildren<uGUI_INavigableIconGrid>();
		if (subMenu == null)
		{
			subMenu = primaryOptions;
		}
		GamepadInputModule.current.SetCurrentGrid(GetCurrentSubMenu());
	}

	public void CloseRightSide()
	{
		MainMenuRightSide.main.OpenGroup("Home");
	}

	public void DeepLinkToLastSave()
	{
		LoadMostRecentSavedGame();
	}

	public override void OnSelect(bool lockMovement)
	{
		base.OnSelect(lockMovement);
		ShowPrimaryOptions(!string.Equals(lastGroup, "Options", StringComparison.OrdinalIgnoreCase));
		rightSide.OpenGroup(string.IsNullOrEmpty(lastGroup) ? "Home" : lastGroup);
	}

	public override void OnDeselect()
	{
		base.OnDeselect();
		GamepadInputModule.current.SetCurrentGrid(null);
	}
}
