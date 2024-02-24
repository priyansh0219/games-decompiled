using UnityEngine;

public static class DayNightUtils
{
	public static float time
	{
		get
		{
			DayNightCycle main = DayNightCycle.main;
			if (!(main != null))
			{
				return 0f;
			}
			return (float)main.timePassed;
		}
	}

	public static float dayScalar
	{
		get
		{
			DayNightCycle main = DayNightCycle.main;
			if (!(main != null))
			{
				return 0.5f;
			}
			return main.GetDayScalar();
		}
	}

	public static float Evaluate(float baseValue, AnimationCurve multiplier)
	{
		if (multiplier == null || multiplier.length == 0)
		{
			return baseValue;
		}
		return baseValue * multiplier.Evaluate(dayScalar);
	}
}
