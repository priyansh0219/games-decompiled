Shader "FX/PowerPreview" {
	Properties {
		_LiquidColor ("_LiquidColor (RGB)", Vector) = (1,1,1,1)
		_BubblesColor ("_BubblesColor (RGB)", Vector) = (1,1,1,1)
		_LiquidGlow ("_LiquidGlow", Float) = 1
		_BubblesGlow ("_BubblesGlow", Float) = 1
		_levelOffset ("_level", Float) = 0
		_MainTex ("Liquid (RGB) Trans (A)", 2D) = "white" {}
		_MainTex2 ("Bubbles (RGB) Trans (A)", 2D) = "white" {}
		_DistortMap ("DistortMap", 2D) = "white" {}
		_scrollSpeed ("_scrollSpeed", Vector) = (1,0,0.5,0)
		_distortScrollSpeed ("_distortScrollSpeed", Vector) = (1,0,0,0)
		_distortStr ("_distortStr - LiquidLevel(R) - LiquidTex(G) - BubblesTex(B) - _levelHardness(A)", Vector) = (0.1,0.1,0.1,1000)
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
	Fallback "Transparent/VertexLit"
}