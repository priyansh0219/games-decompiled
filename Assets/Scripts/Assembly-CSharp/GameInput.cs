using System;
using System.Collections.Generic;
using System.Text;
using Platform.Utils;
using TMPro;
using UnityEngine;

public static class GameInput
{
	public enum Device
	{
		Keyboard = 0,
		Controller = 1
	}

	public enum BindingSet
	{
		Primary = 0,
		Secondary = 1
	}

	public enum InputType
	{
		Button = 0,
		Float = 1,
		Vector2 = 2
	}

	public enum Button
	{
		Jump = 0,
		PDA = 1,
		Deconstruct = 2,
		Exit = 3,
		LeftHand = 4,
		RightHand = 5,
		CycleNext = 6,
		CyclePrev = 7,
		Slot1 = 8,
		Slot2 = 9,
		Slot3 = 10,
		Slot4 = 11,
		Slot5 = 12,
		AltTool = 13,
		TakePicture = 14,
		Reload = 15,
		Sprint = 16,
		MoveUp = 17,
		MoveDown = 18,
		MoveForward = 19,
		MoveBackward = 20,
		MoveLeft = 21,
		MoveRight = 22,
		AutoMove = 23,
		LookUp = 24,
		LookDown = 25,
		LookLeft = 26,
		LookRight = 27,
		UISubmit = 28,
		UICancel = 29,
		UIClear = 30,
		UIAssign = 31,
		UILeft = 32,
		UIRight = 33,
		UIUp = 34,
		UIDown = 35,
		UIMenu = 36,
		UIAdjustLeft = 37,
		UIAdjustRight = 38,
		UINextTab = 39,
		UIPrevTab = 40,
		Feedback = 41,
		UIRightStickAdjustLeft = 42,
		UIRightStickAdjustRight = 43,
		None = 44,
		Move = 45,
		Look = 46,
		UIAdjust = 47
	}

	[Flags]
	public enum InputStateFlags : uint
	{
		None = 0u,
		Down = 1u,
		Up = 2u,
		Held = 4u
	}

	public enum RunModeOption
	{
		HoldToRun = 0,
		HoldToWalk = 1,
		PressToToggle = 2
	}

	public class DeviceComparer : IEqualityComparer<Device>
	{
		public bool Equals(Device x, Device y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(Device obj)
		{
			return (int)obj;
		}
	}

	public class ActionComparer : IEqualityComparer<Button>
	{
		public bool Equals(Button x, Button y)
		{
			return x == y;
		}

		public int GetHashCode(Button action)
		{
			return (int)action;
		}
	}

	public class BindingSetComparer : IEqualityComparer<BindingSet>
	{
		public bool Equals(BindingSet x, BindingSet y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(BindingSet obj)
		{
			return (int)obj;
		}
	}

	public static readonly Button[] AllActions = (Button[])Enum.GetValues(typeof(Button));

	public static readonly BindingSet[] AllBindingSets = (BindingSet[])Enum.GetValues(typeof(BindingSet));

	public static readonly Device[] AllDevices = (Device[])Enum.GetValues(typeof(Device));

	public static readonly DeviceComparer sDeviceComparer = new DeviceComparer();

	public static readonly ActionComparer sActionComparer = new ActionComparer();

	public static readonly BindingSetComparer sBindingSetComparer = new BindingSetComparer();

	public static readonly CachedEnumString<Device> DeviceNames = new CachedEnumString<Device>(sDeviceComparer);

	public static readonly CachedEnumString<Button> ActionNames = new CachedEnumString<Button>(sActionComparer);

	public static readonly CachedEnumString<BindingSet> BindingSetNames = new CachedEnumString<BindingSet>(sBindingSetComparer);

	public const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.UpdateInput;

	private const float autoMoveThreshold = 0.1f;

	private const float runThreshold = 0.9f;

	public const string defaultControlsColor = "#ADF8FFFF";

	private const string commandDebug = "debuginput";

	private static RunModeOption runMode = RunModeOption.HoldToRun;

	private static bool autoMove = false;

	private static IGameInput input;

	private static Device lastPrimaryDevice;

	private static bool bindingsChanged;

	private static bool isRunning;

	private static bool isRunningMoveThreshold;

	private static Vector3 moveDirection;

	private static int clearInputFrame = -1;

	private static bool debugInput = false;

	public static Button button0 => Button.UISubmit;

	public static Button button1 => Button.UICancel;

	public static Button button2 => Button.UIClear;

	public static Button button3 => Button.UIAssign;

	public static bool SwapAcceptCancel => false;

	public static bool IsInitialized => input != null;

	public static bool IsRebinding
	{
		get
		{
			if (input != null)
			{
				return input.IsRebinding;
			}
			return false;
		}
	}

