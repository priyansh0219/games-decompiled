using UnityEngine;

namespace UWE
{
	[RequireComponent(typeof(Camera))]
	public class GraphRenderer : MonoBehaviour
	{
		public int numPoints = 100;

		public bool debugRandomData;

		public float xScale = 1f;

		public float yScale = 1f;

		public Material lineMaterial;

		private float[] data;

		private void Start()
		{
			data = new float[numPoints];
			if (debugRandomData)
			{
				for (int i = 0; i < data.Length; i++)
				{
					data[i] = Random.value;
				}
			}
		}

		private void Update()
		{
		}

		private void GLViewportPoint(float x, float y)
		{
			Vector3 position = new Vector3(x, y, GetComponent<Camera>().nearClipPlane + 1f);
			Vector3 vector = GetComponent<Camera>().ViewportToWorldPoint(position);
			GL.Vertex3(vector.x, vector.y, vector.z);
		}

		private void OnPostRender()
		{
			lineMaterial.SetPass(0);
			GL.PushMatrix();
			GL.Begin(1);
			GL.Color(Color.white);
			for (int i = 0; i < data.Length - 1; i++)
			{
				GLViewportPoint((float)i * 1f / (float)data.Length * xScale, data[i] * yScale);
				GLViewportPoint((float)(i + 1) * 1f / (float)data.Length * xScale, data[i + 1] * yScale);
			}
			GL.End();
			GL.PopMatrix();
		}
	}
}
