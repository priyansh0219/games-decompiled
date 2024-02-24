using System;
using System.IO;
using UWE;

public class CompactOctree
{
	public struct Node
	{
		public byte type;

		public byte density;

		public ushort firstChildId;

		public Node(VoxelandData.OctNode src, ushort firstChildId)
		{
			type = src.type;
			density = src.density;
			this.firstChildId = firstChildId;
		}

		public Node(byte type, byte density, ushort firstChildId)
		{
			this.type = type;
			this.density = density;
			this.firstChildId = firstChildId;
		}

		public VoxelandData.OctNode ToVLNode()
		{
			return new VoxelandData.OctNode(type, density);
		}
	}

	public const int FileVersion = 0;

	public const int BytesPerNode = 4;

	public IAlloc<byte> data { get; private set; }

	public int numNodes => dataCount / 4;

	public int dataCount { get; private set; }

	public bool isFullyLoaded { get; private set; }

	public static int EstimateBytes()
	{
		return (int)CommonByteArrayAllocator.EstimateBytes();
	}

	public static CompactOctree CreateFrom(VoxelandData.OctNode root)
	{
		CompactOctree compactOctree = new CompactOctree();
		compactOctree.Set(root);
		return compactOctree;
	}

	public bool IsEmpty()
	{
		if (data != null)
		{
			if (numNodes == 1)
			{
				return GetType(0) == 0;
			}
			return false;
		}
		return true;
	}

	private void SetNode(int id, byte type, byte density, ushort firstChildId)
	{
		int num = id * 4;
		data[num] = type;
		data[num + 1] = density;
		data[num + 2] = Convert.ToByte(firstChildId & 0xFF);
		data[num + 3] = Convert.ToByte(firstChildId >> 8);
	}

	private Node GetNode(int id)
	{
		int num = id * 4;
		return new Node(data[num], data[num + 1], Convert.ToUInt16((data[num + 3] << 8) + data[num + 2]));
	}

	public bool UsesType(byte type)
	{
		for (int i = 0; i < numNodes; i++)
		{
			if (GetType(i) == type)
			{
				return true;
			}
		}
		return false;
	}

	public void TabulateTypeUsage(int[] type2count)
	{
		for (int i = 0; i < numNodes; i++)
		{
			byte type = GetType(i);
			type2count[type]++;
		}
	}

	private int GetNodeId(int nid, int x, int y, int z, int halfsize)
	{
		int firstChildId = GetNode(nid).firstChildId;
		if (firstChildId == 0)
		{
			return nid;
		}
		if (x < halfsize)
		{
			if (y < halfsize)
			{
				if (z < halfsize)
				{
					return GetNodeId(firstChildId, x, y, z, halfsize >> 1);
				}
				return GetNodeId(firstChildId + 1, x, y, z - halfsize, halfsize >> 1);
			}
			if (z < halfsize)
			{
				return GetNodeId(firstChildId + 2, x, y - halfsize, z, halfsize >> 1);
			}
			return GetNodeId(firstChildId + 3, x, y - halfsize, z - halfsize, halfsize >> 1);
		}
		if (y < halfsize)
		{
			if (z < halfsize)
			{
				return GetNodeId(firstChildId + 4, x - halfsize, y, z, halfsize >> 1);
			}
			return GetNodeId(firstChildId + 5, x - halfsize, y, z - halfsize, halfsize >> 1);
		}
		if (z < halfsize)
		{
			return GetNodeId(firstChildId + 6, x - halfsize, y - halfsize, z, halfsize >> 1);
		}
		return GetNodeId(firstChildId + 7, x - halfsize, y - halfsize, z - halfsize, halfsize >> 1);
	}

	public int GetNodeId(Int3 coords, int treeSize)
	{
		return GetNodeId(0, coords.x, coords.y, coords.z, treeSize >> 1);
	}

	public byte GetType(int node)
	{
		return data[node * 4];
	}

	public byte GetDensity(int node)
	{
		return data[node * 4 + 1];
	}

	public int GetFirstChildId(int node)
	{
		int num = node * 4;
		return (data[num + 3] << 8) + data[num + 2];
	}

	public bool IsLeaf(int node)
	{
		int num = node * 4;
		if (data[num + 2] == 0)
		{
			return data[num + 3] == 0;
		}
		return false;
	}

	private IAlloc<byte> GetByteArray(int size)
	{
		return CommonByteArrayAllocator.Allocate(size);
	}

	public void Read(PoolingBinaryReader r, int version, Int3 batchId, Int3 treeId)
	{
		int size = r.ReadUInt16() * 4;
		data = GetByteArray(size);
		dataCount = size;
		r.Read(data.Array, data.Offset, dataCount);
		isFullyLoaded = true;
	}

	public void Read(BinaryReader r, int version, Int3 batchId, Int3 treeId)
	{
		int size = r.ReadUInt16() * 4;
		data = GetByteArray(size);
		dataCount = size;
		r.Read(data.Array, data.Offset, dataCount);
		isFullyLoaded = true;
	}

	public void UnloadChildren()
	{
		isFullyLoaded = false;
		if (IsLeaf(0))
		{
			return;
		}
		bool flag = true;
		int firstChildId = GetFirstChildId(0);
		for (int i = 0; i < 8; i++)
		{
			if (!IsLeaf(firstChildId + i))
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			IAlloc<byte> destination = CommonByteArrayAllocator.Allocate(36);
			ArrayAllocator<byte>.Alloc.CopyTo(data, destination, 36);
			CommonByteArrayAllocator.Free(data);
			data = destination;
			dataCount = 36;
			for (int j = 0; j < 8; j++)
			{
				int num = firstChildId + j;
				byte type = GetType(num);
				byte density = GetDensity(num);
				SetNode(num, type, density, 0);
			}
		}
	}

