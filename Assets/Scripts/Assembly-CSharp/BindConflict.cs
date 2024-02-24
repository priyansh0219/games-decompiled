public struct BindConflict
{
	public GameInput.Button action;

	public GameInput.BindingSet bindingSet;

	public BindConflict(GameInput.Button action, GameInput.BindingSet bindingSet)
	{
		this.action = action;
		this.bindingSet = bindingSet;
	}
}
