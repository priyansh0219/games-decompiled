using System;
using System.Collections;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class uGUI : MonoBehaviour, IOnQuitBehaviour
{
	private const ManagedUpdate.Queue pdaUpdateQueue = ManagedUpdate.Queue.UpdatePDA;

	private const string mainLevelName = "Main";

	private const string uGUIPrefabPath = "uGUI.prefab";

	private static AsyncOperationHandle<GameObject> uGUIPrefabRequest;

	private static bool isTerminating = false;

	private static uGUI _main;

	private static int _isMainLevel = -1;

	public static bool interactable = true;

	private static readonly Color colorInteractable = new Color(1f, 1f, 1f, 1f);

	private static readonly Color colorDisabled = new Color(0.6f, 0.6f, 0.6f, 1f);

	public static Color? overrideColor;

	[AssertNotNull]
	public GameObject prefabNotificationLabel;

	[AssertNotNull]
	public Canvas screenCanvas;

	[AssertNotNull]
	public uGUI_SceneHUD hud;

	[AssertNotNull]
	public uGUI_SceneLoading loading;

	[AssertNotNull]
	public uGUI_SceneRespawning respawning;

	[AssertNotNull]
	public uGUI_SceneIntro intro;

	[AssertNotNull]
	public uGUI_HardcoreGameOver hardcoreGameOver;

	[AssertNotNull]
	public uGUI_UserInput userInput;

	[AssertNotNull]
	public uGUI_QuickSlots quickSlots;

	[AssertNotNull]
	public uGUI_Overlays overlays;

	[AssertNotNull]
	public uGUI_SceneConfirmation confirmation;

	[AssertNotNull]
	public uGUI_SceneProgressBar progressBar;

	[AssertNotNull]
	public uGUI_SceneControllerDisconnected controllerDisconnected;

	[AssertNotNull]
	public uGUI_ItemSelector itemSelector;

	[AssertNotNull]
	public uGUI_CraftingMenu craftingMenu;

	[AssertNotNull]
	public uGUI_PinnedRecipes pinnedRecipes;

	public GameObject barsPanel;

	[NonSerialized]
	public uGUI_Dialog dialog;

	public static uGUI main
	{
		get
		{
			if (_main == null && !isTerminating)
			{
				Debug.LogError("uGUI was accessed before it could be loaded");
			}
			return _main;
		}
	}

	public static bool isMainLevel
	{
		get
		{
			if (_isMainLevel == -1)
			{
				UpdateLevelIdentifier();
			}
			return _isMainLevel == 1;
		}
	}

	public static bool isIntro
	{
		get
		{
			if (main != null)
			{
				return main.intro.showing;
			}
			return false;
		}
	}

	public static bool isInitialized => _main != null;

	private void Awake()
	{
		if (_main != null)
		{
			Debug.LogError("Multiple uGUI instances found in scene!", this);
			UnityEngine.Object.DestroyImmediate(base.gameObject);
			return;
		}
		_main = this;
		PlatformUtils.RegisterOnQuitBehaviour(this);
		SceneManager.sceneLoaded += OnSceneLoaded;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdatePDA, PDA.PerformUpdate);
		controllerDisconnected.Initialize();
	}

	private void Update()
	{
		uGUI_CanvasScaler.uiScale = MiscSettings.GetUIScale();
		uGUI_GraphicRaycaster.UpdateGraphicRaycasters();
		FreezeTime.Set(FreezeTime.Id.TextInput, TouchScreenKeyboardManager.visible ? 1f : 0f);
	}

	private void OnDestroy()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdatePDA, PDA.PerformUpdate);
		PlatformUtils.DeregisterOnQuitBehaviour(this);
		SceneManager.sceneLoaded -= OnSceneLoaded;
		controllerDisconnected.Deinitialize();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
	{
		UpdateLevelIdentifier();
	}

	private void OnApplicationQuit()
	{
		OnQuit();
	}

	public void OnQuit()
	{
		isTerminating = true;
	}

	public static IEnumerator InitializeAsync()
	{
		if (_main != null)
		{
			yield break;
		}
		if (!uGUIPrefabRequest.IsValid())
		{
			uGUIPrefabRequest = AddressablesUtility.LoadAsync<GameObject>("uGUI.prefab");
			yield return uGUIPrefabRequest;
			if (uGUIPrefabRequest.Status == AsyncOperationStatus.Failed)
			{
				Debug.LogError("Cannot find main uGUI prefab in Resources folder at path 'uGUI.prefab'");
				Debug.Break();
				yield break;
			}
		}
		UWE.Utils.InstantiateWrap(uGUIPrefabRequest.Result);
	}

	public static void Deinitialize()
	{
		isTerminating = true;
		if (_main != null)
		{
			UWE.Utils.DestroyWrap(_main.gameObject);
			_main = null;
			AddressablesUtility.QueueRelease(ref uGUIPrefabRequest);
		}
	}

	public void SetVisible(bool visible)
	{
		Canvas[] componentsInChildren = GetComponentsInChildren<Canvas>();
		foreach (Canvas canvas in componentsInChildren)
		{
			if (!(canvas.sortingLayerName == "DepthClear"))
			{
				canvas.enabled = visible;
			}
		}
	}

	public static float DefaultDeltaTimeProvider()
	{
		return Time.deltaTime;
	}

	private void HideForScreenshots()
	{
		SetVisible(visible: false);
	}

	private void UnhideForScreenshots()
	{
		SetVisible(visible: true);
	}

	private static void UpdateLevelIdentifier()
	{
		if (Application.loadedLevelName.StartsWith("Main", StringComparison.Ordinal))
		{
			_isMainLevel = 1;
		}
		else
		{
			_isMainLevel = 0;
		}
	}
}
