Shader "UWE/Particles/WBOIT-FakeVolumetricLight" {
	Properties {
		_Intensity ("_Intensity", Range(0, 2)) = 0.5
		_Color ("Tint Color", Vector) = (1,1,1,1)
		_MainTex ("_MainTex", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01, 3)) = 1
		_FresnelFade ("Rim Strength", Float) = 2
		_FresnelPow ("Rim Power", Float) = 5
		_ClipOffset ("Near Clip Offset", Float) = 8
		_ClipFade ("Near Clip Fade Distance", Float) = 2
		_Offset ("_GradientOffset", Range(0, 1)) = 0
		_Fallof ("_GradientFallof", Range(0, 1)) = 0
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