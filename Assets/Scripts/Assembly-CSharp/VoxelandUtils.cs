using System.Collections.Generic;
using UWE;
using Unity.Mathematics;
using UnityEngine;

public static class VoxelandUtils
{
	public class TransformedGrid : IVoxelGrid
	{
		public Int3 ofs;

		public Int3 size;

		public int ccwRotations;

		public bool useTileTransform = true;

		public IVoxelGrid src;

		public List<byte> typeMap;

		public void SetSize(VoxelandData data)
		{
			size = new Int3(data.sizeX, data.sizeY, data.sizeZ);
		}

		private Int3 ToSrcSpace(Int3 dsPos)
		{
			if (useTileTransform)
			{
				return Int3.InverseTileTransform(dsPos, size, ofs, ccwRotations);
			}
			return dsPos.RotateXZ(4 - ccwRotations, ofs.ToVector3()) - ofs;
		}

		public bool GetVoxelMask(int x, int y, int z)
		{
			Int3 @int = ToSrcSpace(new Int3(x, y, z));
			return src.GetVoxelMask(@int.x, @int.y, @int.z);
		}

		public VoxelandData.OctNode GetVoxel(int x, int y, int z)
		{
			ToSrcSpace(new Int3(x, y, z));
			VoxelandData.OctNode voxel = src.GetVoxel(x, y, z);
			if (typeMap != null)
			{
				voxel.type = typeMap[voxel.type];
			}
			return voxel;
		}
	}

	public class DenseBufferProxy : IVoxelGrid
	{
		public Array3<byte> types;

		public Array3<byte> densities;

		public Int3 origin;

		public VoxelandData.OctNode GetVoxel(int x, int y, int z)
		{
			VoxelandData.OctNode result = default(VoxelandData.OctNode);
			Int3 @int = new Int3(x, y, z) - origin;
			if (@int < types.Dims())
			{
				result.type = types.Get(@int);
				result.density = densities.Get(@int);
				return result;
			}
			return VoxelandData.OctNode.EmptyNode();
		}

		public bool GetVoxelMask(int x, int y, int z)
		{
			Int3 @int = new Int3(x, y, z);
			if (@int >= origin)
			{
				return @int < origin + types.Dims();
			}
			return false;
		}
	}

	public class BoundingBoxReadWrite : WorldReadWrite
	{
		private VoxelandData data;

		private Int3 mins;

		private Int3 maxs;

		public BoundingBoxReadWrite(VoxelandData data, Int3 mins, Int3 maxs)
		{
			this.data = data;
			this.mins = mins;
			this.maxs = maxs;
		}

		public byte Get(int x, int y, int z)
		{
			Int3 @int = new Int3(x, y, z);
			if (@int > maxs || @int < mins)
			{
				return 0;
			}
			return data.GetVoxel(x, y, z).type;
		}

		public void Set(int x, int y, int z, byte type)
		{
			Int3 @int = new Int3(x, y, z);
			if (!(@int > maxs) && !(@int < mins))
			{
				data.SetBlock(x, y, z, type, 0, skipCollapse: true, threaded: true);
			}
		}

		public bool CanWrite(int x, int y, int z)
		{
			Int3 @int = new Int3(x, y, z);
			if (@int >= mins)
			{
				return @int <= maxs;
			}
			return false;
		}

		public bool CanWrite(Int3 mins, Int3 maxs)
		{
			if (mins >= this.mins)
			{
				return maxs <= this.maxs;
			}
			return false;
		}

		public Int3 GetMins()
		{
			return mins;
		}

		public Int3 GetMaxs()
		{
			return maxs;
		}
	}

	public delegate void RootHandler(Int3 root);

	public delegate bool AbortRequested();

	public class SolidVoxelGrid : IVoxelGrid
	{
		public byte type;

		public Int3.Bounds bounds;

		public SolidVoxelGrid(Int3.Bounds bounds, byte type)
		{
			this.bounds = bounds;
			this.type = type;
		}

		public VoxelandData.OctNode GetVoxel(int x, int y, int z)
		{
			return new VoxelandData.OctNode(type, 0);
		}

		public bool GetVoxelMask(int x, int y, int z)
		{
			return bounds.Contains(new Int3(x, y, z));
		}
	}

	public class SurfacePosition
	{
		public Vector3 position;

		public Vector3 normal;

