Shader "UI/Bar" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[Toggle(USE_BACKGROUND)] _UseBackground ("Use Background", Float) = 1
		_BarTex ("Bar (RGB)", 2D) = "white" {}
		_Amount ("Amount", Range(0, 1)) = 0
		_TopBorder ("TopBorder (pixels / texture size)", Float) = 0
		_BottomBorder ("BottomBorder (pixels / texture size)", Float) = 0
		[KeywordEnum(Horizontal, Vertical)] _Fill ("Fill", Float) = 1
		[Toggle(SUBDIVIDE)] _Subdivide ("Subdivide", Float) = 0
		_Subdivisions ("Subdivisions", Range(0, 20)) = 1
		_SeparatorWidth ("Separator Width (2 * N pixels)", Float) = 2
		_SeparatorSmooth ("Separator Smooth", Range(0, 0.999999)) = 0.5
		[Space] [Toggle(SHEAR)] _Shear ("Shear", Float) = 0
		_ShearTop ("Shear Top", Float) = 0
		_ShearBottom ("Shear Bottom", Float) = 0
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