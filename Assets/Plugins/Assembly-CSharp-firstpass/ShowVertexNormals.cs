using UnityEngine;

public class ShowVertexNormals : MonoBehaviour
{
	public bool Normalize;

	public float Scale = 1f;

	private void OnDrawGizmosSelected()
	{
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
		foreach (MeshFilter obj in componentsInChildren)
		{
			Transform transform = obj.transform;
			Mesh sharedMesh = obj.sharedMesh;
			if (sharedMesh == null)
			{
				continue;
			}
			Vector3[] vertices = sharedMesh.vertices;
			Vector3[] normals = sharedMesh.normals;
			Gizmos.color = Color.magenta;
			for (int j = 0; j < vertices.Length && j < normals.Length; j++)
			{
				Vector3 position = vertices[j];
				Vector3 direction = normals[j];
				if (Normalize)
				{
					direction.Normalize();
				}
				Gizmos.DrawRay(transform.TransformPoint(position), transform.TransformDirection(direction) * Scale);
			}
		}
	}
}
