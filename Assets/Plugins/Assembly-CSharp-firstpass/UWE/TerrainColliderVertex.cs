using UnityEngine;
using UnityEngine.Rendering;

namespace UWE
{
	public struct TerrainColliderVertex
	{
		public static readonly VertexAttributeDescriptor[] layout = new VertexAttributeDescriptor[1]
		{
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0)
		};

		public const int size = 12;

		public Vector3 position;
	}
}
