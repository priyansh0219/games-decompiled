namespace UWE
{
	public class ThreadSafeLinearArrayHeap<T> : LinearArrayHeap<T>
	{
		private object lockObject = new object();

		public ThreadSafeLinearArrayHeap(int elementSize, int maxSize)
			: base(elementSize, maxSize)
		{
		}

		public override IAlloc<T> Allocate(int size)
		{
			lock (lockObject)
			{
				return base.Allocate(size);
			}
		}

		public override void Free(IAlloc<T> a)
		{
			lock (lockObject)
			{
				base.Free(a);
			}
		}

		public override void Reset()
		{
			lock (lockObject)
			{
				base.Reset();
			}
		}
	}
}
