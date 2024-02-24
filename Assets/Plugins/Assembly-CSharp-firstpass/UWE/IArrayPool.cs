namespace UWE
{
	public interface IArrayPool<T> : IArrayPoolBase<T>
	{
		T[] Get(int minLength);

		void Return(T[] arr);
	}
}
