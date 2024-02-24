using UnityEngine;

namespace UWE
{
	public class PartitionedArrayAllocator<T> : IAllocator<T>, IEstimateBytes
	{
		public struct Params
		{
			public int elementSize;

			public int minBucketSize;

			public int maxBucketSize;

			public int pageSize;

			public int initialPageCount;

			public int allocPoolInitialSize;

			public Params(int elementSize, int minBucketSize, int maxBucketSize, int pageSize, int initialPageCount, int allocPoolInitialSize)
			{
				this = default(Params);
				this.elementSize = elementSize;
				this.minBucketSize = minBucketSize;
				this.maxBucketSize = maxBucketSize;
				this.pageSize = pageSize;
				this.initialPageCount = initialPageCount;
				this.allocPoolInitialSize = allocPoolInitialSize;
			}
		}

		private readonly ArrayAllocator<T> largeBlock;

		private readonly ArrayAllocator<T> smallBlock;

		public PartitionedArrayAllocator(ArrayAllocator<T> largeBlockAllocator, ArrayAllocator<T> smallBlockAllocator)
		{
			largeBlock = largeBlockAllocator;
			smallBlock = smallBlockAllocator;
		}

		public PartitionedArrayAllocator(in Params largeBlockParams, in Params smallBlockParams)
		{
			largeBlock = new ArrayAllocator<T>(largeBlockParams.elementSize, largeBlockParams.minBucketSize, largeBlockParams.maxBucketSize, largeBlockParams.pageSize, largeBlockParams.initialPageCount, largeBlockParams.allocPoolInitialSize, coalesceAllocs: false);
			smallBlock = new ArrayAllocator<T>(smallBlockParams.elementSize, smallBlockParams.minBucketSize, smallBlockParams.maxBucketSize, smallBlockParams.pageSize, smallBlockParams.initialPageCount, smallBlockParams.allocPoolInitialSize, coalesceAllocs: false);
		}

		public IAlloc<T> Allocate(int size)
		{
			if (size <= smallBlock.MaxBucketSize)
			{
				lock (smallBlock)
				{
					return smallBlock.Allocate(size);
				}
			}
			if (size <= largeBlock.MaxBucketSize)
			{
				lock (largeBlock)
				{
					return largeBlock.Allocate(size);
				}
			}
			Debug.LogWarningFormat("Allocating {0} bytes. Too big for PartitionedArrayAllocator.", size);
			return new ManagedAlloc<T>(size);
		}

		public void Free(IAlloc<T> ialloc)
		{
			if (!(ialloc is ArrayAllocator<T>.Alloc alloc))
			{
				return;
			}
			if (alloc.Length <= smallBlock.MaxBucketSize)
			{
				lock (smallBlock)
				{
					smallBlock.Free(alloc);
					return;
				}
			}
			lock (largeBlock)
			{
				largeBlock.Free(alloc);
			}
		}

		public void Reset()
		{
			lock (largeBlock)
			{
				largeBlock.Reset();
			}
			lock (smallBlock)
			{
				smallBlock.Reset();
			}
		}

		public long EstimateBytes()
		{
			long num = 0L;
			lock (largeBlock)
			{
				num += largeBlock.EstimateBytes();
			}
			lock (smallBlock)
			{
				return num + smallBlock.EstimateBytes();
			}
		}
	}
}
