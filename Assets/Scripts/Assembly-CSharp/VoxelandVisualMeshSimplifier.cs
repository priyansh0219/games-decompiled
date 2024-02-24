using System;
using Gendarme;
using UWE;
using UnityEngine;
using UnityEngine.Rendering;

public class VoxelandVisualMeshSimplifier : IEstimateBytes
{
	[Serializable]
	public class Settings
	{
		public bool useLowMesh;

		public bool skipSimplify;

		public SimplifyMeshPlugin.Settings simplify = new SimplifyMeshPlugin.Settings();
	}

	public enum State
	{
		None = 0,
		Ready = 1,
		BuffersReady = 2
	}

	private static readonly int[,] faceHiTri2Verts = new int[8, 3]
	{
		{ 7, 0, 1 },
		{ 1, 8, 7 },
		{ 8, 1, 2 },
		{ 2, 3, 8 },
		{ 5, 8, 3 },
		{ 3, 4, 5 },
		{ 6, 7, 8 },
		{ 8, 5, 6 }
	};

	private static readonly int[] hiFaceVerts = new int[9] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

	private static readonly int[,] faceLoTri2Verts = new int[2, 3]
	{
		{ 0, 2, 6 },
		{ 6, 2, 4 }
	};

	private static readonly int[] loFaceVerts = new int[4] { 0, 2, 4, 6 };

	[NonSerialized]
	public Settings settings;

	public bool debugUseMeshBuffers = true;

	public bool debugUseLQShader;

	public bool debugAllOpaque;

	public bool debugSkipMaterials;

	public static bool debugForceAlphaTest = false;

	private MeshBufferPools pools;

	private State state = State.Ready;

	private SimplifyMeshPlugin.Face[] simplifyTris;

	private Vector3[] simplifyVertices;

	private byte[] simplifyFixed;

	private int[] simplifyOld2New;

	private int numTris;

	private int numVerts;

	private int numOrigVisibleVerts;

	private int[] simp2wsVert;

	private bool[] simpTriInLayer;

	private int[] simp2layerVert;

	private VoxelandChunkWorkspace ws;

	private MeshBuffer[] builtLayers;

	private int numBuiltLayers;

	public bool inUse { get; set; }

	public long EstimateBytes()
	{
		if (settings == null || settings.skipSimplify)
		{
			return 0L;
		}
		return simplifyTris.Length * SimplifyMeshPlugin.Face.SizeBytes + 8 + simplifyVertices.Length * 4 * 3 + 8 + simplifyFixed.Length + 8 + simplifyOld2New.Length * 4 + 8 + simp2wsVert.Length * 4 + 8 + simpTriInLayer.Length + 8 + simp2layerVert.Length * 4 + 8;
	}

	public void Reset()
	{
		state = State.Ready;
		numTris = -1;
		numVerts = -1;
		numOrigVisibleVerts = -1;
	}

	public void PrepareBuffers(VoxelandChunkWorkspace ws)
	{
		this.ws = ws;
		int[,] array = (settings.useLowMesh ? faceLoTri2Verts : faceHiTri2Verts);
		int[] array2 = (settings.useLowMesh ? loFaceVerts : hiFaceVerts);
		for (int i = 0; i < ws.verts.Count; i++)
		{
			ws.verts[i].layerVertIndex = -1;
		}
		int length = array.GetLength(0);
		numTris = length * ws.visibleFaces.Count;
		SimplifyMeshPlugin.Face[] array3 = UWE.Utils.EnsureMinSize("simplifyTris", ref simplifyTris, numTris);
		numVerts = 0;
		for (int j = 0; j < ws.visibleFaces.Count; j++)
		{
			VoxelandChunk.VoxelandFace voxelandFace = ws.visibleFaces[j];
			foreach (int num in array2)
			{
				if (voxelandFace.verts[num].layerVertIndex == -1)
				{
					voxelandFace.verts[num].layerVertIndex = numVerts++;
				}
			}
			VoxelandChunk.VoxelandVert[] verts = voxelandFace.verts;
			for (int l = 0; l < length; l++)
			{
				int num2 = length * j + l;
				array3[num2].a = verts[array[l, 0]].layerVertIndex;
				array3[num2].b = verts[array[l, 1]].layerVertIndex;
				array3[num2].c = verts[array[l, 2]].layerVertIndex;
				array3[num2].material = voxelandFace.type;
			}
		}
		Vector3[] array4 = UWE.Utils.EnsureMinSize("simplifyVertices", ref simplifyVertices, numVerts);
		byte[] array5 = UWE.Utils.EnsureMinSize("simplifyFixed", ref simplifyFixed, numVerts);
		foreach (VoxelandChunk.VoxelandVert vert in ws.verts)
		{
			if (vert.layerVertIndex != -1)
			{
				array4[vert.layerVertIndex] = vert.pos;
				array5[vert.layerVertIndex] = Convert.ToByte(vert.ComputeIsOnChunkBorder() ? 1 : 0);
			}
		}
		numOrigVisibleVerts = numVerts;
		state = State.BuffersReady;
	}

