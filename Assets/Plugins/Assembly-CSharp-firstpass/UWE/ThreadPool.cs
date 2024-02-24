using System.Threading;

namespace UWE
{
	public sealed class ThreadPool : IThread
	{
		private readonly WorkerThread[] workers;

		private readonly int numWorkers;

		private volatile int idx;

		public ThreadPool(string name, int numWorkers, ThreadPriority priority, int coreAffinityMask, int initialCapacity)
		{
			this.numWorkers = numWorkers;
			workers = new WorkerThread[numWorkers];
			for (int i = 0; i < numWorkers; i++)
			{
				workers[i] = ThreadUtils.StartWorkerThread(name, $"{name}-{i}", priority, coreAffinityMask, initialCapacity);
			}
		}

		public bool IsRunning()
		{
			WorkerThread[] array = workers;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].IsRunning())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsIdle()
		{
			WorkerThread[] array = workers;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].IsIdle())
				{
					return false;
				}
			}
			return true;
		}

		public bool IsAlive()
		{
			WorkerThread[] array = workers;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].IsAlive())
				{
					return true;
				}
			}
			return false;
		}

		public int GetQueueLength()
		{
			int num = 0;
			WorkerThread[] array = workers;
			foreach (WorkerThread workerThread in array)
			{
				num += workerThread.GetQueueLength();
			}
			return num;
		}

		public void Enqueue(Task.Function task, object owner, object state)
		{
			int num = NextIndex();
			workers[num].Enqueue(task, owner, state);
		}

		public void Stop()
		{
			WorkerThread[] array = workers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Stop();
			}
		}

		private int NextIndex()
		{
			int num = idx;
			int num2;
			do
			{
				num2 = num;
				int value = (num2 + 1) % numWorkers;
				num = Interlocked.CompareExchange(ref idx, value, num2);
			}
			while (num != num2);
			return num;
		}

		public static int GetAffinity(int mask, int idx)
		{
			int num = 0;
			for (int i = 0; i < 30; i++)
			{
				int num2 = 1 << i;
				if ((mask & num2) != 0)
				{
					if (idx == num)
					{
						return num2;
					}
					num++;
				}
			}
			return mask;
		}
	}
}
