Shader "Custom/Mote glow circular scroling" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_MaskTex ("Mask texture", 2D) = "white" {}
		_DistortionTex ("Distortion texture", 2D) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_AgeSecs ("AgeSecs", Float) = 0
		_ScrollSpeed ("ScrollSpeed", Float) = 0.5
		_ScrollScale ("ScrollScale", Float) = 0.5
		_DistortionIntensity ("DistortionIntensity", Float) = 0.5
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