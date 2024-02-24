Shader "FX/WBOIT-SphereVolumeNoise" {
	Properties {
		_Color ("Main Color", Vector) = (1,1,1,1)
		_RadiusSqr ("Radius", Float) = 1
		_Visibility ("Visibility", Float) = 1
		_NoiseSpeed ("Noise Speed (X,Y,Z,W)", Vector) = (1,0.1,1,1)
		_NoiseScale ("_NoiseScale (X,Y,Z)", Vector) = (0.64,0.6,0.62,0)
		_NoiseOffset ("_NoiseOffset (X,Y,Z,W)", Vector) = (0,0,0,0)
		_Octaves ("Noise Octaves", Float) = 2
		_SeaLevel ("_SeaLevel", Float) = 2
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