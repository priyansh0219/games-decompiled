using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class VoxelandHighlight : MonoBehaviour
{
	public enum VoxelandHighlightType
	{
		None = 0,
		Face = 1,
		Multiface = 2,
		Box = 3,
		Sphere = 4,
		Cylinder = 5,
		Mesh = 6
	}

	public Voxeland land;

	public MeshFilter filter;

	public MeshRenderer renderer;

	public readonly Vector3[] verts = new Vector3[6];

	public readonly Vector2[] uvs = new Vector2[6];

	public readonly int[] tris = new int[6];

	public VoxelandHighlightType oldType;

	public VoxelandChunk oldChunk;

	public int oldFaceNum;

	public int oldChunkNumFaces;

	private Mesh faceMesh;

	private Mesh cubeMesh;

	private Mesh sphereMesh;

	private Mesh cylinderMesh;

	public static Vector3[] cubeVerts = new Vector3[24]
	{
		new Vector3(-1f, -1f, -1f),
		new Vector3(1f, -1f, -1f),
		new Vector3(-1f, -1f, 1f),
		new Vector3(1f, -1f, 1f),
		new Vector3(-1f, 1f, -1f),
		new Vector3(1f, 1f, -1f),
		new Vector3(-1f, 1f, 1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(-1f, -1f, -1f),
		new Vector3(-1f, -1f, -1f),
		new Vector3(1f, -1f, -1f),
		new Vector3(1f, -1f, -1f),
		new Vector3(-1f, -1f, 1f),
		new Vector3(-1f, -1f, 1f),
		new Vector3(1f, -1f, 1f),
		new Vector3(1f, -1f, 1f),
		new Vector3(-1f, 1f, -1f),
		new Vector3(-1f, 1f, -1f),
		new Vector3(1f, 1f, -1f),
		new Vector3(1f, 1f, -1f),
		new Vector3(-1f, 1f, 1f),
		new Vector3(-1f, 1f, 1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(1f, 1f, 1f)
	};

	public static Vector2[] cubeUVs = new Vector2[24];

	public static int[] cubeTris = new int[36]
	{
		0, 1, 3, 3, 2, 0, 4, 6, 7, 7,
		5, 4, 8, 16, 18, 18, 10, 8, 11, 19,
		22, 22, 14, 11, 15, 23, 20, 20, 12, 15,
		13, 21, 17, 17, 9, 13
	};

	public static Vector3[] sphereVerts = new Vector3[92]
	{
		new Vector3(0f, 1f, 0f),
		new Vector3(0.894427f, 0.447214f, 0f),
		new Vector3(0.276393f, 0.447214f, 0.850651f),
		new Vector3(-0.723607f, 0.447214f, 0.525731f),
		new Vector3(-0.723607f, 0.447214f, -0.525731f),
		new Vector3(0.276393f, 0.447214f, -0.850651f),
		new Vector3(0.723607f, -0.447214f, 0.525731f),
		new Vector3(-0.276393f, -0.447214f, 0.850651f),
		new Vector3(-0.894427f, -0.447214f, 0f),
		new Vector3(-0.276393f, -0.447214f, -0.850651f),
		new Vector3(0.723607f, -0.447214f, -0.525731f),
		new Vector3(0f, -1f, 0f),
		new Vector3(0.360729f, 0.932671f, 0f),
		new Vector3(0.672883f, 0.739749f, 0f),
		new Vector3(0.111471f, 0.932671f, 0.343074f),
		new Vector3(0.207932f, 0.739749f, 0.63995f),
		new Vector3(-0.291836f, 0.932671f, 0.212031f),
		new Vector3(-0.544374f, 0.739749f, 0.395511f),
		new Vector3(-0.291836f, 0.932671f, -0.212031f),
		new Vector3(-0.544374f, 0.739749f, -0.395511f),
		new Vector3(0.111471f, 0.932671f, -0.343074f),
		new Vector3(0.207932f, 0.739749f, -0.63995f),
		new Vector3(0.784354f, 0.516806f, 0.343074f),
		new Vector3(0.568661f, 0.516806f, 0.63995f),
		new Vector3(-0.0839038f, 0.516806f, 0.851981f),
		new Vector3(-0.432902f, 0.516806f, 0.738584f),
		new Vector3(-0.83621f, 0.516806f, 0.183479f),
		new Vector3(-0.83621f, 0.516806f, -0.183479f),
		new Vector3(-0.432902f, 0.516806f, -0.738584f),
		new Vector3(-0.0839036f, 0.516806f, -0.851981f),
		new Vector3(0.568661f, 0.516806f, -0.63995f),
		new Vector3(0.784354f, 0.516806f, -0.343074f),
		new Vector3(0.964719f, 0.156077f, 0.212031f),
		new Vector3(0.905103f, -0.156077f, 0.395511f),
		new Vector3(0.0964608f, 0.156077f, 0.983023f),
		new Vector3(-0.0964609f, -0.156077f, 0.983024f),
		new Vector3(-0.905103f, 0.156077f, 0.395511f),
		new Vector3(-0.964719f, -0.156077f, 0.212031f),
		new Vector3(-0.655845f, 0.156077f, -0.738585f),
		new Vector3(-0.499768f, -0.156077f, -0.851981f),
		new Vector3(0.499768f, 0.156077f, -0.851981f),
		new Vector3(0.655845f, -0.156077f, -0.738584f),
		new Vector3(0.964719f, 0.156077f, -0.212031f),
		new Vector3(0.905103f, -0.156077f, -0.395511f),
		new Vector3(0.499768f, 0.156077f, 0.851981f),
		new Vector3(0.655845f, -0.156077f, 0.738584f),
		new Vector3(-0.655845f, 0.156077f, 0.738584f),
		new Vector3(-0.499768f, -0.156077f, 0.851981f),
		new Vector3(-0.905103f, 0.156077f, -0.395511f),
		new Vector3(-0.964719f, -0.156077f, -0.212031f),
		new Vector3(0.0964611f, 0.156077f, -0.983024f),
		new Vector3(-0.0964605f, -0.156077f, -0.983023f),
		new Vector3(0.432902f, -0.516806f, 0.738584f),
		new Vector3(0.0839037f, -0.516806f, 0.851981f),
		new Vector3(-0.568661f, -0.516806f, 0.63995f),
		new Vector3(-0.784354f, -0.516806f, 0.343074f),
		new Vector3(-0.784354f, -0.516806f, -0.343074f),
		new Vector3(-0.568661f, -0.516806f, -0.63995f),
		new Vector3(0.083904f, -0.516806f, -0.851981f),
		new Vector3(0.432902f, -0.516806f, -0.738584f),
		new Vector3(0.83621f, -0.516806f, -0.183479f),
		new Vector3(0.83621f, -0.516806f, 0.183479f),
		new Vector3(0.291836f, -0.932671f, 0.212031f),
		new Vector3(0.544374f, -0.739749f, 0.395511f),
		new Vector3(-0.111471f, -0.932671f, 0.343074f),
		new Vector3(-0.207932f, -0.739749f, 0.63995f),
		new Vector3(-0.360729f, -0.932671f, 0f),
		new Vector3(-0.672883f, -0.739749f, 0f),
		new Vector3(-0.111471f, -0.932671f, -0.343074f),
		new Vector3(-0.207932f, -0.739749f, -0.63995f),
		new Vector3(0.291836f, -0.932671f, -0.212031f),
		new Vector3(0.544374f, -0.739749f, -0.395511f),
		new Vector3(0.479506f, 0.805422f, 0.348381f),
		new Vector3(-0.183155f, 0.805422f, 0.563693f),
		new Vector3(-0.592702f, 0.805422f, 0f),
		new Vector3(-0.183155f, 0.805422f, -0.563693f),
		new Vector3(0.479506f, 0.805422f, -0.348381f),
		new Vector3(0.985456f, -0.169933f, 0f),
		new Vector3(0.304522f, -0.169933f, 0.937224f),
		new Vector3(-0.79725f, -0.169933f, 0.579236f),
		new Vector3(-0.79725f, -0.169933f, -0.579236f),
		new Vector3(0.304523f, -0.169933f, -0.937224f),
		new Vector3(0.79725f, 0.169933f, 0.579236f),
		new Vector3(-0.304523f, 0.169933f, 0.937224f),
		new Vector3(-0.985456f, 0.169933f, 0f),
		new Vector3(-0.304522f, 0.169933f, -0.937224f),
		new Vector3(0.79725f, 0.169933f, -0.579236f),
		new Vector3(0.183155f, -0.805422f, 0.563693f),
		new Vector3(-0.479506f, -0.805422f, 0.348381f),
		new Vector3(-0.479506f, -0.805422f, -0.348381f),
		new Vector3(0.183155f, -0.805422f, -0.563693f),
		new Vector3(0.592702f, -0.805422f, 0f)
	};

	public static Vector2[] sphereUVs = new Vector2[92];

	public static int[] sphereTris = new int[540]
	{
		14, 12, 0, 72, 13, 12, 14, 72, 12, 15,
		72, 14, 22, 1, 13, 72, 22, 13, 23, 22,
		72, 15, 23, 72, 2, 23, 15, 16, 14, 0,
		73, 15, 14, 16, 73, 14, 17, 73, 16, 24,
		2, 15, 73, 24, 15, 25, 24, 73, 17, 25,
		73, 3, 25, 17, 18, 16, 0, 74, 17, 16,
		18, 74, 16, 19, 74, 18, 26, 3, 17, 74,
		26, 17, 27, 26, 74, 19, 27, 74, 4, 27,
		19, 20, 18, 0, 75, 19, 18, 20, 75, 18,
		21, 75, 20, 28, 4, 19, 75, 28, 19, 29,
		28, 75, 21, 29, 75, 5, 29, 21, 12, 20,
		0, 76, 21, 20, 12, 76, 20, 13, 76, 12,
		30, 5, 21, 76, 30, 21, 31, 30, 76, 13,
		31, 76, 1, 31, 13, 32, 42, 1, 77, 43,
		42, 32, 77, 42, 33, 77, 32, 60, 10, 43,
		77, 60, 43, 61, 60, 77, 33, 61, 77, 6,
		61, 33, 34, 44, 2, 78, 45, 44, 34, 78,
		44, 35, 78, 34, 52, 6, 45, 78, 52, 45,
		53, 52, 78, 35, 53, 78, 7, 53, 35, 36,
		46, 3, 79, 47, 46, 36, 79, 46, 37, 79,
		36, 54, 7, 47, 79, 54, 47, 55, 54, 79,
		37, 55, 79, 8, 55, 37, 38, 48, 4, 80,
		49, 48, 38, 80, 48, 39, 80, 38, 56, 8,
		49, 80, 56, 49, 57, 56, 80, 39, 57, 80,
		9, 57, 39, 40, 50, 5, 81, 51, 50, 40,
		81, 50, 41, 81, 40, 58, 9, 51, 81, 58,
		51, 59, 58, 81, 41, 59, 81, 10, 59, 41,
		33, 45, 6, 82, 44, 45, 33, 82, 45, 32,
		82, 33, 23, 2, 44, 82, 23, 44, 22, 23,
		82, 32, 22, 82, 1, 22, 32, 35, 47, 7,
		83, 46, 47, 35, 83, 47, 34, 83, 35, 25,
		3, 46, 83, 25, 46, 24, 25, 83, 34, 24,
		83, 2, 24, 34, 37, 49, 8, 84, 48, 49,
		37, 84, 49, 36, 84, 37, 27, 4, 48, 84,
		27, 48, 26, 27, 84, 36, 26, 84, 3, 26,
		36, 39, 51, 9, 85, 50, 51, 39, 85, 51,
		38, 85, 39, 29, 5, 50, 85, 29, 50, 28,
		29, 85, 38, 28, 85, 4, 28, 38, 41, 43,
		10, 86, 42, 43, 41, 86, 43, 40, 86, 41,
		31, 1, 42, 86, 31, 42, 30, 31, 86, 40,
		30, 86, 5, 30, 40, 62, 64, 11, 87, 65,
		64, 62, 87, 64, 63, 87, 62, 53, 7, 65,
		87, 53, 65, 52, 53, 87, 63, 52, 87, 6,
		52, 63, 64, 66, 11, 88, 67, 66, 64, 88,
		66, 65, 88, 64, 55, 8, 67, 88, 55, 67,
		54, 55, 88, 65, 54, 88, 7, 54, 65, 66,
		68, 11, 89, 69, 68, 66, 89, 68, 67, 89,
		66, 57, 9, 69, 89, 57, 69, 56, 57, 89,
		67, 56, 89, 8, 56, 67, 68, 70, 11, 90,
		71, 70, 68, 90, 70, 69, 90, 68, 59, 10,
		71, 90, 59, 71, 58, 59, 90, 69, 58, 90,
		9, 58, 69, 70, 62, 11, 91, 63, 62, 70,
		91, 62, 71, 91, 70, 61, 6, 63, 91, 61,
		63, 60, 61, 91, 71, 60, 91, 10, 60, 71
	};

	public static Vector3[] cylinderVerts = new Vector3[26]
	{
		new Vector3(0f, -1f, 0f),
		new Vector3(1f, -1f, 0f),
		new Vector3(0.8660254f, -1f, 0.5f),
		new Vector3(0.5f, -1f, 0.8660254f),
		new Vector3(6.123234E-17f, -1f, 1f),
		new Vector3(-0.5f, -1f, 0.8660254f),
		new Vector3(-0.8660254f, -1f, 0.5f),
		new Vector3(-1f, -1f, 1.2246469E-16f),
		new Vector3(-0.8660254f, -1f, -0.5f),
		new Vector3(-0.5f, -1f, -0.8660254f),
		new Vector3(-1.8369701E-16f, -1f, -1f),
		new Vector3(0.5f, -1f, -0.8660254f),
		new Vector3(0.8660254f, -1f, -0.5f),
		new Vector3(0f, 1f, 0f),
		new Vector3(1f, 1f, 0f),
		new Vector3(0.8660254f, 1f, 0.5f),
		new Vector3(0.5f, 1f, 0.8660254f),
		new Vector3(6.123234E-17f, 1f, 1f),
		new Vector3(-0.5f, 1f, 0.8660254f),
		new Vector3(-0.8660254f, 1f, 0.5f),
		new Vector3(-1f, 1f, 1.2246469E-16f),
		new Vector3(-0.8660254f, 1f, -0.5f),
		new Vector3(-0.5f, 1f, -0.8660254f),
		new Vector3(-1.8369701E-16f, 1f, -1f),
		new Vector3(0.5f, 1f, -0.8660254f),
		new Vector3(0.8660254f, 1f, -0.5f)
	};

	public static Vector2[] cylinderUVs = new Vector2[26];

	public static int[] cylinderTris = new int[144]
	{
		2, 0, 1, 3, 0, 2, 4, 0, 3, 5,
		0, 4, 6, 0, 5, 7, 0, 6, 8, 0,
		7, 9, 0, 8, 10, 0, 9, 11, 0, 10,
		12, 0, 11, 1, 0, 12, 14, 13, 15, 15,
		13, 16, 16, 13, 17, 17, 13, 18, 18, 13,
		19, 19, 13, 20, 20, 13, 21, 21, 13, 22,
		22, 13, 23, 23, 13, 24, 24, 13, 25, 25,
		13, 14, 1, 14, 2, 14, 15, 2, 2, 15,
		3, 15, 16, 3, 3, 16, 4, 16, 17, 4,
		4, 17, 5, 17, 18, 5, 5, 18, 6, 18,
		19, 6, 6, 19, 7, 19, 20, 7, 7, 20,
		8, 20, 21, 8, 8, 21, 9, 21, 22, 9,
		9, 22, 10, 22, 23, 10, 10, 23, 11, 23,
		24, 11, 11, 24, 12, 24, 25, 12, 12, 25,
		1, 25, 14, 1
	};

	public void Init()
	{
		land = base.transform.parent.GetComponent<Voxeland>();
		base.transform.localPosition = new Vector3(0f, 0f, 0f);
		base.transform.localScale = new Vector3(1f, 1f, 1f);
		base.transform.localRotation = Quaternion.identity;
		filter = base.gameObject.AddComponent<MeshFilter>();
		renderer = base.gameObject.AddComponent<MeshRenderer>();
		renderer.shadowCastingMode = ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		faceMesh = new Mesh();
		faceMesh.name = "Face";
		faceMesh.vertices = verts;
		faceMesh.uv = uvs;
		cubeMesh = new Mesh();
		cubeMesh.name = "Cube";
		cubeMesh.vertices = cubeVerts;
		cubeMesh.uv = cubeUVs;
		cubeMesh.triangles = cubeTris;
		cubeMesh.RecalculateBounds();
		sphereMesh = new Mesh();
		sphereMesh.name = "Sphere";
		sphereMesh.vertices = sphereVerts;
		sphereMesh.uv = sphereUVs;
		sphereMesh.triangles = sphereTris;
		sphereMesh.RecalculateBounds();
		cylinderMesh = new Mesh();
		cylinderMesh.name = "Cylinder";
		cylinderMesh.vertices = cylinderVerts;
		cylinderMesh.uv = cylinderUVs;
		cylinderMesh.triangles = cylinderTris;
		cylinderMesh.RecalculateBounds();
	}

	public void DebugHit(VoxelandChunk chunk, RaycastHit hit)
	{
		MeshCollider meshCollider = hit.collider as MeshCollider;
		if (!(meshCollider == null) && !(meshCollider.sharedMesh == null))
		{
			Mesh sharedMesh = meshCollider.sharedMesh;
			Vector3[] vertices = sharedMesh.vertices;
			int[] triangles = sharedMesh.triangles;
			Vector3 position = vertices[triangles[hit.triangleIndex * 3]];
			Vector3 position2 = vertices[triangles[hit.triangleIndex * 3 + 1]];
			Vector3 position3 = vertices[triangles[hit.triangleIndex * 3 + 2]];
			Transform obj = hit.collider.transform;
			position = obj.TransformPoint(position);
			position2 = obj.TransformPoint(position2);
			position3 = obj.TransformPoint(position3);
			Vector3 vector = (position + position2 + position3) / 3f;
			Debug.Log("Hit triangle " + hit.triangleIndex + " at (" + vector.x + "," + vector.y + "," + vector.z + ")");
		}
	}

	public void DrawFace(VoxelandChunk chunk, int faceNum, Color color)
	{
		if ((bool)chunk && (oldType != VoxelandHighlightType.Face || !(chunk == oldChunk) || faceNum != oldFaceNum))
		{
			Vector3 landSpaceMeshOrigin = chunk.GetLandSpaceMeshOrigin();
			Mesh sharedMesh = chunk.collision.sharedMesh;
			for (int i = 0; i < 6; i++)
			{
				tris[i] = i;
				verts[i] = sharedMesh.vertices[sharedMesh.triangles[6 * faceNum + i]] + landSpaceMeshOrigin;
			}
			faceMesh.vertices = verts;
			faceMesh.triangles = tris;
			faceMesh.RecalculateBounds();
			oldChunk = chunk;
			oldFaceNum = faceNum;
			DrawMeshImpl(color, VoxelandHighlightType.Face, faceMesh);
		}
	}

	public void DrawCube(Color color)
	{
		DrawMeshImpl(color, VoxelandHighlightType.Box, cubeMesh);
	}

	public void DrawSphere(Color color)
	{
		DrawMeshImpl(color, VoxelandHighlightType.Sphere, sphereMesh);
	}

	public void DrawCylinder(Color color)
	{
		DrawMeshImpl(color, VoxelandHighlightType.Cylinder, cylinderMesh);
	}

	public void DrawMesh(Color color, Mesh mesh)
	{
		DrawMeshImpl(color, VoxelandHighlightType.Mesh, mesh);
	}

	private void DrawMeshImpl(Color color, VoxelandHighlightType type, Mesh mesh)
	{
		renderer.sharedMaterial = land.highlightMaterial;
		renderer.enabled = true;
		renderer.sharedMaterial.color = color;
		filter.sharedMesh = mesh;
		oldType = type;
	}

	public void Clear()
	{
		if ((bool)filter.sharedMesh)
		{
			filter.sharedMesh = null;
			oldChunk = null;
			oldType = VoxelandHighlightType.None;
			renderer.enabled = false;
		}
	}
}
