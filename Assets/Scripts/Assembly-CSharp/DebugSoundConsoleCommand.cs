using System.Text;
using UnityEngine;

public class DebugSoundConsoleCommand : MonoBehaviour
{
	public static DebugSoundConsoleCommand main;

	public bool debugIsActive;

	public bool debugMusic;

	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "debugsound");
		DevConsole.RegisterConsoleCommand(this, "debugmusic");
		DevConsole.RegisterConsoleCommand(this, "listmusic");
		DevConsole.RegisterConsoleCommand(this, "listambience");
		main = this;
	}

	private void OnConsoleCommand_debugsound(NotificationCenter.Notification n)
	{
		debugIsActive = !debugIsActive;
	}

	private void OnConsoleCommand_debugmusic(NotificationCenter.Notification n)
	{
		if (n.data != null && n.data.Count == 1 && int.TryParse((string)n.data[0], out var result))
		{
			debugMusic = result > 0;
		}
		FMODGameParams[] array = Object.FindObjectsOfType<FMODGameParams>();
		foreach (FMODGameParams fMODGameParams in array)
		{
			if (fMODGameParams.isPlaying)
			{
				ErrorMessage.AddDebug($"Playing {fMODGameParams.loopingEmitter.asset.name} (only in biome '{fMODGameParams.onlyInBiome}*')");
			}
		}
	}

	private void OnConsoleCommand_listmusic(NotificationCenter.Notification n)
	{
		ListFmodGameParams("music");
	}

	private void OnConsoleCommand_listambience(NotificationCenter.Notification n)
	{
		ListFmodGameParams("background");
	}

	private void ListFmodGameParams(string parentName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		FMODGameParams[] array = Object.FindObjectsOfType<FMODGameParams>();
		for (int i = 0; i < array.Length; i++)
		{
			FMODGameParams fMODGameParams = array[i];
			if ((bool)fMODGameParams.transform.parent && !(fMODGameParams.transform.parent.name != parentName))
			{
				stringBuilder.AppendFormat("{0} ({1}*)", fMODGameParams.loopingEmitter.asset.name, fMODGameParams.onlyInBiome);
				if (i % 2 == 0)
				{
					stringBuilder.Append(", ");
				}
				else
				{
					stringBuilder.Append('\n');
				}
			}
		}
		ErrorMessage.AddDebug(stringBuilder.ToString());
	}

	public bool IsActive()
	{
		return debugIsActive;
	}
}
