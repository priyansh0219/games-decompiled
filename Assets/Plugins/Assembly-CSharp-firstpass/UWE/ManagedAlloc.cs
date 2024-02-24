using System;

namespace UWE
{
	public sealed class ManagedAlloc<T> : IAlloc<T>, IEstimateBytes
	{
		private readonly T[] buffer;

		public IAllocator<T> Heap
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Offset => 0;

		public int Length => buffer.Length;

		public T[] Array => buffer;

		public T this[int Index]
		{
			get
			{
				return buffer[Index];
			}
			set
			{
				buffer[Index] = value;
			}
		}

		public ManagedAlloc(int length)
		{
			buffer = new T[length];
		}

		public long EstimateBytes()
		{
			return buffer.LongLength;
		}
	}
}
