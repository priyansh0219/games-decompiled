using System;
using System.Collections;
using System.Collections.Generic;

namespace UWE
{
	public struct MultiBounds : IEnumerator<Int3>, IEnumerator, IDisposable, IEnumerable<Int3>, IEnumerable
	{
		private Int3.RangeEnumerator current;

		private Int3.Bounds bounds1;

		private Int3.Bounds bounds2;

		private Int3.Bounds bounds3;

		private int currentIndex;

		object IEnumerator.Current => current.Current;

		public Int3 Current => current.Current;

		public MultiBounds(Int3.Bounds b1)
			: this(b1, Int3.Bounds.empty, Int3.Bounds.empty)
		{
		}

		public MultiBounds(Int3.Bounds b1, Int3.Bounds b2, Int3.Bounds b3)
		{
			bounds1 = b1;
			bounds2 = b2;
			bounds3 = b3;
			currentIndex = 1;
			current = bounds1.GetEnumerator();
		}

		public bool Contains(Int3 value)
		{
			if (!bounds1.Contains(value) && !bounds2.Contains(value))
			{
				return bounds3.Contains(value);
			}
			return true;
		}

		public bool MoveNext()
		{
			if (current.MoveNext())
			{
				return true;
			}
			currentIndex++;
			switch (currentIndex)
			{
			case 1:
				current = bounds1.GetEnumerator();
				break;
			case 2:
				current = bounds2.GetEnumerator();
				break;
			case 3:
				current = bounds3.GetEnumerator();
				break;
			default:
				return false;
			}
			return current.MoveNext();
		}

		public void Reset()
		{
			currentIndex = 1;
			current = bounds1.GetEnumerator();
		}

		public void Dispose()
		{
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		IEnumerator<Int3> IEnumerable<Int3>.GetEnumerator()
		{
			return this;
		}

		public MultiBounds GetEnumerator()
		{
			return this;
		}
	}
}