		public Quaternion quat;

		public byte typeNum;
	}

	public static void LayoutDebugGUI(Voxeland land)
	{
		GUILayout.BeginVertical("box");
		GUILayout.Label("-- VoxelandUtils.LayoutDebugGUI --");
		GUILayout.BeginHorizontal("box");
		GUILayout.Label("LOD distance (current: " + land.lodDistance + "):");
		land.lodDistance = Mathf.FloorToInt(GUILayout.HorizontalScrollbar(land.lodDistance, 0f, 1f, 150f));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal("box");
		if (GUILayout.Button("Collisions? " + land.generateCollider))
		{
			land.generateCollider = !land.generateCollider;
			land.DestroyAllChunks();
		}
		if (GUILayout.Button("Shadows? " + land.castShadows))
		{
			land.castShadows = !land.castShadows;
			land.DestroyAllChunks();
		}
		if (GUILayout.Button("Hide meshes? " + land.debugFreezeLOD))
		{
			land.debugFreezeLOD = !land.debugFreezeLOD;
			if (land.debugFreezeLOD)
			{
				land.HideAllChunkRenders();
			}
		}
		if (GUILayout.Button("Dummy material? " + land.debugUseDummyMaterial))
		{
			land.debugUseDummyMaterial = !land.debugUseDummyMaterial;
			land.DestroyAllChunks();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal("box");
		GUILayout.Label("Chunk size (" + land.chunkSize + "):");
		if (GUILayout.Button("8"))
		{
			land.chunkSize = 8;
			land.Rebuild();
		}
		if (GUILayout.Button("16"))
		{
			land.chunkSize = 16;
			land.Rebuild();
		}
		if (GUILayout.Button("24"))
		{
			land.chunkSize = 24;
			land.Rebuild();
		}
		if (GUILayout.Button("32"))
		{
			land.chunkSize = 32;
			land.Rebuild();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal("box");
		if (GUILayout.Button("Rebuild all chunks"))
		{
			land.DestroyAllChunks();
		}
		if (GUILayout.Button("Rebuild closest chunk"))
		{
			land.BuildBestChunk(MainCamera.camera.transform.position, MainCamera.camera.transform.forward, forceRebuild: true);
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	public static Voxeland CreatePreviewLand(Transform parent)
	{
		GameObject gameObject = new GameObject("DO NOT EDIT - preview voxeland");
		gameObject.transform.parent = parent;
		UWE.Utils.ZeroTransform(gameObject.transform);
		Voxeland voxeland = gameObject.AddComponent<Voxeland>();
		voxeland.OnEditorCreate();
		voxeland.chunkSize = 16;
		voxeland.dynamicRebuilding = false;
		voxeland.readOnly = true;
		voxeland.freeze = true;
		return voxeland;
	}

	public static Int3 GetSize(this VoxelandData data)
	{
		return new Int3(data.sizeX, data.sizeY, data.sizeZ);
	}

	public static Int3.Bounds BlockBounds(this VoxelandData data)
	{
		return new Int3.Bounds(Int3.zero, data.GetSize() - 1);
	}

	public static Int3 GetNodeCount(this VoxelandData data)
	{
		return new Int3(data.nodesX, data.nodesY, data.nodesZ);
	}

	public static VoxelandData.OctNode GetNode(this VoxelandData data, Int3 b)
	{
		return data.GetNode(b.x, b.y, b.z);
	}

	public static Int3 ToInt3(this VoxelandCoords c)
	{
		return new Int3(c.x, c.y, c.z);
	}

	public static void SetForRange(this VoxelandData data, IVoxelGrid input, Int3 start, Int3 count, VoxelandData.OctNode.BlendArgs blend)
	{
		data.SetForRange(input, start.x, start.y, start.z, count.x, count.y, count.z, blend);
	}

	public static void CollapseRelevant(this VoxelandData data, Int3 mins, Int3 maxs)
	{
		data.CollapseRelevant(mins.x, mins.y, mins.z, maxs.x, maxs.y, maxs.z);
	}

	public static int GetRootIndex(this VoxelandData data, Int3 root)
	{
		return data.GetRootIndex(root.x, root.y, root.z);
	}

	public static Int3.Bounds GetRootBounds(this VoxelandData data, Int3 root)
	{
		return new Int3.Bounds(root * data.biggestNode, (root + 1) * data.biggestNode - 1);
	}

	public static void ClearRange(this VoxelandData data, Int3.Bounds blocks)
	{
		foreach (Int3 item in blocks / data.biggestNode)
		{
			int rootIndex = data.GetRootIndex(item.x, item.y, item.z);
			if (rootIndex != -1)
			{
				data.roots[rootIndex].Clear();
			}
		}
	}

	public static void UnionInto(this VoxelandData src, VoxelandData dest, Int3.Bounds blocks)
	{
		foreach (Int3 item in blocks / src.biggestNode)
		{
			int rootIndex = src.GetRootIndex(item.x, item.y, item.z);
			if (rootIndex != -1)
			{
				Int3 @int = item * src.biggestNode;
				dest.roots[rootIndex].SetBottomUp(src, @int.x, @int.y, @int.z, src.biggestNode / 2, new VoxelandData.OctNode.BlendArgs(VoxelandData.OctNode.BlendOp.Union, replaceTypes: true, 0));
				dest.roots[rootIndex].Collapse();
			}
		}
	}

	public static void SetBottomUp(this VoxelandData data, Int3 minBlock, Int3 maxBlock, IVoxelGrid input, Int3 inputStart, RootHandler onRootDone = null, AbortRequested checkAbort = null)
	{
		int biggestNode = data.biggestNode;
		foreach (Int3 item in Int3.Range(minBlock / biggestNode, maxBlock / biggestNode))
		{
			int rootIndex = data.GetRootIndex(item.x, item.y, item.z);
			if (rootIndex != -1)
			{
				data.roots[rootIndex].SetBottomUp(input, item.x * biggestNode + inputStart.x, item.y * biggestNode + inputStart.y, item.z * biggestNode + inputStart.z, biggestNode / 2);
				onRootDone?.Invoke(item);
				if (checkAbort != null && checkAbort())
				{
					break;
				}
			}
		}
	}

	public static void RebuildRelevantChunks(this Voxeland land, Int3.Bounds bounds)
	{
		land.RebuildRelevantChunks(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
	}

	public static Int3 GetContainingBlock(this Voxeland land, Vector3 wsPos)
	{
		return Int3.Floor(land.transform.InverseTransformPoint(wsPos));
	}

	public static Int3 GetClosestBlockCorner(this Voxeland land, Vector3 wsPos)
	{
		return Int3.Round(land.transform.InverseTransformPoint(wsPos));
	}

	public static Vector3 GetBlockWorldCenter(this Voxeland land, Int3 block)
	{
		return land.transform.TransformPoint(block.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f));
	}

	public static Vector3 GetBlockWorldCorner(this Voxeland land, Int3 block)
	{
		return land.transform.TransformPoint(block.ToVector3());
	}

	public static void Stamp(Voxeland srcLand, Voxeland destLand, bool recordUndo, bool rebuild)
	{
		srcLand.UpdateData();
		Int3 @int = Int3.Floor(destLand.transform.InverseTransformPoint(srcLand.transform.position));
		int cCWYawTurns = srcLand.transform.GetCCWYawTurns();
		srcLand.data.GetSize().RotateXZ(cCWYawTurns).Abs();
		TransformedGrid transformedGrid = new TransformedGrid();
		transformedGrid.src = srcLand.data;
		transformedGrid.SetSize(srcLand.data);
		transformedGrid.typeMap = destLand.MergeTypes(srcLand.types, errorIfAnyNew: true);
		transformedGrid.ofs = @int;
		transformedGrid.useTileTransform = false;
		transformedGrid.ccwRotations = cCWYawTurns;
		Int3.Bounds bounds = new Int3.Bounds(@int, @int + srcLand.data.GetSize() - 1).boundsRotateXZ(cCWYawTurns);
		if (recordUndo)
		{
			destLand.BeginUndoStep();
			destLand.RegisterExternalUndoRange(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
		}
		destLand.data.SetForRange(transformedGrid, bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.size.x, bounds.size.y, bounds.size.z);
		destLand.data.CollapseRelevant(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
		if (recordUndo)
		{
			destLand.EndUndoStep();
		}
		if (rebuild)
		{
			destLand.RebuildRelevantChunks(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
		}
	}

	public static int TopBlockForHeight(float h)
	{
		return Mathf.FloorToInt(h - 0.5f);
	}

	public static Int3 GetOffset(this IVoxelandChunk chunk)
	{
		return new Int3(chunk.offsetX, chunk.offsetY, chunk.offsetZ);
	}

	public static Int3 GetIndex(this VoxelandChunk chunk)
	{
		return new Int3(chunk.cx, chunk.cy, chunk.cz);
	}

	public static void SetPosition(this VoxelandChunk chunk, int downsamples, Int3 index, int sizeMeters, int overlap)
	{
		int num = 1 << downsamples;
		chunk.transform.localScale = new Vector3(num, num, num);
		chunk.meshRes = (sizeMeters >> downsamples) + overlap * 2;
		chunk.downsamples = downsamples;
		int num2 = overlap << downsamples;
		chunk.offsetX = index.x * sizeMeters - num2;
		chunk.offsetY = index.y * sizeMeters - num2;
		chunk.offsetZ = index.z * sizeMeters - num2;
		chunk.cx = index.x;
		chunk.cy = index.y;
		chunk.cz = index.z;
		chunk.transform.localPosition = new Vector3(chunk.offsetX, chunk.offsetY, chunk.offsetZ);
	}

	public static Int3.Bounds BlocksNeededToMeshChunk(this Int3 chunk, VoxelandData data, int downsamples, int meshRes)
	{
		int s = 3 << downsamples;
		int finePerCoarseCell = meshRes << downsamples;
		return chunk.Refined(finePerCoarseCell).Expanded(s).Clamp(data.BlockBounds());
	}

	public static Int3.Bounds RootsNeededToMeshChunk(this Int3 chunk, VoxelandData data, int downsamples, int meshRes)
	{
		return chunk.BlocksNeededToMeshChunk(data, downsamples, meshRes) / data.biggestNode;
	}

	public static ChunkState GetChunkState(this Voxeland land, Int3 chunkNum)
	{
		return land.chunkWindow[land.GetChunkWindowSlot(chunkNum.x, chunkNum.y, chunkNum.z)];
	}

	public static IEnumerable<Vector3> EnumerateWaterPositions(this Voxeland land, Voxeland.RasterWorkspace rws, Int3 blockOrigin, int chunkSize)
	{
		rws.SetSize(chunkSize);
		land.RasterizeVoxels(rws, blockOrigin.x, blockOrigin.y, blockOrigin.z, 0);
		foreach (Int3 item in Int3.Range(chunkSize))
		{
			byte type = rws.typesGrid[item.x, item.y, item.z];
			byte density = rws.densityGrid[item.x, item.y, item.z];
			if (!VoxelandData.OctNode.IsBelowSurface(type, density))
			{
				Int3 @int = blockOrigin + item;
				Vector3 vector = land.transform.TransformPoint(@int.ToVector3());
				yield return vector + new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
			}
		}
	}

	public static IEnumerable<SurfacePosition> EnumerateSurfacePositions(this Voxeland land, VoxelandChunk chunk, VoxelandTypeBase settings, int seed, Int3.Bounds blockRange)
	{
		SurfacePosition rv = new SurfacePosition();
		foreach (Int3 chunkNum in blockRange / land.chunkSize)
		{
			chunk.SetPosition(0, chunkNum, land.chunkSize, 0);
			chunk.skipHiRes = false;
			chunk.disableGrass = true;
			chunk.BuildMesh(skipRelax: false);
			Vector3 lsChunkOrigin = (chunkNum * land.chunkSize).ToVector3();
			Unity.Mathematics.Random chunkRng = VoxelandMisc.CreateRandom(chunkNum.GetHashCode());
			if (chunk.ws.visibleFaces.Count <= 0)
			{
				continue;
			}
			foreach (VoxelandChunk.GrassPos item in chunk.EnumerateGrass(settings, 0, seed, 0.0))
			{
				Int3 @int = new Int3(item.face.block.x, item.face.block.y, item.face.block.z) - 3;
				Int3 p = chunkNum * land.chunkSize + @int;
				if (blockRange.Contains(p))
				{
					item.ComputeTransform(ref chunkRng, settings);
					rv.position = land.transform.TransformPoint(lsChunkOrigin + item.csOrigin);
					rv.normal = land.transform.TransformDirection(item.faceNormal);
					rv.quat = land.transform.rotation * item.quat;
					rv.typeNum = item.face.type;
					yield return rv;
				}
			}
		}
	}
}