	public static RunModeOption RunMode
	{
		get
		{
			return runMode;
		}
		set
		{
			runMode = value;
		}
	}

	public static bool AutoMove
	{
		get
		{
			return autoMove;
		}
		set
		{
			autoMove = value;
		}
	}

	public static bool IsRunning
	{
		get
		{
			switch (runMode)
			{
			default:
				if (IsPrimaryDeviceGamepad() && isRunningMoveThreshold)
				{
					return true;
				}
				return GetButtonHeld(Button.Sprint);
			case RunModeOption.HoldToWalk:
				return !GetButtonHeld(Button.Sprint);
			case RunModeOption.PressToToggle:
				return isRunning;
			}
		}
	}

	public static Device PrimaryDevice => input.PrimaryDevice;

	public static bool AnyKeyDown => input.AnyKeyDown;

	public static bool clearInput => clearInputFrame >= Time.frameCount;

	public static event Action OnPrimaryDeviceChanged;

	public static event Action OnBindingsChanged;

	public static void Initialize(IGameInput value)
	{
		Deinitialize();
		if (value != null)
		{
			value.Initialize();
			input = value;
			ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateInput, OnUpdate);
			DevConsole.RegisterConsoleCommand("debuginput", OnConsoleCommand_debuginput);
		}
	}

	public static void Deinitialize()
	{
		if (input != null)
		{
			ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdateInput, OnUpdate);
			input.Deinitialize();
			input = null;
			DevConsole.UnregisterConsoleCommand("debuginput", OnConsoleCommand_debuginput);
		}
	}

	public static string AsString(this Device device)
	{
		return DeviceNames.Get(device);
	}

	public static string AsString(this Button action)
	{
		return ActionNames.Get(action);
	}

	public static string AsString(this BindingSet bindingSet)
	{
		return BindingSetNames.Get(bindingSet);
	}

	public static InputType GetInputType(Button action)
	{
		if ((uint)(action - 45) <= 2u)
		{
			return InputType.Vector2;
		}
		return InputType.Button;
	}

	public static void PopulateSettings(uGUI_OptionsPanel panel)
	{
		if (input != null)
		{
			input.PopulateSettings(panel);
		}
	}

	public static bool IsPrimaryDeviceGamepad()
	{
		return PrimaryDevice == Device.Controller;
	}

	public static bool IsBindable(Device device, Button action)
	{
		if (action == Button.None)
		{
			return false;
		}
		if (device == Device.Keyboard && (uint)(action - 24) <= 3u)
		{
			return false;
		}
		if ((uint)(action - 28) <= 15u || action == Button.UIAdjust)
		{
			return false;
		}
		return true;
	}

	public static float GetFloat(Button action)
	{
		if (!clearInput)
		{
			return input.GetFloat(action);
		}
		return 0f;
	}

	public static Vector2 GetVector2(Button action)
	{
		if (!IsRebinding && !clearInput)
		{
			return input.GetVector2(action);
		}
		return Vector2.zero;
	}

	public static bool GetButtonDown(Button action)
	{
		return HasFlag(action, InputStateFlags.Down);
	}

	public static bool GetButtonHeld(Button action)
	{
		return HasFlag(action, InputStateFlags.Held);
	}

	public static float GetButtonHeldTime(Button action)
	{
		if (!clearInput)
		{
			return input.GetButtonHeldTime(action);
		}
		return 0f;
	}

	public static bool GetButtonUp(Button action)
	{
		return HasFlag(action, InputStateFlags.Up);
	}

	public static void ClearInput(int numFrames = 0)
	{
		clearInputFrame = Time.frameCount + numFrames;
		InputUtils.ResetInputAxes();
	}

	public static void SetupDefaultSettings()
	{
		input.SetupDefaultSettings();
	}

	public static Vector2 GetLookDelta()
	{
		if (!clearInput)
		{
			return GetVector2(Button.Look);
		}
		return Vector2.zero;
	}

	public static Vector3 GetMoveDirection()
	{
		if (!clearInput)
		{
			return moveDirection;
		}
		return Vector3.zero;
	}

	public static bool StartRebind(Device device, Button action, BindingSet bindingSet, Action<int> callback)
	{
		if (PrimaryDevice != device)
		{
			return false;
		}
		input.CancelRebind();
		return input.StartRebind(device, action, bindingSet, callback);
	}

	public static void CancelRebind()
	{
		input.CancelRebind();
	}

	public static void SetBinding(Device device, Button action, BindingSet bindingSet, string binding)
	{
		input.SetBinding(device, action, bindingSet, binding);
	}

	public static string GetBinding(Device device, Button action, BindingSet bindingSet)
	{
		return input.GetBinding(device, action, bindingSet);
	}

