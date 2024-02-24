using UnityEngine;

public class SunlightController : FadeLightController
{
	private float lerpRate = 0.5f;

	private float lerpFraction;

	private bool lerpDone;

	public override void Initialize(float fade)
	{
	}

	public override void Update()
	{
	}

	public void FadeTo(float fade, float rate, Color color, float fraction)
	{
	}
}
