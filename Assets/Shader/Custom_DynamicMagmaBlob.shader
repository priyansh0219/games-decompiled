Shader "Custom/DynamicMagmaBlob" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Normal map", 2D) = "bump" {}
		_SpecColor ("Specular Color (RGB) - Shininess (A)", Vector) = (0.5,0.5,0.5,1)
		_SigMap ("SIG map", 2D) = "white" {}
		_RampTex ("Color Ramp", 2D) = "white" {}
		_Emissive ("Emissive map", 2D) = "black" {}
		_FlowSpeed ("Speed", Float) = 0.05
		_EmissiveCut ("Emissive cutoff", Range(0.01, 0.99)) = 0.5
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Transparent/Cutout/VertexLit"
}