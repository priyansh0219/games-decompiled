using UnityEngine;

public class OxygenConsoleCommand : MonoBehaviour
{
	public static OxygenConsoleCommand main;

	public bool infiniteIfEditor = true;

	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "oxygen");
		main = this;
		_ = infiniteIfEditor;
	}

	private void OnConsoleCommand_oxygen(NotificationCenter.Notification n)
	{
		GameModeUtils.ToggleCheat(GameModeOption.NoOxygen);
		ErrorMessage.AddDebug("Oxygen cheat now " + GameModeUtils.IsCheatActive(GameModeOption.NoOxygen));
	}
}
