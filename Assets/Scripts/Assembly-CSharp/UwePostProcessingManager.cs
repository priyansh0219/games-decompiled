using System;
using Gendarme;
using UWE;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.XR;

public class UwePostProcessingManager : MonoBehaviour
{
	[AssertNotNull]
	public PostProcessingBehaviour behaviour;

	[AssertNotNull]
	public PostProcessingProfile defaultProfile;

	public float dofAdaptationSpeed = 50f;

	public float aboveWaterDofFocalLength = 28f;

	public float underWaterDofFocalLength = 44f;

	private static float pdaDofFocusDistance = 0.35f;

	private static float normalDofFocusDistance = 0.5f;

	private static float dofFocalLength;

	private static float targetDofFocalLength;

	private static float dofFocusDistance;

	private static float targetDofFocusDistance;

	private static PostProcessingProfile currentProfile;

	private static AntialiasingModel.Method aaMethod = AntialiasingModel.Method.Fxaa;

	private static int aaQuality = 1;

	private static int ssrQuality = 0;

	private static int aoQuality = 0;

	private static int motionBlurQuality = 0;

	private static int bloomQuality = 2;

	private static int colorGradingMode = 0;

	private static bool bloomEnabled = true;

	private static bool bloomLensDirtEnabled = true;

	private static bool dofEnabled = false;

	private static bool ditheringEnabled = true;

	private static bool settingsChanged = true;

	public static void SetAaMode(int _mode)
	{
		aaMethod = ((_mode >= 1) ? AntialiasingModel.Method.Taa : AntialiasingModel.Method.Fxaa);
		settingsChanged = true;
	}

	public static void SetAaQuality(int _quality)
	{
		aaQuality = _quality;
		settingsChanged = true;
	}

	public static void SetSsrQuality(int _quality)
	{
		ssrQuality = _quality;
		settingsChanged = true;
	}

	public static void SetAoQuality(int _quality)
	{
		aoQuality = _quality;
		settingsChanged = true;
	}

	public static void SetColorGradingMode(int _mode)
	{
		colorGradingMode = _mode;
		settingsChanged = true;
	}

	public static void ToggleBloom(bool _enableComp)
	{
		bloomEnabled = _enableComp;
		settingsChanged = true;
	}

	public static void ToggleBloomLensDirt(bool _enableComp)
	{
		bloomLensDirtEnabled = _enableComp;
		settingsChanged = true;
	}

	public static void SetMotionBlurQuality(int _quality)
	{
		motionBlurQuality = _quality;
		settingsChanged = true;
	}

	public static void ToggleDof(bool _enableComp)
	{
		dofEnabled = _enableComp;
		settingsChanged = true;
	}

	public static void ToggleDithering(bool _enableComp)
	{
		ditheringEnabled = _enableComp;
		settingsChanged = true;
	}

	public static int GetAaMode()
	{
		if (aaMethod != 0)
		{
			return 1;
		}
		return 0;
	}

	public static int GetAaQuality()
	{
		return aaQuality;
	}

	public static int GetSsrQuality()
	{
		return ssrQuality;
	}

	public static int GetAoQuality()
	{
		return aoQuality;
	}

	public static int GetColorGradingMode()
	{
		return colorGradingMode;
	}

	public static bool GetBloomEnabled()
	{
		return bloomEnabled;
	}

	public static bool GetBloomLensDirtEnabled()
	{
		return bloomLensDirtEnabled;
	}

	public static int GetMotionBlurQuality()
	{
		return motionBlurQuality;
	}

	public static bool GetDofEnabled()
	{
		return dofEnabled;
	}

	public static bool GetDitheringEnabled()
	{
		return ditheringEnabled;
	}

	public void SetAA(AntialiasingModel.Method aaMethod, int quality)
	{
		currentProfile.antialiasing.enabled = quality > 0;
		AntialiasingModel.Settings settings = currentProfile.antialiasing.settings;
		settings.method = aaMethod;
		if (aaMethod == AntialiasingModel.Method.Fxaa)
		{
			if (quality < 2)
			{
				settings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.ExtremePerformance;
			}
			else if (quality < 3)
			{
				settings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Default;
			}
			else if (quality < 4)
			{
				settings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.ExtremeQuality;
			}
		}
		currentProfile.antialiasing.settings = settings;
	}

