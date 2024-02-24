namespace rail
{
	public interface IRailIMEHelper
	{
		RailResult EnableIMEHelperTextInputWindow(bool enable, RailWindowPosition position);

		RailResult UpdateIMEHelperTextInputWindowPosition(RailWindowPosition position);
	}
}
