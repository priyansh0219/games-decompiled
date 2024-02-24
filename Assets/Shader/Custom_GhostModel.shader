Shader "Custom/GhostModel" {
	Properties {
		_GridThickness ("Grid Thickness", Float) = 0.02
		_GridSpacing ("Grid Spacing", Float) = 35
		_Tint ("Grid Color", Vector) = (0.7,0.7,1,1)
		_FresnelPow ("_FresnelPow", Float) = 2
		_FresnelFade ("_FresnelFade", Float) = 2
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
}