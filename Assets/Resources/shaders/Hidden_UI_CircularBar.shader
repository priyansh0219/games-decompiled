Shader "Hidden/UI/CircularBar" {
	Properties {
		[HideInInspector] _Value ("Value", Float) = 1
		[HideInInspector] _Width ("Width", Float) = 0.1
		[HideInInspector] _EdgeWidth ("Edge Width", Float) = 0.1
		[HideInInspector] _BorderColor ("Border Color", Vector) = (0,1,1,1)
		[HideInInspector] _BorderWidth ("Border Width", Float) = 0.1
		[HideInInspector] _OverlayTex ("Overlay Texture", 2D) = "black" {}
		[HideInInspector] _Overlay1_ST ("Overlay 1 UV", Vector) = (1,1,0,0)
		[HideInInspector] _Overlay2_ST ("Overlay 2 UV", Vector) = (1,1,0,0)
		[HideInInspector] _OverlayShift ("Overlay Shift", Vector) = (0.3,0.07,0,0)
		[HideInInspector] _OverlayAlpha ("Overlay Alpha", Vector) = (0.5,0.25,0,0)
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
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