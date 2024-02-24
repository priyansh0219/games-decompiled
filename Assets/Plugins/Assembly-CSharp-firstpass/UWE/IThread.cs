namespace UWE
{
	public interface IThread
	{
		bool IsIdle();

		int GetQueueLength();

		void Enqueue(Task.Function task, object owner, object state);

		void Stop();
	}
}
