using System;
using UWE;
using UnityEngine;

public class LightAnimator : MonoBehaviour
{
	public enum Type
	{
		Flicker = 0,
		Pulsate = 1,
		Blink = 2,
		Curve = 3
	}

	[Serializable]
	public class FlickerParameters
	{
		public float minIntensity;

		public float maxIntensity = 10f;

		public float minTime;

		public float maxTime = 0.03f;
	}

	[Serializable]
	public class PulsateParameters
	{
		public float frequency;
	}

	[Serializable]
	public class CurveParameters
	{
		public float frequency;

		public AnimationCurve anim;
	}

	public Type type;

	private float waittime;

	public FlickerParameters flicker;

	public PulsateParameters pulsate;

	public CurveParameters curve;

	private float origIntensity;

	private Light lightComponent;

	private float startTime;

	private void Awake()
	{
		lightComponent = GetComponent<Light>();
		if (lightComponent != null)
		{
			origIntensity = lightComponent.intensity;
		}
		if (curve != null)
		{
			curve.anim.postWrapMode = WrapMode.Loop;
		}
	}

	private void Start()
	{
		startTime = Time.time;
	}

	private void Update()
	{
		if (!(lightComponent != null))
		{
			return;
		}
		float intensity = lightComponent.intensity;
		switch (type)
		{
		case Type.Flicker:
			if (waittime < 0f)
			{
				waittime = UnityEngine.Random.Range(flicker.minTime, flicker.maxTime);
				intensity = Mathf.SmoothStep(flicker.minIntensity, flicker.maxIntensity, UnityEngine.Random.value * origIntensity);
			}
			waittime -= Time.deltaTime;
			break;
		case Type.Pulsate:
			intensity = UWE.Utils.Unlerp(Mathf.Sin((float)System.Math.PI * 2f * pulsate.frequency * Time.time), -1f, 1f) * origIntensity;
			break;
		case Type.Curve:
			intensity = curve.anim.Evaluate(curve.frequency * (Time.time - startTime)) * origIntensity;
			break;
		}
		FlashingLightHelpers.SafeIntensityChangePerFrame(lightComponent, intensity);
	}

	public void DefaultIntensity()
	{
		lightComponent.intensity = origIntensity;
	}
}
