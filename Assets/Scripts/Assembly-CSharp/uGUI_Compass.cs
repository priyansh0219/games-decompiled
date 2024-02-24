using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_Compass : Graphic
{
	[Serializable]
	public class Label
	{
		public string name;

		public float angle;

		public TextMeshProUGUI text;
	}

	private static readonly Vector4 sDefaultTangent = new Vector4(1f, 0f, 0f, -1f);

	private static readonly Vector3 sDefaultNormal = new Vector3(0f, 0f, -1f);

	public bool debug;

	[Range(0f, 1f)]
	public float debugDirection;

	public float size = 3.9f;

	public Vector3 position = new Vector3(0f, -0.54f, -12.6f);

	public Vector3 rotation = new Vector3(-70.8f, 0f, 0f);

	public Vector3 scale = new Vector3(1f, 1f, 1f);

	public float scale2D = 342f;

	public float fov = 90f;

	public float near;

	public float far = 10f;

	[AssertNotNull]
	public Texture2D texture;

	public float radius = 4.5f;

	[Range(0f, 1f)]
	public float alphaFrom = 0.625f;

	[Range(0f, 1f)]
	public float alphaTo = 0.875f;

	public Label[] labels = new Label[8];

	private float _direction;

	private Matrix4x4 _matrixLocal;

	private bool _visible = true;

	private Material _material;

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

	public float direction
	{
		get
		{
			return _direction;
		}
		set
		{
			value -= Mathf.Floor(value);
			if (_direction != value)
			{
				_direction = value;
				UpdateAngle();
				UpdateLabels();
			}
		}
	}

	public Matrix4x4 projection => Matrix4x4.Perspective(fov, 1f, near, far);

	public override Material materialForRendering
	{
		get
		{
			if (_material == null)
			{
				_material = UnityEngine.Object.Instantiate(material);
			}
			return _material;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (_visible)
		{
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

	public void SetVisible(bool visible)
	{
		if (_visible == visible)
		{
			return;
		}
		_visible = visible;
		SetVerticesDirty();
		int i = 0;
		for (int num = labels.Length; i < num; i++)
		{
			Label label = labels[i];
			if (label != null)
			{
				TextMeshProUGUI text = label.text;
				if (!(text == null))
				{
					text.enabled = _visible;
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)_material)
		{
			UnityEngine.Object.Destroy(_material);
		}
	}

	private void UpdateAngle()
	{
		float num = _direction * 360f;
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, Quaternion.Euler(rotation.x, rotation.y, rotation.z - num), scale);
		_matrixLocal = projection * matrix4x;
		materialForRendering.SetFloat(ShaderPropertyID._Angle, (rotation.z - num) * ((float)Math.PI / 180f));
		materialForRendering.SetFloat(ShaderPropertyID._AlphaFrom, alphaFrom);
		materialForRendering.SetFloat(ShaderPropertyID._AlphaTo, alphaTo);
	}

	private void UpdateLabels()
	{
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		int i = 0;
		for (int num = labels.Length; i < num; i++)
		{
			Label label = labels[i];
			if (label == null)
			{
				continue;
			}
			TextMeshProUGUI text = label.text;
			if (!(text == null))
			{
				RectTransform rectTransform = text.rectTransform;
				Transform parent = rectTransform.parent;
				if (!(parent == null))
				{
					float f = (float)Math.PI / 180f * (rotation.z + label.angle);
					Vector3 point = new Vector3(radius * Mathf.Cos(f), radius * Mathf.Sin(f), 0f);
					Vector3 vector = _matrixLocal.MultiplyPoint3x4(point);
					float z = vector.z;
					point = new Vector3(scale2D * vector.x / z, scale2D * vector.y / z, 0f);
					point = localToWorldMatrix.MultiplyPoint3x4(point);
					point = parent.worldToLocalMatrix.MultiplyPoint3x4(point);
					rectTransform.anchoredPosition = point;
					float num2 = (rotation.z + label.angle - 90f - _direction * 360f) / 360f;
					float num3 = Mathf.Clamp01((num2 - Mathf.Floor(num2) - alphaFrom) / (alphaTo - alphaFrom));
					num3 = 1f - Mathf.Abs(2f * num3 - 1f);
					num3 = Mathf.Sin(num3);
					text.color = new Color(1f, 1f, 1f, num3);
				}
			}
		}
	}
}
