public class DiscardTaskResult<T> : TaskResult<T>
{
	public static readonly DiscardTaskResult<T> Instance = new DiscardTaskResult<T>();

	private DiscardTaskResult()
	{
	}

	public override void Set(T value)
	{
	}
}
