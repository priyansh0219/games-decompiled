Shader "Custom/Mote Apocriton Pulse" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Main texture", 2D) = "white" {}
		_PulseTex ("Pulse texture", 2D) = "white" {}
		_distortionIntensity ("distortionIntensity", Float) = 0.05
		_distortionTint ("distortionTint", Vector) = (0,0,0,0)
		_pulseSpeed ("pulseSpeed", Float) = 1
		_ScrollSpeed ("ScrollSpeed", Float) = 0.5
		_ScrollScale ("ScrollScale", Float) = 0.5
		_WaveOpacity ("WaveOpacity", Float) = 0.2
		_AgeSecs ("AgeSecs", Float) = 0
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