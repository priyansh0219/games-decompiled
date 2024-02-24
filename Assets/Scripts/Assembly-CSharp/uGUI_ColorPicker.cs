using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class uGUI_ColorPicker : Selectable, IPointerDownHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, uGUI_IAdjustReceiver, ICompileTimeCheckable
{
	[AssertNotNull]
	public RawImage colorSpectrum;

	[AssertNotNull]
	public RawImage colorGrayscale;

	public RawImage selectedColor;

	[AssertNotNull]
	public RectTransform pointer;

	[AssertNotNull]
	public Slider saturationSlider;

	public ColorChangeEvent onColorChange = new ColorChangeEvent();

	[InputButtonBoolArray]
	[SerializeField]
	private bool[] acceptedMouseButtons = new bool[3] { true, true, true };

	private static Texture2D textureSpectrum;

	private static Texture2D textureGrayscale;

	private static bool initialized = false;

	private RectTransform rt;

	private float _hue;

	private float _saturation = 1f;

	private float _brightness;

	private Color _currentColor = Color.clear;

	private const int period = 1530;

	private static readonly int[] colorComponentOffsets = new int[3] { 1020, 0, 510 };

	private static float[] colorComponents = new float[3];

	private static Color colorResult = new Color(0f, 0f, 0f, 1f);

	public Color currentColor => _currentColor;

	protected override void Awake()
	{
		base.Awake();
		Initialize();
		rt = colorSpectrum.GetComponent<RectTransform>();
		colorSpectrum.texture = textureSpectrum;
		colorGrayscale.texture = textureGrayscale;
		UpdateAll();
	}

	private static void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			textureSpectrum = GetSpectrumTexture(256);
			textureGrayscale = GetGrayscaleTexture(256);
		}
	}

	public void SetHSB(Vector3 hsb)
	{
		Initialize();
		_hue = Mathf.Clamp01(hsb.x);
		_saturation = Mathf.Clamp01(hsb.y);
		_brightness = Mathf.Clamp01(hsb.z);
		saturationSlider.normalizedValue = _saturation;
		UpdateAll();
	}

	public Vector3 GetHSB()
	{
		return new Vector3(_hue, _saturation, _brightness);
	}

	public void SetSaturation(float s)
	{
		s = Mathf.Clamp01(s);
		if (_saturation != s)
		{
			_saturation = s;
			UpdateColor();
			UpdateIndicator();
		}
	}

	private void UpdateAll()
	{
		UpdateColor();
		UpdatePointer();
		UpdateIndicator();
	}

	private void UpdateColor()
	{
		Color color = HSBToColor(new Vector3(_hue, _saturation, _brightness));
		if (!(_currentColor == color))
		{
			_currentColor = color;
			if (selectedColor != null)
			{
				selectedColor.color = _currentColor;
			}
			NotifyColorChange();
		}
	}

	private void UpdatePointer()
	{
		Rect rect = pointer.parent.GetComponent<RectTransform>().rect;
		pointer.localPosition = Vector2.Scale(rect.size, new Vector2(_hue, _brightness)) + rect.min;
		pointer.localPosition = new Vector3(Mathf.Round(pointer.localPosition.x), Mathf.Round(pointer.localPosition.y), pointer.localPosition.z);
	}

	private void UpdateIndicator()
	{
		colorGrayscale.color = new Color(1f, 1f, 1f, 1f - _saturation);
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		if (IsAcceptedButton(eventData.button))
		{
			Vector2 point = Vector2.zero;
			if (GetPosition(eventData, ref point))
			{
				_hue = point.x;
				_brightness = point.y;
				UpdateAll();
			}
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (IsAcceptedButton(eventData.button))
		{
			Vector2 point = Vector2.zero;
			if (GetPosition(eventData, ref point))
			{
				_hue = point.x;
				_brightness = point.y;
				UpdateAll();
			}
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (IsAcceptedButton(eventData.button))
		{
			Vector2 point = Vector2.zero;
			if (GetPosition(eventData, ref point))
			{
				_hue = point.x;
				_brightness = point.y;
				UpdateAll();
			}
		}
	}

	public bool OnAdjust(Vector2 adjustDelta)
	{
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		_hue = Mathf.Clamp01(_hue + adjustDelta.x * unscaledDeltaTime);
		_brightness = Mathf.Clamp01(_brightness + adjustDelta.y * unscaledDeltaTime);
		UpdateAll();
		return true;
	}

	private bool GetPosition(PointerEventData eventData, ref Vector2 point)
	{
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.enterEventCamera, out point))
		{
			return false;
		}
		Vector2 sizeDelta = rt.sizeDelta;
		point += Vector2.Scale(rt.pivot, sizeDelta);
		point.x /= sizeDelta.x;
		point.y /= sizeDelta.y;
		point.x = Mathf.Clamp01(point.x);
		point.y = Mathf.Clamp01(point.y);
		return true;
	}

	private static Texture2D GetSpectrumTexture(int size)
	{
		int num = Mathf.NextPowerOfTwo(size);
		Texture2D texture2D = new Texture2D(num, num, TextureFormat.ARGB32, mipChain: false);
		texture2D.name = "uGUI_ColorPicker.GetSpectrumTexture";
		texture2D.filterMode = FilterMode.Bilinear;
		texture2D.wrapMode = TextureWrapMode.Clamp;
		for (int i = 0; i < num; i++)
		{
			float brightness = (float)i / (float)num;
			for (int j = 0; j < num; j++)
			{
				float hue = (float)j / (float)num;
				texture2D.SetPixel(j, i, GetColorValue(hue, brightness));
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	private static Texture2D GetGrayscaleTexture(int size)
	{
		int num = Mathf.NextPowerOfTwo(size);
		Texture2D texture2D = new Texture2D(num, num, TextureFormat.ARGB32, mipChain: false);
		texture2D.name = "uGUI_ColorPicker.GetGrayscaleTexture";
		texture2D.filterMode = FilterMode.Bilinear;
		texture2D.wrapMode = TextureWrapMode.Clamp;
		for (int i = 0; i < num; i++)
		{
			float t = (float)i / (float)num;
			for (int j = 0; j < num; j++)
			{
				texture2D.SetPixel(j, i, Color.Lerp(Color.black, Color.white, t));
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	private static Color GetColorValue(float hue, float brightness)
	{
		hue = Mathf.Clamp01(hue);
		brightness = Mathf.Clamp01(brightness);
		int num = Mathf.CeilToInt(hue * 1530f);
		for (int i = 0; i < 3; i++)
		{
			int num2 = num;
			int num3 = 0;
			num2 = (num2 - colorComponentOffsets[i]) % 1530;
			if (num2 < 0)
			{
				num2 += 1530;
			}
			if (num2 < 255)
			{
				num3 = num2;
			}
			if (num2 >= 255 && num2 < 765)
			{
				num3 = 255;
			}
			if (num2 >= 765 && num2 < 1020)
			{
				num3 = 1020 - num2;
			}
			colorComponents[i] = (float)num3 * brightness / 255f;
		}
		colorResult.r = colorComponents[0];
		colorResult.g = colorComponents[1];
		colorResult.b = colorComponents[2];
		return colorResult;
	}

	public static Color HSBToColor(Vector3 hsb)
	{
		Color colorValue = GetColorValue(hsb.x, hsb.z);
		return Color.Lerp(Color.Lerp(Color.black, Color.white, hsb.z), colorValue, hsb.y);
	}

	public static Vector3 HSBFromColor(Color color)
	{
		Vector3 result = new Vector3(0f, 0f, 0f);
		float r = color.r;
		float g = color.g;
		float b = color.b;
		float num = Mathf.Max(r, Mathf.Max(g, b));
		if (num <= 0f)
		{
			return result;
		}
		float num2 = Mathf.Min(r, Mathf.Min(g, b));
		float num3 = num - num2;
		if (num > num2)
		{
			if (g == num)
			{
				result.x = (b - r) / num3 * 60f + 120f;
			}
			else if (b == num)
			{
				result.x = (r - g) / num3 * 60f + 240f;
			}
			else if (b > g)
			{
				result.x = (g - b) / num3 * 60f + 360f;
			}
			else
			{
				result.x = (g - b) / num3 * 60f;
			}
			if (result.x < 0f)
			{
				result.x += 360f;
			}
		}
		else
		{
			result.x = 0f;
		}
		result.x *= 0.0027777778f;
		result.y = num3 / num * 1f;
		result.z = num;
		return result;
	}

	private void NotifyColorChange()
	{
		if (onColorChange != null)
		{
			ColorChangeEventData colorChangeEventData = new ColorChangeEventData(EventSystem.current);
			colorChangeEventData.color = _currentColor;
			colorChangeEventData.hsb = new Vector3(_hue, _saturation, _brightness);
			onColorChange.Invoke(colorChangeEventData);
		}
	}

	private bool IsAcceptedButton(PointerEventData.InputButton button)
	{
		return acceptedMouseButtons[(int)button];
	}

	string ICompileTimeCheckable.CompileTimeCheck()
	{
		if (pointer == null)
		{
			return "pointer cannot be null.";
		}
		if (pointer.parent == null)
		{
			return "pointer.parent cannot be null.";
		}
		if (pointer.parent.GetComponent<RectTransform>() == null)
		{
			return "pointer.parent should have RectTransform component.";
		}
		return null;
	}
}