	public void SetSSR(int quality)
	{
		currentProfile.screenSpaceReflection.enabled = quality > 0;
		ScreenSpaceReflectionModel.Settings settings = currentProfile.screenSpaceReflection.settings;
		if (quality < 2)
		{
			settings.reflection.reflectionQuality = ScreenSpaceReflectionModel.SSRResolution.Low;
			settings.reflection.iterationCount = 25;
			settings.reflection.reflectionBlur = 6f;
		}
		else if (quality < 3)
		{
			settings.reflection.iterationCount = 50;
			settings.reflection.reflectionQuality = ScreenSpaceReflectionModel.SSRResolution.Low;
			settings.reflection.reflectionBlur = 3f;
		}
		else if (quality < 4)
		{
			settings.reflection.reflectionQuality = ScreenSpaceReflectionModel.SSRResolution.High;
			settings.reflection.iterationCount = 75;
			settings.reflection.reflectionBlur = 1.5f;
		}
		currentProfile.screenSpaceReflection.settings = settings;
	}

	public void SetAO(int quality)
	{
		currentProfile.ambientOcclusion.enabled = quality > 0;
		AmbientOcclusionModel.Settings settings = currentProfile.ambientOcclusion.settings;
		settings.downsampling = true;
		if (quality < 2)
		{
			settings.sampleCount = AmbientOcclusionModel.SampleCount.Low;
		}
		else if (quality < 3)
		{
			settings.sampleCount = AmbientOcclusionModel.SampleCount.Medium;
		}
		else if (quality < 4)
		{
			settings.sampleCount = AmbientOcclusionModel.SampleCount.High;
		}
		currentProfile.ambientOcclusion.settings = settings;
	}

	public void SetBloom(int quality)
	{
		currentProfile.bloom.enabled = quality > 0;
		BloomModel.Settings settings = currentProfile.bloom.settings;
		if (quality < 2)
		{
			settings.lensDirt.intensity = 0f;
		}
		else if (quality < 3)
		{
			settings.lensDirt.intensity = defaultProfile.bloom.settings.lensDirt.intensity;
		}
		currentProfile.bloom.settings = settings;
	}

	public void SetColorGrading(int quality)
	{
		currentProfile.colorGrading.enabled = quality > 0;
		ColorGradingModel.Settings settings = currentProfile.colorGrading.settings;
		if (quality < 2)
		{
			settings.tonemapping.tonemapper = ColorGradingModel.Tonemapper.Neutral;
		}
		else if (quality < 3)
		{
			settings.tonemapping.tonemapper = ColorGradingModel.Tonemapper.ACES;
		}
		currentProfile.colorGrading.settings = settings;
	}

	public void SetMotionBlur(int quality)
	{
		currentProfile.motionBlur.enabled = quality > 0;
		int sampleCount = 6;
		if (quality < 2)
		{
			sampleCount = 6;
		}
		else if (quality < 3)
		{
			sampleCount = 8;
		}
		if (quality < 4)
		{
			sampleCount = 10;
		}
		MotionBlurModel.Settings settings = currentProfile.motionBlur.settings;
		settings.sampleCount = sampleCount;
		currentProfile.motionBlur.settings = settings;
	}

	public void SetDof(int quality)
	{
		currentProfile.depthOfField.enabled = quality > 0;
	}

