Shader "Custom/FirefoamOverlay" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_MaskTex ("Mask texture", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_Main_TexOffset ("Main texcoord offset", Vector) = (0,0,0,0)
		_Main_TexScale ("Main texcoord scale", Vector) = (1,1,1,1)
		_Mask_TexOffset ("Mask texcoord offset", Vector) = (0,0,0,0)
		_Mask_TexScale ("Mask texcoord scale", Vector) = (1,1,1,1)
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