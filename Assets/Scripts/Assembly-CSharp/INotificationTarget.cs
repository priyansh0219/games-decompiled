public interface INotificationTarget
{
	bool IsVisible();

	bool IsDestroyed();

	void Progress(float progress);
}
