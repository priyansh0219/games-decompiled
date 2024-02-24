using UnityEngine;

[ExecuteInEditMode]
public class DebugVertexNormals : MonoBehaviour
{
	public float length = 1f;

	private Mesh theMesh;

	private void OnDrawGizmos()
	{
		if (theMesh == null)
		{
			MeshFilter component = base.transform.GetComponent<MeshFilter>();
			if (component != null)
			{
				theMesh = component.sharedMesh;
			}
			return;
		}
		Gizmos.color = Color.red;
		Vector3[] normals = theMesh.normals;
		Vector3[] vertices = theMesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			if (normals.Length > i)
			{
				Gizmos.DrawRay(base.transform.TransformPoint(vertices[i]), base.transform.TransformDirection(normals[i] * length));
			}
		}
	}
}