	public static void GetAllActions(Device device, string binding, List<BindConflict> result)
	{
		if (!string.IsNullOrEmpty(binding) && result != null)
		{
			input.GetAllActions(device, binding, result);
		}
	}

	private static void AppendDisplayText(string binding, StringBuilder sb, string color)
	{
		input.AppendDisplayText(binding, sb, color);
	}

	public static void AppendTranslationOrSprite(string id, StringBuilder sb, string color = "#ADF8FFFF")
	{
		if (id == null)
		{
			id = string.Empty;
		}
		if (TMP_Settings.defaultSpriteAsset.GetSpriteIndexFromName(id) >= 0)
		{
			if (string.IsNullOrEmpty(color))
			{
				sb.Append("<sprite name=\"").Append(id).Append("\">");
			}
			else
			{
				sb.Append("<sprite name=\"").Append(id).Append("\" color=")
					.Append(color)
					.Append(">");
			}
		}
		else if (string.IsNullOrEmpty(color))
		{
			sb.Append(Language.main.Get(id));
		}
		else
		{
			AppendColor(color, Language.main.Get(id), sb);
		}
	}

	public static void SerializeSettings(GameSettings.ISerializer serializer)
	{
		string id = input.Id;
		if (!serializer.IsReading())
		{
			serializer.Serialize("InputBackend", id);
		}
		input.SerializeSettings(serializer);
	}

	public static void UpgradeSettings(GameSettings.ISerializer serializer)
	{
		input.UpgradeSettings(serializer);
	}

	public static void SetBindingsChanged()
	{
		bindingsChanged = true;
	}

	public static string FormatButton(Button action, bool allBindingSets = false)
	{
		using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
		{
			StringBuilder sb = stringBuilderPool.sb;
			AppendDisplayText(action, sb, allBindingSets);
			return sb.ToString();
		}
	}

	public static string GetDisplayText(string binding, string color = null)
	{
		using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
		{
			StringBuilder sb = stringBuilderPool.sb;
			AppendDisplayText(binding, sb, color);
			return sb.ToString();
		}
	}

	public static void AppendDisplayText(Button[] actions, StringBuilder sb)
	{
		if (actions == null)
		{
			return;
		}
		int i = 0;
		for (int num = actions.Length; i < num; i++)
		{
			if (i > 0)
			{
				sb.Append(' ');
			}
			AppendDisplayText(actions[i], sb);
		}
	}

	public static void AppendDisplayText(Button action, StringBuilder sb, bool allBindingSets = false)
	{
		string text = null;
		bool flag = true;
		bool flag2 = false;
		BindingSet[] allBindingSets2 = AllBindingSets;
		foreach (BindingSet bindingSet in allBindingSets2)
		{
			text = GetBinding(PrimaryDevice, action, bindingSet);
			if (text != null)
			{
				flag = false;
				if (flag2)
				{
					sb.Append(" / ");
				}
				AppendDisplayText(text, sb, "#ADF8FFFF");
				flag2 = true;
				if (!allBindingSets)
				{
					break;
				}
			}
		}
		if (flag)
		{
			AppendColor("#ADF8FFFF", Language.main.Get("NoInputAssigned"), sb);
		}
	}

	public static void SafeSetBinding(Device device, Button action, BindingSet bindingSet, string binding)
	{
		if (!string.IsNullOrEmpty(binding))
		{
			using (ListPool<BindConflict> listPool = Pool<ListPool<BindConflict>>.Get())
			{
				List<BindConflict> list = listPool.list;
				BindConflicts.GetConflicts(device, binding, action, list);
				for (int i = 0; i < list.Count; i++)
				{
					BindConflict bindConflict = list[i];
					SetBinding(device, bindConflict.action, bindConflict.bindingSet, string.Empty);
				}
			}
			BindingSet[] allBindingSets = AllBindingSets;
			foreach (BindingSet bindingSet2 in allBindingSets)
			{
				if (bindingSet2 != bindingSet && GetBinding(device, action, bindingSet2) == binding)
				{
					SetBinding(device, action, bindingSet2, string.Empty);
				}
			}
		}
		SetBinding(device, action, bindingSet, binding);
	}

