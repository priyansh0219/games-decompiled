using System;
using ProtoBuf;

[ProtoContract]
public struct Grid3Shape : IEquatable<Grid3Shape>
{
	[NonSerialized]
	[ProtoMember(1)]
	public int x;

	[NonSerialized]
	[ProtoMember(2)]
	public int y;

	[NonSerialized]
	[ProtoMember(3)]
	public int z;

	[NonSerialized]
	[ProtoMember(4)]
	public int xy;

	public int Size => xy * z;

	public override bool Equals(object obj)
	{
		if (obj is Grid3Shape)
		{
			return Equals((Grid3Shape)obj);
		}
		return false;
	}

	public bool Equals(Grid3Shape other)
	{
		if (x == other.x && y == other.y)
		{
			return z == other.z;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((12289 * 31 + x) * 31 + y) * 31 + z;
	}

	public static bool operator ==(Grid3Shape left, Grid3Shape right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Grid3Shape left, Grid3Shape right)
	{
		return !left.Equals(right);
	}

	public Int3 ToInt3()
	{
		return new Int3(x, y, z);
	}

	public Grid3Shape(Int3 size)
	{
		x = size.x;
		y = size.y;
		z = size.z;
		xy = x * y;
	}

	public int GetIndex(int i, int j, int k)
	{
		if (i < 0 || i >= x || j < 0 || j >= y || k < 0 || k >= z)
		{
			return -1;
		}
		return i + j * x + k * xy;
	}

	public int GetIndex(Int3 point)
	{
		return GetIndex(point.x, point.y, point.z);
	}

	public Int3 GetPointAsInt3(int index)
	{
		if (index < 0 || index >= Size)
		{
			return new Int3(-1);
		}
		int result;
		int num = Math.DivRem(index, xy, out result);
		int result2;
		int num2 = Math.DivRem(result, x, out result2);
		return new Int3(result2, num2, num);
	}

	public Grid3Point GetPoint(int index)
	{
		if (index < 0 || index >= Size)
		{
			return Grid3Point.Invalid;
		}
		int result;
		int num = Math.DivRem(index, xy, out result);
		int result2;
		int num2 = Math.DivRem(result, x, out result2);
		return new Grid3Point(result2, num2, num, index);
	}

	public Grid3Point GetPoint(int i, int j, int k)
	{
		return new Grid3Point(i, j, k, GetIndex(i, j, k));
	}

	public Grid3Point GetPoint(Int3 point)
	{
		return GetPoint(point.x, point.y, point.z);
	}

	public bool GetNextPoint(ref Grid3Point point)
	{
		point.index++;
		point.x++;
		if (point.x < x)
		{
			return true;
		}
		point.x = 0;
		point.y++;
		if (point.y < y)
		{
			return true;
		}
		point.y = 0;
		point.z++;
		return point.z < z;
	}

	public static Grid3Point[] CreateNeighborStorage()
	{
		return new Grid3Point[6];
	}

	public int GetAboveIndex(ref Grid3Point point)
	{
		if (!point.Valid || point.y + 1 >= y)
		{
			return -1;
		}
		return point.index + x;
	}

	public int GetBelowIndex(ref Grid3Point point)
	{
		if (!point.Valid || point.y <= 0)
		{
			return -1;
		}
		return point.index - x;
	}

	public int GetEastIndex(ref Grid3Point point)
	{
		if (!point.Valid || point.x + 1 >= x)
		{
			return -1;
		}
		return point.index + 1;
	}

	public int GetWestIndex(ref Grid3Point point)
	{
		if (!point.Valid || point.x <= 0)
		{
			return -1;
		}
		return point.index - 1;
	}

	public int GetNorthIndex(ref Grid3Point point)
	{
		if (!point.Valid || point.z + 1 >= z)
		{
			return -1;
		}
		return point.index + xy;
	}

	public int GetSouthIndex(ref Grid3Point point)
	{
		if (!point.Valid || point.z <= 0)
		{
			return -1;
		}
		return point.index - xy;
	}

	public int GetNeighbors(ref Grid3Point point, Grid3Point[] neighbors)
	{
		int num = 0;
		if (point.x > 0)
		{
			neighbors[num] = new Grid3Point(point.x - 1, point.y, point.z, point.index - 1);
			num++;
		}
		if (point.y > 0)
		{
			neighbors[num] = new Grid3Point(point.x, point.y - 1, point.z, point.index - x);
			num++;
		}
		if (point.z > 0)
		{
			neighbors[num] = new Grid3Point(point.x, point.y, point.z - 1, point.index - xy);
			num++;
		}
		if (point.x + 1 < x)
		{
			neighbors[num] = new Grid3Point(point.x + 1, point.y, point.z, point.index + 1);
			num++;
		}
		if (point.y + 1 < y)
		{
			neighbors[num] = new Grid3Point(point.x, point.y + 1, point.z, point.index + x);
			num++;
		}
		if (point.z + 1 < z)
		{
			neighbors[num] = new Grid3Point(point.x, point.y, point.z + 1, point.index + xy);
			num++;
		}
		return num;
	}
}
