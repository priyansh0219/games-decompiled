Shader "Custom/Water depth" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_AlphaAddTex ("Alpha add texture", 2D) = "" {}
		_WaterDepthIntensity ("Water depth intensity", Float) = 1
		_WaterRippleDensity ("Water ripple density", Float) = 1
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
}