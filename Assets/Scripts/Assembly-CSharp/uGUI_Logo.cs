using System;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_Logo : Graphic
{
	public enum DeltaTime
	{
		DeltaTime = 0,
		UnscaledDeltaTime = 1,
		PDADeltaTime = 2
	}

	private static readonly Vector4 sDefaultTangent = new Vector4(1f, 0f, 0f, -1f);

	private static readonly Vector3 sDefaultNormal = new Vector3(0f, 0f, -1f);

	public float size = 3.9f;

	public Vector3 position = new Vector3(0f, -0.54f, -12.6f);

	[NonSerialized]
	public Vector3 rotation = new Vector3(0f, 0f, 0f);

	public Vector3 scale = new Vector3(1f, 1f, 1f);

	public float scale2D = 342f;

	public DeltaTime deltaTime;

	public float fov = 90f;

	public float near;

	public float far = 10f;

	[AssertNotNull]
	public Texture2D texture;

	public float radius = 4.5f;

	public float rotationOffset = -90f;

	public float rotationSpeed;

	[NonSerialized]
	public float angle;

	public override Texture mainTexture
	{
		get
		{
			if (texture != null)
			{
				return texture;
			}
			return base.mainTexture;
		}
	}

	public Matrix4x4 projection => Matrix4x4.Perspective(fov, 1f, near, far);

	private void Update()
	{
		float unscaledDeltaTime;
		switch (deltaTime)
		{
		default:
			unscaledDeltaTime = Time.deltaTime;
			break;
		case DeltaTime.UnscaledDeltaTime:
			unscaledDeltaTime = Time.unscaledDeltaTime;
			break;
		case DeltaTime.PDADeltaTime:
			unscaledDeltaTime = PDA.deltaTime;
			break;
		}
		angle = (angle + rotationSpeed * unscaledDeltaTime) % 180f;
		rotation = new Vector3(rotation.x, rotationOffset + angle, rotation.z);
		SetVerticesDirty();
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		Vector3[] array = new Vector3[4]
		{
			new Vector3(0f - size, 0f - size, 0f),
			new Vector3(0f - size, size, 0f),
			new Vector3(size, size, 0f),
			new Vector3(size, 0f - size, 0f)
		};
		Vector3[] array2 = new Vector3[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, Quaternion.Euler(rotation), scale);
		Matrix4x4 matrix4x2 = projection * matrix4x;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector = matrix4x2.MultiplyPoint3x4(array[i]);
			float z = vector.z;
			array[i] = new Vector3(scale2D * vector.x / z, scale2D * vector.y / z, 0f);
			Vector3 vector2 = array2[i];
			array2[i] = new Vector3(vector2.x / z, vector2.y / z, 1f / z);
		}
		Color32 color = this.color;
		vh.AddVert(array[0], color, new Vector2(array2[0].x, array2[0].y), new Vector2(array2[0].z, 0f), sDefaultTangent, sDefaultNormal);
		vh.AddVert(array[1], color, new Vector2(array2[1].x, array2[1].y), new Vector2(array2[1].z, 0f), sDefaultTangent, sDefaultNormal);
		vh.AddVert(array[2], color, new Vector2(array2[2].x, array2[2].y), new Vector2(array2[2].z, 0f), sDefaultTangent, sDefaultNormal);
		vh.AddVert(array[3], color, new Vector2(array2[3].x, array2[3].y), new Vector2(array2[3].z, 0f), sDefaultTangent, sDefaultNormal);
		vh.AddTriangle(0, 1, 2);
		vh.AddTriangle(2, 3, 0);
	}
}
