using System;
using System.Collections.Generic;
using System.IO;
using UWE;
using UnityEngine;

[Serializable]
public class VoxelandData : ScriptableObject, IVoxelGrid
{
	public interface EventHandler
	{
		void OnRootChanged(int rx, int ry, int rz);
	}

	[Serializable]
	public struct VoxelandAABB
	{
		public int x;

		public int y;

		public int z;

		public int sx;

		public int sy;

		public int sz;

		public bool IntersectsNode(int ofsX, int ofsY, int ofsZ, int halfsize)
		{
			if (x + sx < ofsX || y + sy < ofsY || z + sz < ofsZ || x > ofsX + halfsize * 2 || y > ofsY + halfsize * 2 || z > ofsZ + halfsize * 2)
			{
				return false;
			}
			return true;
		}
	}

	public struct OctNode
	{
		public interface Visitor
		{
			void Visit(OctNode node, int x, int y, int z, int h);
		}

		public enum BlendOp
		{
			Union = 0,
			Intersection = 1,
			Subtraction = 2,
			Overwrite = 3
		}

		[Serializable]
		public struct BlendArgs
		{
			public static readonly BlendArgs Add = new BlendArgs(BlendOp.Union, replaceTypes: false, 0);

			public static readonly BlendArgs Replace = new BlendArgs(BlendOp.Overwrite, replaceTypes: true, 0);

			public BlendOp operation;

			public bool replaceTypes;

			public byte replaceExceptionType;

			public BlendArgs(BlendOp operation, bool replaceTypes, byte replaceExceptionType)
			{
				this.operation = operation;
				this.replaceTypes = replaceTypes;
				this.replaceExceptionType = replaceExceptionType;
			}

			public static BlendArgs NewPassiveAdditive()
			{
				return new BlendArgs(BlendOp.Union, replaceTypes: false, 0);
			}

			public static BlendArgs NewPassiveSubtractive()
			{
				return new BlendArgs(BlendOp.Subtraction, replaceTypes: false, 0);
			}
		}

		public byte type;

		public byte density;

		public OctNode[] childNodes;

		public const int SizeBytes = 8;

		public static readonly ObjectPool<OctNode[]> childNodesPool = ObjectPoolHelper.CreateFixedArrayPool<OctNode>("OctNode::childNodesPool", 1024, 8);

		public const long bytesPerChildNodesArray = 76L;

		public static readonly int[] ChildDX = new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 };

		public static readonly int[] ChildDY = new int[8] { 0, 0, 1, 1, 0, 0, 1, 1 };

		public static readonly int[] ChildDZ = new int[8] { 0, 1, 0, 1, 0, 1, 0, 1 };

		private static ObjectPool<int[]> child2CountPool = ObjectPoolHelper.CreateFixedArrayPool<int>("OctNode::child2CountPool", 32, 8);

		public bool IsEmpty()
		{
			if (type == 0)
			{
				return childNodes == null;
			}
			return false;
		}

		public bool IsLeaf()
		{
			if (childNodes != null)
			{
				return childNodes.Length == 0;
			}
			return true;
		}

		public static OctNode Blend(OctNode n0, OctNode n1, BlendArgs args)
		{
			OctNode result = default(OctNode);
			float a = ((!n0.IsNearSurface()) ? ((n0.type == 0) ? (-2f) : 2f) : n0.GetDecodedNearDensity());
			float num = ((!n1.IsNearSurface()) ? ((n1.type == 0) ? (-2f) : 2f) : n1.GetDecodedNearDensity());
			float num2;
			switch (args.operation)
			{
			case BlendOp.Union:
				num2 = Mathf.Max(a, num);
				break;
			case BlendOp.Intersection:
				num2 = Mathf.Min(a, num);
				break;
			case BlendOp.Subtraction:
				num2 = Mathf.Min(a, 0f - num);
				break;
			case BlendOp.Overwrite:
				num2 = num;
				break;
			default:
				num2 = num;
				break;
			}
			if (num2 < 0f)
			{
				result.type = 0;
			}
			else if (args.replaceTypes)
			{
				if (n1.type != args.replaceExceptionType)
				{
					result.type = n1.type;
				}
				else
				{
					result.type = n0.type;
				}
			}
			else if (n0.type == 0)
			{
				result.type = n1.type;
			}
			else
			{
				result.type = n0.type;
			}
			result.density = EncodeDensity(num2);
			return result;
		}

		public static float DecodeNearDensity(byte d)
		{
			if (d == 0)
			{
				throw new Exception("DecodeNearDensity called for a 'far' voxel");
			}
			return ((float)(int)d - 126f) / 126f;
		}

		public override string ToString()
		{
			if (IsNearSurface())
			{
				return $"type {type}, dist {DecodeNearDensity(density)}, leaf {IsLeaf()}";
			}
			string arg = ((type == 0) ? "out" : "in");
			return $"type {type}, dist far {arg}, leaf {IsLeaf()}";
		}

		public static byte EncodeDensity(float dist)
		{
			if (Mathf.Abs(dist) > 1f)
			{
				return 0;
			}
			return Convert.ToByte(Mathf.FloorToInt(dist * 126f) + 126);
		}

		public float GetDecodedNearDensity()
		{
			return DecodeNearDensity(density);
		}

		public bool IsNearSurface()
		{
			return density != 0;
		}

		public static bool IsBelowSurface(byte type, byte density)
		{
			if (density == 0)
			{
				return type > 0;
			}
			return density >= 126;
		}

		public bool IsNearAndBelowSurface()
		{
			if (type > 0)
			{
				return density >= 126;
			}
			return false;
		}

		public bool IsBelowSurface()
		{
			return IsBelowSurface(type, density);
		}

		[Obsolete("Proven buggy. Sometimes far away voxels end up having non-zero density < 126", true)]
		public static bool IsFarAboveSurface(byte type, byte density)
		{
			if (type == 0)
			{
				return density == 0;
			}
			return false;
		}

		public OctNode(byte type, byte density)
		{
			this.type = type;
			this.density = density;
			childNodes = null;
		}

		public static OctNode EmptyNode()
		{
			return new OctNode(0, 0);
		}

		public void Clear()
		{
			ReleaseChildren();
			type = 0;
			density = 0;
		}

		public static long GetPoolBytesUsed()
		{
			return (long)childNodesPool.numOutstanding * 76L;
		}

		public static long GetPoolBytesTotal()
		{
			return (long)childNodesPool.totalAllocated * 76L;
		}

		public static void ClearPool()
		{
			childNodesPool.Reset();
		}

		private void ReleaseChildren()
		{
			if (childNodes != null)
			{
				for (int i = 0; i < childNodes.Length; i++)
				{
					childNodes[i].ReleaseChildren();
				}
				Array.Clear(childNodes, 0, childNodes.Length);
				childNodesPool.Return(childNodes);
				childNodes = null;
			}
		}

		public void AcquireChildren()
		{
			if (childNodes == null)
			{
				childNodes = childNodesPool.Get();
			}
			if (childNodes == null)
			{
				throw new Exception("Did not successfully acquire an octnode block!");
			}
		}

		public int EstimateBytes()
		{
			int num = 8;
			if (childNodes != null)
			{
				for (int i = 0; i < childNodes.Length; i++)
				{
					num += childNodes[i].EstimateBytes();
				}
			}
			return num;
		}

		public int CountNodes()
		{
			int num = 1;
			if (childNodes != null)
			{
				for (int i = 0; i < childNodes.Length; i++)
				{
					num += childNodes[i].CountNodes();
				}
			}
			return num;
		}

		public int SumTypes()
		{
			int num = type;
			if (childNodes != null)
			{
				for (int i = 0; i < childNodes.Length; i++)
				{
					num += childNodes[i].SumTypes();
				}
			}
			return num;
		}

		public int CountEdges()
		{
			int num = 0;
			if (childNodes != null)
			{
				num += 8;
				for (int i = 0; i < childNodes.Length; i++)
				{
					num += childNodes[i].CountEdges();
				}
			}
			return num;
		}

		public OctNode GetNode(int x, int y, int z, int halfsize)
		{
			if (childNodes == null || childNodes.Length == 0)
			{
				return this;
			}
			if (x < halfsize)
			{
				if (y < halfsize)
				{
					if (z < halfsize)
					{
						return childNodes[0].GetNode(x, y, z, halfsize >> 1);
					}
					return childNodes[1].GetNode(x, y, z - halfsize, halfsize >> 1);
				}
				if (z < halfsize)
				{
					return childNodes[2].GetNode(x, y - halfsize, z, halfsize >> 1);
				}
				return childNodes[3].GetNode(x, y - halfsize, z - halfsize, halfsize >> 1);
			}
			if (y < halfsize)
			{
				if (z < halfsize)
				{
					return childNodes[4].GetNode(x - halfsize, y, z, halfsize >> 1);
				}
				return childNodes[5].GetNode(x - halfsize, y, z - halfsize, halfsize >> 1);
			}
			if (z < halfsize)
			{
				return childNodes[6].GetNode(x - halfsize, y - halfsize, z, halfsize >> 1);
			}
			return childNodes[7].GetNode(x - halfsize, y - halfsize, z - halfsize, halfsize >> 1);
		}

		public void SetNode(int x, int y, int z, int halfsize, byte newType, byte newDensity)
		{
			if (halfsize == 0)
			{
				type = newType;
				density = newDensity;
			}
			else
			{
				if (childNodes == null && type == newType && density == newDensity)
				{
					return;
				}
				if (childNodes == null || childNodes.Length == 0)
				{
					AcquireChildren();
					for (int i = 0; i < 8; i++)
					{
						childNodes[i].type = type;
						childNodes[i].density = density;
					}
				}
				if (x < halfsize)
				{
					if (y < halfsize)
					{
						if (z < halfsize)
						{
							childNodes[0].SetNode(x, y, z, halfsize / 2, newType, newDensity);
						}
						else
						{
							childNodes[1].SetNode(x, y, z - halfsize, halfsize / 2, newType, newDensity);
						}
					}
					else if (z < halfsize)
					{
						childNodes[2].SetNode(x, y - halfsize, z, halfsize / 2, newType, newDensity);
					}
					else
					{
						childNodes[3].SetNode(x, y - halfsize, z - halfsize, halfsize / 2, newType, newDensity);
					}
				}
				else if (y < halfsize)
				{
					if (z < halfsize)
					{
						childNodes[4].SetNode(x - halfsize, y, z, halfsize / 2, newType, newDensity);
					}
					else
					{
						childNodes[5].SetNode(x - halfsize, y, z - halfsize, halfsize / 2, newType, newDensity);
					}
				}
				else if (z < halfsize)
				{
					childNodes[6].SetNode(x - halfsize, y - halfsize, z, halfsize / 2, newType, newDensity);
				}
				else
				{
					childNodes[7].SetNode(x - halfsize, y - halfsize, z - halfsize, halfsize / 2, newType, newDensity);
				}
			}
		}

