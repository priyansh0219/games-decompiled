using System.Collections.Generic;

public class SimpleQueue<T> : IQueue<T>
{
	private readonly Queue<T> queue = new Queue<T>();

	public int Count => queue.Count;

	public void Enqueue(T item)
	{
		queue.Enqueue(item);
	}

	public T Dequeue()
	{
		return queue.Dequeue();
	}

	public void Clear()
	{
		queue.Clear();
	}
}
