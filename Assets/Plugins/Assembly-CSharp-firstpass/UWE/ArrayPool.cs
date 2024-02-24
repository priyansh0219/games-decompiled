using UnityEngine;

namespace UWE
{
	public class ArrayPool<T> : ArrayPoolBase<T>, IArrayPool<T>, IArrayPoolBase<T>
	{
		private readonly ILRUQueue<T[]> leastRecentlyUsed;

		private readonly T[] emptyArray = new T[0];

		private readonly ILRUQueue<T[]> mockLRUQueue = new MockLRUQueue<T[]>();

		public ArrayPool(int elementSize, int bucketStride, int numBucketsToCheck = 0, int desiredMemoryCap = 0)
			: base(elementSize, bucketStride, numBucketsToCheck, desiredMemoryCap)
		{
			ILRUQueue<T[]> iLRUQueue;
			if (desiredMemoryCap <= 0)
			{
				iLRUQueue = mockLRUQueue;
			}
			else
			{
				ILRUQueue<T[]> iLRUQueue2 = new LRUQueue<T[]>();
				iLRUQueue = iLRUQueue2;
			}
			leastRecentlyUsed = iLRUQueue;
		}

		public T[] Get(int minLength)
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
					if ((buckets[i] as ArrayPoolBucket<T>).TryGet(out var result))
					{
						base.poolHits++;
						leastRecentlyUsed.RemoveElement(result);
						return result;
					}
				}
				base.poolMisses++;
				return (buckets[num] as ArrayPoolBucket<T>).AllocateWasteArray(minLength);
			}
		}

		public void Return(T[] arr)
		{
			if (arr == emptyArray || arr.Length == 0)
			{
				return;
			}
			lock (bucketsLock)
			{
				int num = BucketForLength(arr.Length);
				LockedEnsureSize(num + 1);
				(buckets[num] as ArrayPoolBucket<T>).Return(arr);
				leastRecentlyUsed.PushBack(arr);
				if (base.desiredMemoryCap > 0 && base.totalBytesAllocated > base.desiredMemoryCap)
				{
					LockedTryCleanPool();
				}
			}
		}

		protected override void LockedAddBucket(int arraySize)
		{
			buckets.Add(new ArrayPoolBucket<T>(arraySize));
		}

		protected override void LockedWarmupElement(int bucketIndex, int count)
		{
			LockedEnsureSize(bucketIndex + 1);
			ArrayPoolBucket<T> arrayPoolBucket = buckets[bucketIndex] as ArrayPoolBucket<T>;
			SizeForBucket(bucketIndex);
			for (int i = 0; i < count; i++)
			{
				T[] keyElement = arrayPoolBucket.AddArray();
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
				T[] array = leastRecentlyUsed.Peek();
				int index = BucketForLength(array.Length);
				T[] b = (buckets[index] as ArrayPoolBucket<T>).RemoveArray();
				leastRecentlyUsed.SwapElements(array, b);
				leastRecentlyUsed.Pop();
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
