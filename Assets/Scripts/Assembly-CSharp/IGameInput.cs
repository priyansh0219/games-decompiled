using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public interface IGameInput
{
	string Id { get; }

	bool IsRebinding { get; }

	GameInput.Device PrimaryDevice { get; }

	bool AnyKeyDown { get; }

	void Initialize();

	void Deinitialize();

	void OnUpdate();

	void PopulateSettings(uGUI_OptionsPanel panel);

	GameInput.InputStateFlags GetButtonState(GameInput.Button action);

	float GetButtonHeldTime(GameInput.Button action);

	float GetFloat(GameInput.Button action);

	Vector2 GetVector2(GameInput.Button action);

	bool StartRebind(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet, Action<int> callback);

	void CancelRebind();

	void SetBinding(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet, string binding);

	string GetBinding(GameInput.Device device, GameInput.Button action, GameInput.BindingSet bindingSet);

	void AppendDisplayText(string binding, StringBuilder sb, string color);

	void GetAllActions(GameInput.Device device, string binding, List<BindConflict> result);

	void SerializeSettings(GameSettings.ISerializer serializer);

	void UpgradeSettings(GameSettings.ISerializer serializer);

	void SetupDefaultSettings();

	void DoDebug();
}
