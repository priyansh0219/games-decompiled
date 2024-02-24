using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UWE;
using UnityEngine;

public struct Int2 : IEquatable<Int2>
{
	public class EqualityComparer : IEqualityComparer<Int2>
	{
		public bool Equals(Int2 a, Int2 b)
		{
			return a.Equals(b);
		}

		public int GetHashCode(Int2 obj)
		{
			return obj.GetHashCode();
		}
	}

	public struct RangeEnumerator
	{
		public Int2 mins;

		public Int2 maxs;

		private Int2 p;

		public Int2 Current => p;

		public RangeEnumerator(Int2 mins, Int2 maxs)
		{
			if (!(mins <= maxs))
			{
				Debug.LogError(string.Concat("Int2 Range not valid: ", mins, " --> ", maxs));
			}
			this.mins = mins;
			this.maxs = maxs;
			p = mins;
			Reset();
		}

		public bool MoveNext()
		{
			p.y++;
			if (p.y > maxs.y)
			{
				p.x++;
				p.y = mins.y;
			}
			return p <= maxs;
		}

		public void Reset()
		{
			p = new Int2(mins.x, mins.y - 1);
		}

		public RangeEnumerator GetEnumerator()
		{
			return this;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CompareXY : IComparer<Int2>
	{
		private int Compare(int a, int b)
		{
			if (a < b)
			{
				return -1;
			}
			if (a == b)
			{
				return 0;
			}
			return 1;
		}

		public int Compare(Int2 a, Int2 b)
		{
			if (a.x == b.x)
			{
				return Compare(a.y, b.y);
			}
			return Compare(a.x, b.x);
		}
	}

	public static readonly EqualityComparer equalityComparer = new EqualityComparer();

	public int x;

	public int y;

	public static readonly int[] _8NborXOffset = new int[8] { -1, 0, 1, -1, 1, -1, 0, 1 };

	public static readonly int[] _8NborYOffset = new int[8] { 1, 1, 1, 0, 0, -1, -1, -1 };

	public static readonly Int2[] _CornerNborOffset = new Int2[4]
	{
		new Int2(1, 1),
		new Int2(1, -1),
		new Int2(-1, -1),
		new Int2(-1, 1)
	};

	public static readonly int[] _SinTable = new int[4] { 0, 1, 0, -1 };

	public static readonly int[] _CosTable = new int[4] { 1, 0, -1, 0 };

	public static Int2 zero => new Int2(0, 0);

	public Int2(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public Int2(Int2 other)
	{
		x = other.x;
		y = other.y;
	}

	public Int2(int v)
	{
		x = v;
		y = v;
	}

	public Int2 GetNorth()
	{
		return new Int2(x, y + 1);
	}

	public Int2 GetEast()
	{
		return new Int2(x + 1, y);
	}

	public Int2 GetSouth()
	{
		return new Int2(x, y - 1);
	}

	public Int2 GetWest()
	{
		return new Int2(x - 1, y);
	}

	public Int2 RotateCCW(int quads, Int2 center)
	{
		int num = _CosTable[quads % 4];
		int num2 = _SinTable[quads % 4];
		int num3 = x - center.x;
		int num4 = y - center.y;
		return new Int2(num * num3 - num2 * num4 + center.x, num2 * num3 + num * num4 + center.y);
	}

	public Int2 RotateCCW(int quads, Vector2 center)
	{
		int num = _CosTable[quads % 4];
		int num2 = _SinTable[quads % 4];
		float num3 = (float)x - center.x;
		float num4 = (float)y - center.y;
		return RoundToInt2(new Vector2((float)num * num3 - (float)num2 * num4 + center.x, (float)num2 * num3 + (float)num * num4 + center.y));
	}

	public Int2 YX()
	{
		return new Int2(y, x);
	}

	public int RandomHash()
	{
		return Utils.HashInt(x) + Utils.HashInt(y);
	}

	public int Sum()
	{
		return x + y;
	}

	public int Product()
	{
		return x * y;
	}

	public Int2 Add(int dx, int dy)
	{
		return new Int2(x + dx, y + dy);
	}

	public Int2 RandomNbor()
	{
		int num = UnityEngine.Random.Range(0, 8);
		return new Int2(x + _8NborXOffset[num], y + _8NborYOffset[num]);
	}

	public void SetRandomNbor(Int2 src)
	{
		int num = UnityEngine.Random.Range(0, 8);
		x = src.x + _8NborXOffset[num];
		y = src.y + _8NborYOffset[num];
	}

	public Int2 GetNbor(Facing f)
	{
		switch (f)
		{
		case Facing.North:
			return GetNorth();
		case Facing.East:
			return GetEast();
		case Facing.South:
			return GetSouth();
		default:
			return GetWest();
		}
	}

	public Int2 Get4Nbor(int f)
	{
		return GetNbor((Facing)(f % 4));
	}

	public Int2 GetCornerNbor(int f)
	{
		return this + _CornerNborOffset[f % 4];
	}

	public Int2[] Get8Nbors()
	{
		return new Int2[8]
		{
			new Int2(x, y + 1),
			new Int2(x + 1, y + 1),
			new Int2(x + 1, y),
			new Int2(x + 1, y - 1),
			new Int2(x, y - 1),
			new Int2(x - 1, y - 1),
			new Int2(x - 1, y),
			new Int2(x - 1, y + 1)
		};
	}

	public Int2 Get8Nbor(int i)
	{
		return new Int2(x + _8NborXOffset[i], y + _8NborYOffset[i]);
	}

	public Int2[] Get4CornerNbors()
	{
		return new Int2[4]
		{
			new Int2(x + 1, y + 1),
			new Int2(x + 1, y - 1),
			new Int2(x - 1, y - 1),
			new Int2(x - 1, y + 1)
		};
	}

	public Int2[] Get9Nbors()
	{
		return new Int2[9]
		{
			new Int2(x, y),
			new Int2(x, y + 1),
			new Int2(x + 1, y + 1),
			new Int2(x + 1, y),
			new Int2(x + 1, y - 1),
			new Int2(x, y - 1),
			new Int2(x - 1, y - 1),
			new Int2(x - 1, y),
			new Int2(x - 1, y + 1)
		};
	}

	public static bool operator ==(Int2 u, Int2 v)
	{
		return u.Equals(v);
	}

	public static bool operator !=(Int2 u, Int2 v)
	{
		return !u.Equals(v);
	}

	public static Int2 operator %(Int2 u, int d)
	{
		return new Int2(u.x % d, u.y % d);
	}

	public static Int2 operator /(Int2 u, int d)
	{
		return new Int2(u.x / d, u.y / d);
	}

	public static Int2 operator *(Int2 u, int d)
	{
		return new Int2(u.x * d, u.y * d);
	}

	public static Int2 operator -(Int2 u, int d)
	{
		return new Int2(u.x - d, u.y - d);
	}

	public static Int2 operator +(Int2 u, int d)
	{
		return new Int2(u.x + d, u.y + d);
	}

	public static Int2 operator <<(Int2 u, int d)
	{
		return new Int2(u.x << d, u.y << d);
	}

	public static Int2 operator >>(Int2 u, int d)
	{
		return new Int2(u.x >> d, u.y >> d);
	}

	public static Int2 operator +(Int2 u, Int2 v)
	{
		return new Int2(u.x + v.x, u.y + v.y);
	}

	public static Int2 operator -(Int2 u, Int2 v)
	{
		return new Int2(u.x - v.x, u.y - v.y);
	}

	public static Vector2 operator +(Int2 u, Vector2 v)
	{
		return new Vector2((float)u.x + v.x, (float)u.y + v.y);
	}

	public static Vector2 operator +(Int2 u, float s)
	{
		return new Vector2((float)u.x + s, (float)u.y + s);
	}

	public static Vector2 operator -(Int2 u, float s)
	{
		return new Vector2((float)u.x - s, (float)u.y - s);
	}

	public static bool operator <=(Int2 u, Int2 v)
	{
		if (u.x <= v.x)
		{
			return u.y <= v.y;
		}
		return false;
	}

	public static bool operator >=(Int2 u, Int2 v)
	{
		if (u.x >= v.x)
		{
			return u.y >= v.y;
		}
		return false;
	}

	public static bool operator <(Int2 u, Int2 v)
	{
		if (u.x < v.x)
		{
			return u.y < v.y;
		}
		return false;
	}

	public static bool operator >(Int2 u, Int2 v)
	{
		if (u.x > v.x)
		{
			return u.y > v.y;
		}
		return false;
	}

	public static Int2 operator /(Int2 u, Int2 v)
	{
		return new Int2(u.x / v.x, u.y / v.y);
	}

	public static Int2 operator *(Int2 u, Int2 v)
	{
		return new Int2(u.x * v.x, u.y * v.y);
	}

	public override bool Equals(object obj)
	{
		if (obj is Int2)
		{
			return Equals((Int2)obj);
		}
		return false;
	}

	public bool Equals(Int2 other)
	{
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}

	public override string ToString()
	{
		return x + "," + y;
	}

	public override int GetHashCode()
	{
		return (12289 * 31 + x.GetHashCode()) * 31 + y.GetHashCode();
	}

	public float GetDistance(Int2 v)
	{
		int num = v.x - x;
		int num2 = v.y - y;
		return Mathf.Sqrt(num * num + num2 * num2);
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}

	public Int3 X0Y()
	{
		return new Int3(x, 0, y);
	}

	public Int3 XZToInt3(int newy)
	{
		return new Int3(x, newy, y);
	}

	public Int3 AsAxialOffset(int dim)
	{
		switch (dim)
		{
		case 0:
			return new Int3(0, x, y);
		case 1:
			return new Int3(x, 0, y);
		default:
			return new Int3(x, y, 0);
		}
	}

	public void Write(StreamWriter writer)
	{
		writer.WriteLine(ToString());
	}

	public bool Read(StreamReader reader)
	{
		string[] array = reader.ReadLine().Split(' ');
		if (array.Length != 2)
		{
			Debug.LogError("Error while reading Int2 - input is corrupt.");
			return false;
		}
		x = Convert.ToInt32(array[0]);
		y = Convert.ToInt32(array[1]);
		return true;
	}

	public static Int2 Min(Int2 a, Int2 b)
	{
		return new Int2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
	}

	public static Int2 Max(Int2 a, Int2 b)
	{
		return new Int2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
	}

	public Int2 Clamp(Int2 min, Int2 max)
	{
		return new Int2(Mathf.Clamp(x, min.x, max.x), Mathf.Clamp(y, min.y, max.y));
	}

	public static Int2 Floor(Vector2 v)
	{
		return FloorToInt2(v);
	}

	public static Int2 FloorToInt2(Vector2 v)
	{
		return new Int2(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
	}

	public static Int2 CeilToInt2(Vector2 v)
	{
		return new Int2(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
	}

	public static Int2 RoundToInt2(Vector2 v)
	{
		return new Int2(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
	}

	public Int3 ToInt3XZ(int _y)
	{
		return new Int3(x, _y, y);
	}

	public static RangeEnumerator Range(Int2 mins, Int2 maxs)
	{
		return new RangeEnumerator(mins, maxs);
	}

	public static RangeEnumerator Range(Int2 upperBound)
	{
		return new RangeEnumerator(new Int2(0, 0), upperBound - 1);
	}

	public static Int2 Dims<T>(T[,] arr)
	{
		return new Int2(arr.GetLength(0), arr.GetLength(1));
	}

	public static RangeEnumerator Indices<T>(T[,] arr)
	{
		return Range(new Int2(arr.GetLength(0), arr.GetLength(1)));
	}

	public static Int2 Parse(string line)
	{
		string[] array = line.Split(',');
		if (array.Length != 2)
		{
			Debug.LogError("Error while reading Int2 - input is corrupt.");
			return new Int2(0);
		}
		int num = Convert.ToInt32(array[0]);
		int num2 = Convert.ToInt32(array[1]);
		return new Int2(num, num2);
	}

	public static T[,] Allocate<T>(Int2 size)
	{
		return new T[size.x, size.y];
	}
}
