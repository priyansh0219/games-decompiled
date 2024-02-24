using UnityEngine;

namespace RadicalLibrary
{
	public static class CubicBez
	{
		public static Vector3 Interp(Vector3 st, Vector3 en, Vector3 ctrl1, Vector3 ctrl2, float t)
		{
			float num = 1f - t;
			return num * num * num * st + 3f * num * num * t * ctrl1 + 3f * num * t * t * ctrl2 + t * t * t * en;
		}

		public static Vector3 Velocity(Vector3 st, Vector3 en, Vector3 ctrl1, Vector3 ctrl2, float t)
		{
			float num = 1f - t;
			return 3f * num * num * (ctrl1 - st) + 6f * num * t * (ctrl2 - ctrl1) + 3f * t * t * (en - ctrl2);
		}

		public static Vector3 Acceleration(Vector3 st, Vector3 en, Vector3 ctrl1, Vector3 ctrl2, float t)
		{
			float num = 1f - t;
			return 6f * num * (ctrl2 - 2f * ctrl1 + st) + 6f * t * (en - 2f * ctrl2 + ctrl1);
		}

		public static void GizmoDraw(Vector3 st, Vector3 en, Vector3 ctrl1, Vector3 ctrl2, float t)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(st, ctrl1);
			Gizmos.DrawLine(ctrl2, en);
			Gizmos.color = Color.white;
			Vector3 to = st;
			for (int i = 1; i <= 20; i++)
			{
				float t2 = (float)i / 20f;
				Vector3 vector = Interp(st, en, ctrl1, ctrl2, t2);
				Gizmos.DrawLine(vector, to);
				to = vector;
			}
			Gizmos.color = Color.blue;
			Vector3 vector2 = Interp(st, en, ctrl1, ctrl2, t);
			Gizmos.DrawLine(vector2, vector2 + Velocity(st, en, ctrl1, ctrl2, t));
		}
	}
}
