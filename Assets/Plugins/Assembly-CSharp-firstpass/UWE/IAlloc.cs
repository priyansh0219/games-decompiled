namespace UWE
{
	public interface IAlloc<T>
	{
		IAllocator<T> Heap { get; }

		int Offset { get; }

		int Length { get; }

		T[] Array { get; }

		T this[int Index] { get; set; }
	}
}
