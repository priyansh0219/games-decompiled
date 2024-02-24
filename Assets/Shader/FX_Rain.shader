Shader "FX/Rain" {
	Properties {
		_rainAmount ("_rainAmount", Float) = 0
		_Color ("Main Color (RGB)", Vector) = (1,1,1,1)
		_EmissiveStrengh ("Emissive Strength", Float) = 1
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_MainTex2 ("Base 2 (RGB) Trans (A)", 2D) = "white" {}
		_DistortMap ("DistortMap", 2D) = "white" {}
		_scrollSpeed ("Scroll Speed", Vector) = (1,0,0.5,0)
		_distortScrollSpeed ("Scroll Speed", Vector) = (1,0,0,0)
		_distortStr ("Distort Strength", Float) = 1
		_ClipOffset ("Camera Clip Offset", Float) = 8
		_ClipFade ("Camera Clip Fade Distance", Float) = 2
		_SurfLevel ("Surface Level", Float) = 0
		_SurfFade ("Surface Fade", Float) = 2
		_InvFade ("Soft Particles Factor", Range(0.01, 5)) = 1
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
	Fallback "Transparent/VertexLit"
}