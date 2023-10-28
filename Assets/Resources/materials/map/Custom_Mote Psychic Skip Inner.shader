Shader "Custom/Mote Psychic Skip Inner" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Main texture", 2D) = "white" {}
		_innerRingSize ("innerRingSize", Float) = 0.6
		_inTime ("inTime", Float) = 0.4
		_solidTime ("solidTime", Float) = 0.5
		_outTime ("outTime", Float) = 0.3
		_AgeSecs ("AgeSecs", Float) = 0
		_AgeOffset ("AgeOffset", Float) = 0
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