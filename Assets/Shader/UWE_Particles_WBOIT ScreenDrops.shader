Shader "UWE/Particles/WBOIT ScreenDrops" {
	Properties {
		_Color ("Tint Color", Vector) = (1,1,1,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_ColorStrength ("Color multiplier", Vector) = (1,1,1,1)
		_sunSensitivity ("_sunSensitivity", Float) = 1
		_SurfFade ("Surface Fade", Float) = 2
		_SeaLevel ("Surface Level", Float) = 0
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