using UnityEngine;

namespace UWE
{
	public class ThreadSafeGrayscaleTexture
	{
		public int width;

		public int height;

		public float[,] grays;

		public ThreadSafeGrayscaleTexture(Texture2D src)
		{
			width = src.width;
			height = src.height;
			Color[] pixels = src.GetPixels();
			grays = new float[width, height];
			Int2.RangeEnumerator enumerator = Int2.Range(Int2.zero, new Int2(width, height) - 1).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Int2 current = enumerator.Current;
				int num = current.y * width + current.x;
				grays.Set(current, pixels[num].grayscale);
			}
		}

		public float Get(Int2 p)
		{
			return grays.Get(p);
		}

		public float Get(Vector2 uv)
		{
			return grays.BilerpUV(uv);
		}

		public float Get(float u, float v)
		{
			return Get(new Vector2(u, v));
		}
	}
}
