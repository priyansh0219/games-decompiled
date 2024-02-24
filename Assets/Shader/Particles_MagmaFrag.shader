Shader "Particles/MagmaFrag" {
	Properties {
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_ColorStrength ("Color multiplier", Vector) = (1,1,1,1)
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_RampTex ("Color Ramp", 2D) = "white" {}
		_Emissive ("Emissive map", 2D) = "black" {}
		_Cutoff ("Cutoff", Range(0, 1)) = 0
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