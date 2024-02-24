using System;
using UnityEngine;

[Serializable]
public class VoxelandCoords
{
	public int x;

	public int y;

	public int z;

	public int dir;

	public VoxelandCoords(int nx, int ny, int nz)
	{
		x = nx;
		y = ny;
		z = nz;
	}

	public VoxelandCoords(int nx, int ny, int nz, int d)
	{
		x = nx;
		y = ny;
		z = nz;
		dir = d;
	}

	public VoxelandCoords(VoxelandCoords src)
	{
		x = src.x;
		y = src.y;
		z = src.z;
		dir = src.dir;
	}

	public VoxelandCoords(VoxelandChunk chunk, VoxelandChunk.VoxelandFace face)
	{
		x = chunk.offsetX + face.block.x - 2;
		y = chunk.offsetY + face.block.y - 2;
		z = chunk.offsetZ + face.block.z - 2;
		dir = face.dir;
	}

	public void Clamp(VoxelandCoords max)
	{
		x = Mathf.Clamp(x, 0, max.x);
		y = Mathf.Clamp(y, 0, max.y);
		z = Mathf.Clamp(z, 0, max.z);
	}

	public static bool Equals(VoxelandCoords c1, VoxelandCoords c2)
	{
		if (c1.x == c2.x && c1.y == c2.y && c1.z == c2.z)
		{
			return c1.dir == c2.dir;
		}
		return false;
	}

	public static bool EqualPos(VoxelandCoords c1, VoxelandCoords c2)
	{
		if (c1.x == c2.x && c1.y == c2.y)
		{
			return c1.z == c2.z;
		}
		return false;
	}

	public VoxelandCoords GetOpposite()
	{
		switch (dir)
		{
		case 0:
			return new VoxelandCoords(x, y + 1, z, 1);
		case 1:
			return new VoxelandCoords(x, y - 1, z, 0);
		case 2:
			return new VoxelandCoords(x + 1, y, z, 3);
		case 3:
			return new VoxelandCoords(x - 1, y, z, 2);
		case 4:
			return new VoxelandCoords(x, y, z - 1, 5);
		case 5:
			return new VoxelandCoords(x, y, z + 1, 4);
		default:
			return new VoxelandCoords(x, y, z, 0);
		}
	}

	public VoxelandCoords[] GetNeigs(Vector3 range, VoxelandData data)
	{
		int num = Mathf.Max(Mathf.FloorToInt((float)x - range.x), 0);
		int num2 = Mathf.Max(Mathf.FloorToInt((float)y - range.y), 0);
		int num3 = Mathf.Max(Mathf.FloorToInt((float)z - range.z), 0);
		int num4 = Mathf.Min(Mathf.CeilToInt((float)x + range.x), data.sizeX - 1);
		int num5 = Mathf.Min(Mathf.CeilToInt((float)y + range.y), data.sizeY - 1);
		int num6 = Mathf.Min(Mathf.CeilToInt((float)z + range.z), data.sizeZ - 1);
		if (num5 == num2)
		{
			num5++;
		}
		VoxelandCoords[] array = new VoxelandCoords[(num4 - num + 1) * (num5 - num2 + 1) * (num6 - num3 + 1)];
		array[0] = this;
		int num7 = 1;
		for (int i = num; i <= num4; i++)
		{
			for (int j = num2; j <= num5; j++)
			{
				for (int k = num3; k <= num6; k++)
				{
					if (i != x || j != y || k != z)
					{
						array[num7] = new VoxelandCoords(i, j, k, dir);
						num7++;
					}
				}
			}
		}
		foreach (VoxelandCoords voxelandCoords in array)
		{
			voxelandCoords.dir = ((voxelandCoords.x > num && voxelandCoords.x < num4 && voxelandCoords.y > num2 && voxelandCoords.y < num5 && voxelandCoords.z > num3 && voxelandCoords.z < num6) ? (-1) : 0);
		}
		return array;
	}

	public VoxelandCoords[] GetSphereNeigs(Vector3 range, VoxelandData data)
	{
		int num = Mathf.Max(Mathf.FloorToInt((float)x - range.x), 0);
		int num2 = Mathf.Max(Mathf.FloorToInt((float)y - range.y), 0);
		int num3 = Mathf.Max(Mathf.FloorToInt((float)z - range.z), 0);
		int num4 = Mathf.Min(Mathf.CeilToInt((float)x + range.x), data.sizeX - 1);
		int num5 = Mathf.Min(Mathf.CeilToInt((float)y + range.y), data.sizeY - 1);
		int num6 = Mathf.Min(Mathf.CeilToInt((float)z + range.z), data.sizeZ - 1);
		if (num5 == num2)
		{
			num5++;
		}
		Vector3 b = new Vector3(1f / range.x, 1f / range.y, 1f / range.z);
		float num7 = 1f / (range.x * range.z);
		int num8 = 0;
		for (int i = num; i <= num4; i++)
		{
			for (int j = num2; j <= num5; j++)
			{
				for (int k = num3; k <= num6; k++)
				{
					if (Vector3.Scale(new Vector3(i - x, j - y, k - z), b).sqrMagnitude - num7 <= 1f)
					{
						num8++;
					}
				}
			}
		}
		VoxelandCoords[] array = new VoxelandCoords[num8];
		array[0] = this;
		int num9 = 1;
		for (int l = num; l <= num4; l++)
		{
			for (int m = num2; m <= num5; m++)
			{
				for (int n = num3; n <= num6; n++)
				{
					if ((l != x || m != y || n != z) && Vector3.Scale(new Vector3(l - x, m - y, n - z), b).sqrMagnitude - num7 <= 1f)
					{
						array[num9] = new VoxelandCoords(l, m, n, 0);
						num9++;
					}
				}
			}
		}
		return array;
	}

