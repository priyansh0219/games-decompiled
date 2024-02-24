using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR;

public class GameInputSystem : IGameInput
{
	private class DeviceDefinition
	{
		public GameInput.Device device;

		public string layout;

		public BindingDefinitions bindings;

		public DisplayDefinitions display;

		public DeviceDefinition(GameInput.Device device, string layout, BindingDefinitions bindings, DisplayDefinitions display)
		{
			this.device = device;
			this.layout = layout;
			this.bindings = bindings;
			this.display = display;
		}
	}

	private class BindingDefinitions : IEnumerable<KeyValuePair<(GameInput.Button, GameInput.BindingSet), string>>, IEnumerable
	{
		private readonly Dictionary<(GameInput.Button, GameInput.BindingSet), string> dictionary = new Dictionary<(GameInput.Button, GameInput.BindingSet), string>();

		public string this[(GameInput.Button, GameInput.BindingSet) key]
		{
			get
			{
				return dictionary[key];
			}
			set
			{
				dictionary[key] = value;
			}
		}

		public void Add(GameInput.Button action, GameInput.BindingSet bindingSet, string path)
		{
			(GameInput.Button, GameInput.BindingSet) key = (action, bindingSet);
			this[key] = path;
		}

		public void Add(GameInput.Button action, string path)
		{
			Add(action, GameInput.BindingSet.Primary, path);
		}

		public void Add(GameInput.Button action, string path1, string path2)
		{
			Add(action, GameInput.BindingSet.Primary, path1);
			Add(action, GameInput.BindingSet.Secondary, path2);
		}

		public bool TryGetValue((GameInput.Button, GameInput.BindingSet) key, out string path)
		{
			return dictionary.TryGetValue(key, out path);
		}

