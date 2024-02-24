using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class uGUI_CircularBar : Graphic, ILayoutSelfController, ILayoutController
{
	private const string keywordRoundCorners = "ROUND_CORNERS";

	private const string keywordBorder = "BORDER";

	private const string keywordOverlay = "OVERLAY";

	[SerializeField]
	[HideInInspector]
	private Texture2D _texture;

	[SerializeField]
	[HideInInspector]
	private bool _border = true;

	[SerializeField]
	[HideInInspector]
	private Color _borderColor = new Color(0f, 1f, 1f, 1f);

	[SerializeField]
	[HideInInspector]
	private float _edgeWidth = 0.1f;

	[SerializeField]
	[HideInInspector]
	private float _borderWidth = 0.1f;

	[SerializeField]
	[HideInInspector]
	private bool _roundCorners = true;

	[SerializeField]
	[HideInInspector]
	private float _size = 1f;

	[SerializeField]
	[HideInInspector]
	private float _width = 1f;

	[SerializeField]
	[HideInInspector]
	private float _maxValue = 1f;

	[SerializeField]
	[HideInInspector]
	private Texture2D _overlay;

	[SerializeField]
	[HideInInspector]
	private Vector4 _overlay1ST = new Vector4(1f, 1f, 0f, 0f);

	[SerializeField]
	[HideInInspector]
	private Vector4 _overlay2ST = new Vector4(1f, 1f, 0f, 0f);

	[SerializeField]
	[HideInInspector]
	private Vector2 _overlayShift = new Vector2(0.3f, 0.07f);

	[SerializeField]
	[HideInInspector]
	private Vector2 _overlayAlpha = new Vector2(0.5f, 0.25f);

	private Material _material;

	private float _value = 0.75f;

	private DrivenRectTransformTracker _tracker;

	public bool roundCorners
	{
		get
		{
			return _roundCorners;
		}
		set
		{
			if (_roundCorners != value)
			{
				_roundCorners = value;
				SetKeyword("ROUND_CORNERS", _roundCorners);
			}
		}
	}

	public bool border
	{
		get
		{
			return _border;
		}
		set
		{
			if (_border != value)
			{
				_border = value;
				SetKeyword("BORDER", _border);
			}
		}
	}

	public Color borderColor
	{
		get
		{
			return _borderColor;
		}
		set
		{
			if (_borderColor.r != value.r || _borderColor.g != value.g || _borderColor.b != value.b || _borderColor.a != value.a)
			{
				_borderColor = value;
				UpdateMaterialBorderColor();
			}
		}
	}

	public float edgeWidth
	{
		get
		{
			return _edgeWidth;
		}
		set
		{
			if (_edgeWidth != value)
			{
				_edgeWidth = value;
				UpdateMaterialEdgeWidth();
			}
		}
	}

	public float borderWidth
	{
		get
		{
			return _borderWidth;
		}
		set
		{
			if (_borderWidth != value)
			{
				_borderWidth = value;
				UpdateMaterialBorderWidth();
			}
		}
	}

	public float size
	{
		get
		{
			return _size;
		}
		set
		{
			if (value < _width)
			{
				value = _width;
			}
			if (_size != value)
			{
				_size = value;
				SetSizeDirty();
				UpdateMaterialWidth();
			}
		}
	}

	public float width
	{
		get
		{
			return _width;
		}
		set
		{
			if (value < 0f)
			{
				value = 0f;
			}
			if (_width != value)
			{
				_width = value;
				SetSizeDirty();
				UpdateMaterialWidth();
			}
		}
	}

	public float value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				UpdateMaterialValue();
			}
		}
	}

	public float maxValue
	{
		get
		{
			return _maxValue;
		}
		set
		{
			if (_maxValue != value)
			{
				_maxValue = value;
				UpdateMaterialValue();
			}
		}
	}

	public Texture2D texture
	{
		get
		{
			if (!(_texture != null))
			{
				return Graphic.s_WhiteTexture;
			}
			return _texture;
		}
		set
		{
			if (_texture != value)
			{
				_texture = value;
				SetMaterialDirty();
			}
		}
	}

	public Texture2D overlay
	{
		get
		{
			return _overlay;
		}
		set
		{
			if (_overlay != value)
			{
				_overlay = value;
				UpdateMaterialOverlayTexture();
			}
		}
	}

	public Vector4 overlay1ST
	{
		get
		{
			return _overlay1ST;
		}
		set
		{
			if (_overlay1ST != value)
			{
				_overlay1ST = value;
				UpdateMaterialOverlay1ST();
			}
		}
	}

	public Vector4 overlay2ST
	{
		get
		{
			return _overlay2ST;
		}
		set
		{
			if (_overlay2ST != value)
			{
				_overlay2ST = value;
				UpdateMaterialOverlay2ST();
			}
		}
	}

	public float overlay1Shift
	{
		get
		{
			return _overlayShift.x;
		}
		set
		{
			value = Mathf.Clamp(value, -1f, 1f);
			if (_overlayShift.x != value)
			{
				_overlayShift.x = value;
				UpdateMaterialOverlayShift();
			}
		}
	}

	public float overlay2Shift
	{
		get
		{
			return _overlayShift.y;
		}
		set
		{
			value = Mathf.Clamp(value, -1f, 1f);
			if (_overlayShift.y != value)
			{
				_overlayShift.y = value;
				UpdateMaterialOverlayShift();
			}
		}
	}

	public float overlay1Alpha
	{
		get
		{
			return _overlayAlpha.x;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (_overlayAlpha.x != value)
			{
				_overlayAlpha.x = value;
				UpdateMaterialOverlayAlpha();
			}
		}
	}

	public float overlay2Alpha
	{
		get
		{
			return _overlayAlpha.y;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (_overlayAlpha.y != value)
			{
				_overlayAlpha.y = value;
				UpdateMaterialOverlayAlpha();
			}
		}
	}

	public override Texture mainTexture => texture;

	public override Material materialForRendering
	{
		get
		{
			if (_material == null)
			{
				Shader uICircularBar = ShaderManager.preloadedShaders.UICircularBar;
				_material = new Material(uICircularBar);
				SetKeyword("BORDER", _border);
				SetKeyword("ROUND_CORNERS", _roundCorners);
				UpdateMaterialAll();
			}
			return _material;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetSizeDirty();
	}

	protected override void OnDisable()
	{
		_tracker.Clear();
		LayoutRebuilder.MarkLayoutForRebuild(base.rectTransform);
		base.OnDisable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		SetSizeDirty();
	}

	protected override void UpdateMaterial()
	{
		base.UpdateMaterial();
		UpdateMaterialAll();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_material != null)
		{
			Object.Destroy(_material);
		}
	}

	private void UpdateMaterialAll()
	{
		UpdateMaterialWidth();
		UpdateMaterialTexture();
		UpdateMaterialEdgeWidth();
		UpdateMaterialBorderColor();
		UpdateMaterialBorderWidth();
		UpdateMaterialValue();
		UpdateMaterialOverlayTexture();
		UpdateMaterialOverlay1ST();
		UpdateMaterialOverlay2ST();
		UpdateMaterialOverlayShift();
		UpdateMaterialOverlayAlpha();
	}

	private void UpdateMaterialWidth()
	{
		if (!(base.canvas == null))
		{
			float num = 1f / base.canvas.referencePixelsPerUnit;
			float num2 = _size * num;
			float num3 = _width * num;
			num3 /= num2;
			materialForRendering.SetFloat(ShaderPropertyID._Width, num3);
		}
	}

	private void UpdateMaterialTexture()
	{
		materialForRendering.SetTexture(ShaderPropertyID._MainTex, mainTexture);
	}

	private void UpdateMaterialOverlayTexture()
	{
		materialForRendering.SetTexture(ShaderPropertyID._OverlayTex, _overlay);
		SetKeyword("OVERLAY", _overlay != null);
	}

	private void UpdateMaterialOverlay1ST()
	{
		materialForRendering.SetVector(ShaderPropertyID._Overlay1_ST, _overlay1ST);
	}

	private void UpdateMaterialOverlay2ST()
	{
		materialForRendering.SetVector(ShaderPropertyID._Overlay2_ST, _overlay2ST);
	}

	private void UpdateMaterialOverlayShift()
	{
		materialForRendering.SetVector(ShaderPropertyID._OverlayShift, _overlayShift);
	}

	private void UpdateMaterialOverlayAlpha()
	{
		materialForRendering.SetVector(ShaderPropertyID._OverlayAlpha, _overlayAlpha);
	}

	private void UpdateMaterialEdgeWidth()
	{
		materialForRendering.SetFloat(ShaderPropertyID._EdgeWidth, Mathf.Clamp01(_edgeWidth));
	}

	private void UpdateMaterialBorderColor()
	{
		materialForRendering.SetColor(ShaderPropertyID._BorderColor, _borderColor);
	}

	private void UpdateMaterialBorderWidth()
	{
		materialForRendering.SetFloat(ShaderPropertyID._BorderWidth, Mathf.Clamp01(_borderWidth));
	}

	private void UpdateMaterialValue()
	{
		materialForRendering.SetFloat(ShaderPropertyID._Value, Mathf.Clamp01(value * maxValue));
	}

	private void SetKeyword(string keyword, bool value)
	{
		if (value)
		{
			materialForRendering.EnableKeyword(keyword);
		}
		else
		{
			materialForRendering.DisableKeyword(keyword);
		}
	}

	private void HandleLayoutAlongAxis(int axis)
	{
		_tracker.Add(this, base.rectTransform, DrivenTransformProperties.SizeDelta);
		base.rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, size);
	}

	private void SetSizeDirty()
	{
		if (IsActive())
		{
			LayoutRebuilder.MarkLayoutForRebuild(base.rectTransform);
		}
	}

	public void SetLayoutHorizontal()
	{
		_tracker.Clear();
		HandleLayoutAlongAxis(0);
	}

	public void SetLayoutVertical()
	{
		HandleLayoutAlongAxis(1);
	}
}
