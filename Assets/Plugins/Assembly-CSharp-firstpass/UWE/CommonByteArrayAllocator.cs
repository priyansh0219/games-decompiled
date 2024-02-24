using UnityEngine;

namespace UWE
{
	public static class CommonByteArrayAllocator
	{
		public static readonly ArrayAllocator<byte> largeBlock = new ArrayAllocator<byte>(1, 32, 1048576, 4194304, 4, 20000, coalesceAllocs: false);

		public static readonly ArrayAllocator<byte> smallBlock = new ArrayAllocator<byte>(1, 4, 16, 65536, 10, 20000, coalesceAllocs: false);

		public static IAlloc<byte> Allocate(int length)
		{
			if (length <= smallBlock.MaxBucketSize)
			{
				lock (smallBlock)
				{
					return smallBlock.Allocate(length);
				}
			}
			if (length <= largeBlock.MaxBucketSize)
			{
				lock (largeBlock)
				{
					return largeBlock.Allocate(length);
				}
			}
			Debug.LogWarningFormat("Allocating {0} bytes. Too big for CommonByteArrayAllocator.", length);
			return new ManagedAlloc<byte>(length);
		}

		public static void Free(IAlloc<byte> ialloc)
		{
			if (!(ialloc is ArrayAllocator<byte>.Alloc alloc))
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

		public static long EstimateBytes()
		{
			return largeBlock.EstimateBytes() + smallBlock.EstimateBytes();
		}
	}
}
