Shader "FX/WBOIT-CyclopsShield" {
	Properties {
		_Color ("Tint Color", Vector) = (1,1,1,0.5)
		_SolidColor ("_SolidColor", Vector) = (1,1,1,0.5)
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Texture", 2D) = "white" {}
		_ScrollSpeed ("_ScrollSpeed", Vector) = (0.5,0.2,0,0)
		_Intensity ("_Intensity", Range(0, 1)) = 0
		_ImpactIntensity ("_ImpactIntensity", Range(0, 1)) = 0
		_ImpactPosition ("_ImpactPosition", Vector) = (1.5,0.2,0,0)
		_ImpactParams ("_ImpactParams", Vector) = (1.5,0.2,0,0)
		_EnabledSize ("_EnabledSize", Float) = 1
		_DisabledSize ("_DisabledSize", Float) = 0.395
		_WobbleParams ("_Wobble Speed(X), Intensity(Y), Frequency(Z)", Vector) = (1.5,0.2,0,0)
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