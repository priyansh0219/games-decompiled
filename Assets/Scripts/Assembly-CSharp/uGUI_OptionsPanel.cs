using System;
using System.Collections.Generic;
using System.Linq;
using Gendarme;
using TMPro;
using UWE;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class uGUI_OptionsPanel : uGUI_TabbedControlsPanel, ILocalizationCheckable
{
	public class ResolutionEqualityComparer : IEqualityComparer<Resolution>
	{
		public bool Equals(Resolution x, Resolution y)
		{
			if (x.width == y.width)
			{
				return x.height == y.height;
			}
			return false;
		}

		public int GetHashCode(Resolution obj)
		{
			Resolution resolution = obj;
			resolution.refreshRate = 0;
			return resolution.GetHashCode();
		}
	}

	[AssertLocalization]
	private const string labelTerrainChangeRequiresRestart = "TerrainChangeRequiresRestart";

	[AssertLocalization]
	private const string labelDisplayChangeRequiresRestart = "DisplayChangeRequiresRestart";

	[AssertLocalization]
	private const string labelApplyModifiedSettings = "ApplyModifiedSettings";

	private const ManagedUpdate.Queue queueUpdateCanvas = ManagedUpdate.Queue.LateUpdateAfterInput;

	public bool showLanguageOption = true;

	private string[] languages;

	private Toggle subtitlesOption;

	private List<Resolution> resolutions;

	public GameObject keyRedemptionPrefab;

	public GameObject troubleshootingPrefab;

	private uGUI_Choice resolutionOption;

	private uGUI_Choice qualityPresetOption;

	private uGUI_Choice detailOption;

	private uGUI_Choice aaModeOption;

	private uGUI_Choice waterQualityOption;

	private uGUI_Choice aaQualityOption;

	private Toggle bloomOption;

	private Toggle lensDirtOption;

	private Toggle dofOption;

	private uGUI_Choice motionBlurQualityOption;

	private uGUI_Choice aoQualityOption;

	private uGUI_Choice ssrQualityOption;

	private Toggle ditheringOption;

	private GameObject targetFrameRateOption;

	private bool m_syncingGraphicsSettings;

	private int textScaleDirtyFrame = -1;

	[AssertLocalization]
	private const string gammaLabel = "Gamma";

	[AssertLocalization(1)]
	private const string keepResolutionLabel = "KeepResolution";

	[AssertLocalization]
	private const string generalTabLabel = "General";

	[AssertLocalization]
	private const string langugeLabel = "Language";

	[AssertLocalization]
	private const string runInBackgroundLabel = "RunInBackground";

	[AssertLocalization]
	private const string runInBackgroundTooltip = "RunInBackgroundTooltip";

	[AssertLocalization]
	private const string newsEnabledLabel = "NewsEnabled";

	[AssertLocalization]
	private const string subtitlesLabel = "Subtitles";

	[AssertLocalization]
	private const string subtitlesEnabledLabel = "SubtitlesEnabled";

	[AssertLocalization]
	private const string subtitlesSpeedLabel = "SubtitlesSpeed";

	[AssertLocalization]
	private const string displayLabel = "Display";

	[AssertLocalization]
	private const string resolutionLabel = "Resolution";

	[AssertLocalization]
	private const string fullscreenLabel = "Fullscreen";

	[AssertLocalization]
	private const string vsyncLabel = "Vsync";

	[AssertLocalization]
	private const string fpsCapLabel = "FPSCap";

	[AssertLocalization]
	private const string fieldOfViewLabel = "FieldOfView";

	[AssertLocalization]
	private const string uiScaleLabel = "UIScale";

	[AssertLocalization]
	private const string flashesLabel = "Flashes";

	[AssertLocalization]
	private const string pdaPauseLabel = "PDAPause";

	[AssertLocalization]
	private const string vrRenderScaleLabel = "VRRenderScale";

	[AssertLocalization]
	private const string soundLabel = "Sound";

	[AssertLocalization]
	private const string masterVolumeLabel = "MasterVolume";

	[AssertLocalization]
	private const string musicVolumeLabel = "MusicVolume";

	[AssertLocalization]
	private const string voiceVolumeLabel = "VoiceVolume";

	[AssertLocalization]
	private const string ambientVolumeLabel = "AmbientVolume";

	[AssertLocalization]
	private const string graphicsTabLabel = "Graphics";

	[AssertLocalization]
	private const string presetLabel = "Preset";

	[AssertLocalization]
	private static readonly string[] presetOptions = new string[4] { "Low", "Medium", "High", "Custom" };

	[AssertLocalization]
	private static readonly string[] presetOptionsNextGen = new string[2] { "HighFramerate", "HighQuality" };

	[AssertLocalization]
	private const string colorGradingLabel = "ColorGrading";

	[AssertLocalization]
	private static readonly string[] colorGradingOptions = new string[3] { "Off", "Neutral", "Filmic" };

	[AssertLocalization]
	private const string advancedLabel = "Advanced";

	[AssertLocalization]
	private const string detailLabel = "Detail";

	[AssertLocalization]
	private const string waterQualityLabel = "WaterQuality";

	[AssertLocalization]
	private const string antialiasingLabel = "Antialiasing";

	[AssertLocalization]
	private const string antialiasingQualityLabel = "AntialiasingQuality";

	[AssertLocalization]
	private const string bloomLabel = "Bloom";

	[AssertLocalization]
	private const string lensDirtLabel = "LensDirt";

	[AssertLocalization]
	private const string depthOfFieldLabel = "DepthOfField";

	[AssertLocalization]
	private const string motionBlurLabel = "MotionBlurQuality";

	[AssertLocalization]
	private const string ambientOcclusionLabel = "AmbientOcclusion";

	[AssertLocalization]
	private const string screenSpaceReflectionsLabel = "ScreenSpaceReflections";

	[AssertLocalization]
	private const string ditheringLabel = "Dithering";

	[AssertLocalization]
	private static readonly string[] postFXQualityNames = new string[4] { "Off", "Low", "Medium", "High" };

	[AssertLocalization]
	private const string keyRedemptionTabLabel = "KeyRedemption";

	[AssertLocalization]
	private const string troubleshootingTabLabel = "Troubleshooting";

	[AssertLocalization]
	private const string accessibilityTabLabel = "Accessibility";

	private static string[] GetLanguageOptions(out int currentIndex)
	{
		string currentLanguage = Language.main.GetCurrentLanguage();
		string[] array = Language.main.GetLanguages();
		currentIndex = Array.IndexOf(array, currentLanguage);
		return array;
	}

	private static string[] GetLanguageKeys(string[] forLanguages)
	{
		string[] array = new string[forLanguages.Length];
		for (int i = 0; i < forLanguages.Length; i++)
		{
			array[i] = $"Language{forLanguages[i]}";
		}
		return array;
	}

	private static string[] GetAntiAliasingOptions(out int currentIndex)
	{
		currentIndex = UwePostProcessingManager.GetAaMode();
		return new string[2] { "FXAA", "TAA" };
	}

	private static string[] GetColorGradingOptions(out int currentIndex)
	{
		currentIndex = UwePostProcessingManager.GetColorGradingMode();
		return colorGradingOptions;
	}

	private static string[] GetResolutionOptions(out List<Resolution> resolutions)
	{
		resolutions = new List<Resolution>();
		resolutions.AddRange(Screen.resolutions.Distinct(new ResolutionEqualityComparer()));
		string[] array = new string[resolutions.Count];
		Resolution desktopResolution = EditorModifications.desktopResolution;
		float a = (float)desktopResolution.width / (float)desktopResolution.height;
		for (int i = 0; i < resolutions.Count; i++)
		{
			Resolution resolution = resolutions[i];
			float b = (float)resolution.width / (float)resolution.height;
			if (Mathf.Approximately(a, b))
			{
				array[i] = $"{resolution.width} x {resolution.height} *";
			}
			else
			{
				array[i] = $"{resolution.width} x {resolution.height}";
			}
		}
		return array;
	}

	private static int GetCurrentResolutionIndex(List<Resolution> resolutions)
	{
		if (resolutions != null)
		{
			for (int i = 0; i < resolutions.Count; i++)
			{
				Resolution resolution = resolutions[i];
				if (Screen.width == resolution.width && Screen.height == resolution.height)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private static string[] GetDetailOptions(out int currentIndex)
	{
		currentIndex = QualitySettings.GetQualityLevel();
		return QualitySettings.names;
	}

	private void OnLanguageChanged(int index)
	{
		if (languages[index] == Language.main.GetCurrentLanguage())
		{
			UnappliedSettings.Remove(UnappliedSettings.Key.Language);
			return;
		}
		UnappliedSettings.Add(UnappliedSettings.Key.Language, delegate
		{
			string currentLanguage = languages[index];
			Language.main.SetCurrentLanguage(currentLanguage);
		});
	}

	private void OnRunInBackgroundChanged(bool runInBackground)
	{
		MiscSettings.runInBackground = runInBackground;
		ApplicationFocus.OnRunInBackgroundChanged();
	}

	private void OnNewsEnabledChanged(bool newsEnabled)
	{
		MiscSettings.newsEnabled = newsEnabled;
	}

	private void OnShowSubtitlesChanged(bool showSubtitles)
	{
		Language.main.showSubtitles = showSubtitles;
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void OnSubtitlesSpeedChanged(float speed)
	{
		Subtitles.speed = speed;
	}

	private void OnSoundDeviceChanged(int currentIndex)
	{
		SoundSystem.SetDevice(currentIndex);
	}

	private void OnResolutionChanged(int applyIndex)
	{
		if (applyIndex == GetCurrentResolutionIndex(resolutions))
		{
			UnappliedSettings.Remove(UnappliedSettings.Key.Resolution);
			return;
		}
		UnappliedSettings.Add(UnappliedSettings.Key.Resolution, delegate
		{
			int revertIndex = GetCurrentResolutionIndex(resolutions);
			UnappliedSettings.Revert(UnappliedSettings.Key.Resolution, delegate(uGUI_Dialog dialog)
			{
				dialog.Show(Language.main.GetFormat("KeepResolution", 10), delegate(int option)
				{
					if (option <= 0)
					{
						if (revertIndex >= 0)
						{
							Resolution resolution2 = resolutions[revertIndex];
							DisplayManager.SetResolution(resolution2.width, resolution2.height, Screen.fullScreen);
						}
						resolutionOption.value = revertIndex;
					}
				}, dialog.DialogTimeout("KeepResolution", 10), 0, Language.main.Get("RevertChanges"), Language.main.Get("KeepChanges"));
			});
			Resolution resolution = resolutions[applyIndex];
			DisplayManager.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
			resolutionOption.value = applyIndex;
		});
	}

	private void OnScreenChanged()
	{
		if (resolutionOption != null)
		{
			resolutionOption.value = GetCurrentResolutionIndex(resolutions);
			UnappliedSettings.Remove(UnappliedSettings.Key.Resolution);
		}
	}

	private void OnDisplayChanged(int currentIndex)
	{
		_ = Display.displays[currentIndex];
		DisplayManager.SetDesiredDisplayIndex(currentIndex);
		if (DisplayManager.GetDisplayChangeRequiresRestart())
		{
			dialog.Show(Language.main.Get("DisplayChangeRequiresRestart"), null, "OK");
		}
	}

	private void OnFullscreenChanged(bool fullscreen)
	{
		Screen.fullScreen = fullscreen;
	}

	private void OnVSyncChanged(bool vsync)
	{
		GraphicsUtil.SetVSyncEnabled(vsync);
		targetFrameRateOption.SetActive(!vsync);
	}

	private void OnTargetFrameRateChanged(float frameRate)
	{
		GraphicsUtil.SetFrameRate(Mathf.RoundToInt(frameRate));
	}

	private void OnQualityPresetChanged(int option)
	{
		ApplyQualityPreset(option);
		if (GetTerrainChangeRequiresRestart())
		{
			dialog.Show(Language.main.Get("TerrainChangeRequiresRestart"), null, "OK");
		}
	}

	private void OnDetailChanged(int currentIndex)
	{
		GraphicsUtil.SetQualityLevel(currentIndex);
		SyncQualityPresetSelection();
	}

	private void OnWaterQualityChanged(WaterSurface.Quality currentQuality)
	{
		WaterSurface.SetQuality(currentQuality);
		SyncQualityPresetSelection();
	}

	private void OnAAmodeChanged(int mode)
	{
		UwePostProcessingManager.SetAaMode(mode);
		SyncQualityPresetSelection();
	}

	private void OnAAqualityChanged(int currentQuality)
	{
		UwePostProcessingManager.SetAaQuality(currentQuality);
		SyncQualityPresetSelection();
	}

	private void OnAOqualityChanged(int currentQuality)
	{
		UwePostProcessingManager.SetAoQuality(currentQuality);
		SyncQualityPresetSelection();
	}

	private void OnSSRqualityChanged(int currentQuality)
	{
		UwePostProcessingManager.SetSsrQuality(currentQuality);
		SyncQualityPresetSelection();
	}

	private void OnBloomChanged(bool enableComp)
	{
		UwePostProcessingManager.ToggleBloom(enableComp);
		SyncQualityPresetSelection();
	}

	private void OnBloomLensDirtChanged(bool enableComp)
	{
		UwePostProcessingManager.ToggleBloomLensDirt(enableComp);
		SyncQualityPresetSelection();
	}

	private void OnDofChanged(bool enableComp)
	{
		UwePostProcessingManager.ToggleDof(enableComp);
		SyncQualityPresetSelection();
	}

	private void OnMotionBlurQualityChanged(int currentQuality)
	{
		UwePostProcessingManager.SetMotionBlurQuality(currentQuality);
		SyncQualityPresetSelection();
	}

	private void OnDitheringChanged(bool enableComp)
	{
		UwePostProcessingManager.ToggleDithering(enableComp);
		SyncQualityPresetSelection();
	}

	private void OnColorGradingChanged(int mode)
	{
		UwePostProcessingManager.SetColorGradingMode(mode);
		SyncQualityPresetSelection();
	}

	private void OnVRRenderScaleChanged(float value)
	{
		if (Mathf.Approximately(XRSettings.eyeTextureResolutionScale, value))
		{
			UnappliedSettings.Remove(UnappliedSettings.Key.VRRenderScale);
			return;
		}
		UnappliedSettings.Add(UnappliedSettings.Key.VRRenderScale, delegate
		{
			XRSettings.eyeTextureResolutionScale = value;
		});
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void OnFovChanged(float value)
	{
		MiscSettings.fieldOfView = value;
		if (SNCameraRoot.main != null)
		{
			SNCameraRoot.main.SyncFieldOfView();
		}
		SetTextScaleDirty();
	}

	private void SetTextScaleDirty()
	{
		textScaleDirtyFrame = Time.frameCount;
	}

	private void OnUpdateCanvas()
	{
		if (textScaleDirtyFrame < 0)
		{
			return;
		}
		textScaleDirtyFrame = -1;
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfTypeAll(typeof(TextMeshProUGUI));
		for (int i = 0; i < array.Length; i++)
		{
			TextMeshProUGUI textMeshProUGUI = array[i] as TextMeshProUGUI;
			if (textMeshProUGUI != null)
			{
				textMeshProUGUI.SetScaleDirty();
			}
		}
	}

	private void OnFlashesChanged(bool value)
	{
		MiscSettings.flashes = value;
	}

	private void OnPDAPauseChanged(bool value)
	{
		MiscSettings.pdaPause = value;
	}

	private void OnMasterVolumeChanged(float value)
	{
		SoundSystem.SetMasterVolume(value);
	}

	private void OnMusicVolumeChanged(float value)
	{
		SoundSystem.SetMusicVolume(value);
	}

	private void OnVoiceVolumeChanged(float value)
	{
		SoundSystem.SetVoiceVolume(value);
	}

	private void OnAmbientVolumeChanged(float value)
	{
		SoundSystem.SetAmbientVolume(value);
	}

	private void SyncGraphicsSettingsSelection()
	{
		m_syncingGraphicsSettings = true;
		if (detailOption != null)
		{
			detailOption.value = QualitySettings.GetQualityLevel();
		}
		if (waterQualityOption != null)
		{
			waterQualityOption.value = (int)WaterSurface.GetQuality();
		}
		if (aaModeOption != null)
		{
			aaModeOption.value = UwePostProcessingManager.GetAaMode();
		}
		if (aaQualityOption != null)
		{
			aaQualityOption.value = UwePostProcessingManager.GetAaQuality();
		}
		if (aoQualityOption != null)
		{
			aoQualityOption.value = UwePostProcessingManager.GetAoQuality();
		}
		if (ssrQualityOption != null)
		{
			ssrQualityOption.value = UwePostProcessingManager.GetSsrQuality();
		}
		if (bloomOption != null)
		{
			bloomOption.isOn = UwePostProcessingManager.GetBloomEnabled();
		}
		if (lensDirtOption != null)
		{
			lensDirtOption.isOn = UwePostProcessingManager.GetBloomLensDirtEnabled();
		}
		if (dofOption != null)
		{
			dofOption.isOn = UwePostProcessingManager.GetDofEnabled();
		}
		if (motionBlurQualityOption != null)
		{
			motionBlurQualityOption.value = UwePostProcessingManager.GetMotionBlurQuality();
		}
		if (ditheringOption != null)
		{
			ditheringOption.isOn = UwePostProcessingManager.GetDitheringEnabled();
		}
		m_syncingGraphicsSettings = false;
	}

	private bool GetTerrainChangeRequiresRestart()
	{
		if (LargeWorldStreamer.main != null)
		{
			return LargeWorldStreamer.main.streamerV2.GetActiveQualityLevel() != QualitySettings.GetQualityLevel();
		}
		return false;
	}

	private void SyncQualityPresetSelection()
	{
		if (!m_syncingGraphicsSettings && qualityPresetOption != null)
		{
			qualityPresetOption.value = GetQualityPresetIndex();
		}
	}

	private int GetQualityPresetIndex()
	{
		GraphicsPreset[] presets = GraphicsPreset.GetPresets();
		int num = GraphicsPreset.GetPresetIndexForCurrentOptions();
		if (num == -1)
		{
			num = presets.Length;
		}
		return num;
	}

	private void ApplyQualityPreset(int option)
	{
		GraphicsPreset[] presets = GraphicsPreset.GetPresets();
		if (option < presets.Length)
		{
			presets[option].Apply();
			SyncGraphicsSettingsSelection();
		}
	}

	private void Awake()
	{
		applyButton.onClick.AddListener(OnApplyButton);
	}

	protected override void Update()
	{
		UpdateButtonState(backButton, !GameInput.IsPrimaryDeviceGamepad() || VROptions.GetUseGazeBasedCursor());
		UpdateButtonState(applyButton, UnappliedSettings.HasUnappliedSettings);
		UpdateButtonsNavigation();
		UnappliedSettings.Update(dialog);
	}

	public void OnSave(UserStorageUtils.SaveOperation saveOperation)
	{
		if (saveOperation.result == UserStorageUtils.Result.OutOfSpace && PlatformUtils.main.GetServices().GetDisplayOutOfSpaceMessage())
		{
			string format = Language.main.GetFormat("SaveFailedSpace", UWE.Math.CeilDiv(saveOperation.saveDataSize, 1000000));
			uGUI.main.confirmation.Show(format, OnSaveErrorConfirmed);
		}
	}

	public void OnSaveErrorConfirmed(bool confirmed)
	{
		if (confirmed)
		{
			OnApplyButton();
		}
	}

	private void OnApplyButton()
	{
		UnappliedSettings.Apply();
		uGUI_CanvasScaler componentInParent = ingameMenuPanel.GetComponentInParent<uGUI_CanvasScaler>();
		if (componentInParent != null && componentInParent.scaleMode > uGUI_CanvasScaler.ScaleMode.DontScale)
		{
			componentInParent.SetDirty();
		}
		CoroutineHost.StartCoroutine(GameSettings.SaveAsync(OnSave));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		DisplayManager.OnDisplayChanged += OnScreenChanged;
		AddTabs();
		HighlightCurrentTab();
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateAfterInput, OnUpdateCanvas);
	}

	protected override void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdateAfterInput, OnUpdateCanvas);
		DisplayManager.OnDisplayChanged -= OnScreenChanged;
		UnappliedSettings.Clear();
		base.OnDisable();
	}

	private void AddTabs()
	{
		AddGeneralTab();
		AddGraphicsTab();
		GameInput.PopulateSettings(this);
		AddAccessibilityTab();
		PlatformServices services = PlatformUtils.main.GetServices();
		if (services != null && services.GetName() == "Steam")
		{
			AddKeyRedemptionTab();
		}
		AddTroubleshootingTab();
	}

	[SuppressMessage("Subnautica.Rules", "AvoidStringConcatenation")]
	private void AddGeneralTab()
	{
		int tabIndex = AddTab("General");
		if (showLanguageOption)
		{
			languages = GetLanguageOptions(out var currentIndex);
			string[] languageKeys = GetLanguageKeys(languages);
			AddChoiceOption(tabIndex, "Language", languageKeys, currentIndex, OnLanguageChanged);
		}
		AddToggleOption(tabIndex, "RunInBackground", MiscSettings.runInBackground, OnRunInBackgroundChanged, "RunInBackgroundTooltip");
		AddToggleOption(tabIndex, "NewsEnabled", MiscSettings.newsEnabled, OnNewsEnabledChanged);
		AddHeading(tabIndex, "Subtitles");
		subtitlesOption = AddToggleOption(tabIndex, "SubtitlesEnabled", Language.main.showSubtitles, OnShowSubtitlesChanged);
		GameObject obj = AddSliderOption(tabIndex, "SubtitlesSpeed", Subtitles.speed, 1f, 70f, 15f, 1f, OnSubtitlesSpeedChanged, SliderLabelMode.Int, "0");
		obj.TryGetComponent<MenuTooltip>(out var component);
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		obj.AddComponent<SubtitlesSpeedTooltip>();
		AddHeading(tabIndex, "Display");
		if (!XRSettings.enabled)
		{
			string[] resolutionOptions = GetResolutionOptions(out resolutions);
			int currentResolutionIndex = GetCurrentResolutionIndex(resolutions);
			resolutionOption = AddChoiceOption(tabIndex, "Resolution", resolutionOptions, currentResolutionIndex, OnResolutionChanged);
			AddToggleOption(tabIndex, "Fullscreen", Screen.fullScreen, OnFullscreenChanged);
			AddToggleOption(tabIndex, "Vsync", QualitySettings.vSyncCount > 0, OnVSyncChanged);
			targetFrameRateOption = AddSliderOption(tabIndex, "FPSCap", GraphicsUtil.GetFrameRate(), GraphicsUtil.frameRateMin, GraphicsUtil.frameRateMax, 144f, 1f, OnTargetFrameRateChanged, SliderLabelMode.Int, "0");
			OnVSyncChanged(QualitySettings.vSyncCount > 0);
			int num = Display.displays.Length;
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = (i + 1).ToString();
			}
			AddChoiceOption(tabIndex, "Display", array, DisplayManager.GetCurrentDisplayIndex(), OnDisplayChanged);
			AddSliderOption(tabIndex, "FieldOfView", MiscSettings.fieldOfView, 40f, 90f, 60f, 1f, OnFovChanged, SliderLabelMode.Int, "0");
		}
		else
		{
			AddSliderOption(tabIndex, "VRRenderScale", XRSettings.eyeTextureResolutionScale, GameSettings.GetMinVrRenderScale(), 1f, 1f, 0.05f, OnVRRenderScaleChanged, SliderLabelMode.Percent, "0");
		}
		AddHeading(tabIndex, "Sound");
		AddSliderOption(tabIndex, "MasterVolume", SoundSystem.GetMasterVolume(), 1f, OnMasterVolumeChanged);
		AddSliderOption(tabIndex, "MusicVolume", SoundSystem.GetMusicVolume(), 1f, OnMusicVolumeChanged);
		AddSliderOption(tabIndex, "VoiceVolume", SoundSystem.GetVoiceVolume(), 1f, OnVoiceVolumeChanged);
		AddSliderOption(tabIndex, "AmbientVolume", SoundSystem.GetAmbientVolume(), 1f, OnAmbientVolumeChanged);
	}

	private void AddGraphicsTab()
	{
		int tabIndex = AddTab("Graphics");
		AddSliderOption(tabIndex, "Gamma", GammaCorrection.gamma, 0.1f, 2.8f, 1f, 0.01f, delegate(float value)
		{
			GammaCorrection.gamma = value;
		}, SliderLabelMode.Float, "0.00");
		int qualityPresetIndex = GetQualityPresetIndex();
		qualityPresetOption = AddChoiceOption(tabIndex, "Preset", presetOptions, qualityPresetIndex, OnQualityPresetChanged);
		ApplyQualityPreset(qualityPresetIndex);
		int currentIndex;
		string[] items = GetColorGradingOptions(out currentIndex);
		AddChoiceOption(tabIndex, "ColorGrading", items, currentIndex, OnColorGradingChanged);
		AddHeading(tabIndex, "Advanced");
		if ((bool)uGUI_MainMenu.main)
		{
			int currentIndex2;
			string[] detailOptions = GetDetailOptions(out currentIndex2);
			detailOption = AddChoiceOption(tabIndex, "Detail", detailOptions, currentIndex2, OnDetailChanged);
		}
		waterQualityOption = AddChoiceOption(tabIndex, "WaterQuality", WaterSurface.GetQualityOptions(), WaterSurface.GetQuality(), OnWaterQualityChanged);
		int currentIndex3;
		string[] antiAliasingOptions = GetAntiAliasingOptions(out currentIndex3);
		aaModeOption = AddChoiceOption(tabIndex, "Antialiasing", antiAliasingOptions, currentIndex3, OnAAmodeChanged);
		aaQualityOption = AddChoiceOption(tabIndex, "AntialiasingQuality", postFXQualityNames, UwePostProcessingManager.GetAaQuality(), OnAAqualityChanged);
		bloomOption = AddToggleOption(tabIndex, "Bloom", UwePostProcessingManager.GetBloomEnabled(), OnBloomChanged);
		if (!XRSettings.enabled)
		{
			lensDirtOption = AddToggleOption(tabIndex, "LensDirt", UwePostProcessingManager.GetBloomLensDirtEnabled(), OnBloomLensDirtChanged);
			dofOption = AddToggleOption(tabIndex, "DepthOfField", UwePostProcessingManager.GetDofEnabled(), OnDofChanged);
			motionBlurQualityOption = AddChoiceOption(tabIndex, "MotionBlurQuality", postFXQualityNames, UwePostProcessingManager.GetMotionBlurQuality(), OnMotionBlurQualityChanged);
		}
		aoQualityOption = AddChoiceOption(tabIndex, "AmbientOcclusion", postFXQualityNames, UwePostProcessingManager.GetAoQuality(), OnAOqualityChanged);
		if (!XRSettings.enabled)
		{
			ssrQualityOption = AddChoiceOption(tabIndex, "ScreenSpaceReflections", postFXQualityNames, UwePostProcessingManager.GetSsrQuality(), OnSSRqualityChanged);
			ditheringOption = AddToggleOption(tabIndex, "Dithering", UwePostProcessingManager.GetDitheringEnabled(), OnDitheringChanged);
		}
	}

	private void AddKeyRedemptionTab()
	{
		int tabIndex = AddTab("KeyRedemption");
		AddItem(tabIndex, keyRedemptionPrefab);
	}

	private void AddTroubleshootingTab()
	{
		int tabIndex = AddTab("Troubleshooting");
		AddItem(tabIndex, troubleshootingPrefab);
	}

	private void AddAccessibilityTab()
	{
		int tabIndex = AddTab("Accessibility");
		AddSliderOption(tabIndex, "UIScale", MiscSettings.GetUIScale(), 0.7f, 1.4f, 1f, 0.01f, delegate(float value)
		{
			MiscSettings.SetUIScale(value);
		}, SliderLabelMode.Float, "0.00");
		AddToggleOption(tabIndex, "PDAPause", MiscSettings.pdaPause, OnPDAPauseChanged);
		AddToggleOption(tabIndex, "Flashes", MiscSettings.flashes, OnFlashesChanged);
	}

	private string[] GetBindingOptionKeys()
	{
		List<string> list = new List<string>();
		foreach (GameInput.Device value in Enum.GetValues(typeof(GameInput.Device)))
		{
			foreach (GameInput.Button value2 in Enum.GetValues(typeof(GameInput.Button)))
			{
				if (GameInput.IsBindable(value, value2))
				{
					list.Add($"Option{value2.ToString()}");
				}
			}
		}
		return list.ToArray();
	}

	public string CompileTimeCheck(ILanguage language)
	{
		int currentIndex;
		string[] languageKeys = GetLanguageKeys(GetLanguageOptions(out currentIndex));
		string[] bindingOptionKeys = GetBindingOptionKeys();
		return language.CheckKeys(languageKeys) ?? language.CheckKeys(bindingOptionKeys);
	}

	public override bool OnBack()
	{
		if (base.OnBack())
		{
			return true;
		}
		if (UnappliedSettings.HasUnappliedSettings)
		{
			dialog.Show(Language.main.Get("ApplyModifiedSettings"), delegate(int option)
			{
				if (option == 1)
				{
					OnApplyButton();
				}
				else
				{
					UnappliedSettings.Clear();
					Close();
				}
			}, Language.main.Get("No"), Language.main.Get("Yes"));
			return true;
		}
		CoroutineHost.StartCoroutine(GameSettings.SaveAsync(OnSave));
		Close();
		return false;
	}
}
