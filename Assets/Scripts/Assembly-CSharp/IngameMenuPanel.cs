using System;
using UnityEngine;

public class IngameMenuPanel : uGUI_NavigableControlGrid, uGUI_IButtonReceiver
{
	public Action onBack;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			OnBackInternal();
		}
	}

	public bool OnButtonDown(GameInput.Button button)
	{
		if (button == GameInput.button1)
		{
			OnBackInternal();
			return true;
		}
		return false;
	}

	public void OnBack()
	{
		OnBackInternal();
	}

	private void OnBackInternal()
	{
		if (onBack != null)
		{
			onBack();
		}
	}
}
