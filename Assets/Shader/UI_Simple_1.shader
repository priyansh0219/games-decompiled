Shader "UI/Simple" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[Space] [Enum(UnityEngine.Rendering.BlendMode)] _SrcFactor ("SrcColor * ", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstFactor ("DstColor * ", Float) = 10
		[Space] [Enum(UnityEngine.Rendering.BlendMode)] _SrcFactorA ("SrcAlpha * ", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstFactorA ("DstAlpha * ", Float) = 10
		[Space] [Toggle(ALPHA_PREMULTIPLY)] _AlphaPremultiply ("Alpha Premultiply", Float) = 0
		[Space] [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
		[Space] _StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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