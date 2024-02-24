using System;
using UnityEngine;

public class TerrainChunkPiece : MonoBehaviour
{
	public TerrainChunkPieceType pieceType;

	public MeshRenderer meshRenderer;

	public MeshFilter meshFilter;

	public MeshCollider meshCollider;

	[NonSerialized]
	public bool meshPoolingEnabled;

	public virtual Mesh Mesh()
	{
		return null;
	}

	public virtual void SetupForMeshPooling()
	{
	}

	public virtual void OnReturnToPool()
	{
	}
}
