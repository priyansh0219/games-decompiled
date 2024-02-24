Shader "Marmoset/Building/BuiltFadeOut" {
	Properties {
		_Color ("Diffuse Color", Vector) = (1,1,1,1)
		_EmissiveTex ("Emissive texture", 2D) = "white" {}
		_NoiseTex ("Noise texture", 2D) = "white" {}
		_ScrollSpeed ("Scroll Speed", Float) = 0.1
		_BorderColor ("Border Color", Vector) = (0.7,0.7,1,1)
		_AlphaScale ("Alpha Scale", Float) = 0.5
		_FadeAmount ("_FadeAmount", Float) = 0
		_FresnelFade ("Fresnel fade", Float) = 0.5
		_FresnelPow ("Fresnel power", Float) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
}