using System;
using System.Collections;
using System.IO;
using FMODUnity;
using Platform.IO;
using TMPro;
using UWE;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class IngameMenu : uGUI_InputGroup, uGUI_IButtonReceiver
{
	public static IngameMenu main;

	[AssertNotNull]
	public Button saveButton;

	[AssertNotNull]
	public Button feedbackButton;

	[AssertNotNull]
	public Button quitToMainMenuButton;

	[AssertNotNull]
	public Button helpButton;

	[AssertNotNull]
	public Button developerButton;

	[AssertNotNull]
	public Button unstuckButton;

	[AssertNotNull]
	public TextMeshProUGUI quitToMainMenuText;

	[AssertNotNull]
	public TextMeshProUGUI quitLastSaveText;

	[AssertNotNull]
	public uGUI_CanvasScaler canvasScaler;

	public float maxSecondsToBeRecentlySaved;

	public GameObject mainPanel;

	[SerializeField]
	[AssertNotNull]
	private uGUI_GraphicRaycaster interactionRaycaster;

	private bool developerMode;

	private float lastSavedStateTime;

	private const TextureFormat ScreenShotFormat = TextureFormat.RGB24;

	private const int ScreenShotFormatSize = 3;

	private const int ScreenshotScaledMipLevel = 3;

	private Texture2D saveScreenshotOriginal;

	private Texture2D saveScreenshotScaled;

	private const FreezeTime.Id freezerId = FreezeTime.Id.IngameMenu;

	private const FreezeTime.Id quitFreezerId = FreezeTime.Id.Quit;

	[AssertLocalization]
	private const string quitMessage = "QuitToMainMenu";

	[AssertLocalization]
	private const string saveAndQuitMessage = "SaveAndQuitToMainMenu";

	[AssertLocalization(1)]
	private const string saveFailedMessageFormat = "SaveFailedFormat";

	[AssertLocalization]
	public const string saveFailedOutOfSpaceMessage = "SaveFailedSpace";

	[AssertLocalization]
	private const string saveFailedMessage = "SaveFailed";

	[AssertLocalization]
	private const string gameNotSavedMessage = "GameNotSaved";

	[AssertLocalization(1)]
	private const string timeSinceLastSaveFormat = "TimeSinceLastSave";

	private FreezeTimeInputHandler quitFreezerInputHandler = new FreezeTimeInputHandler(FreezeTime.Id.Quit);

	public GameObject currentScreen { get; set; }

	public bool isQuitting { get; private set; }

	public bool canBeOpen { get; set; } = true;


	protected override void Awake()
	{
		main = this;
		base.Awake();
		interactionRaycaster.updateRaycasterStatusDelegate = UpdateRaycasterStatus;
		uGUI_TabbedControlsPanel[] componentsInChildren = GetComponentsInChildren<uGUI_TabbedControlsPanel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].onClose = ClosePanel;
		}
		CreateScreenShotTextures();
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(saveScreenshotOriginal);
		UnityEngine.Object.Destroy(saveScreenshotScaled);
	}

	private void CreateScreenShotTextures()
	{
		saveScreenshotOriginal = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, 4, linear: false)
		{
			name = "IngameMenu.SaveScreenshotCapture"
		};
		int width = CalculateMipDimension(Screen.width, 3);
		int height = CalculateMipDimension(Screen.height, 3);
		saveScreenshotScaled = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false, linear: false)
		{
			name = "IngameMenu.SaveScreenshotScaled"
		};
	}

	private static int CalculateMipDimension(int dim, int mip)
	{
		return dim >> mip;
	}

	private void ResizeScreenshotIfScreenSizeChanged()
	{
		if (saveScreenshotOriginal.width != Screen.width || saveScreenshotOriginal.height != Screen.height)
		{
			saveScreenshotOriginal.Resize(Screen.width, Screen.height);
			int width = CalculateMipDimension(Screen.width, 3);
			int height = CalculateMipDimension(Screen.height, 3);
			saveScreenshotScaled.Resize(width, height);
		}
	}

	private void CaptureSaveScreenshot()
	{
		ResizeScreenshotIfScreenSizeChanged();
		saveScreenshotOriginal.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		saveScreenshotOriginal.Apply();
		ExtractRelevantMipLevel();
	}

	private void ExtractRelevantMipLevel()
	{
		NativeArray<byte> rawTextureData = saveScreenshotOriginal.GetRawTextureData<byte>();
		int num = CalculateMipOffset(saveScreenshotOriginal);
		saveScreenshotScaled.SetPixelData(rawTextureData, 0, num * 3);
	}

	private static int CalculateMipOffset(Texture2D texture)
	{
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			num += (texture.width >> i) * (texture.height >> i);
		}
		return num;
	}

	private void OnEnable()
	{
		uGUI_LegendBar.ClearButtons();
		uGUI_LegendBar.ChangeButton(0, GameInput.FormatButton(GameInput.Button.UICancel), Language.main.GetFormat("Back"));
		uGUI_LegendBar.ChangeButton(1, GameInput.FormatButton(GameInput.Button.UISubmit), Language.main.GetFormat("ItemSelectorSelect"));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		uGUI_LegendBar.ClearButtons();
	}

	private void Start()
	{
		if (!uGUI_FeedbackCollector.main.IsEnabled())
		{
			feedbackButton.gameObject.SetActive(value: false);
			Transform transform = base.transform.Find("Main/ButtonLayout/ButtonFeedback");
			if (transform != null)
			{
				transform.gameObject.SetActive(value: false);
			}
		}
		base.gameObject.SetActive(value: false);
		ResetMenu();
		lastSavedStateTime = Time.timeSinceLevelLoad;
	}

	public void ActivateDeveloperMode()
	{
		if (PlatformUtils.GetDevToolsEnabled())
		{
			developerMode = true;
			developerButton.gameObject.SetActive(value: true);
		}
	}

	protected override void Update()
	{
		bool flag = CanClose();
		if (currentScreen != null && currentScreen != mainPanel && currentScreen.activeInHierarchy && currentScreen.GetComponent<IngameMenuPanel>() != null)
		{
			flag = false;
		}
		if (flag)
		{
			base.Update();
		}
		if (!developerMode && GameInput.IsPrimaryDeviceGamepad() && GameInput.GetButtonHeld(GameInput.Button.MoveUp) && GameInput.GetButtonDown(GameInput.Button.MoveDown))
		{
			ActivateDeveloperMode();
		}
		bool allowSaving = GetAllowSaving();
		saveButton.interactable = allowSaving;
		quitToMainMenuButton.interactable = allowSaving || !GameModeUtils.IsPermadeath();
		unstuckButton.interactable = UnstuckPlayer.CanWarp();
	}

	private void ResetMenu()
	{
		foreach (Transform item in base.gameObject.transform)
		{
			if (item.gameObject.name == "Main")
			{
				item.gameObject.SetActive(value: true);
				currentScreen = item.gameObject;
			}
			else if (item.gameObject.name == "Legend")
			{
				item.gameObject.SetActive(value: true);
			}
			else
			{
				item.gameObject.SetActive(value: false);
			}
		}
	}

	public void Open()
	{
		if (!(Time.timeSinceLevelLoad < 1f) && !FreezeTime.PleaseWait && canBeOpen)
		{
			if (!base.gameObject.activeInHierarchy)
			{
				base.gameObject.SetActive(value: true);
				Select();
			}
			if (XRSettings.enabled)
			{
				HideForScreenshots.Hide(HideForScreenshots.HideType.Mask | HideForScreenshots.HideType.HUD | HideForScreenshots.HideType.ViewModel);
			}
			ChangeSubscreen("Main");
		}
	}

	public void Close()
	{
		Deselect();
	}

	private bool GetAllowSaving()
	{
		if (uGUI.isIntro || IntroLifepodDirector.IsActive || LaunchRocket.isLaunching || (StoryGoalCustomEventHandler.main != null && StoryGoalCustomEventHandler.main.IsSunbeamAnimationActive))
		{
			return false;
		}
		if (PlayerCinematicController.cinematicModeCount > 0)
		{
			if (!(Time.time > PlayerCinematicController.cinematicActivityExpireTime))
			{
				return false;
			}
			Debug.LogError("cinematics have been blocking saves for an unusual length of time; assuming a bug and allowing saving");
		}
		else if (!Player.allowSaving)
		{
			return false;
		}
		return !SaveLoadManager.main.isSaving;
	}

	public override void OnSelect(bool lockMovement)
	{
		base.OnSelect(lockMovement);
		base.gameObject.SetActive(value: true);
		FreezeTime.Begin(FreezeTime.Id.IngameMenu);
		UWE.Utils.lockCursor = false;
		UpdateButtons();
	}

	private void UpdateButtons()
	{
		if (GameModeUtils.IsPermadeath())
		{
			quitToMainMenuText.text = Language.main.Get("SaveAndQuitToMainMenu");
			saveButton.gameObject.SetActive(value: false);
		}
		else
		{
			quitToMainMenuText.text = Language.main.Get("QuitToMainMenu");
			saveButton.gameObject.SetActive(value: true);
		}
	}

	public bool CanClose()
	{
		if (!isQuitting)
		{
			if (!(SaveLoadManager.main == null))
			{
				return !SaveLoadManager.main.isSaving;
			}
			return true;
		}
		return false;
	}

	public override void OnDeselect()
	{
		base.OnDeselect();
		ResetMenu();
		FreezeTime.End(FreezeTime.Id.IngameMenu);
		uGUI_BasicColorSwap[] componentsInChildren = GetComponentsInChildren<uGUI_BasicColorSwap>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].makeTextWhite();
		}
		base.gameObject.SetActive(value: false);
		if (XRSettings.enabled)
		{
			HideForScreenshots.Hide(HideForScreenshots.HideType.None);
		}
	}

	public void QuitSubscreen()
	{
		float num = Time.timeSinceLevelLoad - lastSavedStateTime;
		if (!GameModeUtils.IsPermadeath() && num > maxSecondsToBeRecentlySaved)
		{
			quitLastSaveText.text = Language.main.GetFormat("TimeSinceLastSave", Utils.PrettifyTime((int)num));
			ChangeSubscreen("QuitConfirmationWithSaveWarning");
		}
		else
		{
			ChangeSubscreen("QuitConfirmation");
		}
	}

	public void ChangeSubscreen(string newScreen)
	{
		if (currentScreen.name != newScreen)
		{
			currentScreen.SetActive(value: false);
			currentScreen = base.transform.Find(newScreen).gameObject;
			currentScreen.SetActive(value: true);
		}
		uGUI_INavigableIconGrid componentInChildren = currentScreen.GetComponentInChildren<uGUI_INavigableIconGrid>();
		GamepadInputModule.current.SetCurrentGrid(componentInChildren);
	}

	public IEnumerator QuitGameAsync(bool quitToDesktop)
	{
		if (!quitToDesktop)
		{
			Telemetry.SendGameQuit(quitToDesktop: false);
		}
		UWE.Utils.lockCursor = true;
		isQuitting = true;
		uGUI_LegendBar.ClearButtons();
		if (SaveLoadManager.main.isSaving)
		{
			ChangeSubscreen("Main");
			mainPanel.SetActive(value: false);
			FreezeTime.Begin(FreezeTime.Id.Quit);
			InputHandlerStack.main.Push(quitFreezerInputHandler);
			while (SaveLoadManager.main.isSaving)
			{
				yield return null;
			}
			FreezeTime.End(FreezeTime.Id.Quit);
		}
		if (GameModeUtils.IsPermadeath())
		{
			ChangeSubscreen("Main");
			yield return SaveGameAsync();
		}
		LargeWorldStreamer streamer = LargeWorldStreamer.main;
		if ((bool)streamer && !streamer.IsWorldSettled())
		{
			ChangeSubscreen("Main");
			mainPanel.SetActive(value: false);
			FreezeTime.Begin(FreezeTime.Id.Quit);
			InputHandlerStack.main.Push(quitFreezerInputHandler);
			while (!streamer.IsWorldSettled())
			{
				yield return CoroutineUtils.waitForNextFrame;
			}
			FreezeTime.End(FreezeTime.Id.Quit);
		}
		Close();
		yield return CoroutineUtils.waitForNextFrame;
		if (!quitToDesktop)
		{
			UWE.Utils.lockCursor = false;
			yield return QuitToMainMenuAsync();
		}
		else
		{
			Application.Quit();
		}
	}

	public static IEnumerator QuitToMainMenuAsync()
	{
		Camera[] array = UnityEngine.Object.FindObjectsOfType<Camera>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		PlatformUtils.AddTemporaryCamera();
		yield return null;
		yield return null;
		RuntimeManager.StopAllEvents(allowFadeOut: true);
		SceneCleaner.Open();
	}

	public static bool IsQuitting()
	{
		if (main != null)
		{
			return main.isQuitting;
		}
		return false;
	}

	public void GiveFeedback()
	{
		OpenFeedbackForm();
	}

	public void OpenFeedbackForm()
	{
		Deselect();
		uGUI_FeedbackCollector.main.Open();
	}

	public void OpenFeedbackForums()
	{
		PlatformUtils.OpenURL("http://playsubnautica.com");
	}

	public void ShowHelp()
	{
		PlatformUtils.main.GetServices().ShowHelp();
	}

	public void SaveGame()
	{
		CoroutineHost.StartCoroutine(SaveGameAsync());
	}

	public void QuitGame(bool quitToDesktop)
	{
		CoroutineHost.StartCoroutine(QuitGameAsync(quitToDesktop));
	}

	public void UnstuckSubscreen()
	{
		ChangeSubscreen("UnstuckConfirmation");
	}

	public void PlayForwardSound()
	{
	}

	public void PlayBackSound()
	{
	}

	public void PlayMouseOverSound()
	{
	}

	public void PlayApplySound()
	{
	}

	private void ClosePanel()
	{
		ChangeSubscreen("Main");
	}

	private bool ReportSaveError(SaveLoadManager.SaveResult saveResult)
	{
		if (!saveResult.success)
		{
			string text = null;
			if (saveResult.error == SaveLoadManager.Error.OutOfSpace)
			{
				if (PlatformUtils.main.GetServices().GetDisplayOutOfSpaceMessage())
				{
					text = Language.main.GetFormat("SaveFailedSpace", UWE.Math.CeilDiv(saveResult.size, 1000000));
				}
			}
			else
			{
				text = Language.main.GetFormat("SaveFailedFormat", saveResult.errorMessage);
			}
			if (text != null)
			{
				FreezeTime.Begin(FreezeTime.Id.IngameMenu);
				uGUI.main.confirmation.Show(text, OnSaveErrorConfirmed);
			}
			return true;
		}
		return false;
	}

	private IEnumerator SaveGameAsync()
	{
		mainPanel.SetActive(value: false);
		SaveLoadManager.main.NotifySaveInProgress(isInProgress: true);
		yield return MainGameController.Instance.PerformGarbageAndAssetCollectionAsync();
		yield return new WaitForEndOfFrame();
		CaptureSaveScreenshot();
		mainPanel.SetActive(value: true);
		Close();
		CoroutineTask<SaveLoadManager.SaveResult> saveToTemporaryTask = SaveLoadManager.main.SaveToTemporaryStorageAsync(saveScreenshotScaled);
		yield return saveToTemporaryTask;
		if (ReportSaveError(saveToTemporaryTask.GetResult()))
		{
			SaveLoadManager.main.NotifySaveInProgress(isInProgress: false);
			yield break;
		}
		CoroutineTask<SaveLoadManager.SaveResult> saveToDeepStorageTask = SaveLoadManager.main.SaveToDeepStorageAsync();
		yield return saveToDeepStorageTask;
		SaveLoadManager.main.NotifySaveInProgress(isInProgress: false);
		if (ReportSaveError(saveToDeepStorageTask.GetResult()))
		{
			yield break;
		}
		lastSavedStateTime = Time.timeSinceLevelLoad;
		try
		{
			long num = 0L;
			Platform.IO.FileInfo[] files = new Platform.IO.DirectoryInfo(SaveLoadManager.GetTemporarySavePath()).GetFiles("*", SearchOption.AllDirectories);
			foreach (Platform.IO.FileInfo fileInfo in files)
			{
				num += fileInfo.Length;
			}
			using (GameAnalytics.EventData eventData = GameAnalytics.CustomEvent(GameAnalytics.Event.GameSaved))
			{
				eventData.Add("size", num);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void OnSaveErrorConfirmed(bool confirmed)
	{
		FreezeTime.End(FreezeTime.Id.IngameMenu);
		if (confirmed)
		{
			SaveGame();
		}
		else
		{
			ErrorMessage.AddError(Language.main.Get("GameNotSaved"));
		}
	}

	private void PlaySound(FMODAsset asset)
	{
		FMODUWE.PlayOneShot(asset, Vector3.zero);
	}

	public bool OnButtonDown(GameInput.Button button)
	{
		if (button == GameInput.Button.UIMenu && CanClose())
		{
			Close();
			GameInput.ClearInput();
			return true;
		}
		return false;
	}

	private void UpdateRaycasterStatus(uGUI_GraphicRaycaster raycaster)
	{
		if (GameInput.IsPrimaryDeviceGamepad() && !VROptions.GetUseGazeBasedCursor())
		{
			raycaster.enabled = false;
		}
		else
		{
			raycaster.enabled = base.focused;
		}
	}
}
