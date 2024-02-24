public class uGUI_MainMenuNewGameMenu : uGUI_NavigableControlGrid, uGUI_IButtonReceiver
{
	public bool OnButtonDown(GameInput.Button button)
	{
		if (button == GameInput.Button.UICancel)
		{
			OnBack();
			return true;
		}
		return false;
	}

	public override void DeselectItem()
	{
		if (selectedItem != null)
		{
			uGUI_BasicColorSwap[] allComponentsInChildren = selectedItem.GetAllComponentsInChildren<uGUI_BasicColorSwap>();
			for (int i = 0; i < allComponentsInChildren.Length; i++)
			{
				allComponentsInChildren[i].makeTextWhite();
			}
		}
		base.DeselectItem();
	}

	public void OnBack()
	{
		MainMenuRightSide.main.OpenGroup("Home");
	}
}
