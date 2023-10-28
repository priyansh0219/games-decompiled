Shader "Custom/Multiply Add Scroll Pawn" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_MultiplyTexA ("Multiply texture A", 2D) = "black" {}
		_MultiplyTexB ("Multiply texture B", 2D) = "black" {}
		_DistortionTex ("DistortionTex", 2D) = "black" {}
		_DetailTex ("Detail texture", 2D) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_pawnCenterWorld ("pawnCenterWorld", Vector) = (0,0,0,1)
		_pawnDrawSizeWorld ("pawnDrawSizeWorld", Vector) = (0,0,0,1)
		_AgeSecs ("AgeSecs", Float) = 0
		_texAScrollSpeed ("TexAScrollSpeed", Vector) = (0.5,0.5,0,1)
		_texBScrollSpeed ("TexBScrollSpeed", Vector) = (0.5,0.5,0,1)
		_DetailScrollSpeed ("DetailScrollSpeed", Vector) = (0.5,0.5,0,1)
		_DistortionScrollSpeed ("DistortionScrollSpeed", Vector) = (0.5,0.5,0,1)
		_detailIntensity ("Intensity", Float) = 1
		_texAScale ("TextA Scale", Float) = 1
		_texBScale ("TextB Scale", Float) = 1
		_DetailScale ("Detail Scale", Float) = 1
		_DetailDistortion ("Detail Distortion", Float) = 0
		_DetailOffset ("DetailOffset", Vector) = (0,0,0,1)
		_OutlineMultiplier ("Outline Multiplier", Float) = 0
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