Shader "Custom/Mote proximity scanner radius" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_ScanTex ("Scan texture", 2D) = "black" {}
		_SmokeTex ("Smoky texture", 2D) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_smokeAmount ("smokeAmount", Float) = 1
		_smokeScale ("smokeScale", Float) = 1
		_smokeScrollSpeed ("smokeScrollSpeed", Float) = 1
		_rotationSpeed ("rotationSpeed", Float) = 1
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