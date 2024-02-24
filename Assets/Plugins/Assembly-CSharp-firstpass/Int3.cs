using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ProtoBuf;
using UWE;
using UnityEngine;

[Serializable]
[ProtoContract]
public struct Int3 : IEquatable<Int3>
{
	public class EqualityComparer : IEqualityComparer<Int3>
	{
		public bool Equals(Int3 a, Int3 b)
		{
			return a.Equals(b);
		}

		public int GetHashCode(Int3 obj)
		{
			return obj.GetHashCode();
		}
	}

	public struct RangeEnumerator : IEnumerator<Int3>, IEnumerator, IDisposable
	{
		private Int3 mins;

		private Int3 maxs;

		private Int3 p;

		object IEnumerator.Current => p;

		public Int3 Current => p;

		public RangeEnumerator(Int3 mins, Int3 maxs)
		{
			this.mins = mins;
			this.maxs = maxs;
			p = mins;
			Reset();
		}

		public bool MoveNext()
		{
			p.Next(mins, maxs);
			return p <= maxs;
		}

		public bool MoveNext(int step)
		{
			for (int i = 0; i < step; i++)
			{
				p.Next(mins, maxs);
			}
			return p <= maxs;
		}

		public void Reset()
		{
			p = new Int3(mins.x, mins.y, mins.z - 1);
		}

		public void Dispose()
		{
		}

		public RangeEnumerator GetEnumerator()
		{
			return this;
		}
	}

	public struct MooreEnumerator
	{
		public Int3 center;

		private Int3 offset;

		public Int3 Current => offset + center;

		public MooreEnumerator(Int3 center)
		{
			this.center = center;
			offset = new Int3(0, 0, 0);
			Reset();
		}

		public bool MoveNext()
		{
			offset.z++;
			if (offset.x == 0 && offset.y == 0 && offset.z == 0)
			{
				offset.z++;
			}
			else if (offset.z > 1)
			{
				offset.z = -1;
				offset.y++;
				if (offset.y > 1)
				{
					offset.y = -1;
					offset.x++;
				}
			}
			return offset <= new Int3(1, 1, 1);
		}

		public void Reset()
		{
			offset = new Int3(0, 0, -2);
		}

		public MooreEnumerator GetEnumerator()
		{
			return this;
		}
	}

	[ProtoContract]
	public struct Bounds : IEquatable<Bounds>
	{
		public class EnumerableBox : IEnumerable<Int3>, IEnumerable
		{
			private readonly Bounds bounds;

			public EnumerableBox(Bounds bounds)
			{
				this.bounds = bounds;
			}

			public RangeEnumerator GetEnumerator()
			{
				return bounds.GetRangeEnumerator();
			}

			IEnumerator<Int3> IEnumerable<Int3>.GetEnumerator()
			{
				return bounds.GetRangeEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return bounds.GetRangeEnumerator();
			}
		}

		[ProtoMember(1)]
		public Int3 mins;

		[ProtoMember(2)]
		public Int3 maxs;

		public static readonly Bounds empty = new Bounds(one, zero);

		public Vector3 center => (mins.ToVector3() + (maxs + 1).ToVector3()) * 0.5f;

		public Int3 size => maxs - mins + 1;

		public Bounds(Int3 mins, Int3 maxs)
		{
			this.mins = mins;
			this.maxs = maxs;
		}

		public static Bounds Union(Bounds a, Bounds b)
		{
			return new Bounds(Min(a.mins, b.mins), Max(a.maxs, b.maxs));
		}

		public Bounds Union(Int3 p)
		{
			return new Bounds(Min(mins, p), Max(maxs, p));
		}

