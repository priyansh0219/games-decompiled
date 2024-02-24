public class uGUI_ControlInputPanel : uGUI_InputGroup
{
	public uGUI_NavigableControlGrid controlGrid;

	protected void OnEnable()
	{
		Select();
	}

	public override void OnSelect(bool lockMovement)
	{
		base.OnSelect(lockMovement);
		GamepadInputModule.current.SetCurrentGrid(controlGrid);
	}

	public override void OnDeselect()
	{
		base.OnDeselect();
		GamepadInputModule.current.SetCurrentGrid(null);
	}
}
