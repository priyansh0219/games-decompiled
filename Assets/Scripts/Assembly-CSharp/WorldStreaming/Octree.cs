using System;
using UWE;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace WorldStreaming
{
	public sealed class Octree
	{
		public const int BytesPerNode = 4;

		private readonly Int3 id;

		private NativeArray<byte> data;

		private static byte[] readBuffer = new byte[65536];

		public Octree(Int3 id)
		{
			this.id = id;
			data = SplitNativeArrayPool<byte>.emptyArray;
		}

		public void Clear(SplitNativeArrayPool<byte> allocator)
		{
			if (!(data == SplitNativeArrayPool<byte>.emptyArray))
			{
				allocator.Return(data);
				data = SplitNativeArrayPool<byte>.emptyArray;
			}
		}

		public bool IsEmpty()
		{
			if (!(data == SplitNativeArrayPool<byte>.emptyArray))
			{
				if (data.Length == 4)
				{
					return GetType(0) == 0;
				}
				return false;
			}
			return true;
		}

		private int GetNodeId(int nid, int x, int y, int z, int halfsize)
		{
			int firstChildId = GetFirstChildId(nid);
			if (firstChildId == 0)
			{
				return nid;
			}
			int x2 = ((x < halfsize) ? x : (x - halfsize));
			int y2 = ((y < halfsize) ? y : (y - halfsize));
			int z2 = ((z < halfsize) ? z : (z - halfsize));
			int nid2 = firstChildId + ((x >= halfsize) ? 4 : 0) + ((y >= halfsize) ? 2 : 0) + ((z >= halfsize) ? 1 : 0);
			return GetNodeId(nid2, x2, y2, z2, halfsize >> 1);
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

		private int GetFirstChildId(int node)
		{
			int num = node * 4;
			return (data[num + 3] << 8) + data[num + 2];
		}

		private bool IsLeaf(int node)
		{
			int num = node * 4;
			if (data == SplitNativeArrayPool<byte>.emptyArray || data.Length < num + 4)
			{
				return false;
			}
			if (data[num + 2] == 0)
			{
				return data[num + 3] == 0;
			}
			return false;
		}

		private void MakeLeaf(int node)
		{
			int num = node * 4;
			data[num + 2] = 0;
			data[num + 3] = 0;
		}

		public unsafe void Read(PooledBinaryReader reader, Int3 batchId, SplitNativeArrayPool<byte> allocator)
		{
			int num = reader.ReadUInt16() * 4;
			Clear(allocator);
			if (num == 0)
			{
				return;
			}
			data = allocator.Get(num);
			lock (readBuffer)
			{
				int num2 = 0;
				if (num <= readBuffer.Length)
				{
					num2 = reader.Read(readBuffer, 0, num);
					fixed (byte* ptr = readBuffer)
					{
						void* source = ptr;
						UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(data), source, num2);
					}
					return;
				}
				int num3 = num;
				int num4;
				do
				{
					int count = System.Math.Min(num3, readBuffer.Length);
					if ((num4 = reader.Read(readBuffer, 0, count)) > 0)
					{
						fixed (byte* ptr = readBuffer)
						{
							void* source2 = ptr;
							UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(data.GetSubArray(num2, num4)), source2, num4);
						}
						num2 += num4;
						num3 -= num4;
					}
				}
				while (num4 > 0 && num2 < num);
			}
		}

		public void UnloadChildren(int lod, SplitNativeArrayPool<byte> allocator)
		{
			if (lod >= 1 && !IsLeaf(0) && data.Length != 36)
			{
				NativeArray<byte> dst = allocator.Get(36);
				if (data != SplitNativeArrayPool<byte>.emptyArray && data.Length >= 36)
				{
					NativeArray<byte>.Copy(data, dst, 36);
				}
				Clear(allocator);
				data = dst;
				for (int i = 0; i < 8; i++)
				{
					int node = 1 + i;
					MakeLeaf(node);
				}
			}
		}

		public unsafe void RasterizeNative(int nodeId, Array3<byte> typesOut, Array3<byte> densityOut, Int3 size, Int3 w, Int3 o, int h)
		{
			if (!(data == SplitNativeArrayPool<byte>.emptyArray) && data.Length != 0)
			{
				UnityUWE.RasterizeNativeEntry(ref UnsafeUtilityEx.ArrayElementAsRef<byte>(data.GetUnsafePtr(), 0), Convert.ToUInt16(nodeId), ref typesOut.data[0], ref densityOut.data[0], size, new Int3(typesOut.sizeX, typesOut.sizeY, typesOut.sizeZ), w.x, w.y, w.z, o.x, o.y, o.z, h);
			}
		}
	}
}
