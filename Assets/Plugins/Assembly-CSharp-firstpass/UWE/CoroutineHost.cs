using System.Collections;
using UnityEngine;

namespace UWE
{
	public class CoroutineHost : MonoBehaviour
	{
		private static CoroutineHost main;

		private static CoroutineHost Initialize()
		{
			if (!main)
			{
				GameObject obj = new GameObject("CoroutineHost");
				Object.DontDestroyOnLoad(obj);
				obj.hideFlags = HideFlags.HideInHierarchy;
				main = obj.AddComponent<CoroutineHost>();
			}
			return main;
		}

		public new static Coroutine StartCoroutine(IEnumerator coroutine)
		{
			return ((MonoBehaviour)Initialize()).StartCoroutine(coroutine);
		}

		public new static void StopCoroutine(Coroutine coroutine)
		{
			((MonoBehaviour)Initialize()).StopCoroutine(coroutine);
		}
	}
}
