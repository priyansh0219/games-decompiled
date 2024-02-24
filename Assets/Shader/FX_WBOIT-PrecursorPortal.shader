Shader "FX/WBOIT-PrecursorPortal" {
	Properties {
		_RadialFade ("_RadialFade", Float) = 1
		_ColorCenter ("_ColorCenter", Vector) = (1,1,1,0.5)
		_ColorOuter ("_ColorOuter", Vector) = (1,1,1,0.5)
		_ColorStrength ("Color Strength", Vector) = (1,1,1,1)
		_DetailsColorStrength ("Details Color Strength", Vector) = (1,1,1,1)
		_MainTex ("_MainTex", 2D) = "white" {}
		_DetailsTex ("_DetailsTex", 2D) = "black" {}
		_RipplesDeformTex ("_RipplesDeformTex", 2D) = "white" {}
		_RipplesFrequency ("Ripples Frequency", Float) = 180
		_RipplesPow ("Ripples Pow", Float) = 1.5
		_RefractStrength ("RefractStrength", Float) = 1
		_ScrollSpeed ("Scroll Speed", Float) = 10
		_RotationSpeed ("_RotationSpeed", Float) = 0.1
		_ClipOffset ("Near Clip Offset", Float) = 0
		_ClipFade ("Near Clip Fade Distance", Float) = 0
		_FresnelFade ("Rim Strength", Float) = 1
		_FresnelPow ("Rim Power", Float) = 0
		_InvFade ("Soft Particles Factor", Range(0.01, 3)) = 1
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