		public int GetTopPoint(int x, int z, int halfsize)
		{
			if (childNodes == null || childNodes.Length == 0)
			{
				if (type > 0)
				{
					return 0;
				}
				return -1;
			}
			int num = 0;
			int num2 = 0;
			if (x < halfsize)
			{
				if (z < halfsize)
				{
					num2 = 0;
					num = 2;
				}
				else
				{
					num2 = 1;
					num = 3;
					z -= halfsize;
				}
			}
			else if (z < halfsize)
			{
				num2 = 4;
				num = 6;
				x -= halfsize;
			}
			else
			{
				num2 = 5;
				num = 7;
				x -= halfsize;
				z -= halfsize;
			}
			int topPoint = childNodes[num].GetTopPoint(x, z, halfsize / 2);
			if (topPoint >= 0)
			{
				return topPoint + halfsize;
			}
			topPoint = childNodes[num2].GetTopPoint(x, z, halfsize / 2);
			if (topPoint >= 0)
			{
				return topPoint;
			}
			return -1;
		}

		public int GetTopPoint(int sx, int sz, int ex, int ez, int halfsize)
		{
			if (ex < 0 || sx >= halfsize * 2)
			{
				return -1;
			}
			if (ez < 0 || sz >= halfsize * 2)
			{
				return -1;
			}
			if (childNodes == null || childNodes.Length == 0)
			{
				if (type > 0)
				{
					return 0;
				}
				return -1;
			}
			int a = -1;
			a = Mathf.Max(a, childNodes[2].GetTopPoint(sx, sz, ex, ez, halfsize / 2));
			a = Mathf.Max(a, childNodes[3].GetTopPoint(sx, sz - halfsize, ex, ez - halfsize, halfsize / 2));
			a = Mathf.Max(a, childNodes[6].GetTopPoint(sx - halfsize, sz, ex - halfsize, ez, halfsize / 2));
			a = Mathf.Max(a, childNodes[7].GetTopPoint(sx - halfsize, sz - halfsize, ex - halfsize, ez - halfsize, halfsize / 2));
			if (a >= 0)
			{
				return a + halfsize;
			}
			a = Mathf.Max(a, childNodes[0].GetTopPoint(sx, sz, ex, ez, halfsize / 2));
			a = Mathf.Max(a, childNodes[1].GetTopPoint(sx, sz - halfsize, ex, ez - halfsize, halfsize / 2));
			a = Mathf.Max(a, childNodes[4].GetTopPoint(sx - halfsize, sz, ex - halfsize, ez, halfsize / 2));
			return Mathf.Max(a, childNodes[5].GetTopPoint(sx - halfsize, sz - halfsize, ex - halfsize, ez - halfsize, halfsize / 2));
		}

		public int GetBottomPoint(int sx, int sz, int ex, int ez, int halfsize)
		{
			if (ex < 0 || sx >= halfsize * 2)
			{
				return 1000000;
			}
			if (ez < 0 || sz >= halfsize * 2)
			{
				return 1000000;
			}
			if (childNodes == null || childNodes.Length == 0)
			{
				if (type == 0)
				{
					return 0;
				}
				return 1000000;
			}
			int a = 1000000;
			a = Mathf.Min(a, childNodes[0].GetBottomPoint(sx, sz, ex, ez, halfsize / 2));
			a = Mathf.Min(a, childNodes[1].GetBottomPoint(sx, sz - halfsize, ex, ez - halfsize, halfsize / 2));
			a = Mathf.Min(a, childNodes[4].GetBottomPoint(sx - halfsize, sz, ex - halfsize, ez, halfsize / 2));
			a = Mathf.Min(a, childNodes[5].GetBottomPoint(sx - halfsize, sz - halfsize, ex - halfsize, ez - halfsize, halfsize / 2));
			if (a == 1000000)
			{
				a = Mathf.Min(a, childNodes[2].GetBottomPoint(sx, sz, ex, ez, halfsize / 2));
				a = Mathf.Min(a, childNodes[3].GetBottomPoint(sx, sz - halfsize, ex, ez - halfsize, halfsize / 2));
				a = Mathf.Min(a, childNodes[6].GetBottomPoint(sx - halfsize, sz, ex - halfsize, ez, halfsize / 2));
				a = Mathf.Min(a, childNodes[7].GetBottomPoint(sx - halfsize, sz - halfsize, ex - halfsize, ez - halfsize, halfsize / 2));
				if (a < 1000000)
				{
					a += halfsize;
				}
			}
			return a;
		}

		public void RasterizeExists(Array3<int> windowOut, Int3 windowSize, int wx, int wy, int wz, int ox, int oy, int oz, int h)
		{
			if (childNodes == null || h == 0)
			{
				int num = Mathf.Max(1, 2 * h);
				int value = (IsBelowSurface() ? 1 : (-1));
				int num2 = Mathf.Max(ox - wx, 0);
				int num3 = Mathf.Max(oy - wy, 0);
				int num4 = Mathf.Max(oz - wz, 0);
				int num5 = Mathf.Min(ox - wx + num, windowSize.x);
				int num6 = Mathf.Min(oy - wy + num, windowSize.y);
				int num7 = Mathf.Min(oz - wz + num, windowSize.z);
				for (int i = num2; i < num5; i++)
				{
					for (int j = num3; j < num6; j++)
					{
						for (int k = num4; k < num7; k++)
						{
							windowOut[i, j, k] = value;
						}
					}
				}
			}
			else
			{
				int h2 = h >> 1;
				for (int l = 0; l < 8; l++)
				{
					childNodes[l].RasterizeExists(windowOut, windowSize, wx, wy, wz, ox + ChildDX[l] * h, oy + ChildDY[l] * h, oz + ChildDZ[l] * h, h2);
				}
			}
		}

		public void VisitAll(int ox, int oy, int oz, int h, Visitor visitor)
		{
			visitor.Visit(this, ox, oy, oz, h);
			if (childNodes != null && h != 0)
			{
				int h2 = h >> 1;
				for (int i = 0; i < 8; i++)
				{
					childNodes[i].VisitAll(ox + ChildDX[i] * h, oy + ChildDY[i] * h, oz + ChildDZ[i] * h, h2, visitor);
				}
			}
		}

		public void TraceVisit(int x, int y, int z, int ox, int oy, int oz, int h, Visitor visitor)
		{
			visitor.Visit(this, ox, oy, oz, h);
			if (childNodes == null)
			{
				return;
			}
			int h2 = h >> 1;
			for (int i = 0; i < 8; i++)
			{
				int num = ox + ChildDX[i] * h;
				int num2 = oy + ChildDY[i] * h;
				int num3 = oz + ChildDZ[i] * h;
				int num4 = num + h - 1;
				int num5 = num2 + h - 1;
				int num6 = num3 + h - 1;
				if (x >= num && x <= num4 && y >= num2 && y <= num5 && z >= num3 && z <= num6)
				{
					childNodes[i].TraceVisit(x, y, z, num, num2, num3, h2, visitor);
				}
			}
		}

		public void DebugTraceBlock(int x, int y, int z, int ox, int oy, int oz, int h)
		{
			Debug.Log(x + "," + y + "," + z + " s=" + Mathf.Max(1, 2 * h) + " " + ToString());
			if (childNodes == null)
			{
				return;
			}
			int h2 = h >> 1;
			for (int i = 0; i < 8; i++)
			{
				int num = ox + ChildDX[i] * h;
				int num2 = oy + ChildDY[i] * h;
				int num3 = oz + ChildDZ[i] * h;
				int num4 = num + h - 1;
				int num5 = num2 + h - 1;
				int num6 = num3 + h - 1;
				if (x >= num && x <= num4 && y >= num2 && y <= num5 && z >= num3 && z <= num6)
				{
					childNodes[i].DebugTraceBlock(x, y, z, num, num2, num3, h2);
				}
			}
		}

		public int Prune(Array3<int> window, Int3 windowOrigin, Int3 nodeOrigin, int halfSize)
		{
			if (IsLeaf())
			{
				return PruneLeaf(window, windowOrigin, nodeOrigin, halfSize);
			}
			int num = 0;
			int halfSize2 = halfSize >> 1;
			for (int i = 0; i < 8; i++)
			{
				Int3 childOrigin = GetChildOrigin(nodeOrigin, i, halfSize);
				num += childNodes[i].Prune(window, windowOrigin, childOrigin, halfSize2);
			}
			if (num == 0)
			{
				return 0;
			}
			if (!CanCollapse(out var childId))
			{
				CacheDownsampledType();
				return num;
			}
			type = childNodes[childId].type;
			density = childNodes[childId].density;
			ReleaseChildren();
			return num + PruneLeaf(window, windowOrigin, nodeOrigin, halfSize);
		}

		private int PruneLeaf(Array3<int> window, Int3 windowOrigin, Int3 nodeOrigin, int halfSize)
		{
			if (!IsNearSurface())
			{
				return 0;
			}
			bool flag = false;
			bool flag2 = false;
			int num = ((halfSize == 0) ? 1 : (2 * halfSize));
			Int3 mins = nodeOrigin - 1 - windowOrigin;
			Int3 maxs = nodeOrigin + num - windowOrigin;
			foreach (Int3 item in Int3.Range(mins, maxs))
			{
				int num2 = window.Get(item);
				flag = flag || num2 < 0;
				flag2 = flag2 || num2 > 0;
				if (flag && flag2)
				{
					return 0;
				}
			}
			density = 0;
			return 1;
		}

		public static Int3 GetChildOrigin(Int3 nodeOrigin, int idx, int halfSize)
		{
			Int3 @int = new Int3(ChildDX[idx], ChildDY[idx], ChildDZ[idx]);
			return nodeOrigin + @int * halfSize;
		}

