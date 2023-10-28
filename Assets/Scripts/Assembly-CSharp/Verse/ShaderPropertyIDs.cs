using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class ShaderPropertyIDs
	{
		private static readonly string PlanetSunLightDirectionName = "_PlanetSunLightDirection";

		private static readonly string PlanetSunLightEnabledName = "_PlanetSunLightEnabled";

		private static readonly string PlanetRadiusName = "_PlanetRadius";

		private static readonly string MapSunLightDirectionName = "_CastVect";

		private static readonly string GlowRadiusName = "_GlowRadius";

		private static readonly string GameSecondsName = "_GameSeconds";

		private static readonly string ColorName = "_Color";

		private static readonly string ColorTwoName = "_ColorTwo";

		private static readonly string MaskTexName = "_MaskTex";

		private static readonly string SwayHeadName = "_SwayHead";

		private static readonly string ShockwaveSpanName = "_ShockwaveSpan";

		private static readonly string AgeSecsName = "_AgeSecs";

		private static readonly string RandomPerObjectName = "_RandomPerObject";

		private static readonly string RotationName = "_Rotation";

		private static readonly string OverlayOpacityName = "_OverlayOpacity";

		private static readonly string OverlayColorName = "_OverlayColor";

		private static readonly string AgeSecsPausableName = "_AgeSecsPausable";

		private static readonly string MainTextureOffsetName = "_Main_TexOffset";

		private static readonly string MainTextureScaleName = "_Main_TexScale";

		private static readonly string MaskTextureOffsetName = "_Mask_TexOffset";

		private static readonly string MaskTextureScaleName = "_Mask_TexScale";

		public static int PlanetSunLightDirection = Shader.PropertyToID(PlanetSunLightDirectionName);

		public static int PlanetSunLightEnabled = Shader.PropertyToID(PlanetSunLightEnabledName);

		public static int PlanetRadius = Shader.PropertyToID(PlanetRadiusName);

		public static int MapSunLightDirection = Shader.PropertyToID(MapSunLightDirectionName);

		public static int GlowRadius = Shader.PropertyToID(GlowRadiusName);

		public static int GameSeconds = Shader.PropertyToID(GameSecondsName);

		public static int AgeSecs = Shader.PropertyToID(AgeSecsName);

		public static int AgeSecsPausable = Shader.PropertyToID(AgeSecsPausableName);

		public static int RandomPerObject = Shader.PropertyToID(RandomPerObjectName);

		public static int Rotation = Shader.PropertyToID(RotationName);

		public static int OverlayOpacity = Shader.PropertyToID(OverlayOpacityName);

		public static int OverlayColor = Shader.PropertyToID(OverlayColorName);

		public static int Color = Shader.PropertyToID(ColorName);

		public static int ColorTwo = Shader.PropertyToID(ColorTwoName);

		public static int MaskTex = Shader.PropertyToID(MaskTexName);

		public static int SwayHead = Shader.PropertyToID(SwayHeadName);

		public static int ShockwaveColor = Shader.PropertyToID("_ShockwaveColor");

		public static int ShockwaveSpan = Shader.PropertyToID(ShockwaveSpanName);

		public static int WaterCastVectSun = Shader.PropertyToID("_WaterCastVectSun");

		public static int WaterCastVectMoon = Shader.PropertyToID("_WaterCastVectMoon");

		public static int WaterOutputTex = Shader.PropertyToID("_WaterOutputTex");

		public static int WaterOffsetTex = Shader.PropertyToID("_WaterOffsetTex");

		public static int ShadowCompositeTex = Shader.PropertyToID("_ShadowCompositeTex");

		public static int FallIntensity = Shader.PropertyToID("_FallIntensity");

		public static int MainTextureOffset = Shader.PropertyToID(MainTextureOffsetName);

		public static int MainTextureScale = Shader.PropertyToID(MainTextureScaleName);

		public static int MaskTextureOffset = Shader.PropertyToID(MaskTextureOffsetName);

		public static int MaskTextureScale = Shader.PropertyToID(MaskTextureScaleName);
	}
}
