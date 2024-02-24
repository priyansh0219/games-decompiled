using UnityEngine;

public class TerrainChunkPieceCollider : TerrainChunkPiece
{
	public override void SetupForMeshPooling()
	{
		base.SetupForMeshPooling();
		if (meshPoolingEnabled)
		{
			meshCollider.sharedMesh = new Mesh();
		}
	}

	public override void OnReturnToPool()
	{
		base.OnReturnToPool();
		if (meshPoolingEnabled)
		{
			meshCollider.sharedMesh.Clear(keepVertexLayout: false);
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		Object.Destroy(meshCollider.sharedMesh);
	}

	public override Mesh Mesh()
	{
		return meshCollider.sharedMesh;
	}
}
