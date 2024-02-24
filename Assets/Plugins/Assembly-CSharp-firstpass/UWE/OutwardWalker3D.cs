using System;
using System.Collections;
using System.Collections.Generic;

namespace UWE
{
	public struct OutwardWalker3D : IEnumerator<Int3>, IEnumerator, IDisposable, IEnumerable<Int3>, IEnumerable
	{
		private int ringBound;

		private int currRing;

		private RingWalker3D inner;

		object IEnumerator.Current => inner.Current;

		public Int3 Current => inner.Current;

		public OutwardWalker3D(int ringBound)
		{
			this.ringBound = ringBound;
			currRing = 0;
			inner = new RingWalker3D(0);
		}

		public bool MoveNext()
		{
			if (!inner.MoveNext())
			{
				currRing++;
				inner.Reset(currRing);
				inner.MoveNext();
			}
			return currRing <= ringBound;
		}

		public void Reset(int newRingBound)
		{
			ringBound = newRingBound;
			Reset();
		}

		public void Reset()
		{
			currRing = 0;
			inner.Reset(0);
		}

		public void Dispose()
		{
		}

		public OutwardWalker3D GetEnumerator()
		{
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		IEnumerator<Int3> IEnumerable<Int3>.GetEnumerator()
		{
			return this;
		}
	}
}
