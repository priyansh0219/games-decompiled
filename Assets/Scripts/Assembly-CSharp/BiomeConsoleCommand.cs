using System;
using System.Linq;
using System.Text;
using UnityEngine;

public class BiomeConsoleCommand : MonoBehaviour
{
	public static BiomeConsoleCommand main;

	[AssertNotNull]
	public TeleportCommandData data;

	private void Awake()
	{
		main = this;
		DevConsole.RegisterConsoleCommand(this, "biome");
	}

	private void OnConsoleCommand_biome(NotificationCenter.Notification n)
	{
		bool flag = false;
		if (n.data != null && n.data.Count == 1)
		{
			string keyString = (string)n.data[0];
			TeleportPosition teleportPosition = data.locations.FirstOrDefault((TeleportPosition p) => string.Equals(p.name, keyString, StringComparison.OrdinalIgnoreCase));
			if (teleportPosition != null)
			{
				GotoConsoleCommand.main.GotoPosition(teleportPosition.position);
				flag = true;
			}
		}
		if (flag)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < data.locations.Length; i++)
		{
			TeleportPosition teleportPosition2 = data.locations[i];
			stringBuilder.Append(teleportPosition2.name);
			stringBuilder.Append(", ");
			if (i % 5 == 0)
			{
				stringBuilder.AppendLine();
			}
		}
		ErrorMessage.AddDebug(stringBuilder.ToString());
	}
}
