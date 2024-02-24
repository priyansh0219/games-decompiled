public class MainMenuDeleteGame : uGUI_NavigableControlGrid, uGUI_IButtonReceiver
{
	public MainMenuLoadButton loadButton;

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
		loadButton.CancelDelete();
	}
}
