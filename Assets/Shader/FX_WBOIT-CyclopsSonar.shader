Shader "FX/WBOIT-CyclopsSonar" {
	Properties {
		_Color ("Tint Color", Vector) = (1,1,1,0.5)
		_ColorStrength ("Color Strength", Vector) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Texture", 2D) = "white" {}
		_PingColor ("Ping Color", Vector) = (1,1,1,0.5)
		_PingCenter ("Ping Center(X,Y) - Offset(Z) - Hardness(W)", Vector) = (0.5,0.5,0.1,10)
		_PingFrequency ("Ping Frequency", Float) = 0.1
		_ScrollSpeed ("Scroll Speed", Float) = 0.1
		_DitherIntensity ("Dither Intensity", Range(0, 1)) = 0.1
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