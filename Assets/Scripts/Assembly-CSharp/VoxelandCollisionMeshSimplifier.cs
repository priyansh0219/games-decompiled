using System;
using Gendarme;
using UWE;
using UnityEngine;

public class VoxelandCollisionMeshSimplifier : IEstimateBytes
{
	[Serializable]
	public class Settings
	{
		public int prewarmTriangleCount;

		public int prewarmVertexCount;

		public int triangleSimplifyCutoff = 100;

		public int vertexSimplifyCutoff = 100;

		public bool skipRandomPhase;
	}

	private static readonly int[] LowResFaceVerts = new int[4] { 0, 2, 4, 6 };

	private readonly MeshBuffer meshBuffer = new MeshBuffer();

	[NonSerialized]
	private Settings _settings;

	private SimplifyMeshPlugin.Face[] simplifyTris;

	private Vector3[] simplifyVertices;

	private byte[] simplifyFixed;

	private int[] simplifyOld2New;

	private MeshBufferPools meshBufferPools;

	public bool inUse { get; set; }

	public Settings settings
	{
		get
		{
			return _settings;
		}
		set
		{
			_settings = value;
			if (_settings.prewarmVertexCount > 0)
			{
				if (simplifyVertices == null)
				{
					simplifyVertices = new Vector3[_settings.prewarmVertexCount];
				}
				if (simplifyFixed == null)
				{
					simplifyFixed = new byte[_settings.prewarmVertexCount];
				}
				if (simplifyOld2New == null)
				{
					simplifyOld2New = new int[_settings.prewarmVertexCount];
				}
			}
			if (settings.prewarmTriangleCount > 0 && simplifyTris == null)
			{
				simplifyTris = new SimplifyMeshPlugin.Face[_settings.prewarmTriangleCount];
			}
		}
	}

	public void SetPools(MeshBufferPools pools)
	{
		meshBufferPools = pools;
	}

