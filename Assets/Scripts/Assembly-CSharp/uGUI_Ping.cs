using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class uGUI_Ping : MonoBehaviour
{
	[AssertNotNull]
	public uGUI_Icon icon;

	[AssertNotNull]
	public TextMeshProUGUI infoText;

	[AssertNotNull]
	public TextMeshProUGUI distanceText;

	[AssertNotNull]
	public TextMeshProUGUI suffixText;

	[AssertNotNull]
	public Graphic arrow;

	private RectTransform _rectTransform;

	private string _label;

	private int _distance = int.MinValue;

	private float _scale = -1f;

	private Material _iconMaterial;

	private bool _initialized;

	private Color _iconColor = Color.white;

	private Color _textColor = Color.white;

	[AssertLocalization]
	private const string meterSuffixKey = "MeterSuffix";

	public RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	private void Awake()
	{
		_iconMaterial = new Material(icon.material);
		icon.material = _iconMaterial;
		arrow.material = _iconMaterial;
	}

	public void Initialize()
	{
		_initialized = true;
	}

	public void Uninitialize()
	{
		_initialized = false;
		UpdateIconColor();
		UpdateTextColor();
	}

	public void SetColor(Color color)
	{
		_iconColor.r = color.r;
		_iconColor.g = color.g;
		_iconColor.b = color.b;
		_textColor.r = color.r;
		_textColor.g = color.g;
		_textColor.b = color.b;
		UpdateIconColor();
		UpdateTextColor();
	}

	public void SetIconAlpha(float alpha)
	{
		if (!Mathf.Approximately(_iconColor.a, alpha))
		{
			_iconColor.a = alpha;
			UpdateIconColor();
		}
	}

	public void SetTextAlpha(float alpha)
	{
		if (!Mathf.Approximately(_textColor.a, alpha))
		{
			_textColor.a = alpha;
			UpdateTextColor();
		}
	}

	public float GetTextAlpha()
	{
		return _textColor.a;
	}

	public float GetScale()
	{
		return _scale;
	}

	public void SetScale(float value)
	{
		if (!Mathf.Approximately(_scale, value))
		{
			_scale = value;
			rectTransform.localScale = new Vector3(_scale, _scale, 1f);
			infoText.SetScaleDirty();
			distanceText.SetScaleDirty();
			suffixText.SetScaleDirty();
		}
	}

	public void SetIcon(Sprite sprite)
	{
		icon.sprite = sprite;
	}

	public void SetLabel(string value)
	{
		_label = ((value != null) ? value : string.Empty);
		UpdateText();
	}

	public void SetDistance(float distance)
	{
		int num = Mathf.RoundToInt(distance);
		if (_distance != num)
		{
			_distance = num;
			UpdateText();
		}
	}

	public void SetVisible(bool visible)
	{
		if (!visible)
		{
			_iconColor.a = 0f;
			_textColor.a = 0f;
		}
		base.gameObject.SetActive(visible);
		UpdateIconColor();
		UpdateTextColor();
	}

	public void SetAngle(float angle)
	{
		if (angle < 0f)
		{
			arrow.enabled = false;
			return;
		}
		arrow.enabled = true;
		arrow.rectTransform.rotation = Quaternion.Euler(0f, 0f, angle);
	}

	private void UpdateText()
	{
		string stringForInt = IntStringCache.GetStringForInt(_distance);
		if (string.IsNullOrEmpty(_label))
		{
			infoText.text = string.Empty;
			distanceText.text = stringForInt;
			suffixText.text = Language.main.Get("MeterSuffix");
		}
		else
		{
			infoText.text = _label;
			distanceText.text = stringForInt;
			suffixText.text = Language.main.Get("MeterSuffix");
		}
	}

	private void UpdateIconColor()
	{
		Color iconColor = _iconColor;
		iconColor.a = (_initialized ? iconColor.a : 0f);
		_iconMaterial.SetColor(ShaderPropertyID._Color, iconColor);
	}

	private void UpdateTextColor()
	{
		Color textColor = _textColor;
		textColor.a = (_initialized ? textColor.a : 0f);
		infoText.color = textColor;
		distanceText.color = textColor;
		suffixText.color = textColor;
	}
}
