using System.Collections;

public class MatrixBool
{
	private BitArray data;

	public readonly int Size;

	public MatrixBool(int size, bool defaultValue)
	{
		Size = size;
		int length = (1 + Size) * Size / 2;
		data = new BitArray(length, defaultValue);
	}

	public bool Get(int x, int y)
	{
		return data[GetIndex(x, y)];
	}

	public void Set(int x, int y, bool value)
	{
		data[GetIndex(x, y)] = value;
	}

	public void Clear()
	{
		data.SetAll(value: false);
	}

	private int GetIndex(int x, int y)
	{
		if (x < y)
		{
			int num = x;
			x = y;
			y = num;
		}
		if (y <= 0)
		{
			return x;
		}
		return (2 * Size - y + 1) * y / 2 + (x - y);
	}
}
