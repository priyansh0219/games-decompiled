using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UWE
{
	public abstract class ArrayPoolBase<T> : IArrayPoolBase<T>
	{
		protected readonly List<IArrayPoolBucketBase<T>> buckets = new List<IArrayPoolBucketBase<T>>();

		protected readonly object bucketsLock = new object();

		public int numArraysPooled
		{
			get
			{
				lock (bucketsLock)
				{
					int num = 0;
					foreach (IArrayPoolBucketBase<T> bucket in buckets)
					{
						num += bucket.numArraysPooled;
					}
					return num;
				}
			}
		}

		public int numArraysOutstanding
		{
			get
			{
				lock (bucketsLock)
				{
					int num = 0;
					foreach (IArrayPoolBucketBase<T> bucket in buckets)
					{
						num += bucket.numArraysOutstanding;
					}
					return num;
				}
			}
		}

		public long totalBytesAllocated
		{
			get
			{
				lock (bucketsLock)
				{
					long num = 0L;
					foreach (IArrayPoolBucketBase<T> bucket in buckets)
					{
						num += bucket.totalBytesAllocated;
					}
					return num;
				}
			}
		}

		public int numBuckets
		{
			get
			{
				lock (bucketsLock)
				{
					return buckets.Count;
				}
			}
		}

		public int NumArraysAllocated => numArraysOutstanding + numArraysPooled;

		public int elementSize { get; protected set; }

		public int bucketStride { get; protected set; }

		public int poolHits { get; protected set; }

		public int poolMisses { get; protected set; }

		public int numBucketsToCheck { get; protected set; }

		public int desiredMemoryCap { get; protected set; }

		public ArrayPoolBase(int elementSize, int bucketStride, int numBucketsToCheck = 0, int desiredMemoryCap = 0)
		{
			this.elementSize = elementSize;
			this.bucketStride = bucketStride;
			this.numBucketsToCheck = numBucketsToCheck;
			this.desiredMemoryCap = desiredMemoryCap;
		}

		protected int BucketForLength(int len)
		{
			return (len - 1) / bucketStride;
		}

		protected int SizeForBucket(int bucketIndex)
		{
			return (bucketIndex + 1) * bucketStride;
		}

		protected void LockedEnsureSize(int size)
		{
			while (buckets.Count < size)
			{
				int count = buckets.Count;
				int arraySize = SizeForBucket(count);
				LockedAddBucket(arraySize);
			}
		}

		protected void LockedTryCleanPool()
		{
			if (LockedCanCleanPool())
			{
				float num = 0.8f;
				long targetMemorySize = (long)((float)desiredMemoryCap * num);
				LockedTryCleanPool(targetMemorySize);
			}
		}

		protected abstract void LockedAddBucket(int arraySize);

		protected abstract void LockedWarmupElement(int bucketIndex, int count);

		protected abstract bool LockedCanCleanPool();

		protected abstract void LockedTryCleanPool(long targetMemorySize);

		public long EstimateBytes()
		{
			return totalBytesAllocated;
		}

		public int GetArraySize(int minLength)
		{
			int bucketIndex = BucketForLength(minLength);
			return SizeForBucket(bucketIndex);
		}

		public virtual void Reset()
		{
			lock (bucketsLock)
			{
				foreach (IArrayPoolBucketBase<T> bucket in buckets)
				{
					bucket.Clear();
				}
				buckets.Clear();
				poolHits = 0;
				poolMisses = 0;
			}
		}

		public void ResetCacheStats()
		{
			lock (bucketsLock)
			{
				poolHits = 0;
				poolMisses = 0;
			}
		}

		public void Warmup(AnimationCurve warmupCurve, int maxEntriesPerBucket, int numBuckets, int startBucket)
		{
			lock (bucketsLock)
			{
				for (int i = startBucket; i < numBuckets; i++)
				{
					float time = (float)i / (float)numBuckets;
					int count = (int)(warmupCurve.Evaluate(time) * (float)maxEntriesPerBucket);
					LockedWarmupElement(i, count);
				}
			}
		}

		public void WarmupElement(int bucketIndex, int count)
		{
			lock (bucketsLock)
			{
				LockedWarmupElement(bucketIndex, count);
			}
		}

		public void PrintDebugStats()
		{
			StringBuilder stringBuilder = new StringBuilder();
			lock (bucketsLock)
			{
				stringBuilder.AppendFormat("ArrayPool / Bucket Stride:  {0} / Total Allocated  {1} => {2} bytes / Desired Memory Cap {3} \n", bucketStride, numArraysPooled + numArraysOutstanding, totalBytesAllocated, desiredMemoryCap);
				stringBuilder.AppendFormat("Pool Hits {0} / Pool Misses {1}\n", poolHits, poolMisses);
				stringBuilder.AppendLine("Pool Index,Array Size(Bytes),In,Out,Count,Total Size,Waste,BytesIn,BytesOut");
				int num = 0;
				int num2 = 0;
				long num3 = 0L;
				long num4 = 0L;
				long num5 = 0L;
				int num6 = 0;
				int num7 = 0;
				for (int i = 0; i < buckets.Count; i++)
				{
					IArrayPoolBucketBase<T> arrayPoolBucketBase = buckets[i];
					int arraySize = arrayPoolBucketBase.arraySize;
					int num8 = arrayPoolBucketBase.numArraysPooled;
					int num9 = arrayPoolBucketBase.numArraysOutstanding;
					int num10 = num8 + num9;
					int num11 = arraySize * num10;
					long totalBytesWasted = arrayPoolBucketBase.totalBytesWasted;
					int num12 = num8 * arraySize;
					int num13 = num9 * arraySize;
					stringBuilder.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}\n", i, arraySize, num8, num9, num10, num11, totalBytesWasted, num12, num13);
					num3 += totalBytesWasted;
					num += num10;
					num2 += num11;
					num6 += num12;
					num7 += num13;
					num4 += num8;
					num5 += num9;
				}
				stringBuilder.AppendFormat(",,{0},{1},{2},{3},{4},{5},{6}\n", num4, num5, num, num2, num3, num6, num7);
			}
			Debug.Log(stringBuilder.ToString());
		}

		public void GetBucketInfo(ref int[] arraysPooled, ref int[] arraysOutstanding, ref int[] peakArraysOustanding, ref long[] bytesWasted)
		{
			lock (bucketsLock)
			{
				int count = buckets.Count;
				for (int i = 0; i < count; i++)
				{
					arraysPooled[i] = buckets[i].numArraysPooled;
					arraysOutstanding[i] = buckets[i].numArraysOutstanding;
					bytesWasted[i] = buckets[i].totalBytesWasted;
					peakArraysOustanding[i] = buckets[i].peakArraysOutstanding;
				}
			}
		}
	}
}
