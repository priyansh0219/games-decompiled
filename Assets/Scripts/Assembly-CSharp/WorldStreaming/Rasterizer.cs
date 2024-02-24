using UnityEngine;

namespace WorldStreaming
{
	public static class Rasterizer
	{
		public static void Rasterize(BatchOctreesStreamer streamer, Array3<byte> typesGrid, Array3<byte> densityGrid, Int3 size, Int3 origin, Int3 chunkId, int downsamples)
		{
			Int3 a = origin + (size << downsamples) - 1;
			Int3 min = Int3.FloorDiv(origin, 32);
			Int3 max = Int3.FloorDiv(a, 32);
			foreach (Int3 item in Int3.MinMax(min, max))
			{
				Octree octree = streamer.GetOctree(item);
				if (octree == null)
				{
					Debug.LogWarningFormat("Rasterize octree {0} (batch {1}, origin {2}, size {3}, chunk {4}, downsamples {5}) not loaded", item, item / 5, origin, size, chunkId, downsamples);
				}
				else
				{
					octree.RasterizeNative(0, typesGrid, densityGrid, size, origin >> downsamples, item * 32 >> downsamples, 32 >> downsamples + 1);
				}
			}
		}

		private static void DebugRasterization(Int3 size, Array3<byte> typesGrid, Array3<byte> densityGrid, Int3 w, int D)
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Object.Destroy(gameObject.GetComponent<Collider>());
			Material material = new Material(Shader.Find("Standard"));
			material.color = Color.red;
			Vector3 vector = new Vector3(-2048f, -3040f, -2048f) + VoxelandChunk.half3;
			Transform transform = new GameObject().transform;
			transform.localPosition = vector + (Vector3)(w >> D);
			transform.localScale = (Vector3)new Int3(1 << D);
			foreach (Int3 item in Int3.Range(size))
			{
				byte b = typesGrid.Get(item);
				byte b2 = densityGrid.Get(item);
				if (b != 0)
				{
					float num = 1f;
					if (b2 != 0)
					{
						num = VoxelandData.OctNode.DecodeNearDensity(b2);
					}
					GameObject gameObject2 = Object.Instantiate(gameObject, transform);
					gameObject2.transform.localPosition = (Vector3)item;
					gameObject2.name = $"Type {b}, density {b2}";
					if (num < 0f)
					{
						num = 0f - num;
						gameObject2.GetComponent<MeshRenderer>().sharedMaterial = material;
					}
					gameObject2.transform.localScale = new Vector3(num, num, num);
				}
			}
			Object.Destroy(gameObject);
		}
	}
}
