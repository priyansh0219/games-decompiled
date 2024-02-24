using System;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_Overlays : MonoBehaviour
{
	[Serializable]
	public class Overlay
	{
		public Graphic graphic;

		public AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
	}

	public const int OutOfOxygen = 0;

	public Overlay[] overlays;

	public void Awake()
	{
		int i = 0;
		for (int num = overlays.Length; i < num; i++)
		{
			Overlay overlay = overlays[i];
			SetAlpha(overlay, 0f);
		}
	}

	public void Set(int id, float value)
	{
		if (id >= 0 && id < overlays.Length)
		{
			SetAlpha(overlays[id], value);
		}
	}

	private void SetAlpha(Overlay overlay, float value)
	{
		if (overlay == null)
		{
			return;
		}
		Graphic graphic = overlay.graphic;
		if (!(graphic == null))
		{
			float num = Mathf.Clamp01(overlay.curve.Evaluate(value));
			CanvasRenderer canvasRenderer = graphic.canvasRenderer;
			if (num > 0f)
			{
				graphic.enabled = true;
				canvasRenderer.SetAlpha(num);
			}
			else
			{
				graphic.enabled = false;
			}
		}
	}
}