	public void SetDithering(int quality)
	{
		currentProfile.dithering.enabled = quality > 0;
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void Awake()
	{
		if (currentProfile == null)
		{
			currentProfile = UnityEngine.Object.Instantiate(defaultProfile);
		}
		currentProfile.chromaticAberration.enabled = false;
		behaviour.profile = currentProfile;
		ClosePDA();
		ApplySettingsToProfile();
		DevConsole.RegisterConsoleCommand(this, "debugrendermode");
	}

	private static void ResetSettingsChanged()
	{
		settingsChanged = false;
	}

	private static void UpdateDof(PostProcessingProfile currentProfile, float aboveWaterDofFocalLength, float underWaterDofFocalLength, float dofAdaptationSpeed)
	{
		if (!dofEnabled || XRSettings.enabled || MainCamera.camera == null || Player.main == null)
		{
			return;
		}
		if (MainCamera.camera.transform.position.y > 0f || Player.main.IsInside() || Player.main.precursorOutOfWater)
		{
			targetDofFocalLength = aboveWaterDofFocalLength;
		}
		else
		{
			targetDofFocalLength = underWaterDofFocalLength;
		}
		bool flag = !Mathf.Approximately(dofFocalLength, targetDofFocalLength);
		bool flag2 = !Mathf.Approximately(dofFocusDistance, targetDofFocusDistance);
		if (flag || flag2)
		{
			DepthOfFieldModel.Settings settings = currentProfile.depthOfField.settings;
			if (flag)
			{
				dofFocalLength = Mathf.MoveTowards(dofFocalLength, targetDofFocalLength, dofAdaptationSpeed * Time.deltaTime);
				settings.focalLength = dofFocalLength;
			}
			if (flag2)
			{
				dofFocusDistance = Mathf.MoveTowards(dofFocusDistance, targetDofFocusDistance, dofAdaptationSpeed * Time.deltaTime);
				settings.focusDistance = dofFocusDistance;
			}
			currentProfile.depthOfField.settings = settings;
		}
	}

	public static void OpenPDA()
	{
		targetDofFocusDistance = pdaDofFocusDistance;
	}

	public static void ClosePDA()
	{
		targetDofFocusDistance = normalDofFocusDistance;
	}

	private bool AreAllEffectsDisabled()
	{
		if (!currentProfile.antialiasing.enabled && !currentProfile.screenSpaceReflection.enabled && !currentProfile.ambientOcclusion.enabled && !currentProfile.motionBlur.enabled && !currentProfile.colorGrading.enabled && !currentProfile.bloom.enabled && !currentProfile.depthOfField.enabled)
		{
			return !currentProfile.dithering.enabled;
		}
		return false;
	}

	private void Update()
	{
		if (settingsChanged)
		{
			ApplySettingsToProfile();
			ResetSettingsChanged();
		}
		UpdateDof(currentProfile, aboveWaterDofFocalLength, underWaterDofFocalLength, dofAdaptationSpeed);
	}

	private void ApplySettingsToProfile()
	{
		behaviour.enabled = false;
		int num = 0;
		if (bloomEnabled)
		{
			num = 1;
			if (bloomLensDirtEnabled && !XRSettings.enabled)
			{
				num = 2;
			}
		}
		if (XRSettings.enabled)
		{
			SetAA(AntialiasingModel.Method.Fxaa, aaQuality);
			SetBloom((int)Mathf.Clamp01(num));
			SetSSR(0);
			SetMotionBlur(0);
			SetDof(0);
			SetDithering(0);
		}
		else
		{
			SetAA(aaMethod, aaQuality);
			SetBloom(num);
			SetSSR(ssrQuality);
			SetMotionBlur(motionBlurQuality);
			SetDof(dofEnabled ? 1 : 0);
			SetDithering(ditheringEnabled ? 1 : 0);
		}
		SetAO(aoQuality);
		SetColorGrading(colorGradingMode);
		if (!AreAllEffectsDisabled())
		{
			behaviour.enabled = true;
		}
	}

	private void OnDestroy()
	{
		DepthOfFieldModel.Settings settings = currentProfile.depthOfField.settings;
		dofFocalLength = aboveWaterDofFocalLength;
		settings.focalLength = dofFocalLength;
		dofFocusDistance = normalDofFocusDistance;
		settings.focusDistance = dofFocusDistance;
		currentProfile.depthOfField.settings = settings;
	}

	private void OnConsoleCommand_debugrendermode(NotificationCenter.Notification n)
	{
		if (n != null && n.data != null && n.data.Count > 0 && UWE.Utils.TryParseEnum<BuiltinDebugViewsModel.Mode>((string)n.data[0], out var result))
		{
			BuiltinDebugViewsModel.Settings settings = currentProfile.debugViews.settings;
			settings.mode = result;
			currentProfile.debugViews.settings = settings;
			ErrorMessage.AddDebug($"renderdebugmode now {currentProfile.debugViews.settings.mode}");
		}
		else
		{
			ErrorMessage.AddDebug(string.Format("Usage: renderdebugmode [mode] where mode is one of:\n{0}", string.Join("\n", Enum.GetNames(typeof(BuiltinDebugViewsModel.Mode)))));
		}
	}
}
