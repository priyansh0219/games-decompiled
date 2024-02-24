public interface INotificationListener
{
	void OnAdd(NotificationManager.Group group, string key);

	void OnRemove(NotificationManager.Group group, string key);
}
