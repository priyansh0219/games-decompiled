using System;
using System.Collections.Generic;
using System.Threading;
using Platform.Utils;
using UnityEngine;
using UnityEngine.Profiling;

namespace UWE
{
	public sealed class WorkerThread : IThread
	{
		[ThreadStatic]
		public static int executionCounter;

		private readonly string group;

		private readonly string name;

		private readonly int coreAffinityMask;

		private readonly System.Threading.ThreadPriority priority;

		private Thread thread;

		private readonly Queue<Task> pendingTasks;

		private bool waiting;

		private bool running;

		public WorkerThread(string group, string name, System.Threading.ThreadPriority priority, int coreAffinityMask, int initialCapacity)
		{
			this.group = group;
			this.name = name;
			this.coreAffinityMask = coreAffinityMask;
			this.priority = priority;
			pendingTasks = new Queue<Task>(initialCapacity);
			thread = Platform.Utils.ThreadUtils.AcquireBackgroundThread(Main);
		}

		public void Start()
		{
			running = true;
			Platform.Utils.ThreadUtils.StartBackgroundThread(thread, this);
		}

		public void Stop()
		{
			lock (pendingTasks)
			{
				running = false;
				Monitor.Pulse(pendingTasks);
			}
		}

		public bool IsRunning()
		{
			return running;
		}

		public bool IsIdle()
		{
			if (waiting)
			{
				return GetQueueLength() <= 0;
			}
			return false;
		}

		public int GetQueueLength()
		{
			return pendingTasks.Count;
		}

		public void Enqueue(Task.Function task, object owner, object state)
		{
			lock (pendingTasks)
			{
				Task item = new Task(task, owner, state);
				pendingTasks.Enqueue(item);
				Monitor.Pulse(pendingTasks);
			}
		}

		private bool TryDequeue(out Task task)
		{
			lock (pendingTasks)
			{
				while (running && pendingTasks.Count <= 0)
				{
					waiting = true;
					Monitor.Wait(pendingTasks);
					waiting = false;
				}
				if (running)
				{
					task = pendingTasks.Dequeue();
					return true;
				}
			}
			task = default(Task);
			return false;
		}

		private void Run()
		{
			Platform.Utils.ThreadUtils.SetThreadName(name);
			Platform.Utils.ThreadUtils.SetThreadPriority(priority);
			Platform.Utils.ThreadUtils.SetThreadAffinityMask(coreAffinityMask);
			Task task;
			while (TryDequeue(out task))
			{
				executionCounter++;
				task.Execute();
			}
			Profiler.EndThreadProfiling();
			lock (pendingTasks)
			{
				pendingTasks.Clear();
			}
			Platform.Utils.ThreadUtils.ReleaseBackgroundThread(thread);
			thread = null;
		}

		private static void Main(object state)
		{
			try
			{
				((WorkerThread)state).Run();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public bool IsAlive()
		{
			return thread != null;
		}
	}
}
