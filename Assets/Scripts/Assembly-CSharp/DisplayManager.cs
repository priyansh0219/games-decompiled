using System;
using UnityEngine;
using UnityEngine.XR;

public class DisplayManager : MonoBehaviour
{
	private const string prefsKeyMonitor = "UnitySelectMonitor";

	private static DisplayManager instance;

	private static int currentDisplayIndex = -1;

	private Resolution resolution;

	private bool fullscreen;

	private int vSyncCount;

	private float vrRenderScale;

	public static event Action OnDisplayChanged;

	public static void SetResolution(int width, int height, bool fullscreen)
	{
		Screen.SetResolution(width, height, fullscreen);
	}

	public static Resolution GetResolution()
	{
		Resolution result = default(Resolution);
		result.width = Screen.width;
		result.height = Screen.height;
		return result;
	}

	public static bool GetDisplayChangeRequiresRestart()
	{
		return GetCurrentDisplayIndex() != PlayerPrefs.GetInt("UnitySelectMonitor");
	}

	public static void SetDesiredDisplayIndex(int index)
	{
		GetCurrentDisplayIndex();
		PlayerPrefs.SetInt("UnitySelectMonitor", index);
	}

	public static int GetCurrentDisplayIndex()
	{
		if (currentDisplayIndex < 0)
		{
			int num = Display.displays.Length;
			currentDisplayIndex = PlayerPrefs.GetInt("UnitySelectMonitor", -1);
			if (currentDisplayIndex < 0 || currentDisplayIndex >= num)
			{
				currentDisplayIndex = 0;
			}
		}
		return currentDisplayIndex;
	}

	private void Awake()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		Initialize();
	}

	private void Initialize()
	{
		resolution.width = Screen.width;
		resolution.height = Screen.height;
		fullscreen = Screen.fullScreen;
		vSyncCount = QualitySettings.vSyncCount;
		vrRenderScale = XRSettings.eyeTextureResolutionScale;
		GetCurrentDisplayIndex();
	}

	private void Update()
	{
		Resolution resolution = GetResolution();
		if (resolution.width != this.resolution.width || resolution.height != this.resolution.height || Screen.fullScreen != fullscreen || QualitySettings.vSyncCount != vSyncCount || XRSettings.eyeTextureResolutionScale != vrRenderScale)
		{
			this.resolution = resolution;
			fullscreen = Screen.fullScreen;
			vSyncCount = QualitySettings.vSyncCount;
			vrRenderScale = XRSettings.eyeTextureResolutionScale;
			if (DisplayManager.OnDisplayChanged != null)
			{
				DisplayManager.OnDisplayChanged();
			}
		}
	}
}
