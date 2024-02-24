Shader "Unlit/FX_WarpTube" {
	Properties {
		[MarmoToggle(DETAILONLY,null,BoldLabel)] _DetailsOnly ("DETAILONLY", Float) = 0
		_Color ("Tint Color", Vector) = (1,1,1,0.5)
		_RimColor ("_RimColor", Vector) = (1,1,1,0.5)
		_MainTex ("Texture", 2D) = "white" {}
		_MainSpeed ("_MainSpeed", Vector) = (1,1,1,1)
		_DetailsColor ("_Detail Color", Vector) = (1,1,1,1)
		_DetailTex ("_DetailTex", 2D) = "white" {}
		_MainOffset ("_MainOffset", Float) = 0
		_DetailsSpeed ("DetailSpeed", Vector) = (1,1,1,1)
		_DeformTex ("_DeformTex", 2D) = "bump" {}
		_DeformNormalStrength ("_DeformNormalStrength", Float) = 1
		_DeformStrength ("_DeformStrength", Vector) = (1,1,1,1)
		_DeformSpeed ("_DeformSpeed", Vector) = (1,1,1,1)
		_FresnelFade ("_FresnelFade", Float) = 1
		_FresnelPow ("_FresnelPow", Float) = 1
		_SinWaveFrequency ("_SinWaveFrequency", Vector) = (1,1,1,1)
		_SinWaveSpeed ("_SinWaveSpeed", Vector) = (1,1,1,1)
		_SinWaveStrength ("_SinWaveStrength", Vector) = (1,1,1,1)
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