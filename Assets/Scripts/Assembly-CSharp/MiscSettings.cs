using Gendarme;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
public static class MiscSettings
{
	public const float defaultFieldOfView = 60f;

	public const float minFieldOfView = 40f;

	public const float maxFieldOfView = 90f;

	public static string consoleHistory = string.Empty;

	public static bool cameraBobbing = true;

	public static string email = string.Empty;

	public static bool rememberEmail = false;

	public static bool hideEmailBox = false;

	public static float fieldOfView = 60f;

	public static bool flashes = true;

	public static bool newsEnabled = true;

	public static Utils.MonitoredValue<bool> isFlashesEnabled = new Utils.MonitoredValue<bool>();

	public static bool pdaPause = false;

	private static float uiScale = 1f;

	private static float uiScaleHandheld = 1.4f;

	public static bool runInBackground = true;

	public static void SetupDefaultSettings()
	{
		consoleHistory = string.Empty;
		cameraBobbing = true;
		email = string.Empty;
		rememberEmail = false;
		hideEmailBox = false;
		newsEnabled = true;
		fieldOfView = 60f;
	}

	public static void SetUIScale(float value, DisplayOperationMode mode = DisplayOperationMode.Current)
	{
		if (mode == DisplayOperationMode.Current)
		{
			mode = PlatformUtils.main.GetCurrentDisplayOperationMode();
		}
		switch (mode)
		{
		case DisplayOperationMode.Default:
			uiScale = value;
			break;
		case DisplayOperationMode.Handheld:
			uiScaleHandheld = value;
			break;
		default:
			Debug.LogErrorFormat("Unhandled {0} DisplayOperationMode in SetUIScale(). Value is not set.", mode);
			break;
		}
	}

	public static float GetUIScale(DisplayOperationMode mode = DisplayOperationMode.Current)
	{
		if (mode == DisplayOperationMode.Current)
		{
			mode = PlatformUtils.main.GetCurrentDisplayOperationMode();
		}
		switch (mode)
		{
		case DisplayOperationMode.Default:
			return uiScale;
		case DisplayOperationMode.Handheld:
			return uiScaleHandheld;
		default:
			Debug.LogErrorFormat("Unhandled {0} DisplayOperationMode in GetUIScale(). Returning default uiScale.", mode);
			return uiScale;
		}
	}

	public static void Update()
	{
		isFlashesEnabled.Update(flashes);
	}
}
