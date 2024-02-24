public interface IInputHandler
{
	bool HandleInput();

	bool HandleLateInput();

	void OnFocusChanged(InputFocusMode mode);
}
