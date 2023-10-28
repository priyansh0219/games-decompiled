Shader "Custom/Mote charging pulse" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_DistortionTex ("Distortion texture", 2D) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_NumFrames ("NumFrames", Float) = 1
		_AgeSecs ("AgeSecs", Float) = 0
		_FramesPerSec ("FramesPerSec", Float) = 1
		_Intensity ("Intensity", Float) = 1
		_DistortionScrollSpeed ("DistortionScrollSpeed", Vector) = (0.45,0.45,0,1)
		_DistortionScale ("DistortionScale", Float) = 1
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