		public IEnumerator<KeyValuePair<(GameInput.Button, GameInput.BindingSet), string>> GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<(GameInput.Button, GameInput.BindingSet), string>>)this).GetEnumerator();
		}
	}

	private class DisplayDefinitions : IEnumerable<KeyValuePair<string, string>>, IEnumerable
	{
		private readonly Dictionary<string, string> dictionary = new Dictionary<string, string>();

		public string this[string key]
		{
			get
			{
				return dictionary[key];
			}
			set
			{
				dictionary[key] = value;
			}
		}

		public void Add(string binding, string path)
		{
			this[binding] = path;
		}

		public bool TryGetValue(string binding, out string path)
		{
			return dictionary.TryGetValue(binding, out path);
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, string>>)this).GetEnumerator();
		}

		public void Clear()
		{
			dictionary.Clear();
		}
	}

	private struct DeviceInfo
	{
		public InputDevice inputDevice;

		public DeviceDefinition definition;
	}

	private struct Substring : IEquatable<Substring>
	{
		private readonly string fullString;

		private int start;

		private int length;

		private readonly int hash;

		public Substring(string value, int start = 0, int length = -1)
		{
			fullString = value;
			this.start = start;
			this.length = length;
			hash = 193;
			for (int i = 0; i < this.length; i++)
			{
				char c = fullString[start + i];
				c = char.ToLowerInvariant(c);
				hash = 31 * hash + c;
			}
		}

		public override int GetHashCode()
		{
			return hash;
		}

		public override bool Equals(object obj)
		{
			if (obj is Substring other)
			{
				return Equals(other);
			}
			return false;
		}

		public bool Equals(Substring other)
		{
			if (hash != other.hash || length != other.length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				char num = char.ToLowerInvariant(fullString[start + i]);
				char c = char.ToLowerInvariant(other.fullString[other.start + i]);
				if (num != c)
				{
					return false;
				}
			}
			return true;
		}

		public override string ToString()
		{
			return fullString.Substring(start, length);
		}
	}

	private static readonly BindingDefinitions bindingsKeyboard;

	private static readonly BindingDefinitions bindingsController;

	private static readonly BindingDefinitions bindingsDualShock;

	private static readonly DisplayDefinitions displayKeyboard;

	private static readonly DisplayDefinitions displayController;

	private static readonly DisplayDefinitions displayMacKeyboard;

	private static readonly DisplayDefinitions displayPS4;

	private static readonly DisplayDefinitions displayPS5;

	private static readonly DisplayDefinitions displaySwitch;

	private static readonly GameInput.Button[] bindingOptionsOrder;

	private const string layoutGamepad = "Gamepad";

	private const string layoutKeyboard = "Keyboard";

	private const string layoutMouse = "Mouse";

	private const float defaultMouseSensitivity = 0.15f;

	private const bool defaultControllerEnabled = true;

	private const float defaultControllerSensitivity = 0.405f;

	private const string commandInput2Json = "input2json";

	private const string commandInputOverrides2Json = "inputoverrides2json";

	private const char bindingGroupSeparator = ';';

	private static readonly Dictionary<(GameInput.Device, GameInput.BindingSet), string> compositeBindingGroups;

	private static readonly Dictionary<string, GameInput.Button> nameToAction;

	private static Dictionary<GameInput.Device, DeviceDefinition> defaultDeviceDefinitions;

	private static DeviceDefinition[] deviceDefinitions;

	private const float deadzoneMinRangeMin = 0f;

	private const float deadzoneMinDefault = 0.2f;

	private const float deadzoneMinRangeMax = 0.3f;

	private const float deadzoneMaxRangeMin = 0.8f;

	private const float deadzoneMaxDefault = 0.92f;

	private const float deadzoneMaxRangeMax = 1f;

	private bool invertMouse;

	private bool invertController;

	private float mouseSensitivity = 0.15f;

	private Vector2 controllerSensitivity = new Vector2(0.405f, 0.405f);

	private bool controllerEnabled = true;

	private InputActionAsset inputActionAsset;

	private InputActionMap actionMapGameplay;

	private readonly Dictionary<GameInput.Button, InputAction> actions = new Dictionary<GameInput.Button, InputAction>(GameInput.sActionComparer);

	private string lastLayout;

	private GameInput.Device lastDevice;

	private Dictionary<GameInput.Device, DeviceInfo> lastDevices;

	private static readonly Dictionary<string, DeviceDefinition> layoutToDeviceDefinitionCache;

	private readonly DisplayDefinitions displayNameCache = new DisplayDefinitions();

	private static readonly Dictionary<Substring, string> layoutNameCache;

	private bool anyKeyDown;

	private int anyKeyDownFrame;

	private InputActionRebindingExtensions.RebindingOperation rebindOperation;

	private readonly Dictionary<Guid, float> startTimes = new Dictionary<Guid, float>();

	private uGUI_Dialog dialog;

	private List<uGUI_Bindings> bindingOptions = new List<uGUI_Bindings>();

	private static MethodInfo methodDeferBindingResolution;

	public bool InvertMouse
	{
		get
		{
			return invertMouse;
		}
		private set
		{
			invertMouse = value;
		}
	}

	public bool InvertController
	{
		get
		{
			return invertController;
		}
		private set
		{
			invertController = value;
		}
	}

	public float MouseSensitivity
	{
		get
		{
			return mouseSensitivity;
		}
		private set
		{
			mouseSensitivity = value;
		}
	}

	public bool IsKeyboardOrMouseAvailable
	{
		get
		{
			if (!IsDeviceAvailable("Keyboard"))
			{
				return IsDeviceAvailable("Mouse");
			}
			return true;
		}
	}

	public Vector2 ControllerSensitivity
	{
		get
		{
			return controllerSensitivity;
		}
		private set
		{
			controllerSensitivity = value;
		}
	}

	public bool ControllerEnabled
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
			if (controllerEnabled != value)
			{
				controllerEnabled = value;
				UpdateDevicesState("Gamepad", value);
				GameInput.SetBindingsChanged();
			}
		}
	}

	public string Id => "GameInputSystem";

	public bool IsRebinding => rebindOperation != null;

	public bool AnyKeyDown
	{
		get
		{
			int frameCount = Time.frameCount;
			if (anyKeyDownFrame != frameCount)
			{
				anyKeyDownFrame = frameCount;
				anyKeyDown = false;
				foreach (InputDevice device in InputSystem.devices)
				{
					foreach (InputControl allControl in device.allControls)
					{
						if (allControl is ButtonControl buttonControl && buttonControl.wasPressedThisFrame)
						{
							anyKeyDown = true;
							break;
						}
					}
					if (anyKeyDown)
					{
						break;
					}
				}
			}
			return anyKeyDown;
		}
	}

	public GameInput.Device PrimaryDevice => lastDevice;

	static GameInputSystem()
	{
		bindingsKeyboard = new BindingDefinitions
		{
			{
				GameInput.Button.Jump,
				"<Keyboard>/space"
			},
			{
				GameInput.Button.PDA,
				"<Keyboard>/tab"
			},
			{
				GameInput.Button.Deconstruct,
				"<Keyboard>/q"
			},
			{
				GameInput.Button.Exit,
				"<Keyboard>/e"
			},
			{
				GameInput.Button.LeftHand,
				"<Mouse>/leftButton"
			},
			{
				GameInput.Button.RightHand,
				"<Mouse>/rightButton"
			},
			{
				GameInput.Button.CycleNext,
				"<Mouse>/scroll/up",
				"<Keyboard>/leftBracket"
			},
			{
				GameInput.Button.CyclePrev,
				"<Mouse>/scroll/down",
				"<Keyboard>/rightBracket"
			},
			{
				GameInput.Button.Slot1,
				"<Keyboard>/1"
			},
			{
				GameInput.Button.Slot2,
				"<Keyboard>/2"
			},
			{
				GameInput.Button.Slot3,
				"<Keyboard>/3"
			},
			{
				GameInput.Button.Slot4,
				"<Keyboard>/4"
			},
			{
				GameInput.Button.Slot5,
				"<Keyboard>/5"
			},
			{
				GameInput.Button.AltTool,
				"<Keyboard>/f"
			},
			{
				GameInput.Button.TakePicture,
				"<Keyboard>/f11"
			},
			{
				GameInput.Button.Reload,
				"<Keyboard>/r"
			},
			{
				GameInput.Button.Sprint,
				"<Keyboard>/leftShift"
			},
			{
				GameInput.Button.MoveUp,
				"<Keyboard>/space"
			},
			{
				GameInput.Button.MoveDown,
				"<Keyboard>/c"
			},
			{
				GameInput.Button.AutoMove,
				"<Keyboard>/x"
			},
			{
				GameInput.Button.Feedback,
				"<Keyboard>/f8"
			},
			{
				GameInput.Button.UISubmit,
				"<Mouse>/leftButton"
			},
			{
				GameInput.Button.UICancel,
				"<Mouse>/rightButton",
				"<Keyboard>/escape"
			},
			{
				GameInput.Button.UIClear,
				"<Keyboard>/delete"
			},
			{
				GameInput.Button.UIAssign,
				"<Keyboard>/f"
			},
			{
				GameInput.Button.UIMenu,
				"<Keyboard>/escape"
			},
			{
				GameInput.Button.UILeft,
				"<Keyboard>/leftArrow"
			},
			{
				GameInput.Button.UIRight,
				"<Keyboard>/rightArrow"
			},
			{
				GameInput.Button.UIUp,
				"<Keyboard>/upArrow"
			},
			{
				GameInput.Button.UIDown,
				"<Keyboard>/downArrow"
			},
			{
				GameInput.Button.UINextTab,
				""
			},
			{
				GameInput.Button.UIPrevTab,
				""
			},
			{
				GameInput.Button.UIAdjustLeft,
				""
			},
			{
				GameInput.Button.UIAdjustRight,
				""
			},
			{
				GameInput.Button.MoveForward,
				"<Keyboard>/w"
			},
			{
				GameInput.Button.MoveBackward,
				"<Keyboard>/s"
			},
			{
				GameInput.Button.MoveLeft,
				"<Keyboard>/a"
			},
			{
				GameInput.Button.MoveRight,
				"<Keyboard>/d"
			},
			{
				GameInput.Button.Look,
				"<Mouse>/delta"
			}
		};
		bindingsController = new BindingDefinitions
		{
			{
				GameInput.Button.Jump,
				"<Gamepad>/buttonNorth"
			},
			{
				GameInput.Button.PDA,
				"<Gamepad>/select"
			},
			{
				GameInput.Button.Deconstruct,
				"<Gamepad>/dpad/down"
			},
			{
				GameInput.Button.Exit,
				GameInput.SwapAcceptCancel ? "<Gamepad>/buttonSouth" : "<Gamepad>/buttonEast"
			},
			{
				GameInput.Button.LeftHand,
				GameInput.SwapAcceptCancel ? "<Gamepad>/buttonEast" : "<Gamepad>/buttonSouth",
				"<Gamepad>/leftTrigger"
			},
			{
				GameInput.Button.RightHand,
				"<Gamepad>/rightTrigger"
			},
			{
				GameInput.Button.AltTool,
				"<Gamepad>/dpad/up"
			},
			{
				GameInput.Button.TakePicture,
				"<Gamepad>/rightStickPress"
			},
			{
				GameInput.Button.Reload,
				"<Gamepad>/buttonWest"
			},
			{
				GameInput.Button.Move,
				"<Gamepad>/leftStick"
			},
			{
				GameInput.Button.MoveUp,
				"<Gamepad>/leftShoulder"
			},
			{
				GameInput.Button.MoveDown,
				"<Gamepad>/rightShoulder"
			},
			{
				GameInput.Button.Sprint,
				"<Gamepad>/leftStickPress"
			},
			{
				GameInput.Button.Look,
				"<Gamepad>/rightStick"
			},
			{
				GameInput.Button.CycleNext,
				"<Gamepad>/dpad/right"
			},
			{
				GameInput.Button.CyclePrev,
				"<Gamepad>/dpad/left"
			},
			{
				GameInput.Button.UISubmit,
				GameInput.SwapAcceptCancel ? "<Gamepad>/buttonEast" : "<Gamepad>/buttonSouth"
			},
			{
				GameInput.Button.UICancel,
				GameInput.SwapAcceptCancel ? "<Gamepad>/buttonSouth" : "<Gamepad>/buttonEast"
			},
			{
				GameInput.Button.UIClear,
				"<Gamepad>/buttonWest"
			},
			{
				GameInput.Button.UIAssign,
				"<Gamepad>/buttonNorth"
			},
			{
				GameInput.Button.UIMenu,
				"<Gamepad>/start"
			},
			{
				GameInput.Button.UILeft,
				"<Gamepad>/dpad/left",
				"<Gamepad>/leftStick/left"
			},
			{
				GameInput.Button.UIRight,
				"<Gamepad>/dpad/right",
				"<Gamepad>/leftStick/right"
			},
			{
				GameInput.Button.UIDown,
				"<Gamepad>/dpad/down",
				"<Gamepad>/leftStick/down"
			},
			{
				GameInput.Button.UIUp,
				"<Gamepad>/dpad/up",
				"<Gamepad>/leftStick/up"
			},
			{
				GameInput.Button.UIAdjustLeft,
				"<Gamepad>/leftStick/left",
				"<Gamepad>/dpad/left"
			},
			{
				GameInput.Button.UIAdjustRight,
				"<Gamepad>/leftStick/right",
				"<Gamepad>/dpad/right"
			},
			{
				GameInput.Button.UIRightStickAdjustLeft,
				"<Gamepad>/rightStick/left"
			},
			{
				GameInput.Button.UIRightStickAdjustRight,
				"<Gamepad>/rightStick/right"
			},
			{
				GameInput.Button.UIPrevTab,
				"<Gamepad>/leftShoulder"
			},
			{
				GameInput.Button.UINextTab,
				"<Gamepad>/rightShoulder"
			},
			{
				GameInput.Button.UIAdjust,
				"<Gamepad>/rightStick"
			}
		};
		bindingsDualShock = new BindingDefinitions { 
		{
			GameInput.Button.PDA,
			"<DualShockGamepad>/touchpadButton"
		} };
		displayKeyboard = new DisplayDefinitions
		{
			{ "Delta", "Mouse" },
			{ "Left Button", "MouseButtonLeft" },
			{ "Middle Button", "MouseButtonMiddle" },
			{ "Right Button", "MouseButtonRight" },
			{ "Scroll/Up", "MouseWheelUp" },
			{ "Scroll/Down", "MouseWheelDown" },
			{ "Escape", "KeyboardEsc" },
			{ "Enter", "KeyboardEnter" },
			{ "`", "KeyboardBackquote" },
			{ "'", "KeyboardQuote" },
			{ ";", "KeyboardSemicolon" },
			{ ",", "KeyboardComma" },
			{ ".", "KeyboardPeriod" },
			{ "/", "KeyboardSlash" },
			{ "\\", "KeyboardBackslash" },
			{ "[", "KeyboardLeftBracket" },
			{ "]", "KeyboardRightBracket" },
			{ "-", "KeyboardMinus" },
			{ "=", "KeyboardEquals" },
			{ "Up Arrow", "KeyboardUp" },
			{ "Down Arrow", "KeyboardDown" },
			{ "Left Arrow", "KeyboardLeft" },
			{ "Right Arrow", "KeyboardRight" },
			{ "A", "KeyboardA" },
			{ "B", "KeyboardB" },
			{ "C", "KeyboardC" },
			{ "D", "KeyboardD" },
			{ "E", "KeyboardE" },
			{ "F", "KeyboardF" },
			{ "G", "KeyboardG" },
			{ "H", "KeyboardH" },
			{ "I", "KeyboardI" },
			{ "J", "KeyboardJ" },
			{ "K", "KeyboardK" },
			{ "L", "KeyboardL" },
			{ "M", "KeyboardM" },
			{ "N", "KeyboardN" },
			{ "O", "KeyboardO" },
			{ "P", "KeyboardP" },
			{ "Q", "KeyboardQ" },
			{ "R", "KeyboardR" },
			{ "S", "KeyboardS" },
			{ "T", "KeyboardT" },
			{ "U", "KeyboardU" },
			{ "V", "KeyboardV" },
			{ "W", "KeyboardW" },
			{ "X", "KeyboardX" },
			{ "Y", "KeyboardY" },
			{ "Z", "KeyboardZ" },
			{ "1", "Keyboard1" },
			{ "2", "Keyboard2" },
			{ "3", "Keyboard3" },
			{ "4", "Keyboard4" },
			{ "5", "Keyboard5" },
			{ "6", "Keyboard6" },
			{ "7", "Keyboard7" },
			{ "8", "Keyboard8" },
			{ "9", "Keyboard9" },
			{ "0", "Keyboard0" },
			{ "Left Shift", "LeftShift" },
			{ "Right Shift", "RightShift" },
			{ "Left Control", "LeftControl" },
			{ "Right Control", "RightControl" },
			{ "Backspace", "KeyboardBackspace" },
			{ "Numpad *", "KeyboardMultiply" },
			{ "Numpad +", "KeyboardPlus" },
			{ "F1", "KeyboardF1" },
			{ "F2", "KeyboardF2" },
			{ "F3", "KeyboardF3" },
			{ "F4", "KeyboardF4" },
			{ "F5", "KeyboardF5" },
			{ "F6", "KeyboardF6" },
			{ "F7", "KeyboardF7" },
			{ "F8", "KeyboardF8" },
			{ "F9", "KeyboardF9" },
			{ "F10", "KeyboardF10" },
			{ "F11", "KeyboardF11" },
			{ "F12", "KeyboardF12" }
		};
		displayController = new DisplayDefinitions
		{
			{ "Left Stick/Up", "ControllerLeftStickUp" },
			{ "Left Stick/Down", "ControllerLeftStickDown" },
			{ "Left Stick/Left", "ControllerLeftStickLeft" },
			{ "Left Stick/Right", "ControllerLeftStickRight" },
			{ "Right Stick/Up", "ControllerRightStickUp" },
			{ "Right Stick/Down", "ControllerRightStickDown" },
			{ "Right Stick/Left", "ControllerRightStickLeft" },
			{ "Right Stick/Right", "ControllerRightStickRight" },
			{ "D-Pad/Up", "ControllerDPadUp" },
			{ "D-Pad/Down", "ControllerDPadDown" },
			{ "D-Pad/Left", "ControllerDPadLeft" },
			{ "D-Pad/Right", "ControllerDPadRight" },
			{ "Button North", "ControllerButtonY" },
			{ "Button South", "ControllerButtonA" },
			{ "Button West", "ControllerButtonX" },
			{ "Button East", "ControllerButtonB" },
			{ "Select", "ControllerButtonBack" },
			{ "Start", "ControllerButtonHome" },
			{ "D-Pad", "ControllerDPad" },
			{ "D-Pad Up", "ControllerDPadUp" },
			{ "D-Pad Down", "ControllerDPadDown" },
			{ "D-Pad Left", "ControllerDPadLeft" },
			{ "D-Pad Right", "ControllerDPadRight" },
			{ "Left Stick", "ControllerLeftStick" },
			{ "Left Stick Up", "ControllerLeftStickUp" },
			{ "Left Stick Down", "ControllerLeftStickDown" },
			{ "Left Stick Left", "ControllerLeftStickLeft" },
			{ "Left Stick Right", "ControllerLeftStickRight" },
			{ "Left Stick Press", "ControllerButtonLeftStick" },
			{ "Right Stick", "ControllerRightStick" },
			{ "Right Stick Up", "ControllerRightStickUp" },
			{ "Right Stick Down", "ControllerRightStickDown" },
			{ "Right Stick Left", "ControllerRightStickLeft" },
			{ "Right Stick Right", "ControllerRightStickRight" },
			{ "Right Stick Press", "ControllerButtonRightStick" },
			{ "Left Trigger", "ControllerLeftTrigger" },
			{ "Right Trigger", "ControllerRightTrigger" },
			{ "Left Shoulder", "ControllerButtonLeftBumper" },
			{ "Right Shoulder", "ControllerButtonRightBumper" },
			{ "Y", "ControllerButtonY" },
			{ "A", "ControllerButtonA" },
			{ "X", "ControllerButtonX" },
			{ "B", "ControllerButtonB" },
			{ "Left Bumper", "ControllerButtonLeftBumper" },
			{ "Right Bumper", "ControllerButtonRightBumper" }
		};
		displayMacKeyboard = new DisplayDefinitions
		{
			{ "Escape", "KeyboardMacEsc" },
			{ "Tab", "KeyboardMacTab" },
			{ "Left Shift", "KeyboardMacShift" },
			{ "Right Shift", "KeyboardMacShift" },
			{ "Left Alt", "KeyboardMacAlt" },
			{ "Right Alt", "KeyboardMacAlt" },
			{ "Left Control", "KeyboardMacCtrl" },
			{ "Right Control", "KeyboardMacCtrl" },
			{ "Left System", "KeyboardMacCommand" },
			{ "Right System", "KeyboardMacCommand" },
			{ "Backspace", "KeyboardMacBackspace" },
			{ "Caps Lock", "KeyboardMacCapsLock" }
		};
		displayPS4 = new DisplayDefinitions
		{
			{ "Triangle", "ControllerButtonPs4Triangle" },
			{ "Cross", "ControllerButtonPs4Cross" },
			{ "Square", "ControllerButtonPs4Square" },
			{ "Circle", "ControllerButtonPs4Circle" },
			{ "Share", "ControllerButtonPs4Share" },
			{ "Options", "ControllerButtonPs4Options" },
			{ "D-Pad", "ControllerPs4DPad" },
			{ "D-Pad Up", "ControllerPs4DPadUp" },
			{ "D-Pad Down", "ControllerPs4DPadDown" },
			{ "D-Pad Left", "ControllerPs4DPadLeft" },
			{ "D-Pad Right", "ControllerPs4DPadRight" },
			{ "L3", "ControllerButtonPs4LeftStick" },
			{ "R3", "ControllerButtonPs4RightStick" },
			{ "L2", "ControllerPs4L2" },
			{ "R2", "ControllerPs4R2" },
			{ "L1", "ControllerPs4L1" },
			{ "R1", "ControllerPs4R1" },
			{ "Touchpad Press", "ControllerButtonPs4TouchPad" },
			{ "System", "ControllerPsLogo" }
		};
		displayPS5 = new DisplayDefinitions
		{
			{ "Triangle", "ControllerButtonPs4Triangle" },
			{ "Cross", "ControllerButtonPs4Cross" },
			{ "Square", "ControllerButtonPs4Square" },
			{ "Circle", "ControllerButtonPs4Circle" },
			{ "Share", "ControllerButtonPs5Share" },
			{ "Options", "ControllerButtonPs5Options" },
			{ "D-Pad", "ControllerPs4DPad" },
			{ "D-Pad Up", "ControllerPs4DPadUp" },
			{ "D-Pad Down", "ControllerPs4DPadDown" },
			{ "D-Pad Left", "ControllerPs4DPadLeft" },
			{ "D-Pad Right", "ControllerPs4DPadRight" },
			{ "L3", "ControllerButtonPs4LeftStick" },
			{ "R3", "ControllerButtonPs4RightStick" },
			{ "L2", "ControllerPs5L2" },
			{ "R2", "ControllerPs5R2" },
			{ "L1", "ControllerPs5L1" },
			{ "R1", "ControllerPs5R1" },
			{ "Touchpad Press", "ControllerButtonPs5TouchPad" },
			{ "System", "ControllerPsLogo" }
		};
		displaySwitch = new DisplayDefinitions
		{
			{ "Button North", "ControllerSwitchX" },
			{ "Button South", "ControllerSwitchB" },
			{ "Button West", "ControllerSwitchY" },
			{ "Button East", "ControllerSwitchA" },
			{ "Minus", "ControllerSwitch-" },
			{ "Plus", "ControllerSwitch+" },
			{ "D-Pad", "ControllerSwitchDPad" },
			{ "D-Pad Up", "ControllerSwitchPadUp" },
			{ "D-Pad Down", "ControllerSwitchPadDown" },
			{ "D-Pad Left", "ControllerSwitchPadLeft" },
			{ "D-Pad Right", "ControllerSwitchPadRight" },
			{ "Left Stick", "ControllerSwitchLeftStick" },
			{ "Left Stick Up", "ControllerSwitchLeftStickUp" },
			{ "Left Stick Down", "ControllerSwitchLeftStickDown" },
			{ "Left Stick Left", "ControllerSwitchLeftStickLeft" },
			{ "Left Stick Right", "ControllerSwitchLeftStickRight" },
			{ "Left Stick Press", "ControllerSwitchButtonLeftStick" },
			{ "Right Stick", "ControllerSwitchRightStick" },
			{ "Right Stick Up", "ControllerSwitchRightStickUp" },
			{ "Right Stick Down", "ControllerSwitchRightStickDown" },
			{ "Right Stick Left", "ControllerSwitchRightStickLeft" },
			{ "Right Stick Right", "ControllerSwitchRightStickRight" },
			{ "Right Stick Press", "ControllerSwitchButtonRightStick" },
			{ "ZL", "ControllerSwitchZL" },
			{ "ZR", "ControllerSwitchZR" },
			{ "L", "ControllerSwitchL" },
			{ "R", "ControllerSwitchR" },
			{ "Home", "ControllerSwitchHome" },
			{ "Capture", "ControllerSwitchScreenshot" }
		};
		bindingOptionsOrder = new GameInput.Button[30]
		{
			GameInput.Button.Move,
			GameInput.Button.MoveForward,
			GameInput.Button.MoveBackward,
			GameInput.Button.MoveLeft,
			GameInput.Button.MoveRight,
			GameInput.Button.Look,
			GameInput.Button.LookUp,
			GameInput.Button.LookDown,
			GameInput.Button.LookLeft,
			GameInput.Button.LookRight,
			GameInput.Button.MoveUp,
			GameInput.Button.MoveDown,
			GameInput.Button.AutoMove,
			GameInput.Button.Jump,
			GameInput.Button.Sprint,
			GameInput.Button.PDA,
			GameInput.Button.LeftHand,
			GameInput.Button.RightHand,
			GameInput.Button.Exit,
			GameInput.Button.Reload,
			GameInput.Button.AltTool,
			GameInput.Button.Deconstruct,
			GameInput.Button.CycleNext,
			GameInput.Button.CyclePrev,
			GameInput.Button.Slot1,
			GameInput.Button.Slot2,
			GameInput.Button.Slot3,
			GameInput.Button.Slot4,
			GameInput.Button.Slot5,
			GameInput.Button.TakePicture
		};
		layoutToDeviceDefinitionCache = new Dictionary<string, DeviceDefinition>();
		layoutNameCache = new Dictionary<Substring, string>();
		compositeBindingGroups = new Dictionary<(GameInput.Device, GameInput.BindingSet), string>(GameInput.AllDevices.Length * GameInput.AllBindingSets.Length);
		using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
		{
			StringBuilder sb = stringBuilderPool.sb;
			GameInput.Device[] allDevices = GameInput.AllDevices;
			foreach (GameInput.Device device in allDevices)
			{
				GameInput.BindingSet[] allBindingSets = GameInput.AllBindingSets;
				foreach (GameInput.BindingSet bindingSet in allBindingSets)
				{
					(GameInput.Device, GameInput.BindingSet) key = (device, bindingSet);
					sb.Length = 0;
					sb.Append(device.AsString()).Append(';').Append(bindingSet.AsString());
					string value = sb.ToString();
					compositeBindingGroups.Add(key, value);
				}
			}
		}
		nameToAction = new Dictionary<string, GameInput.Button>(GameInput.AllActions.Length);
		GameInput.Button[] allActions = GameInput.AllActions;
		foreach (GameInput.Button button in allActions)
		{
			nameToAction.Add(button.AsString(), button);
		}
	}

	public void Initialize()
	{
		defaultDeviceDefinitions = new Dictionary<GameInput.Device, DeviceDefinition>(GameInput.AllDevices.Length);
		GameInput.Device[] allDevices = GameInput.AllDevices;
		foreach (GameInput.Device device in allDevices)
		{
			DeviceDefinition value = null;
			switch (device)
			{
			case GameInput.Device.Keyboard:
				value = new DeviceDefinition(GameInput.Device.Keyboard, "Keyboard", bindingsKeyboard, displayKeyboard);
				break;
			case GameInput.Device.Controller:
				value = new DeviceDefinition(GameInput.Device.Controller, "Gamepad", bindingsController, displayController);
				break;
			default:
				Debug.LogErrorFormat("DeviceDefinition for {0} is not set! You should add it in {1}.{2}", device, "GameInputSystem", "Initialize");
				break;
			}
			defaultDeviceDefinitions[device] = value;
		}
		deviceDefinitions = new DeviceDefinition[3]
		{
			new DeviceDefinition(GameInput.Device.Controller, "DualSenseGamepadHID", bindingsDualShock, displayPS5),
			new DeviceDefinition(GameInput.Device.Controller, "DualShock4GamepadHID", bindingsDualShock, displayPS4),
			new DeviceDefinition(GameInput.Device.Controller, "SwitchProControllerHID", bindingsController, displaySwitch)
		};
		lastDevices = new Dictionary<GameInput.Device, DeviceInfo>(GameInput.AllDevices.Length);
		allDevices = GameInput.AllDevices;
		foreach (GameInput.Device device2 in allDevices)
		{
			InputDevice inputDevice = null;
			switch (device2)
			{
			case GameInput.Device.Keyboard:
				inputDevice = Keyboard.current;
				break;
			case GameInput.Device.Controller:
				inputDevice = Gamepad.current;
				break;
			}
			DeviceDefinition deviceDefinition = ((inputDevice != null) ? GetDeviceDefinitionForLayout(inputDevice.layout) : null);
			if (deviceDefinition == null)
			{
				deviceDefinition = defaultDeviceDefinitions[device2];
			}
			lastDevices[device2] = new DeviceInfo
			{
				inputDevice = inputDevice,
				definition = deviceDefinition
			};
		}
		lastDevice = GameInput.Device.Keyboard;
		inputActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
		actionMapGameplay = new InputActionMap("Gameplay");
		inputActionAsset.AddActionMap(actionMapGameplay);
		GameInput.Button[] allActions = GameInput.AllActions;
		foreach (GameInput.Button button in allActions)
		{
			GameInput.InputType inputType = GameInput.GetInputType(button);
			InputActionType type = ((inputType == GameInput.InputType.Button || (uint)(inputType - 1) > 1u) ? InputActionType.Button : InputActionType.Value);
			InputAction inputAction = actionMapGameplay.AddAction(button.AsString(), type);
			inputAction.started += OnActionStarted;
			actions.Add(button, inputAction);
		}
		using (DeferBindingResolution())
		{
			allDevices = GameInput.AllDevices;
			foreach (GameInput.Device key in allDevices)
			{
				AddBindings(defaultDeviceDefinitions[key]);
			}
		}
		allDevices = GameInput.AllDevices;
		foreach (GameInput.Device key2 in allDevices)
		{
			DeviceInfo deviceInfo = lastDevices[key2];
			if (deviceInfo.definition != defaultDeviceDefinitions[key2])
			{
				ChangeBindings(deviceInfo.definition);
			}
		}
		allDevices = GameInput.AllDevices;
		foreach (GameInput.Device device3 in allDevices)
		{
			string text = device3.AsString();
			InputControlScheme inputControlScheme = new InputControlScheme(text, null, text);
			switch (device3)
			{
			case GameInput.Device.Keyboard:
				inputControlScheme = inputControlScheme.WithRequiredDevice("<Keyboard>").WithRequiredDevice("<Mouse>");
				break;
			case GameInput.Device.Controller:
				inputControlScheme = inputControlScheme.WithRequiredDevice("<Gamepad>");
				break;
			}
			inputActionAsset.AddControlScheme(inputControlScheme);
		}
		UpdateDevicesState("Gamepad", controllerEnabled);
		InputSystem.onDeviceChange += OnDeviceChange;
		InputSystem.onAfterUpdate += OnAfterUpdate;
		InputSystem.settings.defaultDeadzoneMin = 0.2f;
		InputSystem.settings.defaultDeadzoneMax = 0.92f;
		actionMapGameplay.Enable();
		GameInput.OnPrimaryDeviceChanged += OnPrimaryDeviceChanged;
		GameInput.OnBindingsChanged += OnBindingsChanged;
		DevConsole.RegisterConsoleCommand("input2json", OnConsoleCommand_input2json);
		DevConsole.RegisterConsoleCommand("inputoverrides2json", OnConsoleCommand_inputoverrides2json);
	}

	public void Deinitialize()
	{
		InputSystem.onAfterUpdate -= OnAfterUpdate;
		GameInput.OnBindingsChanged -= OnBindingsChanged;
		GameInput.OnPrimaryDeviceChanged -= OnPrimaryDeviceChanged;
		inputActionAsset.Disable();
		InputSystem.onDeviceChange -= OnDeviceChange;
		UnityEngine.Object.Destroy(inputActionAsset);
		DevConsole.UnregisterConsoleCommand("input2json", OnConsoleCommand_input2json);
		DevConsole.UnregisterConsoleCommand("inputoverrides2json", OnConsoleCommand_inputoverrides2json);
	}

	public void OnUpdate()
	{
	}

	public void PopulateSettings(uGUI_OptionsPanel panel)
	{
		bindingOptions.Clear();
		dialog = panel.dialog;
		int tabIndex = panel.AddTab("Input");
		panel.AddChoiceOption(tabIndex, "RunMode", Localization.RunModeOptions, (int)GameInput.RunMode, delegate(int value)
		{
			GameInput.RunMode = (GameInput.RunModeOption)value;
		}, "RunModeTooltip");
		panel.AddHeading(tabIndex, "Mouse");
		panel.AddToggleOption(tabIndex, "InvertLook", InvertMouse, OnInvertMouseChanged);
		panel.AddSliderOption(tabIndex, "MouseSensitivity", MouseSensitivity, 0.01f, 1f, 0.15f, 0.01f, OnMouseSensitivityChanged, SliderLabelMode.Percent, "0");
		panel.AddHeading(tabIndex, "Keyboard");
		PopulateBindingSettings(panel, tabIndex, GameInput.Device.Keyboard);
		panel.AddHeading(tabIndex, "Controller");
		if (IsKeyboardOrMouseAvailable)
		{
			panel.AddToggleOption(tabIndex, "EnableController", ControllerEnabled, delegate(bool value)
			{
				ControllerEnabled = value;
			});
		}
		if (XRSettings.enabled)
		{
			panel.AddToggleOption(tabIndex, "GazeBasedCursor", VROptions.gazeBasedCursor, delegate(bool value)
			{
				VROptions.gazeBasedCursor = value;
				GameInput.ClearInput();
			});
		}
		panel.AddToggleOption(tabIndex, "InvertLook", InvertController, OnInvertControllerChanged);
		Vector2 vector = ControllerSensitivity;
		panel.AddSliderOption(tabIndex, "HorizontalSensitivity", vector.x, 0.05f, 1f, 0.405f, 0.05f, OnControllerHorizontalSensitivityChanged, SliderLabelMode.Percent, "0");
		panel.AddSliderOption(tabIndex, "VerticalSensitivity", vector.y, 0.05f, 1f, 0.405f, 0.05f, OnControllerVerticalSensitivityChanged, SliderLabelMode.Percent, "0");
		panel.AddSliderOption(tabIndex, "SticksDeadzoneMin", InputSystem.settings.defaultDeadzoneMin, 0f, 0.3f, 0.2f, 0.01f, delegate(float value)
		{
			InputSystem.settings.defaultDeadzoneMin = value;
		}, SliderLabelMode.Float, "0.00");
		panel.AddSliderOption(tabIndex, "SticksDeadzoneMax", InputSystem.settings.defaultDeadzoneMax, 0.8f, 1f, 0.92f, 0.01f, delegate(float value)
		{
			InputSystem.settings.defaultDeadzoneMax = value;
		}, SliderLabelMode.Float, "0.00");
		PopulateBindingSettings(panel, tabIndex, GameInput.Device.Controller);
	}

	public GameInput.InputStateFlags GetButtonState(GameInput.Button action)
	{
		GameInput.InputStateFlags inputStateFlags = GameInput.InputStateFlags.None;
		if (actions.TryGetValue(action, out var value))
		{
			if (value.WasPressedThisFrame())
			{
				inputStateFlags |= GameInput.InputStateFlags.Down;
			}
			if (value.IsPressed())
			{
				inputStateFlags |= GameInput.InputStateFlags.Held;
			}
			if (value.WasReleasedThisFrame())
			{
				inputStateFlags |= GameInput.InputStateFlags.Up;
			}
		}
		return inputStateFlags;
	}

	public float GetButtonHeldTime(GameInput.Button action)
	{
		float value = 0f;
		if (actions.TryGetValue(action, out var value2) && value2.IsPressed() && startTimes.TryGetValue(value2.id, out value))
		{
			return Time.unscaledTime - value;
		}
		return 0f;
	}

	public float GetFloat(GameInput.Button action)
	{
		return 0f;
	}

	public Vector2 GetVector2(GameInput.Button action)
	{
		if (!actions.TryGetValue(action, out var value))
		{
			return Vector2.zero;
		}
		switch (action)
		{
		case GameInput.Button.Move:
		{
			Vector2 result2 = value.ReadValue<Vector2>();
			float magnitude = result2.magnitude;
			Vector2 result3 = default(Vector2);
			result3.x += (GameInput.GetButtonHeld(GameInput.Button.MoveRight) ? 1f : 0f);
			result3.x -= (GameInput.GetButtonHeld(GameInput.Button.MoveLeft) ? 1f : 0f);
			result3.y += (GameInput.GetButtonHeld(GameInput.Button.MoveForward) ? 1f : 0f);
			result3.y -= (GameInput.GetButtonHeld(GameInput.Button.MoveBackward) ? 1f : 0f);
			float magnitude2 = result3.magnitude;
			if (!(magnitude > magnitude2))
			{
				return result3;
			}
			return result2;
		}
		case GameInput.Button.Look:
		{
			Vector2 vector = Vector2.zero;
			int num = 0;
			InputControl activeControl = value.activeControl;
			if (activeControl != null)
			{
				_ = activeControl.device;
				string layout = activeControl.layout;
				if (InputSystem.IsFirstLayoutBasedOnSecond(layout, "Stick"))
				{
					num = 1;
				}
				else if (InputSystem.IsFirstLayoutBasedOnSecond(layout, "Delta"))
				{
					num = 2;
				}
				vector = value.ReadValue<Vector2>();
				_ = vector.magnitude;
			}
			int num2 = 0;
			Vector2 vector2 = vector;
			switch (num)
			{
			case 1:
			{
				Vector2 vector3 = ControllerSensitivity;
				float magnitude3 = vector2.magnitude;
				Vector2 obj = ((magnitude3 > 0f) ? (vector2 / magnitude3) : Vector2.zero);
				magnitude3 = Mathf.Pow(magnitude3, 2f) * 500f;
				vector2 = obj * magnitude3;
				vector2 *= vector3 * Time.deltaTime;
				if (InvertController)
				{
					vector2.y = 0f - vector2.y;
				}
				break;
			}
			case 2:
			{
				float num3 = MouseSensitivity;
				num3 *= 1.5f;
				num3 *= 0.5f;
				vector2 *= num3;
				if (InvertMouse)
				{
					vector2.y = 0f - vector2.y;
				}
				break;
			}
			}
			return vector2;
		}
		case GameInput.Button.UIAdjust:
		{
			Vector2 result = Vector2.zero;
			if (value.activeControl != null)
			{
				result = value.ReadValue<Vector2>();
			}
			return result;
		}
		default:
			return Vector2.zero;
		}
	}

	public bool StartRebind(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet, Action<int> callback)
	{
		if (TryGetBinding(device, action, bindingSet, out var inputAction, out var _))
		{
			PerformRebind(inputAction, device, action, bindingSet, callback);
			return true;
		}
		return false;
	}

	private void PerformRebind(InputAction inputAction, GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet, Action<int> callback)
	{
		inputAction.Disable();
		Action cleanup = delegate
		{
			inputAction.Enable();
			rebindOperation.Dispose();
			rebindOperation = null;
		};
		rebindOperation = new InputActionRebindingExtensions.RebindingOperation().WithAction(inputAction).OnMatchWaitForAnother(0.05f).WithMatchingEventsBeingSuppressed()
			.WithCancelingThrough("<Keyboard>/escape")
			.OnCancel(delegate
			{
				GameInput.ClearInput(1);
				cleanup();
				if (callback != null)
				{
					try
					{
						callback(-1);
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
					}
				}
			})
			.OnComplete(delegate
			{
				GameInput.ClearInput(1);
				cleanup();
				if (callback != null)
				{
					try
					{
						callback(1);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			})
			.OnApplyBinding(delegate(InputActionRebindingExtensions.RebindingOperation op, string path)
			{
				if (op.scores.Count > 0 && op.scores[0] > 0f)
				{
					GameInput.TryBind(device, action, bindingSet, path);
				}
			});
		Func<InputControl, InputEventPtr, float> callback2 = ComputeRebindScoreDefault;
		GameInput.InputType inputType = GameInput.GetInputType(action);
		switch (device)
		{
		case GameInput.Device.Keyboard:
			rebindOperation.WithControlsHavingToMatchPath("<Keyboard>").WithControlsHavingToMatchPath("<Mouse>").WithControlsExcluding("<Pointer>/position")
				.WithControlsExcluding("<Touchscreen>/touch*/position")
				.WithControlsExcluding("<Touchscreen>/touch*/delta")
				.WithControlsExcluding("<Mouse>/clickCount")
				.WithControlsExcluding("<Mouse>/delta")
				.WithControlsExcluding("<Keyboard>/anyKey")
				.WithControlsExcluding("<Keyboard>/leftMeta")
				.WithControlsExcluding("<Keyboard>/rightMeta");
			callback2 = ComputeRebindScoreForButton;
			break;
		case GameInput.Device.Controller:
			rebindOperation.WithControlsHavingToMatchPath("<Gamepad>").WithControlsExcluding("<Gamepad>/leftTriggerButton").WithControlsExcluding("<Gamepad>/rightTriggerButton");
			switch (inputType)
			{
			case GameInput.InputType.Button:
				callback2 = ComputeRebindScoreForButton;
				break;
			case GameInput.InputType.Float:
				callback2 = ComputeRebindScoreForFloat;
				break;
			case GameInput.InputType.Vector2:
				callback2 = ComputeRebindScoreForVector2;
				break;
			}
			break;
		}
		rebindOperation.WithExpectedControlType(string.Empty).OnComputeScore(callback2);
		rebindOperation.Start();
	}

	public void CancelRebind()
	{
		if (rebindOperation != null)
		{
			rebindOperation.Cancel();
			rebindOperation.Dispose();
			rebindOperation = null;
		}
	}

	public void SetBinding(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet, string binding)
	{
		if (TryGetBinding(device, action, bindingSet, out var inputAction, out var bindingIndex))
		{
			InputBinding bindingOverride = inputAction.bindings[bindingIndex];
			if (bindingOverride.overridePath != binding)
			{
				bindingOverride.overridePath = binding;
				inputAction.ApplyBindingOverride(bindingIndex, bindingOverride);
				GameInput.SetBindingsChanged();
			}
		}
	}

	public string GetBinding(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet)
	{
		if (TryGetBinding(device, action, bindingSet, out var inputAction, out var bindingIndex))
		{
			return inputAction.bindings[bindingIndex].effectivePath;
		}
		return string.Empty;
	}

	public void AppendDisplayText(string binding, StringBuilder sb, string color)
	{
		if (!string.IsNullOrEmpty(binding))
		{
			DeviceInfo deviceInfo = default(DeviceInfo);
			GameInput.Device key = GameInput.Device.Keyboard;
			if (TryGetLayout(binding, out string layout))
			{
				key = GetDeviceDefinitionForLayout(layout).device;
				deviceInfo = lastDevices[key];
			}
			if (!displayNameCache.TryGetValue(binding, out var path))
			{
				path = InputControlPath.ToHumanReadableString(binding, InputControlPath.HumanReadableStringOptions.OmitDevice, deviceInfo.inputDevice);
				displayNameCache.Add(binding, path);
			}
			string path2 = null;
			DeviceDefinition definition = deviceInfo.definition;
			if ((definition == null || !definition.display.TryGetValue(path, out path2)) && !defaultDeviceDefinitions[key].display.TryGetValue(path, out path2))
			{
				path2 = path;
			}
			GameInput.AppendTranslationOrSprite(path2, sb, color);
		}
	}

	public void GetAllActions(GameInput.Device device, string binding, List<BindConflict> result)
	{
		string value = device.AsString();
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		foreach (KeyValuePair<GameInput.Button, InputAction> action in actions)
		{
			ReadOnlyArray<InputBinding> bindings = action.Value.bindings;
			for (int i = 0; i < bindings.Count; i++)
			{
				InputBinding inputBinding = bindings[i];
				string groups = inputBinding.groups;
				if (string.IsNullOrEmpty(groups) || !groups.Contains(value))
				{
					continue;
				}
				string effectivePath = inputBinding.effectivePath;
				if (string.IsNullOrEmpty(effectivePath) || (!IsFirstPathBasedOnSecond(binding, effectivePath) && !IsFirstPathBasedOnSecond(effectivePath, binding)))
				{
					continue;
				}
				GameInput.BindingSet[] allBindingSets = GameInput.AllBindingSets;
				foreach (GameInput.BindingSet bindingSet in allBindingSets)
				{
					string value2 = bindingSet.AsString();
					if (groups.Contains(value2))
					{
						result.Add(new BindConflict(action.Key, bindingSet));
						break;
					}
				}
			}
		}
	}

	public void SerializeSettings(GameSettings.ISerializer serializer)
	{
		GameInput.RunMode = (GameInput.RunModeOption)serializer.Serialize("InputSystem/RunMode", (int)GameInput.RunMode);
		InvertMouse = serializer.Serialize("InputSystem/InvertMouse", InvertMouse);
		MouseSensitivity = serializer.Serialize("InputSystem/MouseSensitivity", MouseSensitivity);
		ControllerEnabled = serializer.Serialize("InputSystem/ControllerEnabled", ControllerEnabled);
		InvertController = serializer.Serialize("InputSystem/InvertController", InvertController);
		Vector2 vector = ControllerSensitivity;
		vector.x = serializer.Serialize("InputSystem/ControllerSensitivityX", vector.x);
		vector.y = serializer.Serialize("InputSystem/ControllerSensitivityY", vector.y);
		ControllerSensitivity = vector;
		InputSystem.settings.defaultDeadzoneMin = serializer.Serialize("InputSystem/DeadzoneMin", InputSystem.settings.defaultDeadzoneMin);
		InputSystem.settings.defaultDeadzoneMax = serializer.Serialize("InputSystem/DeadzoneMax", InputSystem.settings.defaultDeadzoneMax);
		GameInput.Device[] allDevices = GameInput.AllDevices;
		foreach (GameInput.Device device in allDevices)
		{
			GameInput.Button[] allActions = GameInput.AllActions;
			foreach (GameInput.Button button in allActions)
			{
				if (!GameInput.IsBindable(device, button))
				{
					continue;
				}
				GameInput.BindingSet[] allBindingSets = GameInput.AllBindingSets;
				foreach (GameInput.BindingSet bindingSet in allBindingSets)
				{
					if (!TryGetBinding(device, button, bindingSet, out var inputAction, out var bindingIndex))
					{
						continue;
					}
					string name = string.Format("{0}Binding/{1}/{2}/{3}", "InputSystem/", device.AsString(), button, bindingSet.AsString());
					InputBinding inputBinding = inputAction.bindings[bindingIndex];
					if (serializer.IsReading())
					{
						string text = serializer.Serialize(name, null);
						if (text != null)
						{
							GameInput.SetBinding(device, button, bindingSet, text);
						}
					}
					else
					{
						string overridePath = inputBinding.overridePath;
						if (overridePath != null)
						{
							serializer.Serialize(name, overridePath);
						}
					}
				}
			}
		}
	}

	public void UpgradeSettings(GameSettings.ISerializer serializer)
	{
		serializer.Serialize("Version", 0);
		_ = 10;
	}

	public void SetupDefaultSettings()
	{
		SetupDefaultBindings();
		InvertMouse = false;
		MouseSensitivity = 0.15f;
		ControllerEnabled = true;
		InvertController = false;
		ControllerSensitivity = new Vector2(0.405f, 0.405f);
		InputSystem.settings.defaultDeadzoneMin = 0.2f;
		InputSystem.settings.defaultDeadzoneMax = 0.92f;
	}

	public void DoDebug()
	{
		DoDebugImpl();
	}

	private void OnAfterUpdate()
	{
		InputDevice inputDevice = null;
		string text = lastLayout;
		foreach (KeyValuePair<GameInput.Button, InputAction> action in actions)
		{
			InputAction value = action.Value;
			if (!value.IsInProgress())
			{
				continue;
			}
			InputControl activeControl = value.activeControl;
			if (activeControl == null)
			{
				continue;
			}
			InputDevice device = activeControl.device;
			if (device != null)
			{
				string layout = device.layout;
				if (!string.IsNullOrEmpty(layout))
				{
					inputDevice = device;
					text = layout;
				}
			}
		}
		if (!(lastLayout != text) || string.IsNullOrEmpty(text))
		{
			return;
		}
		lastLayout = text;
		DeviceDefinition deviceDefinitionForLayout = GetDeviceDefinitionForLayout(text);
		if (deviceDefinitionForLayout == null)
		{
			return;
		}
		GameInput.Device device2 = deviceDefinitionForLayout.device;
		if (lastDevice != device2)
		{
			lastDevice = device2;
			GameInput.SetBindingsChanged();
		}
		if (lastDevices[device2].definition != deviceDefinitionForLayout)
		{
			DeviceInfo deviceInfo = default(DeviceInfo);
			deviceInfo.inputDevice = inputDevice;
			deviceInfo.definition = deviceDefinitionForLayout;
			DeviceInfo value2 = deviceInfo;
			if (string.Equals(text, "Mouse", StringComparison.OrdinalIgnoreCase))
			{
				value2.inputDevice = lastDevices[device2].inputDevice;
			}
			lastDevices[device2] = value2;
			ChangeBindings(deviceDefinitionForLayout);
			displayNameCache.Clear();
			GameInput.SetBindingsChanged();
		}
	}

	private bool TryGetBinding(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet, out InputAction inputAction, out int bindingIndex)
	{
		string value = device.AsString();
		string value2 = bindingSet.AsString();
		if (actions.TryGetValue(action, out var value3))
		{
			inputAction = value3;
			ReadOnlyArray<InputBinding> bindings = value3.bindings;
			for (int i = 0; i < bindings.Count; i++)
			{
				string groups = bindings[i].groups;
				if (!string.IsNullOrEmpty(groups) && groups.Contains(value) && groups.Contains(value2))
				{
					bindingIndex = i;
					return true;
				}
			}
		}
		inputAction = null;
		bindingIndex = -1;
		return false;
	}

	private void OnActionStarted(InputAction.CallbackContext ctx)
	{
		startTimes[ctx.action.id] = Time.unscaledTime;
	}

	public unsafe static float ComputeRebindScoreDefault(InputControl control, InputEventPtr eventPtr)
	{
		void* statePtrFromStateEvent = control.GetStatePtrFromStateEvent(eventPtr);
		float num = control.EvaluateMagnitude(statePtrFromStateEvent);
		if (!control.synthetic)
		{
			num += 1f;
		}
		return num;
	}

	public static float ComputeRebindScoreForButton(InputControl control, InputEventPtr eventPtr)
	{
		float num = ComputeRebindScoreDefault(control, eventPtr);
		string layout = control.layout;
		if (InputSystem.IsFirstLayoutBasedOnSecond(layout, "Button"))
		{
			num += 3f;
		}
		else if (InputSystem.IsFirstLayoutBasedOnSecond(layout, "Axis"))
		{
			num += 1f;
			if (control.synthetic)
			{
				num += 1.5f;
			}
		}
		return num;
	}

	public static float ComputeRebindScoreForFloat(InputControl control, InputEventPtr eventPtr)
	{
		float num = ComputeRebindScoreDefault(control, eventPtr);
		if (InputSystem.IsFirstLayoutBasedOnSecond(control.layout, "Axis"))
		{
			num += 1f;
		}
		return num;
	}

	public static float ComputeRebindScoreForVector2(InputControl control, InputEventPtr eventPtr)
	{
		float num = ComputeRebindScoreDefault(control, eventPtr);
		string layout = control.layout;
		if (InputSystem.IsFirstLayoutBasedOnSecond(layout, "Stick"))
		{
			return num + 1f;
		}
		if (InputSystem.IsFirstLayoutBasedOnSecond(layout, "Dpad"))
		{
			return num + 1f;
		}
		return float.MinValue;
	}

	private string GetCompositeGroup(GameInput.Device device, GameInput.BindingSet bindingSet)
	{
		(GameInput.Device, GameInput.BindingSet) key = (device, bindingSet);
		if (compositeBindingGroups.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	private void AddBindings(DeviceDefinition deviceDefinition)
	{
		BindingDefinitions bindings = deviceDefinition.bindings;
		foreach (KeyValuePair<GameInput.Button, InputAction> action in actions)
		{
			GameInput.Button key = action.Key;
			GameInput.BindingSet[] allBindingSets = GameInput.AllBindingSets;
			foreach (GameInput.BindingSet bindingSet in allBindingSets)
			{
				(GameInput.Button, GameInput.BindingSet) key2 = (key, bindingSet);
				if (!bindings.TryGetValue(key2, out var _))
				{
					bindings.Add(key, bindingSet, string.Empty);
				}
			}
		}
		foreach (KeyValuePair<(GameInput.Button, GameInput.BindingSet), string> item3 in bindings)
		{
			(GameInput.Button, GameInput.BindingSet) key3 = item3.Key;
			GameInput.Button item = key3.Item1;
			GameInput.BindingSet item2 = key3.Item2;
			string compositeGroup = GetCompositeGroup(deviceDefinition.device, item2);
			string value = item3.Value;
			if (actions.TryGetValue(item, out var value2))
			{
				value2.AddBinding(value, null, null, compositeGroup);
			}
		}
	}

	private void ChangeBindings(DeviceDefinition definition)
	{
		GameInput.Device device = definition.device;
		BindingDefinitions bindings = defaultDeviceDefinitions[device].bindings;
		using (DeferBindingResolution())
		{
			string value = device.AsString();
			foreach (InputActionMap actionMap in inputActionAsset.actionMaps)
			{
				ReadOnlyArray<InputBinding> bindings2 = actionMap.bindings;
				for (int i = 0; i < bindings2.Count; i++)
				{
					InputBinding inputBinding = bindings2[i];
					string groups = inputBinding.groups;
					if (string.IsNullOrEmpty(groups) || !groups.Contains(value) || !nameToAction.TryGetValue(inputBinding.action, out var value2))
					{
						continue;
					}
					GameInput.BindingSet item = GameInput.BindingSet.Primary;
					bool flag = false;
					GameInput.BindingSet[] allBindingSets = GameInput.AllBindingSets;
					foreach (GameInput.BindingSet bindingSet in allBindingSets)
					{
						if (groups.Contains(bindingSet.AsString()))
						{
							item = bindingSet;
							flag = true;
							break;
						}
					}
					if (flag)
					{
						string path = null;
						(GameInput.Button, GameInput.BindingSet) key = (value2, item);
						if (!definition.bindings.TryGetValue(key, out path) && definition.bindings != bindings)
						{
							bindings.TryGetValue(key, out path);
						}
						if (path != null)
						{
							actionMap.ChangeBinding(i).WithPath(path);
						}
					}
				}
			}
		}
	}

	private void RemoveAllBindingOverrides(GameInput.Device device)
	{
		using (DeferBindingResolution())
		{
			GameInput.SetBindingsChanged();
			string value = device.AsString();
			foreach (InputActionMap actionMap in inputActionAsset.actionMaps)
			{
				ReadOnlyArray<InputBinding> bindings = actionMap.bindings;
				for (int i = 0; i < bindings.Count; i++)
				{
					string groups = bindings[i].groups;
					if (!string.IsNullOrEmpty(groups) && groups.Contains(value))
					{
						actionMap.ApplyBindingOverride(i, default(InputBinding));
					}
				}
			}
		}
	}

	private void SetupDefaultBindings()
	{
		using (DeferBindingResolution())
		{
			RemoveAllBindingOverrides(GameInput.Device.Keyboard);
			RemoveAllBindingOverrides(GameInput.Device.Controller);
		}
	}

	private void UpdateDevicesState(string layout, bool value)
	{
		foreach (InputDevice device in InputSystem.devices)
		{
			UpdateDeviceState(device, layout, value);
		}
	}

	private void UpdateDeviceState(InputDevice device, string layout, bool value)
	{
		if (InputSystem.IsFirstLayoutBasedOnSecond(device.layout, layout))
		{
			if (value)
			{
				InputSystem.EnableDevice(device);
			}
			else
			{
				InputSystem.DisableDevice(device);
			}
		}
	}

	private static bool IsDeviceAvailable(string layout)
	{
		foreach (InputDevice device in InputSystem.devices)
		{
			if (InputSystem.IsFirstLayoutBasedOnSecond(device.layout, layout))
			{
				return true;
			}
		}
		return false;
	}

	private void OnDeviceChange(InputDevice device, InputDeviceChange change)
	{
		switch (change)
		{
		case InputDeviceChange.Added:
			UpdateDeviceState(device, "Gamepad", controllerEnabled);
			break;
		case InputDeviceChange.ConfigurationChanged:
			if (InputSystem.IsFirstLayoutBasedOnSecond(device.layout, "Keyboard"))
			{
				displayNameCache.Clear();
				GameInput.SetBindingsChanged();
			}
			break;
		}
	}

	private void PopulateBindingSettings(uGUI_OptionsPanel panel, int tabIndex, GameInput.Device device)
	{
		panel.AddBindingsHeader(tabIndex);
		UnityAction callback = delegate
		{
			RemoveAllBindingOverrides(device);
		};
		panel.AddButton(tabIndex, "ResetToDefault", callback);
		List<GameInput.Button> list = new List<GameInput.Button>();
		GameInput.Button[] allActions = GameInput.AllActions;
		foreach (GameInput.Button button in allActions)
		{
			if (device == GameInput.Device.Controller)
			{
				if ((uint)(button - 19) <= 3u || (uint)(button - 24) <= 3u)
				{
					continue;
				}
			}
			else if (device == GameInput.Device.Keyboard && (uint)(button - 45) <= 1u)
			{
				continue;
			}
			if (GameInput.IsBindable(device, button))
			{
				list.Add(button);
			}
		}
		list.Sort(delegate(GameInput.Button a, GameInput.Button b)
		{
			int num = Array.IndexOf(bindingOptionsOrder, a);
			int num2 = Array.IndexOf(bindingOptionsOrder, b);
			if (num < 0)
			{
				num = Array.IndexOf(GameInput.AllActions, a) + GameInput.AllActions.Length;
			}
			if (num2 < 0)
			{
				num2 = Array.IndexOf(GameInput.AllActions, b) + GameInput.AllActions.Length;
			}
			return num.CompareTo(num2);
		});
		foreach (GameInput.Button item2 in list)
		{
			string label = "Option" + item2.AsString();
			uGUI_Bindings item = panel.AddBindingOption(tabIndex, label, device, item2);
			bindingOptions.Add(item);
		}
	}

	private void OnPrimaryDeviceChanged()
	{
		for (int num = bindingOptions.Count - 1; num >= 0; num--)
		{
			uGUI_Bindings uGUI_Bindings2 = bindingOptions[num];
			if (uGUI_Bindings2 == null)
			{
				bindingOptions.RemoveAt(num);
			}
			else
			{
				uGUI_Bindings2.OnPrimaryDeviceChanged();
			}
		}
	}

	private void OnBindingsChanged()
	{
		for (int num = bindingOptions.Count - 1; num >= 0; num--)
		{
			uGUI_Bindings uGUI_Bindings2 = bindingOptions[num];
			if (uGUI_Bindings2 == null)
			{
				bindingOptions.RemoveAt(num);
			}
			else
			{
				uGUI_Bindings2.OnBindingsChanged();
			}
		}
	}

	private static DeviceDefinition GetDeviceDefinitionForLayout(string layout)
	{
		if (!layoutToDeviceDefinitionCache.TryGetValue(layout, out var value))
		{
			if (InputSystem.IsFirstLayoutBasedOnSecond(layout, "Gamepad"))
			{
				value = defaultDeviceDefinitions[GameInput.Device.Controller];
			}
			else if (InputSystem.IsFirstLayoutBasedOnSecond(layout, "Keyboard") || InputSystem.IsFirstLayoutBasedOnSecond(layout, "Mouse"))
			{
				value = defaultDeviceDefinitions[GameInput.Device.Keyboard];
			}
			DeviceDefinition[] array = deviceDefinitions;
			foreach (DeviceDefinition deviceDefinition in array)
			{
				if (InputSystem.IsFirstLayoutBasedOnSecond(layout, deviceDefinition.layout))
				{
					value = deviceDefinition;
					break;
				}
			}
			if (value == null)
			{
				Debug.LogErrorFormat("Can't recognize device with layout \"{0}\"! Most likely - it needs to be defined in {1}.", layout, "deviceDefinitions");
			}
			layoutToDeviceDefinitionCache.Add(layout, value);
		}
		return value;
	}

	private static bool TryGetLayout(string path, out Substring layout)
	{
		int num = path.IndexOf('<');
		if (num >= 0)
		{
			num++;
			int num2 = path.IndexOf('>', num);
			if (num2 > num)
			{
				layout = new Substring(path, num, num2 - num);
				return true;
			}
		}
		layout = default(Substring);
		return false;
	}

	private static bool TryGetLayout(string path, out string layout)
	{
		if (TryGetLayout(path, out Substring layout2))
		{
			if (!layoutNameCache.TryGetValue(layout2, out layout))
			{
				layout = layout2.ToString();
				layoutNameCache.Add(layout2, layout);
			}
			return true;
		}
		layout = string.Empty;
		return false;
	}

	public static bool IsFirstPathBasedOnSecond(string p1, string p2)
	{
		if (p1 == null || p2 == null)
		{
			return p1 == p2;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while (num4 >= 0)
		{
			num4 = p2.IndexOf('/', num2);
			int num5 = ((num4 < 0) ? p2.Length : num4) - num2;
			num3 = p1.IndexOf('/', num);
			if (((num3 < 0) ? p1.Length : num3) - num != num5)
			{
				return false;
			}
			for (int i = 0; i < num5; i++)
			{
				if (char.ToLowerInvariant(p1[num + i]) != char.ToLowerInvariant(p2[num2 + i]))
				{
					return false;
				}
			}
			num = num3 + 1;
			num2 = num4 + 1;
		}
		return true;
	}

	private static IDisposable DeferBindingResolution()
	{
		if (methodDeferBindingResolution == null)
		{
			methodDeferBindingResolution = typeof(InputActionRebindingExtensions).GetMethod("DeferBindingResolution", BindingFlags.Static | BindingFlags.NonPublic);
		}
		return (IDisposable)methodDeferBindingResolution.Invoke(null, null);
	}

	private void OnInvertMouseChanged(bool value)
	{
		InvertMouse = value;
	}

	private void OnMouseSensitivityChanged(float value)
	{
		MouseSensitivity = value;
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

	private void DoDebugImpl()
	{
		Dbg.Write("lastLayout: {0} lastDevice: {1}", lastLayout, lastDevice);
		Dbg.Write("lastDevices: ");
		GameInput.Device[] allDevices = GameInput.AllDevices;
		foreach (GameInput.Device device in allDevices)
		{
			DeviceInfo deviceInfo = lastDevices[device];
			Dbg.Write(" {0}: layout:\"{1}\"", device, deviceInfo.definition.layout);
		}
		Dbg.Write("anyKeyDown: {0}", AnyKeyDown);
		Dbg.Write("IsKeyboardOrMouseAvailable: {0}", IsKeyboardOrMouseAvailable);
		Vector3 moveDirection = GameInput.GetMoveDirection();
		Vector2 vector = GetVector2(GameInput.Button.Move);
		Vector2 vector2 = GetVector2(GameInput.Button.Look);
		Dbg.Write("deadZoneMin: {0} deadZoneMax: {1}", InputSystem.settings.defaultDeadzoneMin, InputSystem.settings.defaultDeadzoneMax);
		Dbg.Write("moveDirection: {0:0.0000} {1:0.0000} {2:0.0000}", moveDirection.x, moveDirection.y, moveDirection.z);
		Dbg.Write("move         : {0:0.0000} {1:0.0000}", vector.x, vector.y);
		Dbg.Write("look         : {0:0.0000} {1:0.0000}", vector2.x, vector2.y);
		Dbg.Write("devices ({0} total):", InputSystem.devices.Count);
		for (int j = 0; j < InputSystem.devices.Count; j++)
		{
			InputDevice inputDevice = InputSystem.devices[j];
			Dbg.Write(" {0}. deviceId:{1} displayName:{2} enabled:{3} added:{4}", j, inputDevice.deviceId, inputDevice.displayName, inputDevice.enabled, inputDevice.added);
		}
		using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
		{
			StringBuilder sb = stringBuilderPool.sb;
			foreach (KeyValuePair<GameInput.Button, InputAction> action in actions)
			{
				InputAction value = action.Value;
				if (!value.IsInProgress())
				{
					continue;
				}
				InputControl activeControl = value.activeControl;
				if (activeControl != null)
				{
					InputDevice device2 = activeControl.device;
					if (device2 != null && !string.IsNullOrEmpty(device2.layout))
					{
						sb.Append("\n").Append("<color=#7092BE>action:</color>").Append(action.Key)
							.Append(" ")
							.Append("<color=#7092BE>type:</color>")
							.Append(value.type)
							.Append(" ")
							.Append("<color=#7092BE>phase:</color>")
							.Append(value.phase)
							.Append(" ")
							.Append("<color=#7092BE>deviceLayout:</color>")
							.Append(device2.layout)
							.Append(" ")
							.Append("<color=#7092BE>controlLayout:</color>")
							.Append(activeControl.layout)
							.Append(" ")
							.Append("<color=#7092BE>control:</color>")
							.Append(activeControl)
							.Append(" ");
					}
				}
			}
			if (sb.Length > 0)
			{
				Dbg.Write(sb);
			}
		}
		if (rebindOperation != null)
		{
			Dbg.Write("rebindOperation:");
			for (int k = 0; k < rebindOperation.candidates.Count; k++)
			{
				InputControl inputControl = rebindOperation.candidates[k];
				float num = rebindOperation.scores[k];
				Dbg.Write(" * {0} {1} {2}", k, inputControl, num);
			}
		}
		Dbg.Write("layoutNameCache ({0} total):", layoutNameCache.Count);
		foreach (KeyValuePair<Substring, string> item in layoutNameCache)
		{
			Dbg.Write(" {0}", item.Value);
		}
		Dbg.Write("displayNameCache:");
		foreach (KeyValuePair<string, string> item2 in displayNameCache)
		{
			Dbg.Write(" \"{0}\" = \"{1}\"", item2.Key, item2.Value);
		}
	}

	private void OnConsoleCommand_input2json(NotificationCenter.Notification n)
	{
		string text = inputActionAsset.ToJson();
		Debug.Log(text);
		GUIUtility.systemCopyBuffer = text;
	}

	private void OnConsoleCommand_inputoverrides2json(NotificationCenter.Notification n)
	{
		string text = inputActionAsset.SaveBindingOverridesAsJson();
		Debug.Log(text);
		GUIUtility.systemCopyBuffer = text;
	}
}
