using UnityEngine;

namespace RadicalLibrary
{
	public static class CRSpline
	{
		public static Vector3 Interp(Vector3[] pts, float t)
		{
			int num = pts.Length - 3;
			int num2 = Mathf.Min(Mathf.FloorToInt(t * (float)num), num - 1);
			float num3 = t * (float)num - (float)num2;
			Vector3 vector = pts[num2];
			Vector3 vector2 = pts[num2 + 1];
			Vector3 vector3 = pts[num2 + 2];
			Vector3 vector4 = pts[num2 + 3];
			return 0.5f * ((-vector + 3f * vector2 - 3f * vector3 + vector4) * (num3 * num3 * num3) + (2f * vector - 5f * vector2 + 4f * vector3 - vector4) * (num3 * num3) + (-vector + vector3) * num3 + 2f * vector2);
		}

		public static Vector3 InterpConstantSpeedOld(Vector3[] pts, float t)
		{
			int num = pts.Length - 3;
			float num2 = 0f;
			float[] array = new float[pts.Length - 1];
			for (int i = 0; i < pts.Length - 1; i++)
			{
				num2 += (array[i] = (pts[i + 1] - pts[i]).magnitude);
			}
			int num3 = 1;
			float num4 = 0f;
			double num5 = 0.0;
			do
			{
				double num6 = num4 / num2;
				double num7 = (num4 + array[num3]) / num2;
				num5 = ((double)t - num6) / (num7 - num6);
				if (!double.IsNaN(num5) && !(num5 < 0.0) && !(num5 > 1.0))
				{
					break;
				}
				num4 += array[num3];
				num3++;
			}
			while (num3 < num + 1);
			num5 = Mathf.Clamp01((float)num5);
			Vector3 vector = pts[num3 - 1];
			Vector3 vector2 = pts[num3];
			Vector3 vector3 = pts[num3 + 1];
			Vector3 vector4 = pts[num3 + 2];
			return 0.5f * ((-vector + 3f * vector2 - 3f * vector3 + vector4) * (float)(num5 * num5 * num5) + (2f * vector - 5f * vector2 + 4f * vector3 - vector4) * (float)(num5 * num5) + (-vector + vector3) * (float)num5 + 2f * vector2);
		}

		public static Vector3 InterpConstantSpeed(Vector3[] pts, float t)
		{
			int num = pts.Length - 3;
			float num2 = 0f;
			float[] array = new float[pts.Length - 1];
			for (int i = 0; i < pts.Length - 1; i++)
			{
				num2 += (array[i] = (pts[i + 1] - pts[i]).magnitude);
			}
			int num3 = 1;
			float num4 = 0f;
			double num5 = 0.0;
			do
			{
				double num6 = num4 / num2;
				double num7 = (num4 + array[num3]) / num2;
				num5 = ((double)t - num6) / (num7 - num6);
				if (!double.IsNaN(num5) && !(num5 < 0.0) && !(num5 > 1.0))
				{
					break;
				}
				num4 += array[num3];
				num3++;
			}
			while (num3 < num + 1);
			num5 = Mathf.Clamp01((float)num5);
			Vector3 vector = pts[num3 - 1];
			Vector3 vector2 = pts[num3];
			Vector3 vector3 = pts[num3 + 1];
			Vector3 vector4 = pts[num3 + 2];
			return 0.5f * ((-vector + 3f * vector2 - 3f * vector3 + vector4) * (float)(num5 * num5 * num5) + (2f * vector - 5f * vector2 + 4f * vector3 - vector4) * (float)(num5 * num5) + (-vector + vector3) * (float)num5 + 2f * vector2);
		}

		public static Vector3 Velocity(Vector3[] pts, float t)
		{
			int num = pts.Length - 3;
			int num2 = Mathf.Min(Mathf.FloorToInt(t * (float)num), num - 1);
			float num3 = t * (float)num - (float)num2;
			Vector3 vector = pts[num2];
			Vector3 vector2 = pts[num2 + 1];
			Vector3 vector3 = pts[num2 + 2];
			Vector3 vector4 = pts[num2 + 3];
			return 1.5f * (-vector + 3f * vector2 - 3f * vector3 + vector4) * (num3 * num3) + (2f * vector - 5f * vector2 + 4f * vector3 - vector4) * num3 + 0.5f * vector3 - 0.5f * vector;
		}

		public static void GizmoDraw(Vector3[] pts, float t)
		{
			Gizmos.color = Color.white;
			Vector3 to = Interp(pts, 0f);
			for (int i = 1; i <= 20; i++)
			{
				float t2 = (float)i / 20f;
				Vector3 vector = Interp(pts, t2);
				Gizmos.DrawLine(vector, to);
				to = vector;
			}
			Gizmos.color = Color.blue;
			Vector3 vector2 = Interp(pts, t);
			Gizmos.DrawLine(vector2, vector2 + Velocity(pts, t));
		}
	}
}
