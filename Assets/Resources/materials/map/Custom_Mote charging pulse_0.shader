Shader "Custom/Mote charging pulse" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_RandomTex ("Random texture", 2D) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_FlickerFrequency ("FlickerFrequency", Float) = 1
		_FlickerAmount ("FlickerAmount", Float) = 1
		_InnerCircleSize ("InnerCircleSize", Float) = 1
		_InnerCircleIntensity ("InnerCircleSize", Float) = 0.2
		_InnerCircleShimmerAmount ("InnerCircleShimmerAmount", Float) = 0.1
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