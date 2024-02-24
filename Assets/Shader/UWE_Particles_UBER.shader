Shader "UWE/Particles/UBER" {
	Properties {
		[Enum(TwoSided,0,OneSidedReverseWEIRD,1,OneSided,2)] _MyCullVariable ("Cull", Float) = 2
		[HideInInspector] _Mode ("__mode", Float) = 2
		[HideInInspector] _SrcBlend ("__src", Float) = 1
		[HideInInspector] _DstBlend ("__dst", Float) = 1
		[HideInInspector] _SrcBlend2 ("__src2", Float) = 0
		[HideInInspector] _DstBlend2 ("__dst2", Float) = 10
		[HideInInspector] _Ztest ("__Ztest", Float) = 2
		[HideInInspector] _AffectedByDayNightCycle ("_AffectedByDayNightCycle", Float) = 1
		_ZOffset ("Depth Offset", Float) = 0
		_Color ("Tint Color", Vector) = (1,1,1,0.5)
		_ColorStrength ("Color multiplier", Vector) = (1,1,1,1)
		_ColorStrengthAtNight ("Color at night", Vector) = (1,1,1,1)
		_Cutoff ("_Cutoff", Float) = 0.001
		[KeywordEnum(Off, Vertex, Pixel)] FX_LightMode ("LightMode", Float) = 0
		_SelfIllumination ("_SelfIllumination", Range(0, 1)) = 0.003
		_LightWrapAround ("LightWrapAround", Range(0, 1)) = 0.2
		_NormalSharpness ("NormalSharpness", Range(0, 1)) = 0.25
		_LightDesaturation ("_LightDesaturation", Range(0, 1)) = 0.8
		_NormalStrength ("_NormalStrength", Float) = 1
		_NormalMap ("_NormalMap", 2D) = "bump" {}
		_NormalMap_Speed ("_NormalScrollSpeed", Vector) = (0.1,0.1,0,0)
		_NormalMap_TriplanarUV ("Triplanar _Normal UV (x,y,z)", Vector) = (1,1,1,1)
		_AlphaPow ("_AlphaPow", Float) = 1
		_MainTex ("Particle Texture", 2D) = "white" {}
		_MainTex_Speed ("Scroll Speed", Vector) = (0.1,0.1,0,0)
		_MainTex_TriplanarUV ("Triplanar _MainTex UV (x,y,z)", Vector) = (1,1,1,1)
		_MainTex2 ("Multiply with Main", 2D) = "white" {}
		_MainTex2_Speed ("Triplanar MulMap ScrollSpeed (x,y,z)", Vector) = (0.1,0.1,0,0)
		_MainTex2_TriplanarUV ("Triplanar _MulTex UV (x,y,z)", Vector) = (1,1,1,1)
		_DeformStrength ("Deform Strength", Float) = 0.2
		_DeformMap ("Deform Map", 2D) = "white" {}
		_DeformMap_Speed ("Deform Speed", Vector) = (0.1,0.1,0,0)
		_DeformMap_TriplanarUV ("Triplanar _Deform UV (x,y,z)", Vector) = (1,1,1,1)
		_RefractStrength ("RefractMap Strength", Float) = 0.1
		_RefractMap ("RefractMap", 2D) = "bump" {}
		_RefractMap_Speed ("Refract ScrollSpeed (x,y,z)", Vector) = (0.1,0.1,0,0)
		_RefractMap_TriplanarUV ("Triplanar RefractMap UV (x,y,z)", Vector) = (1,1,1,1)
		_ClipOffset ("Near Clip Offset", Float) = 8
		_ClipFade ("Near Clip Fade Distance", Float) = 2
		_FresnelFade ("Rim Strength", Float) = 2
		_FresnelPow ("Rim Power", Float) = 5
		_SurfFade ("Surface Fade", Float) = 2
		_SeaLevel ("Surface Level", Float) = 0
		_LocalFloodLevel ("_Local Flood Level", Float) = 0
		_InvFade ("Soft Particles Factor", Range(0.01, 3)) = 1
		_Scale ("Amplitude Main(X,Y,Z)", Vector) = (0,1,0,0)
		_Frequency ("Frequency Main(X,Y,Z)", Vector) = (0.5,0.5,0.5,0)
		_Speed ("MainSpeed (X)", Float) = 1
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
	//CustomEditor "ParticlesShaderGUI"
}