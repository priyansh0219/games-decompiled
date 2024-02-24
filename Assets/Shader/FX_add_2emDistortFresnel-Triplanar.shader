Shader "FX/add_2emDistortFresnel-Triplanar" {
	Properties {
		_Color ("Diffuse Color", Vector) = (1,1,1,1)
		_ColorStrength ("Color Strength", Vector) = (1,1,1,1)
		_ColorStrengthAtNight ("Color Strength At Night", Vector) = (1,1,1,1)
		_EmissiveTex ("Emissive 1", 2D) = "white" {}
		_EmissUV ("_EmissUV", Vector) = (0.1,0.1,0.1,0)
		_ScrollSpeed ("ScrollSpeed", Vector) = (0.1,0.1,0.1,0.1)
		_EmissiveTex2 ("Emissive 2", 2D) = "white" {}
		_Emiss2UV ("_Emiss2UV", Vector) = (0.1,0.1,0.1,0)
		_ScrollSpeed2 ("ScrollSpeed2", Vector) = (0.1,0.1,0.1,0.1)
		_FadeAmount ("_FadeAmount", Float) = 0
		_FresnelFade ("Fresnel fade", Float) = 0.5
		_FresnelPow ("Fresnel power", Float) = 0.5
		_InvFade ("Border Fade Out", Float) = 0.5
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