	public long EstimateBytes()
	{
		if (simplifyVertices == null)
		{
			return -1L;
		}
		return simplifyTris.Length * SimplifyMeshPlugin.Face.SizeBytes + simplifyVertices.Length * 4 * 3 + simplifyFixed.Length + simplifyOld2New.Length * 4;
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public void AttachTo(IVoxelandChunk2 chunk, TerrainPoolManager terrainPoolManager)
	{
		if (meshBuffer.isDegenerateCollider || meshBuffer.numTris < 1)
		{
			meshBuffer.Return();
			return;
		}
		TerrainChunkPiece terrainChunkPiece = terrainPoolManager.Get(TerrainChunkPieceType.Collider, chunk.transform);
		terrainPoolManager.SetTerrainPiecePositionAndScale(chunk.transform, terrainChunkPiece);
		MeshCollider meshCollider = terrainChunkPiece.meshCollider;
		terrainChunkPiece.gameObject.layer = 30;
		chunk.collision = terrainChunkPiece.meshCollider;
		chunk.chunkPieces.Add(terrainChunkPiece);
		Mesh meshForPiece = terrainPoolManager.GetMeshForPiece(terrainChunkPiece);
		meshBuffer.Upload(meshForPiece, keepVertexLayout: false);
		if (SNUtils.VerboseDebug)
		{
			Debug.LogFormat("setting collider sharedmesh to #verts = {0}, #inds = {1}", meshBuffer.numVerts, meshBuffer.numTris);
			UWE.Utils.DumpOBJFile(SNUtils.InsideDevTemp("last_colmesh.obj"), meshBuffer);
		}
		try
		{
			meshCollider.sharedMesh = meshForPiece;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			meshCollider.sharedMesh = null;
		}
		if (SNUtils.VerboseDebug)
		{
			Debug.Log("after set sharedMesh");
		}
		meshBuffer.Return();
	}

	public void Build(VoxelandChunkWorkspace ws, bool writeOutput = false)
	{
		if (ws.visibleFaces.Count == 0)
		{
			meshBuffer.Clear();
			return;
		}
		for (int i = 0; i < ws.verts.Count; i++)
		{
			ws.verts[i].layerVertIndex = -1;
			ws.verts[i].layerLowVertIndex = -1;
		}
		int numFaces = 2 * ws.visibleFaces.Count;
		int num = LowResFaceVerts.Length;
		SimplifyMeshPlugin.Face[] array = UWE.Utils.EnsureMinSize("simplifyTris", ref simplifyTris, numFaces);
		int numVerts = 0;
		for (int j = 0; j < ws.visibleFaces.Count; j++)
		{
			VoxelandChunk.VoxelandFace voxelandFace = ws.visibleFaces[j];
			for (int k = 0; k < num; k++)
			{
				if (voxelandFace.verts[LowResFaceVerts[k]].layerLowVertIndex == -1)
				{
					voxelandFace.verts[LowResFaceVerts[k]].layerLowVertIndex = numVerts++;
				}
			}
			array[2 * j].a = voxelandFace.verts[0].layerLowVertIndex;
			array[2 * j].b = voxelandFace.verts[2].layerLowVertIndex;
			array[2 * j].c = voxelandFace.verts[4].layerLowVertIndex;
			array[2 * j + 1].a = voxelandFace.verts[0].layerLowVertIndex;
			array[2 * j + 1].b = voxelandFace.verts[4].layerLowVertIndex;
			array[2 * j + 1].c = voxelandFace.verts[6].layerLowVertIndex;
		}
		Vector3[] array2 = UWE.Utils.EnsureMinSize("simplifyVertices", ref simplifyVertices, numVerts);
		byte[] array3 = UWE.Utils.EnsureMinSize("simplifyFixed", ref simplifyFixed, numVerts);
		foreach (VoxelandChunk.VoxelandVert vert in ws.verts)
		{
			if (vert.layerLowVertIndex != -1)
			{
				array2[vert.layerLowVertIndex] = vert.pos;
				array3[vert.layerLowVertIndex] = Convert.ToByte(vert.ComputeIsOnChunkBorder() ? 1 : 0);
			}
		}
		if (numFaces > settings.triangleSimplifyCutoff && numVerts > settings.vertexSimplifyCutoff)
		{
			int[] array4 = UWE.Utils.EnsureMinSize("simplifyOld2New", ref simplifyOld2New, numVerts);
			if (writeOutput)
			{
				Debug.Log(numVerts + "/" + numFaces);
			}
			SimplifyMeshPlugin.SimplifyMesh(0.8f, 0f, ref array2[0], ref array3[0], ref numVerts, ref array[0], ref numFaces, ref array4[0], settings.skipRandomPhase, writeOutput);
			if (numFaces == 0)
			{
				Debug.Log("WARNING WARNING WARNING: Some how ended up with 0 tris after simplification!");
				meshBuffer.Clear();
				return;
			}
			if (numVerts == 0)
			{
				Debug.Log("WARNING WARNING WARNING: Some how ended up with 0 verts but " + numFaces + " tris after simp..??");
				meshBuffer.Clear();
				return;
			}
		}
		meshBuffer.Acquire(meshBufferPools, numVerts, numFaces * 3, MeshBuffer.MeshBufferType.Collider);
		for (int l = 0; l < numVerts; l++)
		{
			meshBuffer.colliderVertices[l] = new TerrainColliderVertex
			{
				position = array2[l]
			};
		}
		for (int m = 0; m < numFaces; m++)
		{
			meshBuffer.triangles[3 * m] = (ushort)array[m].a;
			meshBuffer.triangles[3 * m + 1] = (ushort)array[m].b;
			meshBuffer.triangles[3 * m + 2] = (ushort)array[m].c;
		}
		for (int n = 0; n < numVerts; n++)
		{
			TerrainColliderVertex terrainColliderVertex = meshBuffer.colliderVertices[n];
			if (terrainColliderVertex.position.HasAnyNaNs() || terrainColliderVertex.position.HasAnyInfs())
			{
				Vector3 position = ((n > 0) ? meshBuffer.colliderVertices[n - 1].position : Vector3.zero);
				meshBuffer.colliderVertices[n] = new TerrainColliderVertex
				{
					position = position
				};
			}
		}
		meshBuffer.RecalculateBoundsThreaded();
		meshBuffer.DetermineIfDegenerateThreaded();
	}
}
