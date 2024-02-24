using UnityEngine;
using UnityEngine.Rendering;

namespace UWE
{
	public struct TerrainLayerVertex
	{
		public static readonly VertexAttributeDescriptor[] layout = new VertexAttributeDescriptor[3]
		{
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
			new VertexAttributeDescriptor(VertexAttribute.Normal),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
		};

		public const int size = 32;

		public Vector3 position;

		public Vector3 normal;

		public Vector2 uv;
	}
}
