using UnityEngine;

namespace Verse
{
	public static class ShaderUtility
	{
		public static bool SupportsMaskTex(this Shader shader)
		{
			if (!(shader == ShaderDatabase.CutoutComplex) && !(shader == ShaderDatabase.CutoutSkinOverlay) && !(shader == ShaderDatabase.Wound) && !(shader == ShaderDatabase.FirefoamOverlay))
			{
				return shader == ShaderDatabase.CutoutWithOverlay;
			}
			return true;
		}

		public static Shader GetSkinShader(bool skinColorOverriden)
		{
			if (skinColorOverriden)
			{
				return ShaderDatabase.CutoutSkinColorOverride;
			}
			return ShaderDatabase.CutoutSkin;
		}
	}
}
