using System;
using UnityEngine;

namespace UWE
{
	public struct CubeFace : IEquatable<CubeFace>
	{
		public enum Names
		{
			Left = 0,
			Right = 1,
			Top = 2,
			Bottom = 3,
			Front = 4,
			Back = 5
		}

		public int face;

		public static Int3[] Normal = new Int3[6]
		{
			new Int3(-1, 0, 0),
			new Int3(1, 0, 0),
			new Int3(0, 1, 0),
			new Int3(0, -1, 0),
			new Int3(0, 0, -1),
			new Int3(0, 0, 1)
		};

		public static Int3[] Tangent = new Int3[6]
		{
			new Int3(0, 0, -1),
			new Int3(0, 0, 1),
			new Int3(1, 0, 0),
			new Int3(1, 0, 0),
			new Int3(1, 0, 0),
			new Int3(-1, 0, 0)
		};

		public static Int3[] Bitangent = new Int3[6]
		{
			new Int3(0, 1, 0),
			new Int3(0, 1, 0),
			new Int3(0, 0, 1),
			new Int3(0, 0, -1),
			new Int3(0, 1, 0),
			new Int3(0, 1, 0)
		};

		public static Int3[] Origin = new Int3[6]
		{
			new Int3(0, 0, 1),
			new Int3(1, 0, 0),
			new Int3(0, 1, 0),
			new Int3(0, 0, 1),
			new Int3(0, 0, 0),
			new Int3(1, 0, 1)
		};

		public static int[] Opposite = new int[6] { 1, 0, 3, 2, 5, 4 };

		public static string[] Name = new string[6] { "-X", "+X", "+Y", "-Y", "-Z", "+Z" };

		public Int3 normal => Normal[face];

		public Int3 tangent => Tangent[face];

		public Int3 bitangent => Bitangent[face];

		public Int3 origin => Origin[face];

		public CubeFace opposite => new CubeFace(Opposite[face]);

		public CubeFace(int face)
		{
			this.face = face;
		}

		public CubeFace RotateXZ(int turns)
		{
			Int3 @int = normal.RotateXZ(turns);
			for (int i = 0; i < 6; i++)
			{
				if (Normal[i] == @int)
				{
					return new CubeFace(i);
				}
			}
			Debug.LogError("CubeFace.TurnXZ failed");
			return new CubeFace(0);
		}

		public Int2 GetSize(Int3 boxSize)
		{
			return new Int2(Mathf.Abs(Int3.Dot(tangent, boxSize)), Mathf.Abs(Int3.Dot(bitangent, boxSize)));
		}

		public override string ToString()
		{
			return Name[face];
		}

		public override int GetHashCode()
		{
			return face.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is CubeFace)
			{
				return Equals((CubeFace)obj);
			}
			return false;
		}

		public bool Equals(CubeFace other)
		{
			return face == other.face;
		}

		public static Quaternion GetQuat(int f)
		{
			return Quaternion.LookRotation(Normal[f].ToVector3(), Tangent[f].ToVector3());
		}
	}
}
