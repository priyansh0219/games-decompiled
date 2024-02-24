namespace UnityEngine.PostProcessing
{
	public sealed class UserLutComponent : PostProcessingComponentRenderTexture<UserLutModel>
	{
		private static class Uniforms
		{
			internal static readonly int _UserLut = Shader.PropertyToID("_UserLut");

			internal static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");
		}

		public override bool active
		{
			get
			{
				if (base.model == null)
				{
					return false;
				}
				UserLutModel.Settings settings = base.model.settings;
				if (base.model.enabled && settings.lut != null && settings.contribution > 0f && settings.lut.height == (int)Mathf.Sqrt(settings.lut.width))
				{
					return !context.interrupted;
				}
				return false;
			}
		}

		public override void Prepare(Material material)
		{
			UserLutModel.Settings settings = base.model.settings;
			material.EnableKeyword("USER_LUT");
			material.SetTexture(Uniforms._UserLut, settings.lut);
			material.SetVector(Uniforms._UserLut_Params, new Vector4(1f / (float)settings.lut.width, 1f / (float)settings.lut.height, (float)settings.lut.height - 1f, settings.contribution));
		}

		public void OnGUI()
		{
			UserLutModel.Settings settings = base.model.settings;
			GUI.DrawTexture(new Rect(context.viewport.x * (float)Screen.width + 8f, 8f, settings.lut.width, settings.lut.height), settings.lut);
		}
	}
}
