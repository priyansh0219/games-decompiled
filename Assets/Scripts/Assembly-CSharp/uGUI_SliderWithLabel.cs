using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_SliderWithLabel : MonoBehaviour
{
	public Slider slider;

	public TextMeshProUGUI label;

	public float defaultValue = 0.5f;

	public SliderLabelMode mode;

	public string floatFormat = "0.0";

	public Func<float, string> formatDelegate;

	private void Awake()
	{
		slider.onValueChanged.AddListener(OnValueChanged);
		UpdateLabel();
	}

	private void OnValueChanged(float value)
	{
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		switch (mode)
		{
		case SliderLabelMode.Percent:
			label.text = IntStringCache.GetStringForInt(Mathf.RoundToInt(slider.value * 100f));
			break;
		case SliderLabelMode.Int:
			label.text = IntStringCache.GetStringForInt(Mathf.RoundToInt(slider.value));
			break;
		case SliderLabelMode.Float:
			label.text = slider.value.ToString(floatFormat);
			break;
		case SliderLabelMode.Delegate:
			label.text = formatDelegate(slider.value);
			break;
		}
	}
}
