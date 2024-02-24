using System.Collections.Generic;
using UnityEngine;

public interface IVoxelandChunk2
{
	List<MeshFilter> hiFilters { get; }

	List<MeshRenderer> hiRenders { get; }

	Transform transform { get; }

	GameObject gameObject { get; }

	List<MeshFilter> grassFilters { get; }

	List<MeshRenderer> grassRenders { get; }

	List<TerrainChunkPiece> chunkPieces { get; }

	MeshCollider collision { get; set; }

	MeshCollider EnsureCollision();
}
