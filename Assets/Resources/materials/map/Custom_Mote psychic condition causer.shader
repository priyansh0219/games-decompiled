Shader "Custom/Mote psychic condition causer" {
	Properties {
		_MainTex ("Main texture", 2D) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_distortionIntensity ("distortionIntensity", Float) = 0.05
		_distortionScale ("distortionScale", Float) = 1
		_brightnessMultiplier ("brightnessMultiplier", Float) = 1
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