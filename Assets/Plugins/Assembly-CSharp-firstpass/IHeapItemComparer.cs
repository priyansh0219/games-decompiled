public interface IHeapItemComparer<T>
{
	bool Dominates(T a, T b);
}