		public int Collapse()
		{
			if (IsLeaf())
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < 8; i++)
			{
				num += childNodes[i].Collapse();
			}
			if (CanCollapse(out var childId))
			{
				type = childNodes[childId].type;
				density = childNodes[childId].density;
				ReleaseChildren();
				num++;
			}
			else
			{
				CacheDownsampledType();
			}
			return num;
		}

		private bool CanCollapse(out int childId)
		{
			for (int i = 0; i < 8; i++)
			{
				if (!childNodes[i].IsLeaf())
				{
					childId = -1;
					return false;
				}
			}
			bool flag = false;
			bool flag2 = false;
			for (int j = 0; j < 8; j++)
			{
				bool flag3 = childNodes[j].IsBelowSurface();
				flag = flag || !flag3;
				flag2 = flag2 || flag3;
				if (flag && flag2)
				{
					childId = -1;
					return false;
				}
			}
			int num = 0;
			bool flag4 = true;
			for (int k = 0; k < 8; k++)
			{
				if (childNodes[k].IsNearSurface())
				{
					flag4 = false;
					num = k;
					break;
				}
			}
			if (flag4)
			{
				childId = 0;
				return true;
			}
			bool result = true;
			for (int l = 0; l < 8; l++)
			{
				if (childNodes[l].IsNearSurface())
				{
					if (childNodes[l].type != childNodes[num].type)
					{
						result = false;
						break;
					}
					if (childNodes[l].density != childNodes[num].density)
					{
						result = false;
						break;
					}
				}
			}
			childId = num;
			return result;
		}

		public void Visualize(int offsetX, int offsetY, int offsetZ, int halfsize, bool tree, bool fill)
		{
			if (childNodes != null && childNodes.Length != 0)
			{
				childNodes[0].Visualize(offsetX, offsetY, offsetZ, halfsize / 2, tree, fill);
				childNodes[1].Visualize(offsetX, offsetY, offsetZ + halfsize, halfsize / 2, tree, fill);
				childNodes[2].Visualize(offsetX, offsetY + halfsize, offsetZ, halfsize / 2, tree, fill);
				childNodes[3].Visualize(offsetX, offsetY + halfsize, offsetZ + halfsize, halfsize / 2, tree, fill);
				childNodes[4].Visualize(offsetX + halfsize, offsetY, offsetZ, halfsize / 2, tree, fill);
				childNodes[5].Visualize(offsetX + halfsize, offsetY, offsetZ + halfsize, halfsize / 2, tree, fill);
				childNodes[6].Visualize(offsetX + halfsize, offsetY + halfsize, offsetZ, halfsize / 2, tree, fill);
				childNodes[7].Visualize(offsetX + halfsize, offsetY + halfsize, offsetZ + halfsize, halfsize / 2, tree, fill);
				return;
			}
			float num = Mathf.Sqrt(halfsize * 2) / 4f;
			Vector2 vector = new Vector2(1f - num, num);
			vector /= vector.sqrMagnitude;
			Gizmos.color = new Color(vector.x, vector.y, 0f, num * 0.75f + 0.25f);
			int num2 = halfsize * 2;
			float num3 = halfsize;
			if (halfsize == 0)
			{
				num2 = 1;
				num3 = 0.5f;
			}
			if ((fill && type > 0 && (childNodes == null || childNodes.Length == 0)) || tree)
			{
				Gizmos.DrawWireCube(new Vector3((float)offsetX + num3, (float)offsetY + num3, (float)offsetZ + num3), new Vector3(num2, num2, num2));
			}
		}

		public bool SetBottomUp(IVoxelGrid grid, int x, int y, int z, int h)
		{
			return SetBottomUp(grid, x, y, z, h, BlendArgs.Add);
		}

		public bool SetBottomUp(IVoxelGrid grid, int x, int y, int z, int h, BlendArgs blend)
		{
			if (h == 0)
			{
				ReleaseChildren();
				if (grid.GetVoxelMask(x, y, z))
				{
					OctNode voxel = grid.GetVoxel(x, y, z);
					OctNode octNode = Blend(this, voxel, blend);
					type = octNode.type;
					density = octNode.density;
				}
				return true;
			}
			if (childNodes == null)
			{
				AcquireChildren();
				for (int i = 0; i < 8; i++)
				{
					childNodes[i].type = type;
					childNodes[i].density = density;
				}
			}
			bool flag = true;
			bool flag2 = true;
			for (int j = 0; j < 8; j++)
			{
				if (!childNodes[j].SetBottomUp(grid, x + ChildDX[j] * h, y + ChildDY[j] * h, z + ChildDZ[j] * h, h / 2, blend))
				{
					flag = false;
				}
				if (childNodes[j].type != childNodes[0].type)
				{
					flag2 = false;
				}
				if (childNodes[j].density != childNodes[0].density)
				{
					flag2 = false;
				}
			}
			if (flag && flag2)
			{
				type = childNodes[0].type;
				density = childNodes[0].density;
				ReleaseChildren();
			}
			else
			{
				CacheDownsampledType();
			}
			return flag && flag2;
		}

		public void GenerateFromHeightmap(float[,] map1, float[,] map2, float[,] map3, float[,] map4, byte type1, byte type2, byte type3, byte type4, int offsetX, int offsetY, int offsetZ, int halfsize)
		{
			throw new Exception("TODO 7/15/2014 2:24:00 PM Steve");
		}

		public void GenerateFromOctree(VoxelandData data, int offsetX, int offsetY, int offsetZ, int halfsize, int dataOffsetX, int dataOffsetY, int dataOffsetZ)
		{
			byte b = 0;
			byte b2 = 0;
			OctNode closestNode = data.GetClosestNode(offsetX + dataOffsetX, offsetY + dataOffsetY, offsetZ + dataOffsetZ);
			b = closestNode.type;
			b2 = closestNode.density;
			bool flag = true;
			byte b3 = 0;
			byte b4 = 0;
			if (halfsize != 0)
			{
				for (int i = offsetX; i < offsetX + halfsize * 2; i++)
				{
					for (int j = offsetY; j < offsetY + halfsize * 2; j++)
					{
						for (int k = offsetZ; k < offsetZ + halfsize * 2; k++)
						{
							OctNode closestNode2 = data.GetClosestNode(i + dataOffsetX, j + dataOffsetY, k + dataOffsetZ);
							b3 = closestNode2.type;
							b4 = closestNode2.density;
							if (b3 != b || b4 != b2)
							{
								flag = false;
								break;
							}
						}
					}
				}
			}
			if (flag)
			{
				type = b;
				density = b2;
				ReleaseChildren();
				return;
			}
			AcquireChildren();
			childNodes[0].GenerateFromOctree(data, offsetX, offsetY, offsetZ, halfsize / 2, dataOffsetX, dataOffsetY, dataOffsetZ);
			childNodes[1].GenerateFromOctree(data, offsetX, offsetY, offsetZ + halfsize, halfsize / 2, dataOffsetX, dataOffsetY, dataOffsetZ);
			childNodes[2].GenerateFromOctree(data, offsetX, offsetY + halfsize, offsetZ, halfsize / 2, dataOffsetX, dataOffsetY, dataOffsetZ);
			childNodes[3].GenerateFromOctree(data, offsetX, offsetY + halfsize, offsetZ + halfsize, halfsize / 2, dataOffsetX, dataOffsetY, dataOffsetZ);
			childNodes[4].GenerateFromOctree(data, offsetX + halfsize, offsetY, offsetZ, halfsize / 2, dataOffsetX, dataOffsetY, dataOffsetZ);
			childNodes[5].GenerateFromOctree(data, offsetX + halfsize, offsetY, offsetZ + halfsize, halfsize / 2, dataOffsetX, dataOffsetY, dataOffsetZ);
			childNodes[6].GenerateFromOctree(data, offsetX + halfsize, offsetY + halfsize, offsetZ, halfsize / 2, dataOffsetX, dataOffsetY, dataOffsetZ);
			childNodes[7].GenerateFromOctree(data, offsetX + halfsize, offsetY + halfsize, offsetZ + halfsize, halfsize / 2, dataOffsetX, dataOffsetY, dataOffsetZ);
		}

		public void GenerateFromTwoOctrees(VoxelandData data, VoxelandData backgroundData, int offsetX, int offsetY, int offsetZ, int halfsize, int dataOffsetX, int dataOffsetY, int dataOffsetZ)
		{
			throw new Exception("NOT MAINTAINED --Steve");
		}

		public void GenerateFromArray(int[] blocksArray, bool[] existsArray, int offsetX, int offsetY, int offsetZ, int halfsize, int maxX, int maxY, int maxZ)
		{
			throw new Exception("NOT MAINTAINED --Steve");
		}

		private byte ComputeMostCommonChildType()
		{
			int[] array = child2CountPool.Get();
			for (int i = 0; i < 8; i++)
			{
				OctNode octNode = childNodes[i];
				if (!octNode.IsNearAndBelowSurface())
				{
					continue;
				}
				for (int j = 0; j <= i; j++)
				{
					if (octNode.type == childNodes[j].type)
					{
						array[j]++;
						break;
					}
				}
			}
			int num = 0;
			byte b = childNodes[0].type;
			for (int k = 1; k < 8; k++)
			{
				if (array[k] > num || (array[k] == num && childNodes[k].type > b))
				{
					num = array[k];
					b = childNodes[k].type;
				}
			}
			Array.Clear(array, 0, array.Length);
			child2CountPool.Return(array);
			return b;
		}

		private void CacheDownsampledType()
		{
			if (childNodes == null)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < 8; i++)
			{
				if (childNodes[i].IsNearSurface())
				{
					num += childNodes[i].density;
					num2++;
				}
			}
			if (num2 == 0)
			{
				density = 0;
				type = childNodes[0].type;
				return;
			}
			int num3 = num / num2;
			if (num3 < 0 || num3 >= 256)
			{
				throw new Exception("Got an average density that was out of byte range: " + num3);
			}
			density = Convert.ToByte(num3);
			type = ComputeMostCommonChildType();
		}

		public void Read(PoolingBinaryReader reader, int version)
		{
			byte b = reader.ReadByte();
			if (b != byte.MaxValue)
			{
				type = b;
				if (version >= 1)
				{
					density = reader.ReadByte();
				}
				else
				{
					density = 0;
				}
				if (childNodes != null)
				{
					Debug.LogWarning("Read leaf octNode but node already had childNodes. Should not happen!");
					ReleaseChildren();
				}
			}
			else
			{
				AcquireChildren();
				for (int i = 0; i < 8; i++)
				{
					childNodes[i].Read(reader, version);
				}
				CacheDownsampledType();
			}
		}

		public int ReadFromArray(byte[] bytes, int cursor, int version)
		{
			byte b = bytes[cursor++];
			if (b != byte.MaxValue)
			{
				type = b;
				if (version >= 1)
				{
					density = bytes[cursor++];
				}
				else
				{
					density = 0;
				}
				ReleaseChildren();
			}
			else
			{
				AcquireChildren();
				for (int i = 0; i < 8; i++)
				{
					cursor = childNodes[i].ReadFromArray(bytes, cursor, version);
				}
			}
			return cursor;
		}

		public int Write(BinaryWriter writer)
		{
			if (childNodes == null || childNodes.Length == 0)
			{
				writer.Write(type);
				writer.Write(density);
				return 2;
			}
			writer.Write(Convert.ToByte(255));
			int num = 1;
			for (int i = 0; i < 8; i++)
			{
				num += childNodes[i].Write(writer);
			}
			return num;
		}

		public OctNode DeepCopy()
		{
			OctNode result = default(OctNode);
			result.type = type;
			result.density = density;
			if (childNodes != null)
			{
				result.AcquireChildren();
				for (int i = 0; i < 8; i++)
				{
					result.childNodes[i] = childNodes[i].DeepCopy();
				}
			}
			return result;
		}

		public void Rasterize(Array3<byte> typesOut, Array3<byte> densityOut, Int3 arraySize, int wx, int wy, int wz, int ox, int oy, int oz, int h)
		{
			if (childNodes == null || h == 0)
			{
				int num = ((h == 0) ? 1 : (2 * h));
				int num2 = Mathf.Max(ox - wx, 0);
				int num3 = Mathf.Max(oy - wy, 0);
				int num4 = Mathf.Max(oz - wz, 0);
				int num5 = Mathf.Min(ox - wx + num, arraySize.x);
				int num6 = Mathf.Min(oy - wy + num, arraySize.y);
				int num7 = Mathf.Min(oz - wz + num, arraySize.z);
				for (int i = num2; i < num5; i++)
				{
					for (int j = num3; j < num6; j++)
					{
						for (int k = num4; k < num7; k++)
						{
							typesOut[i, j, k] = type;
							densityOut[i, j, k] = density;
						}
					}
				}
				return;
			}
			int num8 = wx + arraySize.x;
			int num9 = wy + arraySize.y;
			int num10 = wz + arraySize.z;
			int h2 = h >> 1;
			for (int l = 0; l < 8; l++)
			{
				int num11 = ox + h * ChildDX[l];
				int num12 = oy + h * ChildDY[l];
				int num13 = oz + h * ChildDZ[l];
				int num14 = num11 + h - 1;
				int num15 = num12 + h - 1;
				int num16 = num13 + h - 1;
				if (num14 >= wx && num11 < num8 && num15 >= wy && num12 < num9 && num16 >= wz && num13 < num10)
				{
					childNodes[l].Rasterize(typesOut, densityOut, arraySize, wx, wy, wz, num11, num12, num13, h2);
				}
			}
		}

		public void ReplaceType(byte oldType, byte newType)
		{
			if (childNodes == null)
			{
				if (type == oldType)
				{
					type = newType;
				}
			}
			else
			{
				for (int i = 0; i < childNodes.Length; i++)
				{
					childNodes[i].ReplaceType(oldType, newType);
				}
			}
		}

		public void TabulateTypeUsage(int[] type2count)
		{
			type2count[type]++;
			if (childNodes != null)
			{
				for (int i = 0; i < 8; i++)
				{
					childNodes[i].TabulateTypeUsage(type2count);
				}
			}
		}

		public void ApplyTypeConversionTable(byte[] type2new)
		{
			type = type2new[type];
			if (childNodes != null)
			{
				for (int i = 0; i < 8; i++)
				{
					childNodes[i].ApplyTypeConversionTable(type2new);
				}
			}
		}
	}

	[Serializable]
	public struct TreeState
	{
		public enum State
		{
			NotExist = 0,
			Ready = 1,
			BeingWritten = 2,
			BeingRead = 3
		}

		public State state;

		public int numReaders;

		public void Reset()
		{
			state = State.NotExist;
			numReaders = 0;
		}
	}

	[Serializable]
	public class LegacyToDensityTranslator : IVoxelGrid
	{
		public VoxelandData data;

		public LegacyToDensityTranslator(VoxelandData data)
		{
			this.data = data;
		}

		public OctNode GetVoxel(int x, int y, int z)
		{
			OctNode result = OctNode.EmptyNode();
			if (!data.CheckBounds(x, y, z))
			{
				return result;
			}
			OctNode node = data.GetNode(x, y, z);
			result.type = node.type;
			if (result.type == 0)
			{
				if (data.IsNearSurfaceByType(x, y, z))
				{
					sbyte maxNborBulge = data.GetMaxNborBulge(x, y, z);
					float dist = Mathf.Clamp(1f / bulge2distInvSlope * (float)maxNborBulge - 0.5f, -1f, 0f - Mathf.Epsilon);
					result.density = OctNode.EncodeDensity(dist);
				}
				else
				{
					result.density = 0;
				}
			}
			else if (data.IsNearSurfaceByType(x, y, z))
			{
				sbyte maxNborBulge = (sbyte)node.density;
				float dist = Mathf.Clamp(1f / bulge2distInvSlope * (float)maxNborBulge + 0.5f, 0f, 1f);
				result.density = OctNode.EncodeDensity(dist);
			}
			else
			{
				result.density = 0;
			}
			return result;
		}

		public bool GetVoxelMask(int x, int y, int z)
		{
			if (!data.CheckBounds(x, y, z))
			{
				return false;
			}
			if (data.GetNode(x, y, z).type > 0)
			{
				return true;
			}
			return data.IsNearSurfaceByType(x, y, z);
		}
	}

	[Serializable]
	public class MyUndoAction : VoxelandUndoAction
	{
		public VoxelandData data;

		public List<int> nums = new List<int>();

		public List<OctNode> nodes = new List<OctNode>();

		public MyUndoAction(VoxelandData data)
		{
			this.data = data;
		}

		public void Perform()
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				data.roots[nums[i]] = nodes[i];
				int num = nums[i];
				data.OnRootEdited(num);
			}
		}
	}

	[Serializable]
	public class FlatVoxelInput : IVoxelGrid
	{
		public float height;

		public byte type;

		public OctNode GetVoxel(int x, int y, int z)
		{
			float num = (float)y + 0.5f;
			float num2 = height - num;
			OctNode result = default(OctNode);
			result.type = (byte)((num2 > 0f) ? type : 0);
			result.density = OctNode.EncodeDensity(num2);
			return result;
		}

		public bool GetVoxelMask(int x, int y, int z)
		{
			return true;
		}
	}

	public const int CurrentVersion = 3;

	public static readonly Stack<string> ProgressTitleStack = new Stack<string>();

	[NonSerialized]
	public bool dirty = true;

	[NonSerialized]
	private OctNode[] _roots;

	[NonSerialized]
	public TreeState[] treeStates;

	[HideInInspector]
	public List<byte> octreeSerialBytes;

	[HideInInspector]
	public List<int> octreeSerialIndex;

	[HideInInspector]
	public int octreeSerialVersion;

	[HideInInspector]
	public byte[] octreeBytesArray;

	[HideInInspector]
	public Voxeland creator;

	[NonSerialized]
	public EventHandler eventHandler;

	public static float bulge2distInvSlope = 200f;

	public int biggestNode = 16;

	public int nodesX;

	public int nodesY;

	public int nodesZ;

	public int sizeX;

	public int sizeY;

	public int sizeZ;

	private int newSizeX;

	private int newSizeY;

	private int newSizeZ;

	private int newBiggestNode;

	private int offsetX;

	private int offsetY;

	private int offsetZ;

	[HideInInspector]
	public bool[] guiGenerateExtend = new bool[10];

	[HideInInspector]
	public bool[] guiGenerateCheck = new bool[10] { true, true, true, true, true, true, true, true, true, true };

	[HideInInspector]
	public int genPlanarType = 1;

	[HideInInspector]
	public int genPlanarLevel = 1;

	[HideInInspector]
	public int genNoiseType = 1;

	[HideInInspector]
	public bool genNoiseAdditive;

	[HideInInspector]
	public int genNoiseFractals = 6;

	[HideInInspector]
	public float genNoiseFractalMin = 3f;

	[HideInInspector]
	public float genNoiseFractalMax = 100f;

	[HideInInspector]
	public float genNoiseValueMin = 5f;

	[HideInInspector]
	public float genNoiseValueMax = 50f;

	[HideInInspector]
	public int genErosionGulliesIterations = 40;

	[HideInInspector]
	public float genErosionGulliesMudAmount = 0.02f;

	[HideInInspector]
	public float genErosionGulliesBlurValue = 0.1f;

	[HideInInspector]
	public int genErosionGulliesDeblur = 5;

	[HideInInspector]
	public float genErosionGulliesWind = 1.5f;

	[HideInInspector]
	public int genErosionSedimentType = 2;

	[HideInInspector]
	public int genErosionSedimentBlurIterations = 500;

	[HideInInspector]
	public int genErosionSedimentHollowIterations = 50;

	[HideInInspector]
	public float genErosionSedimentAdjustLevel = -2f;

	[HideInInspector]
	public float genErosionSedimentDepth = 1.5f;

	[HideInInspector]
	public int genCoverType = 3;

	[HideInInspector]
	public int genCoverBlurIterations = 30;

	[HideInInspector]
	public float genCoverPlanarDelta = 0.7f;

	[HideInInspector]
	public float genCoverPikesDelta = 0.018f;

	[NonSerialized]
	private MyUndoAction currUndoStep;

	public OctNode[] roots
	{
		get
		{
			return _roots;
		}
		set
		{
			_roots = value;
			dirty = true;
		}
	}

	public void SerializeOctrees()
	{
		octreeSerialBytes = null;
		octreeSerialIndex = null;
		octreeSerialVersion = 3;
		octreeBytesArray = ToBytesArray();
	}

	public byte[] ToBytesArray()
	{
		if (roots == null || roots.Length == 0)
		{
			return new byte[0];
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(memoryStream);
		for (int i = 0; i < roots.Length; i++)
		{
			roots[i].Write(writer);
		}
		return memoryStream.ToArray();
	}

	public void SerializeInto(VoxelandSerialData dest)
	{
		dest.octreeSerialBytes = null;
		dest.octreeSerialIndex = null;
		dest.version = 3;
		dest.sizeX = sizeX;
		dest.sizeY = sizeY;
		dest.sizeZ = sizeZ;
		dest.rootSize = biggestNode;
		dest.octreeBytesArray = ToBytesArray();
	}

	public bool IsNearSurfaceByType(int x, int y, int z)
	{
		bool flag = GetNode(x, y, z).type > 0;
		for (int i = 0; i < 6; i++)
		{
			int x2 = x + VoxelandChunk.VoxelandFace.dirToPosX[i];
			int y2 = y + VoxelandChunk.VoxelandFace.dirToPosY[i];
			int z2 = z + VoxelandChunk.VoxelandFace.dirToPosZ[i];
			if (CheckBounds(x2, y2, z2) && GetNode(x2, y2, z2).type > 0 != flag)
			{
				return true;
			}
		}
		return false;
	}

	public sbyte GetMaxNborBulge(int x, int y, int z)
	{
		sbyte b = sbyte.MinValue;
		for (int i = 0; i < 6; i++)
		{
			int x2 = x + VoxelandChunk.VoxelandFace.dirToPosX[i];
			int y2 = y + VoxelandChunk.VoxelandFace.dirToPosY[i];
			int z2 = z + VoxelandChunk.VoxelandFace.dirToPosZ[i];
			if (CheckBounds(x2, y2, z2))
			{
				OctNode node = GetNode(x2, y2, z2);
				if (node.type > 0)
				{
					sbyte b2 = (sbyte)node.density;
					b = ((b2 > b) ? b2 : b);
				}
			}
		}
		return b;
	}

	public void SetToBytes(byte[] bytesArray, int version, Voxeland debugLand)
	{
		int num = 0;
		ClearOctrees();
		if (version < 3)
		{
			long ticks = DateTime.Now.Ticks;
			VoxelandData voxelandData = ScriptableObject.CreateInstance<VoxelandData>();
			voxelandData.ClearToNothing(sizeX, sizeY, sizeZ, biggestNode);
			using (MemoryStream stream = new MemoryStream(bytesArray, writable: false))
			{
				using (PooledBinaryReader pooledBinaryReader = new PooledBinaryReader(stream))
				{
					for (num = 0; num < voxelandData.roots.Length; num++)
					{
						voxelandData.roots[num].Read(pooledBinaryReader, version);
					}
				}
			}
			LegacyToDensityTranslator src = new LegacyToDensityTranslator(voxelandData);
			SetForRange(src, 0, 0, 0, sizeX - 1, sizeY - 1, sizeZ - 1, OctNode.BlendArgs.Add);
			for (num = 0; num < roots.Length; num++)
			{
				roots[num].Collapse();
			}
			UnityEngine.Object.DestroyImmediate(voxelandData);
			if (debugLand != null)
			{
				LogMSSince(ticks, "Upgrading blocks to level set rep, land name = " + debugLand.gameObject.GetFullHierarchyPath());
			}
			else
			{
				LogMSSince(ticks, "Upgrading blocks to level set rep (unknown data source)");
			}
			return;
		}
		using (MemoryStream stream2 = new MemoryStream(bytesArray, writable: false))
		{
			using (PooledBinaryReader pooledBinaryReader2 = new PooledBinaryReader(stream2))
			{
				for (num = 0; num < roots.Length; num++)
				{
					roots[num].Read(pooledBinaryReader2, version);
				}
			}
		}
	}

	private static void LogMSSince(long t0, string label)
	{
		long num = (DateTime.Now.Ticks - t0) / 10000;
		Debug.LogFormat("{0} ms {1}", label, num);
	}

	public void UnserializeFrom(VoxelandSerialData src, Voxeland land)
	{
		if ((src.version >= 3 || (src.octreeSerialBytes != null && src.octreeSerialBytes.Count != 0)) && (src.version < 3 || (src.octreeBytesArray != null && src.octreeBytesArray.Length != 0)))
		{
			byte[] bytesArray = src.octreeBytesArray;
			if (src.version < 3)
			{
				bytesArray = src.octreeSerialBytes.ToArray();
			}
			ClearToNothing(src.sizeX, src.sizeY, src.sizeZ, src.rootSize);
			SetToBytes(bytesArray, src.version, land);
		}
	}

	public void UnserializeOctrees()
	{
		if ((octreeSerialVersion >= 3 || (octreeSerialBytes != null && octreeSerialBytes.Count != 0)) && (octreeSerialVersion < 3 || (octreeBytesArray != null && octreeBytesArray.Length != 0)))
		{
			byte[] bytesArray = octreeBytesArray;
			if (octreeSerialVersion < 3)
			{
				bytesArray = octreeSerialBytes.ToArray();
			}
			SetToBytes(bytesArray, octreeSerialVersion, null);
		}
	}

	public void BeginUndoStep()
	{
	}

	public VoxelandUndoAction EndUndoStep()
	{
		return null;
	}

	public void OnRootEdited(int num)
	{
		if (eventHandler != null)
		{
			eventHandler.OnRootChanged(GetRootX(num), GetRootY(num), GetRootZ(num));
		}
	}

	public void RecordUndoNode(int num)
	{
	}

	public int GetRootIndexForBlock(int bx, int by, int bz)
	{
		int rx = bx / biggestNode;
		int ry = by / biggestNode;
		int rz = bz / biggestNode;
		return GetRootIndex(rx, ry, rz);
	}

	public int GetRootIndex(int rx, int ry, int rz)
	{
		if (rx < 0 || rx >= nodesX || ry < 0 || ry >= nodesY || rz < 0 || rz >= nodesZ)
		{
			return -1;
		}
		return rz * nodesY * nodesX + ry * nodesX + rx;
	}

	public int GetRootX(int rid)
	{
		return rid % (nodesY * nodesX) % nodesX;
	}

	public int GetRootY(int rid)
	{
		return (rid - GetRootX(rid)) % (nodesY * nodesX) / nodesX;
	}

	public int GetRootZ(int rid)
	{
		return (rid - GetRootX(rid) - GetRootY(rid) * nodesX) / (nodesY * nodesX);
	}

	public bool IsTreeEmpty(int rx, int ry, int rz)
	{
		int rootIndex = GetRootIndex(rx, ry, rz);
		if (rootIndex == -1)
		{
			return true;
		}
		if (roots != null && roots.Length > rootIndex)
		{
			return roots[rootIndex].IsEmpty();
		}
		return false;
	}

	public bool IsRangeEmpty(int firstX, int firstY, int firstZ, int lastX, int lastY, int lastZ)
	{
		for (int i = firstX / biggestNode; i <= lastX / biggestNode; i++)
		{
			for (int j = firstY / biggestNode; j <= lastY / biggestNode; j++)
			{
				for (int k = firstZ / biggestNode; k <= lastZ / biggestNode; k++)
				{
					if (!IsTreeEmpty(i, j, k))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public OctNode GetNode(int x, int y, int z)
	{
		int num = x / biggestNode;
		int num2 = y / biggestNode;
		int num3 = z / biggestNode;
		if (roots == null)
		{
			Debug.LogError("No valid data! Returning nonsense");
			return new OctNode(0, 0);
		}
		int rootIndex = GetRootIndex(num, num2, num3);
		if (rootIndex >= roots.Length || rootIndex < 0)
		{
			throw new Exception("Requested block was out of bounds. Requested node = " + num + "," + num2 + "," + num3 + ", block = " + x + "," + y + "," + z + " num roots = " + roots.Length);
		}
		return roots[rootIndex].GetNode(x - num * biggestNode, y - num2 * biggestNode, z - num3 * biggestNode, biggestNode / 2);
	}

	public OctNode GetClosestNode(int x, int y, int z)
	{
		int x2 = Mathf.Min(sizeX - 1, Mathf.Max(0, x));
		int y2 = Mathf.Min(sizeY - 1, Mathf.Max(0, y));
		int z2 = Mathf.Min(sizeZ - 1, Mathf.Max(0, z));
		return GetNode(x2, y2, z2);
	}

	public bool CheckBounds(int x, int y, int z)
	{
		if (x >= 0 && x < sizeX && y >= 0 && y < sizeY && z >= 0)
		{
			return z < sizeZ;
		}
		return false;
	}

	public int GetBlock(int x, int y, int z)
	{
		return GetNode(x, y, z).type;
	}

	public OctNode GetVoxel(int x, int y, int z)
	{
		return GetNode(x, y, z);
	}

	public bool GetVoxelMask(int x, int y, int z)
	{
		if (x < 0 || y < 0 || z < 0)
		{
			return false;
		}
		if (x >= sizeX || y >= sizeY || z >= sizeZ)
		{
			return false;
		}
		return true;
	}

	public void CollapseOctree()
	{
		for (int i = 0; i < roots.Length; i++)
		{
			roots[i].Collapse();
		}
	}

	public void CollapseRelevant(int x0, int y0, int z0, int x1, int y1, int z1)
	{
		for (int i = x0 / biggestNode; i <= x1 / biggestNode; i++)
		{
			for (int j = y0 / biggestNode; j <= y1 / biggestNode; j++)
			{
				for (int k = z0 / biggestNode; k <= z1 / biggestNode; k++)
				{
					int rootIndex = GetRootIndex(i, j, k);
					if (rootIndex != -1)
					{
						roots[rootIndex].Collapse();
					}
				}
			}
		}
	}

	public void VisitAll(OctNode.Visitor visitor)
	{
		if (roots == null)
		{
			return;
		}
		for (int i = 0; i < nodesX; i++)
		{
			for (int j = 0; j < nodesY; j++)
			{
				for (int k = 0; k < nodesZ; k++)
				{
					int rootIndex = GetRootIndex(i, j, k);
					if (rootIndex != -1)
					{
						roots[rootIndex].VisitAll(i * biggestNode, j * biggestNode, k * biggestNode, biggestNode / 2, visitor);
					}
				}
			}
		}
	}

	public void TraceVisit(OctNode.Visitor visitor, int x, int y, int z)
	{
		if (roots != null)
		{
			int rootIndex = GetRootIndex(x / biggestNode, y / biggestNode, z / biggestNode);
			if (rootIndex != -1)
			{
				roots[rootIndex].TraceVisit(x, y, z, x / biggestNode * biggestNode, y / biggestNode * biggestNode, z / biggestNode * biggestNode, biggestNode / 2, visitor);
			}
		}
	}

	public void DebugTraceBlock(int x, int y, int z)
	{
		int rootIndex = GetRootIndex(x / biggestNode, y / biggestNode, z / biggestNode);
		if (rootIndex != -1)
		{
			roots[rootIndex].DebugTraceBlock(x, y, z, x / biggestNode * biggestNode, y / biggestNode * biggestNode, z / biggestNode * biggestNode, biggestNode / 2);
		}
	}

	public void SetNodeFast(int x, int y, int z, OctNode node)
	{
		int rootIndexForBlock = GetRootIndexForBlock(x, y, z);
		roots[rootIndexForBlock].SetNode(x % biggestNode, y % biggestNode, z % biggestNode, biggestNode / 2, node.type, node.density);
	}

	public void SetBlock(int x, int y, int z, byte type, byte density, bool skipCollapse, bool threaded)
	{
		int rootIndexForBlock = GetRootIndexForBlock(x, y, z);
		if (rootIndexForBlock != -1)
		{
			OnRootEdited(rootIndexForBlock);
			roots[rootIndexForBlock].SetNode(x % biggestNode, y % biggestNode, z % biggestNode, biggestNode / 2, type, density);
			if (!skipCollapse)
			{
				roots[rootIndexForBlock].Collapse();
			}
		}
	}

	public void SetBlock(int x, int y, int z, byte type, byte density)
	{
		SetBlock(x, y, z, type, density, skipCollapse: false, threaded: false);
	}

	public void BlendNode(int x, int y, int z, OctNode srcNode, OctNode.BlendArgs blend)
	{
		int rootIndexForBlock = GetRootIndexForBlock(x, y, z);
		if (rootIndexForBlock != -1)
		{
			OnRootEdited(rootIndexForBlock);
			OctNode octNode = OctNode.Blend(GetNode(x, y, z), srcNode, blend);
			roots[rootIndexForBlock].SetNode(x % biggestNode, y % biggestNode, z % biggestNode, biggestNode / 2, octNode.type, octNode.density);
		}
	}

	public bool Exists(int x, int y, int z)
	{
		return GetNode(x, y, z).type > 0;
	}

	public bool SafeExists(int x, int y, int z)
	{
		if (CheckBounds(x, y, z))
		{
			return Exists(x, y, z);
		}
		return false;
	}

	public int GetTopPoint(int x, int z)
	{
		int num = x / biggestNode;
		int num2 = z / biggestNode;
		for (int num3 = nodesY - 1; num3 >= 0; num3--)
		{
			int topPoint = roots[num2 * nodesY * nodesX + num3 * nodesX + num].GetTopPoint(x - num * biggestNode, z - num2 * biggestNode, biggestNode / 2);
			if (topPoint >= 0)
			{
				return topPoint + num3 * biggestNode;
			}
		}
		return 0;
	}

	public int GetTopPoint(int sx, int sz, int ex, int ez)
	{
		int num = Mathf.Clamp(sx / biggestNode, 0, nodesX - 1);
		int num2 = Mathf.Clamp(sz / biggestNode, 0, nodesZ - 1);
		int num3 = Mathf.Clamp(ex / biggestNode, 0, nodesX - 1);
		int num4 = Mathf.Clamp(ez / biggestNode, 0, nodesZ - 1);
		int num5 = -1;
		for (int num6 = nodesY - 1; num6 >= 0; num6--)
		{
			for (int i = num; i <= num3; i++)
			{
				for (int j = num2; j <= num4; j++)
				{
					num5 = Mathf.Max(num5, roots[j * nodesY * nodesX + num6 * nodesX + i].GetTopPoint(sx - i * biggestNode, sz - j * biggestNode, ex - i * biggestNode, ez - j * biggestNode, biggestNode / 2));
				}
			}
			if (num5 >= 0)
			{
				return num5 + num6 * biggestNode;
			}
		}
		return 0;
	}

	public int GetBottomPoint(int sx, int sz, int ex, int ez)
	{
		int num = Mathf.Clamp(sx / biggestNode, 0, nodesX - 1);
		int num2 = Mathf.Clamp(sz / biggestNode, 0, nodesZ - 1);
		int num3 = Mathf.Clamp(ex / biggestNode, 0, nodesX - 1);
		int num4 = Mathf.Clamp(ez / biggestNode, 0, nodesZ - 1);
		int num5 = 1000000;
		for (int i = 0; i < nodesY; i++)
		{
			for (int j = num; j <= num3; j++)
			{
				for (int k = num2; k <= num4; k++)
				{
					num5 = Mathf.Min(num5, roots[k * nodesY * nodesX + i * nodesX + j].GetBottomPoint(sx - j * biggestNode, sz - k * biggestNode, ex - j * biggestNode, ez - k * biggestNode, biggestNode / 2));
				}
			}
			if (num5 < 1000000)
			{
				return num5 + i * biggestNode;
			}
		}
		return 0;
	}

	[Obsolete("NOTE Jonas: I have low confidence in the correctness of this method", true)]
	public void RasterizeExists(Array3<int> windowOut, Int3 windowSize, int wx, int wy, int wz, int downsampleLevels)
	{
		int num = wx + (windowSize.x << downsampleLevels) - 1;
		int num2 = wy + (windowSize.y << downsampleLevels) - 1;
		int num3 = wz + (windowSize.z << downsampleLevels) - 1;
		int num4 = biggestNode;
		for (int i = Mathf.Max(0, wx / num4); i <= Mathf.Min(num / num4, nodesX - 1); i++)
		{
			for (int j = Mathf.Max(0, wy / num4); j <= Mathf.Min(num2 / num4, nodesY - 1); j++)
			{
				for (int k = Mathf.Max(0, wz / num4); k <= Mathf.Min(num3 / num4, nodesZ - 1); k++)
				{
					int rootIndex = GetRootIndex(i, j, k);
					roots[rootIndex].RasterizeExists(windowOut, windowSize, wx >> downsampleLevels, wy >> downsampleLevels, wz >> downsampleLevels, i * num4 >> downsampleLevels, j * num4 >> downsampleLevels, k * num4 >> downsampleLevels, num4 >> downsampleLevels + 1);
				}
			}
		}
	}

	public void Visualize(bool nodes, bool fill)
	{
		if (roots == null || roots.Length == 0)
		{
			return;
		}
		for (int i = 0; i < nodesX; i++)
		{
			for (int j = 0; j < nodesY; j++)
			{
				for (int k = 0; k < nodesZ; k++)
				{
					roots[k * nodesY * nodesX + j * nodesX + i].Visualize(i * biggestNode, j * biggestNode, k * biggestNode, biggestNode / 2, nodes, fill);
				}
			}
		}
	}

	public bool HasBlockAbove(int x, int y, int z)
	{
		for (int num = sizeY - 1; num >= y; num--)
		{
			if (Exists(x, num, z))
			{
				return true;
			}
		}
		return false;
	}

	public bool ValidateSize(int size, int chunkSize)
	{
		return Mathf.RoundToInt((float)size * 1f / ((float)chunkSize * 1f)) * chunkSize == size;
	}

	public void FreeOctrees()
	{
		if (roots != null)
		{
			for (int i = 0; i < roots.Length; i++)
			{
				roots[i].Clear();
			}
			roots = null;
		}
	}

	public void ClearOctrees()
	{
		if (roots != null)
		{
			for (int i = 0; i < roots.Length; i++)
			{
				roots[i].Clear();
			}
		}
		else
		{
			roots = new OctNode[nodesX * nodesY * nodesZ];
		}
		if (roots.Length != nodesX * nodesY * nodesZ)
		{
			roots = new OctNode[nodesX * nodesY * nodesZ];
		}
	}

	public void ClearToNothing()
	{
		ClearToNothing(sizeX, sizeY, sizeZ, biggestNode);
	}

	public void ClearToNothing(int newX, int newY, int newZ, int newMaxNodeSize, bool clear = true)
	{
		sizeX = newX;
		sizeY = newY;
		sizeZ = newZ;
		biggestNode = newMaxNodeSize;
		nodesX = UWE.Math.CeilDiv(newX, biggestNode);
		nodesY = UWE.Math.CeilDiv(newY, biggestNode);
		nodesZ = UWE.Math.CeilDiv(newZ, biggestNode);
		if (clear)
		{
			ClearOctrees();
		}
	}

	public void Copy(VoxelandData data)
	{
		sizeX = data.sizeX;
		sizeY = data.sizeY;
		sizeZ = data.sizeZ;
		biggestNode = data.biggestNode;
		nodesX = data.nodesX;
		nodesY = data.nodesY;
		nodesZ = data.nodesZ;
		FreeOctrees();
		roots = new OctNode[nodesX * nodesY * nodesZ];
		for (int i = 0; i < nodesX; i++)
		{
			for (int j = 0; j < nodesY; j++)
			{
				for (int k = 0; k < nodesZ; k++)
				{
					int num = k * nodesY * nodesX + j * nodesX + i;
					roots[num] = data.roots[num].DeepCopy();
				}
			}
		}
		SerializeOctrees();
	}

	public void Rasterize(Array3<byte> windowOut, Array3<byte> densityOut, Int3 arraySize, int wx, int wy, int wz, int downsampleLevels)
	{
		int num = wx + (arraySize.x << downsampleLevels) - 1;
		int num2 = wy + (arraySize.y << downsampleLevels) - 1;
		int num3 = wz + (arraySize.z << downsampleLevels) - 1;
		int num4 = biggestNode;
		for (int i = Mathf.Max(0, wx / num4); i <= System.Math.Min(num / num4, nodesX - 1); i++)
		{
			for (int j = Mathf.Max(0, wy / num4); j <= System.Math.Min(num2 / num4, nodesY - 1); j++)
			{
				for (int k = Mathf.Max(0, wz / num4); k <= System.Math.Min(num3 / num4, nodesZ - 1); k++)
				{
					int rootIndex = GetRootIndex(i, j, k);
					roots[rootIndex].Rasterize(windowOut, densityOut, arraySize, wx >> downsampleLevels, wy >> downsampleLevels, wz >> downsampleLevels, i * num4 >> downsampleLevels, j * num4 >> downsampleLevels, k * num4 >> downsampleLevels, num4 >> downsampleLevels + 1);
				}
			}
		}
	}

	public void ReplaceType(int x0, int y0, int z0, int x1, int y1, int z1, byte oldType, byte newType)
	{
		for (int i = x0 / biggestNode; i <= x1 / biggestNode; i++)
		{
			for (int j = y0 / biggestNode; j <= y1 / biggestNode; j++)
			{
				for (int k = z0 / biggestNode; k <= z1 / biggestNode; k++)
				{
					int rootIndex = GetRootIndex(i, j, k);
					if (rootIndex != -1)
					{
						RecordUndoNode(rootIndex);
						roots[rootIndex].ReplaceType(oldType, newType);
						OnRootEdited(rootIndex);
					}
				}
			}
		}
	}

	public void Resize(int newX, int newY, int newZ, int newNode)
	{
		int num = Mathf.CeilToInt(1f * (float)newX / (float)newNode);
		int num2 = Mathf.CeilToInt(1f * (float)newY / (float)newNode);
		int num3 = Mathf.CeilToInt(1f * (float)newZ / (float)newNode);
		OctNode[] array = new OctNode[num * num2 * num3];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				for (int k = 0; k < num3; k++)
				{
					int num4 = k * num2 * num + j * num + i;
					array[num4].GenerateFromOctree(this, i * newNode, j * newNode, k * newNode, newNode / 2, 0, 0, 0);
				}
			}
		}
		FreeOctrees();
		roots = array;
		sizeX = newX;
		sizeY = newY;
		sizeZ = newZ;
		biggestNode = newNode;
		nodesX = num;
		nodesY = num2;
		nodesZ = num3;
	}

	public void Offset(int stepX, int stepY, int stepZ)
	{
		OctNode[] array = new OctNode[nodesX * nodesY * nodesZ];
		for (int i = 0; i < nodesX; i++)
		{
			for (int j = 0; j < nodesY; j++)
			{
				for (int k = 0; k < nodesZ; k++)
				{
					int num = k * nodesY * nodesX + j * nodesX + i;
					array[num].GenerateFromOctree(this, i * biggestNode, j * biggestNode, k * biggestNode, biggestNode / 2, -stepX, -stepY, -stepZ);
				}
			}
		}
		FreeOctrees();
		roots = array;
	}

	public void Insert(VoxelandData data, int stepX, int stepY, int stepZ)
	{
		OctNode[] array = new OctNode[nodesX * nodesY * nodesZ];
		for (int i = 0; i < nodesX; i++)
		{
			for (int j = 0; j < nodesY; j++)
			{
				for (int k = 0; k < nodesZ; k++)
				{
					int num = k * nodesY * nodesX + j * nodesX + i;
					array[num].GenerateFromTwoOctrees(data, this, i * biggestNode, j * biggestNode, k * biggestNode, biggestNode / 2, -stepX, -stepY, -stepZ);
				}
			}
		}
		FreeOctrees();
		roots = array;
	}

	public static VoxelandData New(int newX, int newY, int newZ, int newNode, int filledLevel, bool saveToAsset)
	{
		return null;
	}

	public int EstimateOctreeBytes()
	{
		int num = 0;
		if (roots == null)
		{
			return 0;
		}
		for (int i = 0; i < roots.Length; i++)
		{
			num += roots[i].EstimateBytes();
		}
		return num;
	}

	public int GetLargestNodeCount()
	{
		int num = 0;
		for (int i = 0; i < roots.Length; i++)
		{
			num = System.Math.Max(roots[i].CountNodes(), num);
		}
		return num;
	}

	public float[,] GetHeighmap()
	{
		float[,] array = new float[sizeX, sizeZ];
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeZ; j++)
			{
				for (int k = 0; k < sizeY; k++)
				{
					if (Exists(i, k, j))
					{
						array[i, j] = k;
					}
				}
			}
		}
		return array;
	}

	public void SetForRange(IVoxelGrid src, int bx, int by, int bz, int sx, int sy, int sz)
	{
		SetForRange(src, bx, by, bz, sx, sy, sz, OctNode.BlendArgs.Add);
	}

	public void SetForRange(IVoxelGrid src, int bx, int by, int bz, int sx, int sy, int sz, OctNode.BlendArgs blend)
	{
		for (int i = bx / biggestNode; i <= (bx + sx - 1) / biggestNode; i++)
		{
			for (int j = by / biggestNode; j <= (by + sy - 1) / biggestNode; j++)
			{
				for (int k = bz / biggestNode; k <= (bz + sz - 1) / biggestNode; k++)
				{
					int rootIndex = GetRootIndex(i, j, k);
					if (rootIndex != -1)
					{
						RecordUndoNode(rootIndex);
						OnRootEdited(rootIndex);
						roots[rootIndex].SetBottomUp(src, i * biggestNode, j * biggestNode, k * biggestNode, biggestNode / 2, blend);
					}
				}
			}
		}
	}

	public void SetHeightmap(float[,] map1, float[,] map2, float[,] map3, float[,] map4, int type1, int type2, int type3, int type4, int height, int nodeSize)
	{
		nodesX = Mathf.CeilToInt(1f * (float)map1.GetLength(0) / (float)nodeSize);
		nodesY = Mathf.CeilToInt(1f * (float)height / (float)nodeSize);
		nodesZ = Mathf.CeilToInt(1f * (float)map1.GetLength(1) / (float)nodeSize);
		FreeOctrees();
		roots = new OctNode[nodesX * nodesY * nodesZ];
		for (int i = 0; i < nodesX; i++)
		{
			for (int j = 0; j < nodesY; j++)
			{
				for (int k = 0; k < nodesZ; k++)
				{
					int num = k * nodesY * nodesX + j * nodesX + i;
					roots[num] = default(OctNode);
					roots[num].GenerateFromHeightmap(map1, map2, map3, map4, (byte)type1, (byte)type2, (byte)type3, (byte)type4, i * nodeSize, j * nodeSize, k * nodeSize, nodeSize / 2);
				}
			}
		}
	}

	public void SetHeightmap(float[,] map, int type, int height, int nodeSize)
	{
		float[,] array = new float[map.GetLength(0), map.GetLength(1)];
		SetHeightmap(map, array, array, array, type, type, type, type, height, nodeSize);
	}

	public void HeightmapToTexture(float[,] map, float factor)
	{
		int length = map.GetLength(0);
		int length2 = map.GetLength(1);
		Texture2D texture2D = new Texture2D(length, length2);
		texture2D.name = "HeightmapToTexture";
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				texture2D.SetPixel(i, j, new Color(map[i, j] * factor - 2f, map[i, j] * factor - 1f, map[i, j] * factor));
			}
		}
		texture2D.Apply();
	}

	public void SubstractHeightmap(float[,] map1, float[,] map2, float[,] map3, float[,] map4, float[,] substract, float factor)
	{
		int length = map1.GetLength(0);
		int length2 = map1.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				float num = substract[i, j] * factor;
				float num2 = map4[i, j];
				map4[i, j] = Mathf.Max(0f, num2 - num);
				num = Mathf.Max(0f, num - num2);
				num2 = map3[i, j];
				map3[i, j] = Mathf.Max(0f, num2 - num);
				num = Mathf.Max(0f, num - num2);
				num2 = map2[i, j];
				map2[i, j] = Mathf.Max(0f, num2 - num);
				num = Mathf.Max(0f, num - num2);
				num2 = map1[i, j];
				map1[i, j] = Mathf.Max(0f, num2 - num);
				num = Mathf.Max(0f, num - num2);
				substract[i, j] -= num;
			}
		}
	}

	public float[,] SumHeightmaps(float[,] map1, float[,] map2, float[,] map3, float[,] map4)
	{
		int length = map1.GetLength(0);
		int length2 = map1.GetLength(1);
		float[,] array = new float[length, length2];
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				array[i, j] = Mathf.Clamp(map1[i, j] + map2[i, j] + map3[i, j] + map4[i, j], 0f, sizeY - 1);
			}
		}
		return array;
	}

	public void GeneratePlanar(float[,] map, int level)
	{
		int length = map.GetLength(0);
		int length2 = map.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				map[i, j] = level;
			}
		}
	}

	public void GenerateFromTexture(float[,] map)
	{
	}

	public void GenerateNoise(float[,] map, int fractals, float fractalMin, float fractalMax, float valueMin, float valueMax)
	{
		int length = map.GetLength(0);
		int length2 = map.GetLength(1);
		float num = (fractalMax - fractalMin) / (float)fractals;
		float num2 = (valueMax - valueMin) / (float)fractals;
		for (int num3 = fractals - 1; num3 >= 0; num3--)
		{
			float num4 = fractalMin + num * (float)num3;
			float num5 = valueMin + num2 * (float)num3;
			float num6 = UnityEngine.Random.value * num4;
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					map[i, j] += Mathf.PerlinNoise((float)i / num4 + num6, (float)j / num4 + num6) * num5;
				}
			}
		}
	}

	public void GenerateErosionGullies(float[,] map, float[,] refHeights, int iterations, float mudAmount, float blurValue, int deblurIterations)
	{
		int length = map.GetLength(0);
		int length2 = map.GetLength(1);
		float num = 0f;
		float num2 = float.PositiveInfinity;
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				map[i, j] = refHeights[i, j];
				if (refHeights[i, j] > num)
				{
					num = refHeights[i, j];
				}
				if (refHeights[i, j] < num2)
				{
					num2 = refHeights[i, j];
				}
			}
		}
		for (int k = 0; k < iterations; k++)
		{
			Vector4[,] array = new Vector4[length, length2];
			float[,] array2 = new float[length, length2];
			float[,] array3 = new float[length, length2];
			for (int l = 1; l < length - 1; l++)
			{
				for (int m = 1; m < length2 - 1; m++)
				{
					Vector4 vector = new Vector4(Mathf.Max(map[l - 1, m] - map[l + 1, m], 0f), Mathf.Max(map[l + 1, m] - map[l - 1, m], 0f), Mathf.Max(map[l, m - 1] - map[l, m + 1], 0f), Mathf.Max(map[l, m + 1] - map[l, m - 1], 0f));
					float num3 = vector.x + vector.y + vector.z + vector.w;
					if (num3 > 0.1f)
					{
						array[l, m] = vector / num3;
					}
					else
					{
						array[l, m] = new Vector4(0.25f, 0.25f, 0.25f, 0.25f);
					}
					float b = Mathf.Max(map[l, m] - map[l - 1, m], map[l, m] - map[l + 1, m]);
					b = Mathf.Max(map[l, m] - map[l, m - 1], b);
					b = Mathf.Max(map[l, m] - map[l, m + 1], b);
					array2[l, m] = b;
					array3[l, m] = Mathf.Max((map[l - 1, m] + map[l + 1, m] + map[l, m - 1] + map[l, m + 1]) * 0.25f - map[l, m], 0f) * 0.5f;
				}
			}
			float[,] array4 = new float[length, length2];
			float[,] array5 = new float[length, length2];
			int[] array6 = new int[length * length2];
			for (int n = 0; n < length; n++)
			{
				for (int num4 = 0; num4 < length2; num4++)
				{
					array4[n, num4] = 1f;
					array6[num4 * sizeX + n] = (int)(map[n, num4] * 4f);
				}
			}
			int num5 = (int)(num * 4f);
			while ((float)num5 >= num2 - 5f)
			{
				for (int num6 = 1; num6 < length2 - 1; num6++)
				{
					int num7 = num6 * sizeX;
					for (int num8 = 1; num8 < length - 1; num8++)
					{
						if (array6[num7 + num8] == num5)
						{
							Vector4 vector2 = array[num8, num6];
							float num9 = array4[num8, num6];
							array4[num8 + 1, num6] += num9 * vector2.x;
							array4[num8 - 1, num6] += num9 * vector2.y;
							array4[num8, num6 + 1] += num9 * vector2.z;
							array4[num8, num6 - 1] += num9 * vector2.w;
							array5[num8, num6] += array4[num8, num6];
							array4[num8, num6] = 0f;
						}
					}
				}
				num5--;
			}
			for (int num10 = 0; num10 < length; num10++)
			{
				for (int num11 = 0; num11 < length2; num11++)
				{
					map[num10, num11] -= Mathf.Clamp(array5[num10, num11], 0f, 100f) * array2[num10, num11] * mudAmount;
				}
			}
			for (int num12 = 1; num12 < length - 1; num12++)
			{
				for (int num13 = 1; num13 < length2 - 1; num13++)
				{
					map[num12, num13] = map[num12, num13] * (1f - blurValue) + map[num12 + 1, num13] * blurValue * 0.25f + map[num12 - 1, num13] * blurValue * 0.25f + map[num12, num13 - 1] * blurValue * 0.25f + map[num12, num13 + 1] * blurValue * 0.25f;
				}
			}
		}
		for (int num14 = 0; num14 < length; num14++)
		{
			for (int num15 = 0; num15 < length2; num15++)
			{
				map[num14, num15] = Mathf.Max(0f, refHeights[num14, num15] - map[num14, num15]);
			}
		}
		for (int num16 = 0; num16 < length; num16++)
		{
			for (int num17 = 0; num17 < length2; num17++)
			{
				map[num16, num17] += UnityEngine.Random.value * genErosionGulliesWind;
			}
		}
	}

	public void GenerateSediment(float[,] map, float[,] refHeights, int blurIterations, int hollowIterations, float deep)
	{
		int length = map.GetLength(0);
		int length2 = map.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				map[i, j] = refHeights[i, j] + genErosionSedimentAdjustLevel;
			}
		}
		for (int k = 0; k < blurIterations; k++)
		{
			for (int l = 1; l < length - 1; l++)
			{
				for (int m = 1; m < length2 - 1; m++)
				{
					map[l, m] = Mathf.Max(0f, map[l, m] * 0.5f + map[l + 1, m] * 0.125f + map[l - 1, m] * 0.125f + map[l, m - 1] * 0.125f + map[l, m + 1] * 0.125f);
				}
			}
		}
		for (int n = 0; n < length; n++)
		{
			for (int num = 0; num < length2; num++)
			{
				map[n, num] = Mathf.Max(0f, map[n, num] - refHeights[n, num]) * deep;
			}
		}
	}

	public void GenerateCover(float[,] map, float[,] refHeights, float blurIterations, float normalDelta, float pikesDelta)
	{
		int length = map.GetLength(0);
		int length2 = map.GetLength(1);
		float[,] array = new float[length, length2];
		float[,] array2 = new float[length, length2];
		for (int i = 1; i < length - 1; i++)
		{
			for (int j = 1; j < length2 - 1; j++)
			{
				array[i, j] = refHeights[i - 1, j] * 0.25f + refHeights[i + 1, j] * 0.25f + refHeights[i, j - 1] * 0.25f + refHeights[i, j + 1] * 0.25f - refHeights[i, j];
				array2[i, j] = Mathf.Abs(map[i - 1, j] + refHeights[i - 1, j] - (map[i + 1, j] + refHeights[i + 1, j])) + Mathf.Abs(map[i, j - 1] + refHeights[i, j - 1] - (map[i, j + 1] + refHeights[i, j + 1]));
			}
		}
		for (int k = 0; (float)k < blurIterations; k++)
		{
			for (int l = 1; l < length - 1; l++)
			{
				for (int m = 1; m < length2 - 1; m++)
				{
					array[l, m] = array[l, m] * 0.33f + array[l + 1, m] * 0.1675f + array[l - 1, m] * 0.1675f + array[l, m - 1] * 0.1675f + array[l, m + 1] * 0.1675f;
					array2[l, m] = array2[l, m] * 0.33f + array2[l + 1, m] * 0.1675f + array2[l - 1, m] * 0.1675f + array2[l, m - 1] * 0.1675f + array2[l, m + 1] * 0.1675f;
				}
			}
		}
		for (int n = 0; n < length; n++)
		{
			for (int num = 0; num < length2; num++)
			{
				if (1f / array2[n, num] - normalDelta > 0f)
				{
					map[n, num] += 1f;
				}
				if (array[n, num] - pikesDelta > 0f)
				{
					map[n, num] += 1f;
				}
				map[n, num] = Mathf.Clamp01(map[n, num]);
			}
		}
	}

	public void GenerateAll()
	{
		float[,] array = new float[newSizeX, newSizeZ];
		float num = 0f;
		float num2 = 0f;
		if (guiGenerateCheck[1])
		{
			num2 += 1f;
		}
		if (guiGenerateCheck[2])
		{
			num2 += 2f;
		}
		if (guiGenerateCheck[3])
		{
			num2 += 5f;
		}
		if (guiGenerateCheck[4])
		{
			num2 += 5f;
		}
		if (guiGenerateCheck[5])
		{
			num2 += 2f;
		}
		float[,] array2;
		if (guiGenerateCheck[1])
		{
			DisplayProgress("Planar", 0f);
			num += 1f;
			array2 = new float[newSizeX, newSizeZ];
			GeneratePlanar(array2, genPlanarLevel);
		}
		else
		{
			array2 = array;
		}
		float[,] array3;
		if (guiGenerateCheck[2])
		{
			DisplayProgress("Noise", num / num2);
			num += 2f;
			array3 = new float[newSizeX, newSizeZ];
			GenerateNoise(array3, genNoiseFractals, genNoiseFractalMin, genNoiseFractalMax, genNoiseValueMin, genNoiseValueMax);
		}
		else
		{
			array3 = array;
		}
		if (guiGenerateCheck[3])
		{
			DisplayProgress("Erosion Gullies", num / num2);
			num += 5f;
			float[,] array4 = new float[newSizeX, newSizeZ];
			GenerateErosionGullies(array4, SumHeightmaps(array2, array3, array, array), genErosionGulliesIterations, genErosionGulliesMudAmount, genErosionGulliesBlurValue, genErosionGulliesDeblur);
			SubstractHeightmap(array2, array3, array, array, array4, 1f);
		}
		float[,] array5;
		if (guiGenerateCheck[4])
		{
			DisplayProgress("Erosion Sediment", num / num2);
			num += 3f;
			array5 = new float[newSizeX, newSizeZ];
			GenerateSediment(array5, SumHeightmaps(array2, array3, array, array), genErosionSedimentBlurIterations, genErosionSedimentHollowIterations, genErosionSedimentDepth);
			SubstractHeightmap(array2, array3, array, array, array5, 0.5f);
		}
		else
		{
			array5 = array;
		}
		float[,] array6;
		if (guiGenerateCheck[5])
		{
			DisplayProgress("Cover", num / num2);
			num += 2f;
			array6 = new float[newSizeX, newSizeZ];
			GenerateCover(array6, SumHeightmaps(array2, array3, array5, array), genCoverBlurIterations, genCoverPlanarDelta, genCoverPikesDelta);
			SubstractHeightmap(array2, array3, array5, array, array6, 1f);
		}
		else
		{
			array6 = array;
		}
		DisplayProgress("Setting Heightmap", 0.99f);
		SetHeightmap(array2, array3, array5, array6, genPlanarType, genNoiseType, genErosionSedimentType, genCoverType, newSizeY, newBiggestNode);
		sizeX = newSizeX;
		sizeY = newSizeY;
		sizeZ = newSizeZ;
		biggestNode = newBiggestNode;
	}

	public void DisplayProgress(string info, float progress)
	{
	}

	public void OnDestroy()
	{
		FreeOctrees();
	}

	public void OnApplicationQuit()
	{
		FreeOctrees();
	}

	public void TabulateTypeUsage(int[] type2count)
	{
		OctNode[] array = roots;
		foreach (OctNode octNode in array)
		{
			octNode.TabulateTypeUsage(type2count);
		}
	}

	public void ApplyTypeConversionTable(byte[] type2new)
	{
		OctNode[] array = roots;
		foreach (OctNode octNode in array)
		{
			octNode.ApplyTypeConversionTable(type2new);
		}
		CollapseOctree();
	}
}
