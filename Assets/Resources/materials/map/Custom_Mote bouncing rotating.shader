Shader "Custom/Mote bouncing rotating" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_BounceTex ("Bounce texture", 2D) = "black" {}
		_RotationTex ("Rotation texture", 2D) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_AgeSecs ("AgeSecs", Float) = 0
		_bounceSpeed ("bounceSpeed", Float) = 1
		_bounceAmplitude ("bounceAmplitude", Float) = 1
		_rotationSpeed ("bounceSpeed", Float) = 1
		_rotationAmplitude ("bounceAmplitude", Float) = 1
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