using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/Color Adjustments/Grayscale")]
	public class Grayscale : ImageEffectBase
	{
		public Texture textureRamp;

		public float rampOffset;

		public float effectAmount = 1f;

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			base.material.SetTexture(ShaderPropertyID._RampTex, textureRamp);
			base.material.SetFloat(ShaderPropertyID._RampOffset, rampOffset);
			base.material.SetFloat(ShaderPropertyID._EffectAmount, effectAmount);
			Graphics.Blit(source, destination, base.material);
		}
	}
}
