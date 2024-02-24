using System.Linq;
using UnityEngine;

namespace RadicalLibrary
{
	public static class Spline
	{
		public class Path
		{
			private Vector3[] _path;

			public Vector3[] path
			{
				get
				{
					return _path;
				}
				set
				{
					_path = value;
				}
			}

			public int Length
			{
				get
				{
					if (path == null)
					{
						return 0;
					}
					return path.Length;
				}
			}

			public Vector3 this[int index] => path[index];

			public static implicit operator Path(Vector3[] path)
			{
				return new Path
				{
					path = path
				};
			}

			public static implicit operator Vector3[](Path p)
			{
				if (p == null)
				{
					return new Vector3[0];
				}
				return p.path;
			}

			public static implicit operator Path(Transform[] path)
			{
				return new Path
				{
					path = path.Select((Transform p) => p.position).ToArray()
				};
			}

			public static implicit operator Path(GameObject[] path)
			{
				return new Path
				{
					path = path.Select((GameObject p) => p.transform.position).ToArray()
				};
			}
		}

		public static Vector3 Interp(Path pts, float t)
		{
			return Interp(pts, t, EasingType.Linear);
		}

		public static Vector3 Interp(Path pts, float t, EasingType ease)
		{
			return Interp(pts, t, ease, easeIn: true);
		}

		public static Vector3 Interp(Path pts, float t, EasingType ease, bool easeIn)
		{
			return Interp(pts, t, ease, easeIn, easeOut: true);
		}

		public static Vector3 Interp(Path pts, float t, EasingType ease, bool easeIn, bool easeOut)
		{
			t = Ease(t, ease, easeIn, easeOut);
			if (pts.Length == 0)
			{
				return Vector3.zero;
			}
			if (pts.Length == 1)
			{
				return pts[0];
			}
			if (pts.Length == 2)
			{
				return Vector3.Lerp(pts[0], pts[1], t);
			}
			if (pts.Length == 3)
			{
				return QuadBez.Interp(pts[0], pts[2], pts[1], t);
			}
			if (pts.Length == 4)
			{
				return CubicBez.Interp(pts[0], pts[3], pts[1], pts[2], t);
			}
			return CRSpline.Interp(Wrap(pts), t);
		}

		private static float Ease(float t)
		{
			return Ease(t, EasingType.Linear);
		}

		private static float Ease(float t, EasingType ease)
		{
			return Ease(t, ease, easeIn: true);
		}

		private static float Ease(float t, EasingType ease, bool easeIn)
		{
			return Ease(t, ease, easeIn, easeOut: true);
		}

		private static float Ease(float t, EasingType ease, bool easeIn, bool easeOut)
		{
			t = Mathf.Clamp01(t);
			if (easeIn && easeOut)
			{
				t = Easing.EaseInOut(t, ease);
			}
			else if (easeIn)
			{
				t = Easing.EaseIn(t, ease);
			}
			else if (easeOut)
			{
				t = Easing.EaseOut(t, ease);
			}
			return t;
		}

		public static Vector3 InterpConstantSpeed(Path pts, float t)
		{
			return InterpConstantSpeed(pts, t, EasingType.Linear);
		}

		public static Vector3 InterpConstantSpeed(Path pts, float t, EasingType ease)
		{
			return InterpConstantSpeed(pts, t, ease, easeIn: true);
		}

		public static Vector3 InterpConstantSpeed(Path pts, float t, EasingType ease, bool easeIn)
		{
			return InterpConstantSpeed(pts, t, ease, easeIn, easeOut: true);
		}

		public static Vector3 InterpConstantSpeed(Path pts, float t, EasingType ease, bool easeIn, bool easeOut)
		{
			t = Ease(t, ease, easeIn, easeOut);
			if (pts.Length == 0)
			{
				return Vector3.zero;
			}
			if (pts.Length == 1)
			{
				return pts[0];
			}
			if (pts.Length == 2)
			{
				return Vector3.Lerp(pts[0], pts[1], t);
			}
			if (pts.Length == 3)
			{
				return QuadBez.Interp(pts[0], pts[2], pts[1], t);
			}
			if (pts.Length == 4)
			{
				return CubicBez.Interp(pts[0], pts[3], pts[1], pts[2], t);
			}
			return CRSpline.InterpConstantSpeed(Wrap(pts), t);
		}

