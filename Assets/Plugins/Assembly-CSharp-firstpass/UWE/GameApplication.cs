using UnityEngine;

namespace UWE
{
	public class GameApplication
	{
		public static bool isQuitting { get; private set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void AppAwake()
		{
			Application.quitting += OnApplicationQuitting;
		}

		private static void OnApplicationQuitting()
		{
			isQuitting = true;
		}

		public static void NotifyApplicationQuitting()
		{
			OnApplicationQuitting();
		}
	}
}
