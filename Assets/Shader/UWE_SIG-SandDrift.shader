Shader "UWE/SIG-SandDrift" {
	Properties {
		_Color ("Main Color", Vector) = (1,1,1,1)
		_ScrollColor ("_ScrollColor", Vector) = (1,1,1,1)
		_SpecColor ("Specular Color", Vector) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ScrollTex ("_ScrollTex (RGB)", 2D) = "white" {}
		_ScrollSpeed ("_ScrollSpeed", Vector) = (0.1,0.1,0,0)
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_SIGMap ("Spec(R) Illum(G) Gloss(B) map", 2D) = "black" {}
		_EmissionScale ("Emission Scale", Range(0, 2)) = 1
		_Cutoff ("_Cutoff", Range(0, 1)) = 0.9
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