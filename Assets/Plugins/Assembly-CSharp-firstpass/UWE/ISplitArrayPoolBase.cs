namespace UWE
{
	public interface ISplitArrayPoolBase
	{
		int NumArraysAllocated { get; }

		int NumArraysOutstanding { get; }

		int PoolHits { get; }

		int PoolMisses { get; }

		void Reset();

		void ResetCacheStats();

		long EstimateBytes();

		void PrintDebugStats();
	}
}
