Shader "UWE/Terrain/TriplanarLavaFlow" {
	Properties {
		_Color ("Main Color (RGB)", Vector) = (1,1,1,1)
		[HDR] _SpecColor ("Specular Color", Vector) = (1,1,1,1)
		_MainTex ("Base (RGB) Splotch(A)", 2D) = "green" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_SIGMap ("Spec(R) Illum(G) Gloss(B) map", 2D) = "black" {}
		_TriplanarScale ("TriplanarScale", Float) = 0.1
		_FlowTex ("Flow Texture", 2D) = "white" {}
		_FlowBumpMap ("Flow Normal Map", 2D) = "bump" {}
		_FlowSIG ("Flow SIG", 2D) = "white" {}
		_FlowMap ("Flow Map", 2D) = "white" {}
		_NoiseMap ("Noise Map", 2D) = "black" {}
		_FlowAmount ("Flow Amount", Float) = 0.5
		_Cycle ("Cycle", Float) = 1
		_Speed ("Speed", Float) = 0.05
		_MaskSpeed ("Mask Speed", Float) = 0.05
		_FlowScale ("Mask Scale", Float) = 0.4
		_InnerBorderBlendRange ("Inner Border Softness", Range(0, 1)) = 0.5
		_InnerBorderBlendOffset ("Inner Border Offset", Range(0, 1)) = 1
		_BorderTint ("Border Tint (RGB)", Vector) = (1,1,1,1)
		_BorderBlendRange ("Border Softness", Range(0, 1)) = 0.5
		_BorderBlendOffset ("Border Offset", Range(0, 1)) = 0
		_Gloss ("Gloss", Range(0, 1)) = 0.5
		[Enum(Off,0,On,1)] _ZWrite ("DO NOT EDIT", Float) = 0
		[Enum(Blue,2,Green,4,Red,8,RGB,14,All,255)] _ColorMask ("DO NOT EDIT ColorMask", Float) = 14
		[Enum(Zero,0,One,1,SrcAlpha,5,OneMinusSrcAlpha,10)] _BlendSrcFactor ("Blend Src", Float) = 5
		[Enum(Zero,0,One,1,SrcAlpha,5,OneMinusSrcAlpha,10)] _BlendDstFactor ("Blend Dst", Float) = 10
		_IsOpaque ("DO NOT EDIT IsOpaque", Float) = 0
		_AlphaTestValue ("DO NOT EDIT AlphaTestValue", Float) = 0
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
	Fallback "Bumped Specular"
}