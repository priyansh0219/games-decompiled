Shader "Custom/Bullet shield psychic" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_DistortionTex ("Distortion texture", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_PulseInterval ("PulseInterval", Float) = 0.7
		_DistortionScale ("DistortionScale", Float) = 0.5
		_DistortionIntensity ("DistortionIntensity", Float) = 0.2
		_MinAlpha ("MinAlpha", Float) = 0.25
		_MaxAlpha ("MaxAlpha", Float) = 0.5
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