Shader "Custom/Snow" {
	Properties {
		_MainTex ("Main texture", 2D) = "" {}
		_PollutedTex ("Polluted texture", 2D) = "" {}
		_MacroTex ("Macro texture", 2D) = "" {}
		_AlphaAddTex ("Alpha add texture", 2D) = "" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}