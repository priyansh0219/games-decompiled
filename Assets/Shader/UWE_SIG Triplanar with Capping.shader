Shader "UWE/SIG Triplanar with Capping" {
	Properties {
		_Color ("Main Color (RGB)", Vector) = (1,1,1,1)
		_SpecColor ("Specular Color", Vector) = (1,1,1,1)
		_CapColor ("Cap Color (RGB)", Vector) = (1,1,1,1)
		_CapSpecColor ("Cap Specular Color", Vector) = (1,1,1,1)
		_CapTexture ("Cap Base (RGB) Splotch(A)", 2D) = "green" {}
		_CapBumpMap ("Cap Normal map", 2D) = "bump" {}
		_CapSIGMap ("Cap Spec(R) Illum(G) map *Gloss/B ignored*", 2D) = "black" {}
		_CapEmissionScale ("Cap Emission Scale", Range(0, 2)) = 1
		_SideTexture ("Side Base (RGB) Splotch(A)", 2D) = "yellow" {}
		_SideBumpMap ("Side Normal map", 2D) = "bump" {}
		_SideSIGMap ("Side Spec(R) Illum(G) map *Gloss/B ignored*", 2D) = "black" {}
		_SideEmissionScale ("Side Emission Scale", Range(0, 2)) = 1
		_CapScale ("CapScale", Float) = 0.1
		_SideScale ("SideScale", Float) = 0.1
		_TriplanarBlendRange ("Triplanar Blend Sharpness", Range(0.1, 80)) = 2
		_CapBorderBlendRange ("Cap Border Softness", Range(0, 1)) = 0.1
		_CapBorderBlendOffset ("Cap Border Offset", Range(-1, 0)) = 0
		_CapBorderBlendAngle ("Cap Border Angle", Range(0.5, 5)) = 1
		_InnerBorderBlendRange ("Inner Border Softness", Range(0, 1)) = 0.5
		_InnerBorderBlendOffset ("Inner Border Offset", Range(0, 1)) = 1
		_Gloss ("Gloss", Range(0, 1)) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	Fallback "Specular"
}