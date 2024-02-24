using System;
using UWE;
using UnityEngine;

public class GrowMixin : MonoBehaviour
{
	public float lifeTime;

	public float kMaturityTime = 120f;

	private float timeDisabled;

	public float debugGrowthScalar = 1f;

	[NonSerialized]
	public Event<float> growScalarChanged = new Event<float>();

	[NonSerialized]
	public Event<bool> fullyGrown = new Event<bool>();

	private void OnEnable()
	{
		if (!Utils.NearlyEqual(timeDisabled, 0f))
		{
			Grow(Time.time - timeDisabled);
		}
	}

	private void OnDisable()
	{
		timeDisabled = Time.time;
	}

	private void FixedUpdate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Grow(Time.deltaTime);
		}
	}

	private void Grow(float growTime)
	{
		float a = lifeTime;
		lifeTime += growTime * debugGrowthScalar;
		lifeTime = Mathf.Clamp(lifeTime, 0f, kMaturityTime);
		if (!Utils.NearlyEqual(a, lifeTime))
		{
			growScalarChanged.Trigger(lifeTime / kMaturityTime);
		}
		if (Utils.NearlyEqual(lifeTime, kMaturityTime))
		{
			fullyGrown.Trigger(parms: true);
			base.enabled = false;
		}
	}
}
