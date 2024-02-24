public class IngameMenuQuitConfirmation : uGUI_NavigableControlGrid, uGUI_IButtonReceiver
{
	public IngameMenu menu;

	private bool quitToDesktop;

	public void SetQuitToDesktop(bool _quitToDesktop)
	{
		quitToDesktop = _quitToDesktop;
	}

	public void OnYes()
	{
		menu.QuitGame(quitToDesktop);
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
