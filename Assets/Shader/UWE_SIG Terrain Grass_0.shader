Shader "UWE/SIG Terrain Grass" {
	Properties {
		_Color ("Top Color1 (RGB) Opacity(A)", Vector) = (1,1,1,1)
		_BotColor ("Bottom Color1 (RGB) Opacity(A)", Vector) = (1,1,1,1)
		_Color2 ("Top Color2 (RGB) Opacity(A)", Vector) = (1,1,1,1)
		_BotColor2 ("Bottom Color2 (RGB) Opacity(A)", Vector) = (1,1,1,1)
		_GradientParams ("Gradient Offset(X), Fallof(Y)", Vector) = (0,1,0,0)
		_Mask ("Color Mask (R)", 2D) = "black" {}
		_MaskScale ("MaskScale", Float) = 1
		_MaskStr ("MaskStrength", Float) = 1
		_SpecColor ("Specular Color (RGB)", Vector) = (1,1,1,1)
		_MainTex ("Diffuse(RGB) Opacity(A)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_SIGMap ("Spec(R) Illum(G) Gloss(B) map", 2D) = "black" {}
		_SIGstr ("Spec(X), Illum(Y), Gloss(Z), IllumAtNight(W) Strengths", Vector) = (1,1,1,0)
		_Cutoff ("Border Alpha", Range(0, 1)) = 0.9
		_ForceNormals ("Force normals to (0,1,0)", Range(0, 1)) = 0
		[Enum(TwoSided,0,OneSidedReverseWEIRD,1,OneSided,2)] _MyCullVariable ("Cull", Float) = 0
		_ObjectRight ("Object Right Axis", Vector) = (1,0,0,0)
		_ObjectUp ("Object Up Axis", Vector) = (0,1,0,0)
		_WaveUpMin ("Influence Cutoff Y", Float) = 0
		_WaveAmount ("Wave Amount", Float) = 0.3
		_WaveSpeed ("Wave Speed", Range(0, 1)) = 0.5
		_TimeOffset ("DEBUG: time offset", Float) = 0
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
	Fallback "Specular"
}