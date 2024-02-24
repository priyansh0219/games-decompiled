using System.Collections;
using UWE;

public class AsyncAwaiter : IEnumerator
{
	private readonly IAsyncOperation operation;

	public object Current => CoroutineUtils.waitForNextFrame;

	public AsyncAwaiter(IAsyncOperation operation)
	{
		this.operation = operation;
	}

	public bool MoveNext()
	{
		return !operation.isDone;
	}

	public void Reset()
	{
	}
}
