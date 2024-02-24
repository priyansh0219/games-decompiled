Shader "UI/Icon" {
	Properties {
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcFactor ("SrcColor * ", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstFactor ("DstColor * ", Float) = 10
		[Space] [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[Space] [HideInInspector] _NotificationOverlayTex ("", 2D) = "white" {}
		[HideInInspector] _Chroma ("Chroma", Range(0, 1)) = 1
		[HideInInspector] _NotificationStrength ("", Float) = 0
		_FillRect ("Fill (XY = Min, ZW = Size)", Vector) = (-10,-10,20,20)
		_FillValue ("Fill Value", Float) = 0.75
		[Space] [Toggle(SLICE_9_GRID)] _Slice9Grid ("Slice9Grid", Float) = 0
		_Size ("Size (XY, in pixels)", Vector) = (100,100,0,0)
		_Radius ("Radius (in pixels)", Float) = 5
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