Shader "FX/WBOIT-WaterBase" {
	Properties {
		[Enum(TwoSided,0,OneSidedReverseWEIRD,1,OneSided,2)] _MyCullVariable ("Cull", Float) = 0
		_Color ("Main Color", Vector) = (1,1,1,1)
		_SpecColor ("Specular Color (RGB) - Shininess (A)", Vector) = (0.5,0.5,0.5,1)
		_Shininess ("Shininess", Range(0.01, 2)) = 0.078125
		_MainTex ("Diffuse (RGB) - Gloss (A)", 2D) = "white" {}
		_NoiseTex ("Noise", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_MaskMap ("MaskMap", 2D) = "white" {}
		_ClipedValue ("ClipedValue", Float) = 0
		_MaskPow ("_MaskPow", Float) = 2.2
		_ClipMultiplier ("_ClipMultiplier", Float) = 1.3
		_FlowMap ("FlowMap", 2D) = "white" {}
		_TexScale ("Texture Scale (Main - Noise - Normal - Flow", Vector) = (1,1,1,1)
		_Scale ("Amplitude", Vector) = (0,1,0,0)
		_Frequency ("Frequency", Vector) = (0.5,0.5,0.5,0)
		_Dir ("Direction", Range(0, 1)) = 0
		_Speed ("Speed", Float) = 5
		_Cycle ("Flow Cycle", Float) = 1
		_ScrollSpeed ("ScrollSpeed", Vector) = (0.1,0.1,0.1,0)
		_ClipFade ("Near Clip Fade", Float) = 2
		_MainFoam ("Main Foam", Float) = 1
		_TopFoamHeight ("Foam Height", Float) = 10
		_BorderFoam ("Border Foam", Float) = 5
		_InvFade ("Border Fade Out", Float) = 0.5
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
	Fallback "Diffuse"
}