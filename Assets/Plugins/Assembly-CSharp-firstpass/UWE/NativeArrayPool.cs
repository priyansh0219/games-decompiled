using Unity.Collections;
using UnityEngine;

namespace UWE
{
	public class NativeArrayPool<T> : ArrayPoolBase<T>, INativeArrayPool<T>, IArrayPoolBase<T> where T : struct
	{
		public static readonly NativeArray<T> emptyArray;

		private readonly ILRUQueue<NativeArray<T>> leastRecentlyUsed;

		private readonly ILRUQueue<NativeArray<T>> mockLRUQueue = new MockLRUQueue<NativeArray<T>>();

		public NativeArrayPool(int elementSize, int bucketStride, int numBucketsToCheck = 0, int desiredMemoryCap = 0)
			: base(elementSize, bucketStride, numBucketsToCheck, desiredMemoryCap)
		{
			ILRUQueue<NativeArray<T>> iLRUQueue;
			if (desiredMemoryCap <= 0)
			{
				iLRUQueue = mockLRUQueue;
			}
			else
			{
				ILRUQueue<NativeArray<T>> iLRUQueue2 = new LRUQueue<NativeArray<T>>();
				iLRUQueue = iLRUQueue2;
			}
			leastRecentlyUsed = iLRUQueue;
		}

		public NativeArray<T> Get(int minLength)
		{
			if (minLength <= 0)
			{
				return emptyArray;
			}
			lock (bucketsLock)
			{
				int num = BucketForLength(minLength);
				LockedEnsureSize(num + 1);
				int num2 = Mathf.Min(num + base.numBucketsToCheck, buckets.Count - 1);
				for (int i = num; i <= num2; i++)
				{
					if ((buckets[i] as NativeArrayPoolBucket<T>).TryGet(out var result))
					{
						base.poolHits++;
						leastRecentlyUsed.RemoveElement(result);
						return result;
					}
				}
				base.poolMisses++;
				return (buckets[num] as NativeArrayPoolBucket<T>).AllocateWasteArray(minLength);
			}
		}

		public void Return(NativeArray<T> arr)
		{
			if (arr == emptyArray || arr.Length == 0)
			{
				return;
			}
			lock (bucketsLock)
			{
				int num = BucketForLength(arr.Length);
				LockedEnsureSize(num + 1);
				(buckets[num] as NativeArrayPoolBucket<T>).Return(arr);
				leastRecentlyUsed.PushBack(arr);
				if (base.desiredMemoryCap > 0 && base.totalBytesAllocated > base.desiredMemoryCap)
				{
					LockedTryCleanPool();
				}
			}
		}

		protected override void LockedAddBucket(int arraySize)
		{
			buckets.Add(new NativeArrayPoolBucket<T>(arraySize));
		}

		protected override void LockedWarmupElement(int bucketIndex, int count)
		{
			LockedEnsureSize(bucketIndex + 1);
			NativeArrayPoolBucket<T> nativeArrayPoolBucket = buckets[bucketIndex] as NativeArrayPoolBucket<T>;
			SizeForBucket(bucketIndex);
			for (int i = 0; i < count; i++)
			{
				NativeArray<T> keyElement = nativeArrayPoolBucket.AddArray();
				leastRecentlyUsed.PushBack(keyElement);
			}
		}

		protected override bool LockedCanCleanPool()
		{
			return leastRecentlyUsed.Count > 0;
		}

		protected override void LockedTryCleanPool(long targetMemorySize)
		{
			while (leastRecentlyUsed.Count > 0 && base.totalBytesAllocated > targetMemorySize)
			{
				NativeArray<T> a = leastRecentlyUsed.Peek();
				int index = BucketForLength(a.Length);
				NativeArray<T> b = (buckets[index] as NativeArrayPoolBucket<T>).RemoveArray();
				leastRecentlyUsed.SwapElements(a, b);
				leastRecentlyUsed.Pop();
				b.Dispose();
			}
		}

		public override void Reset()
		{
			lock (bucketsLock)
			{
				leastRecentlyUsed.Clear();
			}
			base.Reset();
		}
	}
}
