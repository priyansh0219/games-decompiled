using System;
using UnityEngine;

namespace Verse
{
	public static class IntVec3Utility
	{
		public static IntVec3 ToIntVec3(this Vector3 vect)
		{
			return new IntVec3(vect);
		}

		public static float DistanceTo(this IntVec3 a, IntVec3 b)
		{
			return (a - b).LengthHorizontal;
		}

		public static int DistanceToSquared(this IntVec3 a, IntVec3 b)
		{
			return (a - b).LengthHorizontalSquared;
		}

		public static IntVec3 RotatedBy(this IntVec3 orig, RotationDirection rot)
		{
			switch (rot)
			{
			case RotationDirection.None:
				return orig;
			case RotationDirection.Clockwise:
				return new IntVec3(orig.z, orig.y, -orig.x);
			case RotationDirection.Opposite:
				return new IntVec3(-orig.x, orig.y, -orig.z);
			case RotationDirection.Counterclockwise:
				return new IntVec3(-orig.z, orig.y, orig.x);
			default:
				throw new NotImplementedException();
			}
		}

		public static IntVec3 RotatedBy(this IntVec3 orig, Rot4 rot)
		{
			switch (rot.AsInt)
			{
			case 0:
				return orig;
			case 1:
				return new IntVec3(orig.z, orig.y, -orig.x);
			case 2:
				return new IntVec3(-orig.x, orig.y, -orig.z);
			case 3:
				return new IntVec3(-orig.z, orig.y, orig.x);
			default:
				return orig;
			}
		}

		public static IntVec3 Inverse(this IntVec3 orig)
		{
			return new IntVec3(-orig.x, -orig.y, -orig.z);
		}

		public static int ManhattanDistanceFlat(IntVec3 a, IntVec3 b)
		{
			return Math.Abs(a.x - b.x) + Math.Abs(a.z - b.z);
		}

		public static IntVec3 RandomHorizontalOffset(float maxDist)
		{
			return Vector3Utility.RandomHorizontalOffset(maxDist).ToIntVec3();
		}

		public static int DistanceToEdge(this IntVec3 v, Map map)
		{
			return Mathf.Max(Mathf.Min(Mathf.Min(Mathf.Min(v.x, v.z), map.Size.x - v.x - 1), map.Size.z - v.z - 1), 0);
		}

		public static int Determinant(this IntVec3 p, IntVec3 p1, IntVec3 p2)
		{
			int num = (p2.x - p1.x) * (p.z - p1.z) - (p.x - p1.x) * (p2.z - p1.z);
			if (num > 0)
			{
				return -1;
			}
			if (num < 0)
			{
				return 1;
			}
			return 0;
		}
	}
}
