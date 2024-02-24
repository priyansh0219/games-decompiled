using System;
using System.Collections;
using System.Collections.Generic;

namespace UWE
{
	public struct RingWalker3D : IEnumerator<Int3>, IEnumerator, IDisposable, IEnumerable<Int3>, IEnumerable
	{
		private int ring;

		private int x;

		private int y;

		private int z;

		object IEnumerator.Current => new Int3(x, y, z);

		public Int3 Current => new Int3(x, y, z);

		public RingWalker3D(int ring)
		{
			this = default(RingWalker3D);
			this.ring = ring;
			Reset();
		}

		public bool MoveNext()
		{
			if (x == -ring || x == ring || y == -ring || y == ring)
			{
				z++;
			}
			else
			{
				z += 2 * ring;
			}
			if (z > ring)
			{
				y++;
				z = -ring;
			}
			if (y > ring)
			{
				x++;
				y = -ring;
			}
			return x <= ring;
		}

		public void Reset(int newRing)
		{
			ring = newRing;
			Reset();
		}

		public void Reset()
		{
			x = -ring;
			y = -ring;
			z = -ring - 1;
		}

		public void Dispose()
		{
		}

		public RingWalker3D GetEnumerator()
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
