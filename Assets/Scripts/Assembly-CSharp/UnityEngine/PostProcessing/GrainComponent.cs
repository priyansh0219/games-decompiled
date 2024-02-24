namespace UnityEngine.PostProcessing
{
	public sealed class GrainComponent : PostProcessingComponentRenderTexture<GrainModel>
	{
		private static class Uniforms
		{
			internal static readonly int _Grain_Params1 = Shader.PropertyToID("_Grain_Params1");

			internal static readonly int _Grain_Params2 = Shader.PropertyToID("_Grain_Params2");

			internal static readonly int _GrainTex = Shader.PropertyToID("_GrainTex");

			internal static readonly int _Phase = Shader.PropertyToID("_Phase");
		}

		private RenderTexture m_GrainLookupRT;

		public override bool active
		{
			get
			{
				if (base.model != null && base.model.enabled && base.model.settings.intensity > 0f && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
				{
					return !context.interrupted;
				}
				return false;
			}
		}

		public override void OnDisable()
		{
			GraphicsUtils.Destroy(m_GrainLookupRT);
			m_GrainLookupRT = null;
		}

		public override void Prepare(Material material)
		{
			GrainModel.Settings settings = base.model.settings;
			material.EnableKeyword("GRAIN");
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float value = Random.value;
			float value2 = Random.value;
			if (m_GrainLookupRT == null || !m_GrainLookupRT.IsCreated())
			{
				GraphicsUtils.Destroy(m_GrainLookupRT);
				m_GrainLookupRT = new RenderTexture(192, 192, 0, RenderTextureFormat.ARGBHalf)
				{
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Repeat,
					anisoLevel = 0,
					name = "Grain Lookup Texture"
				};
				m_GrainLookupRT.Create();
			}
			Material material2 = context.materialFactory.Get("Hidden/Post FX/Grain Generator");
			material2.SetFloat(Uniforms._Phase, realtimeSinceStartup / 20f);
			Graphics.Blit(null, m_GrainLookupRT, material2, settings.colored ? 1 : 0);
			material.SetTexture(Uniforms._GrainTex, m_GrainLookupRT);
			material.SetVector(Uniforms._Grain_Params1, new Vector2(settings.luminanceContribution, settings.intensity * 20f));
			material.SetVector(Uniforms._Grain_Params2, new Vector4((float)context.width / (float)m_GrainLookupRT.width / settings.size, (float)context.height / (float)m_GrainLookupRT.height / settings.size, value, value2));
		}
	}
}
