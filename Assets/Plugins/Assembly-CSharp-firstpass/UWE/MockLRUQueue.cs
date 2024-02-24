namespace UWE
{
	public class MockLRUQueue<T> : ILRUQueue<T>
	{
		public int Count => 0;

		public void PushBack(T keyElement)
		{
		}

		public void RemoveElement(T element)
		{
		}

		public T Peek()
		{
			return default(T);
		}

		public T Pop()
		{
			return default(T);
		}

		public void SwapElements(T a, T b)
		{
		}

		public void Clear()
		{
		}
	}
}
