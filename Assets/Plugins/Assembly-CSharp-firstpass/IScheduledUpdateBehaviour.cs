public interface IScheduledUpdateBehaviour : IManagedBehaviour
{
	int scheduledUpdateIndex { get; set; }

	void ScheduledUpdate();
}
