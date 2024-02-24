using UnityEngine;

public class PlayerToolConsoleCommands : MonoBehaviour
{
	public static bool debugFirstUse { get; private set; }

	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "firstuse");
	}

	private void OnConsoleCommand_firstuse(NotificationCenter.Notification n)
	{
		debugFirstUse = !debugFirstUse;
		ErrorMessage.AddDebug($"firstUse cheat is now {debugFirstUse}");
	}
}