		private static float Clamp(float f)
		{
			return Mathf.Clamp01(f);
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition)
		{
			return MoveOnPath(pts, currentPosition, ref pathPosition, 1f);
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition, float maxSpeed)
		{
			return MoveOnPath(pts, currentPosition, ref pathPosition, maxSpeed, 100f);
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition, float maxSpeed, float smoothnessFactor)
		{
			return MoveOnPath(pts, currentPosition, ref pathPosition, maxSpeed, smoothnessFactor, EasingType.Linear);
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition, float maxSpeed, float smoothnessFactor, EasingType ease)
		{
			return MoveOnPath(pts, currentPosition, ref pathPosition, maxSpeed, smoothnessFactor, ease, easeIn: true);
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition, float maxSpeed, float smoothnessFactor, EasingType ease, bool easeIn)
		{
			return MoveOnPath(pts, currentPosition, ref pathPosition, maxSpeed, smoothnessFactor, ease, easeIn, easeOut: true);
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition, float maxSpeed, float smoothnessFactor, EasingType ease, bool easeIn, bool easeOut)
		{
			maxSpeed *= Time.deltaTime;
			pathPosition = Clamp(pathPosition);
			Vector3 vector = InterpConstantSpeed(pts, pathPosition, ease, easeIn, easeOut);
			float magnitude;
			while ((magnitude = (vector - currentPosition).magnitude) <= maxSpeed && pathPosition != 1f)
			{
				pathPosition = Clamp(pathPosition + 1f / smoothnessFactor);
				vector = InterpConstantSpeed(pts, pathPosition, ease, easeIn, easeOut);
			}
			if (magnitude != 0f)
			{
				currentPosition = Vector3.MoveTowards(currentPosition, vector, maxSpeed);
			}
			return currentPosition;
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition, ref Quaternion rotation)
		{
			return MoveOnPath(pts, currentPosition, ref pathPosition, ref rotation, 1f);
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition, ref Quaternion rotation, float maxSpeed)
		{
			return MoveOnPath(pts, currentPosition, ref pathPosition, ref rotation, maxSpeed, 100f);
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition, ref Quaternion rotation, float maxSpeed, float smoothnessFactor)
		{
			return MoveOnPath(pts, currentPosition, ref pathPosition, ref rotation, maxSpeed, smoothnessFactor, EasingType.Linear);
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition, ref Quaternion rotation, float maxSpeed, float smoothnessFactor, EasingType ease)
		{
			return MoveOnPath(pts, currentPosition, ref pathPosition, ref rotation, maxSpeed, smoothnessFactor, ease, easeIn: true);
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition, ref Quaternion rotation, float maxSpeed, float smoothnessFactor, EasingType ease, bool easeIn)
		{
			return MoveOnPath(pts, currentPosition, ref pathPosition, ref rotation, maxSpeed, smoothnessFactor, ease, easeIn, easeOut: true);
		}

		public static Vector3 MoveOnPath(Path pts, Vector3 currentPosition, ref float pathPosition, ref Quaternion rotation, float maxSpeed, float smoothnessFactor, EasingType ease, bool easeIn, bool easeOut)
		{
			Vector3 vector = MoveOnPath(pts, currentPosition, ref pathPosition, maxSpeed, smoothnessFactor, ease, easeIn, easeOut);
			rotation = (vector.Equals(currentPosition) ? Quaternion.identity : Quaternion.LookRotation(vector - currentPosition));
			return vector;
		}

		public static Quaternion RotationBetween(Path pts, float t1, float t2)
		{
			return RotationBetween(pts, t1, t2, EasingType.Linear);
		}

		public static Quaternion RotationBetween(Path pts, float t1, float t2, EasingType ease)
		{
			return RotationBetween(pts, t1, t2, ease, easeIn: true);
		}

		public static Quaternion RotationBetween(Path pts, float t1, float t2, EasingType ease, bool easeIn)
		{
			return RotationBetween(pts, t1, t2, ease, easeIn, easeOut: true);
		}

		public static Quaternion RotationBetween(Path pts, float t1, float t2, EasingType ease, bool easeIn, bool easeOut)
		{
			return Quaternion.LookRotation(Interp(pts, t2, ease, easeIn, easeOut) - Interp(pts, t1, ease, easeIn, easeOut));
		}

		public static Vector3 Velocity(Path pts, float t)
		{
			return Velocity(pts, t, EasingType.Linear);
		}

		public static Vector3 Velocity(Path pts, float t, EasingType ease)
		{
			return Velocity(pts, t, ease, easeIn: true);
		}

		public static Vector3 Velocity(Path pts, float t, EasingType ease, bool easeIn)
		{
			return Velocity(pts, t, ease, easeIn, easeOut: true);
		}

		public static Vector3 Velocity(Path pts, float t, EasingType ease, bool easeIn, bool easeOut)
		{
			t = Ease(t);
			if (pts.Length == 0)
			{
				return Vector3.zero;
			}
			if (pts.Length == 1)
			{
				return pts[0];
			}
			if (pts.Length == 2)
			{
				return Vector3.Lerp(pts[0], pts[1], t);
			}
			if (pts.Length == 3)
			{
				return QuadBez.Velocity(pts[0], pts[2], pts[1], t);
			}
			if (pts.Length == 3)
			{
				return CubicBez.Velocity(pts[0], pts[3], pts[1], pts[2], t);
			}
			return CRSpline.Velocity(Wrap(pts), t);
		}

		public static Vector3[] Wrap(Vector3[] path)
		{
			return new Vector3[1] { path[0] }.Concat(path).Concat(new Vector3[1] { path[path.Length - 1] }).ToArray();
		}

		public static void GizmoDraw(Vector3[] pts, float t)
		{
			GizmoDraw(pts, t, EasingType.Linear);
		}

		public static void GizmoDraw(Vector3[] pts, float t, EasingType ease)
		{
			GizmoDraw(pts, t, ease, easeIn: true);
		}

		public static void GizmoDraw(Vector3[] pts, float t, EasingType ease, bool easeIn)
		{
			GizmoDraw(pts, t, ease, easeIn, easeOut: true);
		}

		public static void GizmoDraw(Vector3[] pts, float t, EasingType ease, bool easeIn, bool easeOut)
		{
			Gizmos.color = Color.white;
			Vector3 to = Interp(pts, 0f);
			for (int i = 1; i <= 20; i++)
			{
				float t2 = (float)i / 20f;
				Vector3 vector = Interp(pts, t2, ease, easeIn, easeOut);
				Gizmos.DrawLine(vector, to);
				to = vector;
			}
			Gizmos.color = Color.blue;
			Vector3 vector2 = Interp(pts, t, ease, easeIn, easeOut);
			Gizmos.DrawLine(vector2, vector2 + Velocity(pts, t, ease, easeIn, easeOut));
		}
	}
}
