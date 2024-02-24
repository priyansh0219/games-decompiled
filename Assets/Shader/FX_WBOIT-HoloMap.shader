Shader "FX/WBOIT-HoloMap" {
	Properties {
		_Color ("Tint Color", Vector) = (1,1,1,0.5)
		_ColorStrength ("Color Strength", Vector) = (1,1,1,1)
		_MinShadingSmoothness ("Min Shading Smoothness", Float) = 0.2
		_MaxShadingSmoothness ("Max Shading Smoothness", Float) = 0.5
		_MainTex ("Texture", 2D) = "white" {}
		_FresnelFade ("Rim Strength", Float) = 2
		_FresnelPow ("Rim Power", Float) = 5
		_ScanIntensity ("Scan Intensity", Float) = 0.5
		_ScanFrequency ("Scan Frequency", Float) = 0.1
		_ScanSpeed ("Scan Speed", Float) = 0.1
		_ScanWidth ("Scan Width", Float) = 0.3
		_WireframeIntensity ("Wireframe Intensity", Float) = 0.5
		_WireframeDensity ("Wireframe Density", Float) = 2
		_WireframeWidth ("Wireframe Width", Float) = 0.1
		_FadeRadius ("Fade Radius", Float) = 0.33
		_FadeSharpness ("Fade Sharpness", Float) = 2
		_NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.1
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