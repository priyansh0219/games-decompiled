using UnityEngine;

public class SwimmingRippleMover : RippleMover
{
	private float lastSpeedFactor;

	public override float GetRippleAmount()
	{
		float num = (lastSpeedFactor = Mathf.MoveTowards(lastSpeedFactor, Player.main.IsSwimming() ? 1f : 0f, 2f * Time.deltaTime));
		return kRippleAmount * num;
	}
}
