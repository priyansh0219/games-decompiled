using System;
using TMPro;
using UnityEngine;

public class uGUI_EscapePod : MonoBehaviour
{
	public static uGUI_EscapePod main;

	public TextMeshProUGUI header;

	public TextMeshProUGUI content;

	public TextMeshProUGUI power;

	private CanvasRenderer headerCanvasRenderer;

	private float headerBlinkFreq = 4f;

	private void Awake()
	{
		if (main != null)
		{
			Debug.LogError("Duplicate uGUI_EscapePod found!");
			UnityEngine.Object.Destroy(this);
			return;
		}
		main = this;
		headerCanvasRenderer = header.canvasRenderer;
		main.SetHeader(Language.main.Get("IntroEscapePod4Header"), new Color32(159, 243, 63, byte.MaxValue));
		main.SetContent(Language.main.Get("IntroEscapePod4Content"), new Color32(159, 243, 63, byte.MaxValue));
		main.SetPower(Language.main.Get("IntroEscapePod4Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
	}

	private void Update()
	{
		if (headerBlinkFreq > 0f)
		{
			float headerAlpha = Mathf.Lerp(0f, 1f, Mathf.Sin(headerBlinkFreq * (float)Math.PI * Time.time) * 0.5f + 0.5f);
			SetHeaderAlpha(headerAlpha);
		}
	}

	public void SetHeader(string text, Color color, float blinkFreq = -1f)
	{
		header.text = text;
		header.color = color;
		headerBlinkFreq = blinkFreq;
		if (headerBlinkFreq < 0f)
		{
			SetHeaderAlpha(1f);
		}
	}

	public void SetContent(string text, Color color)
	{
		content.text = text;
		content.color = color;
	}

	public void SetPower(string text, Color color)
	{
		power.text = text;
		power.color = color;
	}

	private void SetHeaderAlpha(float alpha)
	{
		if (!(headerCanvasRenderer == null))
		{
			headerCanvasRenderer.SetAlpha(alpha);
		}
	}
}
