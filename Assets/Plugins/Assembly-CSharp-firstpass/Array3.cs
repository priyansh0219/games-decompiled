using System;
using System.Collections;
using System.Collections.Generic;

public class Array3<T> : IEnumerable<T>, IEnumerable
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private int i;

		private T[] items;

		object IEnumerator.Current => items[i];

		public T Current => items[i];

		public Enumerator(T[] _items)
		{
			items = _items;
			i = -1;
		}

		public bool MoveNext()
		{
			i++;
			return i < items.Length;
		}

		public void Reset()
		{
			i = -1;
		}

		public void Dispose()
		{
		}
	}

	public readonly int sizeX;

	public readonly int sizeY;

	public readonly int sizeZ;

	public readonly T[] data;

	public int Length => data.Length;

	public T this[int x, int y, int z]
	{
		get
		{
			return Get(x, y, z);
		}
		set
		{
			Set(x, y, z, value);
		}
	}

	public Array3(int uniformSize)
		: this(uniformSize, uniformSize, uniformSize)
	{
	}

	public Array3(int sizeX, int sizeY, int sizeZ)
	{
		this.sizeX = sizeX;
		this.sizeY = sizeY;
		this.sizeZ = sizeZ;
		data = new T[sizeX * sizeY * sizeZ];
	}

	public int GetLength(int dimension)
	{
		switch (dimension)
		{
		case 0:
			return sizeX;
		case 1:
			return sizeY;
		case 2:
			return sizeZ;
		default:
			return 0;
		}
	}

	public T Get(int x, int y, int z)
	{
		return data[GetIndex(x, y, z)];
	}

	public void Set(int x, int y, int z, T value)
	{
		data[GetIndex(x, y, z)] = value;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(data);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>)data).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)data).GetEnumerator();
	}

	public bool CheckBounds(int x, int y, int z)
	{
		if (x >= 0 && x < sizeX && y >= 0 && y < sizeY && z >= 0)
		{
			return z < sizeZ;
		}
		return false;
	}

	public void Clear()
	{
		Array.Clear(data, 0, data.Length);
	}

	private int GetIndex(int x, int y, int z)
	{
		return x * (sizeY * sizeZ) + y * sizeZ + z;
	}
}
