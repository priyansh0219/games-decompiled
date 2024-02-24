using System.IO;

namespace UWE
{
	public class InfiniteGrid<T>
	{
		public class Pos
		{
			public int x;

			public int y;

			public Pos(int _x, int _y)
			{
				x = _x;
				y = _y;
			}
		}

		public delegate bool ForEachFunc(int x, int y, T value);

		public delegate bool PickFunc(int x, int y, T value);

		private Grid<T> q1 = new Grid<T>();

		private Grid<T> q2 = new Grid<T>();

		private Grid<T> q3 = new Grid<T>();

		private Grid<T> q4 = new Grid<T>();

		public int GetQuadrantOf(int x, int y)
		{
			if (x >= 0)
			{
				if (y >= 0)
				{
					return 1;
				}
				return 4;
			}
			if (y >= 0)
			{
				return 2;
			}
			return 3;
		}

		public Grid<T> GetQuadrant(int q)
		{
			switch (q)
			{
			case 1:
				return q1;
			case 2:
				return q2;
			case 3:
				return q3;
			default:
				return q4;
			}
		}

		public T Get(int x, int y, T defaultVal)
		{
			if (!GetExists(x, y))
			{
				return defaultVal;
			}
			if (x >= 0)
			{
				if (y >= 0)
				{
					return q1.Get(x, y);
				}
				return q4.Get(x, -y - 1);
			}
			if (y >= 0)
			{
				return q2.Get(-x - 1, y);
			}
			return q3.Get(-x - 1, -y - 1);
		}

		public T Get(Int2 p, T defaultVal)
		{
			return Get(p.x, p.y, defaultVal);
		}

		public void Set(int x, int y, T item)
		{
			if (x >= 0)
			{
				if (y >= 0)
				{
					q1.Set(x, y, item);
				}
				else
				{
					q4.Set(x, -y - 1, item);
				}
			}
			else if (y >= 0)
			{
				q2.Set(-x - 1, y, item);
			}
			else
			{
				q3.Set(-x - 1, -y - 1, item);
			}
		}

		public void Set(Int2 u, T item)
		{
			Set(u.x, u.y, item);
		}

		private bool GetExists(int x, int y)
		{
			if (x >= 0)
			{
				if (y >= 0)
				{
					return q1.GetExists(x, y);
				}
				return q4.GetExists(x, -y - 1);
			}
			if (y >= 0)
			{
				return q2.GetExists(-x - 1, y);
			}
			return q3.GetExists(-x - 1, -y - 1);
		}

		public void WriteToText(StreamWriter sw, Grid<T>.EntryWriter writer)
		{
			q1.WriteToText(sw, writer);
			q2.WriteToText(sw, writer);
			q3.WriteToText(sw, writer);
			q4.WriteToText(sw, writer);
		}

		public void Clear()
		{
			q1.Clear();
			q2.Clear();
			q3.Clear();
			q4.Clear();
		}

		private int QuadToCartX(int quadX, int quad)
		{
			switch (quad)
			{
			case 1:
			case 4:
				return quadX;
			default:
				return -1 - quadX;
			}
		}

		private int QuadToCartY(int quadY, int quad)
		{
			switch (quad)
			{
			case 1:
			case 2:
				return quadY;
			default:
				return -1 - quadY;
			}
		}

		public void ForEach(ForEachFunc func)
		{
			for (int i = 1; i <= 4; i++)
			{
				Grid<T> quadrant = GetQuadrant(i);
				for (int j = 0; j < quadrant.GetXSize(); j++)
				{
					for (int k = 0; k < quadrant.GetYSize(j); k++)
					{
						if (!func(QuadToCartX(j, i), QuadToCartY(k, i), quadrant.Get(j, k)))
						{
							return;
						}
					}
				}
			}
		}

		public int Count(PickFunc func)
		{
			int num = 0;
			ForEach(delegate(int x, int y, T value)
			{
				if (func(x, y, value))
				{
					num++;
				}
				return true;
			});
			return num;
		}

		public bool Pick(PickFunc func, int n, ref Int2 pickedPos)
		{
			bool found = false;
			int num = 0;
			Int2 pos = default(Int2);
			ForEach(delegate(int x, int y, T value)
			{
				if (func(x, y, value))
				{
					if (num == n)
					{
						pos = new Int2(x, y);
						found = true;
						return false;
					}
					num++;
				}
				return true;
			});
			pickedPos = pos;
			return found;
		}
	}
}
