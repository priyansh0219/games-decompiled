using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class PlatformUtils : MonoBehaviour
{
	public delegate void LoginFinishedDelegate(bool success);

	public delegate void ControllerDisconnectedDelegate();

	public const bool isEditor = false;

	public const bool isPlaying = true;

	public const bool isConsolePlatform = false;

	public const bool isSwitchPlatform = false;

	public const bool isPS4Platform = false;

	public const bool isPS5Platform = false;

	public const bool isXboxOnePlatform = false;

	public const bool isGameCoreScarlettPlatform = false;

	public const bool isPS4Pro = false;

	public const bool isXboxOneX = false;

	public const bool isPlaystationPlatform = false;

	public const bool isXboxPlatform = false;

	public const bool isShippingRelease = false;

	public const bool hasFixedCulture = false;

	public bool launchDeepLink;

	public LoginFinishedDelegate OnLoginFinished;

	public ControllerDisconnectedDelegate OnControllerDisconnected;

	private PlatformServices services;

	public const float DefaultUiScale = 1f;

	private const string prefabPath = "PlatformUtils";

	private static PlatformUtils _main;

	private static string _temporaryCachePath;

	private static readonly List<IOnQuitBehaviour> deferredRegisterQuitBehaviours = new List<IOnQuitBehaviour>();

	public static bool isWindowsStore => PlatformServicesWindows.IsPresent();

	public static PlatformUtils main
	{
		get
		{
			if (_main == null)
			{
				Initialize();
			}
			return _main;
		}
	}

	public static string temporaryCachePath
	{
		get
		{
			if (_temporaryCachePath == null)
			{
				_temporaryCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Unknown Worlds\\Subnautica").Replace('\\', '/');
			}
			return _temporaryCachePath;
		}
	}

	public static RenderTextureFormat defaultHDRFormat => RenderTextureFormat.DefaultHDR;

	public static RenderTextureFormat defaultAlphaHDRFormat => RenderTextureFormat.DefaultHDR;

	public static bool GetDevToolsEnabled()
	{
		return main.GetServices()?.GetDevToolsEnabled() ?? false;
	}

	public static void SetDevToolsEnabled(bool enabled)
	{
		main.GetServices()?.SetDevToolsEnabled(enabled);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void OnBeforeSceneLoadRuntimeMethod()
	{
	}

	public PlatformServices GetServices()
	{
		return services;
	}

	public static void Initialize()
	{
		if (!(_main != null))
		{
			GameObject gameObject = Resources.Load<GameObject>("PlatformUtils");
			if (gameObject == null)
			{
				Debug.LogError("Cannot find PlatformUtils prefab");
				Debug.Break();
			}
			else
			{
				UnityEngine.Object.Instantiate(gameObject).name = gameObject.name;
			}
		}
	}

	private void Awake()
	{
		if (_main != null)
		{
			Debug.LogError("Multiple PlatformUtils instances found in scene!", this);
			Debug.Break();
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
		else
		{
			_main = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			StartCoroutine(PlatformInitAsync());
		}
	}

	private IEnumerator PlatformInitAsync()
	{
		PlatformServices platformServices = null;
		if (platformServices == null && PlatformServicesSteam.IsPresent())
		{
			Debug.LogFormat("Initializing Steam services");
			PlatformServicesSteam steamServices = new PlatformServicesSteam();
			yield return steamServices.InitializeAsync();
			platformServices = steamServices;
		}
		if (platformServices == null && PlatformServicesEpic.IsPresent())
		{
			Debug.LogFormat("Initializing Epic services");
			PlatformServicesEpic platformServicesEpic = new PlatformServicesEpic();
			if (platformServicesEpic.Initialize())
			{
				platformServices = platformServicesEpic;
			}
		}
		if (platformServices == null && PlatformServicesDiscord.IsPresent())
		{
			Debug.LogFormat("Initializing Discord services");
			PlatformServicesDiscord discordServices = new PlatformServicesDiscord();
			yield return discordServices.InitializeAsync();
			platformServices = discordServices;
		}
		if (platformServices == null && PlatformServicesWindows.IsPresent())
		{
			Debug.LogFormat("Initializing Windows Store services");
			PlatformServicesWindows windowsServices = new PlatformServicesWindows();
			yield return windowsServices.InitializeAsync();
			platformServices = windowsServices;
		}
		if (platformServices == null && PlatformServicesArc.IsPresent())
		{
			Debug.LogFormat("Initializing Arc services");
			platformServices = new PlatformServicesArc();
		}
		if (platformServices == null && PlatformServicesRail.IsPresent())
		{
			Debug.LogFormat("Initializing WeGame services");
			PlatformServicesRail platformServicesRail = new PlatformServicesRail();
			if (platformServicesRail.Initialize())
			{
				platformServices = platformServicesRail;
			}
		}
		if (platformServices == null)
		{
			Application.Quit();
		}
		if (platformServices == null)
		{
			PlatformServicesNull nullServices = new PlatformServicesNull(PlatformServicesNull.DefaultSavePath);
			yield return nullServices.InitializeAsync();
			platformServices = nullServices;
		}
		if (platformServices.GetSupportsVirtualKeyboard())
		{
			TouchScreenKeyboardManager.SetKeyboard(new UWETouchScreenKeyboard());
		}
		if (!GameInput.IsInitialized)
		{
			IGameInput gameInput = new GameInputSystem();
			if (gameInput != null)
			{
				GameInput.Initialize(gameInput);
			}
		}
		services = platformServices;
		foreach (IOnQuitBehaviour deferredRegisterQuitBehaviour in deferredRegisterQuitBehaviours)
		{
			RegisterOnQuitBehaviour(deferredRegisterQuitBehaviour);
		}
		deferredRegisterQuitBehaviours.Clear();
	}

	private void OnDestroy()
	{
		if (services != null)
		{
			services.Shutdown();
			services = null;
		}
	}

	public UserStorage GetUserStorage()
	{
		return services.GetUserStorage();
	}

	public bool IsUserLoggedIn()
	{
		return services.IsUserLoggedIn();
	}

	public void StartLogOnUserAsync(int gamepadIndex)
	{
		StartCoroutine(LogOnUserAsync(gamepadIndex));
	}

	private IEnumerator LogOnUserAsync(int gamepadIndex)
	{
		PlatformServicesUtils.AsyncOperation asyncOperation = services.LogOnUserAsync(gamepadIndex);
		yield return asyncOperation;
		OnLoginFinished(asyncOperation.GetSuccessful());
	}

	public void LogOffUser()
	{
		services.LogOffUser();
	}

	public string GetLoggedInUserName()
	{
		string userName = services.GetUserName();
		if (userName != null)
		{
			return userName;
		}
		return string.Empty;
	}

	public bool ReconnectController(int gamepadIndex)
	{
		return ReconnectControllerImpl(gamepadIndex);
	}

	public string GetCurrentUserId()
	{
		return main.GetServices().GetUserId();
	}

	public static void OpenURL(string url, bool overlay = false)
	{
		PlatformServices platformServices = main.GetServices();
		if (platformServices != null)
		{
			platformServices.OpenURL(url, overlay);
			return;
		}
		Debug.LogWarningFormat("Failed to find platform services for opening a URL, using default method for URL - {0}", url);
		PlatformServicesUtils.DefaultOpenURL(url);
	}

	public static void AddTemporaryCamera()
	{
		Camera camera = new GameObject().AddComponent<Camera>();
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = Color.black;
		camera.cullingMask = 0;
	}

	public void OnGamepadStateChange(uint index, bool isConnected)
	{
		if (!isConnected && index == 1 && IsUserLoggedIn())
		{
			OnControllerDisconnected?.Invoke();
		}
	}

	private bool ReconnectControllerImpl(int gamepadIndex)
	{
		return services.ReconnectController(gamepadIndex);
	}

	private IEnumerator ScreenshotEncode(string fileName)
	{
		yield return new WaitForEndOfFrame();
		Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false)
		{
			name = "PlatformUtils.ScreenshotEncode"
		};
		texture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		texture.Apply();
		yield return null;
		byte[] bytes = texture.EncodeToPNG();
		File.WriteAllBytes(fileName, bytes);
		UnityEngine.Object.DestroyObject(texture);
	}

	public bool CaptureScreenshot(string fileName)
	{
		ScreenCapture.CaptureScreenshot(fileName);
		return true;
	}

	public static bool SupportsComputeShaders()
	{
		if (GraphicsUtil.IsOpenGL())
		{
			return false;
		}
		return SystemInfo.supportsComputeShaders;
	}

	public static void SetLightbarColor(Color inColor, int userIndex = 0)
	{
	}

	public static void ResetLightbarColor(int userIndex = 0)
	{
	}

	private void Update()
	{
		if (services != null)
		{
			services.Update();
		}
	}

	public static string GetCurrentCultureName()
	{
		return CultureInfo.CurrentCulture.Name;
	}

	public static void RegisterOnQuitBehaviour(IOnQuitBehaviour behaviour)
	{
		if (main.GetServices() == null)
		{
			deferredRegisterQuitBehaviours.Add(behaviour);
			Debug.Log("OnQuit behaviour queued for deferred registering, platform services aren't initialized.");
		}
		else
		{
			main.GetServices().RegisterOnQuitBehaviour(behaviour);
		}
	}

	public static void DeregisterOnQuitBehaviour(IOnQuitBehaviour behaviour)
	{
		if (_main == null || _main.services == null)
		{
			deferredRegisterQuitBehaviours.Remove(behaviour);
		}
		else
		{
			main.GetServices().DeregisterOnQuitBehaviour(behaviour);
		}
	}

	public DisplayOperationMode GetCurrentDisplayOperationMode()
	{
		if (services != null)
		{
			return services.GetCurrentDisplayOperationMode();
		}
		Debug.LogWarning("Missing platform services, returning default display mode.");
		return DisplayOperationMode.Default;
	}

	public float GetDefaultUiScale(DisplayOperationMode displayOperationMode)
	{
		if (services != null)
		{
			return services.GetDefaultUiScale(displayOperationMode);
		}
		Debug.LogWarning("Missing platform services, returning default UI scale.");
		return 1f;
	}
}
