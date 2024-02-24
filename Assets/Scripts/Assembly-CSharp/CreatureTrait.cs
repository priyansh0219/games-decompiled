using System;
using UnityEngine;

[Serializable]
public class CreatureTrait
{
	public float Value;

	public float Falloff;

	public CreatureTrait(float value, float falloff = 0f)
	{
		Value = value;
		Falloff = falloff;
	}

	public void UpdateTrait(float timePassed)
	{
		Value = Mathf.Clamp01(Value - timePassed * Falloff);
	}

	public void Add(float amount)
	{
		Value = Mathf.Clamp01(Value + amount);
	}
}
