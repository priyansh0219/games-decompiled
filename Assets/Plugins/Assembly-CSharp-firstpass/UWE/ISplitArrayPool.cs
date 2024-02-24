namespace UWE
{
	public interface ISplitArrayPool<T> : ISplitArrayPoolBase
	{
		IArrayPool<T> poolSmall { get; }

		IArrayPool<T> poolBig { get; }

		T[] Get(int minLength);

		void Return(T[] arr);
	}
}
