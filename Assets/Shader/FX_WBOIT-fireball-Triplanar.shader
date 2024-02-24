Shader "FX/WBOIT-fireball-Triplanar" {
	Properties {
		[Enum(TwoSided,0,OneSidedReverseWEIRD,1,OneSided,2)] _MyCullVariable ("Cull", Float) = 2
		_Color ("Diffuse Color", Vector) = (1,1,1,1)
		_FragColor ("Frag Color", Vector) = (1,1,1,1)
		_ColorStrength ("Color Strength", Vector) = (1,1,1,1)
		_ColorStrengthAtNight ("Color Strength At Night", Vector) = (1,1,1,1)
		_EmissiveTex ("Emissive 1", 2D) = "white" {}
		_EmissUV ("_EmissUV", Vector) = (0.1,0.1,0.1,0)
		_ScrollSpeed ("ScrollSpeed", Vector) = (0.1,0.1,0.1,0.1)
		_EmissiveTex2 ("Emissive 2", 2D) = "white" {}
		_Emiss2UV ("_Emiss2UV", Vector) = (0.1,0.1,0.1,0)
		_ScrollSpeed2 ("ScrollSpeed2", Vector) = (0.1,0.1,0.1,0.1)
		_RefractStrength ("Refraction Strength(X,Y) Offset(Z,W)", Vector) = (0.1,0.1,0.05,0.05)
		_RefractMap ("RefractMap", 2D) = "bump" {}
		_RefractUV ("_RefractUV", Vector) = (0.1,0.1,0.1,0)
		_RefractSpeed ("RefractMap Speed", Vector) = (0,0,0,0)
		_DeformMap ("Deform Map", 2D) = "white" {}
		_DeformUV ("_DeformUV", Vector) = (0.1,0.1,0.1,0)
		_DeformStrength ("Deform Strength", Float) = 0.2
		_DeformSpeed ("Deform Speed", Vector) = (0.1,0.1,0,0)
		_FragsTex ("_FragsTex", 2D) = "white" {}
		_FragsUV ("_FragsUV", Vector) = (0.1,0.1,0.1,0)
		_ScrollSpeed3 ("Frags ScrollSpeed", Vector) = (0.1,0.1,0.1,0.1)
		_DeformStrengthFrag ("Frags Deform Strength", Float) = 0
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
	//CustomEditor "FireballMaterialEditor"
}