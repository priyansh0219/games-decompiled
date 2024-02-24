Shader "Custom/DistanceField/ClipDistanceField" {
	Properties {
		_DistanceFieldTexture ("Distance field", 3D) = "" {}
		[HideInInspector] _DistanceFieldMin ("DistanceFieldMin", Vector) = (0,0,0,0)
		[HideInInspector] _DistanceFieldSizeRcp ("DistanceFieldSizeRcp", Vector) = (0,0,0,0)
		[HideInInspector] _DistanceFieldScale ("DistanceFieldScale", Float) = 0
		[HideInInspector] _ObjectScale ("ObjectScale", Vector) = (0,0,0,0)
		[HideInInspector] _WaterDisplacementTexture ("WaterDisplacementTexture", 2D) = "" {}
		[HideInInspector] _WaterPatchLength ("WaterPatchLength", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
}