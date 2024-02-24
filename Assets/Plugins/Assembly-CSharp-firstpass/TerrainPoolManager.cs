using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPoolManager : MonoBehaviour
{
	public TerrainChunkPiece chunkPrefab;

	public TerrainChunkPiece chunkLayerPrefab;

	public TerrainChunkPiece chunkGrassPrefab;

	public TerrainChunkPiece chunkColliderPrefab;

	private readonly Stack<TerrainChunkPiece> chunkPool = new Stack<TerrainChunkPiece>();

	private readonly Stack<TerrainChunkPiece> chunkLayerPool = new Stack<TerrainChunkPiece>();

	private readonly Stack<TerrainChunkPiece> chunkGrassPool = new Stack<TerrainChunkPiece>();

	private readonly Stack<TerrainChunkPiece> chunkColliderPool = new Stack<TerrainChunkPiece>();

	public bool useFlatHierarchy = true;

	public bool poolingEnabled = true;

	public bool meshPoolingEnabled = true;

	public void Start()
	{
		InterpretCommandLineDebugFlags();
		if (!poolingEnabled)
		{
			useFlatHierarchy = false;
			meshPoolingEnabled = false;
			return;
		}
		Warm(TerrainChunkPieceType.Root, 512);
		Warm(TerrainChunkPieceType.Layer, 3000);
		Warm(TerrainChunkPieceType.Grass, 2000);
		Warm(TerrainChunkPieceType.Collider, 300);
	}

	public TerrainChunkPiece Get(TerrainChunkPieceType pieceType, Transform parent)
	{
		Stack<TerrainChunkPiece> stack = PoolForPieceType(pieceType);
		if (stack.Count == 0 || !poolingEnabled)
		{
			return CreateNewPiece(pieceType, parent);
		}
		TerrainChunkPiece terrainChunkPiece = stack.Pop();
		SetParent(terrainChunkPiece, parent);
		return terrainChunkPiece;
	}

	private TerrainChunkPiece CreateNewPiece(TerrainChunkPieceType pieceType, Transform parent)
	{
		TerrainChunkPiece terrainChunkPiece = UnityEngine.Object.Instantiate(PrefabForPieceType(pieceType));
		SetupPieceForMeshPooling(terrainChunkPiece);
		SetParent(terrainChunkPiece, parent);
		if (pieceType == TerrainChunkPieceType.Collider)
		{
			terrainChunkPiece.gameObject.SetActive(value: false);
		}
		return terrainChunkPiece;
	}

	private void SetupPieceForMeshPooling(TerrainChunkPiece piece)
	{
		piece.meshPoolingEnabled = meshPoolingEnabled;
		if (meshPoolingEnabled)
		{
			piece.SetupForMeshPooling();
		}
	}

	public Mesh GetMeshForPiece(TerrainChunkPiece piece)
	{
		if (!meshPoolingEnabled)
		{
			return new Mesh();
		}
		return piece.Mesh();
	}

	private void SetParent(TerrainChunkPiece piece, Transform parent)
	{
		if (!useFlatHierarchy)
		{
			piece.transform.SetParent(parent);
		}
	}

	public void Return(TerrainChunkPiece piece)
	{
		if (poolingEnabled)
		{
			piece.OnReturnToPool();
			SetParent(piece, base.transform);
			PoolForPieceType(piece.pieceType).Push(piece);
		}
		else
		{
			UnityEngine.Object.Destroy(piece.gameObject);
		}
	}

	public void SetTerrainPiecePositionAndScale(Transform chunkRoot, TerrainChunkPiece chunkPiece)
	{
		if (useFlatHierarchy)
		{
			chunkPiece.transform.localPosition = chunkRoot.position;
			chunkPiece.transform.localScale = chunkRoot.localScale;
		}
		else
		{
			chunkPiece.transform.localPosition = Vector3.zero;
			chunkPiece.transform.localScale = Vector3.one;
		}
	}

	private Stack<TerrainChunkPiece> PoolForPieceType(TerrainChunkPieceType pieceType)
	{
		switch (pieceType)
		{
		case TerrainChunkPieceType.Root:
			return chunkPool;
		case TerrainChunkPieceType.Layer:
			return chunkLayerPool;
		case TerrainChunkPieceType.Grass:
			return chunkGrassPool;
		case TerrainChunkPieceType.Collider:
			return chunkColliderPool;
		default:
			return null;
		}
	}

	private TerrainChunkPiece PrefabForPieceType(TerrainChunkPieceType pieceType)
	{
		switch (pieceType)
		{
		case TerrainChunkPieceType.Root:
			return chunkPrefab;
		case TerrainChunkPieceType.Layer:
			return chunkLayerPrefab;
		case TerrainChunkPieceType.Grass:
			return chunkGrassPrefab;
		case TerrainChunkPieceType.Collider:
			return chunkColliderPrefab;
		default:
			return null;
		}
	}

	private void Warm(TerrainChunkPieceType pieceType, int initialSize)
	{
		Stack<TerrainChunkPiece> stack = PoolForPieceType(pieceType);
		for (int i = 0; i < initialSize; i++)
		{
			TerrainChunkPiece item = CreateNewPiece(pieceType, base.transform);
			stack.Push(item);
		}
	}

	private static bool GetCommandLineFlag(string argumentName)
	{
		if (Array.IndexOf(Environment.GetCommandLineArgs(), argumentName) >= 0)
		{
			return true;
		}
		return false;
	}

	private void InterpretCommandLineDebugFlags()
	{
		if (GetCommandLineFlag("-terrain_pooling_disabled"))
		{
			poolingEnabled = false;
		}
		if (GetCommandLineFlag("-terrain_flathierarchy_disabled"))
		{
			useFlatHierarchy = false;
		}
		if (GetCommandLineFlag("-terrain_meshpooling_disabled"))
		{
			meshPoolingEnabled = false;
		}
	}
}
