using System;
using System.Diagnostics;
using UnityEngine;

namespace UWE
{
	[RequireComponent(typeof(Camera))]
	public class FrameTimeOverlay : MonoBehaviour
	{
		[Serializable]
		public class BaseLine
		{
			public float ms;

			public Color color;
		}

		[AssertNotNull]
		public Camera camera;

		public int bufferSize = 200;

		public float xScale = 1f;

		public float msMax = 100f;

		public bool debugGameTimeSpam;

		public Material lineMaterial;

		public BaseLine[] baseLines;

		private bool[] gc;

		private int lastGCCount;

		private float[] data;

		private int dataCursor;

		private Stopwatch watch = new Stopwatch();

		public float yScale => 1f / msMax;

		private void Start()
		{
			data = new float[bufferSize];
			gc = new bool[bufferSize];
		}

		private void OnEnable()
		{
			watch.Reset();
		}

		private void UpdateData()
		{
			if (debugGameTimeSpam)
			{
				UnityEngine.Debug.Log(Time.deltaTime * 1000f + " ms");
			}
			if (watch.IsRunning)
			{
				int num = GC.CollectionCount(0);
				gc[dataCursor] = num > lastGCCount;
				lastGCCount = num;
				float num2 = watch.ElapsedMilliseconds;
				data[dataCursor] = num2;
				dataCursor = (dataCursor + 1) % data.Length;
			}
			watch.Restart();
		}

		public float GetFrameMS()
		{
			if (data == null)
			{
				return -1f;
			}
			if (dataCursor == 0)
			{
				return data[data.Length - 1];
			}
			return data[dataCursor - 1];
		}

		private void GLViewportPoint(float x, float y)
		{
			x = Mathf.Clamp(x, 0f, 1f);
			y = Mathf.Clamp(y, 0f, 1f);
			Vector3 position = new Vector3(x, y, camera.nearClipPlane + 1f);
			Vector3 vector = camera.ViewportToWorldPoint(position);
			GL.Vertex3(vector.x, vector.y, vector.z);
		}

		private void OnPostRender()
		{
			UpdateData();
			lineMaterial.SetPass(0);
			GL.PushMatrix();
			GL.Begin(1);
			GL.Color(Color.green);
			for (int i = 0; i < data.Length - 1; i++)
			{
				int num = (dataCursor + i) % data.Length;
				int num2 = (dataCursor + i + 1) % data.Length;
				GLViewportPoint((float)i * 1f / (float)data.Length * xScale, data[num] * yScale);
				GLViewportPoint((float)(i + 1) * 1f / (float)data.Length * xScale, data[num2] * yScale);
				if (gc[num])
				{
					GL.Color(Color.red);
					GLViewportPoint((float)i * 1f / (float)data.Length * xScale, 0f);
					GLViewportPoint((float)i * 1f / (float)data.Length * xScale, 0.5f);
					GL.Color(Color.green);
				}
			}
			BaseLine[] array = baseLines;
			foreach (BaseLine baseLine in array)
			{
				GL.Color(baseLine.color);
				GLViewportPoint(0f * xScale, baseLine.ms * yScale);
				GLViewportPoint(1f * xScale, baseLine.ms * yScale);
			}
			GL.End();
			GL.PopMatrix();
		}
	}
}
