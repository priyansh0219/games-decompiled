using System;
using System.Collections.Generic;
using UnityEngine;

public struct uGUI_ColorScope : IDisposable
{
	private static Stack<Color?> stack = new Stack<Color?>();

	private bool disposed;

	public uGUI_ColorScope(Color color)
	{
		disposed = false;
		BeginColor(color);
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			EndColor();
		}
	}

	private static void BeginColor(Color color)
	{
		stack.Push(uGUI.overrideColor);
		uGUI.overrideColor = color;
	}

	private static void EndColor()
	{
		if (stack.Count > 0)
		{
			uGUI.overrideColor = stack.Pop();
		}
	}
}
