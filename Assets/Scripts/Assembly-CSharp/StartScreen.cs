using System;
using System.Collections;
using Gendarme;
using Platform.Utils;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{
	public StartScreenPressStart pressStart;

	public GameObject mainMenuFaderRef;

	[SerializeField]
	[AssertNotNull]
	private FlashingLightsDisclaimer disclaimer;

	[SuppressMessage("Gendarme.Rules.Performance", "AvoidUnusedPrivateFieldsRule")]
	private StartScreenFade startScreenFade;

	private bool menuInputEnabled;

	private int loginGamepadIndex = -1;

	[AssertLocalization(1)]
	private const string loadFailedFormat = "LoadFailedFormat";

	[AssertLocalization]
	private const string loadFailed = "LoadFailed";

	[AssertLocalization]
	private const string loadOptionsFailedMessage = "LoadOptionsFailed";

	private const float showDisclaimerAfterSeconds = 0.5f;

	private const float startScreenFadeTime = 0.1f;

	private const float minDisclaimerShowTime = 4f;

	private const string menuEnviornmentWaterScapeName = "Waterscape";

	private const float delayInSecondsBeforeFading = 0.8f;

	private AsyncOperationHandle asyncLoadMenuEnv;

	private WaterSurface waterSurface;

	private bool menuEnvAlreadyLoaded;

	private float elapsedFadeDelayTime;

	private bool isMenuEnvironmentLoaded;

	private IEnumerator initializeCoroutine;

	public void SetStartMenuInputActive(bool active)
	{
		menuInputEnabled = active;
	}

	public void HideDisclaimer()
	{
		disclaimer.StartHidingDisclaimer();
	}

	private static IEnumerator FadeGraphicIn(CanvasRenderer renderer, float fadeOutTime = 0.25f)
	{
		renderer.gameObject.SetActive(value: true);
		float alpha = 0f;
		while (alpha <= 1f)
		{
			alpha += Time.unscaledDeltaTime / fadeOutTime;
			renderer.SetAlpha(alpha);
			yield return null;
		}
	}

	private static IEnumerator FadeGraphicOut(CanvasRenderer renderer, float fadeOutTime = 0.25f)
	{
		float alpha = 1f;
		while (alpha >= 0f)
		{
			alpha -= Time.unscaledDeltaTime / fadeOutTime;
			renderer.SetAlpha(alpha);
			yield return null;
		}
		renderer.gameObject.SetActive(value: false);
	}

	private void Awake()
	{
		startScreenFade = mainMenuFaderRef.GetComponent<StartScreenFade>();
		startScreenFade.enabled = false;
		TryToShowDisclaimer();
	}

	private IEnumerator Start()
	{
		while (PlatformUtils.main.GetServices() == null)
		{
			yield return null;
		}
		GraphicsUtil.SetFrameRate(144);
		MiscSettings.SetUIScale(PlatformUtils.main.GetDefaultUiScale(DisplayOperationMode.Default), DisplayOperationMode.Default);
		MiscSettings.SetUIScale(PlatformUtils.main.GetDefaultUiScale(DisplayOperationMode.Handheld), DisplayOperationMode.Handheld);
		UnityUWE.Return123Test();
		initializeCoroutine = Initialize();
		StartCoroutine(initializeCoroutine);
		asyncLoadMenuEnv = AddressablesUtility.LoadSceneAsync("MenuEnvironment", LoadSceneMode.Additive);
	}

	private IEnumerator Initialize()
	{
		yield return uGUI.InitializeAsync();
		yield return BaseGhost.InitializeAsync();
		yield return BaseDeconstructable.InitializeAsync();
	}

	private bool GetPressedController(out int controllerIndex)
	{
		controllerIndex = -1;
		if (menuInputEnabled)
		{
			for (int i = 0; i < 8; i++)
			{
				if (InputUtils.GetKeyUp((KeyCode)(357 + i * 20)))
				{
					controllerIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	private void Update()
	{
		CheckIsMenuEnvironmentLoaded();
		if (!uGUI.isInitialized)
		{
			return;
		}
		PlatformServices services = PlatformUtils.main.GetServices();
		if (services == null)
		{
			return;
		}
		bool supportsDynamicLogOn = services.GetSupportsDynamicLogOn();
		bool flag = true;
		int controllerIndex = -1;
		bool flag2 = supportsDynamicLogOn;
		if (AutomatedProfiler.IsActive())
		{
			flag2 = false;
		}
		if (flag2)
		{
			pressStart.gameObject.SetActive(value: true);
			flag = GetPressedController(out controllerIndex);
		}
		if (!pressStart.IsLoading && flag)
		{
			pressStart.SetLoading(_loading: true);
			loginGamepadIndex = controllerIndex;
			if (supportsDynamicLogOn)
			{
				PlatformUtils main = PlatformUtils.main;
				main.OnLoginFinished = (PlatformUtils.LoginFinishedDelegate)Delegate.Combine(main.OnLoginFinished, new PlatformUtils.LoginFinishedDelegate(OnLoginFinished));
				PlatformUtils.main.StartLogOnUserAsync(loginGamepadIndex);
			}
			else
			{
				StartLoadingSavedata();
			}
		}
	}

	private void CheckIsMenuEnvironmentLoaded()
	{
		if (isMenuEnvironmentLoaded || !asyncLoadMenuEnv.IsValid() || !asyncLoadMenuEnv.IsDone)
		{
			return;
		}
		if (!menuEnvAlreadyLoaded)
		{
			waterSurface = GameObject.Find("Waterscape").GetComponent<WaterSurface>();
			menuEnvAlreadyLoaded = true;
		}
		if (!waterSurface.IsLoadingDisplacementTextures())
		{
			elapsedFadeDelayTime += Time.unscaledDeltaTime;
			if (elapsedFadeDelayTime >= 0.8f)
			{
				isMenuEnvironmentLoaded = true;
				OnMenuEnvironmentLoaded();
			}
		}
	}

	private void OnMenuEnvironmentLoaded()
	{
	}

	private IEnumerator ActivatePressStartMenu()
	{
		yield return HideDisclaimerAfterMinShowTimeReached();
		HideStartScreen();
		SetStartMenuInputActive(active: true);
	}

	private void OnLoginFinished(bool success)
	{
		PlatformUtils main = PlatformUtils.main;
		main.OnLoginFinished = (PlatformUtils.LoginFinishedDelegate)Delegate.Remove(main.OnLoginFinished, new PlatformUtils.LoginFinishedDelegate(OnLoginFinished));
		if (success)
		{
			StartLoadingSavedata();
		}
		else
		{
			pressStart.SetLoading(_loading: false);
		}
	}

	private void StartLoadingSavedata()
	{
		CoroutineHost.StartCoroutine(Load());
	}

	private IEnumerator Load()
	{
		PlatformServices services;
		while ((services = PlatformUtils.main.GetServices()) == null)
		{
			yield return null;
		}
		UserStorage userStorage = services.GetUserStorage();
		UserStorageUtils.AsyncOperation initTask = userStorage.InitializeAsync();
		yield return initTask;
		if (!initTask.GetSuccessful())
		{
			Debug.LogErrorFormat("Save data init failed ({0})", initTask.result);
			string format = Language.main.GetFormat("LoadFailedFormat", initTask.errorMessage);
			uGUI.main.confirmation.Show(format, OnLoadErrorConfirmed);
			HideDisclaimer();
			yield break;
		}
		CoroutineTask<SaveLoadManager.LoadResult> loadSlotsTask = SaveLoadManager.main.LoadSlotsAsync();
		yield return loadSlotsTask;
		SaveLoadManager.LoadResult result = loadSlotsTask.GetResult();
		if (!result.success)
		{
			string format2 = Language.main.GetFormat("LoadFailedFormat", result.errorMessage);
			uGUI.main.confirmation.Show(format2, OnLoadErrorConfirmed);
			HideDisclaimer();
			yield break;
		}
		CoroutineTask<bool> loadOptionsTask = GameSettings.LoadAsync();
		yield return loadOptionsTask;
		if (!loadOptionsTask.GetResult())
		{
			string descriptionText = Language.main.Get("LoadOptionsFailed");
			uGUI.main.confirmation.Show(descriptionText, OnLoadErrorConfirmed);
			HideDisclaimer();
		}
		else
		{
			ApplicationFocus.OnRunInBackgroundChanged();
			yield return LoadMainMenu();
		}
	}

	private void OnLoadErrorConfirmed(bool confirmed)
	{
		if (confirmed)
		{
			StartLoadingSavedata();
		}
		else
		{
			CoroutineHost.StartCoroutine(LoadMainMenu());
		}
	}

	private IEnumerator LoadMainMenu()
	{
		AsyncOperationHandle asyncOperationHandle = AddressablesUtility.LoadSceneAsync("XMenu", LoadSceneMode.Additive);
		yield return asyncOperationHandle;
		uGUI_MainMenu.main.EnableInput(enable: false);
		yield return initializeCoroutine;
		yield return HideDisclaimerAfterMinShowTimeReached();
		uGUI_MainMenu.main.EnableInput(enable: true);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator ShowDisclaimerAfterDelay(float showDisclaimerAfterSeconds)
	{
		yield return new WaitForSeconds(showDisclaimerAfterSeconds);
		HideStartScreen();
		TryToShowDisclaimer();
	}

	private void TryToShowDisclaimer()
	{
		disclaimer.TryToShow();
	}

	private IEnumerator HideDisclaimerAfterMinShowTimeReached()
	{
		if (disclaimer != null && disclaimer.IsShown())
		{
			while (disclaimer.GetShowTime() < 4f)
			{
				yield return null;
			}
			HideDisclaimer();
			WaitForSecondsRealtime fadeRoutine = new WaitForSecondsRealtime(0.6f);
			while (fadeRoutine.MoveNext())
			{
				yield return null;
			}
		}
	}

	private void HideStartScreen()
	{
		startScreenFade.StartToFade(0.1f);
	}
}
