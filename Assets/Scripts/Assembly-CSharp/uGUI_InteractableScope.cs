using System;
using System.Collections.Generic;

public struct uGUI_InteractableScope : IDisposable
{
	private static Stack<bool> stack = new Stack<bool>();

	private bool disposed;

	public uGUI_InteractableScope(bool interactable)
	{
		disposed = false;
		BeginInteractable(interactable);
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			EndInteractable();
		}
	}

	private static void BeginInteractable(bool interactable)
	{
		stack.Push(uGUI.interactable);
		uGUI.interactable &= interactable;
	}

	private static void EndInteractable()
	{
		if (stack.Count > 0)
		{
			uGUI.interactable = stack.Pop();
		}
	}
}
