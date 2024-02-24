using System.Collections.Generic;
using System.Threading;

namespace UWE
{
	public class BlockingQueue<T>
	{
		private Queue<T> queue = new Queue<T>();

		public void Add(T item)
		{
			lock (queue)
			{
				queue.Enqueue(item);
				Monitor.Pulse(queue);
			}
		}

		public T Take()
		{
			lock (queue)
			{
				while (queue.Count < 1)
				{
					Monitor.Wait(queue);
				}
				return queue.Dequeue();
			}
		}

		public void Clear()
		{
			lock (queue)
			{
				queue.Clear();
			}
		}
	}
}
