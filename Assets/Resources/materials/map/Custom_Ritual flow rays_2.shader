Shader "Custom/Ritual flow rays" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_SmokeTex1 ("Smoke texture 1", 2D) = "white" {}
		_SmokeTex2 ("Smoke texture 2", 2D) = "white" {}
		_MaskTex ("Mask texture", 2D) = "white" {}
		_DistortionTex ("Distortion texture", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_AgeSecs ("AgeSecs", Float) = 0
		_ScrollSpeed ("ScrollSpeed", Float) = 0.1
		_Thickness ("Thickness", Float) = 10
		_Distortion ("Distortion", Float) = 1
		_Detail ("Detail", Float) = 0
		_VerticalScale ("VerticalScale", Float) = 1.3
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