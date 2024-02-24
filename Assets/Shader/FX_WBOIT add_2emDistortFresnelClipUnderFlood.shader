Shader "FX/WBOIT add_2emDistortFresnelClipUnderFlood" {
	Properties {
		[HideInInspector] _AffectedByDayNightCycle ("_AffectedByDayNightCycle", Float) = 1
		_Color ("Tint Color", Vector) = (1,1,1,0.5)
		_ColorStrength ("Color multiplier", Vector) = (1,1,1,1)
		_ColorStrengthAtNight ("Color at night", Vector) = (1,1,1,1)
		_Cutoff ("_Cutoff", Float) = 0.001
		_AlphaPow ("_AlphaPow", Float) = 1
		[KeywordEnum(Off, Vertex, Pixel)] FX_LightMode ("LightMode", Float) = 0
		_SelfIllumination ("_SelfIllumination", Range(0, 1)) = 0.003
		_LightWrapAround ("LightWrapAround", Range(0, 1)) = 0.2
		_NormalSharpness ("NormalSharpness", Range(0, 1)) = 0.25
		_LightDesaturation ("_LightDesaturation", Range(0, 1)) = 0.8
		_MainTex ("Particle Texture", 2D) = "white" {}
		_MainTex_Speed ("Scroll Speed", Vector) = (0.1,0.1,0,0)
		_MainTex2 ("Multiply with Main", 2D) = "white" {}
		_MainTex2_Speed ("Triplanar MulMap ScrollSpeed (x,y,z)", Vector) = (0.1,0.1,0,0)
		_DeformStrength ("Deform Strength", Float) = 0.2
		_DeformMap ("Deform Map", 2D) = "white" {}
		_DeformMap_Speed ("Deform Speed", Vector) = (0.1,0.1,0,0)
		_RefractStrength ("RefractMap Strength", Float) = 0.1
		_RefractMap ("RefractMap", 2D) = "bump" {}
		_RefractMap_Speed ("Refract ScrollSpeed (x,y,z)", Vector) = (0.1,0.1,0,0)
		_ClipOffset ("Near Clip Offset", Float) = 8
		_ClipFade ("Near Clip Fade Distance", Float) = 2
		_FresnelFade ("Rim Strength", Float) = 2
		_FresnelPow ("Rim Power", Float) = 5
		_InvFade ("Soft Particles Factor", Range(0.01, 3)) = 1
		_SurfFade ("Surface Fade", Float) = 2
		_LocalFloodLevel ("_LocalFloodLevel", Float) = 2
		_gravityStrength ("Gravity Strength", Float) = 2
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