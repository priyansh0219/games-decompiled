Shader "Hidden/UWE/Capped" {
	Properties {
		[MarmoToggle(UWE_SIG,null,BoldLabel)] _EnableSIG ("SIG", Float) = 0
		_Color ("Main Color", Vector) = (1,1,1,1)
		[HDR] _SpecColor ("Specular Color", Vector) = (0.5,0.5,0.5,1)
		_Shininess ("Shininess", Range(0.03, 1)) = 0.078125
		_SideTexture ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_SideBumpMap ("Normalmap", 2D) = "bump" {}
		_CapTexture ("Cap (RGB) Gloss (A)", 2D) = "white" {}
		_CapBumpMap ("Cap Normalmap", 2D) = "bump" {}
		_CapBorderBlendRange ("Cap Border Softness", Range(0, 1)) = 0.1
		_CapBorderBlendOffset ("Cap Border Offset", Range(-1, 0)) = 0
		_CapBorderBlendAngle ("Cap Border Angle", Range(0.5, 5)) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}