using UnityEngine;

public class NoDamageConsoleCommand : MonoBehaviour
{
	public static NoDamageConsoleCommand main;

	private bool noDamageCheat;

	private void Awake()
	{
		main = this;
		DevConsole.RegisterConsoleCommand(this, "nodamage");
	}

	private void OnConsoleCommand_nodamage(NotificationCenter.Notification n)
	{
		noDamageCheat = !noDamageCheat;
		ErrorMessage.AddDebug("nodamage cheat is now " + noDamageCheat);
	}

	public bool GetNoDamageCheat()
	{
		return noDamageCheat;
	}

	public void SetNoDamageCheat(bool state)
	{
		noDamageCheat = state;
	}
}
