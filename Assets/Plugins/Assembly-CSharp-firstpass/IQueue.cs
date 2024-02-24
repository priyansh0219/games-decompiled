public interface IQueue<T>
{
	int Count { get; }

	void Enqueue(T item);

	T Dequeue();

	void Clear();
}
