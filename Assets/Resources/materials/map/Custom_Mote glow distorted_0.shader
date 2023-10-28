Shader "Custom/Mote glow distorted" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_DistortionTex ("Distortion texture", 2D) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_distortionScrollSpeed ("distortionScrollSpeed", Vector) = (0.15,0.15,0,1)
		_distortionScale ("distortionScale", Float) = 0.2
		_distortionIntensity ("distortionIntensity", Float) = 0.3
		_wordSpaceDistortionToggle ("wordSpaceDistortionToggle", Float) = 0
		_textureRepeatAmount ("textureRepeatScale", Vector) = (0,0,0,1)
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