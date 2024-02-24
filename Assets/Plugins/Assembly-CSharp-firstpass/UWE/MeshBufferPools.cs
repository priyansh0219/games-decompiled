using UnityEngine;

namespace UWE
{
	public class MeshBufferPools : IEstimateBytes
	{
		public NativeLinearArrayHeap<TerrainLayerVertex> terrainLayerVertices;

		public NativeLinearArrayHeap<TerrainGrassVertex> terrainGrassVertices;

		public NativeLinearArrayHeap<TerrainColliderVertex> terrainColliderVertices;

		public NativeLinearArrayHeap<ushort> indices;

		public const int defaultLayerVertexMax = 655360;

		public const int defaultGrassVertexMax = 65536;

		public const int defaultColliderVertexMax = 32768;

		public const int defaultIndexMax = 3670016;

		public MeshBufferPools(int layerVertexMax = 655360, int grassVertexMax = 65536, int colliderVertexMax = 32768, int indexMax = 3670016)
		{
			terrainLayerVertices = new NativeLinearArrayHeap<TerrainLayerVertex>(32, layerVertexMax);
			terrainGrassVertices = new NativeLinearArrayHeap<TerrainGrassVertex>(64, grassVertexMax);
			terrainColliderVertices = new NativeLinearArrayHeap<TerrainColliderVertex>(12, colliderVertexMax);
			indices = new NativeLinearArrayHeap<ushort>(2, indexMax);
		}

		public bool TryReset()
		{
			if (terrainLayerVertices.Outstanding > 0 || terrainGrassVertices.Outstanding > 0 || terrainColliderVertices.Outstanding > 0 || indices.Outstanding > 0)
			{
				return false;
			}
			terrainLayerVertices.Reset();
			terrainGrassVertices.Reset();
			terrainColliderVertices.Reset();
			indices.Reset();
			return true;
		}

		public void Dispose()
		{
			terrainLayerVertices.Dispose();
			terrainGrassVertices.Dispose();
			terrainColliderVertices.Dispose();
			indices.Dispose();
		}

		public long EstimateBytes()
		{
			return terrainLayerVertices.EstimateBytes() + terrainGrassVertices.EstimateBytes() + terrainColliderVertices.EstimateBytes() + indices.EstimateBytes();
		}

		public static void LayoutGUI(MeshBufferPools[] pools)
		{
			long num = 0L;
			long num2 = 0L;
			long num3 = 0L;
			long num4 = 0L;
			foreach (MeshBufferPools meshBufferPools in pools)
			{
				num += meshBufferPools.terrainLayerVertices.EstimateBytes();
				num2 += meshBufferPools.terrainGrassVertices.EstimateBytes();
				num3 += meshBufferPools.terrainColliderVertices.EstimateBytes();
				num4 += meshBufferPools.indices.EstimateBytes();
			}
			GUILayout.Label($"Layer Vertices:  {(float)num * 9.536743E-07f:0.0} MB");
			GUILayout.Label($"Grass Vertices:  {(float)num2 * 9.536743E-07f:0.0} MB");
			GUILayout.Label($"Collider Vertices:  {(float)num3 * 9.536743E-07f:0.0} MB");
			GUILayout.Label($"Int pools: {(float)num4 * 9.536743E-07f:0.0} MB");
		}
	}
}
