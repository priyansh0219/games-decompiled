namespace UWE
{
	public interface ILRUQueue<T>
	{
		int Count { get; }

		T Peek();

		T Pop();

		void PushBack(T keyElement);

		void RemoveElement(T element);

		void SwapElements(T a, T b);

		void Clear();
	}
}
