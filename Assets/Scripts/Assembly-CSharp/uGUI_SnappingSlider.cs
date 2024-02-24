using UnityEngine;
using UnityEngine.UI;

public class uGUI_SnappingSlider : Slider, uGUI_INavigableControl
{
	private const ManagedUpdate.Queue queueLayoutComplete = ManagedUpdate.Queue.UILayoutComplete;

	public RectTransform defaultValueRect;

	public float _defaultValue = 0.5f;

	public float step = 1f;

	private bool ready;

	private static Vector3[] fourCorners = new Vector3[4];

	public float defaultValue
	{
		get
		{
			return _defaultValue;
		}
		set
		{
			_defaultValue = value;
			UpdateDefaultValueRect();
		}
	}

	public float unsnappedValue
	{
		get
		{
			return value;
		}
		set
		{
			base.Set(value);
		}
	}

	public float normalizedUnsnappedValue
	{
		get
		{
			return base.normalizedValue;
		}
		set
		{
			base.Set(Mathf.Lerp(base.minValue, base.maxValue, value));
		}
	}

	public void OnLayoutComplete()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
		ready = true;
	}

	protected override void Start()
	{
		base.Start();
		UpdateDefaultValueRect();
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
	}

	private float SnapValue(float value)
	{
		if (!ready)
		{
			return value;
		}
		RectTransform component = GetComponent<RectTransform>();
		component.GetWorldCorners(fourCorners);
		Camera uICamera = ManagedCanvasUpdate.GetUICamera();
		Vector3 vector = uICamera.WorldToScreenPoint(fourCorners[0]);
		float num = Mathf.Abs(uICamera.WorldToScreenPoint(fourCorners[2]).x - vector.x);
		if (Mathf.Approximately(num, 0f))
		{
			return value;
		}
		float num2 = 20f / uGUI_CanvasScaler.GetInverseScale(component);
		float num3 = (base.maxValue - base.minValue) * num2 / num;
		value = ((!(Mathf.Abs(value - defaultValue) < num3)) ? SnapToStep(value) : defaultValue);
		return value;
	}

	private float SnapToStep(float value)
	{
		int num = Mathf.RoundToInt(value / step);
		if ((double)(value - (float)num * step) >= 0.5 * (double)step)
		{
			num++;
		}
		return Mathf.Clamp((float)num * step, base.minValue, base.maxValue);
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		UpdateDefaultValueRect();
	}

	private void UpdateDefaultValueRect()
	{
		if (defaultValueRect != null)
		{
			bool flag = base.direction == Direction.RightToLeft || base.direction == Direction.TopToBottom;
			int index = ((base.direction != 0 && base.direction != Direction.RightToLeft) ? 1 : 0);
			float num = (defaultValue - base.minValue) / (base.maxValue - base.minValue);
			Vector2 zero = Vector2.zero;
			Vector2 one = Vector2.one;
			float num3 = (one[index] = (flag ? (1f - num) : num));
			zero[index] = num3;
			defaultValueRect.anchorMin = zero;
			defaultValueRect.anchorMax = one;
		}
	}

	void uGUI_INavigableControl.OnMove(int dirX, int dirY)
	{
		if (dirX > 0)
		{
			unsnappedValue = SnapToStep(unsnappedValue + step);
		}
		else if (dirX < 0)
		{
			unsnappedValue = SnapToStep(unsnappedValue - step);
		}
	}

	protected override void Set(float input, bool sendCallback = true)
	{
		base.Set(SnapValue(input), sendCallback);
	}
}
