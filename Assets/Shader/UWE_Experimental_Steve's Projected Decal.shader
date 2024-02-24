Shader "UWE/Experimental/Steve's Projected Decal" {
	Properties {
		_Color ("Main Color (RGBA)", Vector) = (1,1,1,1)
		_SpecColor ("Specular Color", Vector) = (1,1,1,1)
		_MainTex ("Base (RGB) Alpha(A)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_SIGMap ("Spec(R) Illum(G) map *Gloss/B ignored*", 2D) = "black" {}
		_Scale ("Uniform Scale", Float) = 1
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
	Fallback "Specular"
}