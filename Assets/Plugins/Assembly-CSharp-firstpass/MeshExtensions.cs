using System;
using UnityEngine;

public static class MeshExtensions
{
	[Obsolete("Use SetVertices(vertices, start, length) instead.")]
	public static void SetVertices(this Mesh mesh, Vector3[] vertices, int num)
	{
		mesh.SetVertices(vertices, 0, num);
	}

	[Obsolete("Use SetColors(colors, start, length) instead.")]
	public static void SetColors(this Mesh mesh, Color[] colors, int num)
	{
		mesh.SetColors(colors, 0, num);
	}

	[Obsolete("Use SetColors(colors, start, length) instead.")]
	public static void SetColors(this Mesh mesh, Color32[] colors, int num)
	{
		mesh.SetColors(colors, 0, num);
	}

	[Obsolete("Use SetNormals(normals, start, length) instead.")]
	public static void SetNormals(this Mesh mesh, Vector3[] normals, int num)
	{
		mesh.SetNormals(normals, 0, num);
	}

	[Obsolete("Use SetTangents(tangets, start, length) instead.")]
	public static void SetTangents(this Mesh mesh, Vector4[] tangents, int num)
	{
		mesh.SetTangents(tangents, 0, num);
	}

	[Obsolete("Use SetUVs(channel, uvs, start, length) instead.")]
	public static void SetUVs(this Mesh mesh, int channel, Vector2[] uvs, int num)
	{
		mesh.SetUVs(channel, uvs, 0, num);
	}

	[Obsolete("Use SetTriangles(indices, start, length, submesh, calculateBounds) instead.")]
	public static void SetTriangles(this Mesh mesh, int submesh, int[] indices, int num, bool calculateBounds)
	{
		mesh.SetTriangles(indices, 0, num, submesh, calculateBounds);
	}
}
