namespace UWE
{
	public class SplitArrayPool<T> : ISplitArrayPool<T>, ISplitArrayPoolBase where T : new()
	{
		private readonly int splitThreshold;

		public IArrayPool<T> poolSmall { get; private set; }

		public IArrayPool<T> poolBig { get; private set; }

		public int NumArraysAllocated => poolSmall.NumArraysAllocated + poolBig.NumArraysAllocated;

		public int NumArraysOutstanding => poolSmall.numArraysOutstanding + poolBig.numArraysOutstanding;

		public int PoolHits => poolSmall.poolHits + poolBig.poolHits;

		public int PoolMisses => poolSmall.poolMisses + poolBig.poolMisses;

		public SplitArrayPool(int elementSize, int splitThreshold, int smallArrayPoolBucketSize, int smallArrayBucketsToSearch, int smallArrayPoolMaxSize, int bigArrayPoolBucketSize, int bigArrayBucketsToSearch, int bigArrayPoolMaxSize)
		{
			poolSmall = new ArrayPool<T>(elementSize, smallArrayPoolBucketSize, smallArrayBucketsToSearch, smallArrayPoolMaxSize);
			poolBig = new ArrayPool<T>(elementSize, bigArrayPoolBucketSize, bigArrayBucketsToSearch, bigArrayPoolMaxSize);
			this.splitThreshold = splitThreshold;
		}

		public void Reset()
		{
			poolSmall.Reset();
			poolBig.Reset();
		}

		public void ResetCacheStats()
		{
			poolSmall.ResetCacheStats();
			poolBig.ResetCacheStats();
		}

		public T[] Get(int minLength)
		{
			return ((poolSmall.GetArraySize(minLength) < splitThreshold) ? poolSmall : poolBig).Get(minLength);
		}

		public void Return(T[] arr)
		{
			((arr.Length < splitThreshold) ? poolSmall : poolBig).Return(arr);
		}

		public long EstimateBytes()
		{
			return poolSmall.EstimateBytes() + poolBig.EstimateBytes();
		}

		public void PrintDebugStats()
		{
			poolSmall.PrintDebugStats();
			poolBig.PrintDebugStats();
		}
	}
}
