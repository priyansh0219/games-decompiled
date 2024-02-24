using System;
using System.Collections.Generic;
using System.Text;
using Steamworks;
using UnityEngine;

public class GameInputSteam : IGameInput
{
	private InputActionSetHandle_t actionSetGeneral;

	private InputAnalogActionHandle_t analogActionMove;

	private InputAnalogActionHandle_t analogActionLook;

	private Dictionary<GameInput.Button, InputDigitalActionHandle_t> digitalActions = new Dictionary<GameInput.Button, InputDigitalActionHandle_t>();

	private InputHandle_t[] inputHandles = new InputHandle_t[16];

	private int connectedControllersCount;

	private Vector2 inputMove;

	private Vector2 inputLook;

	public int GetConnectedControllersCount => connectedControllersCount;

	public string Id => "GameInputSteam";

	public bool IsRebinding => false;

	public bool AnyKeyDown => false;

	public GameInput.Device PrimaryDevice => GameInput.Device.Keyboard;

	public void Initialize()
	{
		SteamInput.Init(bExplicitlyCallRunFrame: true);
		SteamInput.SetInputActionManifestFilePath(SNUtils.InsideUnmanaged("SteamInput/steam_input_manifest.vdf"));
		actionSetGeneral = SteamInput.GetActionSetHandle("General");
		analogActionMove = SteamInput.GetAnalogActionHandle("Move");
		analogActionLook = SteamInput.GetAnalogActionHandle("Look");
		AddDigitalAction("Jump", GameInput.Button.Jump);
		AddDigitalAction("PDA", GameInput.Button.PDA);
		AddDigitalAction("Deconstruct", GameInput.Button.Deconstruct);
		AddDigitalAction("Exit", GameInput.Button.Exit);
		AddDigitalAction("LeftHand", GameInput.Button.LeftHand);
		AddDigitalAction("RightHand", GameInput.Button.RightHand);
		AddDigitalAction("CycleNext", GameInput.Button.CycleNext);
		AddDigitalAction("CyclePrev", GameInput.Button.CyclePrev);
		AddDigitalAction("Slot1", GameInput.Button.Slot1);
		AddDigitalAction("Slot2", GameInput.Button.Slot2);
		AddDigitalAction("Slot3", GameInput.Button.Slot3);
		AddDigitalAction("Slot4", GameInput.Button.Slot4);
		AddDigitalAction("Slot5", GameInput.Button.Slot5);
		AddDigitalAction("AltTool", GameInput.Button.AltTool);
		AddDigitalAction("TakePicture", GameInput.Button.TakePicture);
		AddDigitalAction("Reload", GameInput.Button.Reload);
		AddDigitalAction("Sprint", GameInput.Button.Sprint);
		AddDigitalAction("MoveUp", GameInput.Button.MoveUp);
		AddDigitalAction("MoveDown", GameInput.Button.MoveDown);
		AddDigitalAction("AutoMove", GameInput.Button.AutoMove);
	}

	public void Deinitialize()
	{
		SteamInput.Shutdown();
	}

	public void OnUpdate()
	{
		SteamInput.RunFrame();
		connectedControllersCount = SteamInput.GetConnectedControllers(inputHandles);
		Dbg.Write("SteamController ({0} total):", connectedControllersCount);
		for (int i = 0; i < connectedControllersCount; i++)
		{
			Dbg.Write("    i: {0}", i);
			InputHandle_t inputHandle_t = inputHandles[i];
			if (inputHandle_t == (InputHandle_t)0uL)
			{
				continue;
			}
			int gamepadIndexForController = SteamInput.GetGamepadIndexForController(inputHandle_t);
			ESteamInputType inputTypeForHandle = SteamInput.GetInputTypeForHandle(inputHandle_t);
			SteamInput.ActivateActionSet(inputHandle_t, actionSetGeneral);
			Dbg.Write("    index: {0} inputType: {1}", gamepadIndexForController, inputTypeForHandle);
			InputAnalogActionData_t analogActionData = SteamInput.GetAnalogActionData(inputHandle_t, analogActionMove);
			Dbg.Write("        Move active: {0} mode: {1} x: {2} y: {3}", analogActionData.bActive, analogActionData.eMode, analogActionData.x, analogActionData.y);
			if (analogActionData.bActive > 0)
			{
				inputMove.Set(analogActionData.x, 0f - analogActionData.y);
			}
			InputAnalogActionData_t analogActionData2 = SteamInput.GetAnalogActionData(inputHandle_t, analogActionLook);
			Dbg.Write("        Look active: {0} mode: {1} x: {2} y: {3}", analogActionData2.bActive, analogActionData2.eMode, analogActionData2.x, analogActionData2.y);
			if (analogActionData2.bActive > 0)
			{
				inputLook.Set(analogActionData2.x, analogActionData2.y);
			}
			foreach (KeyValuePair<GameInput.Button, InputDigitalActionHandle_t> digitalAction in digitalActions)
			{
				GameInput.Button key = digitalAction.Key;
				InputDigitalActionHandle_t value = digitalAction.Value;
				InputDigitalActionData_t digitalActionData = SteamInput.GetDigitalActionData(inputHandle_t, value);
				Dbg.Write("        {0} active: {1} state: {2}", key, digitalActionData.bActive, digitalActionData.bState);
			}
		}
	}

	public void PopulateSettings(uGUI_OptionsPanel panel)
	{
	}

	public GameInput.InputStateFlags GetButtonState(GameInput.Button action)
	{
		return GameInput.InputStateFlags.None;
	}

	public float GetButtonHeldTime(GameInput.Button action)
	{
		return 0f;
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
			return inputMove;
		case GameInput.Button.Look:
			return inputLook;
		default:
			return Vector2.zero;
		}
	}

	public bool StartRebind(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet, Action<int> callback)
	{
		return false;
	}

	public void CancelRebind()
	{
	}

	public void SetBinding(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet, string binding)
	{
	}

	public string GetBinding(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet)
	{
		return string.Empty;
	}

	public void AppendDisplayText(string binding, StringBuilder sb, string color)
	{
	}

	public void GetAllActions(GameInput.Device device, string binding, List<BindConflict> result)
	{
	}

	public void SerializeSettings(GameSettings.ISerializer serializer)
	{
	}

	public void UpgradeSettings(GameSettings.ISerializer serializer)
	{
	}

	public void SetupDefaultSettings()
	{
	}

	public void DoDebug()
	{
		Dbg.Write("TODO GameInputSteam Debug");
	}

	private void AddDigitalAction(string pszActionName, GameInput.Button button)
	{
		InputDigitalActionHandle_t digitalActionHandle = SteamInput.GetDigitalActionHandle(pszActionName);
		digitalActions.Add(button, digitalActionHandle);
	}

	private void AddDigitalAction(InputDigitalActionHandle_t handle, GameInput.Button[] buttons)
	{
		foreach (GameInput.Button key in buttons)
		{
			digitalActions.Add(key, handle);
		}
	}
}
