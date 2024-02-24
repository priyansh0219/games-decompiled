using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class Heap<T>
{
	private readonly IComparer<T> comparer;

	private readonly List<T> heap;

	public int Count => heap.Count;

	public int LastIndex => Count - 1;

	public int Capacity
	{
		get
		{
			return heap.Capacity;
		}
		set
		{
			heap.Capacity = value;
		}
	}

	public Heap(IComparer<T> comparer)
	{
		this.comparer = comparer;
		heap = new List<T>();
	}

	public Heap(IComparer<T> comparer, int capacity)
	{
		this.comparer = comparer;
		heap = new List<T>(capacity);
	}

	public void Enqueue(ICollection<T> items)
	{
		int count = Count;
		heap.AddRange(items);
		for (int i = count; i < Count; i++)
		{
			BubbleUp(i);
		}
	}

	public void Enqueue(T item)
	{
		heap.Add(item);
		BubbleUp(LastIndex);
	}

	public T Dequeue()
	{
		T result = heap[0];
		Swap(LastIndex, 0);
		heap.RemoveAt(LastIndex);
		BubbleDown(0);
		return result;
	}

	public T Peek()
	{
		return heap[0];
	}

	private void BubbleUp(int i)
	{
		if (i != 0)
		{
			int num = Parent(i);
			if (comparer.Compare(heap[i], heap[num]) < 0)
			{
				Swap(i, num);
				BubbleUp(num);
			}
		}
	}

	private void BubbleDown(int i)
	{
		int num = FirstChild(i);
		int num2 = num + 1;
		if (num < Count)
		{
			int num3 = ((num2 >= Count || comparer.Compare(heap[num], heap[num2]) < 0) ? num : num2);
			if (comparer.Compare(heap[num3], heap[i]) < 0)
			{
				Swap(i, num3);
				BubbleDown(num3);
			}
		}
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

	private static int FirstChild(int i)
	{
		return (i + 1) * 2 - 1;
	}

	public void Clear()
	{
		heap.Clear();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return heap.GetEnumerator();
	}

	public void DebugPrint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (T item in heap)
		{
			stringBuilder.Append(item);
			stringBuilder.Append(", ");
		}
		Debug.Log(stringBuilder.ToString());
	}
}
