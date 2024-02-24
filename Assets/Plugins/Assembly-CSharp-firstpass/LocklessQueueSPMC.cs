using System;
using System.Threading;
using UnityEngine;

public class LocklessQueueSPMC<T>
{
	private readonly T[] queue;

	private int writeIndex;

	private int readIndex;

	private int count;

	private AutoResetEvent QueueEvent = new AutoResetEvent(initialState: false);

	private object objectLock = new object();

	public int ReadIndex => readIndex;

	public int WriteIndex => writeIndex;

	public int Count => count;

	public int MaxCount => queue.Length;

	public bool SuppressResizeMessage { get; set; }

	public LocklessQueueSPMC(uint MAX_QUEUE_SIZE = 128u)
	{
		queue = new T[MAX_QUEUE_SIZE];
	}

	public bool Push(T item)
	{
		int num = writeIndex;
		if (CountToIndex(num + 1) == CountToIndex(readIndex))
		{
			if (!SuppressResizeMessage)
			{
				Debug.LogWarningFormat("Queue ran out of space. Consider resizing. Current Size = {0}", queue.Length);
				SuppressResizeMessage = true;
			}
			return false;
		}
		queue[CountToIndex(num)] = item;
		Interlocked.Increment(ref writeIndex);
		Interlocked.Increment(ref count);
		return true;
	}

	public bool Pop(out T outItem)
	{
		int num = 0;
		int num2 = 0;
		do
		{
			num = readIndex;
			num2 = CountToIndex(num);
			if (num2 == CountToIndex(writeIndex))
			{
				outItem = default(T);
				return false;
			}
			outItem = queue[num2];
		}
		while (Interlocked.CompareExchange(ref readIndex, num + 1, num) != num);
		Interlocked.Decrement(ref count);
		QueueEvent.Set();
		return true;
	}

	private int CountToIndex(int count)
	{
		return count % queue.Length;
	}

	public bool IsEmpty()
	{
		return Count == 0;
	}

	public void Clear()
	{
		Interlocked.Exchange(ref writeIndex, 0);
		Interlocked.Exchange(ref readIndex, 0);
		Interlocked.Exchange(ref count, 0);
	}

	public T[] GetAllItems()
	{
		lock (objectLock)
		{
			T[] array = new T[Count];
			int num = CountToIndex(readIndex);
			int num2 = CountToIndex(writeIndex);
			if (num2 > num)
			{
				Array.Copy(queue, num, array, 0, num2 - num);
			}
			else
			{
				int num3 = MaxCount - num;
				Array.Copy(queue, num, array, 0, num3);
				Array.Copy(queue, 0, array, num3, num2);
			}
			return array;
		}
	}
}
