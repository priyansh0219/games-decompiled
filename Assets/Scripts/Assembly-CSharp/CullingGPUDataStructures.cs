using UnityEngine;

public class CullingGPUDataStructures
{
	public struct RendererBoundingBox
	{
		public Vector3 min;

		public Vector3 max;
	}

	public struct OcclusionResult
	{
		public float visible;
	}

	public const int boundingBoxesBufferStride = 24;

	public const int visibilityResultsBufferStride = 4;
}