	public VoxelandCoords[] GetCylinderNeigs(Vector3 range, VoxelandData data)
	{
		int num = Mathf.Max(Mathf.FloorToInt((float)x - range.x), 0);
		int num2 = Mathf.Max(Mathf.FloorToInt((float)y - range.y), 0);
		int num3 = Mathf.Max(Mathf.FloorToInt((float)z - range.z), 0);
		int num4 = Mathf.Min(Mathf.CeilToInt((float)x + range.x), data.sizeX - 1);
		int num5 = Mathf.Min(Mathf.CeilToInt((float)y + range.y), data.sizeY - 1);
		int num6 = Mathf.Min(Mathf.CeilToInt((float)z + range.z), data.sizeZ - 1);
		if (num5 == num2)
		{
			num5++;
		}
		Vector2 b = new Vector2(1f / range.x, 1f / range.z);
		float num7 = 0f;
		int num8 = 0;
		for (int i = num; i <= num4; i++)
		{
			for (int j = num2; j <= num5; j++)
			{
				for (int k = num3; k <= num6; k++)
				{
					if (Vector2.Scale(new Vector2(i - x, k - z), b).sqrMagnitude - num7 <= 1f)
					{
						num8++;
					}
				}
			}
		}
		VoxelandCoords[] array = new VoxelandCoords[num8];
		array[0] = this;
		int num9 = 1;
		for (int l = num; l <= num4; l++)
		{
			for (int m = num2; m <= num5; m++)
			{
				for (int n = num3; n <= num6; n++)
				{
					if ((l != x || m != y || n != z) && Vector2.Scale(new Vector2(l - x, n - z), b).sqrMagnitude - num7 <= 1f)
					{
						array[num9] = new VoxelandCoords(l, m, n, 0);
						num9++;
					}
				}
			}
		}
		return array;
	}

	public VoxelandCoords[] GetAABBNeigs(Bounds bounds, Quaternion rotation, Vector3 scale, VoxelandData data)
	{
		Vector3 v = Vector3.Scale(bounds.extents, scale);
		Vector3 vector = rotation * bounds.center;
		Vector3 vector2 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector3 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		foreach (Int3 item in Int3.Range(Int3.zero, Int3.one))
		{
			Vector3 vector4 = Int3.Scale(2 * item - 1, v);
			Vector3 rhs = rotation * vector4;
			vector2 = Vector3.Min(vector2, rhs);
			vector3 = Vector3.Max(vector3, rhs);
		}
		Int3 @int = new Int3(x, y, z);
		Int3.Bounds bounds2 = new Int3.Bounds(Int3.Floor(vector + vector2) + @int, Int3.Ceil(vector + vector3) + @int);
		int inclusiveVolume = bounds2.GetInclusiveVolume();
		VoxelandCoords[] array = new VoxelandCoords[Mathf.Max(1, inclusiveVolume)];
		int num = 0;
		if (bounds2.Contains(@int))
		{
			array[num++] = this;
		}
		foreach (Int3 item2 in bounds2)
		{
			if (!(item2 == @int))
			{
				array[num++] = new VoxelandCoords(item2.x, item2.y, item2.z, 0);
			}
		}
		return array;
	}

	public bool IsSameBlock(VoxelandCoords other)
	{
		if (x == other.x && y == other.y)
		{
			return z == other.z;
		}
		return false;
	}

	public override string ToString()
	{
		return "(" + x + "," + y + "," + z + "," + dir + ")";
	}

	public Vector3 GetCenter()
	{
		return new Vector3(x, y, z) + Voxeland.half3;
	}

	public VoxelandCoords GetNegativeNeighbor()
	{
		return new VoxelandCoords(x - 1, y - 1, z - 1);
	}

	public VoxelandCoords GetPositiveNeighbor()
	{
		return new VoxelandCoords(x + 1, y + 1, z + 1);
	}

	public Int3 ToInt3()
	{
		return new Int3(x, y, z);
	}
}
