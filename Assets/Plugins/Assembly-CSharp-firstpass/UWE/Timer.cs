namespace UWE
{
	public static class Timer
	{
		private static TimerStack main = new TimerStack();

		public static void Begin(string label)
		{
			main.Begin(label, 0f);
		}

		public static void Begin(string label, float minLogMS)
		{
			main.Begin(label, minLogMS);
		}

		public static float End()
		{
			return main.End();
		}
	}
}
