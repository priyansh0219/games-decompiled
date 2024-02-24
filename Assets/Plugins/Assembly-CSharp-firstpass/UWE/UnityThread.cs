using System.Collections;
using System.Collections.Generic;

namespace UWE
{
	public sealed class UnityThread : IThread
	{
		private readonly Queue<Task> tasks;

		public readonly string name;

		public UnityThread(string name, int initialCapacity)
		{
			tasks = new Queue<Task>(initialCapacity);
			this.name = name;
		}

		public bool IsIdle()
		{
			return GetQueueLength() <= 0;
		}

		public int GetQueueLength()
		{
			return tasks.Count;
		}

		public void Pump(int numTasks)
		{
			for (int i = 0; i < numTasks; i++)
			{
				if (!TryDequeue(out var task))
				{
					break;
				}
				task.Execute();
			}
		}

		public IEnumerator Pump()
		{
			while (true)
			{
				int count;
				lock (tasks)
				{
					count = tasks.Count;
				}
				Pump(count);
				yield return null;
			}
		}

		public void Enqueue(Task.Function task, object owner, object state)
		{
			Task item = new Task(task, owner, state);
			lock (tasks)
			{
				tasks.Enqueue(item);
			}
		}

		private bool TryDequeue(out Task task)
		{
			lock (tasks)
			{
				if (tasks.Count > 0)
				{
					task = tasks.Dequeue();
					return true;
				}
			}
			task = default(Task);
			return false;
		}

		public void Stop()
		{
		}
	}
}
