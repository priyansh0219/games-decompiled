using System;

namespace UWE
{
	public class ObjectPool<T> where T : class
	{
		private readonly string name;

		private readonly Func<T> allocate;

		private readonly T[] items;

		private int count;

		private int highWaterMark;

		public int numOutstanding { get; private set; }

		public int totalAllocated { get; private set; }

		public ObjectPool(string name, int capacity, Func<T> allocate)
		{
			this.name = name;
			this.allocate = allocate;
			items = new T[capacity];
		}

		public T Get()
		{
			lock (items)
			{
				numOutstanding++;
				if (count > 0)
				{
					T val = items[--count];
					if (val != null)
					{
						return val;
					}
				}
				totalAllocated++;
			}
			return allocate();
		}

		public void Return(T obj)
		{
			lock (items)
			{
				if (count < items.Length)
				{
					items[count++] = obj;
					if (count > highWaterMark)
					{
						highWaterMark = count;
					}
				}
				numOutstanding--;
			}
		}

		public int Size()
		{
			return count;
		}

		public void Reset()
		{
			lock (items)
			{
				Array.Clear(items, 0, items.Length);
				count = 0;
				highWaterMark = 0;
				totalAllocated = 0;
				numOutstanding = 0;
			}
		}

		public int GetHighWaterMark()
		{
			return highWaterMark;
		}
	}
}
