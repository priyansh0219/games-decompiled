using System.Text;
using Steamworks;

namespace Verse.Steam
{
	public static class SteamManager
	{
		private static bool initializedInt;

		public static bool Initialized => initializedInt;

		public static bool Active => true;

		public static void InitIfNeeded()
		{
		}

		public static void Update()
		{
			if (initializedInt)
			{
				SteamAPI.RunCallbacks();
				SteamDeck.Update();
			}
		}

		public static void ShutdownSteam()
		{
			if (initializedInt)
			{
				SteamDeck.Shutdown();
				SteamAPI.Shutdown();
				initializedInt = false;
			}
		}

		private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
		{
			Log.Error(pchDebugText.ToString());
		}
	}
}
