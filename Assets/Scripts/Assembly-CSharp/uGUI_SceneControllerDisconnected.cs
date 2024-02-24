using System;
using TMPro;
using UnityEngine;

public class uGUI_SceneControllerDisconnected : MonoBehaviour
{
	[AssertNotNull]
	public TextMeshProUGUI text1;

	[AssertNotNull]
	public TextMeshProUGUI text2;

	[AssertNotNull]
	public TextMeshProUGUI usernameText;

	private bool initialized;

	private PlatformUtils platformUtils;

	private bool isMainMenuHidden;

	[AssertLocalization]
	private const string controllerDisconnected1Key = "ControllerDisconnected1";

	[AssertLocalization]
	private const string controllerDisconnected2Key = "ControllerDisconnected2";

	public void Initialize()
	{
		platformUtils = PlatformUtils.main;
		PlatformUtils obj = platformUtils;
		obj.OnControllerDisconnected = (PlatformUtils.ControllerDisconnectedDelegate)Delegate.Combine(obj.OnControllerDisconnected, new PlatformUtils.ControllerDisconnectedDelegate(OnControllerDisconnected));
		Language.OnLanguageChanged += OnLanguageChanged;
		initialized = true;
	}

	public void Deinitialize()
	{
		if (initialized)
		{
			Language.OnLanguageChanged -= OnLanguageChanged;
			PlatformUtils obj = platformUtils;
			obj.OnControllerDisconnected = (PlatformUtils.ControllerDisconnectedDelegate)Delegate.Remove(obj.OnControllerDisconnected, new PlatformUtils.ControllerDisconnectedDelegate(OnControllerDisconnected));
			initialized = false;
		}
	}

	private void OnDestroy()
	{
		Deinitialize();
	}

	private void Awake()
	{
		UpdateMessageText();
		usernameText.text = PlatformUtils.main.GetLoggedInUserName();
	}

	private void UpdateMessageText()
	{
		text1.text = Language.main.Get("ControllerDisconnected1");
		text2.text = Language.main.Get("ControllerDisconnected2");
	}

	private void OnLanguageChanged()
	{
		UpdateMessageText();
	}

	private void LateUpdate()
	{
		int num = -1;
		for (int i = 0; i < 8; i++)
		{
			int num2 = 350 + i * 20;
			for (int j = 0; j < 20; j++)
			{
				if (Input.GetKeyUp((KeyCode)(num2 + j)))
				{
					num = i;
				}
			}
		}
		if (num < 0)
		{
			if (Mathf.Max(Input.GetAxis("ControllerAxis3"), 0f) > 0.2f)
			{
				num = 0;
			}
			if (Mathf.Max(0f - Input.GetAxis("ControllerAxis3"), 0f) > 0.2f)
			{
				num = 0;
			}
		}
		if (num != -1 && PlatformUtils.main.ReconnectController(num))
		{
			base.gameObject.SetActive(value: false);
			if (IngameMenu.main != null && isMainMenuHidden)
			{
				IngameMenu.main.gameObject.GetComponent<Canvas>().enabled = true;
				isMainMenuHidden = false;
			}
		}
	}

	private void OnControllerDisconnected()
	{
		if (WaitScreen.IsWaiting)
		{
			Invoke("OnControllerDisconnected", 0.1f);
			return;
		}
		usernameText.text = PlatformUtils.main.GetLoggedInUserName();
		base.gameObject?.SetActive(value: true);
		if (CanUseIngameMenu() && !isMainMenuHidden)
		{
			isMainMenuHidden = true;
			IngameMenu.main.Open();
			IngameMenu.main.gameObject.GetComponent<Canvas>().enabled = false;
		}
	}

	private bool CanUseIngameMenu()
	{
		if (IngameMenu.main != null)
		{
			if ((bool)FPSInputModule.current)
			{
				return !FPSInputModule.current.lockPauseMenu;
			}
			return false;
		}
		return false;
	}
}
