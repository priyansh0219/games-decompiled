using UWE;
using UnityEngine;

public class BuildModeInputHandler : IInputHandler
{
	public bool canHandleInput;

	private int focusFrame = -1;

	bool IInputHandler.HandleInput()
	{
		return true;
	}

	bool IInputHandler.HandleLateInput()
	{
		if (!canHandleInput)
		{
			return false;
		}
		FPSInputModule.current.EscapeMenu();
		Builder.Update();
		if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
		{
			UWE.Utils.lockCursor = true;
		}
		Builder.additiveRotation = Builder.CalculateAdditiveRotationFromInput(Builder.additiveRotation);
		if (UWE.Utils.lockCursor && GameInput.GetButtonDown(GameInput.Button.LeftHand))
		{
			if (Builder.TryPlace())
			{
				return false;
			}
		}
		else if (focusFrame != Time.frameCount && GameInput.GetButtonDown(GameInput.Button.RightHand))
		{
			return false;
		}
		return true;
	}

	void IInputHandler.OnFocusChanged(InputFocusMode mode)
	{
		switch (mode)
		{
		case InputFocusMode.Add:
		case InputFocusMode.Restore:
			focusFrame = Time.frameCount;
			UWE.Utils.lockCursor = true;
			break;
		case InputFocusMode.Remove:
			Builder.End();
			break;
		case InputFocusMode.Suspend:
			break;
		}
	}
}
