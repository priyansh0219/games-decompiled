using System;
using UnityEngine;

namespace UWE
{
	public static class Easing
	{
		private static class Sine
		{
			public static float EaseIn(float s)
			{
				return Mathf.Sin(s * ((float)System.Math.PI / 2f) - (float)System.Math.PI / 2f) + 1f;
			}

			public static float EaseOut(float s)
			{
				return Mathf.Sin(s * ((float)System.Math.PI / 2f));
			}

			public static float EaseInOut(float s)
			{
				return (Mathf.Sin(s * (float)System.Math.PI - (float)System.Math.PI / 2f) + 1f) / 2f;
			}
		}

		private static class Power
		{
			public static float EaseIn(float s, int power)
			{
				return Mathf.Pow(s, power);
			}

			public static float EaseOut(float s, int power)
			{
				int num = ((power % 2 != 0) ? 1 : (-1));
				return (float)num * (Mathf.Pow(s - 1f, power) + (float)num);
			}

			public static float EaseInOut(float s, int power)
			{
				s *= 2f;
				if (s < 1f)
				{
					return EaseIn(s, power) / 2f;
				}
				int num = ((power % 2 != 0) ? 1 : (-1));
				return (float)num / 2f * (Mathf.Pow(s - 2f, power) + (float)(num * 2));
			}
		}

		public static float Ease(float linearStep, float acceleration, EasingType type)
		{
			float to = ((acceleration > 0f) ? EaseIn(linearStep, type) : ((acceleration < 0f) ? EaseOut(linearStep, type) : linearStep));
			return MathHelper.Lerp(linearStep, to, Mathf.Abs(acceleration));
		}

		public static float EaseIn(float linearStep, EasingType type)
		{
			switch (type)
			{
			case EasingType.Step:
				return (!((double)linearStep < 0.5)) ? 1 : 0;
			case EasingType.Linear:
				return linearStep;
			case EasingType.Sine:
				return Sine.EaseIn(linearStep);
			case EasingType.Quadratic:
				return Power.EaseIn(linearStep, 2);
			case EasingType.Cubic:
				return Power.EaseIn(linearStep, 3);
			case EasingType.Quartic:
				return Power.EaseIn(linearStep, 4);
			case EasingType.Quintic:
				return Power.EaseIn(linearStep, 5);
			default:
				throw new NotImplementedException();
			}
		}

		public static float EaseOut(float linearStep, EasingType type)
		{
			switch (type)
			{
			case EasingType.Step:
				return (!((double)linearStep < 0.5)) ? 1 : 0;
			case EasingType.Linear:
				return linearStep;
			case EasingType.Sine:
				return Sine.EaseOut(linearStep);
			case EasingType.Quadratic:
				return Power.EaseOut(linearStep, 2);
			case EasingType.Cubic:
				return Power.EaseOut(linearStep, 3);
			case EasingType.Quartic:
				return Power.EaseOut(linearStep, 4);
			case EasingType.Quintic:
				return Power.EaseOut(linearStep, 5);
			default:
				throw new NotImplementedException();
			}
		}

		public static float EaseInOut(float linearStep, EasingType easeInType, EasingType easeOutType)
		{
			if (!((double)linearStep < 0.5))
			{
				return EaseInOut(linearStep, easeOutType);
			}
			return EaseInOut(linearStep, easeInType);
		}

		public static float EaseInOut(float linearStep, EasingType type)
		{
			switch (type)
			{
			case EasingType.Step:
				return (!((double)linearStep < 0.5)) ? 1 : 0;
			case EasingType.Linear:
				return linearStep;
			case EasingType.Sine:
				return Sine.EaseInOut(linearStep);
			case EasingType.Quadratic:
				return Power.EaseInOut(linearStep, 2);
			case EasingType.Cubic:
				return Power.EaseInOut(linearStep, 3);
			case EasingType.Quartic:
				return Power.EaseInOut(linearStep, 4);
			case EasingType.Quintic:
				return Power.EaseInOut(linearStep, 5);
			default:
				throw new NotImplementedException();
			}
		}
	}
}
