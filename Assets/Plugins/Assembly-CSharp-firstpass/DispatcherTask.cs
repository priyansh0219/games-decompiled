using System.Collections;
using System.Threading;

public class DispatcherTask : IEnumerator
{
	private readonly IEnumerator coroutine;

	private bool done;

	private readonly object doneLock = new object();

	public object Current => coroutine.Current;

	public DispatcherTask(IEnumerator coroutine)
	{
		this.coroutine = coroutine;
	}

	public bool MoveNext()
	{
		if (!coroutine.MoveNext())
		{
			lock (doneLock)
			{
				done = true;
				Monitor.Pulse(doneLock);
			}
			return false;
		}
		return true;
	}

	public void Reset()
	{
		coroutine.Reset();
	}

	public void Wait()
	{
		lock (doneLock)
		{
			while (!done)
			{
				Monitor.Wait(doneLock);
			}
		}
	}
}
