public static class GameInputExtensions
{
	public static bool GetButtonDown(GameInput.Button[] buttons)
	{
		int i = 0;
		for (int num = buttons.Length; i < num; i++)
		{
			if (GameInput.GetButtonDown(buttons[i]))
			{
				return true;
			}
		}
		return false;
	}
}
