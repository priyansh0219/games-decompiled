using System;
using UnityEngine;

namespace UWE
{
	public class ArrayAllocator<T> : IAllocator<T>, IEstimateBytes
	{
		public class Alloc : IAlloc<T>, IEstimateBytes
		{
			public IAllocator<T> Heap
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public int Offset { get; set; }

			public byte BucketIndex { get; set; }

			public int MaxSize => 1 << (int)BucketIndex;

			public int Length { get; set; }

			public Alloc Next { get; set; }

			public MemoryPage Page { get; set; }

			public T[] Array => Page.Array;

			public T this[int Index]
			{
				get
				{
					return Get(Index);
				}
				set
				{
					Set(Index, value);
				}
			}

			public void Set(int i, T value)
			{
				Array[Offset + i] = value;
			}

			public T Get(int i)
			{
				return Array[Offset + i];
			}

			public void Write(int startOffset, T[] values)
			{
				System.Array.Copy(values, 0, Array, startOffset + Offset, values.Length);
			}

			public static void CopyTo(IAlloc<T> source, IAlloc<T> destination, int numElements)
			{
				System.Array.Copy(source.Array, source.Offset, destination.Array, destination.Offset, numElements);
			}

			public long EstimateBytes()
			{
				return 33L;
			}
		}

		private class Bucket
		{
			public MemoryPage Page { get; private set; }

			public Alloc Root { get; protected set; }

			public int Count { get; protected set; }

			public byte Index { get; set; }

			public int Size => 1 << (int)Index;

			public int WasteElements { get; protected set; }

			public int InUse { get; protected set; }

			public int PeakInUse { get; protected set; }

			public int Free => Count;

			public Bucket(MemoryPage page, byte index)
			{
				Page = page;
				Index = index;
				Count = 0;
			}

			public void Push(Alloc alloc)
			{
				alloc.Length = -1;
				alloc.Next = Root;
				Root = alloc;
				int count = Count + 1;
				Count = count;
			}

			public Alloc Pop()
			{
				int count = Count - 1;
				Count = count;
				Alloc root = Root;
				Root = root.Next;
				root.Next = null;
				return root;
			}

			public void DebugAllocated(Alloc a)
			{
				int inUse = InUse + 1;
				InUse = inUse;
				WasteElements += a.MaxSize - a.Length;
				PeakInUse = Mathf.Max(PeakInUse, InUse);
			}

			public void DebugFreed(Alloc a)
			{
				int inUse = InUse - 1;
				InUse = inUse;
				WasteElements -= a.MaxSize - a.Length;
			}

			public void ForceReset()
			{
				Root = null;
				InUse = 0;
				WasteElements = 0;
				Count = 0;
				PeakInUse = 0;
			}
		}

		public class MemoryPage
		{
			private readonly Bucket[] buckets;

			public T[] Array { get; private set; }

			public int Offset { get; private set; }

			public int Remaining => Array.Length - Offset;

			public MemoryPage Next { get; set; }

			public ArrayAllocator<T> Allocator { get; private set; }

			public MemoryPage(ArrayAllocator<T> allocator, int arraySize, byte minBucketIndex, byte maxBucketIndex, bool coalesceAllocs)
			{
				Allocator = allocator;
				Array = new T[arraySize];
				buckets = new Bucket[maxBucketIndex + 1];
				for (byte b = minBucketIndex; b <= maxBucketIndex; b++)
				{
					buckets[b] = new Bucket(this, b);
					buckets[b].Index = b;
				}
			}

			public bool Allocate(int size, byte bucketIndex, ref Alloc alloc)
			{
				Bucket bucket = buckets[bucketIndex];
				if (bucket.Count > 0)
				{
					alloc = bucket.Pop();
					alloc.Length = size;
					bucket.DebugAllocated(alloc);
					return true;
				}
				for (int i = bucketIndex + 1; i < buckets.Length; i++)
				{
					if (buckets[i].Count > 0)
					{
						alloc = buckets[i].Pop();
						alloc.BucketIndex = bucketIndex;
						for (int num = i - 1; num >= bucketIndex; num--)
						{
							Alloc orCreateEmptyAlloc = Allocator.GetOrCreateEmptyAlloc();
							orCreateEmptyAlloc.Page = alloc.Page;
							orCreateEmptyAlloc.BucketIndex = (byte)num;
							orCreateEmptyAlloc.Offset = alloc.Offset + buckets[num].Size;
							buckets[num].Push(orCreateEmptyAlloc);
						}
						alloc.Length = size;
						bucket.DebugAllocated(alloc);
						return true;
					}
				}
				if (alloc == null && Remaining >= bucket.Size)
				{
					alloc = Allocator.GetOrCreateEmptyAlloc();
					alloc.Page = this;
					alloc.BucketIndex = bucketIndex;
					alloc.Offset = Offset;
					Offset += bucket.Size;
					alloc.Length = size;
					bucket.DebugAllocated(alloc);
					return true;
				}
				return false;
			}

			public void Free(Alloc allocation)
			{
				Bucket obj = buckets[allocation.BucketIndex];
				obj.DebugFreed(allocation);
				obj.Push(allocation);
			}

			public void ForceReset()
			{
				for (byte b = 0; b < buckets.Length; b++)
				{
					if (buckets[b] != null)
					{
						buckets[b].ForceReset();
					}
				}
				Offset = 0;
			}

			public void GetDebugInfo(int[] bucketFree, int[] bucketInUse, int[] bucketPeakInUse, long[] bucketWaste)
			{
				for (byte b = 0; b < buckets.Length; b++)
				{
					bucketFree[b] += ((buckets[b] != null) ? buckets[b].Free : 0);
					bucketInUse[b] += ((buckets[b] != null) ? buckets[b].InUse : 0);
					bucketPeakInUse[b] += ((buckets[b] != null) ? buckets[b].PeakInUse : 0);
					bucketWaste[b] += ((buckets[b] != null) ? buckets[b].WasteElements : 0);
				}
			}
		}

