using System;
using Unity.Mathematics;
using UnityEngine;

public static class VoxelandMisc
{
	[Serializable]
	public class BoxInput : IVoxelGrid
	{
		public byte type;

		public Bounds bounds;

		public BoxInput()
		{
		}

		public BoxInput(byte type, Vector3 mins, Vector3 maxs)
		{
			this.type = type;
			bounds.SetMinMax(mins, maxs);
		}

		public VoxelandData.OctNode GetVoxel(int x, int y, int z)
		{
			Vector3 p = new Vector3(x, y, z) + Voxeland.half3;
			float dist = SignedDistToBox(bounds, p);
			VoxelandData.OctNode result = default(VoxelandData.OctNode);
			result.type = type;
			result.density = VoxelandData.OctNode.EncodeDensity(dist);
			return result;
		}

		public bool GetVoxelMask(int x, int y, int z)
		{
			Bounds bounds = default(Bounds);
			bounds.SetMinMax(this.bounds.min - new Vector3(1f, 1f, 1f), this.bounds.max + new Vector3(1f, 1f, 1f));
			return bounds.Contains(new Vector3((float)x + 0.5f, (float)y + 0.5f, (float)z + 0.5f));
		}
	}

	public static float SignedDistToBox(Bounds bounds, Vector3 p)
	{
		float num = bounds.SqrDistance(p);
		if (num <= 0f)
		{
			float a = p.x - bounds.min.x;
			float a2 = bounds.max.x - p.x;
			float a3 = p.y - bounds.min.y;
			float a4 = bounds.max.y - p.y;
			float a5 = p.z - bounds.min.z;
			float b = bounds.max.z - p.z;
			return Mathf.Min(a, Mathf.Min(a2, Mathf.Min(a3, Mathf.Min(a4, Mathf.Min(a5, b)))));
		}
		return Mathf.Sqrt(num) * -1f;
	}

	public static Unity.Mathematics.Random CreateRandom(int seed)
	{
		return new Unity.Mathematics.Random((seed == 0) ? 1u : ((uint)seed));
	}
}
