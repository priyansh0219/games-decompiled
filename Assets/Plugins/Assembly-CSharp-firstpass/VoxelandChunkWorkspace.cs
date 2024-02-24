using System;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;

[Serializable]
public class VoxelandChunkWorkspace : IEstimateBytes
{
	private class Pool<T> where T : new()
	{
		private List<T> items;

		private int currentIndex;

		public int Count => items.Count;

		public Pool(int initialSize)
		{
			items = new List<T>(initialSize);
			for (int i = 0; i < initialSize; i++)
			{
				items.Add(new T());
			}
		}

		public T Get()
		{
			T val;
			if (currentIndex < items.Count)
			{
				val = items[currentIndex];
			}
			else
			{
				val = new T();
				items.Add(val);
			}
			currentIndex++;
			return val;
		}

		public void Reset()
		{
			currentIndex = 0;
		}
	}

	[Serializable]
	public struct ChunkFaceId
	{
		public byte dx;

		public byte dy;

		public byte dz;

		public byte dir;

		public const int Bytes = 4;

		public override string ToString()
		{
			return dx + "," + dy + "," + dz + ":" + dir;
		}

		public void Reset()
		{
			dx = 0;
			dy = 0;
			dz = 0;
			dir = 0;
		}
	}

	private const int blockPoolInitialSize = 5200;

	private const int facePoolInitialSize = 9300;

	private const int vertPoolInitialSize = 14800;

	private const int grassVertPoolInitialSize = 0;

	private const int grassTriPoolInitialSize = 0;

	[NonSerialized]
	private readonly Pool<VoxelandChunk.VoxelandBlock> blockPool = new Pool<VoxelandChunk.VoxelandBlock>(5200);

	[NonSerialized]
	private readonly Pool<VoxelandChunk.VoxelandFace> facePool = new Pool<VoxelandChunk.VoxelandFace>(9300);

	[NonSerialized]
	private readonly Pool<VoxelandChunk.VoxelandVert> vertPool = new Pool<VoxelandChunk.VoxelandVert>(14800);

	[NonSerialized]
	public Array3<VoxelandChunk.VoxelandBlock> blocks;

	public Int3 blocksLen;

	public int maxMeshRes;

	[NonSerialized]
	public readonly List<VoxelandChunk.VoxelandFace> faces = new List<VoxelandChunk.VoxelandFace>();

	[NonSerialized]
	public readonly List<VoxelandChunk.VoxelandFace> visibleFaces = new List<VoxelandChunk.VoxelandFace>();

	[NonSerialized]
	public readonly List<VoxelandChunk.VoxelandVert> verts = new List<VoxelandChunk.VoxelandVert>();

	public Voxeland.RasterWorkspace rws;

	private readonly Pool<VoxelandChunk.VLGrassVert> grassVertPool = new Pool<VoxelandChunk.VLGrassVert>(0);

	private readonly Pool<VoxelandChunk.VLGrassTri> grassTriPool = new Pool<VoxelandChunk.VLGrassTri>(0);

	[NonSerialized]
	public readonly List<VoxelandChunk.VLGrassMesh> grassMeshes = new List<VoxelandChunk.VLGrassMesh>();

	public int nextGrassMesh;

	[NonSerialized]
	public ChunkFaceId[] faceList;

	private static int nextWorkspaceId;

	private int workspaceId;

	[NonSerialized]
	public VoxelandChunk.VoxelandFace[] layerFaces;

	[NonSerialized]
	public VoxelandChunk.VoxelandVert[] layerVerts;

	[NonSerialized]
	public VoxelandChunk.VoxelandVert[] lowLayerVerts;

	public int WorkspaceId => workspaceId;

	public long EstimateBytes()
	{
		return rws.EstimateBytes() + blockPool.Count * VoxelandChunk.VoxelandBlock.EstimateBytes() + facePool.Count * VoxelandChunk.VoxelandFace.EstimateBytes() + vertPool.Count * VoxelandChunk.VoxelandVert.EstimateBytes() + blocks.Length * 4 + faceList.Length * 4;
	}

	public void LogMemoryProfile()
	{
		Debug.LogFormat("RWS: {0}", (float)rws.EstimateBytes() / 1024f / 1024f);
		Debug.LogFormat("blockPool: {0}", (float)(blockPool.Count * VoxelandChunk.VoxelandBlock.EstimateBytes()) / 1024f / 1024f);
		Debug.LogFormat("facePool: {0}", (float)(facePool.Count * VoxelandChunk.VoxelandFace.EstimateBytes()) / 1024f / 1024f);
		Debug.LogFormat("vertPool: {0}", (float)(vertPool.Count * VoxelandChunk.VoxelandVert.EstimateBytes()) / 1024f / 1024f);
		Debug.LogFormat("blocks grid: {0}", (float)(blocks.Length * 4) / 1024f / 1024f);
		Debug.LogFormat("faceList: {0}", (float)(faceList.Length * 4) / 1024f / 1024f);
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	public VoxelandChunkWorkspace()
	{
		maxMeshRes = 0;
		workspaceId = ++nextWorkspaceId;
	}

	public VoxelandChunkWorkspace(int maxMeshRes)
	{
		SetSize(maxMeshRes);
	}

	public void SetSize(int meshRes)
	{
		int num = meshRes + 4;
		blocksLen = new Int3(num, num, num);
		if (maxMeshRes < meshRes)
		{
			maxMeshRes = meshRes;
			blocks = new Array3<VoxelandChunk.VoxelandBlock>(num, num, num);
			int num2 = maxMeshRes + 6;
			faceList = new ChunkFaceId[num2 * num2 * num2];
			for (int i = 0; i < faceList.Length; i++)
			{
				faceList[i].Reset();
			}
		}
		blockPool.Reset();
		facePool.Reset();
		vertPool.Reset();
		blocks.Clear();
		faces.Clear();
		visibleFaces.Clear();
		verts.Clear();
		grassVertPool.Reset();
		grassTriPool.Reset();
		nextGrassMesh = 0;
		rws.SetSize(meshRes);
	}

	public VoxelandChunk.VoxelandBlock NewBlock(int x, int y, int z)
	{
		return blockPool.Get().Reset(x, y, z);
	}

	public VoxelandChunk.VoxelandFace NewFace()
	{
		return facePool.Get().Reset();
	}

	public VoxelandChunk.VoxelandVert NewVert()
	{
		return vertPool.Get().Reset();
	}

	public VoxelandChunk.VLGrassVert NewGrassVert()
	{
		return grassVertPool.Get();
	}

	public VoxelandChunk.VLGrassTri NewGrassTri()
	{
		return grassTriPool.Get();
	}

	public string GetPoolSizeInfo()
	{
		return $"wsPools> block:{blockPool.Count} face:{facePool.Count} vert:{vertPool.Count} grassVert:{grassVertPool.Count} grassTri:{grassTriPool.Count}";
	}
}
