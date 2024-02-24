Shader "FX/Scanning" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("_Color", Vector) = (1,1,1,1)
		_ColorStrength ("Color Strength", Vector) = (0.5,1.25,2.5,1)
		_ColorStrengthAtNight ("Color Strength At Night", Vector) = (0.5,1.25,2.5,1)
		_ScrollSpeed ("ScrollSpeed", Vector) = (0.1,0.1,0.1,0.1)
		_FresnelFade ("Fresnel fade", Float) = 1.5
		_FresnelPow ("Fresnel power", Float) = 8
		_TimeScale ("Time Scale", Float) = 1
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