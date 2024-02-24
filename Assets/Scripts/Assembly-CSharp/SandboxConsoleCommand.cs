using UnityEngine;

public class SandboxConsoleCommand : MonoBehaviour
{
	public static SandboxConsoleCommand main;

	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "sandbox");
		main = this;
	}

	private void OnConsoleCommand_sandbox(NotificationCenter.Notification n)
	{
		Application.LoadLevel("CreatureSandbox");
	}
}
