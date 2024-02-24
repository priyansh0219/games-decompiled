using System;
using System.Collections.Generic;

public abstract class Pool<T> : IDisposable where T : Pool<T>, new()
{
	private static readonly Stack<Pool<T>> pool = new Stack<Pool<T>>();

	private bool disposed;

	public static T Get()
	{
		T val = null;
		if (pool.Count > 0)
		{
			val = (T)pool.Pop();
		}
		if (val == null)
		{
			val = new T();
		}
		val.disposed = false;
		return val;
	}

	protected abstract void Deinitialize();

	private void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
			Deinitialize();
			if (disposing)
			{
				pool.Push(this);
			}
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~Pool()
	{
		Dispose(disposing: false);
	}
}
