Shader "Image Effects/EndSequenceWarp" {
	Properties {
		_Amount ("_Amount", Float) = 0
		_MainTex ("_MainTex", 2D) = "white" {}
		_ColorCenter ("_ColorCenter", Vector) = (0,0,0,0.5)
		_ColorOuter ("_ColorOuter", Vector) = (0.71,0.76,0.83,0.5)
		_ColorStrength ("Color multiplier", Vector) = (0.1,5,0.1,0.1)
		_BackgroundTex ("Background (RGB)", 2D) = "white" {}
		_DetailTex ("Details (RGBA)", 2D) = "Black" {}
		_NervesTex ("Nerves (RGBA)", 2D) = "Black" {}
		_MainScrollSpeed ("_MainScrollSpeed", Vector) = (-0.5,-1.75,0.5,1)
		_DetailScrollSpeed ("_DetailScrollSpeed", Vector) = (-0.6,-1,-0.7,-0.2)
		_NervesScrollSpeed ("_NervesScrollSpeed", Vector) = (0.1,-5,-0.1,-2)
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