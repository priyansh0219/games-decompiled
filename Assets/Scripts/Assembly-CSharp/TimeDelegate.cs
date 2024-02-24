using UnityEngine;

public class TimeDelegate
{
	public delegate float TimeDelegateFunction();

	private TimeDelegateFunction timeDelegate;

	public float GetTime()
	{
		float result = ((DayNightCycle.main != null) ? DayNightCycle.main.timePassedAsFloat : Time.time);
		if (timeDelegate != null)
		{
			result = timeDelegate();
		}
		return result;
	}

	public void SetTimeDelegate(TimeDelegateFunction tdf)
	{
		timeDelegate = tdf;
	}
}
