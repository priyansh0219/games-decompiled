public class uGUI_FeedbackPanel : uGUI_NavigableControlGrid
{
	public uGUI_FeedbackPanel leftPanel;

	public uGUI_FeedbackPanel rightPanel;

	public override uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY)
	{
		if (dirX < 0)
		{
			return leftPanel;
		}
		if (dirX > 0)
		{
			return rightPanel;
		}
		return null;
	}
}
