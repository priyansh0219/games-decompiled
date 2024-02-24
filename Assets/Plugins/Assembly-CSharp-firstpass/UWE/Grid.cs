using System.Collections.Generic;
using System.IO;

namespace UWE
{
	public class Grid<T>
	{
		public delegate void EntryWriter(T entry, StreamWriter sw);

		public class Iterator
		{
			private Grid<T> host;

			public int x { get; private set; }

			public int y { get; private set; }

			public Iterator(Grid<T> host)
			{
				this.host = host;
				x = 0;
				y = 0;
			}

			public void Next()
			{
				y++;
				if (y >= host.GetYSize(x))
				{
					y = 0;
					x++;
				}
			}

			public bool IsDone()
			{
				return x >= host.GetXSize();
			}
		}

		private List<List<T>> data = new List<List<T>>();

		public T Get(Int2 p)
		{
			return Get(p.x, p.y);
		}

		public bool GetExists(int x, int y)
		{
			if (x >= data.Count)
			{
				return false;
			}
			if (y >= data[x].Count)
			{
				return false;
			}
			return true;
		}

		public T Get(int x, int y)
		{
			if (x >= data.Count)
			{
				return default(T);
			}
			if (y >= data[x].Count)
			{
				return default(T);
			}
			return data[x][y];
		}

		public void AddRow()
		{
			data.Add(new List<T>());
		}

		public void Set(int x, int y, T item)
		{
			while (x >= data.Count)
			{
				AddRow();
			}
			while (y >= data[x].Count)
			{
				data[x].Add(default(T));
			}
			data[x][y] = item;
		}

		public int GetXSize()
		{
			return data.Count;
		}

		public int GetYSize(int x)
		{
			return data[x].Count;
		}

		public void WriteToText(StreamWriter sw, EntryWriter writer)
		{
			sw.WriteLine(string.Concat(data.Count));
			for (int i = 0; i < data.Count; i++)
			{
				sw.WriteLine(string.Concat(data[i].Count));
				for (int j = 0; j < data[i].Count; j++)
				{
					writer(data[i][j], sw);
				}
			}
		}

		public void Clear()
		{
			for (int i = 0; i < data.Count; i++)
			{
				data[i].Clear();
			}
			data.Clear();
		}

		public Iterator Begin()
		{
			return new Iterator(this);
		}
	}
}
