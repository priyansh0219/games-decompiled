Shader "Custom/WaterSurface" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_NormalsTex ("Base (RGB)", 2D) = "white" {}
		_ZWriteMode ("ZWriteMode", Float) = 0
		_MyCullVariable ("MyCullVariable", Float) = 0
		_TexelLength2 ("", Float) = 0
		_Refraction0 ("", Float) = 0
		_ReflectionColor ("", Vector) = (0,0,0,0)
		_RefractionColor ("", Vector) = (0,0,0,0)
		_FoamTexture ("", 2D) = "" {}
		_FoamMaskTexture ("", 2D) = "" {}
		_FoamSmoothing ("", Float) = 0
		_FoamAmountTexture ("", 2D) = "" {}
		_FoamScale ("", Float) = 0
		_FoamDistance ("", Float) = 0
		_SubSurfaceFoamColor ("", Vector) = (0,0,0,0)
		_SubSurfaceFoamScale ("", Float) = 0
		_FoamAmountMultiplier ("", Float) = 0
		_BackLightTint ("", Vector) = (0,0,0,0)
		_WaveHeightThicknessScale ("", Float) = 0
		_ClipTexture ("", 2D) = "" {}
		_SunReflectionGloss ("", Float) = 0
		_SunReflectionAmount ("", Float) = 0
		_ScreenSpaceRefractionRatio ("", Float) = 0
		_ScreenSpaceInternalReflectionFlatness ("", Float) = 0
		_PixelStride ("", Float) = 0
		_PixelStrideZCuttoff ("", Float) = 0
		_PixelZSize ("", Float) = 0
		_Iterations ("", Float) = 0
		_BinarySearchIterations ("", Float) = 0
		_MaxRayDistance ("", Float) = 0
		_ScreenEdgeFadeStart ("", Float) = 0
		_EyeFadeStart ("", Float) = 0
		_EyeFadeEnd ("", Float) = 0
		_CapturedDepthSurface ("", 2D) = "" {}
		_RefractionTexture ("", 2D) = "" {}
		_betaR ("", Vector) = (0,0,0,0)
		_betaM ("", Vector) = (0,0,0,0)
		_SunBurstTexture ("", 2D) = "" {}
		_mieConst ("", Vector) = (0,0,0,0)
		_miePhase_g ("", Vector) = (0,0,0,0)
		_GroundColor ("", Vector) = (0,0,0,0)
		_NightHorizonColor ("", Vector) = (0,0,0,0)
		_NightZenithColor ("", Vector) = (0,0,0,0)
		_MoonInnerCorona ("", Vector) = (0,0,0,0)
		_MoonOuterCorona ("", Vector) = (0,0,0,0)
		_MoonSize ("", Float) = 0
		_colorCorrection ("", Vector) = (0,0,0,0)
		_PlanetTexture ("", 2D) = "" {}
		_PlanetNormalMap ("", 2D) = "" {}
		_PlanetRimColor ("", Vector) = (0,0,0,0)
		_PlanetAmbientLight ("", Vector) = (0,0,0,0)
		_PlanetLightWrap ("", Float) = 0
		_PlanetInnerCorona ("", Vector) = (0,0,0,0)
		_PlanetOuterCorona ("", Vector) = (0,0,0,0)
		_MoonSampler ("", 2D) = "" {}
		_CloudsTexture ("", 2D) = "" {}
		_CloudsAlphaSaturation ("", Float) = 0
		_CloudsAttenuation ("", Float) = 0
		_CloudsScatteringMultiplier ("", Float) = 0
		_CloudsScatteringExponent ("", Float) = 0
		_SunColorMultiplier ("", Float) = 0
		_SkyColorMultiplier ("", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}