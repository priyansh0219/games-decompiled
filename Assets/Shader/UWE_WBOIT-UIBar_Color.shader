Shader "UWE/WBOIT-UIBar_Color" {
	Properties {
		_Color ("Main Color (RGB)", Vector) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BarTex ("Bar (RGB)", 2D) = "white" {}
		_Amount ("Amount", Range(0, 1)) = 0
		_TopBorder ("TopBorder", Float) = 0
		_BottomBorder ("BottomBorder", Float) = 0
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