	public static void TryBind(Device device, Button action, BindingSet bindingSet, string binding)
	{
		uGUI_Dialog uGUI_Dialog2 = ((uGUI.main != null) ? uGUI.main.dialog : null);
		if (uGUI_Dialog2 == null)
		{
			Debug.LogError("uGUI.main.dialog has to be set prior to calling this method");
			return;
		}
		if (string.IsNullOrEmpty(binding))
		{
			string binding2 = GetBinding(device, action, bindingSet);
			if (string.IsNullOrEmpty(binding2))
			{
				return;
			}
			string displayText = GetDisplayText(binding2, "#ADF8FFFF");
			string text = string.Format(Language.main.Get("UnbindFormat"), displayText, string.Format("<color=#ADF8FFFF>{0}</color>", Language.main.Get("Option" + action)));
			uGUI_Dialog2.Show(text, delegate(int option)
			{
				if (option == 1)
				{
					SetBinding(device, action, bindingSet, string.Empty);
				}
			}, Language.main.Get("No"), Language.main.Get("Yes"));
			return;
		}
		using (ListPool<BindConflict> listPool = Pool<ListPool<BindConflict>>.Get())
		{
			List<BindConflict> list = listPool.list;
			BindConflicts.GetConflicts(device, binding, action, list);
			if (list.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				string value = Language.main.Get("InputSeparator");
				for (int i = 0; i < list.Count; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(value);
					}
					stringBuilder.AppendFormat("<color=#ADF8FFFF>{0}</color>", Language.main.Get("Option" + list[i].action));
				}
				string displayText2 = GetDisplayText(binding, "#ADF8FFFF");
				string format = Language.main.GetFormat("BindConflictFormat", displayText2, stringBuilder, string.Format("<color=#ADF8FFFF>{0}</color>", Language.main.Get("Option" + action)));
				uGUI_Dialog2.Show(format, delegate(int option)
				{
					if (option == 1)
					{
						SafeSetBinding(device, action, bindingSet, binding);
					}
				}, Language.main.Get("No"), Language.main.Get("Yes"));
			}
			else
			{
				SafeSetBinding(device, action, bindingSet, binding);
			}
		}
	}

	public static float Resample(float value, float min, float max = 1f)
	{
		float num = ((value >= 0f) ? 1f : (-1f));
		value *= num;
		value = (value - min) / (max - min);
		if (value < 0f)
		{
			value = 0f;
		}
		else if (value > 1f)
		{
			value = 1f;
		}
		return num * value;
	}

	public static Vector2 Resample(Vector2 value, float min, float max = 1f)
	{
		float magnitude = value.magnitude;
		if (magnitude > 0f)
		{
			value /= magnitude;
			magnitude = (magnitude - min) / (max - min);
			if (magnitude < 0f)
			{
				magnitude = 0f;
			}
			else if (magnitude > 1f)
			{
				magnitude = 1f;
			}
			return value * magnitude;
		}
		return value;
	}

	private static void OnUpdate()
	{
		input.OnUpdate();
		UpdateMove();
		Device primaryDevice = input.PrimaryDevice;
		if (lastPrimaryDevice != primaryDevice)
		{
			lastPrimaryDevice = primaryDevice;
			bindingsChanged = true;
			if (GameInput.OnPrimaryDeviceChanged != null)
			{
				try
				{
					GameInput.OnPrimaryDeviceChanged();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		if (bindingsChanged)
		{
			bindingsChanged = false;
			if (GameInput.OnBindingsChanged != null)
			{
				try
				{
					GameInput.OnBindingsChanged();
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			}
		}
		if (debugInput)
		{
			input.DoDebug();
		}
	}

	private static void UpdateMove()
	{
		Vector2 vector = GetVector2(Button.Move);
		float num = 0f;
		num += (GetButtonHeld(Button.MoveUp) ? 1f : 0f);
		num -= (GetButtonHeld(Button.MoveDown) ? 1f : 0f);
		if (autoMove && vector.x * vector.x + vector.y * vector.y > 0.010000001f)
		{
			autoMove = false;
		}
		if (autoMove)
		{
			moveDirection.Set(0f, num, 1f);
		}
		else
		{
			moveDirection.Set(vector.x, num, vector.y);
		}
		if (IsPrimaryDeviceGamepad())
		{
			if (autoMove)
			{
				isRunningMoveThreshold = false;
			}
			else
			{
				isRunningMoveThreshold = moveDirection.sqrMagnitude > 0.80999994f;
				if (!isRunningMoveThreshold)
				{
					moveDirection /= 0.9f;
				}
			}
		}
		if (runMode == RunModeOption.PressToToggle && GetButtonDown(Button.Sprint))
		{
			isRunning = !isRunning;
		}
	}

	private static void AppendColor(string color, string text, StringBuilder sb)
	{
		sb.Append("<color=").Append(color).Append(">")
			.Append(text)
			.Append("</color>");
	}

	private static bool HasFlag(Button action, InputStateFlags flag)
	{
		if (clearInput)
		{
			return false;
		}
		return (input.GetButtonState(action) & flag) != 0;
	}

	private static void OnConsoleCommand_debuginput(NotificationCenter.Notification n)
	{
		debugInput = !debugInput;
	}
}
