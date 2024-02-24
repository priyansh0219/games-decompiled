using System;
using Unity.Collections;
using UnityEngine;

namespace UWE
{
	public sealed class NativeLinearArrayHeap<T> : IDisposable, IAllocator<T>, IEstimateBytes where T : struct
	{
		private class Alloc : IAlloc<T>
		{
			public IAllocator<T> Heap { get; }

			public int Offset { get; }

			public int Length { get; }

			public NativeLinearArrayHeap<T> ActualHeap { get; }

			public T[] Array
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

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

			public Alloc(NativeLinearArrayHeap<T> heap, int offset, int length)
			{
				Heap = heap;
				Offset = offset;
				Length = length;
				ActualHeap = heap;
			}

			public void Set(int i, T value)
			{
				ActualHeap.buffer[Offset + i] = value;
			}

			public T Get(int i)
			{
				return ActualHeap.buffer[Offset + i];
			}
		}

		public NativeArray<T> buffer;

		private int offset;

		private readonly object lockObject = new object();

		public int Highwater { get; private set; }

		public int Outstanding { get; private set; }

		public int PeakOutstanding { get; private set; }

		public int ElementSize { get; }

		public int MaxSize => buffer.Length;

		public NativeLinearArrayHeap(int elementSize, int maxSize)
		{
			buffer = new NativeArray<T>(maxSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			offset = 0;
			ElementSize = elementSize;
			Outstanding = 0;
		}

		public IAlloc<T> Allocate(int size)
		{
			lock (lockObject)
			{
				if (offset + size > buffer.Length)
				{
					throw new ArgumentException("Allocated too much for a NativeLinearArrayHeap.");
				}
				Alloc result = new Alloc(this, offset, size);
				offset += size;
				int outstanding = Outstanding + 1;
				Outstanding = outstanding;
				PeakOutstanding = Mathf.Max(PeakOutstanding, Outstanding);
				Highwater = Mathf.Max(Highwater, offset);
				return result;
			}
		}

		public void Free(IAlloc<T> a)
		{
			lock (lockObject)
			{
				int outstanding = Outstanding - 1;
				Outstanding = outstanding;
			}
		}

		public void Reset()
		{
			lock (lockObject)
			{
				offset = 0;
			}
		}

		public long EstimateBytes()
		{
			return ElementSize * MaxSize;
		}

		public void Dispose()
		{
			lock (lockObject)
			{
				buffer.Dispose();
			}
		}
	}
}
