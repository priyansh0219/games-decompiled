Shader "FX/WBOIT-BaseBoxFogVolume" {
	Properties {
		_Color ("Main Color", Vector) = (1,1,1,1)
		_BoxMin ("Box Min (X,Y,Z)", Vector) = (0,0,0,0)
		_BoxMax ("Box Max(X,Y,Z)", Vector) = (1,1,1,0)
		_Visibility ("Visibility", Float) = 1
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
}