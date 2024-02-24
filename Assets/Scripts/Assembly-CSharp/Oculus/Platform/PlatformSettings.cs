using UnityEngine;

namespace Oculus.Platform
{
	public sealed class PlatformSettings : ScriptableObject
	{
		[SerializeField]
		private string ovrAppID = "";

		private static PlatformSettings instance;

		public static string AppID
		{
			get
			{
				return Instance.ovrAppID;
			}
			set
			{
				Instance.ovrAppID = value;
			}
		}

		public static PlatformSettings Instance
		{
			get
			{
				if (instance == null)
				{
					instance = Resources.Load<PlatformSettings>("OculusPlatformSettings");
				}
				return instance;
			}
			set
			{
				instance = value;
			}
		}
	}
}
