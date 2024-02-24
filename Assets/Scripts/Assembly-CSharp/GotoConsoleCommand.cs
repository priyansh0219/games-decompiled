using System;
using System.Collections;
using System.Text;
using Gendarme;
using UWE;
using UnityEngine;

public class GotoConsoleCommand : MonoBehaviour
{
	public static GotoConsoleCommand main;

	[AssertNotNull]
	public TeleportCommandData data;

	private Coroutine gotoRoutine;

	private bool movingPlayer;

	private bool continueSpam;

	private void Awake()
	{
		main = this;
		DevConsole.RegisterConsoleCommand(this, "goto");
		DevConsole.RegisterConsoleCommand(this, "gotofast");
		DevConsole.RegisterConsoleCommand(this, "gotospam");
		DevConsole.RegisterConsoleCommand(this, "gotostop");
	}

	private void OnConsoleCommand_gotospam(NotificationCenter.Notification n)
	{
		continueSpam = true;
		StartCoroutine(GotoSpam());
	}

	private void OnConsoleCommand_gotostop(NotificationCenter.Notification n)
	{
		continueSpam = false;
	}

	private IEnumerator GotoSpam()
	{
		int choice1 = UnityEngine.Random.Range(0, data.locations.Length);
		int choice2 = UnityEngine.Random.Range(0, data.locations.Length);
		while (continueSpam)
		{
			yield return new WaitForSeconds(UnityEngine.Random.value * 3f);
			TeleportPosition teleportPosition = data.locations[choice1];
			int num = choice2;
			choice2 = choice1;
			choice1 = num;
			ErrorMessage.AddDebug("Jumping to position: " + teleportPosition.position);
			Player.main.SetPosition(teleportPosition.position);
			Player.main.OnPlayerPositionCheat();
		}
	}

	private IEnumerator GotoLocation(Vector3 position, bool gotoImmediate)
	{
		Vector3 dest = position;
		Vector3 vector = dest - Player.main.transform.position;
		Vector3 direction = vector.normalized;
		float magnitude = vector.magnitude;
		if (gotoImmediate)
		{
			Player.main.SetPosition(dest);
		}
		else
		{
			float num = 2.5f;
			float travelSpeed2 = 250f;
			if (magnitude / travelSpeed2 > num)
			{
				travelSpeed2 = magnitude / num;
			}
			movingPlayer = true;
			Player.main.playerController.SetEnabled(enabled: false);
			while (true)
			{
				Vector3 position2 = Player.main.transform.position;
				float magnitude2 = (dest - position2).magnitude;
				float num2 = travelSpeed2 * Time.deltaTime;
				if (magnitude2 < num2)
				{
					break;
				}
				Vector3 position3 = position2 + direction * num2;
				Player.main.SetPosition(position3);
				yield return CoroutineUtils.waitForNextFrame;
			}
			Player.main.SetPosition(dest);
		}
		if (position.y > 0f)
		{
			float travelSpeed2 = 15f;
			new Bounds(position, Vector3.zero);
			while (!LargeWorldStreamer.main.IsWorldSettled())
			{
				travelSpeed2 -= Time.deltaTime;
				if (travelSpeed2 < 0f)
				{
					break;
				}
				yield return CoroutineUtils.waitForNextFrame;
			}
		}
		Player.main.OnPlayerPositionCheat();
		Player.main.playerController.SetEnabled(enabled: true);
		movingPlayer = false;
	}

	public bool GotoLocation(string locationName, bool gotoImmediate)
	{
		TeleportPosition[] locations = data.locations;
		foreach (TeleportPosition teleportPosition in locations)
		{
			if (string.Equals(teleportPosition.name, locationName, StringComparison.OrdinalIgnoreCase))
			{
				ErrorMessage.AddDebug($"Jumping to {teleportPosition.name} at {teleportPosition.position}");
				GotoPosition(teleportPosition.position, gotoImmediate);
				return true;
			}
		}
		return false;
	}

	public void GotoPosition(Vector3 position, bool gotoImmediate = false)
	{
		if (gotoRoutine != null)
		{
			StopCoroutine(gotoRoutine);
			if (movingPlayer)
			{
				Player.main.playerController.SetEnabled(enabled: true);
				movingPlayer = false;
			}
		}
		gotoRoutine = StartCoroutine(GotoLocation(position, gotoImmediate));
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private void HandleGotoCommand(NotificationCenter.Notification n, bool gotoImmediate)
	{
		if (n.data != null && n.data.Count == 1)
		{
			string locationName = (string)n.data[0];
			if (GotoLocation(locationName, gotoImmediate))
			{
				return;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = ((n.data != null && n.data.Count == 1) ? ((string)n.data[0]) : string.Empty);
		bool flag = !string.IsNullOrEmpty(text);
		if (flag)
		{
			stringBuilder.AppendFormat("LOCATIONS MATCHING: {0}\n", text);
		}
		for (int i = 0; i < data.locations.Length; i++)
		{
			TeleportPosition teleportPosition = data.locations[i];
			if (!flag || teleportPosition.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				stringBuilder.Append(teleportPosition.name);
				stringBuilder.Append(", ");
			}
		}
		ErrorMessage.AddDebug(stringBuilder.ToString());
		Debug.Log(stringBuilder.ToString());
	}

	private void OnConsoleCommand_goto(NotificationCenter.Notification n)
	{
		HandleGotoCommand(n, gotoImmediate: false);
	}

	private void OnConsoleCommand_gotofast(NotificationCenter.Notification n)
	{
		HandleGotoCommand(n, gotoImmediate: true);
	}
}
