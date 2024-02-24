using System.Collections.Generic;
using UnityEngine;

namespace UWE
{
	public class DebugCube : MonoBehaviour
	{
		public Color color = Color.blue;

		private static Stack<DebugCube> stack = new Stack<DebugCube>();

		public static DebugCube Create(string name)
		{
			DebugCube debugCube = ((stack.Count > 0) ? stack.Peek() : null);
			GameObject gameObject = new GameObject("");
			if (debugCube != null)
			{
				gameObject.transform.parent = debugCube.transform;
			}
			gameObject.name = name;
			return gameObject.AddComponent<DebugCube>();
		}

		public static DebugCube BeginGroup(string name)
		{
			DebugCube debugCube = Create(name);
			stack.Push(debugCube);
			debugCube.gameObject.name = name;
			return debugCube;
		}

		public static void EndGroup()
		{
			stack.Pop();
		}
	}
}
