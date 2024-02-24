public interface IConcurrentCollection<T>
{
	bool TryAdd(T item);

	bool TryTake(out T item);
}
