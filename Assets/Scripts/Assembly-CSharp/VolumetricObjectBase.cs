using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VolumetricObjectBase : MonoBehaviour
{
	public string volumeShader = "";

	protected Material volumetricMaterial;

	public float visibility = 3f;

	public Color volumeColor = new Color(1f, 1f, 1f, 1f);

	public Texture2D texture;

	public float textureScale = 1f;

	public Vector3 textureMovement = new Vector3(0f, -0.1f, 0f);

	protected Mesh meshInstance;

	protected Material materialInstance;

	protected Transform thisTransform;

	protected float previousVisibility = 1f;

	protected Color previousVolumeColor = new Color(1f, 1f, 1f, 1f);

	protected Vector3 forcedLocalScale = Vector3.one;

	protected Texture2D previousTexture;

	protected float previousTextureScale = 10f;

	protected Vector3 previousTextureMovement = new Vector3(0f, 0.1f, 0f);

	protected Vector3[] unitVerts = new Vector3[8];

	protected virtual void OnEnable()
	{
		SetupUnitVerts();
		thisTransform = base.transform;
		if (meshInstance != null)
		{
			Object.Destroy(meshInstance);
		}
		meshInstance = CreateCube();
		GetComponent<MeshFilter>().sharedMesh = meshInstance;
		if (materialInstance != null)
		{
			Object.Destroy(materialInstance);
		}
		if (volumeShader == "")
		{
			PopulateShaderName();
		}
		volumetricMaterial = new Material(Shader.Find(volumeShader));
		MeshRenderer component = GetComponent<MeshRenderer>();
		component.sharedMaterial = volumetricMaterial;
		materialInstance = component.sharedMaterial;
		component.castShadows = false;
		component.receiveShadows = false;
		if ((bool)Camera.current)
		{
			Camera.current.depthTextureMode |= DepthTextureMode.Depth;
		}
		if ((bool)MainCamera.camera)
		{
			MainCamera.camera.depthTextureMode |= DepthTextureMode.Depth;
		}
		UpdateVolume();
	}

	protected virtual void OnDestroy()
	{
		CleanUp();
	}

	protected virtual void OnDisable()
	{
		CleanUp();
	}

	protected virtual void CleanUp()
	{
		if ((bool)materialInstance)
		{
			Object.DestroyImmediate(materialInstance);
		}
		if ((bool)meshInstance)
		{
			Object.DestroyImmediate(meshInstance);
		}
	}

	public virtual void PopulateShaderName()
	{
	}

	private void LateUpdate()
	{
		if (HasChanged())
		{
			SetChangedValues();
			UpdateVolume();
		}
	}

	public virtual bool HasChanged()
	{
		if (visibility != previousVisibility || volumeColor != previousVolumeColor || thisTransform.localScale != forcedLocalScale || texture != previousTexture || textureScale != previousTextureScale || textureMovement != previousTextureMovement)
		{
			return true;
		}
		return false;
	}

	protected virtual void SetChangedValues()
	{
		previousVisibility = visibility;
		previousVolumeColor = volumeColor;
		thisTransform.localScale = forcedLocalScale;
		previousTexture = texture;
		previousTextureScale = textureScale;
		previousTextureMovement = textureMovement;
	}

	public virtual void UpdateVolume()
	{
	}

	public void SetupUnitVerts()
	{
		float num = 0.5f;
		unitVerts[0].x = 0f - num;
		unitVerts[0].y = 0f - num;
		unitVerts[0].z = 0f - num;
		unitVerts[1].x = num;
		unitVerts[1].y = 0f - num;
		unitVerts[1].z = 0f - num;
		unitVerts[2].x = num;
		unitVerts[2].y = num;
		unitVerts[2].z = 0f - num;
		unitVerts[3].x = num;
		unitVerts[3].y = 0f - num;
		unitVerts[3].z = num;
		unitVerts[4].x = num;
		unitVerts[4].y = num;
		unitVerts[4].z = num;
		unitVerts[5].x = 0f - num;
		unitVerts[5].y = num;
		unitVerts[5].z = 0f - num;
		unitVerts[6].x = 0f - num;
		unitVerts[6].y = num;
		unitVerts[6].z = num;
		unitVerts[7].x = 0f - num;
		unitVerts[7].y = 0f - num;
		unitVerts[7].z = num;
	}

	public Mesh CreateCube()
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[unitVerts.Length];
		unitVerts.CopyTo(array, 0);
		mesh.vertices = array;
		int[] array2 = new int[36];
		int num = 0;
		array2[num] = 0;
		num++;
		array2[num] = 2;
		num++;
		array2[num] = 1;
		num++;
		array2[num] = 0;
		num++;
		array2[num] = 5;
		num++;
		array2[num] = 2;
		num++;
		array2[num] = 3;
		num++;
		array2[num] = 6;
		num++;
		array2[num] = 7;
		num++;
		array2[num] = 3;
		num++;
		array2[num] = 4;
		num++;
		array2[num] = 6;
		num++;
		array2[num] = 1;
		num++;
		array2[num] = 4;
		num++;
		array2[num] = 3;
		num++;
		array2[num] = 1;
		num++;
		array2[num] = 2;
		num++;
		array2[num] = 4;
		num++;
		array2[num] = 7;
		num++;
		array2[num] = 5;
		num++;
		array2[num] = 0;
		num++;
		array2[num] = 7;
		num++;
		array2[num] = 6;
		num++;
		array2[num] = 5;
		num++;
		array2[num] = 7;
		num++;
		array2[num] = 1;
		num++;
		array2[num] = 3;
		num++;
		array2[num] = 7;
		num++;
		array2[num] = 0;
		num++;
		array2[num] = 1;
		num++;
		array2[num] = 5;
		num++;
		array2[num] = 4;
		num++;
		array2[num] = 2;
		num++;
		array2[num] = 5;
		num++;
		array2[num] = 6;
		num++;
		array2[num] = 4;
		num++;
		mesh.triangles = array2;
		mesh.RecalculateBounds();
		return mesh;
	}

	public void ScaleMesh(Mesh mesh, Vector3 scaleFactor)
	{
		ScaleMesh(mesh, scaleFactor, Vector3.zero);
	}

	public void ScaleMesh(Mesh mesh, Vector3 scaleFactor, Vector3 addVector)
	{
		Vector3[] array = new Vector3[mesh.vertexCount];
		for (int i = 0; i < mesh.vertexCount; i++)
		{
			array[i] = ScaleVector(unitVerts[i], scaleFactor) + addVector;
		}
		mesh.vertices = array;
	}

	private Vector3 ScaleVector(Vector3 vector, Vector3 scale)
	{
		return new Vector3(vector.x * scale.x, vector.y * scale.y, vector.z * scale.z);
	}

	public Mesh CopyMesh(Mesh original)
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[original.vertices.Length];
		original.vertices.CopyTo(array, 0);
		mesh.vertices = array;
		Vector2[] array2 = new Vector2[original.uv.Length];
		original.uv.CopyTo(array2, 0);
		mesh.uv = array2;
		Vector2[] array3 = new Vector2[original.uv2.Length];
		original.uv2.CopyTo(array3, 0);
		mesh.uv2 = array3;
		Vector2[] array4 = new Vector2[original.uv2.Length];
		original.uv2.CopyTo(array4, 0);
		mesh.uv2 = array4;
		Vector3[] array5 = new Vector3[original.normals.Length];
		original.normals.CopyTo(array5, 0);
		mesh.normals = array5;
		Vector4[] array6 = new Vector4[original.tangents.Length];
		original.tangents.CopyTo(array6, 0);
		mesh.tangents = array6;
		Color[] array7 = new Color[original.colors.Length];
		original.colors.CopyTo(array7, 0);
		mesh.colors = array7;
		mesh.subMeshCount = original.subMeshCount;
		for (int i = 0; i < original.subMeshCount; i++)
		{
			int[] triangles = original.GetTriangles(i);
			int[] array8 = new int[triangles.Length];
			triangles.CopyTo(array8, 0);
			mesh.SetTriangles(triangles, i);
		}
		mesh.RecalculateBounds();
		return mesh;
	}
}
