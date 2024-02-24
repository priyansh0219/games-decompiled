namespace UWE
{
	public static class Math
	{
		public static int PositiveModulo(int v, int m)
		{
			return (v % m + m) % m;
		}

		public static int NegativeModulo(int v, int m)
		{
			return (v % m - m) % m;
		}

		public static int FloorDiv(int a, int b)
		{
			return (a - (a % b + b) % b) / b;
		}

		public static int CeilDiv(int a, int b)
		{
			return (a - (a % b - b) % b) / b;
		}

		public static float Sanitize(float v)
		{
			return Sanitize(v * 10f, 0.05f) / 10f;
		}

		public static float Sanitize(float v, float epsilon)
		{
			int num = (int)v;
			float num2 = v - (float)num;
			if (num2 > 0f - epsilon && num2 < epsilon)
			{
				return num;
			}
			if (num2 > 1f - epsilon)
			{
				return num + 1;
			}
			if (num2 < epsilon - 1f)
			{
				return num - 1;
			}
			return v;
		}
	}
}
