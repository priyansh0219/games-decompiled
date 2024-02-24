using System;

namespace UWE
{
	public static class MathHelper
	{
		public const float Pi = (float)System.Math.PI;

		public const float HalfPi = (float)System.Math.PI / 2f;

		public static float Lerp(float from, float to, float step)
		{
			return (to - from) * step + from;
		}
	}
}
