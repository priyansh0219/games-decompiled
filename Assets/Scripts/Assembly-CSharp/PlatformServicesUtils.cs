using System;
using System.Collections;
using System.IO;
using UnityEngine;

public static class PlatformServicesUtils
{
	public class AsyncOperation : IEnumerator
	{
		private bool done;

		private bool success;

		public object Current => null;

		public bool MoveNext()
		{
			return !done;
		}

		public void SetComplete(bool _success)
		{
			success = _success;
			done = true;
		}

		public void Reset()
		{
		}

		public bool GetSuccessful()
		{
			return success;
		}
	}

	public delegate void VirtualKeyboardFinished(bool success, bool cancelled, string text);

	public delegate void AchievementsChanged();

	private static string desktopUserMusicPath;

	public static string GetDesktopUserMusicPath()
	{
		if (desktopUserMusicPath == null)
		{
			desktopUserMusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
			desktopUserMusicPath = Path.Combine(desktopUserMusicPath, Application.companyName);
			desktopUserMusicPath = Path.Combine(desktopUserMusicPath, "Subnautica");
		}
		return desktopUserMusicPath;
	}

	public static bool IsRuntimePluginDllPresent(string name)
	{
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
			return File.Exists($"{Application.dataPath}/Plugins/x86_64/{name}.dll");
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
			return Directory.Exists($"{Application.dataPath}/Plugins/{name}.bundle");
		default:
			Debug.LogWarningFormat("Unhandled platform {0} when checking for {1}", Application.platform, name);
			return false;
		}
	}

	public static void DefaultOpenURL(string url)
	{
		Application.OpenURL(url);
	}
}
