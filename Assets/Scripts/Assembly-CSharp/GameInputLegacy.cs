using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Platform.Utils;
using UWE;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class GameInputLegacy : IGameInput
{
	private class RebindOperation
	{
		public GameInput.Device device;

		public GameInput.Button action;

		public GameInput.BindingSet bindingSet;

		public Action<int> callback;

		public void Done(int state)
		{
			GameInput.ClearInput();
			Action<int> action = callback;
			callback = null;
			try
			{
				action?.Invoke(state);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private enum AnalogAxis
	{
		ControllerRightStickX = 0,
		ControllerRightStickY = 1,
		ControllerLeftStickX = 2,
		ControllerLeftStickY = 3,
		ControllerLeftTrigger = 4,
		ControllerRightTrigger = 5,
		ControllerDPadX = 6,
		ControllerDPadY = 7,
		MouseX = 8,
		MouseY = 9,
		MouseWheel = 10
	}

	private struct InputState
	{
		public GameInput.InputStateFlags flags;

		public float timeDown;
	}

	public enum ControllerLayout
	{
		Automatic = 0,
		Xbox360 = 1,
		XboxOne = 2,
		PS4 = 3,
		Switch = 4,
		Scarlett = 5,
		PS5 = 6
	}

	private struct Input
	{
		public string name;

		public KeyCode keyCode;

		public AnalogAxis axis;

		public bool axisPositive;

		public GameInput.Device device;

		public float axisDeadZone;
	}

	[AssertLocalization(1)]
	private const string keepControllerLayoutLabel = "KeepControllerLayout";

	[AssertLocalization]
	private const string controllerLayoutLabel = "ControllerLayout";

	[AssertLocalization]
	private static readonly string[] controllerLayoutOptions = GetControllerLayoutOptions();

	[AssertLocalization]
	private const string customStick = "Custom";

	[AssertLocalization]
	private const string leftStickUp = "ControllerLeftStickUp";

	[AssertLocalization]
	private const string leftStickDown = "ControllerLeftStickDown";

	[AssertLocalization]
	private const string leftStickLeft = "ControllerLeftStickLeft";

	[AssertLocalization]
	private const string leftStickRight = "ControllerLeftStickRight";

	[AssertLocalization]
	private const string rightStickUp = "ControllerRightStickUp";

	[AssertLocalization]
	private const string rightStickDown = "ControllerRightStickDown";

	[AssertLocalization]
	private const string rightStickLeft = "ControllerRightStickLeft";

	[AssertLocalization]
	private const string rightStickRight = "ControllerRightStickRight";

	[AssertLocalization]
	private const string optionMoveLabel = "OptionMove";

	[AssertLocalization]
	private const string optionLookLabel = "OptionLook";

	private const float deadZoneLeftStick = 0.2f;

	private const float deadZoneRightStick = 0.2f;

	private const float defaultMouseSensitivity = 0.15f;

	private const float defaultControllerSensitivity = 0.405f;

	private HashSet<GameInput.Button> allowedStickConflicts = new HashSet<GameInput.Button>
	{
		GameInput.Button.MoveForward,
		GameInput.Button.MoveBackward,
		GameInput.Button.MoveLeft,
		GameInput.Button.MoveRight,
		GameInput.Button.LookUp,
		GameInput.Button.LookDown,
		GameInput.Button.LookLeft,
		GameInput.Button.LookRight
	};

	private static readonly KeyCode[] allKeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));

	private static readonly AnalogAxis[] allAnalogAxes = (AnalogAxis[])Enum.GetValues(typeof(AnalogAxis));

	private bool controllerEnabled = true;

	private bool invertMouse;

	private bool invertController;

	private float mouseSensitivity = 0.15f;

	private Vector2 controllerSensitivity = new Vector2(0.405f, 0.405f);

	private ControllerLayout chosenControllerLayout;

	private bool rumbleEnabled = true;

	private float[] axisValues;

	private float[] lastAxisValues;

	private InputState[] inputStates;

	private Array3<int> buttonBindings;

	private int numDevices;

	private int numButtons;

	private int numBindingSets;

	private List<Input> inputs = new List<Input>();

	private GameInput.Device lastDevice;

	private ControllerLayout automaticControllerLayout = ControllerLayout.Xbox360;

	private bool keyboardAvailable;

	private bool controllerAvailable;

	private Coroutine updateAvailableDevicesRoutine;

	private uGUI_Choice controllerLayoutOption;

	private uGUI_Choice controllerMoveStickOption;

	private uGUI_Choice controllerLookStickOption;

	private List<uGUI_Bindings> bindings = new List<uGUI_Bindings>();

	private int[] lastInputPressed;

	private RebindOperation rebindOperation;

	private static readonly string[] allAxisNames = new string[25]
	{
		"Forward", "Vertical", "Mouse X", "Mouse Y", "Mouse ScrollWheel", "Submit", "Cancel", "Oculus_GearVR_LThumbstickX", "Oculus_GearVR_LThumbstickY", "Oculus_GearVR_RThumbstickX",
		"Oculus_GearVR_RThumbstickY", "Oculus_GearVR_DpadX", "Oculus_GearVR_DpadY", "Oculus_GearVR_LIndexTrigger", "Oculus_GearVR_RIndexTrigger", "ControllerAxis1", "ControllerAxis2", "ControllerAxis3", "ControllerAxis4", "ControllerAxis5",
		"ControllerAxis6", "ControllerAxis7", "ControllerAxis8", "ControllerAxis9", "ControllerAxis10"
	};

	private float MouseSensitivity
	{
		get
		{
			return mouseSensitivity;
		}
		set
		{
			mouseSensitivity = value;
		}
	}

	private Vector2 ControllerSensitivity
	{
		get
		{
			return controllerSensitivity;
		}
		set
		{
			controllerSensitivity = value;
		}
	}

	private bool InvertMouse
	{
		get
		{
			return invertMouse;
		}
		set
		{
			invertMouse = value;
		}
	}

	private bool InvertController
	{
		get
		{
			return invertController;
		}
		set
		{
			invertController = value;
		}
	}

	private bool ControllerEnabled
	{
		get
		{
			return controllerEnabled;
		}
		set
		{
			if (XRSettings.enabled)
			{
				value = true;
			}
			controllerEnabled = value;
			GameInput.SetBindingsChanged();
		}
	}

	private bool RumbleEnabled
	{
		get
		{
			return rumbleEnabled;
		}
		set
		{
			rumbleEnabled = value;
		}
	}

	private ControllerLayout ChosenControllerLayout
	{
		get
		{
			return chosenControllerLayout;
		}
		set
		{
			chosenControllerLayout = value;
			GameInput.SetBindingsChanged();
		}
	}

	public string Id => "GameInputLegacy";

	public bool IsRebinding => rebindOperation != null;

	public GameInput.Device PrimaryDevice => lastDevice;

	public bool AnyKeyDown => InputUtils.anyKeyDown;

	private static string GetDisplayText(ControllerLayout layout, string binding)
	{
		switch (layout)
		{
		case ControllerLayout.PS4:
			switch (binding)
			{
			case "ControllerButtonA":
				return "ControllerButtonPs4Cross";
			case "ControllerButtonB":
				return "ControllerButtonPs4Circle";
			case "ControllerButtonY":
				return "ControllerButtonPs4Triangle";
			case "ControllerButtonX":
				return "ControllerButtonPs4Square";
			case "ControllerButtonBack":
				return "ControllerButtonPs4TouchPad";
			case "ControllerButtonHome":
				return "ControllerButtonPs4Options";
			case "ControllerButtonLeftBumper":
				return "ControllerPs4L1";
			case "ControllerButtonRightBumper":
				return "ControllerPs4R1";
			case "ControllerLeftTrigger":
				return "ControllerPs4L2";
			case "ControllerRightTrigger":
				return "ControllerPs4R2";
			case "ControllerButtonLeftStick":
				return "ControllerButtonPs4LeftStick";
			case "ControllerButtonRightStick":
				return "ControllerButtonPs4RightStick";
			case "ControllerDPadRight":
				return "ControllerPs4DPadRight";
			case "ControllerDPadLeft":
				return "ControllerPs4DPadLeft";
			case "ControllerDPadUp":
				return "ControllerPs4DPadUp";
			case "ControllerDPadDown":
				return "ControllerPs4DPadDown";
			}
			break;
		case ControllerLayout.PS5:
			switch (binding)
			{
			case "ControllerButtonA":
				return "ControllerButtonPs4Cross";
			case "ControllerButtonB":
				return "ControllerButtonPs4Circle";
			case "ControllerButtonY":
				return "ControllerButtonPs4Triangle";
			case "ControllerButtonX":
				return "ControllerButtonPs4Square";
			case "ControllerButtonBack":
				return "ControllerButtonPs5TouchPad";
			case "ControllerButtonHome":
				return "ControllerButtonPs5Options";
			case "ControllerButtonLeftBumper":
				return "ControllerPs4L1";
			case "ControllerButtonRightBumper":
				return "ControllerPs4R1";
			case "ControllerLeftTrigger":
				return "ControllerPs5L2";
			case "ControllerRightTrigger":
				return "ControllerPs5R2";
			case "ControllerButtonLeftStick":
				return "ControllerButtonPs4LeftStick";
			case "ControllerButtonRightStick":
				return "ControllerButtonPs4RightStick";
			case "ControllerDPadRight":
				return "ControllerPs4DPadRight";
			case "ControllerDPadLeft":
				return "ControllerPs4DPadLeft";
			case "ControllerDPadUp":
				return "ControllerPs4DPadUp";
			case "ControllerDPadDown":
				return "ControllerPs4DPadDown";
			}
			break;
		case ControllerLayout.Switch:
			switch (binding)
			{
			case "ControllerButtonA":
				return "ControllerSwitchA";
			case "ControllerButtonB":
				return "ControllerSwitchB";
			case "ControllerButtonX":
				return "ControllerSwitchY";
			case "ControllerButtonY":
				return "ControllerSwitchX";
			case "ControllerButtonBack":
				return "ControllerSwitch-";
			case "ControllerButtonHome":
				return "ControllerSwitch+";
			case "ControllerButtonLeftBumper":
				return "ControllerSwitchL";
			case "ControllerButtonRightBumper":
				return "ControllerSwitchR";
			case "ControllerLeftTrigger":
				return "ControllerSwitchZL";
			case "ControllerRightTrigger":
				return "ControllerSwitchZR";
			case "ControllerDPadRight":
				return "ControllerSwitchPadRight";
			case "ControllerDPadLeft":
				return "ControllerSwitchPadLeft";
			case "ControllerDPadUp":
				return "ControllerSwitchPadUp";
			case "ControllerDPadDown":
				return "ControllerSwitchPadDown";
			case "ControllerButtonLeftStick":
				return "ControllerSwitchButtonLeftStick";
			case "ControllerButtonRightStick":
				return "ControllerSwitchButtonRightStick";
			}
			break;
		}
		return binding;
	}

	private void AddKeyInput(string name, KeyCode keyCode, GameInput.Device device)
	{
		Input item = default(Input);
		item.name = name;
		item.keyCode = keyCode;
		item.device = device;
		inputs.Add(item);
	}

	private void AddAxisInput(string name, AnalogAxis axis, bool axisPositive, GameInput.Device device, float deadzone = 0f)
	{
		Input item = default(Input);
		item.name = name;
		item.keyCode = KeyCode.None;
		item.axis = axis;
		item.axisPositive = axisPositive;
		item.axisDeadZone = deadzone;
		item.device = device;
		inputs.Add(item);
	}

	private void ClearBindings(GameInput.Device device)
	{
		int inputIndex = -1;
		for (int i = 0; i < numButtons; i++)
		{
			for (int j = 0; j < numBindingSets; j++)
			{
				SetBindingInternal(device, (GameInput.Button)i, (GameInput.BindingSet)j, inputIndex);
			}
		}
	}

	private void SetupDefaultKeyboardBindings()
	{
		ClearBindings(GameInput.Device.Keyboard);
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.Jump, GameInput.BindingSet.Primary, "Space");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.PDA, GameInput.BindingSet.Primary, "Tab");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.Deconstruct, GameInput.BindingSet.Primary, "Q");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.Exit, GameInput.BindingSet.Primary, "E");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.LeftHand, GameInput.BindingSet.Primary, "MouseButtonLeft");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.RightHand, GameInput.BindingSet.Primary, "MouseButtonRight");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.AltTool, GameInput.BindingSet.Primary, "F");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.Slot1, GameInput.BindingSet.Primary, "1");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.Slot2, GameInput.BindingSet.Primary, "2");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.Slot3, GameInput.BindingSet.Primary, "3");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.Slot4, GameInput.BindingSet.Primary, "4");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.Slot5, GameInput.BindingSet.Primary, "5");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.TakePicture, GameInput.BindingSet.Primary, "F11");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.Reload, GameInput.BindingSet.Primary, "R");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.MoveForward, GameInput.BindingSet.Primary, "W");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.MoveBackward, GameInput.BindingSet.Primary, "S");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.MoveLeft, GameInput.BindingSet.Primary, "A");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.MoveRight, GameInput.BindingSet.Primary, "D");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.MoveUp, GameInput.BindingSet.Primary, "Space");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.MoveDown, GameInput.BindingSet.Primary, "C");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.AutoMove, GameInput.BindingSet.Primary, "X");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.Sprint, GameInput.BindingSet.Primary, "LeftShift");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.CycleNext, GameInput.BindingSet.Primary, "MouseWheelUp");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.CyclePrev, GameInput.BindingSet.Primary, "MouseWheelDown");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.CycleNext, GameInput.BindingSet.Secondary, "LeftBracket");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.CyclePrev, GameInput.BindingSet.Secondary, "RightBracket");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.UISubmit, GameInput.BindingSet.Primary, "MouseButtonLeft");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.UICancel, GameInput.BindingSet.Primary, "MouseButtonRight");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.UICancel, GameInput.BindingSet.Secondary, "Escape");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.UIMenu, GameInput.BindingSet.Primary, "Escape");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.UIClear, GameInput.BindingSet.Primary, "Delete");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.UIAssign, GameInput.BindingSet.Primary, "F");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.Feedback, GameInput.BindingSet.Primary, "F8");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.UILeft, GameInput.BindingSet.Primary, "LeftArrow");
		SetBindingInternal(GameInput.Device.Keyboard, GameInput.Button.UIRight, GameInput.BindingSet.Primary, "RightArrow");
	}

	private void SetupDefaultControllerBindings()
	{
		ClearBindings(GameInput.Device.Controller);
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.Jump, GameInput.BindingSet.Primary, "ControllerButtonY");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.PDA, GameInput.BindingSet.Primary, "ControllerButtonBack");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.Deconstruct, GameInput.BindingSet.Primary, "ControllerDPadDown");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.Exit, GameInput.BindingSet.Primary, GameInput.SwapAcceptCancel ? "ControllerButtonA" : "ControllerButtonB");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LeftHand, GameInput.BindingSet.Primary, GameInput.SwapAcceptCancel ? "ControllerButtonB" : "ControllerButtonA");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LeftHand, GameInput.BindingSet.Secondary, "ControllerLeftTrigger");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.RightHand, GameInput.BindingSet.Primary, "ControllerRightTrigger");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.AltTool, GameInput.BindingSet.Primary, "ControllerDPadUp");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.TakePicture, GameInput.BindingSet.Primary, "ControllerButtonRightStick");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.Reload, GameInput.BindingSet.Primary, "ControllerButtonX");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveForward, GameInput.BindingSet.Primary, "ControllerLeftStickUp");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveBackward, GameInput.BindingSet.Primary, "ControllerLeftStickDown");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveLeft, GameInput.BindingSet.Primary, "ControllerLeftStickLeft");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveRight, GameInput.BindingSet.Primary, "ControllerLeftStickRight");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveUp, GameInput.BindingSet.Primary, "ControllerButtonLeftBumper");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveDown, GameInput.BindingSet.Primary, "ControllerButtonRightBumper");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.Sprint, GameInput.BindingSet.Primary, "ControllerButtonLeftStick");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LookUp, GameInput.BindingSet.Primary, "ControllerRightStickUp");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LookDown, GameInput.BindingSet.Primary, "ControllerRightStickDown");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LookLeft, GameInput.BindingSet.Primary, "ControllerRightStickLeft");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LookRight, GameInput.BindingSet.Primary, "ControllerRightStickRight");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.CycleNext, GameInput.BindingSet.Primary, "ControllerDPadRight");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.CyclePrev, GameInput.BindingSet.Primary, "ControllerDPadLeft");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UISubmit, GameInput.BindingSet.Primary, GameInput.SwapAcceptCancel ? "ControllerButtonB" : "ControllerButtonA");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UICancel, GameInput.BindingSet.Primary, GameInput.SwapAcceptCancel ? "ControllerButtonA" : "ControllerButtonB");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIClear, GameInput.BindingSet.Primary, "ControllerButtonX");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIAssign, GameInput.BindingSet.Primary, "ControllerButtonY");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIMenu, GameInput.BindingSet.Primary, "ControllerButtonHome");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UILeft, GameInput.BindingSet.Primary, "ControllerDPadLeft");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIRight, GameInput.BindingSet.Primary, "ControllerDPadRight");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIDown, GameInput.BindingSet.Primary, "ControllerDPadDown");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIUp, GameInput.BindingSet.Primary, "ControllerDPadUp");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UILeft, GameInput.BindingSet.Secondary, "ControllerLeftStickLeftMenu");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIRight, GameInput.BindingSet.Secondary, "ControllerLeftStickRightMenu");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIDown, GameInput.BindingSet.Secondary, "ControllerLeftStickDownMenu");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIUp, GameInput.BindingSet.Secondary, "ControllerLeftStickUpMenu");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIAdjustLeft, GameInput.BindingSet.Primary, "ControllerLeftStickLeft");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIAdjustRight, GameInput.BindingSet.Primary, "ControllerLeftStickRight");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIAdjustLeft, GameInput.BindingSet.Secondary, "ControllerDPadLeft");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIAdjustRight, GameInput.BindingSet.Secondary, "ControllerDPadRight");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIRightStickAdjustLeft, GameInput.BindingSet.Primary, "ControllerRightStickLeft");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIRightStickAdjustRight, GameInput.BindingSet.Primary, "ControllerRightStickRight");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIPrevTab, GameInput.BindingSet.Primary, "ControllerButtonLeftBumper");
		SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UINextTab, GameInput.BindingSet.Primary, "ControllerButtonRightBumper");
	}

	private void SetBindingInternal(GameInput.Device device, GameInput.Button button, GameInput.BindingSet bindingSet, int inputIndex)
	{
		buttonBindings[(int)device, (int)button, (int)bindingSet] = inputIndex;
		GameInput.SetBindingsChanged();
	}

	private int GetBindingInternal(GameInput.Device device, GameInput.Button button, GameInput.BindingSet bindingSet)
	{
		return buttonBindings[(int)device, (int)button, (int)bindingSet];
	}

	private void SetBindingInternal(GameInput.Device device, GameInput.Button button, GameInput.BindingSet bindingSet, string input)
	{
		int inputIndex = GetInputIndex(input);
		if (inputIndex == -1 && !string.IsNullOrEmpty(input))
		{
			Debug.LogErrorFormat("GameInput: Input {0} not found", input);
		}
		SetBindingInternal(device, button, bindingSet, inputIndex);
	}

	private float GetAnalogValueForButton(GameInput.Button button)
	{
		float num = 0f;
		if (!GameInput.clearInput && !IsRebinding)
		{
			for (int i = 0; i < numDevices; i++)
			{
				for (int j = 0; j < numBindingSets; j++)
				{
					int bindingInternal = GetBindingInternal((GameInput.Device)i, button, (GameInput.BindingSet)j);
					if (bindingInternal != -1)
					{
						if (inputs[bindingInternal].keyCode == KeyCode.None)
						{
							num = Mathf.Max(num, axisValues[(int)inputs[bindingInternal].axis] * (inputs[bindingInternal].axisPositive ? 1f : (-1f)));
						}
						else if ((inputStates[bindingInternal].flags & GameInput.InputStateFlags.Held) != 0)
						{
							num = 1f;
						}
					}
				}
			}
		}
		return num;
	}

	private InputState GetInputStateForButton(GameInput.Button button)
	{
		InputState result = default(InputState);
		if (!GameInput.clearInput && !IsRebinding)
		{
			for (int i = 0; i < numDevices; i++)
			{
				for (int j = 0; j < numBindingSets; j++)
				{
					int bindingInternal = GetBindingInternal((GameInput.Device)i, button, (GameInput.BindingSet)j);
					if (bindingInternal != -1)
					{
						result.flags |= inputStates[bindingInternal].flags;
						result.timeDown = Mathf.Max(result.timeDown, inputStates[bindingInternal].timeDown);
					}
				}
			}
		}
		return result;
	}

	private IEnumerator UpdateAvailableDevices()
	{
		WaitForSecondsRealtime wait = new WaitForSecondsRealtime(1f);
		while (true)
		{
			UpdateControllerAvailable();
			UpdateKeyboardAvailable();
			if (lastDevice == GameInput.Device.Keyboard && !keyboardAvailable)
			{
				lastDevice = GameInput.Device.Controller;
			}
			if (lastDevice == GameInput.Device.Controller && !controllerAvailable)
			{
				lastDevice = GameInput.Device.Keyboard;
			}
			yield return wait;
		}
	}

	private void ClearLastInputPressed()
	{
		for (int i = 0; i < numDevices; i++)
		{
			lastInputPressed[i] = -1;
		}
	}

	private GameInput.Device GetDeviceForAxis(AnalogAxis axis)
	{
		if (axis == AnalogAxis.MouseX || axis == AnalogAxis.MouseY || axis == AnalogAxis.MouseWheel)
		{
			return GameInput.Device.Keyboard;
		}
		return GameInput.Device.Controller;
	}

	private void UpdateAxisValues(bool useKeyboard, bool useController)
	{
		for (int i = 0; i < axisValues.Length; i++)
		{
			axisValues[i] = 0f;
		}
		if (useController)
		{
			float value3;
			float value4;
			float num;
			float num2;
			Vector2 value = default(Vector2);
			Vector2 value2 = default(Vector2);
			if (GetUseOculusInputManager())
			{
				Vector2 vector = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
				value.x = vector.x;
				value.y = 0f - vector.y;
				Vector2 vector2 = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
				value2.x = vector2.x;
				value2.y = 0f - vector2.y;
				value3 = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger);
				value4 = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
				num = 0f;
				if (OVRInput.Get(OVRInput.RawButton.DpadLeft))
				{
					num -= 1f;
				}
				if (OVRInput.Get(OVRInput.RawButton.DpadRight))
				{
					num += 1f;
				}
				num2 = 0f;
				if (OVRInput.Get(OVRInput.RawButton.DpadDown))
				{
					num2 -= 1f;
				}
				if (OVRInput.Get(OVRInput.RawButton.DpadUp))
				{
					num2 += 1f;
				}
			}
			else
			{
				ControllerLayout controllerLayout = GetControllerLayout();
				switch (controllerLayout)
				{
				default:
					throw new NotImplementedException($"{controllerLayout} ControllerLayout support is not implemented!");
				case ControllerLayout.Xbox360:
					value.x = UnityEngine.Input.GetAxis("ControllerAxis1");
					value.y = UnityEngine.Input.GetAxis("ControllerAxis2");
					value2.x = UnityEngine.Input.GetAxis("ControllerAxis4");
					value2.y = UnityEngine.Input.GetAxis("ControllerAxis5");
					value3 = Mathf.Max(0f - UnityEngine.Input.GetAxis("ControllerAxis3"), 0f);
					value4 = Mathf.Max(UnityEngine.Input.GetAxis("ControllerAxis3"), 0f);
					num = UnityEngine.Input.GetAxis("ControllerAxis6");
					num2 = UnityEngine.Input.GetAxis("ControllerAxis7");
					break;
				case ControllerLayout.XboxOne:
					value.x = UnityEngine.Input.GetAxis("ControllerAxis1");
					value.y = UnityEngine.Input.GetAxis("ControllerAxis2");
					value2.x = UnityEngine.Input.GetAxis("ControllerAxis4");
					value2.y = UnityEngine.Input.GetAxis("ControllerAxis5");
					value3 = Mathf.Max(UnityEngine.Input.GetAxis("ControllerAxis3"), 0f);
					value4 = Mathf.Max(0f - UnityEngine.Input.GetAxis("ControllerAxis3"), 0f);
					num = UnityEngine.Input.GetAxis("ControllerAxis6");
					num2 = UnityEngine.Input.GetAxis("ControllerAxis7");
					break;
				case ControllerLayout.Switch:
					value.x = InputUtils.GetAxis("ControllerAxis1");
					value.y = InputUtils.GetAxis("ControllerAxis2");
					value2.x = InputUtils.GetAxis("ControllerAxis4");
					value2.y = InputUtils.GetAxis("ControllerAxis5");
					value3 = Mathf.Max(InputUtils.GetAxis("ControllerAxis3"), 0f);
					value4 = Mathf.Max(0f - InputUtils.GetAxis("ControllerAxis3"), 0f);
					num = InputUtils.GetAxis("ControllerAxis6");
					num2 = InputUtils.GetAxis("ControllerAxis7");
					break;
				case ControllerLayout.Scarlett:
					value.x = UnityEngine.Input.GetAxis("ControllerAxis1");
					value.y = UnityEngine.Input.GetAxis("ControllerAxis2");
					value2.x = UnityEngine.Input.GetAxis("ControllerAxis4");
					value2.y = UnityEngine.Input.GetAxis("ControllerAxis5");
					value3 = Mathf.Max(UnityEngine.Input.GetAxis("ControllerAxis9"), 0f);
					value4 = Mathf.Max(UnityEngine.Input.GetAxis("ControllerAxis10"), 0f);
					num = UnityEngine.Input.GetAxis("ControllerAxis6");
					num2 = UnityEngine.Input.GetAxis("ControllerAxis7");
					break;
				case ControllerLayout.PS4:
					value.x = UnityEngine.Input.GetAxis("ControllerAxis1");
					value.y = UnityEngine.Input.GetAxis("ControllerAxis2");
					value2.x = UnityEngine.Input.GetAxis("ControllerAxis3");
					value2.y = UnityEngine.Input.GetAxis("ControllerAxis6");
					value3 = UnityEngine.Input.GetAxis("ControllerAxis4") * 0.5f + 0.5f;
					value4 = UnityEngine.Input.GetAxis("ControllerAxis5") * 0.5f + 0.5f;
					num = UnityEngine.Input.GetAxis("ControllerAxis7");
					num2 = UnityEngine.Input.GetAxis("ControllerAxis8");
					break;
				case ControllerLayout.PS5:
					value.x = UnityEngine.Input.GetAxis("ControllerAxis1");
					value.y = UnityEngine.Input.GetAxis("ControllerAxis2");
					value2.x = UnityEngine.Input.GetAxis("ControllerAxis3");
					value2.y = UnityEngine.Input.GetAxis("ControllerAxis6");
					value3 = UnityEngine.Input.GetAxis("ControllerAxis4") * 0.5f + 0.5f;
					value4 = UnityEngine.Input.GetAxis("ControllerAxis5") * 0.5f + 0.5f;
					num = UnityEngine.Input.GetAxis("ControllerAxis7");
					num2 = UnityEngine.Input.GetAxis("ControllerAxis8");
					break;
				}
			}
			value = GameInput.Resample(value, 0.2f);
			axisValues[2] = value.x;
			axisValues[3] = value.y;
			value2 = GameInput.Resample(value2, 0.2f);
			axisValues[0] = value2.x;
			axisValues[1] = value2.y;
			axisValues[4] = GameInput.Resample(value3, 0.001f);
			axisValues[5] = GameInput.Resample(value4, 0.001f);
			axisValues[6] = GameInput.Resample(num, 0.001f);
			axisValues[7] = GameInput.Resample(num2, 0.001f);
		}
		if (useKeyboard)
		{
			axisValues[10] = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
			axisValues[8] = UnityEngine.Input.GetAxisRaw("Mouse X");
			axisValues[9] = UnityEngine.Input.GetAxisRaw("Mouse Y");
		}
		for (int j = 0; j < axisValues.Length; j++)
		{
			AnalogAxis axis = (AnalogAxis)j;
			GameInput.Device deviceForAxis = GetDeviceForAxis(axis);
			float f = lastAxisValues[j] - axisValues[j];
			lastAxisValues[j] = axisValues[j];
			if (deviceForAxis != lastDevice)
			{
				float num3 = 0.1f;
				if (Mathf.Abs(f) > num3)
				{
					lastDevice = deviceForAxis;
				}
				else
				{
					axisValues[j] = 0f;
				}
			}
		}
	}

	private GameInput.InputStateFlags GetInputState(KeyCode keyCode)
	{
		GameInput.InputStateFlags inputStateFlags = GameInput.InputStateFlags.None;
		if (InputUtils.GetKey(keyCode))
		{
			inputStateFlags |= GameInput.InputStateFlags.Held;
		}
		if (InputUtils.GetKeyDown(keyCode))
		{
			inputStateFlags |= GameInput.InputStateFlags.Down;
		}
		if (InputUtils.GetKeyUp(keyCode))
		{
			inputStateFlags |= GameInput.InputStateFlags.Up;
		}
		return inputStateFlags;
	}

	private void UpdateKeyInputs(bool useKeyboard, bool useController)
	{
		ControllerLayout controllerLayout = GetControllerLayout();
		float unscaledTime = Time.unscaledTime;
		int num = -1;
		PlatformServices services = PlatformUtils.main.GetServices();
		if (services != null)
		{
			num = services.GetActiveController();
		}
		for (int i = 0; i < inputs.Count; i++)
		{
			InputState inputState = default(InputState);
			inputState.timeDown = inputStates[i].timeDown;
			GameInput.Device device = inputs[i].device;
			KeyCode keyCodeForControllerLayout = GetKeyCodeForControllerLayout(inputs[i].keyCode, controllerLayout);
			if (keyCodeForControllerLayout != 0)
			{
				KeyCode keyCode = keyCodeForControllerLayout;
				if (num >= 1)
				{
					keyCode = keyCodeForControllerLayout + num * 20;
				}
				inputState.flags |= GetInputState(keyCode);
				if (inputState.flags != 0 && (controllerEnabled || device != GameInput.Device.Controller))
				{
					lastDevice = device;
				}
			}
			else
			{
				bool flag = (inputStates[i].flags & GameInput.InputStateFlags.Held) != 0;
				float num2 = axisValues[(int)inputs[i].axis];
				bool flag2 = ((!inputs[i].axisPositive) ? (num2 < 0f - inputs[i].axisDeadZone) : (num2 > inputs[i].axisDeadZone));
				if (flag2)
				{
					inputState.flags |= GameInput.InputStateFlags.Held;
				}
				if (flag2 && !flag)
				{
					inputState.flags |= GameInput.InputStateFlags.Down;
				}
				if (!flag2 && flag)
				{
					inputState.flags |= GameInput.InputStateFlags.Up;
				}
			}
			if ((inputState.flags & GameInput.InputStateFlags.Down) != 0)
			{
				lastInputPressed[(int)device] = i;
				inputState.timeDown = unscaledTime;
			}
			if ((device == GameInput.Device.Controller && !useController) || (device == GameInput.Device.Keyboard && !useKeyboard))
			{
				inputState.flags = GameInput.InputStateFlags.None;
				if ((inputStates[i].flags & GameInput.InputStateFlags.Held) != 0)
				{
					inputState.flags |= GameInput.InputStateFlags.Up;
				}
			}
			inputStates[i] = inputState;
		}
	}

	private KeyCode GetKeyCodeForControllerLayout(KeyCode keyCode, ControllerLayout controllerLayout)
	{
		if (controllerLayout == ControllerLayout.PS4 || controllerLayout == ControllerLayout.PS5)
		{
			switch (keyCode)
			{
			case KeyCode.JoystickButton0:
				return KeyCode.JoystickButton1;
			case KeyCode.JoystickButton1:
				return KeyCode.JoystickButton2;
			case KeyCode.JoystickButton2:
				return KeyCode.JoystickButton0;
			case KeyCode.JoystickButton3:
				return KeyCode.JoystickButton3;
			case KeyCode.JoystickButton4:
				return KeyCode.JoystickButton4;
			case KeyCode.JoystickButton5:
				return KeyCode.JoystickButton5;
			case KeyCode.JoystickButton6:
				return KeyCode.JoystickButton13;
			case KeyCode.JoystickButton7:
				return KeyCode.JoystickButton9;
			case KeyCode.JoystickButton8:
				return KeyCode.JoystickButton10;
			case KeyCode.JoystickButton9:
				return KeyCode.JoystickButton11;
			case KeyCode.JoystickButton10:
				return KeyCode.JoystickButton8;
			case KeyCode.JoystickButton11:
				return KeyCode.JoystickButton15;
			case KeyCode.JoystickButton12:
				return KeyCode.JoystickButton12;
			case KeyCode.JoystickButton13:
				return KeyCode.JoystickButton6;
			case KeyCode.JoystickButton14:
				return KeyCode.JoystickButton7;
			case KeyCode.JoystickButton15:
				return KeyCode.JoystickButton14;
			default:
				return keyCode;
			}
		}
		return keyCode;
	}

	private bool IsKeyboardAvailable()
	{
		return keyboardAvailable;
	}

	private bool IsControllerAvailable()
	{
		return controllerAvailable;
	}

	private void ScanInputs()
	{
		bool useKeyboard = IsKeyboardAvailable();
		bool useController = IsControllerAvailable() && controllerEnabled;
		ClearLastInputPressed();
		UpdateAxisValues(useKeyboard, useController);
		UpdateKeyInputs(useKeyboard, useController);
	}

	private string GetKeyCodeAsInputName(KeyCode keyCode)
	{
		switch (keyCode)
		{
		case KeyCode.Mouse0:
			return "MouseButtonLeft";
		case KeyCode.Mouse1:
			return "MouseButtonRight";
		case KeyCode.Mouse2:
			return "MouseButtonMiddle";
		case KeyCode.JoystickButton0:
			return "ControllerButtonA";
		case KeyCode.JoystickButton1:
			return "ControllerButtonB";
		case KeyCode.JoystickButton2:
			return "ControllerButtonX";
		case KeyCode.JoystickButton3:
			return "ControllerButtonY";
		case KeyCode.JoystickButton4:
			return "ControllerButtonLeftBumper";
		case KeyCode.JoystickButton5:
			return "ControllerButtonRightBumper";
		case KeyCode.JoystickButton6:
			return "ControllerButtonBack";
		case KeyCode.JoystickButton7:
			return "ControllerButtonHome";
		case KeyCode.JoystickButton8:
			return "ControllerButtonLeftStick";
		case KeyCode.JoystickButton9:
			return "ControllerButtonRightStick";
		case KeyCode.Alpha0:
			return "0";
		case KeyCode.Alpha1:
			return "1";
		case KeyCode.Alpha2:
			return "2";
		case KeyCode.Alpha3:
			return "3";
		case KeyCode.Alpha4:
			return "4";
		case KeyCode.Alpha5:
			return "5";
		case KeyCode.Alpha6:
			return "6";
		case KeyCode.Alpha7:
			return "7";
		case KeyCode.Alpha8:
			return "8";
		case KeyCode.Alpha9:
			return "9";
		default:
			return keyCode.ToString();
		}
	}

	private GameInput.Device GetKeyCodeDevice(KeyCode keyCode)
	{
		if (keyCode >= KeyCode.JoystickButton0 && keyCode <= KeyCode.Joystick8Button19)
		{
			return GameInput.Device.Controller;
		}
		return GameInput.Device.Keyboard;
	}

	private int GetInputIndex(string name)
	{
		for (int i = 0; i < inputs.Count; i++)
		{
			if (inputs[i].name == name)
			{
				return i;
			}
		}
		return -1;
	}

	private void UpdateKeyboardAvailable()
	{
		bool flag = true;
		if (flag != keyboardAvailable)
		{
			keyboardAvailable = flag;
			GameInput.SetBindingsChanged();
		}
	}

	private ControllerLayout GetControllerLayout()
	{
		if (chosenControllerLayout != 0)
		{
			return chosenControllerLayout;
		}
		return automaticControllerLayout;
	}

	private ControllerLayout GetControllerLayoutFromName(string controllerName)
	{
		switch (controllerName)
		{
		default:
			return ControllerLayout.Xbox360;
		case "Controller (Xbox One For Windows)":
			return ControllerLayout.XboxOne;
		case "Wireless Controller":
			return ControllerLayout.PS4;
		case "Union Controller":
			return ControllerLayout.Scarlett;
		}
	}

	private bool GetUseOculusInputManager()
	{
		if (!XRSettings.enabled || OVRManager.instance == null)
		{
			return false;
		}
		return (OVRInput.GetConnectedControllers() & OVRInput.Controller.Gamepad) != 0;
	}

	private void UpdateControllerAvailable()
	{
		bool flag = false;
		if (GetUseOculusInputManager())
		{
			flag = true;
			automaticControllerLayout = ControllerLayout.XboxOne;
		}
		else if (Application.platform == RuntimePlatform.GameCoreScarlett)
		{
			flag = true;
			automaticControllerLayout = ControllerLayout.Scarlett;
		}
		else if (Application.platform == RuntimePlatform.XboxOne)
		{
			flag = true;
			automaticControllerLayout = ControllerLayout.XboxOne;
		}
		else if (Application.platform == RuntimePlatform.Switch)
		{
			flag = true;
			automaticControllerLayout = ControllerLayout.Switch;
		}
		else
		{
			string[] joystickNames = UnityEngine.Input.GetJoystickNames();
			int num = 0;
			if (num < joystickNames.Length)
			{
				string controllerName = joystickNames[num];
				flag = true;
				automaticControllerLayout = GetControllerLayoutFromName(controllerName);
			}
		}
		if (flag != controllerAvailable)
		{
			controllerAvailable = flag;
			GameInput.SetBindingsChanged();
		}
	}

	private static int GetMaximumEnumValue(Type enumType)
	{
		int[] array = (int[])Enum.GetValues(enumType);
		int num = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			int num2 = array[i];
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	private void UpdateRebind()
	{
		if (rebindOperation == null)
		{
			return;
		}
		GameInput.Device device = rebindOperation.device;
		GameInput.Button action = rebindOperation.action;
		GameInput.BindingSet bindingSet = rebindOperation.bindingSet;
		string text = GetPressedInput(device);
		GameInput.Device primaryDevice = GameInput.PrimaryDevice;
		bool flag = false;
		if (text != null)
		{
			if (IsBindable(text))
			{
				flag = true;
				if (text == "Escape")
				{
					text = null;
				}
			}
			else
			{
				text = null;
			}
		}
		else if (primaryDevice != device && GetPressedInput(primaryDevice) != null)
		{
			flag = true;
		}
		if (flag)
		{
			RebindOperation obj = rebindOperation;
			rebindOperation = null;
			obj.Done((text != null) ? 1 : 0);
		}
		if (text != null)
		{
			GameInput.TryBind(device, action, bindingSet, text);
		}
	}

	private static bool IsBindable(string str)
	{
		switch (str)
		{
		case "ControllerLeftStickRightMenu":
		case "ControllerLeftStickLeftMenu":
		case "ControllerLeftStickUpMenu":
		case "ControllerLeftStickDownMenu":
		case "ControllerButtonHome":
			return false;
		default:
			return true;
		}
	}

	private string GetPressedInput(GameInput.Device device)
	{
		int num = lastInputPressed[(int)device];
		if (num != -1)
		{
			return inputs[num].name;
		}
		return null;
	}

	private void SetupDefaultBindings(GameInput.Device device)
	{
		switch (device)
		{
		case GameInput.Device.Keyboard:
			SetupDefaultKeyboardBindings();
			break;
		case GameInput.Device.Controller:
			SetupDefaultControllerBindings();
			break;
		}
	}

	public void Initialize()
	{
		InputUtils.Initialize();
		int num = GetMaximumEnumValue(typeof(AnalogAxis)) + 1;
		axisValues = new float[num];
		lastAxisValues = new float[num];
		numButtons = GetMaximumEnumValue(typeof(GameInput.Button)) + 1;
		numDevices = GetMaximumEnumValue(typeof(GameInput.Device)) + 1;
		numBindingSets = GetMaximumEnumValue(typeof(GameInput.BindingSet)) + 1;
		buttonBindings = new Array3<int>(numDevices, numButtons, numBindingSets);
		lastInputPressed = new int[numDevices];
		ClearLastInputPressed();
		KeyCode[] array = allKeyCodes;
		foreach (KeyCode keyCode in array)
		{
			if (keyCode != 0 && (keyCode < KeyCode.Joystick1Button0 || keyCode > KeyCode.Joystick8Button19))
			{
				AddKeyInput(GetKeyCodeAsInputName(keyCode), keyCode, GetKeyCodeDevice(keyCode));
			}
		}
		AddAxisInput("MouseWheelUp", AnalogAxis.MouseWheel, axisPositive: true, GameInput.Device.Keyboard);
		AddAxisInput("MouseWheelDown", AnalogAxis.MouseWheel, axisPositive: false, GameInput.Device.Keyboard);
		AddAxisInput("ControllerRightStickRight", AnalogAxis.ControllerRightStickX, axisPositive: true, GameInput.Device.Controller);
		AddAxisInput("ControllerRightStickLeft", AnalogAxis.ControllerRightStickX, axisPositive: false, GameInput.Device.Controller);
		AddAxisInput("ControllerRightStickUp", AnalogAxis.ControllerRightStickY, axisPositive: false, GameInput.Device.Controller);
		AddAxisInput("ControllerRightStickDown", AnalogAxis.ControllerRightStickY, axisPositive: true, GameInput.Device.Controller);
		AddAxisInput("ControllerLeftStickRight", AnalogAxis.ControllerLeftStickX, axisPositive: true, GameInput.Device.Controller);
		AddAxisInput("ControllerLeftStickLeft", AnalogAxis.ControllerLeftStickX, axisPositive: false, GameInput.Device.Controller);
		AddAxisInput("ControllerLeftStickUp", AnalogAxis.ControllerLeftStickY, axisPositive: false, GameInput.Device.Controller);
		AddAxisInput("ControllerLeftStickDown", AnalogAxis.ControllerLeftStickY, axisPositive: true, GameInput.Device.Controller);
		AddAxisInput("ControllerLeftStickRightMenu", AnalogAxis.ControllerLeftStickX, axisPositive: true, GameInput.Device.Controller, 0.75f);
		AddAxisInput("ControllerLeftStickLeftMenu", AnalogAxis.ControllerLeftStickX, axisPositive: false, GameInput.Device.Controller, 0.75f);
		AddAxisInput("ControllerLeftStickUpMenu", AnalogAxis.ControllerLeftStickY, axisPositive: false, GameInput.Device.Controller, 0.75f);
		AddAxisInput("ControllerLeftStickDownMenu", AnalogAxis.ControllerLeftStickY, axisPositive: true, GameInput.Device.Controller, 0.75f);
		AddAxisInput("ControllerLeftTrigger", AnalogAxis.ControllerLeftTrigger, axisPositive: true, GameInput.Device.Controller, 0.5f);
		AddAxisInput("ControllerRightTrigger", AnalogAxis.ControllerRightTrigger, axisPositive: true, GameInput.Device.Controller, 0.5f);
		AddAxisInput("ControllerDPadRight", AnalogAxis.ControllerDPadX, axisPositive: true, GameInput.Device.Controller);
		AddAxisInput("ControllerDPadLeft", AnalogAxis.ControllerDPadX, axisPositive: false, GameInput.Device.Controller);
		AddAxisInput("ControllerDPadUp", AnalogAxis.ControllerDPadY, axisPositive: true, GameInput.Device.Controller);
		AddAxisInput("ControllerDPadDown", AnalogAxis.ControllerDPadY, axisPositive: false, GameInput.Device.Controller);
		inputStates = new InputState[inputs.Count];
		for (int j = 0; j < numDevices; j++)
		{
			SetupDefaultBindings((GameInput.Device)j);
		}
		updateAvailableDevicesRoutine = CoroutineHost.StartCoroutine(UpdateAvailableDevices());
		GameInput.OnPrimaryDeviceChanged += OnPrimaryDeviceChanged;
		GameInput.OnBindingsChanged += OnBindingsChanged;
	}

	public void Deinitialize()
	{
		GameInput.OnPrimaryDeviceChanged -= OnPrimaryDeviceChanged;
		GameInput.OnBindingsChanged -= OnBindingsChanged;
		if (updateAvailableDevicesRoutine != null)
		{
			CoroutineHost.StopCoroutine(updateAvailableDevicesRoutine);
		}
	}

	public void OnUpdate()
	{
		InputUtils.Update();
		ScanInputs();
		UpdateRebind();
	}

	public void PopulateSettings(uGUI_OptionsPanel panel)
	{
		bindings.Clear();
		int tabIndex = panel.AddTab("Input");
		panel.AddChoiceOption(tabIndex, "RunMode", Localization.RunModeOptions, (int)GameInput.RunMode, OnRunModeChanged, "RunModeTooltip");
		if (IsKeyboardAvailable())
		{
			panel.AddHeading(tabIndex, "Mouse");
			panel.AddToggleOption(tabIndex, "InvertLook", InvertMouse, OnInvertMouseChanged);
			panel.AddSliderOption(tabIndex, "MouseSensitivity", MouseSensitivity, 0.01f, 1f, 0.15f, 0.01f, OnMouseSensitivityChanged, SliderLabelMode.Percent, "0");
			panel.AddHeading(tabIndex, "Keyboard");
			AddBindings(panel, tabIndex, GameInput.Device.Keyboard);
		}
		if (IsControllerAvailable())
		{
			panel.AddHeading(tabIndex, "Controller");
			if (IsKeyboardAvailable())
			{
				panel.AddToggleOption(tabIndex, "EnableController", ControllerEnabled, OnControllerEnabledChanged);
			}
			if (XRSettings.enabled)
			{
				panel.AddToggleOption(tabIndex, "GazeBasedCursor", VROptions.gazeBasedCursor, OnGazeBasedCursorChanged);
			}
			ControllerLayout currentIndex = ChosenControllerLayout;
			controllerLayoutOption = panel.AddChoiceOption(tabIndex, "ControllerLayout", controllerLayoutOptions, (int)currentIndex, OnControllerLayoutChanged);
			panel.AddToggleOption(tabIndex, "InvertLook", InvertController, OnInvertControllerChanged);
			Vector2 vector = ControllerSensitivity;
			panel.AddSliderOption(tabIndex, "HorizontalSensitivity", vector.x, 0.05f, 1f, 0.405f, 0.05f, OnControllerHorizontalSensitivityChanged, SliderLabelMode.Percent, "0");
			panel.AddSliderOption(tabIndex, "VerticalSensitivity", vector.y, 0.05f, 1f, 0.405f, 0.05f, OnControllerVerticalSensitivityChanged, SliderLabelMode.Percent, "0");
			AddBindings(panel, tabIndex, GameInput.Device.Controller);
		}
	}

	public GameInput.InputStateFlags GetButtonState(GameInput.Button button)
	{
		return GetInputStateForButton(button).flags;
	}

	public float GetButtonHeldTime(GameInput.Button action)
	{
		InputState inputStateForButton = GetInputStateForButton(action);
		if ((inputStateForButton.flags & GameInput.InputStateFlags.Held) == 0)
		{
			return 0f;
		}
		return Time.unscaledTime - inputStateForButton.timeDown;
	}

	public float GetFloat(GameInput.Button action)
	{
		return 0f;
	}

	public Vector2 GetVector2(GameInput.Button action)
	{
		switch (action)
		{
		case GameInput.Button.Move:
		{
			float num = 0f;
			num += GetAnalogValueForButton(GameInput.Button.MoveForward);
			num -= GetAnalogValueForButton(GameInput.Button.MoveBackward);
			return new Vector2(0f - GetAnalogValueForButton(GameInput.Button.MoveLeft) + GetAnalogValueForButton(GameInput.Button.MoveRight), num);
		}
		case GameInput.Button.Look:
		{
			Vector2 zero2 = Vector2.zero;
			if (!IsRebinding && !GameInput.clearInput)
			{
				if (controllerEnabled)
				{
					Vector2 zero3 = Vector2.zero;
					float f = GetAnalogValueForButton(GameInput.Button.LookRight) - GetAnalogValueForButton(GameInput.Button.LookLeft);
					float f2 = GetAnalogValueForButton(GameInput.Button.LookUp) - GetAnalogValueForButton(GameInput.Button.LookDown);
					zero3.x = Mathf.Sign(f) * Mathf.Pow(Mathf.Abs(f), 2f) * 500f * controllerSensitivity.x * Time.deltaTime;
					zero3.y = Mathf.Sign(f2) * Mathf.Pow(Mathf.Abs(f2), 2f) * 500f * controllerSensitivity.y * Time.deltaTime;
					if (invertController)
					{
						zero3.y = 0f - zero3.y;
					}
					zero2 += zero3;
				}
				if (IsKeyboardAvailable())
				{
					float num2 = mouseSensitivity;
					float num3 = mouseSensitivity;
					Vector2 zero4 = Vector2.zero;
					zero4.x += axisValues[8] * num3;
					zero4.y += axisValues[9] * num2;
					if (invertMouse)
					{
						zero4.y = 0f - zero4.y;
					}
					zero2 += zero4;
				}
			}
			return zero2;
		}
		case GameInput.Button.UIAdjust:
		{
			Vector2 zero = Vector2.zero;
			if (controllerEnabled)
			{
				zero.x += axisValues[0];
				zero.y -= axisValues[1];
			}
			return zero;
		}
		default:
			return Vector2.zero;
		}
	}

	public bool StartRebind(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet, Action<int> callback)
	{
		rebindOperation = new RebindOperation
		{
			device = device,
			action = action,
			bindingSet = bindingSet,
			callback = callback
		};
		return true;
	}

	public void CancelRebind()
	{
		if (rebindOperation != null)
		{
			RebindOperation obj = rebindOperation;
			rebindOperation = null;
			obj.Done(0);
		}
	}

	public void SetBinding(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet, string binding)
	{
		SetBindingInternal(device, action, bindingSet, binding);
	}

	public string GetBinding(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet)
	{
		int bindingInternal = GetBindingInternal(device, action, bindingSet);
		if (bindingInternal == -1)
		{
			return null;
		}
		return inputs[bindingInternal].name;
	}

	public void AppendDisplayText(string binding, StringBuilder sb, string color)
	{
		GameInput.AppendTranslationOrSprite(GetDisplayText(GetControllerLayout(), binding), sb, color);
	}

	public void GetAllActions(GameInput.Device device, string binding, List<BindConflict> result)
	{
		int inputIndex = GetInputIndex(binding);
		if (inputIndex == -1 && !string.IsNullOrEmpty(binding))
		{
			Debug.LogErrorFormat("GameInput: Input {0} not found", binding);
			return;
		}
		for (int i = 0; i < numButtons; i++)
		{
			GameInput.Button button = (GameInput.Button)i;
			for (int j = 0; j < numBindingSets; j++)
			{
				GameInput.BindingSet bindingSet = (GameInput.BindingSet)j;
				if (GetBindingInternal(device, button, bindingSet) == inputIndex)
				{
					result.Add(new BindConflict(button, bindingSet));
				}
			}
		}
	}

	public void SerializeSettings(GameSettings.ISerializer serializer)
	{
		GameInput.RunMode = (GameInput.RunModeOption)serializer.Serialize("Input/RunMode", (int)GameInput.RunMode);
		InvertMouse = serializer.Serialize("Input/InvertMouse", InvertMouse);
		MouseSensitivity = serializer.Serialize("Input/MouseSensitivity", MouseSensitivity);
		ControllerEnabled = serializer.Serialize("Input/ControllerEnabled", ControllerEnabled);
		InvertController = serializer.Serialize("Input/InvertController", InvertController);
		Vector2 vector = ControllerSensitivity;
		vector.x = serializer.Serialize("Input/ControllerSensitivityX", vector.x);
		vector.y = serializer.Serialize("Input/ControllerSensitivityY", vector.y);
		ControllerSensitivity = vector;
		ChosenControllerLayout = (ControllerLayout)serializer.Serialize("Input/ChosenControllerLayout", (int)ChosenControllerLayout);
		foreach (GameInput.Device value in Enum.GetValues(typeof(GameInput.Device)))
		{
			foreach (GameInput.Button value2 in Enum.GetValues(typeof(GameInput.Button)))
			{
				if (!GameInput.IsBindable(value, value2))
				{
					continue;
				}
				foreach (GameInput.BindingSet value3 in Enum.GetValues(typeof(GameInput.BindingSet)))
				{
					string binding = GameInput.GetBinding(value, value2, value3);
					string name = string.Format("{0}Binding/{1}/{2}/{3}", "Input/", value, value2, value3);
					GameInput.SetBinding(value, value2, value3, serializer.Serialize(name, binding));
				}
			}
		}
	}

	public void UpgradeSettings(GameSettings.ISerializer serializer)
	{
		int num = serializer.Serialize("Version", 0);
		if (num < 1)
		{
			GameInput.SetBinding(GameInput.Device.Controller, GameInput.Button.Reload, GameInput.BindingSet.Primary, "ControllerButtonX");
			GameInput.SetBinding(GameInput.Device.Controller, GameInput.Button.Exit, GameInput.BindingSet.Primary, "ControllerButtonB");
		}
		if (num < 2)
		{
			GameInput.SetBinding(GameInput.Device.Controller, GameInput.Button.Deconstruct, GameInput.BindingSet.Primary, "ControllerDPadDown");
		}
		if (num < 9 && GameInput.SwapAcceptCancel)
		{
			GameInput.SafeSetBinding(GameInput.Device.Controller, GameInput.Button.Exit, GameInput.BindingSet.Primary, "ControllerButtonA");
			GameInput.SafeSetBinding(GameInput.Device.Controller, GameInput.Button.Exit, GameInput.BindingSet.Secondary, string.Empty);
			GameInput.SafeSetBinding(GameInput.Device.Controller, GameInput.Button.LeftHand, GameInput.BindingSet.Primary, "ControllerButtonB");
			GameInput.SafeSetBinding(GameInput.Device.Controller, GameInput.Button.LeftHand, GameInput.BindingSet.Secondary, "ControllerLeftTrigger");
		}
		if (num < 10 && Enum.TryParse<ControllerLayout>(serializer.Serialize("Input/ControllerLayout", ChosenControllerLayout.ToString()), out var result))
		{
			ChosenControllerLayout = result;
		}
	}

	public void SetupDefaultSettings()
	{
		SetupDefaultControllerBindings();
		invertMouse = false;
		mouseSensitivity = 0.15f;
		controllerAvailable = false;
		invertController = false;
		controllerSensitivity = new Vector2(0.405f, 0.405f);
	}

	public void DoDebug()
	{
		Dbg.Write(GetDebug());
	}

	private static string[] GetControllerLayoutOptions()
	{
		List<string> list = new List<string>();
		foreach (object value in Enum.GetValues(typeof(ControllerLayout)))
		{
			list.Add("ControllerLayout" + value.ToString());
		}
		return list.ToArray();
	}

	private void OnInvertMouseChanged(bool value)
	{
		InvertMouse = value;
	}

	private void OnMouseSensitivityChanged(float value)
	{
		MouseSensitivity = value;
	}

	private void OnRunModeChanged(int value)
	{
		GameInput.RunMode = (GameInput.RunModeOption)value;
	}

	private void OnControllerEnabledChanged(bool value)
	{
		ControllerEnabled = value;
	}

	private void OnGazeBasedCursorChanged(bool gazeBasedCursor)
	{
		VROptions.gazeBasedCursor = gazeBasedCursor;
		GameInput.ClearInput();
	}

	private void OnInvertControllerChanged(bool value)
	{
		InvertController = value;
	}

	private void OnControllerHorizontalSensitivityChanged(float value)
	{
		Vector2 vector = ControllerSensitivity;
		vector.x = value;
		ControllerSensitivity = vector;
	}

	private void OnControllerVerticalSensitivityChanged(float value)
	{
		Vector2 vector = ControllerSensitivity;
		vector.y = value;
		ControllerSensitivity = vector;
	}

	private void AddBindings(uGUI_OptionsPanel panel, int tabIndex, GameInput.Device device)
	{
		panel.AddBindingsHeader(tabIndex);
		UnityAction callback = delegate
		{
			SetupDefaultBindings(device);
			if (controllerMoveStickOption != null)
			{
				controllerMoveStickOption.value = 0;
			}
			if (controllerLookStickOption != null)
			{
				controllerLookStickOption.value = 1;
			}
		};
		panel.AddButton(tabIndex, "ResetToDefault", callback);
		if (device == GameInput.Device.Controller)
		{
			controllerMoveStickOption = AddStickOptions(panel, tabIndex, device, "OptionMove", GameInput.Button.MoveForward, GameInput.Button.MoveBackward, GameInput.Button.MoveLeft, GameInput.Button.MoveRight);
			controllerLookStickOption = AddStickOptions(panel, tabIndex, device, "OptionLook", GameInput.Button.LookUp, GameInput.Button.LookDown, GameInput.Button.LookLeft, GameInput.Button.LookRight);
		}
		foreach (GameInput.Button value in Enum.GetValues(typeof(GameInput.Button)))
		{
			if (device == GameInput.Device.Controller)
			{
				if ((uint)(value - 19) <= 3u || (uint)(value - 24) <= 3u)
				{
					continue;
				}
			}
			else if (device == GameInput.Device.Keyboard && (uint)(value - 45) <= 1u)
			{
				continue;
			}
			if (GameInput.IsBindable(device, value))
			{
				string label = "Option" + value;
				uGUI_Bindings item = panel.AddBindingOption(tabIndex, label, device, value);
				bindings.Add(item);
			}
		}
	}

	private void OnControllerLayoutChanged(int layoutIndex)
	{
		if (layoutIndex == (int)ChosenControllerLayout)
		{
			UnappliedSettings.Remove(UnappliedSettings.Key.ControllerLayout);
			return;
		}
		UnappliedSettings.Add(UnappliedSettings.Key.ControllerLayout, delegate
		{
			int revertIndex = (int)ChosenControllerLayout;
			UnappliedSettings.Revert(UnappliedSettings.Key.ControllerLayout, delegate(uGUI_Dialog dialog)
			{
				dialog.Show(Language.main.GetFormat("KeepControllerLayout", 10), delegate(int option)
				{
					if (option <= 0)
					{
						ChosenControllerLayout = (ControllerLayout)revertIndex;
						controllerLayoutOption.value = (int)ChosenControllerLayout;
					}
				}, dialog.DialogTimeout("KeepControllerLayout", 10), 0, Language.main.Get("RevertChanges"), Language.main.Get("KeepChanges"));
			});
			ChosenControllerLayout = (ControllerLayout)layoutIndex;
			controllerLayoutOption.value = (int)ChosenControllerLayout;
		});
	}

	private static int GetStickOption(GameInput.Device device, GameInput.Button buttonUp, GameInput.Button buttonDown, GameInput.Button buttonLeft, GameInput.Button buttonRight)
	{
		int result = 2;
		if (GameInput.GetBinding(device, buttonUp, GameInput.BindingSet.Secondary) == null && GameInput.GetBinding(device, buttonDown, GameInput.BindingSet.Secondary) == null && GameInput.GetBinding(device, buttonLeft, GameInput.BindingSet.Secondary) == null && GameInput.GetBinding(device, buttonRight, GameInput.BindingSet.Secondary) == null)
		{
			if (GameInput.GetBinding(device, buttonUp, GameInput.BindingSet.Primary) == "ControllerLeftStickUp" && GameInput.GetBinding(device, buttonDown, GameInput.BindingSet.Primary) == "ControllerLeftStickDown" && GameInput.GetBinding(device, buttonLeft, GameInput.BindingSet.Primary) == "ControllerLeftStickLeft" && GameInput.GetBinding(device, buttonRight, GameInput.BindingSet.Primary) == "ControllerLeftStickRight")
			{
				result = 0;
			}
			if (GameInput.GetBinding(device, buttonUp, GameInput.BindingSet.Primary) == "ControllerRightStickUp" && GameInput.GetBinding(device, buttonDown, GameInput.BindingSet.Primary) == "ControllerRightStickDown" && GameInput.GetBinding(device, buttonLeft, GameInput.BindingSet.Primary) == "ControllerRightStickLeft" && GameInput.GetBinding(device, buttonRight, GameInput.BindingSet.Primary) == "ControllerRightStickRight")
			{
				result = 1;
			}
		}
		return result;
	}

	private uGUI_Choice AddStickOptions(uGUI_OptionsPanel panel, int tabIndex, GameInput.Device device, string label, GameInput.Button buttonUp, GameInput.Button buttonDown, GameInput.Button buttonLeft, GameInput.Button buttonRight)
	{
		GameObject[] customBindingObjects = new GameObject[4];
		int stickOption = GetStickOption(device, buttonUp, buttonDown, buttonLeft, buttonRight);
		UnityAction<int> callback = delegate(int option)
		{
			bool active = option == 2;
			customBindingObjects[0].SetActive(active);
			customBindingObjects[1].SetActive(active);
			customBindingObjects[2].SetActive(active);
			customBindingObjects[3].SetActive(active);
			using (ListPool<KeyValuePair<GameInput.Button, string>> listPool = Pool<ListPool<KeyValuePair<GameInput.Button, string>>>.Get())
			{
				using (ListPool<BindConflict> listPool2 = Pool<ListPool<BindConflict>>.Get())
				{
					List<KeyValuePair<GameInput.Button, string>> list = listPool.list;
					List<BindConflict> list2 = listPool2.list;
					switch (option)
					{
					case 0:
						list.Add(new KeyValuePair<GameInput.Button, string>(buttonUp, "ControllerLeftStickUp"));
						list.Add(new KeyValuePair<GameInput.Button, string>(buttonDown, "ControllerLeftStickDown"));
						list.Add(new KeyValuePair<GameInput.Button, string>(buttonLeft, "ControllerLeftStickLeft"));
						list.Add(new KeyValuePair<GameInput.Button, string>(buttonRight, "ControllerLeftStickRight"));
						break;
					case 1:
						list.Add(new KeyValuePair<GameInput.Button, string>(buttonUp, "ControllerRightStickUp"));
						list.Add(new KeyValuePair<GameInput.Button, string>(buttonDown, "ControllerRightStickDown"));
						list.Add(new KeyValuePair<GameInput.Button, string>(buttonLeft, "ControllerRightStickLeft"));
						list.Add(new KeyValuePair<GameInput.Button, string>(buttonRight, "ControllerRightStickRight"));
						break;
					}
					for (int i = 0; i < list.Count; i++)
					{
						KeyValuePair<GameInput.Button, string> keyValuePair = list[i];
						GameInput.Button key = keyValuePair.Key;
						string value = keyValuePair.Value;
						BindConflicts.GetConflicts(device, value, key, list2);
						for (int j = 0; j < list2.Count; j++)
						{
							BindConflict bindConflict = list2[j];
							if (!allowedStickConflicts.Contains(bindConflict.action))
							{
								GameInput.SetBinding(device, bindConflict.action, bindConflict.bindingSet, string.Empty);
							}
						}
						GameInput.SetBinding(device, key, GameInput.BindingSet.Primary, value);
					}
				}
			}
			if (option != 2)
			{
				GameInput.SetBinding(device, buttonUp, GameInput.BindingSet.Secondary, string.Empty);
				GameInput.SetBinding(device, buttonDown, GameInput.BindingSet.Secondary, string.Empty);
				GameInput.SetBinding(device, buttonLeft, GameInput.BindingSet.Secondary, string.Empty);
				GameInput.SetBinding(device, buttonRight, GameInput.BindingSet.Secondary, string.Empty);
			}
		};
		string[] items = new string[3]
		{
			GameInput.GetDisplayText("ControllerLeftStick"),
			GameInput.GetDisplayText("ControllerRightStick"),
			"Custom"
		};
		uGUI_Choice result = panel.AddChoiceOption(tabIndex, label, items, stickOption, callback);
		panel.AddBindingOption(tabIndex, $"Option{buttonUp.ToString()}", device, buttonUp, out customBindingObjects[0]);
		panel.AddBindingOption(tabIndex, $"Option{buttonDown.ToString()}", device, buttonDown, out customBindingObjects[1]);
		panel.AddBindingOption(tabIndex, $"Option{buttonLeft.ToString()}", device, buttonLeft, out customBindingObjects[2]);
		panel.AddBindingOption(tabIndex, $"Option{buttonRight.ToString()}", device, buttonRight, out customBindingObjects[3]);
		customBindingObjects[0].SetActive(stickOption == 2);
		customBindingObjects[1].SetActive(stickOption == 2);
		customBindingObjects[2].SetActive(stickOption == 2);
		customBindingObjects[3].SetActive(stickOption == 2);
		return result;
	}

	private void OnPrimaryDeviceChanged()
	{
		for (int num = bindings.Count - 1; num >= 0; num--)
		{
			uGUI_Bindings uGUI_Bindings2 = bindings[num];
			if (uGUI_Bindings2 == null)
			{
				bindings.RemoveAt(num);
			}
			else
			{
				uGUI_Bindings2.OnPrimaryDeviceChanged();
			}
		}
	}

	private void OnBindingsChanged()
	{
		for (int num = bindings.Count - 1; num >= 0; num--)
		{
			uGUI_Bindings uGUI_Bindings2 = bindings[num];
			if (uGUI_Bindings2 == null)
			{
				bindings.RemoveAt(num);
			}
			else
			{
				uGUI_Bindings2.OnBindingsChanged();
			}
		}
		int num2 = 2;
		if (controllerMoveStickOption != null && controllerMoveStickOption.value != num2 && GetStickOption(GameInput.Device.Controller, GameInput.Button.MoveForward, GameInput.Button.MoveBackward, GameInput.Button.MoveLeft, GameInput.Button.MoveRight) == num2)
		{
			controllerMoveStickOption.value = num2;
		}
		if (controllerLookStickOption != null && controllerLookStickOption.value != num2 && GetStickOption(GameInput.Device.Controller, GameInput.Button.LookUp, GameInput.Button.LookDown, GameInput.Button.LookLeft, GameInput.Button.LookRight) == num2)
		{
			controllerLookStickOption.value = num2;
		}
	}

	private string GetDebug()
	{
		PlatformUtils.main.GetServices();
		using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
		{
			StringBuilder sb = stringBuilderPool.sb;
			sb.AppendFormat("timeScale: {0}", Time.timeScale);
			string[] joystickNames = UnityEngine.Input.GetJoystickNames();
			sb.AppendFormat("\n<color=red>Controllers ({0} total):</color>", joystickNames.Length);
			for (int i = 0; i < joystickNames.Length; i++)
			{
				string text = joystickNames[i];
				sb.AppendFormat("\n    {0} name: \"{1}\" autoLayout: {2}", i, text, GetControllerLayoutFromName(text));
			}
			sb.Append("\n\n<color=red>Active Controller:</color>");
			sb.AppendFormat("\n    controllerLayout: {0}", GetControllerLayout());
			sb.Append("\n\n<color=red>Translated:</color>");
			AnalogAxis[] array = allAnalogAxes;
			foreach (AnalogAxis analogAxis in array)
			{
				float num = axisValues[(int)analogAxis];
				sb.AppendFormat("\n    {0} {1}", analogAxis, num);
			}
			for (int k = 0; k < numDevices; k++)
			{
				for (int l = 0; l < numButtons; l++)
				{
					for (int m = 0; m < numBindingSets; m++)
					{
						int bindingInternal = GetBindingInternal((GameInput.Device)k, (GameInput.Button)l, (GameInput.BindingSet)m);
						if (bindingInternal != -1)
						{
							InputState inputState = inputStates[bindingInternal];
							GameInput.InputStateFlags flags = inputState.flags;
							if (flags != 0)
							{
								Input input = inputs[bindingInternal];
								sb.AppendFormat("\n    device: {0} button: {1} bindingSet: {2} state: {3} timeDown: {4} input: ({5}) {6}", (GameInput.Device)k, (GameInput.Button)l, (GameInput.BindingSet)m, flags, inputState.timeDown, bindingInternal, input.name);
							}
						}
					}
				}
			}
			sb.Append("\n\n<color=red>Raw Buttons:</color>");
			KeyCode[] array2 = allKeyCodes;
			foreach (KeyCode keyCode in array2)
			{
				if (UnityEngine.Input.GetKey(keyCode))
				{
					sb.Append("\n    ").Append(keyCode);
				}
			}
			sb.Append("\n\n<color=red>Raw Axes:</color>");
			string[] array3 = allAxisNames;
			foreach (string text2 in array3)
			{
				float axisRaw = UnityEngine.Input.GetAxisRaw(text2);
				sb.AppendFormat("\n    {0} {1}", text2, axisRaw);
			}
			return sb.ToString();
		}
	}
}
