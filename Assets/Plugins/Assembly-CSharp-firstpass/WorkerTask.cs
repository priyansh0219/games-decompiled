using System;
using System.Collections;
using System.Threading;
using UWE;

public class WorkerTask : IWorkerTask, IEnumerator
{
	private static readonly WorkerThread workerThread = ThreadUtils.StartWorkerThread("I/O", "WorkerTask", ThreadPriority.BelowNormal, -2, 64);

	private readonly Action task;

	private bool done;

	public static readonly Task.Function ExecuteTaskDelegate = ExecuteTask;

	public object Current => CoroutineUtils.waitForNextFrame;

	public WorkerTask(Action task)
	{
		this.task = task;
	}

	public bool MoveNext()
	{
		return !done;
	}

	public void Reset()
	{
	}

	public void Execute()
	{
		try
		{
			task();
		}
		finally
		{
			done = true;
		}
	}

	public static WorkerTask Launch(Action task)
	{
		WorkerTask result = new WorkerTask(task);
		Utils.EnqueueWrap(workerThread, result);
		return result;
	}

	private static void ExecuteTask(object owner, object state)
	{
		((IWorkerTask)owner).Execute();
	}
}
