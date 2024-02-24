using System.Collections;

public class CoroutineTask<T> : IEnumerator
{
	private readonly IEnumerator coroutine;

	private readonly TaskResult<T> result;

	public object Current => coroutine.Current;

	public CoroutineTask(IEnumerator coroutine, TaskResult<T> result)
	{
		this.coroutine = coroutine;
		this.result = result;
	}

	public T GetResult()
	{
		return result.Get();
	}

	public bool MoveNext()
	{
		return coroutine.MoveNext();
	}

	public void Reset()
	{
		coroutine.Reset();
	}
}
