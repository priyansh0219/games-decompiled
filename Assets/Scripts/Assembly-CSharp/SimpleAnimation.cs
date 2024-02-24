using System;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SimpleAnimation : MonoBehaviour
{
	public bool useUnscaledTime;

	private float defaultAlpha = 1f;

	private Quaternion defaultLocalRotation = Quaternion.identity;

	private Vector3 defaultLocalScale = Vector3.one;

	public bool rotation;

	public Vector3 rotationSpeed;

	public bool pulse;

	public float pulseFrequency = 1f;

	[Range(0f, 1f)]
	public float pulseMin = 0.5f;

	[Range(0f, 1f)]
	public float pulseMax = 1f;

	public bool ping;

	public float pingSpeed = 1f;

	public AnimationCurve pingScale = new AnimationCurve();

	public AnimationCurve pingAlpha = new AnimationCurve();

	private RectTransform rt;

	private CanvasRenderer cr;

	private CanvasGroup cg;

	private void Awake()
	{
		rt = GetComponent<RectTransform>();
		cr = GetComponent<CanvasRenderer>();
		cg = GetComponent<CanvasGroup>();
		defaultLocalRotation = rt.localRotation;
		defaultLocalScale = rt.localScale;
		if (cg != null)
		{
			defaultAlpha = cg.alpha;
		}
		else if (cr != null)
		{
			defaultAlpha = cr.GetAlpha();
		}
	}

	private void Update()
	{
		float num = (useUnscaledTime ? Time.unscaledTime : Time.time);
		float num2 = (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
		if (rotation)
		{
			rt.Rotate(rotationSpeed * num2);
		}
		if (pulse && (cr != null || cg != null))
		{
			float alpha = Mathf.Lerp(pulseMin, pulseMax, Mathf.Sin(pulseFrequency * (float)Math.PI * num) * 0.5f + 0.5f);
			if (cg != null)
			{
				cg.alpha = alpha;
			}
			else
			{
				cr.SetAlpha(alpha);
			}
		}
		if (ping && (cr != null || cg != null))
		{
			float time = num * pingSpeed % 1f;
			float num3 = pingScale.Evaluate(time);
			float alpha2 = pingAlpha.Evaluate(time);
			rt.localScale = new Vector3(num3, num3, defaultLocalScale.z);
			if (cg != null)
			{
				cg.alpha = alpha2;
			}
			else
			{
				cr.SetAlpha(alpha2);
			}
		}
	}

	public void Off()
	{
		rotation = false;
		pulse = false;
		ping = false;
		rt.localRotation = defaultLocalRotation;
		rt.localScale = defaultLocalScale;
		if (cg != null)
		{
			cg.alpha = defaultAlpha;
		}
		else if (cr != null)
		{
			cr.SetAlpha(defaultAlpha);
		}
	}
}
