using System;
using UnityEngine;

public class uGUI_TextFade : uGUI_Text
{
	protected Sequence sequence;

	protected override void Awake()
	{
		base.Awake();
		sequence = new Sequence();
	}

	private void Update()
	{
		if (sequence.active)
		{
			sequence.Update();
			float t = sequence.t;
			bool flag = t != 0f;
			if (text.enabled != flag)
			{
				text.enabled = flag;
			}
			float alpha = 0.5f * (1f - Mathf.Cos((float)Math.PI * t));
			SetAlpha(alpha);
		}
	}

	public void FadeIn(float duration, SequenceCallback callback)
	{
		sequence.Set(duration, target: true, callback);
	}

	public void FadeOut(float duration, SequenceCallback callback)
	{
		sequence.Set(duration, target: false, callback);
	}

	public void StopFade()
	{
		sequence.Reset();
	}

	public void SetState(bool enabled)
	{
		text.enabled = enabled;
		sequence.ForceState(enabled);
	}

	protected void SetAlpha(float a)
	{
		Color color = text.color;
		text.color = new Color(color.r, color.g, color.b, a);
	}
}
