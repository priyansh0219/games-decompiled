Shader "UWE/SIG Alpha Border" {
	Properties {
		_Color ("Tint(RGB) Opacity(A)", Vector) = (1,1,1,1)
		_SpecColor ("Specular Color(RGB)", Vector) = (1,1,1,1)
		_MainTex ("Diffuse(RGB) Opacity(A)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_SIGMap ("Spec(R) Illum(G) Gloss(B) map", 2D) = "black" {}
		_Cutoff ("Border Alpha", Range(0, 1)) = 0.9
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