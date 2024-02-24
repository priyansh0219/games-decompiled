namespace UWE
{
	public static class ProfilingTimer
	{
		public static void Begin(string label)
		{
			Timer.Begin(label);
		}

		public static float End()
		{
			return Timer.End();
		}
	}
}
