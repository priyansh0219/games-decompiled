using Unity.Collections;

namespace UWE
{
	public class SplitNativeArrayPool<T> : ISplitNativeArrayPool<T>, ISplitArrayPoolBase where T : struct
	{
		private readonly int splitThreshold;

		public static NativeArray<T> emptyArray => NativeArrayPool<T>.emptyArray;

		public INativeArrayPool<T> poolSmall { get; private set; }

		public INativeArrayPool<T> poolBig { get; private set; }

		public int NumArraysAllocated => poolSmall.NumArraysAllocated + poolBig.NumArraysAllocated;

		public int NumArraysOutstanding => poolSmall.numArraysOutstanding + poolBig.numArraysOutstanding;

		public int PoolHits => poolSmall.poolHits + poolBig.poolHits;

		public int PoolMisses => poolSmall.poolMisses + poolBig.poolMisses;

		public SplitNativeArrayPool(int elementSize, int splitThreshold, int smallArrayPoolBucketSize, int smallArrayBucketsToSearch, int smallArrayPoolMaxSize, int bigArrayPoolBucketSize, int bigArrayBucketsToSearch, int bigArrayPoolMaxSize)
		{
			poolSmall = new NativeArrayPool<T>(elementSize, smallArrayPoolBucketSize, smallArrayBucketsToSearch, smallArrayPoolMaxSize);
			poolBig = new NativeArrayPool<T>(elementSize, bigArrayPoolBucketSize, bigArrayBucketsToSearch, bigArrayPoolMaxSize);
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

		public NativeArray<T> Get(int minLength)
		{
			return ((poolSmall.GetArraySize(minLength) < splitThreshold) ? poolSmall : poolBig).Get(minLength);
		}

		public void Return(NativeArray<T> arr)
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
