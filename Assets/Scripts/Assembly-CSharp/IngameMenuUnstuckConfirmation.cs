public class IngameMenuUnstuckConfirmation : uGUI_NavigableControlGrid, uGUI_IButtonReceiver
{
	public IngameMenu menu;

	public void OnYes()
	{
		UnstuckPlayer.TryUnstuck();
		menu.Close();
	}

	public void OnNo()
	{
		menu.ChangeSubscreen("Main");
	}

	public bool OnButtonDown(GameInput.Button button)
	{
		if (button == GameInput.Button.UICancel)
		{
			OnBack();
			return true;
		}
		return false;
	}

	public void OnBack()
	{
		OnNo();
	}
}
