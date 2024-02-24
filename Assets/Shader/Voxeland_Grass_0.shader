Shader "Voxeland/Grass" {
	Properties {
		_Color ("Main Color", Vector) = (1,1,1,1)
		_MainTex ("Diffuse(RGB). Cutout(A)", 2D) = "gray" {}
		_Cutoff ("Alpha Ref", Range(0, 1)) = 0.33
		_Ambient ("Additional Ambient", Vector) = (1,1,1,1)
		_AnimSpeed ("Animation Speed (cycles/sec)", Float) = 0.2
		_AnimStrength ("Animation Strength", Range(0, 1)) = 0.33
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
	Fallback "Transparent/Cutout/VertexLit"
}