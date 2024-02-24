namespace UWE
{
	public interface IArrayPoolBucketBase<T>
	{
		int numArraysPooled { get; }

		int numArraysOutstanding { get; }

		int peakArraysOutstanding { get; }

		long totalBytesAllocated { get; }

		long totalBytesWasted { get; }

		int arraySize { get; }

		void Clear();
	}
}