		public bool Equals(Bounds other)
		{
			if (mins.Equals(other.mins))
			{
				return maxs.Equals(other.maxs);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Bounds)
			{
				return Equals((Bounds)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (12289 * 31 + mins.GetHashCode()) * 31 + maxs.GetHashCode();
		}

		public static bool operator ==(Bounds u, Bounds v)
		{
			return u.Equals(v);
		}

		public static bool operator !=(Bounds u, Bounds v)
		{
			return !u.Equals(v);
		}

		public bool Intersects(Bounds other)
		{
			if (maxs.x >= other.mins.x && mins.x <= other.maxs.x && maxs.y >= other.mins.y && mins.y <= other.maxs.y && maxs.z >= other.mins.z)
			{
				return mins.z <= other.maxs.z;
			}
			return false;
		}

		public Bounds Intersect(Bounds other)
		{
			return IntersectionWith(other);
		}

		public Bounds IntersectionWith(Bounds other)
		{
			return new Bounds(Max(mins, other.mins), Min(maxs, other.maxs));
		}

		public bool Contains(Bounds other)
		{
			if (mins <= other.mins)
			{
				return maxs >= other.maxs;
			}
			return false;
		}

		public bool Contains(Int3 p)
		{
			if (mins <= p)
			{
				return maxs >= p;
			}
			return false;
		}

		public bool CellContains(Vector3 pos)
		{
			Int3 @int = Floor(pos);
			if (@int >= mins)
			{
				return @int <= maxs;
			}
			return false;
		}

		public static Bounds NullBounds()
		{
			return new Bounds(new Int3(int.MaxValue), new Int3(int.MinValue));
		}

		public void Expand(int s)
		{
			Expand(new Int3(s));
		}

		public void Expand(Int3 s)
		{
			maxs += s;
			mins -= s;
		}

		public Bounds Expanded(int s)
		{
			return new Bounds(mins - s, maxs + s);
		}

		public void Contract(int s)
		{
			Contract(new Int3(s));
		}

		public void Contract(Int3 s)
		{
			maxs -= s;
			maxs = Max(mins, maxs);
		}

		public void Extrude(Int3 d)
		{
			mins = Min(mins, mins + d);
			maxs = Max(maxs, maxs + d);
		}

		public void Unextrude(Int3 d)
		{
			mins = Max(mins, mins + d);
			maxs = Min(maxs, maxs + d);
		}

		public void Move(Int3 dt)
		{
			mins += dt;
			maxs += dt;
		}

		public override string ToString()
		{
			return string.Concat(mins, " -> ", maxs);
		}

		private RangeEnumerator GetRangeEnumerator()
		{
			return new RangeEnumerator(mins, maxs);
		}

		public RangeEnumerator GetEnumerator()
		{
			return GetRangeEnumerator();
		}

		public Vector3 GetFaceCenter(int face)
		{
			CubeFace cubeFace = new CubeFace(face);
			return center + Vector3.Dot(cubeFace.normal.ToVector3(), size.ToVector3() / 2f) * cubeFace.normal.Abs().ToVector3();
		}

		public bool IsOnXFace(Int3 p)
		{
			if (p.x != mins.x)
			{
				return p.x == maxs.x;
			}
			return true;
		}

		public bool IsOnYFace(Int3 p)
		{
			if (p.y != mins.y)
			{
				return p.y == maxs.y;
			}
			return true;
		}

		public bool IsOnZFace(Int3 p)
		{
			if (p.z != mins.z)
			{
				return p.z == maxs.z;
			}
			return true;
		}

		public Bounds boundsRotateXZ(int ccwTurns)
		{
			Vector3 vector = mins.ToVector3().AddScalar(0.5f);
			Vector3 vector2 = maxs.ToVector3().AddScalar(0.5f);
			Vector3 vector3 = mins.ToVector3();
			Quaternion quaternion = Quaternion.AngleAxis((float)(4 - ccwTurns) * 90f, Vector3.up);
			Vector3 v = quaternion * (vector - vector3) + vector3;
			Vector3 v2 = quaternion * (vector2 - vector3) + vector3;
			Int3 a = Floor(v);
			Int3 b = Floor(v2);
			return new Bounds(Min(a, b), Max(a, b));
		}

		public static Bounds operator *(Bounds b, int s)
		{
			return new Bounds(b.mins * s, b.maxs * s);
		}

		public static Bounds operator *(Bounds b, Int3 s)
		{
			return new Bounds(b.mins * s, b.maxs * s);
		}

		public static Bounds operator /(Bounds b, int s)
		{
			return new Bounds(b.mins / s, b.maxs / s);
		}

		public static Bounds operator /(Bounds b, Int3 s)
		{
			return new Bounds(b.mins / s, b.maxs / s);
		}

		public static Bounds operator +(Bounds b, Int3 s)
		{
			return new Bounds(b.mins + s, b.maxs + s);
		}

		public static Bounds operator -(Bounds b, Int3 s)
		{
			return new Bounds(b.mins - s, b.maxs - s);
		}

		public static Bounds operator <<(Bounds b, int s)
		{
			return new Bounds(b.mins << s, b.maxs << s);
		}

		public static Bounds operator >>(Bounds b, int s)
		{
			return new Bounds(b.mins >> s, b.maxs >> s);
		}

		public UnityEngine.Bounds ToUnityBounds()
		{
			return new UnityEngine.Bounds(mins.ToVector3(), maxs.ToVector3());
		}

		public static Bounds FinerBounds(Int3 coarseCell, int finePerCoarseCell)
		{
			return FinerBounds(new Bounds(coarseCell, coarseCell), new Int3(finePerCoarseCell));
		}

		public static Bounds FinerBounds(Int3 coarseCell, Int3 finePerCoarseCell)
		{
			return FinerBounds(new Bounds(coarseCell, coarseCell), finePerCoarseCell);
		}

		public static Bounds FinerBounds(Bounds coarseBounds, Int3 finePerCoarseCell)
		{
			return new Bounds(coarseBounds.mins * finePerCoarseCell, (coarseBounds.maxs + 1) * finePerCoarseCell - 1);
		}

		public static Bounds InnerCoarserBounds(Bounds fineBounds, Int3 finePerCoarseCell)
		{
			return new Bounds(CeilDiv(fineBounds.mins, finePerCoarseCell), FloorDiv(fineBounds.maxs + 1, finePerCoarseCell) - 1);
		}

		public static Bounds OuterCoarserBounds(Bounds fineBounds, Int3 finePerCoarseCell)
		{
			return new Bounds(FloorDiv(fineBounds.mins, finePerCoarseCell), CeilDiv(fineBounds.maxs + 1, finePerCoarseCell) - 1);
		}

		public Int3 Clamp(Int3 p)
		{
			return Int3.Clamp(p, mins, maxs);
		}

		public Bounds Clamp(Int3 cmins, Int3 cmaxs)
		{
			return new Bounds(mins.Clamp(cmins, cmaxs), maxs.Clamp(cmins, cmaxs));
		}

		public Bounds Clamp(Bounds c)
		{
			return Clamp(c.mins, c.maxs);
		}

		public int GetInclusiveVolume()
		{
			return (maxs - mins + 1).Product();
		}

		public EnumerableBox ToEnumerable()
		{
			return new EnumerableBox(this);
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CompareXYZ : IComparer<Int3>
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

		public int Compare(Int3 a, Int3 b)
		{
			if (a.x == b.x)
			{
				if (a.y == b.y)
				{
					return Compare(a.z, b.z);
				}
				return Compare(a.y, b.y);
			}
			return Compare(a.x, b.x);
		}
	}

	public struct VNNEnumerator
	{
		private static int[] DX = new int[6] { -1, 1, 0, 0, 0, 0 };

		private static int[] DY = new int[6] { 0, 0, -1, 1, 0, 0 };

		private static int[] DZ = new int[6] { 0, 0, 0, 0, -1, 1 };

		private int curr;

		private Int3 center;

		public Int3 Current => center + new Int3(DX[curr], DY[curr], DZ[curr]);

		public VNNEnumerator(Int3 center)
		{
			this.center = center;
			curr = -1;
		}

		public void Reset()
		{
			curr = -1;
		}

		public bool MoveNext()
		{
			curr++;
			return curr < 6;
		}

		public VNNEnumerator GetEnumerator()
		{
			return this;
		}
	}

	public static readonly EqualityComparer equalityComparer = new EqualityComparer();

	[ProtoMember(1)]
	public int x;

	[ProtoMember(2)]
	public int y;

	[ProtoMember(3)]
	public int z;

	public static int[] _CosTable = new int[4] { 1, 0, -1, 0 };

	public static int[] _SinTable = new int[4] { 0, 1, 0, -1 };

	public static readonly Int3 zero = new Int3(0);

	public static readonly Int3 one = new Int3(1);

	public static readonly Int3 negativeOne = new Int3(-1);

	public static readonly Int3 xUnit = new Int3(1, 0, 0);

	public static readonly Int3 yUnit = new Int3(0, 1, 0);

	public static readonly Int3 zUnit = new Int3(0, 0, 1);

	public Int2 xz => XZ();

	public Int2 yz => YZ();

	public Int2 xy => XY();

	public int this[int index]
	{
		get
		{
			switch (index)
			{
			case 0:
				return x;
			case 1:
				return y;
			case 2:
				return z;
			default:
				throw new ArgumentOutOfRangeException("index", "Invalid Int3 index!");
			}
		}
		set
		{
			switch (index)
			{
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			case 2:
				z = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Invalid Vector3 index!");
			}
		}
	}

	public Int3(int _x, int _y, int _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	public Int3(Int3 other)
	{
		x = other.x;
		y = other.y;
		z = other.z;
	}

	public Int3(int v)
	{
		x = v;
		y = v;
		z = v;
	}

	public Int3 PlusX(int d)
	{
		return new Int3(x + d, y, z);
	}

	public Int3 PlusY(int d)
	{
		return new Int3(x, y + d, z);
	}

	public Int3 PlusZ(int d)
	{
		return new Int3(x, y, z + d);
	}

	public Int2 AsAxialOffset(int axis)
	{
		switch (axis)
		{
		case 0:
			return yz;
		case 1:
			return xz;
		default:
			return xy;
		}
	}

	public int Product()
	{
		return x * y * z;
	}

	public int Sum()
	{
		return x + y + z;
	}

	public int SquareMagnitude()
	{
		return x * x + y * y + z * z;
	}

	public int MaxComponent()
	{
		return Mathf.Max(x, Mathf.Max(y, z));
	}

	public Int3 Abs()
	{
		return new Int3(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z));
	}

	public Int3 Snap(int step, Int3 offset)
	{
		return new Int3(x.Snap(step, offset.x), y.Snap(step, offset.y), z.Snap(step, offset.z));
	}

	public int XMajorFlatIndex(Int3 counts)
	{
		return z * counts.y * counts.x + y * counts.x + x;
	}

	public static Int3 operator <<(Int3 u, int s)
	{
		return new Int3(u.x << s, u.y << s, u.z << s);
	}

	public static Int3 operator >>(Int3 u, int s)
	{
		return new Int3(u.x >> s, u.y >> s, u.z >> s);
	}

	public static bool operator ==(Int3 u, Int3 v)
	{
		return u.Equals(v);
	}

	public static bool operator !=(Int3 u, Int3 v)
	{
		return !u.Equals(v);
	}

	public static bool operator ==(Int3 u, int s)
	{
		if (u.x == s && u.y == s)
		{
			return u.z == s;
		}
		return false;
	}

	[Obsolete("Probably buggy", true)]
	public static bool operator !=(Int3 u, int s)
	{
		if (u.x != s && u.y != s)
		{
			return u.z != s;
		}
		return false;
	}

	public static Int3 operator -(Int3 v)
	{
		return new Int3(-v.x, -v.y, -v.z);
	}

	public static Int3 operator +(Int3 v)
	{
		return v;
	}

	public static Vector3 operator +(Int3 u, Vector3 v)
	{
		return new Vector3((float)u.x + v.x, (float)u.y + v.y, (float)u.z + v.z);
	}

	public static Int3 operator +(Int3 u, Int3 v)
	{
		return new Int3(u.x + v.x, u.y + v.y, u.z + v.z);
	}

	public static Int3 operator +(Int3 u, int s)
	{
		return new Int3(u.x + s, u.y + s, u.z + s);
	}

	public static Int3 operator -(Int3 u, Int3 v)
	{
		return new Int3(u.x - v.x, u.y - v.y, u.z - v.z);
	}

	public static Int3 operator -(Int3 u, int s)
	{
		return new Int3(u.x - s, u.y - s, u.z - s);
	}

	public static Int3 operator *(Int3 u, Int3 v)
	{
		return new Int3(u.x * v.x, u.y * v.y, u.z * v.z);
	}

	public static Int3 operator *(Int3 u, int s)
	{
		return new Int3(u.x * s, u.y * s, u.z * s);
	}

	public static Int3 operator *(int s, Int3 u)
	{
		return new Int3(u.x * s, u.y * s, u.z * s);
	}

	public static explicit operator Int3(Vector3 v)
	{
		return new Int3((int)v.x, (int)v.y, (int)v.z);
	}

	public static explicit operator Vector3(Int3 v)
	{
		return new Vector3(v.x, v.y, v.z);
	}

	public static int Dot(Int3 u, Int3 v)
	{
		return Scale(u, v).Sum();
	}

	public static Int3 Scale(Int3 u, Int3 v)
	{
		return u * v;
	}

	public static Vector3 Scale(Vector3 u, Int3 v)
	{
		return new Vector3(u.x * (float)v.x, u.y * (float)v.y, u.z * (float)v.z);
	}

	public static Vector3 Scale(Int3 u, Vector3 v)
	{
		return Scale(v, u);
	}

	public static Int3 AbsScale(Int3 u, Int3 v)
	{
		return new Int3(Mathf.Abs(u.x * v.x), Mathf.Abs(u.y * v.y), Mathf.Abs(u.z * v.z));
	}

	public static Vector3 Div(Vector3 u, Int3 v)
	{
		return new Vector3(u.x / (float)v.x, u.y / (float)v.y, u.z / (float)v.z);
	}

	public static Vector3 Div(Int3 u, Vector3 v)
	{
		return new Vector3((float)u.x / v.x, (float)u.y / v.y, (float)u.z / v.z);
	}

	public static Int3 PositiveModulo(Int3 a, Int3 b)
	{
		return new Int3(UWE.Math.PositiveModulo(a.x, b.x), UWE.Math.PositiveModulo(a.y, b.y), UWE.Math.PositiveModulo(a.z, b.z));
	}

	public static Int3 FloorDiv(Int3 a, int b)
	{
		return new Int3(UWE.Math.FloorDiv(a.x, b), UWE.Math.FloorDiv(a.y, b), UWE.Math.FloorDiv(a.z, b));
	}

	public static Int3 FloorDiv(Int3 a, Int3 b)
	{
		return new Int3(UWE.Math.FloorDiv(a.x, b.x), UWE.Math.FloorDiv(a.y, b.y), UWE.Math.FloorDiv(a.z, b.z));
	}

	public static Int3 CeilDiv(Int3 a, int b)
	{
		return new Int3(UWE.Math.CeilDiv(a.x, b), UWE.Math.CeilDiv(a.y, b), UWE.Math.CeilDiv(a.z, b));
	}

	public static Int3 CeilDiv(Int3 a, Int3 b)
	{
		return new Int3(UWE.Math.CeilDiv(a.x, b.x), UWE.Math.CeilDiv(a.y, b.y), UWE.Math.CeilDiv(a.z, b.z));
	}

	public static bool operator <=(Int3 u, Int3 v)
	{
		if (u.x <= v.x && u.y <= v.y)
		{
			return u.z <= v.z;
		}
		return false;
	}

	public static bool operator >=(Int3 u, Int3 v)
	{
		if (u.x >= v.x && u.y >= v.y)
		{
			return u.z >= v.z;
		}
		return false;
	}

	public static bool operator <(Int3 u, Int3 v)
	{
		if (u.x < v.x && u.y < v.y)
		{
			return u.z < v.z;
		}
		return false;
	}

	public static bool operator >(Int3 u, Int3 v)
	{
		if (u.x > v.x && u.y > v.y)
		{
			return u.z > v.z;
		}
		return false;
	}

	public static bool operator >=(Int3 u, int s)
	{
		if (u.x >= s && u.y >= s)
		{
			return u.z >= s;
		}
		return false;
	}

	public static bool operator <=(Int3 u, int s)
	{
		if (u.x <= s && u.y <= s)
		{
			return u.z <= s;
		}
		return false;
	}

	public static bool operator <(Int3 u, int s)
	{
		if (u.x < s && u.y < s)
		{
			return u.z < s;
		}
		return false;
	}

	public static bool operator >(Int3 u, int s)
	{
		if (u.x > s && u.y > s)
		{
			return u.z > s;
		}
		return false;
	}

	public static Int3 operator /(Int3 u, Int3 v)
	{
		return new Int3(u.x / v.x, u.y / v.y, u.z / v.z);
	}

	public static Int3 operator /(Int3 u, int s)
	{
		return new Int3(u.x / s, u.y / s, u.z / s);
	}

	public static Int3 operator %(Int3 u, int s)
	{
		return new Int3(u.x % s, u.y % s, u.z % s);
	}

	public static Int3 operator %(Int3 u, Int3 v)
	{
		return new Int3(u.x % v.x, u.y % v.y, u.z % v.z);
	}

	public Int3 Plus(int dx, int dy, int dz)
	{
		return new Int3(x + dx, y + dy, z + dz);
	}

	public Int3 CeilDiv(Int3 other)
	{
		return new Int3(Utils.CeilDiv(x, other.x), Utils.CeilDiv(y, other.y), Utils.CeilDiv(z, other.z));
	}

	public Int3 CeilDiv(int d)
	{
		return new Int3(Utils.CeilDiv(x, d), Utils.CeilDiv(y, d), Utils.CeilDiv(z, d));
	}

	public Int3 RoundDiv(int d)
	{
		return new Int3(Utils.RoundDiv(x, d), Utils.RoundDiv(y, d), Utils.RoundDiv(z, d));
	}

	public override string ToString()
	{
		return x + "," + y + "," + z;
	}

	public string ToCsvString()
	{
		return x + " " + y + " " + z;
	}

	public static int SquareDistance(Int3 a, Int3 b)
	{
		int num = a.x - b.x;
		int num2 = a.y - b.y;
		int num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public override int GetHashCode()
	{
		return ((12289 * 31 + x) * 31 + y) * 31 + z;
	}

	public int GetMoreRandomHash()
	{
		return Utils.HashInt(Utils.HashInt(x) + Utils.HashInt(y) + Utils.HashInt(z));
	}

	public override bool Equals(object obj)
	{
		if (obj is Int3)
		{
			return Equals((Int3)obj);
		}
		return false;
	}

	public bool Equals(Int3 other)
	{
		if (x == other.x && y == other.y)
		{
			return z == other.z;
		}
		return false;
	}

	public IEnumerable<Int3> Get26Neighbors()
	{
		for (int dx = -1; dx <= 1; dx++)
		{
			for (int dy = -1; dy <= 1; dy++)
			{
				for (int dz = -1; dz <= 1; dz++)
				{
					if (dx != 0 || dy != 0 || dz != 0)
					{
						yield return new Int3(x + dx, y + dy, z + dz);
					}
				}
			}
		}
	}

	public void Write(StreamWriter writer)
	{
		writer.WriteLine(x + " " + y + " " + z);
	}

	public void BinWrite(BinaryWriter w)
	{
		w.Write(x);
		w.Write(y);
		w.Write(z);
	}

	public Int3 RotateXZ(int ccwTurns, Vector3 pivot)
	{
		Vector3 vector = ToVector3().AddScalar(0.5f);
		return Floor(Quaternion.AngleAxis((float)(4 - ccwTurns) * 90f, Vector3.up) * (vector - pivot) + pivot);
	}

	public Int3 RotateXZ(int ccwTurns)
	{
		int num = _CosTable[ccwTurns % 4];
		int num2 = _SinTable[ccwTurns % 4];
		return new Int3(num * x - num2 * z, y, num2 * x + num * z);
	}

	public static Int3 Parse(string line, char sep = ' ')
	{
		string[] array = line.Split(sep);
		if (array.Length != 3)
		{
			throw new FormatException("Error while parsing an Int3 from: " + line + ". sep = " + sep);
		}
		int num = Convert.ToInt32(array[0]);
		int num2 = Convert.ToInt32(array[1]);
		int num3 = Convert.ToInt32(array[2]);
		return new Int3(num, num2, num3);
	}

	public static bool TryParse(string line, char sep, out Int3 result)
	{
		result = zero;
		string[] array = line.Split(sep);
		if (array.Length != 3)
		{
			return false;
		}
		if (!int.TryParse(array[0], out var result2) || !int.TryParse(array[1], out var result3) || !int.TryParse(array[2], out var result4))
		{
			return false;
		}
		result = new Int3(result2, result3, result4);
		return true;
	}

	public static Int3 ParseLine(StreamReader reader)
	{
		string text = reader.ReadLine();
		if (text == null)
		{
			throw new FormatException("No line to read");
		}
		string[] array = text.Split(' ');
		if (array.Length != 3)
		{
			throw new FormatException("Did not find 3 numbers separated by a space! Line: " + text);
		}
		int num = Convert.ToInt32(array[0]);
		int num2 = Convert.ToInt32(array[1]);
		int num3 = Convert.ToInt32(array[2]);
		return new Int3(num, num2, num3);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x, y, z);
	}

	public Int3 Clamp(Int3 mins, Int3 maxs)
	{
		return Min(maxs, Max(mins, this));
	}

	public bool Within(Int3 mins, Int3 maxs)
	{
		if (this >= mins)
		{
			return this <= maxs;
		}
		return false;
	}

	public Int2 XZ()
	{
		return new Int2(x, z);
	}

	public Int2 YZ()
	{
		return new Int2(y, z);
	}

	public Int2 XY()
	{
		return new Int2(x, y);
	}

	public Int3 XY(int newZ)
	{
		return new Int3(x, y, newZ);
	}

	public Int3 XZ(int newY)
	{
		return new Int3(x, newY, z);
	}

	public Int3 X0Z()
	{
		return new Int3(x, 0, z);
	}

	public static Int3 FromXZ(Int2 xz, int y)
	{
		return new Int3(xz.x, y, xz.y);
	}

	public Int3 ZYX()
	{
		return new Int3(z, y, x);
	}

	public Int3 XY0()
	{
		return new Int3(x, y, 0);
	}

	public static Int3 Floor(Vector3 v)
	{
		return new Int3(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
	}

	public static Int3 Ceil(Vector3 v)
	{
		return new Int3(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y), Mathf.CeilToInt(v.z));
	}

	public static Int3 Floor(float x, float y, float z)
	{
		return new Int3(Mathf.FloorToInt(x), Mathf.FloorToInt(y), Mathf.FloorToInt(z));
	}

	public static Int3 Round(Vector3 v)
	{
		return new Int3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
	}

	public static Int3 Min(Int3 a, Int3 b)
	{
		return new Int3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
	}

	public static Int3 Max(Int3 a, Int3 b)
	{
		return new Int3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
	}

	public static Int3 ClosestPowerOfTwo(Int3 v)
	{
		return new Int3(Mathf.ClosestPowerOfTwo(v.x), Mathf.ClosestPowerOfTwo(v.y), Mathf.ClosestPowerOfTwo(v.z));
	}

	public static Int3 Clamp(Int3 a, Int3 min, Int3 max)
	{
		return new Int3(Mathf.Clamp(a.x, min.x, max.x), Mathf.Clamp(a.y, min.y, max.y), Mathf.Clamp(a.z, min.z, max.z));
	}

	public static Int3 Lerp(Int3 a, Int3 b, int t0, int t1, int t)
	{
		return new Int3(Utils.Lerp(a.x, b.x, t0, t1, t), Utils.Lerp(a.y, b.y, t0, t1, t), Utils.Lerp(a.z, b.z, t0, t1, t));
	}

	public bool AnyGreaterThan(int s)
	{
		if (x <= s && y <= s)
		{
			return z > s;
		}
		return true;
	}

	public bool AnyLessThan(int s)
	{
		if (x >= s && y >= s)
		{
			return z < s;
		}
		return true;
	}

	public static Int3 RandomWithin(Int3 mins, Int3 maxs, System.Random rng)
	{
		Int3 @int = maxs - mins + 1;
		return mins + new Int3(rng.Next(@int.x), rng.Next(@int.y), rng.Next(@int.z));
	}

	public static Int3 InverseTileTransform(Int3 wsPos, Int3 tileSize, Int3 wsOffset, int xzRots)
	{
		tileSize = tileSize * 2 + 1;
		Int3 @int = ((xzRots % 2 == 0) ? tileSize : tileSize.ZYX());
		return (((wsPos - wsOffset) * 2 + 1 - @int / 2).RotateXZ(4 - xzRots) + tileSize / 2 - 1) / 2;
	}

	public void Next(Int3 mins, Int3 maxs)
	{
		z++;
		if (z > maxs.z)
		{
			y++;
			if (y > maxs.y)
			{
				x++;
				y = mins.y;
			}
			z = mins.z;
		}
	}

	public static OutwardWalker3D Rings(int ringBound)
	{
		return new OutwardWalker3D(ringBound);
	}

	public static RangeEnumerator Range(Int3 mins, Int3 maxs)
	{
		return new RangeEnumerator(mins, maxs);
	}

	public static RangeEnumerator Range(Int3 upperBound)
	{
		return new RangeEnumerator(new Int3(0, 0, 0), upperBound - 1);
	}

	public static RangeEnumerator Range(int upperBound)
	{
		return new RangeEnumerator(new Int3(0, 0, 0), new Int3(upperBound - 1, upperBound - 1, upperBound - 1));
	}

	public static Bounds MinMax(Int3 min, Int3 max)
	{
		return new Bounds(min, max);
	}

	public static Bounds CenterSize(Int3 center, Int3 size)
	{
		Int3 @int = center - size / 2;
		Int3 maxs = @int + size - 1;
		return new Bounds(@int, maxs);
	}

	public static MooreEnumerator MooreNbors(Int3 center)
	{
		throw new Exception("Not tested!");
	}

	public static Int3 AxisUnit(int axis)
	{
		switch (axis)
		{
		case 0:
			return xUnit;
		case 1:
			return yUnit;
		default:
			return zUnit;
		}
	}

	public Bounds Refined(Int3 finePerCoarseCell)
	{
		return Bounds.FinerBounds(this, finePerCoarseCell);
	}

	public Bounds Refined(int finePerCoarseCell)
	{
		return Bounds.FinerBounds(this, new Int3(finePerCoarseCell));
	}

	public Bounds RingBounds(int num)
	{
		return new Bounds(this, this).Expanded(num);
	}

	public static Int3 FromRGB(Color32 c)
	{
		return new Int3(c.r, c.g, c.b);
	}

	public Color ToColor256(float alpha)
	{
		return new Color((float)x / 255f, (float)y / 255f, (float)z / 255f, alpha);
	}

	public Bounds WithOneRing()
	{
		return new Bounds(this, this).Expanded(1);
	}

	public VNNEnumerator VNNbors()
	{
		return new VNNEnumerator(this);
	}

	public static HashSet<Int3> ReadSet(string fname)
	{
		HashSet<Int3> hashSet = new HashSet<Int3>();
		if (!File.Exists(fname))
		{
			return hashSet;
		}
		using (StreamReader streamReader = FileUtils.ReadTextFile(fname))
		{
			while (true)
			{
				string text = streamReader.ReadLine();
				if (text == null || text.Trim() == "")
				{
					break;
				}
				Int3 item = Parse(text, ',');
				hashSet.Add(item);
			}
		}
		return hashSet;
	}

	public static void WriteSet(HashSet<Int3> set, string fname)
	{
		using (StreamWriter streamWriter = FileUtils.CreateTextFile(fname))
		{
			foreach (Int3 item in set)
			{
				streamWriter.WriteLine(string.Concat(item));
			}
		}
	}

	public static void WriteAll(ICollection<Int3> col, TextWriter writer)
	{
		foreach (Int3 item in col)
		{
			writer.WriteLine(string.Concat(item));
		}
	}

	public static void WriteAll(ICollection<Int3> col, string fname)
	{
		using (StreamWriter streamWriter = FileUtils.CreateTextFile(fname))
		{
			foreach (Int3 item in col)
			{
				streamWriter.WriteLine(string.Concat(item));
			}
		}
	}
}
