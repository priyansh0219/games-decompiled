Shader "Custom/Mote glow distort background" {
	Properties {
		_MainTex ("Main texture", 2D) = "black" {}
		_DistortionTex ("Distortion texture", 2D) = "black" {}
		_NoiseTex ("Noise texture", 2D) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_distortionIntensity ("distortionIntensity", Float) = 0.04
		_brightnessMultiplier ("brightnessMultiplier", Float) = 1
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