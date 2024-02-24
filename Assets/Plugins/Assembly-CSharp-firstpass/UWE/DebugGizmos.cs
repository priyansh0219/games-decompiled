using System.Collections.Generic;
using UnityEngine;

namespace UWE
{
	public class DebugGizmos : MonoBehaviour
	{
		public interface Element
		{
			void OnDrawGizmos();

			string GetLabel();

			Vector3 GetLabelPos();
		}

		public class Box : Element
		{
			public Vector3 center;

			public Vector3 size;

			public Color color;

			public string label;

			public void OnDrawGizmos()
			{
				Gizmos.color = color;
				Gizmos.DrawCube(center, size);
			}

			public string GetLabel()
			{
				return label;
			}

			public Vector3 GetLabelPos()
			{
				return center;
			}
		}

		public class Sphere : Element
		{
			public Vector3 center;

			public float radius;

			public Color color;

			public string label;

			public void OnDrawGizmos()
			{
				Gizmos.color = color;
				Gizmos.DrawSphere(center, radius);
			}

			public string GetLabel()
			{
				return label;
			}

			public Vector3 GetLabelPos()
			{
				return center;
			}
		}

		public class Line : Element
		{
			public Vector3 a;

			public Vector3 b;

			public Color color;

			public void OnDrawGizmos()
			{
				Gizmos.color = color;
				Gizmos.DrawLine(a, b);
			}

			public string GetLabel()
			{
				return null;
			}

			public Vector3 GetLabelPos()
			{
				return a;
			}
		}

		public class Point : Element
		{
			public Vector3 pos;

			public Color color;

			public string label;

			public void OnDrawGizmos()
			{
				Gizmos.color = color;
				Gizmos.DrawCube(pos, new Vector3(0.05f, 0.05f, 0.05f));
			}

			public string GetLabel()
			{
				return label;
			}

			public Vector3 GetLabelPos()
			{
				return pos;
			}
		}

		public class Quad : Element
		{
			public Vector3 a;

			public Vector3 b;

			public Vector3 c;

			public Vector3 d;

			public Color color;

			public string label;

			public void OnDrawGizmos()
			{
				Gizmos.color = color;
				Gizmos.DrawLine(a, b);
				Gizmos.DrawLine(b, c);
				Gizmos.DrawLine(c, d);
				Gizmos.DrawLine(d, a);
			}

			public string GetLabel()
			{
				return label;
			}

			public Vector3 GetLabelPos()
			{
				return (a + b + c + d) / 4f;
			}
		}

		private static DebugGizmos main;

		public List<Element> elements = new List<Element>();

		private static DebugGizmos Get()
		{
			if (main == null)
			{
				Object[] array = Resources.FindObjectsOfTypeAll(typeof(DebugGizmos));
				for (int i = 0; i < array.Length; i++)
				{
					main = (DebugGizmos)array[i];
				}
				if (main == null)
				{
					main = new GameObject("__DEBUG_GIZMOS__").AddComponent<DebugGizmos>();
				}
			}
			return main;
		}

		public static void Clear()
		{
			Get().elements.Clear();
		}

		public static void AddBox(Vector3 center, Vector3 size, Color color, string label = null)
		{
			Box box = new Box();
			box.center = center;
			box.size = size;
			box.color = color;
			box.label = label;
			Get().elements.Add(box);
		}

		public static void AddSphere(Vector3 center, float radius, Color color, string label = null)
		{
			Sphere sphere = new Sphere();
			sphere.center = center;
			sphere.radius = radius;
			sphere.color = color;
			sphere.label = label;
			Get().elements.Add(sphere);
		}

		public static void AddLine(Vector3 a, Vector3 b, Color color)
		{
			Line line = new Line();
			line.a = a;
			line.b = b;
			line.color = color;
			Get().elements.Add(line);
		}

		public static void AddPoint(Vector3 pos, Color color, string label = null)
		{
			Point point = new Point();
			point.pos = pos;
			point.color = color;
			point.label = label;
			Get().elements.Add(point);
		}

		public static void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color, string label = null)
		{
			Quad quad = new Quad();
			quad.a = a;
			quad.b = b;
			quad.c = c;
			quad.d = d;
			quad.color = color;
			quad.label = label;
			Get().elements.Add(quad);
		}

		public static void AddQuad(Int3 a, Int3 b, Int3 c, Int3 d, Color color, string label = null)
		{
			AddQuad(a.ToVector3(), b.ToVector3(), c.ToVector3(), d.ToVector3(), color, label);
		}
	}
}
