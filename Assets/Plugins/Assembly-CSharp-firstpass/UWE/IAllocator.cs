namespace UWE
{
	public interface IAllocator<T>
	{
		IAlloc<T> Allocate(int size);

		void Free(IAlloc<T> a);

		void Reset();
	}
}
