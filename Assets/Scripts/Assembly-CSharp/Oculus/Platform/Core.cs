using System;
using UnityEngine;

namespace Oculus.Platform
{
	public sealed class Core
	{
		private static bool IsPlatformInitialized;

		public static bool IsInitialized()
		{
			return IsPlatformInitialized;
		}

		internal static void ForceInitialized()
		{
			IsPlatformInitialized = true;
		}

		public static void Initialize()
		{
			if (string.IsNullOrEmpty(PlatformSettings.AppID))
			{
				throw new UnityException("Update your app id by selecting 'Oculus Platform' -> 'Platform Settings'");
			}
			Initialize(PlatformSettings.AppID);
		}

		public static void Initialize(string appId)
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				IsPlatformInitialized = new AndroidPlatform().Initialize();
			}
			else
			{
				if (Application.platform != RuntimePlatform.WindowsPlayer)
				{
					throw new NotImplementedException("Oculus platform is not implemented on this platform yet.");
				}
				IsPlatformInitialized = new WindowsPlatform().Initialize(appId);
			}
			if (!IsPlatformInitialized)
			{
				throw new UnityException("Oculus Platform failed to initialize.");
			}
		}
	}
}