	public void NotifyUnload()
	{
		CommonByteArrayAllocator.Free(data);
		data = null;
		dataCount = 0;
	}

	public void Write(BinaryWriter w)
	{
		w.Write(Convert.ToUInt16(numNodes));
		w.Write(data.Array, data.Offset, dataCount);
	}

	private void SetInternal(VoxelandData.OctNode node, int nodeId, ref ushort nextFreeId)
	{
		if (node.IsLeaf())
		{
			SetNode(nodeId, node.type, node.density, 0);
			return;
		}
		ushort num = nextFreeId;
		SetNode(nodeId, node.type, node.density, num);
		nextFreeId += 8;
		for (int i = 0; i < 8; i++)
		{
			SetInternal(node.childNodes[i], num + i, ref nextFreeId);
		}
	}

	public void Set(VoxelandData.OctNode root)
	{
		int size = root.CountNodes() * 4;
		data = GetByteArray(size);
		dataCount = size;
		ushort nextFreeId = 1;
		SetInternal(root, 0, ref nextFreeId);
	}

	public int SumTypes()
	{
		int num = 0;
		for (int i = 0; i < numNodes; i++)
		{
			num += GetType(i);
		}
		return num;
	}

	private VoxelandData.OctNode ToVLOctNodeRecursive(int nid)
	{
		Node node = GetNode(nid);
		VoxelandData.OctNode result = node.ToVLNode();
		if (!IsLeaf(nid))
		{
			result.childNodes = VoxelandData.OctNode.childNodesPool.Get();
			for (int i = 0; i < 8; i++)
			{
				result.childNodes[i] = ToVLOctNodeRecursive(node.firstChildId + i);
			}
		}
		return result;
	}

	public VoxelandData.OctNode ToVLOctree()
	{
		return ToVLOctNodeRecursive(0);
	}

	public void Rasterize(Array3<byte> typesOut, Array3<byte> densityOut, Int3 size, int wx, int wy, int wz, int ox, int oy, int oz, int h)
	{
		Rasterize(0, typesOut, densityOut, size, wx, wy, wz, ox, oy, oz, h);
	}

	public void Rasterize(int nodeId, Array3<byte> typesOut, Array3<byte> densityOut, Int3 size, int wx, int wy, int wz, int ox, int oy, int oz, int h)
	{
		Node node = GetNode(nodeId);
		if (node.firstChildId == 0 || h == 0)
		{
			int num = ((h == 0) ? 1 : (2 * h));
			int num2 = ((ox > wx) ? (ox - wx) : 0);
			int num3 = ((oy > wy) ? (oy - wy) : 0);
			int num4 = ((oz > wz) ? (oz - wz) : 0);
			int num5 = System.Math.Min(ox + num - wx, size.x);
			int num6 = System.Math.Min(oy + num - wy, size.y);
			int num7 = System.Math.Min(oz + num - wz, size.z);
			for (int i = num2; i < num5; i++)
			{
				for (int j = num3; j < num6; j++)
				{
					for (int k = num4; k < num7; k++)
					{
						typesOut[i, j, k] = node.type;
						densityOut[i, j, k] = node.density;
					}
				}
			}
			return;
		}
		int num8 = wx + size.x - 1;
		int num9 = wy + size.y - 1;
		int num10 = wz + size.z - 1;
		int h2 = h >> 1;
		for (int l = 0; l < 8; l++)
		{
			int num11 = ox + h * VoxelandData.OctNode.ChildDX[l];
			int num12 = oy + h * VoxelandData.OctNode.ChildDY[l];
			int num13 = oz + h * VoxelandData.OctNode.ChildDZ[l];
			int num14 = num11 + h - 1;
			int num15 = num12 + h - 1;
			int num16 = num13 + h - 1;
			if (num14 >= wx && num11 <= num8 && num15 >= wy && num12 <= num9 && num16 >= wz && num13 <= num10)
			{
				int nodeId2 = node.firstChildId + l;
				Rasterize(nodeId2, typesOut, densityOut, size, wx, wy, wz, num11, num12, num13, h2);
			}
		}
	}

	private static void RasterizeNativeEntry(ref byte nodes, ushort nodeId, ref byte typesOut, ref byte densityOut, Int3 usedSize, Int3 gridSize, int wx, int wy, int wz, int ox, int oy, int oz, int h)
	{
		UnityUWE.RasterizeNativeEntry(ref nodes, nodeId, ref typesOut, ref densityOut, usedSize, gridSize, wx, wy, wz, ox, oy, oz, h);
	}

	public void RasterizeNative(int nodeId, Array3<byte> typesOut, Array3<byte> densityOut, Int3 size, int wx, int wy, int wz, int ox, int oy, int oz, int h)
	{
		if (dataCount != 0)
		{
			RasterizeNativeEntry(ref data.Array[data.Offset], Convert.ToUInt16(nodeId), ref typesOut.data[0], ref densityOut.data[0], size, typesOut.Dims(), wx, wy, wz, ox, oy, oz, h);
		}
	}

	public bool EqualTo(CompactOctree other)
	{
		if (other.dataCount != dataCount)
		{
			return false;
		}
		for (int i = 0; i < dataCount; i++)
		{
			if (other.data[i] != data[i])
			{
				return false;
			}
		}
		return true;
	}
}
