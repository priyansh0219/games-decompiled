using UnityEngine;

public class TerrainChunkPieceLayer : TerrainChunkPiece
{
	public MonoBehaviour cullingOccluder;

	public override void SetupForMeshPooling()
	{
		base.SetupForMeshPooling();
		if (meshPoolingEnabled)
		{
			meshFilter.sharedMesh = new Mesh();
		}
	}

	public override void OnReturnToPool()
	{
		base.OnReturnToPool();
		if (meshPoolingEnabled)
		{
			meshFilter.sharedMesh.Clear(keepVertexLayout: false);
			meshFilter.sharedMesh.UploadMeshData(markNoLongerReadable: false);
		}
		meshRenderer.enabled = false;
		if (cullingOccluder != null)
		{
			cullingOccluder.enabled = false;
		}
	}

	private void OnDestroy()
	{
		Object.Destroy(meshFilter.sharedMesh);
	}

	public override Mesh Mesh()
	{
		return meshFilter.sharedMesh;
	}
}
