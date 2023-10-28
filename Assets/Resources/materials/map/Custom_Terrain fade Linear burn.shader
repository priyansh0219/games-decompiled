Shader "Custom/Terrain fade Linear burn" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_PollutionTintColor ("PollutionTintColor", Vector) = (1,1,1,1)
		_BurnTex ("Burn texture", 2D) = "white" {}
		_BurnColor ("BurnColor", Vector) = (1,1,1,1)
		_BurnScale ("BurnScale", Vector) = (1,1,1,1)
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