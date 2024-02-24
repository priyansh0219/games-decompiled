using UnityEngine;
using UnityEngine.Rendering;

namespace UWE
{
	public class MeshBuffer
	{
		public enum MeshBufferType
		{
			Layer = 0,
			Grass = 1,
			Collider = 2
		}

		private MeshBufferPools pools;

		public int numVerts;

		public int numTris;

		public Bounds bounds;

		public bool isDegenerateCollider;

		public IAlloc<TerrainLayerVertex> layerVertices { get; private set; }

		public IAlloc<TerrainGrassVertex> grassVertices { get; private set; }

		public IAlloc<TerrainColliderVertex> colliderVertices { get; private set; }

		public IAlloc<ushort> triangles { get; private set; }

		public void Clear(bool clearCaches = false)
		{
			pools = null;
			numVerts = 0;
			numTris = 0;
			layerVertices = null;
			grassVertices = null;
			colliderVertices = null;
			triangles = null;
		}

		public void Clamp(int maxVerts, int maxTris)
		{
			numVerts = Mathf.Min(numVerts, maxVerts);
			numTris = Mathf.Min(numTris, maxTris);
		}

		public void Acquire(MeshBufferPools pools, int numVerts, int numTris, MeshBufferType type)
		{
			this.pools = pools;
			this.numVerts = numVerts;
			this.numTris = numTris;
			switch (type)
			{
			case MeshBufferType.Layer:
				layerVertices = pools.terrainLayerVertices.Allocate(numVerts);
				break;
			case MeshBufferType.Grass:
				grassVertices = pools.terrainGrassVertices.Allocate(numVerts);
				break;
			case MeshBufferType.Collider:
				colliderVertices = pools.terrainColliderVertices.Allocate(numVerts);
				break;
			}
			triangles = pools.indices.Allocate(numTris);
		}

		public void Return()
		{
			if (pools != null)
			{
				if (layerVertices != null)
				{
					pools.terrainLayerVertices.Free(layerVertices);
				}
				if (grassVertices != null)
				{
					pools.terrainGrassVertices.Free(grassVertices);
				}
				if (colliderVertices != null)
				{
					pools.terrainColliderVertices.Free(colliderVertices);
				}
				pools.indices.Free(triangles);
				Clear();
			}
		}

		public void Upload(Mesh m, bool keepVertexLayout = true)
		{
			if (numVerts != 0)
			{
				MeshUpdateFlags flags = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds;
				bool flag = true;
				if (layerVertices != null)
				{
					m.SetVertexBufferParams(numVerts, TerrainLayerVertex.layout);
					m.SetVertexBufferData(pools.terrainLayerVertices.buffer, layerVertices.Offset, 0, numVerts, 0, flags);
				}
				else if (grassVertices != null)
				{
					m.SetVertexBufferParams(numVerts, TerrainGrassVertex.layout);
					m.SetVertexBufferData(pools.terrainGrassVertices.buffer, grassVertices.Offset, 0, numVerts, 0, flags);
				}
				else if (colliderVertices != null)
				{
					m.SetVertexBufferParams(numVerts, TerrainColliderVertex.layout);
					m.SetVertexBufferData(pools.terrainColliderVertices.buffer, colliderVertices.Offset, 0, numVerts, 0, flags);
				}
				else
				{
					flag = false;
					Debug.LogError("Trying to upload an empty mesh");
				}
				if (flag)
				{
					m.SetIndexBufferParams(numTris, IndexFormat.UInt16);
					m.SetIndexBufferData(pools.indices.buffer, triangles.Offset, 0, numTris, flags);
					SubMeshDescriptor desc = new SubMeshDescriptor(0, numTris);
					desc.vertexCount = numVerts;
					desc.baseVertex = 0;
					desc.firstVertex = 0;
					m.subMeshCount = 1;
					m.SetSubMesh(0, desc, flags);
					m.bounds = bounds;
					m.MarkModified();
				}
			}
		}

		public void RecalculateBoundsThreaded()
		{
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			for (int i = 0; i < numVerts; i++)
			{
				Vector3 vector = Vector3.zero;
				if (layerVertices != null)
				{
					vector = layerVertices[i].position;
				}
				else if (grassVertices != null)
				{
					vector = grassVertices[i].position;
				}
				else if (colliderVertices != null)
				{
					vector = colliderVertices[i].position;
				}
				if (!IsNaNOrInfinity(vector.x))
				{
					min.x = Mathf.Min(min.x, vector.x);
					max.x = Mathf.Max(max.x, vector.x);
				}
				if (!IsNaNOrInfinity(vector.y))
				{
					min.y = Mathf.Min(min.y, vector.y);
					max.y = Mathf.Max(max.y, vector.y);
				}
				if (!IsNaNOrInfinity(vector.z))
				{
					min.z = Mathf.Min(min.z, vector.z);
					max.z = Mathf.Max(max.z, vector.z);
				}
			}
			bounds.min = min;
			bounds.max = max;
		}

		private static bool IsNaNOrInfinity(float x)
		{
			if (!float.IsNaN(x))
			{
				return float.IsInfinity(x);
			}
			return true;
		}

		public void DetermineIfDegenerateThreaded()
		{
			isDegenerateCollider = true;
			for (int i = 0; i < numTris; i += 3)
			{
				int index = triangles[i];
				int index2 = triangles[i + 1];
				int index3 = triangles[i + 2];
				Vector3 position = colliderVertices[index].position;
				Vector3 position2 = colliderVertices[index2].position;
				Vector3 position3 = colliderVertices[index3].position;
				float num = Vector3.Distance(position, position2);
				float num2 = Vector3.Distance(position2, position3);
				float num3 = Vector3.Distance(position, position3);
				if (num + num2 > num3 && num + num3 > num2 && num2 + num3 > num)
				{
					isDegenerateCollider = false;
					break;
				}
			}
		}
	}
}
