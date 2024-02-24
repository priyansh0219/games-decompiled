Shader "Unlit/FX-FakeGodRays" {
	Properties {
		_Color ("Tint Color", Vector) = (1,1,1,0.5)
		_MainTex ("Texture", 2D) = "white" {}
		_RayDir ("_RayDir", Vector) = (0.1,-2,0,0)
		_ScrollSpeed ("_ScrollSpeed", Vector) = (0.05,-0.04,0,0)
		_Length ("_Length", Float) = 1
		_FresnelFade ("_FresnelFade", Float) = 1
		_FresnelPow ("_FresnelPow", Float) = 1
		_InvFade ("_InvFade", Range(0.01, 5)) = 3
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