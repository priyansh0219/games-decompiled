using UWE;

public class FreezeTimeInputHandler : IInputHandler
{
	private FreezeTime.Id id;

	public FreezeTime.Id Id => id;

	public FreezeTimeInputHandler(FreezeTime.Id id)
	{
		this.id = id;
	}

	public bool HandleInput()
	{
		return FreezeTime.Contains(id);
	}

	public bool HandleLateInput()
	{
		return FreezeTime.Contains(id);
	}

	public void OnFocusChanged(InputFocusMode mode)
	{
	}
}
