using System;
using System.Collections.Generic;

namespace UnityEngine.PostProcessing
{
	public sealed class RenderTextureFactory : IDisposable
	{
		private HashSet<RenderTexture> m_TemporaryRTs;

		public RenderTextureFactory()
		{
			m_TemporaryRTs = new HashSet<RenderTexture>();
		}

		public RenderTexture Get(RenderTexture baseRenderTexture)
		{
			return Get(baseRenderTexture.width, baseRenderTexture.height, baseRenderTexture.depth, baseRenderTexture.format, (!baseRenderTexture.sRGB) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB, baseRenderTexture.filterMode, baseRenderTexture.wrapMode);
		}

		public RenderTexture Get(int width, int height, int depthBuffer = 0, RenderTextureFormat format = RenderTextureFormat.DefaultHDR, RenderTextureReadWrite rw = RenderTextureReadWrite.Default, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp, string name = "FactoryTempTexture")
		{
			RenderTexture renderTexture = ((!DynamicResolution.IsEnabled()) ? RenderTexture.GetTemporary(width, height, depthBuffer, format, rw) : RenderTexture.GetTemporary(width, height, depthBuffer, format, rw, 1, RenderTextureMemoryless.None, VRTextureUsage.None, useDynamicScale: true));
			renderTexture.filterMode = filterMode;
			renderTexture.wrapMode = wrapMode;
			renderTexture.name = name;
			m_TemporaryRTs.Add(renderTexture);
			return renderTexture;
		}

		public void Release(RenderTexture rt)
		{
			if (!(rt == null))
			{
				if (!m_TemporaryRTs.Contains(rt))
				{
					throw new ArgumentException($"Attempting to remove a RenderTexture that was not allocated: {rt}");
				}
				m_TemporaryRTs.Remove(rt);
				RenderTexture.ReleaseTemporary(rt);
			}
		}

		public void ReleaseAll()
		{
			HashSet<RenderTexture>.Enumerator enumerator = m_TemporaryRTs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				RenderTexture.ReleaseTemporary(enumerator.Current);
			}
			m_TemporaryRTs.Clear();
		}

		public void Dispose()
		{
			ReleaseAll();
		}
	}
}