	public void DoSimplify()
	{
		UWE.Utils.EnsureMinSize("simplifyOld2New", ref simplifyOld2New, numVerts);
		SimplifyMeshPlugin.SimplifyMesh(settings.simplify, simplifyVertices, simplifyFixed, ref numVerts, simplifyTris, ref numTris, simplifyOld2New);
		UWE.Utils.EnsureMinSize("simp2wsVert", ref simp2wsVert, numVerts);
		for (int i = 0; i < ws.verts.Count; i++)
		{
			VoxelandChunk.VoxelandVert voxelandVert = ws.verts[i];
			if (voxelandVert.layerVertIndex != -1)
			{
				int num = i;
				int num2 = simplifyOld2New[voxelandVert.layerVertIndex];
				if (num2 != -1)
				{
					simp2wsVert[num2] = num;
				}
			}
		}
	}

	public void ComputerLayersPhase1(IVoxelandChunk chunk, MeshBufferPools pools)
	{
		this.pools = pools;
		_ = chunk.usedTypes;
		UWE.Utils.EnsureMinSize("simpTriInLayer", ref simpTriInLayer, numTris);
		UWE.Utils.EnsureMinSize("simp2layerVert", ref simp2layerVert, numVerts);
		UWE.Utils.EnsureMinSize("builtLayers", ref builtLayers, chunk.usedTypes.Count);
		numBuiltLayers = chunk.usedTypes.Count;
		for (int i = 0; i < chunk.usedTypes.Count; i++)
		{
			int num = 0;
			int num2 = 0;
			Array.Clear(simpTriInLayer, 0, simpTriInLayer.Length);
			Array.Clear(simp2layerVert, 0, simp2layerVert.Length);
			for (int j = 0; j < numTris; j++)
			{
				SimplifyMeshPlugin.Face face = simplifyTris[j];
				bool flag = false;
				if (i == 0)
				{
					flag = true;
				}
				else
				{
					for (int k = 0; k < 3; k++)
					{
						if (ws.verts[simp2wsVert[face.GetVert(k)]].GetCachedBlendWeight(i) > 0f)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					continue;
				}
				simpTriInLayer[j] = true;
				num2++;
				for (int l = 0; l < 3; l++)
				{
					if (simp2layerVert[face.GetVert(l)] == 0)
					{
						simp2layerVert[face.GetVert(l)] = num + 1;
						num++;
					}
				}
			}
			MeshBuffer meshBuffer = new MeshBuffer();
			builtLayers[i] = meshBuffer;
			if (num2 == 0)
			{
				meshBuffer.Clear();
				continue;
			}
			meshBuffer.Acquire(pools, num, num2 * 3, MeshBuffer.MeshBufferType.Layer);
			for (int m = 0; m < numVerts; m++)
			{
				if (simp2layerVert[m] != 0)
				{
					int index = simp2layerVert[m] - 1;
					int index2 = simp2wsVert[m];
					VoxelandChunk.VoxelandVert voxelandVert = ws.verts[index2];
					TerrainLayerVertex value = default(TerrainLayerVertex);
					value.position = simplifyVertices[m];
					value.normal = voxelandVert.normal;
					value.uv.Set((i == 0) ? 1f : voxelandVert.GetCachedBlendWeight(i), voxelandVert.gloss);
					meshBuffer.layerVertices[index] = value;
				}
			}
			int num3 = 0;
			for (int n = 0; n < numTris; n++)
			{
				if (simpTriInLayer[n])
				{
					SimplifyMeshPlugin.Face face2 = simplifyTris[n];
					for (int num4 = 0; num4 < 3; num4++)
					{
						int vert = face2.GetVert(num4);
						int num5 = simp2layerVert[vert] - 1;
						meshBuffer.triangles[3 * num3 + num4] = (ushort)num5;
					}
					num3++;
				}
			}
		}
		ws = null;
	}

	private void ComputeLayersPhase1NoSimplify(IVoxelandChunk chunk, MeshBufferPools pools)
	{
		ws = chunk.ws;
		int[,] array = (settings.useLowMesh ? faceLoTri2Verts : faceHiTri2Verts);
		if (!settings.useLowMesh)
		{
			_ = hiFaceVerts;
		}
		else
		{
			_ = loFaceVerts;
		}
		int length = array.GetLength(0);
		this.pools = pools;
		int count = chunk.usedTypes.Count;
		int count2 = ws.verts.Count;
		int count3 = ws.visibleFaces.Count;
		UWE.Utils.EnsureMinSize("builtLayers", ref builtLayers, count);
		numBuiltLayers = count;
		for (int i = 0; i < count; i++)
		{
			int num = 0;
			int num2 = 0;
			for (int j = 0; j < count2; j++)
			{
				ws.verts[j].layerVertIndex = -1;
			}
			MeshBuffer meshBuffer = null;
			builtLayers[i] = null;
			for (int k = 0; k < count3; k++)
			{
				VoxelandChunk.VoxelandVert[] verts = ws.visibleFaces[k].verts;
				for (int l = 0; l < length; l++)
				{
					bool flag = false;
					if (i == 0)
					{
						flag = true;
					}
					else
					{
						for (int m = 0; m < 3; m++)
						{
							if (verts[array[l, m]].GetCachedBlendWeight(i) > 0f)
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						continue;
					}
					if (meshBuffer == null)
					{
						builtLayers[i] = new MeshBuffer();
						builtLayers[i].Acquire(pools, count2, count3 * length * 3, MeshBuffer.MeshBufferType.Layer);
						meshBuffer = builtLayers[i];
					}
					for (int n = 0; n < 3; n++)
					{
						int num3 = array[l, n];
						VoxelandChunk.VoxelandVert voxelandVert = verts[num3];
						if (voxelandVert.layerVertIndex == -1)
						{
							voxelandVert.layerVertIndex = num++;
							TerrainLayerVertex value = default(TerrainLayerVertex);
							value.position = voxelandVert.pos;
							value.normal = voxelandVert.normal;
							value.uv.Set((i == 0) ? 1f : voxelandVert.GetCachedBlendWeight(i), voxelandVert.gloss);
							meshBuffer.layerVertices[voxelandVert.layerVertIndex] = value;
						}
						meshBuffer.triangles[3 * num2 + n] = (ushort)voxelandVert.layerVertIndex;
					}
					num2++;
				}
			}
			if (meshBuffer == null)
			{
				continue;
			}
			meshBuffer.numVerts = num;
			meshBuffer.numTris = num2 * 3;
			if (num2 == 0)
			{
				if (debugUseMeshBuffers)
				{
					meshBuffer.Return();
				}
				else
				{
					meshBuffer.Clear();
				}
			}
		}
		ws = null;
	}

	public void DumpObj()
	{
		SimplifyMeshPlugin.DumpObj(ref simplifyVertices[0], ref simplifyFixed[0], numVerts, ref simplifyTris[0], numTris);
	}

	private TerrainChunkPiece GetLayerObj(TerrainPoolManager terrainPoolManager, IVoxelandChunk2 chunk, ref MeshFilter filter, ref MeshRenderer render)
	{
		TerrainChunkPiece terrainChunkPiece = null;
		if (terrainPoolManager != null)
		{
			terrainChunkPiece = terrainPoolManager.Get(TerrainChunkPieceType.Layer, chunk.transform);
			terrainPoolManager.SetTerrainPiecePositionAndScale(chunk.transform, terrainChunkPiece);
		}
		else
		{
			GameObject gameObject = new GameObject("ChunkLayer");
			terrainChunkPiece = gameObject.AddComponent<TerrainChunkPiece>();
			terrainChunkPiece.pieceType = TerrainChunkPieceType.Layer;
			terrainChunkPiece.meshFilter = gameObject.AddComponent<MeshFilter>();
			terrainChunkPiece.meshRenderer = gameObject.AddComponent<MeshRenderer>();
			gameObject.transform.SetParent(chunk.transform, worldPositionStays: false);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			terrainChunkPiece.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		}
		render = terrainChunkPiece.meshRenderer;
		filter = terrainChunkPiece.meshFilter;
		chunk.hiFilters.Add(filter);
		chunk.hiRenders.Add(render);
		chunk.chunkPieces.Add(terrainChunkPiece);
		return terrainChunkPiece;
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public void BuildLayerObjects(VoxelandChunk chunk, bool castShadows, int addSortingValue, TerrainPoolManager terrainPoolManager)
	{
		BuildLayerObjects(chunk, chunk, castShadows, addSortingValue, terrainPoolManager);
	}

	public void BuildLayerObjects(IVoxelandChunk2 chunk, IVoxelandChunkInfo info, bool castShadows, int addSortingValue, TerrainPoolManager terrainPoolManager)
	{
		for (int i = 0; i < info.usedTypes.Count; i++)
		{
			MeshFilter filter = null;
			MeshRenderer render = null;
			TerrainChunkPiece piece = null;
			MeshBuffer meshBuffer = builtLayers[i];
			if (meshBuffer == null || meshBuffer.layerVertices == null)
			{
				continue;
			}
			if (i >= chunk.hiFilters.Count)
			{
				piece = GetLayerObj(terrainPoolManager, chunk, ref filter, ref render);
			}
			else
			{
				filter = chunk.hiFilters[i];
				render = chunk.hiRenders[i];
			}
			filter.sharedMesh = terrainPoolManager.GetMeshForPiece(piece);
			meshBuffer.Upload(filter.sharedMesh);
			meshBuffer.Return();
			if (i == 0)
			{
				render.sortingOrder = 0;
				render.shadowCastingMode = (castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
				VoxelandChunk.TypeUse typeUse = info.usedTypes[i];
				VoxelandBlockType voxelandBlockType = info.land.types[typeUse.num];
				if (voxelandBlockType == null)
				{
					Debug.LogFormat("No block type at index {0} - using a fallback type instead.", typeUse.num.ToString());
					voxelandBlockType = VoxelandChunk.GetFallbackBlockType(info.land.types);
				}
				render.sharedMaterial = voxelandBlockType.opaqueMaterial;
			}
			else
			{
				render.sortingOrder = i + addSortingValue;
				render.shadowCastingMode = ShadowCastingMode.Off;
				VoxelandBlockType voxelandBlockType2 = info.land.types[info.usedTypes[i].num];
				render.sharedMaterial = (debugForceAlphaTest ? voxelandBlockType2.alphaTestMat : voxelandBlockType2.material);
			}
			render.enabled = false;
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public void Build(IVoxelandChunk chunk, MeshBufferPools pools, bool debug = false)
	{
		Reset();
		if (settings.skipSimplify)
		{
			ComputeLayersPhase1NoSimplify(chunk, pools);
		}
		else
		{
			PrepareBuffers(chunk.ws);
			long num = DateTime.Now.Ticks % 10000;
			if (debug)
			{
				Debug.LogFormat("{0} Before #verts/tris: {1}/{2}", num, numVerts, numTris);
			}
			DoSimplify();
			if (debug)
			{
				Debug.LogFormat("{0} After #verts/tris: {1}/{2}", num, numVerts, numTris);
			}
			ComputerLayersPhase1(chunk, pools);
		}
		MeshBuffer[] array = builtLayers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.RecalculateBoundsThreaded();
		}
	}
}
