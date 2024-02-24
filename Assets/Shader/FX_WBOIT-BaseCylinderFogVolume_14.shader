Shader "FX/WBOIT-BaseCylinderFogVolume" {
	Properties {
		_Color ("Main Color", Vector) = (1,1,1,1)
		_Visibility ("Visibility", Float) = 1
		_CylRadius ("Radius", Float) = 1
		_MinY ("Min Y", Float) = -0.5
		_MaxY ("Max Y", Float) = 0.5
		_BIsForwardLighting ("Is Forward Lighting", Float) = 0
		_LocalFloodLevel ("_BaseFloodLevel", Float) = 0
		_AffectedBySkyExposure ("_AffectedBySkyExposure", Float) = 0
		_SelfIllumination ("_SelfIllumination", Range(0, 1)) = 0.2
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
	//CustomEditor "FloodFogMaterialEditor"
}