using System.Collections;

public class DynamicPriorityQueue<T> : IQueue<T> where T : IPriorityQueueItem
{
	private sealed class PriorityComparer : IHeapItemComparer<T>
	{
		public bool Dominates(T a, T b)
		{
			return Dominates(a.GetPriority(), b.GetPriority());
		}

		public bool Dominates(float aPriority, float bPriority)
		{
			return aPriority < bPriority;
		}
	}

	public sealed class UpdateHeapCoroutine : StateMachineBase<DynamicPriorityQueue<T>>
	{
		private int curIndex = int.MaxValue;

		public override bool MoveNext()
		{
			if (curIndex >= host.Count)
			{
				curIndex = host.Count - 1;
			}
			if (curIndex > 0)
			{
				host.UpdateEntry(curIndex);
				curIndex--;
				return true;
			}
			return false;
		}

		public override void Reset()
		{
			curIndex = int.MaxValue;
		}
	}

	private static readonly PriorityComparer comparer = new PriorityComparer();

	private readonly DynamicHeap<T> heap = new DynamicHeap<T>(comparer);

	private static readonly StateMachinePool<UpdateHeapCoroutine, DynamicPriorityQueue<T>> updateHeapCoroutinePool = new StateMachinePool<UpdateHeapCoroutine, DynamicPriorityQueue<T>>();

	public int Count => heap.Count;

	public int LastIndex => Count - 1;

	public void Enqueue(T item)
	{
		heap.Enqueue(item);
	}

	public T Dequeue()
	{
		return heap.Dequeue();
	}

	public void Clear()
	{
		heap.Clear();
	}

	public void UpdateEntry(int index)
	{
		T entry = heap.GetEntry(index);
		float priority = entry.GetPriority();
		float bPriority = entry.UpdatePriority();
		if (comparer.Dominates(priority, bPriority))
		{
			heap.BubbleDown(index);
		}
		else
		{
			heap.BubbleUp(index);
		}
	}

	public void UpdateAllPriorities()
	{
		for (int i = 0; i < heap.Count; i++)
		{
			heap.GetEntry(i).UpdatePriority();
		}
		for (int j = 1; j < heap.Count; j++)
		{
			heap.BubbleUp(j);
		}
	}

	public IEnumerator UpdateHeap()
	{
		return updateHeapCoroutinePool.Get(this);
	}
}
