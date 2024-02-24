using System.Collections.Generic;
using System.Threading;

namespace UWE
{
	public sealed class BoundedObjectPool<T> where T : class, new()
	{
		private readonly T[] items;

		private readonly T[] allItems;

		private int length;

		public BoundedObjectPool(int length)
		{
			this.length = length;
			items = new T[length];
			allItems = new T[length];
			for (int i = 0; i < length; i++)
			{
				items[i] = new T();
				allItems[i] = items[i];
			}
		}

		public T Get()
		{
			lock (items)
			{
				while (length < 1)
				{
					Monitor.Wait(items);
				}
				return items[--length];
			}
		}

		public void Return(T item)
		{
			lock (items)
			{
				if (length < items.Length)
				{
					items[length++] = item;
					Monitor.Pulse(items);
				}
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)allItems).GetEnumerator();
		}
	}
}
