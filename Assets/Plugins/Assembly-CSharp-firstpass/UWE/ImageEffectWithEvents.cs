using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace UWE
{
	public class ImageEffectWithEvents : PostEffectsBase
	{
		public delegate void OnRenderImageHandler(RenderTexture source, RenderTexture destination);

		public struct OnRenderImageWrapper : IDisposable
		{
			private RenderTexture src;

			private RenderTexture dest;

			private ImageEffectWithEvents effect;

			public OnRenderImageWrapper(ImageEffectWithEvents effect, RenderTexture src, RenderTexture dest)
			{
				this.effect = effect;
				this.src = src;
				this.dest = dest;
				if (effect.beforeOnRenderImage != null)
				{
					effect.beforeOnRenderImage(src, dest);
				}
			}

			public void Dispose()
			{
				if (effect.afterOnRenderImage != null)
				{
					effect.afterOnRenderImage(src, dest);
				}
			}
		}

		public event OnRenderImageHandler beforeOnRenderImage;

		public event OnRenderImageHandler afterOnRenderImage;
	}
}