		private MemoryPage pageRoot;

		private Alloc emptyAlloc;

		private Alloc allocPool;

		public readonly int PageSize;

		public readonly byte MinBucketIndex;

		public readonly byte MaxBucketIndex;

		public readonly int ElementSize;

		public readonly bool CoalesceAllocs;

		public int PageCount { get; private set; }

		public int BucketCount => MaxBucketIndex + 1;

		public int MinBucketSize => 1 << (int)MinBucketIndex;

		public int MaxBucketSize => 1 << (int)MaxBucketIndex;

		public ArrayAllocator(int elementSize, int minBucketSize, int maxBucketSize, int pageSize, int initialPageCount, int allocPoolInitialSize, bool coalesceAllocs)
		{
			coalesceAllocs = false;
			PageSize = pageSize;
			PageCount = initialPageCount;
			ElementSize = elementSize;
			CoalesceAllocs = coalesceAllocs;
			GetBucketIndex(pageSize);
			MinBucketIndex = GetBucketIndex(minBucketSize);
			if (maxBucketSize > 0)
			{
				MaxBucketIndex = (byte)Mathf.Min(GetBucketIndex(maxBucketSize), GetBucketIndex(pageSize));
			}
			else
			{
				MaxBucketIndex = GetBucketIndex(pageSize);
			}
			for (int i = 0; i < initialPageCount; i++)
			{
				pageRoot = new MemoryPage(this, pageSize, MinBucketIndex, MaxBucketIndex, CoalesceAllocs)
				{
					Next = pageRoot
				};
			}
			emptyAlloc = new Alloc();
			emptyAlloc.Page = pageRoot;
			emptyAlloc.Offset = 0;
			emptyAlloc.Length = 0;
			emptyAlloc.BucketIndex = 0;
			for (int j = 0; j < allocPoolInitialSize; j++)
			{
				allocPool = new Alloc
				{
					Next = allocPool
				};
			}
		}

		public IAlloc<T> Allocate(int size)
		{
			if (size == 0)
			{
				return emptyAlloc;
			}
			byte bucketIndex = System.Math.Max(MinBucketIndex, GetBucketIndex(size));
			Alloc alloc = null;
			MemoryPage next = pageRoot;
			while (next != null && !next.Allocate(size, bucketIndex, ref alloc))
			{
				next = next.Next;
			}
			if (alloc == null)
			{
				Debug.LogWarningFormat("ArrayAllocator - Dynamically allocating page! Increase Page Initial Count or Size! ElementSize {0}, MinBucketSize {1}, MaxBucketSize {2}, PageSize {3}, PageCount {4}", ElementSize, MinBucketSize, MaxBucketSize, PageSize, PageCount);
				MemoryPage memoryPage = new MemoryPage(this, PageSize, MinBucketIndex, MaxBucketIndex, CoalesceAllocs);
				memoryPage.Next = pageRoot;
				pageRoot = memoryPage;
				int pageCount = PageCount + 1;
				PageCount = pageCount;
				if (!memoryPage.Allocate(size, bucketIndex, ref alloc))
				{
					Debug.LogErrorFormat("ArrayAllocator - New page of size {0} could not allocate size {1}. What?!", PageSize, size);
				}
			}
			alloc.Length = size;
			return alloc;
		}

		public void Free(IAlloc<T> allocation)
		{
			if (allocation != emptyAlloc && allocation is Alloc alloc)
			{
				alloc.Page.Free(alloc);
			}
		}

		public Alloc GetOrCreateEmptyAlloc()
		{
			if (allocPool != null)
			{
				Alloc alloc = allocPool;
				allocPool = allocPool.Next;
				alloc.Next = null;
				return alloc;
			}
			return new Alloc();
		}

		public void GetDebugInfo(int[] bucketFree, int[] bucketInUse, int[] bucketPeakInUse, long[] bucketWaste, int[] pageInUse, int[] pageFree)
		{
			Array.Clear(bucketFree, 0, bucketFree.Length);
			Array.Clear(bucketInUse, 0, bucketInUse.Length);
			Array.Clear(bucketPeakInUse, 0, bucketPeakInUse.Length);
			Array.Clear(bucketWaste, 0, bucketWaste.Length);
			int num = 0;
			for (MemoryPage next = pageRoot; next != null; next = next.Next)
			{
				next.GetDebugInfo(bucketFree, bucketInUse, bucketPeakInUse, bucketWaste);
				pageInUse[num] = next.Offset;
				pageFree[num] = next.Remaining;
				num++;
			}
		}

		public void Reset()
		{
			for (MemoryPage next = pageRoot; next != null; next = next.Next)
			{
				next.ForceReset();
			}
		}

		private static byte GetBucketIndex(int size)
		{
			return (byte)Mathf.CeilToInt(Mathf.Log(size) * 1.442695f);
		}

		private static bool IsPowerOfTwo(int x)
		{
			if (x > 0)
			{
				return (x & (x - 1)) == 0;
			}
			return false;
		}

		public long EstimateBytes()
		{
			return (long)PageCount * (long)PageSize * ElementSize;
		}

		public override string ToString()
		{
			return $"[ArrayAllocator: PageSize={PageSize}, PageCount={PageCount}, BucketCount={BucketCount}, MinBucketSize={MinBucketSize}, MaxBucketSize={MaxBucketSize}, MinBucketIndex={MinBucketIndex}, MaxBucketIndex={MaxBucketIndex}, ElementSize={ElementSize}, CoalesceAllocs={CoalesceAllocs}]";
		}
	}
}
