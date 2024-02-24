Shader "uSky/uSkyBox Horizon Offset" {
	Properties {
		[HideInInspector] _SkyMultiplier ("_SkyMultiplier", Vector) = (1,4,0.15,0)
		[HideInInspector] _betaR ("BetaR", Vector) = (0.0058,0.0136,0.0331,0)
		[HideInInspector] _betaM ("BetaM", Vector) = (0.004,0.005,0.006,0)
		[HideInInspector] _GroundColor ("Ground Color", Vector) = (0.2,0.6,1.4,0)
		[HideInInspector] _SunDir ("Sun Direction", Vector) = (0.321,0.766,-0.557,0)
		[HideInInspector] _NightHorizonColor ("_Night Horizon Color", Vector) = (0.43,0.47,0.5,1)
		[HideInInspector] _NightZenithColor ("Night Zenith Color", Vector) = (0.00532,0.00707,0.001,0)
		[HideInInspector] _MoonOuterCorona ("Moon Outer Corona Color", Vector) = (0.73,0.89,1,1)
		[HideInInspector] _colorCorrection ("Color Correction", Vector) = (1,1,0,0)
		[NoScaleOffset] _MoonSampler ("Moon", 2D) = "black" {}
		[HideInInspector] _OuterSpaceIntensity ("Outer Space Intensity", Range(0, 2)) = 0.25
		[NoScaleOffset] _OuterSpaceCube ("Outer Space Cubemap", Cube) = "black" {}
		_OffsetHeight ("offset Height", Range(0.06, 0.4)) = 0
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