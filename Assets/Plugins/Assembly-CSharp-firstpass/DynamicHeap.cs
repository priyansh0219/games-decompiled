using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DynamicHeap<T> : IQueue<T>
{
	private readonly List<T> heap = new List<T>();

	private IHeapItemComparer<T> comparer;

	public int Count => heap.Count;

	public int LastIndex => Count - 1;

	public DynamicHeap(IHeapItemComparer<T> comparer)
	{
		this.comparer = comparer;
	}

	public void Enqueue(ICollection<T> items)
	{
		heap.AddRange(items);
		for (int i = 1; i < Count; i++)
		{
			BubbleUp(i);
		}
	}

	public void Enqueue(T item)
	{
		try
		{
			heap.Add(item);
			BubbleUp(LastIndex);
		}
		finally
		{
		}
	}

	public T Dequeue()
	{
		try
		{
			T result = heap[0];
			Swap(LastIndex, 0);
			heap.RemoveAt(LastIndex);
			BubbleDown(0);
			return result;
		}
		finally
		{
		}
	}

	public bool IsValid(int i)
	{
		if (i > 0)
		{
			return i < Count;
		}
		return false;
	}

	public void BubbleUp(int i)
	{
		if (i != 0)
		{
			int num = Parent(i);
			if (comparer.Dominates(heap[i], heap[num]))
			{
				Swap(i, num);
				BubbleUp(num);
			}
		}
	}

	public void BubbleDown(int i)
	{
		int dominating = GetDominating(i);
		if (dominating != i)
		{
			Swap(i, dominating);
			BubbleDown(dominating);
		}
	}

	public T GetEntry(int i)
	{
		return heap[i];
	}

	private int GetDominating(int i)
	{
		int oldNode = i;
		oldNode = GetDominating(ChildA(i), oldNode);
		return GetDominating(ChildB(i), oldNode);
	}

	private int GetDominating(int newNode, int oldNode)
	{
		if (IsValid(newNode) && comparer.Dominates(heap[newNode], heap[oldNode]))
		{
			return newNode;
		}
		return oldNode;
	}

	private void Swap(int i, int j)
	{
		T value = heap[i];
		heap[i] = heap[j];
		heap[j] = value;
	}

	private static int Parent(int i)
	{
		return (i + 1) / 2 - 1;
	}

	private static int ChildA(int i)
	{
		return (i + 1) * 2 - 1;
	}

	private static int ChildB(int i)
	{
		return ChildA(i) + 1;
	}

	public void Clear()
	{
		heap.Clear();
	}

	public void DebugPrint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			int num = 0;
			int num2 = 0;
			while (true)
			{
				for (int i = 0; (double)i < Math.Pow(2.0, num); i++)
				{
					stringBuilder.AppendFormat("{0} ", heap[num2++]);
					if (num2 >= Count)
					{
						return;
					}
				}
				stringBuilder.AppendLine();
				num++;
			}
		}
		finally
		{
			Debug.Log(stringBuilder.ToString());
		}
	}
}
