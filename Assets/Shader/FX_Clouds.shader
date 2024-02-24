Shader "FX/Clouds" {
	Properties {
		[MarmoToggle(ENABLE_ISLAND_CLOUDS,null,BoldLabel)] _IslandClouds ("IslandClouds", Float) = 0
		[MarmoToggle(ENABLE_DOME_CLOUDS,null,BoldLabel)] _DomeClouds ("DomeClouds", Float) = 0
		[MarmoToggle(ENABLE_PLANET_CLOUDS,null,BoldLabel)] _PlanetClouds ("PlanetClouds", Float) = 0
		[Enum(TwoSided,0,OneSidedReverseWEIRD,1,OneSided,2)] _MyCullVariable ("Cull", Float) = 2
		_ZOffset ("Depth Offset", Float) = 0
		_Color ("Tint Color", Vector) = (1,1,1,0.5)
		_MainTex ("Particle Texture", 2D) = "black" {}
		_ColorStrength ("Color multiplier", Vector) = (1,1,1,1)
		_ColorStrengthAtNight ("Color at night", Vector) = (1,1,1,1)
		_LightAmount ("_LightAmount", Float) = 0.5
		_StepSize ("_StepSize", Float) = 0.008
		_CloudsScatteringExponent ("_CloudsScatteringExponent", Float) = 0.5
		_CloudsScatteringMultiplier ("_CloudsScatteringMultiplier", Float) = 0.5
		_CloudsAttenuation ("_CloudsAttenuation", Float) = 0.5
		_SkyColorMultiplier ("_SkyColorMultiplier", Float) = 3.72
		_SunColorMultiplier ("_SunColorMultiplier", Float) = 1.87
		_ChannelLerp ("_ChannelLerp", Vector) = (1,1,1,1)
		_ChannelLerp2 ("_ChannelLerp2", Vector) = (1,1,1,0)
		_LightMin ("Min Light", Float) = 0.05
		_LightMul ("Light Multiplier", Float) = 1
		_HorizonFallof ("_HorizonFallof", Float) = 0.5
		_FadeInSkyColor ("_FadeInSkyColor", Float) = 0.5
		_AlphaPow ("_AlphaPow", Float) = 1
		_MainTex2 ("Multiply with Main", 2D) = "black" {}
		_scrollSpeed ("Scroll Speed", Vector) = (0.1,0.1,0,0)
		_DeformMap ("Deform Map", 2D) = "white" {}
		_DeformStrength ("Deform Strength", Vector) = (0.1,0.1,0,0)
		_DeformSpeed ("Deform Speed", Vector) = (0.1,0.1,0,0)
		_MixerTex ("_MixerTex", 2D) = "black" {}
		_MixStrength ("_MixStrength", Vector) = (0.1,0.1,0,0)
		_MixSpeed ("_MixSpeed", Vector) = (0.1,0.1,0,0)
		_ClipOffset ("Near Clip Offset", Float) = 8
		_ClipFade ("Near Clip Fade Distance", Float) = 2
		_FresnelFade ("Rim Strength", Float) = 2
		_FresnelPow ("Rim Power", Float) = 5
		_SurfFade ("Surface Fade", Float) = 2
		_SeaLevel ("Surface Level", Float) = 0
		_InvFade ("Soft Particles Factor", Range(0.01, 3)) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}