using UnityEngine;
using UnityEngine.Rendering;

namespace UWE
{
	public struct TerrainGrassVertex
	{
		public static readonly VertexAttributeDescriptor[] layout = new VertexAttributeDescriptor[5]
		{
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
			new VertexAttributeDescriptor(VertexAttribute.Normal),
			new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
			new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
		};

		public const int size = 64;

		public Vector3 position;

		public Vector3 normal;

		public Vector4 tangent;

		public Color color;

		public Vector2 uv;
	}
}
