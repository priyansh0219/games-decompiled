using System.Collections.Generic;
using PriorityQueueInternal;

public sealed class PriorityQueue<T> where T : class
{
	private readonly Heap<Item> heap;

	public int Count => heap.Count;

	public PriorityQueue()
	{
		heap = new Heap<Item>(Comparer.comparer);
	}

	public PriorityQueue(int capacity)
	{
		heap = new Heap<Item>(Comparer.comparer, capacity);
	}

	public void Enqueue(T item, int priority)
	{
		heap.Enqueue(new Item(item, priority));
	}

	public bool TryDequeue(out T result)
	{
		if (heap.Count <= 0)
		{
			result = null;
			return false;
		}
		result = (T)heap.Dequeue().item;
		return true;
	}

	public void Clear()
	{
		heap.Clear();
	}

	public IEnumerator<Item> GetEnumerator()
	{
		return heap.GetEnumerator();
	}
}
