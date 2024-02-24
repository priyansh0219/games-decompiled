using System.Diagnostics;

public static class DebugDisplayTimer
{
	public static long Start()
	{
		return Stopwatch.GetTimestamp();
	}

	[Conditional("ENABLE_DISPLAY_TIMER")]
	public static void End(long startTime, string messageFormat, object messageArg1)
	{
	}

	[Conditional("ENABLE_DISPLAY_TIMER")]
	public static void End(long startTime, string messageFormat, params object[] messageArgs)
	{
	}

	[Conditional("ENABLE_DISPLAY_TIMER")]
	public static void End(long startTime, float minTime, string messageFormat, params object[] messageArgs)
	{
		float num = (float)((double)(Stopwatch.GetTimestamp() - startTime) / (double)Stopwatch.Frequency);
		if (num >= minTime)
		{
			DebugDisplay.AddLine($"{string.Format(messageFormat, messageArgs)} {num * 1000f:0.00} ms");
		}
	}
}
