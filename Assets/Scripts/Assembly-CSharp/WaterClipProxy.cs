using System.Collections;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WaterClipProxy : MonoBehaviour, ICompileTimeCheckable
{
	public enum Shape
	{
		DistanceField = 0,
		Box = 1
	}

	public Shape shape;

	public bool immovable;

	public Material clipMaterial;

	public AssetReferenceDistanceField distanceFieldRef;

	private bool initialized;

	private Texture3D distanceFieldTexture;

	private Vector3 distanceFieldMin;

	private Vector3 distanceFieldMax;

	private Vector3 distanceFieldSize;

	private WaterSurface waterSurface;

	private AsyncOperationHandle<DistanceField> request;

	private IEnumerator LoadAsync()
	{
		UnloadDistanceField();
		Vector3 borderSizeScaled = default(Vector3);
		borderSizeScaled.x = waterSurface.foamDistance / base.transform.lossyScale.x;
		borderSizeScaled.y = waterSurface.foamDistance / base.transform.lossyScale.y;
		borderSizeScaled.z = waterSurface.foamDistance / base.transform.lossyScale.z;
		if (shape == Shape.DistanceField)
		{
			request = AddressablesUtility.LoadAsync<DistanceField>(distanceFieldRef.RuntimeKey);
			yield return request;
			DistanceField distanceField = request.Result;
			if (distanceField == null)
			{
				Debug.LogErrorFormat("Couldn't load '{0}'", distanceFieldRef.RuntimeKey);
				distanceField = new DistanceField();
			}
			distanceFieldMin = distanceField.min;
			distanceFieldMax = distanceField.max;
			distanceFieldSize = distanceFieldMax - distanceFieldMin;
			distanceFieldTexture = distanceField.texture;
			Vector3 extents = distanceFieldSize * 0.5f + borderSizeScaled;
			Vector3 vector = (distanceFieldMin + distanceFieldMax) * 0.5f;
			if (immovable && !GetIntersectsWaterSurface(vector, distanceField.meshBoundsSize * 0.5f))
			{
				base.gameObject.SetActive(value: false);
				yield break;
			}
			CreateBoxMesh(vector, extents);
		}
		else if (shape == Shape.Box)
		{
			Vector3 vector2 = Vector3.one * 0.5f + borderSizeScaled;
			Vector3 zero = Vector3.zero;
			if (immovable && !GetIntersectsWaterSurface(zero, vector2))
			{
				base.gameObject.SetActive(value: false);
				yield break;
			}
			CreateBoxMesh(zero, vector2);
		}
		MeshRenderer meshRenderer = base.gameObject.EnsureComponent<MeshRenderer>();
		meshRenderer.material = clipMaterial;
		clipMaterial = meshRenderer.material;
		UpdateMaterial();
	}

	private void UnloadDistanceField()
	{
		distanceFieldTexture = null;
		distanceFieldMin = Vector3.zero;
		distanceFieldMax = Vector3.zero;
		distanceFieldSize = Vector3.zero;
		if (request.IsValid())
		{
			AddressablesUtility.QueueRelease(ref request);
		}
	}

	private void OnDestroy()
	{
		UnloadDistanceField();
	}

	private void Start()
	{
		waterSurface = WaterSurface.Get();
	}

	public void Rebuild()
	{
		initialized = false;
	}

	private void Update()
	{
		if (!initialized)
		{
			CoroutineHost.StartCoroutine(LoadAsync());
			initialized = true;
		}
	}

	private void UpdateMaterial()
	{
		if (shape == Shape.DistanceField)
		{
			clipMaterial.SetTexture(ShaderPropertyID._DistanceFieldTexture, distanceFieldTexture);
			clipMaterial.SetVector(ShaderPropertyID._DistanceFieldMin, distanceFieldMin);
			clipMaterial.SetVector(ShaderPropertyID._DistanceFieldSizeRcp, new Vector3(1f / distanceFieldSize.x, 1f / distanceFieldSize.y, 1f / distanceFieldSize.z));
			clipMaterial.SetFloat(ShaderPropertyID._DistanceFieldScale, 5f);
		}
		else if (shape == Shape.Box)
		{
			clipMaterial.SetVector(ShaderPropertyID._ObjectScale, base.transform.lossyScale);
		}
		if (waterSurface != null)
		{
			clipMaterial.SetTexture(ShaderPropertyID._WaterDisplacementTexture, waterSurface.GetDisplacementTexture());
			clipMaterial.SetFloat(ShaderPropertyID._WaterPatchLength, waterSurface.GetPatchLength());
		}
		if (shape == Shape.DistanceField)
		{
			clipMaterial.EnableKeyword("SHAPE_DISTANCE_FIELD");
			clipMaterial.DisableKeyword("SHAPE_BOX");
		}
		else if (shape == Shape.Box)
		{
			clipMaterial.EnableKeyword("SHAPE_BOX");
			clipMaterial.DisableKeyword("SHAPE_DISTANCE_FIELD");
		}
	}

	private void CreateBoxMesh(Vector3 center, Vector3 extents)
	{
		MeshFilter meshFilter = base.gameObject.EnsureComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		Vector3 vector = center + new Vector3(0f - extents.x, 0f - extents.y, extents.z);
		Vector3 vector2 = center + new Vector3(extents.x, 0f - extents.y, extents.z);
		Vector3 vector3 = center + new Vector3(extents.x, 0f - extents.y, 0f - extents.z);
		Vector3 vector4 = center + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z);
		Vector3 vector5 = center + new Vector3(0f - extents.x, extents.y, extents.z);
		Vector3 vector6 = center + new Vector3(extents.x, extents.y, extents.z);
		Vector3 vector7 = center + new Vector3(extents.x, extents.y, 0f - extents.z);
		Vector3 vector8 = center + new Vector3(0f - extents.x, extents.y, 0f - extents.z);
		Vector3[] vertices = new Vector3[24]
		{
			vector, vector2, vector3, vector4, vector8, vector5, vector, vector4, vector5, vector6,
			vector2, vector, vector7, vector8, vector4, vector3, vector6, vector7, vector3, vector2,
			vector8, vector7, vector6, vector5
		};
		int[] triangles = new int[36]
		{
			3, 1, 0, 3, 2, 1, 7, 5, 4, 7,
			6, 5, 11, 9, 8, 11, 10, 9, 15, 13,
			12, 15, 14, 13, 19, 17, 16, 19, 18, 17,
			23, 21, 20, 23, 22, 21
		};
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
		meshFilter.mesh = mesh;
	}

	private void OnValidate()
	{
		UpdateMaterial();
	}

	private void OnDrawGizmosSelected()
	{
		if (shape == Shape.Box)
		{
			Gizmos.color = Color.yellow;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
		}
	}

	private bool GetIntersectsWaterSurface(Vector3 localPosition, Vector3 localSize)
	{
		Vector3 vector = base.transform.TransformPoint(localPosition);
		Vector3 vector2 = localSize.x * base.transform.right;
		Vector3 vector3 = localSize.y * base.transform.up;
		Vector3 vector4 = localSize.z * base.transform.forward;
		float num = Mathf.Abs(vector2.y + vector3.y + vector4.y);
		float num2 = 1f;
		if (vector.y - num > num2 || vector.y + num < 0f - num2)
		{
			return false;
		}
		return true;
	}

	public string CompileTimeCheck()
	{
		return null;
	}
}
