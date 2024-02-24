Shader "UI/IconBar" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[Space] _Size ("Size (XY, in pixels)", Vector) = (100,100,0,0)
		_Radius ("Radius (in pixels)", Float) = 33
		_Width ("Width (in pixels)", Float) = 8
		_Cut ("Cut (scalar)", Range(0, 1)) = 0.2
		_Antialias ("Antialias (in pixels)", Float) = 1.5
		_Edge ("Edge (in pixels)", Float) = 2.3
		[Space] _ColorBackground ("Color Background", Vector) = (0.212,0.318,0.525,1)
		_ColorEdge ("Color Edge", Vector) = (0.867,0.961,0.996,0.7)
		_Value0 ("Value 0", Float) = 0.2
		_Value1 ("Value 1", Float) = 0.5
		_Value2 ("Value 2", Float) = 0.8
		_Color0 ("Color 0", Vector) = (0.816,0.447,0.325,1)
		_Color1 ("Color 1", Vector) = (0.976,0.839,0.341,1)
		_Color2 ("Color 2", Vector) = (0.643,0.843,0.412,1)
		[Space] _Value ("Value", Range(0, 1)) = 0.75
		_StencilComp ("Stencil Comparison", Float) = 8
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