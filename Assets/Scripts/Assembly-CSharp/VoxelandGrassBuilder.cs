using System;
using System.Collections.Generic;
using System.Linq;
using UWE;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class VoxelandGrassBuilder : IEstimateBytes
{
	private enum State
	{
		Init = 0,
		Setup = 1,
		Meshed = 2
	}

	[Serializable]
	public class Settings
	{
		public float reduction;

		public int maxVerts = 10000;

		public int maxTris = 10000;
	}

	private readonly List<MeshBuffer> builtMeshes = new List<MeshBuffer>();

	private readonly List<VoxelandBlockType> types = new List<VoxelandBlockType>();

	private MeshBufferPools pools;

	private State state;

	public bool inUse { get; set; }

	public long EstimateBytes()
	{
		return 52L;
	}

	public void Reset(MeshBufferPools pools)
	{
		this.pools = pools;
		builtMeshes.Clear();
		types.Clear();
		state = State.Setup;
	}

	public void CreateMeshData(IVoxelandChunk chunk, Settings settings)
	{
		int num = 0;
		int num2 = 0;
		int count = chunk.usedTypes.Count;
		for (int i = 0; i < count; i++)
		{
			byte num3 = chunk.usedTypes[i].num;
			VoxelandBlockType voxelandBlockType = chunk.land.types[num3];
			if (!voxelandBlockType.hasGrassAbove || !voxelandBlockType.hasGrassAbove)
			{
				continue;
			}
			VoxelandBlockType voxelandBlockType2 = voxelandBlockType;
			int randSeed = num3;
			if (voxelandBlockType2.grassVerts == null)
			{
				continue;
			}
			int num4 = Mathf.Min(settings.maxVerts - num, 65535);
			int num5 = System.Math.Min(val2: (settings.maxTris * 3 - num2) / voxelandBlockType2.grassTris.Length, val1: num4 / voxelandBlockType2.grassVerts.Length);
			num5 = (int)((float)num5 * 0.8f);
			int num6 = chunk.EnumerateGrass(voxelandBlockType2, num3, randSeed, settings.reduction).Count();
			if (num6 == 0)
			{
				continue;
			}
			float num7 = settings.reduction;
			if (num6 > num5)
			{
				num7 = Mathf.Lerp(settings.reduction, 1f, 1f - (float)num5 / (float)num6);
				num6 = num5;
			}
			int num8 = voxelandBlockType2.grassVerts.Length * num6;
			int num9 = voxelandBlockType2.grassTris.Length * num6;
			if (num8 == 0 || num9 == 0)
			{
				continue;
			}
			MeshBuffer meshBuffer = new MeshBuffer();
			meshBuffer.Acquire(pools, num8, num9, MeshBuffer.MeshBufferType.Grass);
			if (meshBuffer.grassVertices == null)
			{
				Debug.LogFormat("Failed to get grass buffer for {0} verts", num8);
				continue;
			}
			builtMeshes.Add(meshBuffer);
			types.Add(voxelandBlockType2);
			int num10 = 0;
			int maxTris = 0;
			int num11 = 0;
			Unity.Mathematics.Random rng = VoxelandMisc.CreateRandom(chunk.offsetX * 9999 + chunk.offsetY * 999 + chunk.offsetZ * 99);
			foreach (VoxelandChunk.GrassPos item in chunk.EnumerateGrass(voxelandBlockType2, num3, randSeed, num7))
			{
				if (num11 >= num5)
				{
					break;
				}
				item.ComputeTransform(ref rng, voxelandBlockType2);
				int num12 = num10;
				for (int j = 0; j < voxelandBlockType2.grassVerts.Length; j++)
				{
					TerrainGrassVertex value = default(TerrainGrassVertex);
					value.position = item.csOrigin + item.quat * (item.scale * voxelandBlockType2.grassVerts[j]);
					value.normal = item.quat * voxelandBlockType2.grassNormals[j];
					value.tangent = item.quat * voxelandBlockType2.grassTangents[j];
					value.uv = voxelandBlockType2.grassUVs[j];
					float num13 = Vector3.Dot(value.position - item.csOrigin, item.faceNormal);
					value.color = new Color((float)(int)rng.NextByte() / 255f, (float)(int)rng.NextByte() / 255f, (float)(int)rng.NextByte() / 255f, Mathf.Clamp01(num13 / 5f));
					meshBuffer.grassVertices[num10] = value;
					num10++;
				}
				for (int k = 0; k < voxelandBlockType2.grassTris.Length; k++)
				{
					int num14 = num12 + voxelandBlockType2.grassTris[k];
					meshBuffer.triangles[maxTris++] = (ushort)num14;
				}
				num11++;
			}
			meshBuffer.Clamp(num10, maxTris);
			num += num8;
			num2 += num9;
		}
		foreach (MeshBuffer builtMesh in builtMeshes)
		{
			builtMesh?.RecalculateBoundsThreaded();
		}
		state = State.Meshed;
	}

	private TerrainChunkPiece GetGrassObj(IVoxelandChunk2 chunk, TerrainPoolManager terrainPoolManager)
	{
		if (terrainPoolManager != null)
		{
			TerrainChunkPiece terrainChunkPiece = terrainPoolManager.Get(TerrainChunkPieceType.Grass, chunk.transform);
			terrainPoolManager.SetTerrainPiecePositionAndScale(chunk.transform, terrainChunkPiece);
			return terrainChunkPiece;
		}
		GameObject gameObject = new GameObject("ChunkGrass");
		TerrainChunkPiece terrainChunkPiece2 = gameObject.AddComponent<TerrainChunkPiece>();
		terrainChunkPiece2.pieceType = TerrainChunkPieceType.Grass;
		terrainChunkPiece2.meshFilter = gameObject.AddComponent<MeshFilter>();
		terrainChunkPiece2.meshRenderer = gameObject.AddComponent<MeshRenderer>();
		gameObject.transform.SetParent(chunk.transform, worldPositionStays: false);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		terrainChunkPiece2.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		return terrainChunkPiece2;
	}

	public void CreateUnityMeshes(IVoxelandChunk2 chunk, TerrainPoolManager terrainPoolManager)
	{
		int num = 0;
		for (num = 0; num < builtMeshes.Count; num++)
		{
			TerrainChunkPiece grassObj = GetGrassObj(chunk, terrainPoolManager);
			chunk.grassFilters.Add(grassObj.meshFilter);
			chunk.grassRenders.Add(grassObj.meshRenderer);
			chunk.chunkPieces.Add(grassObj);
			MeshFilter meshFilter = chunk.grassFilters[num];
			meshFilter.gameObject.SetActive(value: true);
			MeshRenderer meshRenderer = chunk.grassRenders[num];
			VoxelandBlockType voxelandBlockType = types[num];
			meshFilter.sharedMesh = terrainPoolManager.GetMeshForPiece(grassObj);
			meshRenderer.sharedMaterial = voxelandBlockType.grassMaterial;
			MeshBuffer meshBuffer = builtMeshes[num];
			meshBuffer.Upload(meshFilter.sharedMesh);
			meshBuffer.Return();
		}
		state = State.Init;
	}
}
