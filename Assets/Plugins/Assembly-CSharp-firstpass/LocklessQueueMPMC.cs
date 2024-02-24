using System;
using System.Threading;
using UnityEngine;

public class LocklessQueueMPMC<T>
{
	private readonly T[] queue;

	private int writeIndex;

	private int readIndex;

	private int maximumReadIndex;

	private int count;

	private int commitCount;

	private object objectLock = new object();

	public int ReadIndex => readIndex;

	public int WriteIndex => writeIndex;

	public int Count => count;

	public bool SuppressResizeMessage { get; set; }

	public int MaxCount => queue.Length;

	public LocklessQueueMPMC(int MAX_QUEUE_ITEMS = 128)
	{
		queue = new T[MAX_QUEUE_ITEMS];
	}

	public bool Push(T item)
	{
		int num = 0;
		int num2 = 0;
		do
		{
			num = writeIndex;
			num2 = readIndex;
			if (num == writeIndex && CountToIndex(num + 1) == CountToIndex(num2))
			{
				if (!SuppressResizeMessage)
				{
					Debug.LogWarningFormat("Queue ran out of space. Consider resizing. Current Size = {0}", queue.Length);
					SuppressResizeMessage = true;
				}
				return false;
			}
		}
		while (Interlocked.CompareExchange(ref writeIndex, num + 1, num) != num);
		queue[CountToIndex(num)] = item;
		int num3 = Interlocked.Increment(ref commitCount);
		if (num3 == writeIndex)
		{
			maximumReadIndex = num3;
		}
		Interlocked.Increment(ref count);
		return true;
	}

	public bool Pop(out T outItem)
	{
		int num = 0;
		int num2 = 0;
		while (true)
		{
			num = readIndex;
			num2 = CountToIndex(num);
			if (num == readIndex)
			{
				if (num2 == CountToIndex(maximumReadIndex))
				{
					outItem = default(T);
					return false;
				}
				outItem = queue[num2];
				if (Interlocked.CompareExchange(ref readIndex, num + 1, num) == num)
				{
					break;
				}
			}
		}
		Interlocked.Decrement(ref count);
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
		Interlocked.Exchange(ref maximumReadIndex, 0);
		Interlocked.Exchange(ref commitCount, 0);
		Interlocked.Exchange(ref count, 0);
	}

	public T[] GetAllItems()
	{
		lock (objectLock)
		{
			T[] array = new T[Count];
			if (Count > 0)
			{
				int num = CountToIndex(readIndex);
				int num2 = CountToIndex(maximumReadIndex);
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
			}
			return array;
		}
	}
}
