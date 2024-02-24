using UnityEngine;

public class ShaderPropertyID
{
	public static readonly int _MainTex = Shader.PropertyToID("_MainTex");

	public static readonly int _MainTex_ST = Shader.PropertyToID("_MainTex_ST");

	public static readonly int _Color = Shader.PropertyToID("_Color");

	public static readonly int _Tint = Shader.PropertyToID("_Tint");

	public static readonly int _Amount = Shader.PropertyToID("_Amount");

	public static readonly int _AlphaStr = Shader.PropertyToID("_AlphaStr");

	public static readonly int _Cutoff = Shader.PropertyToID("_Cutoff");

	public static readonly int _SubConstructProgress = Shader.PropertyToID("_SubConstructProgress");

	public static readonly int _UwePowerLoss = Shader.PropertyToID("_UwePowerLoss");

	public static readonly int _LocalFloodLevel = Shader.PropertyToID("_LocalFloodLevel");

	public static readonly int _ClipedValue = Shader.PropertyToID("_ClipedValue");

	public static readonly int _minYpos = Shader.PropertyToID("_minYpos");

	public static readonly int _maxYpos = Shader.PropertyToID("_maxYpos");

	public static readonly int _Built = Shader.PropertyToID("_Built");

	public static readonly int _GlowColor = Shader.PropertyToID("_GlowColor");

	public static readonly int _ScaleModifier = Shader.PropertyToID("_ScaleModifier");

	public static readonly int _Intensity = Shader.PropertyToID("_Intensity");

	public static readonly int _Offset = Shader.PropertyToID("_Offset");

	public static readonly int _Fallof = Shader.PropertyToID("_Fallof");

	public static readonly int _Fill = Shader.PropertyToID("_Fill");

	public static readonly int _FabricatorPosY = Shader.PropertyToID("_FabricatorPosY");

	public static readonly int _EmissiveTex = Shader.PropertyToID("_EmissiveTex");

	public static readonly int _NoiseTex = Shader.PropertyToID("_NoiseTex");

	public static readonly int _TopBorder = Shader.PropertyToID("_TopBorder");

	public static readonly int _BottomBorder = Shader.PropertyToID("_BottomBorder");

	public static readonly int _Chroma = Shader.PropertyToID("_Chroma");

	public static readonly int _NotificationStrength = Shader.PropertyToID("_NotificationStrength");

	public static readonly int _OverlayTex = Shader.PropertyToID("_OverlayTex");

	public static readonly int _BorderColor = Shader.PropertyToID("_BorderColor");

	public static readonly int _BorderWidth = Shader.PropertyToID("_BorderWidth");

	public static readonly int _EdgeWidth = Shader.PropertyToID("_EdgeWidth");

	public static readonly int _Width = Shader.PropertyToID("_Width");

	public static readonly int _Value = Shader.PropertyToID("_Value");

	public static readonly int _Overlay1_ST = Shader.PropertyToID("_Overlay1_ST");

	public static readonly int _Overlay2_ST = Shader.PropertyToID("_Overlay2_ST");

	public static readonly int _OverlayShift = Shader.PropertyToID("_OverlayShift");

	public static readonly int _OverlayAlpha = Shader.PropertyToID("_OverlayAlpha");

	public static readonly int _FillRect = Shader.PropertyToID("_FillRect");

	public static readonly int _FillValue = Shader.PropertyToID("_FillValue");

	public static readonly int _SrcFactor = Shader.PropertyToID("_SrcFactor");

	public static readonly int _DstFactor = Shader.PropertyToID("_DstFactor");

	public static readonly int _SrcFactorA = Shader.PropertyToID("_SrcFactorA");

	public static readonly int _DstFactorA = Shader.PropertyToID("_DstFactorA");

	public static readonly int _AlphaPremultiply = Shader.PropertyToID("_AlphaPremultiply");

	public static readonly int _Angle = Shader.PropertyToID("_Angle");

	public static readonly int _AlphaFrom = Shader.PropertyToID("_AlphaFrom");

	public static readonly int _AlphaTo = Shader.PropertyToID("_AlphaTo");

	public static readonly int _BuildParams = Shader.PropertyToID("_BuildParams");

	public static readonly int _NoiseStr = Shader.PropertyToID("_NoiseStr");

	public static readonly int _NoiseThickness = Shader.PropertyToID("_NoiseThickness");

	public static readonly int _BuildLinear = Shader.PropertyToID("_BuildLinear");

	public static readonly int _MyCullVariable = Shader.PropertyToID("_MyCullVariable");

	public static readonly int _Size = Shader.PropertyToID("_Size");

	public static readonly int _Radius = Shader.PropertyToID("_Radius");

	public static readonly int _Subdivisions = Shader.PropertyToID("_Subdivisions");

	public static readonly int _Direction = Shader.PropertyToID("_Direction");

	public static readonly int _AffectedByDayNightCycle = Shader.PropertyToID("_AffectedByDayNightCycle");

	public static readonly int _Outdoors = Shader.PropertyToID("_Outdoors");

	public static readonly int _ExposureIBL = Shader.PropertyToID("_ExposureIBL");

	public static readonly int _TextureStep = Shader.PropertyToID("_TextureStep");

	public static readonly int _TextureOffset = Shader.PropertyToID("_TextureOffset");

	public static readonly int _PositionTex = Shader.PropertyToID("_PositionTex");

	public static readonly int _VelocityTex = Shader.PropertyToID("_VelocityTex");

	public static readonly int _TargetPositionTex = Shader.PropertyToID("_TargetPositionTex");

	public static readonly int _ForceAndSpeedTex = Shader.PropertyToID("_ForceAndSpeedTex");

	public static readonly int _RepulsorPos = Shader.PropertyToID("_RepulsorPos");

	public static readonly int _RepulseForce = Shader.PropertyToID("_RepulseForce");

	public static readonly int _SeaLevel = Shader.PropertyToID("_SeaLevel");

	public static readonly int _Gravity = Shader.PropertyToID("_Gravity");

	public static readonly int _TargetPositionScale = Shader.PropertyToID("_TargetPositionScale");

	public static readonly int _ForceAndSpeedRangeAndMin = Shader.PropertyToID("_ForceAndSpeedRangeAndMin");

	public static readonly int _SnapToTargetPosition = Shader.PropertyToID("_SnapToTargetPosition");

	public static readonly int _LocalToWorldMatrix = Shader.PropertyToID("_LocalToWorldMatrix");

	public static readonly int _BlurSize = Shader.PropertyToID("_BlurSize");

	public static readonly int _NoisePerChannel = Shader.PropertyToID("_NoisePerChannel");

	public static readonly int SelfBleedReduction = Shader.PropertyToID("SelfBleedReduction");

	public static readonly int HalfSampling = Shader.PropertyToID("HalfSampling");

	public static readonly int Orthographic = Shader.PropertyToID("Orthographic");

	public static readonly int _ShadeColorFromSun = Shader.PropertyToID("_ShadeColorFromSun");

	public static readonly int _ShadeColorFromSky = Shader.PropertyToID("_ShadeColorFromSky");

	public static readonly int g_SrcData = Shader.PropertyToID("g_SrcData");

	public static readonly int g_DstData = Shader.PropertyToID("g_DstData");

	public static readonly int _InputH0 = Shader.PropertyToID("_InputH0");

	public static readonly int _InputOmega = Shader.PropertyToID("_InputOmega");

	public static readonly int _OutputHt = Shader.PropertyToID("_OutputHt");

	public static readonly int _CameraInside = Shader.PropertyToID("_CameraInside");

	public static readonly int _DitherTexture = Shader.PropertyToID("_DitherTexture");

	public static readonly int PreserveDetails = Shader.PropertyToID("PreserveDetails");

	public static readonly int ShadeColorFromSun = Shader.PropertyToID("ShadeColorFromSun");

	public static readonly int ShadeColorFromSky = Shader.PropertyToID("ShadeColorFromSky");

	public static readonly int _SceneFogParams = Shader.PropertyToID("_SceneFogParams");

	public static readonly int _SceneFogMode = Shader.PropertyToID("_SceneFogMode");

	public static readonly int _fParams = Shader.PropertyToID("_fParams");

	public static readonly int _BoxMin = Shader.PropertyToID("_BoxMin");

	public static readonly int _BoxMax = Shader.PropertyToID("_BoxMax");

	public static readonly int _Visibility = Shader.PropertyToID("_Visibility");

	public static readonly int _UniformOcclusion = Shader.PropertyToID("_UniformOcclusion");

	public static readonly int _SkyMatrix = Shader.PropertyToID("_SkyMatrix");

	public static readonly int _CubeHDR = Shader.PropertyToID("_CubeHDR");

	public static readonly int _SpecularExp = Shader.PropertyToID("_SpecularExp");

	public static readonly int _SpecularScale = Shader.PropertyToID("_SpecularScale");

	public static readonly int _EmissionLM = Shader.PropertyToID("_EmissionLM");

	public static readonly int _BlendWeightIBL = Shader.PropertyToID("_BlendWeightIBL");

	public static readonly int _ImportantLog = Shader.PropertyToID("_ImportantLog");

	public static readonly int _ScanIntensity = Shader.PropertyToID("_ScanIntensity");

	public static readonly int _ScanFrequency = Shader.PropertyToID("_ScanFrequency");

	public static readonly int _MapCenterWorldPos = Shader.PropertyToID("_MapCenterWorldPos");

	public static readonly int _FadeRadius = Shader.PropertyToID("_FadeRadius");

	public static readonly int _FadeSharpness = Shader.PropertyToID("_FadeSharpness");

	public static readonly int _FillSack = Shader.PropertyToID("_FillSack");

	public static readonly int _GlowStrength = Shader.PropertyToID("_GlowStrength");

	public static readonly int _GlowStrengthNight = Shader.PropertyToID("_GlowStrengthNight");

	public static readonly int _Hypnotize = Shader.PropertyToID("_Hypnotize");

	public static readonly int _UweLightScalar = Shader.PropertyToID("_UweLightScalar");

	public static readonly int _AtmoColor = Shader.PropertyToID("_AtmoColor");

	public static readonly int _UweAtmoLightFade = Shader.PropertyToID("_UweAtmoLightFade");

	public static readonly int _UweLocalLightScalar = Shader.PropertyToID("_UweLocalLightScalar");

	public static readonly int _ImagePlaneSize = Shader.PropertyToID("_ImagePlaneSize");

	public static readonly int _CameraToWorldMatrix = Shader.PropertyToID("_CameraToWorldMatrix");

	public static readonly int _EdgeThresholdMin = Shader.PropertyToID("_EdgeThresholdMin");

	public static readonly int _EdgeThreshold = Shader.PropertyToID("_EdgeThreshold");

	public static readonly int _EdgeSharpness = Shader.PropertyToID("_EdgeSharpness");

	public static readonly int _OffsetScale = Shader.PropertyToID("_OffsetScale");

	public static readonly int _BlurRadius = Shader.PropertyToID("_BlurRadius");

	public static readonly int _Brightness = Shader.PropertyToID("_Brightness");

	public static readonly int _Gray = Shader.PropertyToID("_Gray");

	public static readonly int _EmissiveCut = Shader.PropertyToID("_EmissiveCut");

	public static readonly int _ImpactIntensity = Shader.PropertyToID("_ImpactIntensity");

	public static readonly int _ImpactPosition = Shader.PropertyToID("_ImpactPosition");

	public static readonly int _InfectionAmount = Shader.PropertyToID("_InfectionAmount");

	public static readonly int _InfectionAlbedomap = Shader.PropertyToID("_InfectionAlbedomap");

	public static readonly int _InfectionNormalMap = Shader.PropertyToID("_InfectionNormalMap");

	public static readonly int _UWE_CTime = Shader.PropertyToID("_UWE_CTime");

	public static readonly int _GlobalSeaLevel = Shader.PropertyToID("_GlobalSeaLevel");

	public static readonly int _Camera2World = Shader.PropertyToID("_Camera2World");

	public static readonly int _World2MainCamera = Shader.PropertyToID("_World2MainCamera");

	public static readonly int _CameraFOVDegs = Shader.PropertyToID("_CameraFOVDegs");

	public static readonly int _RandPhase = Shader.PropertyToID("_RandPhase");

	public static readonly int _UweVrFadeAmount = Shader.PropertyToID("_UweVrFadeAmount");

	public static readonly int _NoiseVolume = Shader.PropertyToID("_NoiseVolume");

	public static readonly int _DitherTex = Shader.PropertyToID("_DitherTex");

	public static readonly int _NoiseScale = Shader.PropertyToID("_NoiseScale");

	public static readonly int _NoiseSpeed = Shader.PropertyToID("_NoiseSpeed");

	public static readonly int _Diffusion = Shader.PropertyToID("_Diffusion");

	public static readonly int _AspectRatio = Shader.PropertyToID("_AspectRatio");

	public static readonly int _NoiseFactor = Shader.PropertyToID("_NoiseFactor");

	public static readonly int _time = Shader.PropertyToID("_time");

	public static readonly int _Strength = Shader.PropertyToID("_Strength");

	public static readonly int _ChromaticOffset = Shader.PropertyToID("_ChromaticOffset");

	public static readonly int lum = Shader.PropertyToID("lum");

	public static readonly int noiseFactor = Shader.PropertyToID("noiseFactor");

	public static readonly int time = Shader.PropertyToID("time");

	public static readonly int _imgHeight = Shader.PropertyToID("_imgHeight");

	public static readonly int _imgWidth = Shader.PropertyToID("_imgWidth");

	public static readonly int _SonarPingDistance = Shader.PropertyToID("_SonarPingDistance");

	public static readonly int _TransfuserLevel = Shader.PropertyToID("_TransfuserLevel");

	public static readonly int _DispTex = Shader.PropertyToID("_DispTex");

	public static readonly int _Displacement = Shader.PropertyToID("_Displacement");

	public static readonly int _BurstParams = Shader.PropertyToID("_BurstParams");

	public static readonly int _startTime = Shader.PropertyToID("_startTime");

	public static readonly int _RipplePos1 = Shader.PropertyToID("_RipplePos1");

	public static readonly int _DeformMap = Shader.PropertyToID("_DeformMap");

	public static readonly int _SizeRangeAndMin = Shader.PropertyToID("_SizeRangeAndMin");

	public static readonly int _DeltaTime = Shader.PropertyToID("_DeltaTime");

	public static readonly int _MaxDistanceFromCenter = Shader.PropertyToID("_MaxDistanceFromCenter");

	public static readonly int _AlphaPow = Shader.PropertyToID("_AlphaPow");

	public static readonly int _MaskTex = Shader.PropertyToID("_MaskTex");

	public static readonly int _MainTex2 = Shader.PropertyToID("_MainTex2");

	public static readonly int _InvFade = Shader.PropertyToID("_InvFade");

	public static readonly int _ClipFade = Shader.PropertyToID("_ClipFade");

	public static readonly int _SunColor = Shader.PropertyToID("_SunColor");

	public static readonly int _CloudsColor = Shader.PropertyToID("_CloudsColor");

	public static readonly int _cloudAmount = Shader.PropertyToID("_cloudAmount");

	public static readonly int _sunAmount = Shader.PropertyToID("_sunAmount");

	public static readonly int _rainAmount = Shader.PropertyToID("_rainAmount");

	public static readonly int _snowAmount = Shader.PropertyToID("_snowAmount");

	public static readonly int _EmissiveStrengh = Shader.PropertyToID("_EmissiveStrengh");

	public static readonly int _horizonFogAmount = Shader.PropertyToID("_horizonFogAmount");

	public static readonly int ProjectionMatrixInverse = Shader.PropertyToID("ProjectionMatrixInverse");

	public static readonly int _ColorDownsampled = Shader.PropertyToID("_ColorDownsampled");

	public static readonly int Radius = Shader.PropertyToID("Radius");

	public static readonly int Bias = Shader.PropertyToID("Bias");

	public static readonly int DepthTolerance = Shader.PropertyToID("DepthTolerance");

	public static readonly int ZThickness = Shader.PropertyToID("ZThickness");

	public static readonly int Intensity = Shader.PropertyToID("Intensity");

	public static readonly int SampleDistributionCurve = Shader.PropertyToID("SampleDistributionCurve");

	public static readonly int ColorBleedAmount = Shader.PropertyToID("ColorBleedAmount");

	public static readonly int DrawDistance = Shader.PropertyToID("DrawDistance");

	public static readonly int DrawDistanceFadeSize = Shader.PropertyToID("DrawDistanceFadeSize");

	public static readonly int BrightnessThreshold = Shader.PropertyToID("BrightnessThreshold");

	public static readonly int Downsamp = Shader.PropertyToID("Downsamp");

	public static readonly int BlurDepthTolerance = Shader.PropertyToID("BlurDepthTolerance");

	public static readonly int Near = Shader.PropertyToID("Near");

	public static readonly int Far = Shader.PropertyToID("Far");

	public static readonly int Kernel = Shader.PropertyToID("Kernel");

	public static readonly int _SSAO = Shader.PropertyToID("_SSAO");

	public static readonly int _DistanceFieldTexture = Shader.PropertyToID("_DistanceFieldTexture");

	public static readonly int _DistanceFieldMin = Shader.PropertyToID("_DistanceFieldMin");

	public static readonly int _DistanceFieldSizeRcp = Shader.PropertyToID("_DistanceFieldSizeRcp");

	public static readonly int _DistanceFieldScale = Shader.PropertyToID("_DistanceFieldScale");

	public static readonly int _ObjectScale = Shader.PropertyToID("_ObjectScale");

	public static readonly int _WaterDisplacementTexture = Shader.PropertyToID("_WaterDisplacementTexture");

	public static readonly int _WaterPatchLength = Shader.PropertyToID("_WaterPatchLength");

	public static readonly int _Exposure = Shader.PropertyToID("_Exposure");

	public static readonly int thread_count = Shader.PropertyToID("thread_count");

	public static readonly int istride = Shader.PropertyToID("istride");

	public static readonly int ostride = Shader.PropertyToID("ostride");

	public static readonly int pstride = Shader.PropertyToID("pstride");

	public static readonly int phase_base = Shader.PropertyToID("phase_base");

	public static readonly int _NoiseTexture = Shader.PropertyToID("_NoiseTexture");

	public static readonly int _FlowSpeed = Shader.PropertyToID("_FlowSpeed");

	public static readonly int _UweExtinctionTexture = Shader.PropertyToID("_UweExtinctionTexture");

	public static readonly int _UweScatteringTexture = Shader.PropertyToID("_UweScatteringTexture");

	public static readonly int _UweEmissiveTexture = Shader.PropertyToID("_UweEmissiveTexture");

	public static readonly int _UweVolumeTextureSlices = Shader.PropertyToID("_UweVolumeTextureSlices");

	public static readonly int _TextureSize = Shader.PropertyToID("_TextureSize");

	public static readonly int _InvTextureSize = Shader.PropertyToID("_InvTextureSize");

	public static readonly int _TextureMinIndex = Shader.PropertyToID("_TextureMinIndex");

	public static readonly int _SrcTextureSize = Shader.PropertyToID("_SrcTextureSize");

	public static readonly int _DstTextureSize = Shader.PropertyToID("_DstTextureSize");

	public static readonly int _InvDstTextureSize = Shader.PropertyToID("_InvDstTextureSize");

	public static readonly int _InvNumBiomes = Shader.PropertyToID("_InvNumBiomes");

	public static readonly int _Modifier = Shader.PropertyToID("_Modifier");

	public static readonly int _BiomeValueTex = Shader.PropertyToID("_BiomeValueTex");

	public static readonly int _MetersPerPixel = Shader.PropertyToID("_MetersPerPixel");

	public static readonly int _WorldToLocalMatrix = Shader.PropertyToID("_WorldToLocalMatrix");

	public static readonly int _SphereRadius = Shader.PropertyToID("_SphereRadius");

	public static readonly int _BoxExtents = Shader.PropertyToID("_BoxExtents");

	public static readonly int _CapsuleRadiusExtent = Shader.PropertyToID("_CapsuleRadiusExtent");

	public static readonly int _Value1 = Shader.PropertyToID("_Value1");

	public static readonly int _Value2 = Shader.PropertyToID("_Value2");

	public static readonly int _UweCameraToVolumeMatrix = Shader.PropertyToID("_UweCameraToVolumeMatrix");

	public static readonly int _UweWorldToVolumeMatrix = Shader.PropertyToID("_UweWorldToVolumeMatrix");

	public static readonly int _WBOITATexture = Shader.PropertyToID("_WBOITATexture");

	public static readonly int _WBOITBTexture = Shader.PropertyToID("_WBOITBTexture");

	public static readonly int _UnderWaterBoost = Shader.PropertyToID("_UnderWaterBoost");

	public static readonly int _UnderWaterSkyBrightness = Shader.PropertyToID("_UnderWaterSkyBrightness");

	public static readonly int _BlurParams = Shader.PropertyToID("_BlurParams");

	public static readonly int _Offsets = Shader.PropertyToID("_Offsets");

	public static readonly int _LowRez = Shader.PropertyToID("_LowRez");

	public static readonly int _WrapSize = Shader.PropertyToID("_WrapSize");

	public static readonly int _NormalsTex = Shader.PropertyToID("_NormalsTex");

	public static readonly int _TexelLength2 = Shader.PropertyToID("_TexelLength2");

	public static readonly int _ProjectionDepth = Shader.PropertyToID("_ProjectionDepth");

	public static readonly int _RefractionIndex = Shader.PropertyToID("_RefractionIndex");

	public static readonly int _WaveTime = Shader.PropertyToID("_WaveTime");

	public static readonly int _WaveStart = Shader.PropertyToID("_WaveStart");

	public static readonly int _WaveLength = Shader.PropertyToID("_WaveLength");

	public static readonly int _ActualDim = Shader.PropertyToID("_ActualDim");

	public static readonly int _InWidth = Shader.PropertyToID("_InWidth");

	public static readonly int _OutWidth = Shader.PropertyToID("_OutWidth");

	public static readonly int _OutHeight = Shader.PropertyToID("_OutHeight");

	public static readonly int _DtxAddressOffset = Shader.PropertyToID("_DtxAddressOffset");

	public static readonly int _DtyAddressOffset = Shader.PropertyToID("_DtyAddressOffset");

	public static readonly int _Time = Shader.PropertyToID("_Time");

	public static readonly int _UnscaledTime = Shader.PropertyToID("_UnscaledTime");

	public static readonly int _PDATime = Shader.PropertyToID("_PDATime");

	public static readonly int _ChoppyScale = Shader.PropertyToID("_ChoppyScale");

	public static readonly int _DxAddressOffset = Shader.PropertyToID("_DxAddressOffset");

	public static readonly int _DyAddressOffset = Shader.PropertyToID("_DyAddressOffset");

	public static readonly int _InputDxyz = Shader.PropertyToID("_InputDxyz");

	public static readonly int _CausticsTexture = Shader.PropertyToID("_CausticsTexture");

	public static readonly int _StartDistance = Shader.PropertyToID("_StartDistance");

	public static readonly int _MaxDistance = Shader.PropertyToID("_MaxDistance");

	public static readonly int _ShaftsScale = Shader.PropertyToID("_ShaftsScale");

	public static readonly int _CameraToCausticsMatrix = Shader.PropertyToID("_CameraToCausticsMatrix");

	public static readonly int _OriginalTex = Shader.PropertyToID("_OriginalTex");

	public static readonly int _WaterDisplacementMap = Shader.PropertyToID("_WaterDisplacementMap");

	public static readonly int _Refraction0 = Shader.PropertyToID("_Refraction0");

	public static readonly int _ReflectionColor = Shader.PropertyToID("_ReflectionColor");

	public static readonly int _RefractionColor = Shader.PropertyToID("_RefractionColor");

	public static readonly int _RefractionTexture = Shader.PropertyToID("_RefractionTexture");

	public static readonly int _FoamTexture = Shader.PropertyToID("_FoamTexture");

	public static readonly int _FoamMaskTexture = Shader.PropertyToID("_FoamMaskTexture");

	public static readonly int _FoamSmoothing = Shader.PropertyToID("_FoamSmoothing");

	public static readonly int _FoamAmountTexture = Shader.PropertyToID("_FoamAmountTexture");

	public static readonly int _FoamScale = Shader.PropertyToID("_FoamScale");

	public static readonly int _FoamDistance = Shader.PropertyToID("_FoamDistance");

	public static readonly int _SubSurfaceFoamColor = Shader.PropertyToID("_SubSurfaceFoamColor");

	public static readonly int _SubSurfaceFoamScale = Shader.PropertyToID("_SubSurfaceFoamScale");

	public static readonly int _FoamAmountMultiplier = Shader.PropertyToID("_FoamAmountMultiplier");

	public static readonly int _BackLightTint = Shader.PropertyToID("_BackLightTint");

	public static readonly int _WaveHeightThicknessScale = Shader.PropertyToID("_WaveHeightThicknessScale");

	public static readonly int _ClipTexture = Shader.PropertyToID("_ClipTexture");

	public static readonly int _SunReflectionGloss = Shader.PropertyToID("_SunReflectionGloss");

	public static readonly int _SunReflectionAmount = Shader.PropertyToID("_SunReflectionAmount");

	public static readonly int _ScreenSpaceRefractionRatio = Shader.PropertyToID("_ScreenSpaceRefractionRatio");

	public static readonly int _ScreenSpaceInternalReflectionFlatness = Shader.PropertyToID("_ScreenSpaceInternalReflectionFlatness");

	public static readonly int _ZWriteMode = Shader.PropertyToID("_ZWriteMode");

	public static readonly int _PixelStride = Shader.PropertyToID("_PixelStride");

	public static readonly int _PixelStrideZCuttoff = Shader.PropertyToID("_PixelStrideZCuttoff");

	public static readonly int _PixelZSize = Shader.PropertyToID("_PixelZSize");

	public static readonly int _Iterations = Shader.PropertyToID("_Iterations");

	public static readonly int _BinarySearchIterations = Shader.PropertyToID("_BinarySearchIterations");

	public static readonly int _MaxRayDistance = Shader.PropertyToID("_MaxRayDistance");

	public static readonly int _ScreenEdgeFadeStart = Shader.PropertyToID("_ScreenEdgeFadeStart");

	public static readonly int _EyeFadeStart = Shader.PropertyToID("_EyeFadeStart");

	public static readonly int _EyeFadeEnd = Shader.PropertyToID("_EyeFadeEnd");

	public static readonly int _UnderWaterRefraction = Shader.PropertyToID("_UnderWaterRefraction");

	public static readonly int _WorldToClipMatrix = Shader.PropertyToID("_WorldToClipMatrix");

	public static readonly int _MeanSkyColor = Shader.PropertyToID("_MeanSkyColor");

	public static readonly int _SkyMap = Shader.PropertyToID("_SkyMap");

	public static readonly int _DisplacementLoadScale = Shader.PropertyToID("_DisplacementLoadScale");

	public static readonly int _DisplacementLoadOffset = Shader.PropertyToID("_DisplacementLoadOffset");

	public static readonly int _Frame0Tex = Shader.PropertyToID("_Frame0Tex");

	public static readonly int _Frame1Tex = Shader.PropertyToID("_Frame1Tex");

	public static readonly int _Frame2Tex = Shader.PropertyToID("_Frame2Tex");

	public static readonly int _Frame3Tex = Shader.PropertyToID("_Frame3Tex");

	public static readonly int _Fraction = Shader.PropertyToID("_Fraction");

	public static readonly int _DisplacementTex = Shader.PropertyToID("_DisplacementTex");

	public static readonly int _DisplacementStoreScale = Shader.PropertyToID("_DisplacementStoreScale");

	public static readonly int _DisplacementStoreOffset = Shader.PropertyToID("_DisplacementStoreOffset");

	public static readonly int _Texture = Shader.PropertyToID("_Texture");

	public static readonly int _Scale = Shader.PropertyToID("_Scale");

	public static readonly int _DisplacementTexel = Shader.PropertyToID("_DisplacementTexel");

	public static readonly int _FoamDecay = Shader.PropertyToID("_FoamDecay");

	public static readonly int _FoamRate = Shader.PropertyToID("_FoamRate");

	public static readonly int _RenderBufferSize = Shader.PropertyToID("_RenderBufferSize");

	public static readonly int _OneDividedByRenderBufferSize = Shader.PropertyToID("_OneDividedByRenderBufferSize");

	public static readonly int _CameraProjectionMatrix = Shader.PropertyToID("_CameraProjectionMatrix");

	public static readonly int _CameraInverseProjectionMatrix = Shader.PropertyToID("_CameraInverseProjectionMatrix");

	public static readonly int _UweFogEnabled = Shader.PropertyToID("_UweFogEnabled");

	public static readonly int _UweAerialFogEnabled = Shader.PropertyToID("_UweAerialFogEnabled");

	public static readonly int _UweVsWaterPlane = Shader.PropertyToID("_UweVsWaterPlane");

	public static readonly int _UweCausticsScale = Shader.PropertyToID("_UweCausticsScale");

	public static readonly int _UweCausticsAmount = Shader.PropertyToID("_UweCausticsAmount");

	public static readonly int _UweWaterTransmission = Shader.PropertyToID("_UweWaterTransmission");

	public static readonly int _UweWaterEmissionAmbientScale = Shader.PropertyToID("_UweWaterEmissionAmbientScale");

	public static readonly int _UweExtinctionAndScatteringScale = Shader.PropertyToID("_UweExtinctionAndScatteringScale");

	public static readonly int _UweFogVsLightDirection = Shader.PropertyToID("_UweFogVsLightDirection");

	public static readonly int _UweFogWsLightDirection = Shader.PropertyToID("_UweFogWsLightDirection");

	public static readonly int _UweFogLightColor = Shader.PropertyToID("_UweFogLightColor");

	public static readonly int _UweFogLightGreyscaleColor = Shader.PropertyToID("_UweFogLightGreyscaleColor");

	public static readonly int _UweFogLightAmount = Shader.PropertyToID("_UweFogLightAmount");

	public static readonly int _UweColorCastFactor = Shader.PropertyToID("_UweColorCastFactor");

	public static readonly int _UweAboveWaterFogStartDistance = Shader.PropertyToID("_UweAboveWaterFogStartDistance");

	public static readonly int _UweFogMiePhaseConst = Shader.PropertyToID("_UweFogMiePhaseConst");

	public static readonly int _UweSunAttenuationFactor = Shader.PropertyToID("_UweSunAttenuationFactor");

	public static readonly int _NotificationOverlayTex = Shader.PropertyToID("_NotificationOverlayTex");

	public static readonly int _HorizonOffset = Shader.PropertyToID("_HorizonOffset");

	public static readonly int _FrustumCornersWS = Shader.PropertyToID("_FrustumCornersWS");

	public static readonly int _CameraWS = Shader.PropertyToID("_CameraWS");

	public static readonly int _UweBottomAmbientColor = Shader.PropertyToID("_UweBottomAmbientColor");

	public static readonly int _UweTopAmbientColor = Shader.PropertyToID("_UweTopAmbientColor");

	public static readonly int _betaR = Shader.PropertyToID("_betaR");

	public static readonly int _betaM = Shader.PropertyToID("_betaM");

	public static readonly int _SunBurstTexture = Shader.PropertyToID("_SunBurstTexture");

	public static readonly int _mieConst = Shader.PropertyToID("_mieConst");

	public static readonly int _miePhase_g = Shader.PropertyToID("_miePhase_g");

	public static readonly int _GroundColor = Shader.PropertyToID("_GroundColor");

	public static readonly int _NightHorizonColor = Shader.PropertyToID("_NightHorizonColor");

	public static readonly int _NightZenithColor = Shader.PropertyToID("_NightZenithColor");

	public static readonly int _MoonInnerCorona = Shader.PropertyToID("_MoonInnerCorona");

	public static readonly int _MoonOuterCorona = Shader.PropertyToID("_MoonOuterCorona");

	public static readonly int _MoonSize = Shader.PropertyToID("_MoonSize");

	public static readonly int _colorCorrection = Shader.PropertyToID("_colorCorrection");

	public static readonly int _SunSize = Shader.PropertyToID("_SunSize");

	public static readonly int _PlanetRadius = Shader.PropertyToID("_PlanetRadius");

	public static readonly int _PlanetTexture = Shader.PropertyToID("_PlanetTexture");

	public static readonly int _PlanetNormalMap = Shader.PropertyToID("_PlanetNormalMap");

	public static readonly int _PlanetRimColor = Shader.PropertyToID("_PlanetRimColor");

	public static readonly int _PlanetAmbientLight = Shader.PropertyToID("_PlanetAmbientLight");

	public static readonly int _PlanetLightWrap = Shader.PropertyToID("_PlanetLightWrap");

	public static readonly int _PlanetInnerCorona = Shader.PropertyToID("_PlanetInnerCorona");

	public static readonly int _PlanetOuterCorona = Shader.PropertyToID("_PlanetOuterCorona");

	public static readonly int _MoonSampler = Shader.PropertyToID("_MoonSampler");

	public static readonly int _CloudsTexture = Shader.PropertyToID("_CloudsTexture");

	public static readonly int _AuroraColorGradient = Shader.PropertyToID("_AuroraColorGradient");

	public static readonly int _AuroraTexture = Shader.PropertyToID("_AuroraTexture");

	public static readonly int _AuroraTextureScale = Shader.PropertyToID("_AuroraTextureScale");

	public static readonly int _AuroraScrollSpeed = Shader.PropertyToID("_AuroraScrollSpeed");

	public static readonly int _AuroraIntensity = Shader.PropertyToID("_AuroraIntensity");

	public static readonly int _AuroraAlphaSaturation = Shader.PropertyToID("_AuroraAlphaSaturation");

	public static readonly int _AuroraDeformTexture = Shader.PropertyToID("_AuroraDeformTexture");

	public static readonly int _AuroraDeformIntensity = Shader.PropertyToID("_AuroraDeformIntensity");

	public static readonly int _CloudsAlphaSaturation = Shader.PropertyToID("_CloudsAlphaSaturation");

	public static readonly int _SunColorMultiplier = Shader.PropertyToID("_SunColorMultiplier");

	public static readonly int _SkyColorMultiplier = Shader.PropertyToID("_SkyColorMultiplier");

	public static readonly int _CloudsAttenuation = Shader.PropertyToID("_CloudsAttenuation");

	public static readonly int _CloudsScatteringMultiplier = Shader.PropertyToID("_CloudsScatteringMultiplier");

	public static readonly int _CloudsScatteringExponent = Shader.PropertyToID("_CloudsScatteringExponent");

	public static readonly int _EndSequenceSunBurstColor = Shader.PropertyToID("_EndSequenceSunBurstColor");

	public static readonly int _WorldToMoonMatrix = Shader.PropertyToID("_WorldToMoonMatrix");

	public static readonly int _RocketMatrix = Shader.PropertyToID("_RocketMatrix");

	public static readonly int _SkyMultiplier = Shader.PropertyToID("_SkyMultiplier");

	public static readonly int _PlanetPos = Shader.PropertyToID("_PlanetPos");

	public static readonly int _WorldToSpaceMatrix = Shader.PropertyToID("_WorldToSpaceMatrix");

	public static readonly int StarIntensity = Shader.PropertyToID("StarIntensity");

	public static readonly int _SpaceTransition = Shader.PropertyToID("_SpaceTransition");

	public static readonly int rotationMatrix = Shader.PropertyToID("rotationMatrix");

	public static readonly int _WorldToCloudsMatrix = Shader.PropertyToID("_WorldToCloudsMatrix");

	public static readonly int _SecondaryLightDir = Shader.PropertyToID("_SecondaryLightDir");

	public static readonly int _SecondaryLightColor = Shader.PropertyToID("_SecondaryLightColor");

	public static readonly int _SecondaryLightPow = Shader.PropertyToID("_SecondaryLightPow");

	public static readonly int _SunDir = Shader.PropertyToID("_SunDir");

	public static readonly int _SkyFogDensity = Shader.PropertyToID("_SkyFogDensity");

	public static readonly int _SkyFogColor = Shader.PropertyToID("_SkyFogColor");

	public static readonly int _Eclipse = Shader.PropertyToID("_Eclipse");

	public static readonly int _SrcBlend = Shader.PropertyToID("_SrcBlend");

	public static readonly int _DstBlend = Shader.PropertyToID("_DstBlend");

	public static readonly int _ColorRampOffset = Shader.PropertyToID("_ColorRampOffset");

	public static readonly int _Gloss = Shader.PropertyToID("_Gloss");

	public static readonly int _ZWrite = Shader.PropertyToID("_ZWrite");

	public static readonly int _ColorMask = Shader.PropertyToID("_ColorMask");

	public static readonly int _BlendSrcFactor = Shader.PropertyToID("_BlendSrcFactor");

	public static readonly int _BlendDstFactor = Shader.PropertyToID("_BlendDstFactor");

	public static readonly int _IsOpaque = Shader.PropertyToID("_IsOpaque");

	public static readonly int _AlphaTestValue = Shader.PropertyToID("_AlphaTestValue");

	public static readonly int _SpecColor = Shader.PropertyToID("_SpecColor");

	public static readonly int _Threshhold = Shader.PropertyToID("_Threshhold");

	public static readonly int _TintColor = Shader.PropertyToID("_TintColor");

	public static readonly int _Saturation = Shader.PropertyToID("_Saturation");

	public static readonly int _StretchWidth = Shader.PropertyToID("_StretchWidth");

	public static readonly int _ColorBuffer = Shader.PropertyToID("_ColorBuffer");

	public static readonly int colorA = Shader.PropertyToID("colorA");

	public static readonly int colorB = Shader.PropertyToID("colorB");

	public static readonly int colorC = Shader.PropertyToID("colorC");

	public static readonly int colorD = Shader.PropertyToID("colorD");

	public static readonly int offsets = Shader.PropertyToID("offsets");

	public static readonly int _threshold = Shader.PropertyToID("_threshold");

	public static readonly int tintColor = Shader.PropertyToID("tintColor");

	public static readonly int stretchWidth = Shader.PropertyToID("stretchWidth");

	public static readonly int threshold = Shader.PropertyToID("threshold");

	public static readonly int useSrcAlphaAsMask = Shader.PropertyToID("useSrcAlphaAsMask");

	public static readonly int vignetteIntensity = Shader.PropertyToID("vignetteIntensity");

	public static readonly int _Parameter = Shader.PropertyToID("_Parameter");

	public static readonly int _Bloom = Shader.PropertyToID("_Bloom");

	public static readonly int _InvViewProj = Shader.PropertyToID("_InvViewProj");

	public static readonly int _PrevViewProj = Shader.PropertyToID("_PrevViewProj");

	public static readonly int _ToPrevViewProjCombined = Shader.PropertyToID("_ToPrevViewProjCombined");

	public static readonly int _MaxVelocity = Shader.PropertyToID("_MaxVelocity");

	public static readonly int _MaxRadiusOrKInPaper = Shader.PropertyToID("_MaxRadiusOrKInPaper");

	public static readonly int _MinVelocity = Shader.PropertyToID("_MinVelocity");

	public static readonly int _VelocityScale = Shader.PropertyToID("_VelocityScale");

	public static readonly int _Jitter = Shader.PropertyToID("_Jitter");

	public static readonly int _VelTex = Shader.PropertyToID("_VelTex");

	public static readonly int _NeighbourMaxTex = Shader.PropertyToID("_NeighbourMaxTex");

	public static readonly int _TileTexDebug = Shader.PropertyToID("_TileTexDebug");

	public static readonly int _BlurDirectionPacked = Shader.PropertyToID("_BlurDirectionPacked");

	public static readonly int _DisplayVelocityScale = Shader.PropertyToID("_DisplayVelocityScale");

	public static readonly int _SoftZDistance = Shader.PropertyToID("_SoftZDistance");

	public static readonly int _RgbTex = Shader.PropertyToID("_RgbTex");

	public static readonly int _ZCurve = Shader.PropertyToID("_ZCurve");

	public static readonly int _RgbDepthTex = Shader.PropertyToID("_RgbDepthTex");

	public static readonly int selColor = Shader.PropertyToID("selColor");

	public static readonly int targetColor = Shader.PropertyToID("targetColor");

	public static readonly int _ClutTex = Shader.PropertyToID("_ClutTex");

	public static readonly int _RampTex = Shader.PropertyToID("_RampTex");

	public static readonly int _MainTexBlurred = Shader.PropertyToID("_MainTexBlurred");

	public static readonly int intensity = Shader.PropertyToID("intensity");

	public static readonly int threshhold = Shader.PropertyToID("threshhold");

	public static readonly int _AdaptTex = Shader.PropertyToID("_AdaptTex");

	public static readonly int _CurTex = Shader.PropertyToID("_CurTex");

	public static readonly int _AdaptParams = Shader.PropertyToID("_AdaptParams");

	public static readonly int _HrDepthTex = Shader.PropertyToID("_HrDepthTex");

	public static readonly int _LrDepthTex = Shader.PropertyToID("_LrDepthTex");

	public static readonly int _FgOverlap = Shader.PropertyToID("_FgOverlap");

	public static readonly int _CurveParams = Shader.PropertyToID("_CurveParams");

	public static readonly int _BlurredColor = Shader.PropertyToID("_BlurredColor");

	public static readonly int _SpawnHeuristic = Shader.PropertyToID("_SpawnHeuristic");

	public static readonly int _BokehParams = Shader.PropertyToID("_BokehParams");

	public static readonly int pointBuffer = Shader.PropertyToID("pointBuffer");

	public static readonly int _Screen = Shader.PropertyToID("_Screen");

	public static readonly int _FgCocMask = Shader.PropertyToID("_FgCocMask");

	public static readonly int _ForegroundBlurExtrude = Shader.PropertyToID("_ForegroundBlurExtrude");

	public static readonly int _InvRenderTargetSize = Shader.PropertyToID("_InvRenderTargetSize");

	public static readonly int _TapLow = Shader.PropertyToID("_TapLow");

	public static readonly int _TapMedium = Shader.PropertyToID("_TapMedium");

	public static readonly int _TapLowBackground = Shader.PropertyToID("_TapLowBackground");

	public static readonly int _TapLowForeground = Shader.PropertyToID("_TapLowForeground");

	public static readonly int _TapHigh = Shader.PropertyToID("_TapHigh");

	public static readonly int _Source = Shader.PropertyToID("_Source");

	public static readonly int _ArScale = Shader.PropertyToID("_ArScale");

	public static readonly int _Sensitivity = Shader.PropertyToID("_Sensitivity");

	public static readonly int _BgFade = Shader.PropertyToID("_BgFade");

	public static readonly int _SampleDistance = Shader.PropertyToID("_SampleDistance");

	public static readonly int _BgColor = Shader.PropertyToID("_BgColor");

	public static readonly int _Exponent = Shader.PropertyToID("_Exponent");

	public static readonly int _Threshold = Shader.PropertyToID("_Threshold");

	public static readonly int _HeightParams = Shader.PropertyToID("_HeightParams");

	public static readonly int _DistanceParams = Shader.PropertyToID("_DistanceParams");

	public static readonly int _RampOffset = Shader.PropertyToID("_RampOffset");

	public static readonly int _EffectAmount = Shader.PropertyToID("_EffectAmount");

	public static readonly int _RotationMatrix = Shader.PropertyToID("_RotationMatrix");

	public static readonly int _CenterRadius = Shader.PropertyToID("_CenterRadius");

	public static readonly int _AccumOrig = Shader.PropertyToID("_AccumOrig");

	public static readonly int _DX11NoiseTime = Shader.PropertyToID("_DX11NoiseTime");

	public static readonly int _MidGrey = Shader.PropertyToID("_MidGrey");

	public static readonly int _NoiseAmount = Shader.PropertyToID("_NoiseAmount");

	public static readonly int _GrainTex = Shader.PropertyToID("_GrainTex");

	public static readonly int _ScratchTex = Shader.PropertyToID("_ScratchTex");

	public static readonly int _GrainOffsetScale = Shader.PropertyToID("_GrainOffsetScale");

	public static readonly int _ScratchOffsetScale = Shader.PropertyToID("_ScratchOffsetScale");

	public static readonly int _UV_Transform = Shader.PropertyToID("_UV_Transform");

	public static readonly int _Overlay = Shader.PropertyToID("_Overlay");

	public static readonly int _ProjInfo = Shader.PropertyToID("_ProjInfo");

	public static readonly int _ProjectionInv = Shader.PropertyToID("_ProjectionInv");

	public static readonly int _Rand = Shader.PropertyToID("_Rand");

	public static readonly int _Radius2 = Shader.PropertyToID("_Radius2");

	public static readonly int _BlurFilterDistance = Shader.PropertyToID("_BlurFilterDistance");

	public static readonly int _Axis = Shader.PropertyToID("_Axis");

	public static readonly int _AOTex = Shader.PropertyToID("_AOTex");

	public static readonly int _RandomTexture = Shader.PropertyToID("_RandomTexture");

	public static readonly int _FarCorner = Shader.PropertyToID("_FarCorner");

	public static readonly int _Params = Shader.PropertyToID("_Params");

	public static readonly int _TexelOffsetScale = Shader.PropertyToID("_TexelOffsetScale");

	public static readonly int _BlurRadius4 = Shader.PropertyToID("_BlurRadius4");

	public static readonly int _SunPosition = Shader.PropertyToID("_SunPosition");

	public static readonly int _SunThreshold = Shader.PropertyToID("_SunThreshold");

	public static readonly int _Skybox = Shader.PropertyToID("_Skybox");

	public static readonly int _BlurArea = Shader.PropertyToID("_BlurArea");

	public static readonly int _Blurred = Shader.PropertyToID("_Blurred");

	public static readonly int _RangeScale = Shader.PropertyToID("_RangeScale");

	public static readonly int _Curve = Shader.PropertyToID("_Curve");

	public static readonly int _ExposureAdjustment = Shader.PropertyToID("_ExposureAdjustment");

	public static readonly int _AdaptionSpeed = Shader.PropertyToID("_AdaptionSpeed");

	public static readonly int _HdrParams = Shader.PropertyToID("_HdrParams");

	public static readonly int _SmallTex = Shader.PropertyToID("_SmallTex");

	public static readonly int _Blur = Shader.PropertyToID("_Blur");

	public static readonly int _VignetteTex = Shader.PropertyToID("_VignetteTex");

	public static readonly int _ChromaticAberration = Shader.PropertyToID("_ChromaticAberration");

	public static readonly int _AxialAberration = Shader.PropertyToID("_AxialAberration");

	public static readonly int _BlurDistance = Shader.PropertyToID("_BlurDistance");

	public static readonly int _Luminance = Shader.PropertyToID("_Luminance");

	public static readonly int _BiomeMapTex = Shader.PropertyToID("_BiomeMapTex");

	public static readonly int _SrcBlend2 = Shader.PropertyToID("_SrcBlend2");

	public static readonly int _DstBlend2 = Shader.PropertyToID("_DstBlend2");

	public static readonly int _Mode = Shader.PropertyToID("_Mode");

	public static readonly int _VertexDebugMode = Shader.PropertyToID("_VertexDebugMode");

	public static readonly int _VertexDebugChannel = Shader.PropertyToID("_VertexDebugChannel");

	public static readonly int _VertexDebugScale = Shader.PropertyToID("_VertexDebugScale");

	public static readonly int RenderType = Shader.PropertyToID("RenderType");

	public static readonly int _BumpMap = Shader.PropertyToID("_BumpMap");

	public static readonly int _DetailNormalMap = Shader.PropertyToID("_DetailNormalMap");

	public static readonly int _SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");

	public static readonly int _MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");

	public static readonly int _ParallaxMap = Shader.PropertyToID("_ParallaxMap");

	public static readonly int _DetailAlbedoMap = Shader.PropertyToID("_DetailAlbedoMap");

	public static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");

	public static readonly int _UWE_EditorTime = Shader.PropertyToID("_UWE_EditorTime");

	public static readonly int _MarmoSpecEnum = Shader.PropertyToID("_MarmoSpecEnum");

	public static readonly int _EnableCutOff = Shader.PropertyToID("_EnableCutOff");

	public static readonly int _EnableSimpleGlass = Shader.PropertyToID("_EnableSimpleGlass");

	public static readonly int _EnableGlow = Shader.PropertyToID("_EnableGlow");

	public static readonly int _GlowUVfromVC = Shader.PropertyToID("_GlowUVfromVC");

	public static readonly int _EnableDetailMaps = Shader.PropertyToID("_EnableDetailMaps");

	public static readonly int _EnableLightmap = Shader.PropertyToID("_EnableLightmap");

	public static readonly int _Enable3Color = Shader.PropertyToID("_Enable3Color");

	public static readonly int FX = Shader.PropertyToID("FX");

	public static readonly int FX_Vertex = Shader.PropertyToID("FX_Vertex");

	public static readonly int _AddSrcBlend = Shader.PropertyToID("_AddSrcBlend");

	public static readonly int _AddDstBlend = Shader.PropertyToID("_AddDstBlend");

	public static readonly int _AddSrcBlend2 = Shader.PropertyToID("_AddSrcBlend2");

	public static readonly int _AddDstBlend2 = Shader.PropertyToID("_AddDstBlend2");

	public static readonly int _SpecTex = Shader.PropertyToID("_SpecTex");

	public static readonly int _EnableSIG = Shader.PropertyToID("_EnableSIG");

	public static readonly int _CapSIGMap = Shader.PropertyToID("_CapSIGMap");

	public static readonly int _SideSIGMap = Shader.PropertyToID("_SideSIGMap");

	public static readonly int _SIGMap = Shader.PropertyToID("_SIGMap");

	public static readonly int _Illum = Shader.PropertyToID("_Illum");

	public static readonly int FX_LightMode = Shader.PropertyToID("FX_LightMode");

	public static readonly int _WsHeadPosition = Shader.PropertyToID("_WsHeadPosition");

	public static readonly int _MinDistance = Shader.PropertyToID("_MinDistance");

	public static readonly int _FalloffPower = Shader.PropertyToID("_FalloffPower");

	public static readonly int _VrFadeMask = Shader.PropertyToID("_VrFadeMask");

	public static readonly int _OnePixel = Shader.PropertyToID("_OnePixel");

	public static readonly int _NoiseTilingPerChannel = Shader.PropertyToID("_NoiseTilingPerChannel");

	public static readonly int _CascadeIndex = Shader.PropertyToID("_CascadeIndex");

	public static readonly int _LightTraceStep = Shader.PropertyToID("_LightTraceStep");

	public static readonly int _AutoExposure = Shader.PropertyToID("_AutoExposure");

	public static readonly int _Uwe_BlueNoiseMap = Shader.PropertyToID("_Uwe_BlueNoiseMap");

	public static readonly int _Uwe_RandomVector = Shader.PropertyToID("_Uwe_RandomVector");

	public static readonly int _ScreenSpaceReflectionMaxDistance = Shader.PropertyToID("_ScreenSpaceReflectionMaxDistance");

	public static readonly int _ScreenSpaceReflectionMaxSteps = Shader.PropertyToID("_ScreenSpaceReflectionMaxSteps");

	public static readonly int _CapturedDepthSurface = Shader.PropertyToID("_CapturedDepthSurface");

	public static readonly int _ModelScale = Shader.PropertyToID("_ModelScale");

	public static readonly int _ColorStrength = Shader.PropertyToID("_ColorStrength");

	public static readonly int _ColorStrengthAtNight = Shader.PropertyToID("_ColorStrengthAtNight");

	public static readonly int _BlendSrcFactorA = Shader.PropertyToID("_BlendSrcFactorA");

	public static readonly int _BlendDstFactorA = Shader.PropertyToID("_BlendDstFactorA");

	public static readonly int _Clouds = Shader.PropertyToID("_Clouds");

	public static readonly int _SunScreenSpace = Shader.PropertyToID("_SunScreenSpace");

	public static readonly int _SampleCount = Shader.PropertyToID("_SampleCount");

	public static readonly int _Density = Shader.PropertyToID("_Density");

	public static readonly int _Decay = Shader.PropertyToID("_Decay");

	public static readonly int _Weight = Shader.PropertyToID("_Weight");

	public static readonly int _SubFrame = Shader.PropertyToID("_SubFrame");

	public static readonly int _PrevFrame = Shader.PropertyToID("_PrevFrame");

	public static readonly int _CloudBottomFade = Shader.PropertyToID("_CloudBottomFade");

	public static readonly int _MaxIterations = Shader.PropertyToID("_MaxIterations");

	public static readonly int _SampleScalar = Shader.PropertyToID("_SampleScalar");

	public static readonly int _SampleThreshold = Shader.PropertyToID("_SampleThreshold");

	public static readonly int _LODDistance = Shader.PropertyToID("_LODDistance");

	public static readonly int _RayMinimumY = Shader.PropertyToID("_RayMinimumY");

	public static readonly int _DetailScale = Shader.PropertyToID("_DetailScale");

	public static readonly int _ErosionEdgeSize = Shader.PropertyToID("_ErosionEdgeSize");

	public static readonly int _CloudDistortion = Shader.PropertyToID("_CloudDistortion");

	public static readonly int _CloudDistortionScale = Shader.PropertyToID("_CloudDistortionScale");

	public static readonly int _HorizonFadeScalar = Shader.PropertyToID("_HorizonFadeScalar");

	public static readonly int _HorizonFadeStartAlpha = Shader.PropertyToID("_HorizonFadeStartAlpha");

	public static readonly int _OneMinusHorizonFadeStartAlpha = Shader.PropertyToID("_OneMinusHorizonFadeStartAlpha");

	public static readonly int _Perlin3D = Shader.PropertyToID("_Perlin3D");

	public static readonly int _Detail3D = Shader.PropertyToID("_Detail3D");

	public static readonly int _BaseOffset = Shader.PropertyToID("_BaseOffset");

	public static readonly int _DetailOffset = Shader.PropertyToID("_DetailOffset");

	public static readonly int _BaseScale = Shader.PropertyToID("_BaseScale");

	public static readonly int _LightScalar = Shader.PropertyToID("_LightScalar");

	public static readonly int _AmbientScalar = Shader.PropertyToID("_AmbientScalar");

	public static readonly int _CloudHeightGradient1 = Shader.PropertyToID("_CloudHeightGradient1");

	public static readonly int _CloudHeightGradient2 = Shader.PropertyToID("_CloudHeightGradient2");

	public static readonly int _CloudHeightGradient3 = Shader.PropertyToID("_CloudHeightGradient3");

	public static readonly int _Coverage = Shader.PropertyToID("_Coverage");

	public static readonly int _LightDirection = Shader.PropertyToID("_LightDirection");

	public static readonly int _LightColor = Shader.PropertyToID("_LightColor");

	public static readonly int _CloudBaseColor = Shader.PropertyToID("_CloudBaseColor");

	public static readonly int _CloudTopColor = Shader.PropertyToID("_CloudTopColor");

	public static readonly int _HorizonCoverageStart = Shader.PropertyToID("_HorizonCoverageStart");

	public static readonly int _HorizonCoverageEnd = Shader.PropertyToID("_HorizonCoverageEnd");

	public static readonly int _ForwardScatteringG = Shader.PropertyToID("_ForwardScatteringG");

	public static readonly int _BackwardScatteringG = Shader.PropertyToID("_BackwardScatteringG");

	public static readonly int _DarkOutlineScalar = Shader.PropertyToID("_DarkOutlineScalar");

	public static readonly int _SunRayLength = Shader.PropertyToID("_SunRayLength");

	public static readonly int _ConeRadius = Shader.PropertyToID("_ConeRadius");

	public static readonly int _RayStepLength = Shader.PropertyToID("_RayStepLength");

	public static readonly int _Curl2D = Shader.PropertyToID("_Curl2D");

	public static readonly int _CoverageScale = Shader.PropertyToID("_CoverageScale");

	public static readonly int _CoverageOffset = Shader.PropertyToID("_CoverageOffset");

	public static readonly int _Random0 = Shader.PropertyToID("_Random0");

	public static readonly int _Random1 = Shader.PropertyToID("_Random1");

	public static readonly int _Random2 = Shader.PropertyToID("_Random2");

	public static readonly int _Random3 = Shader.PropertyToID("_Random3");

	public static readonly int _Random4 = Shader.PropertyToID("_Random4");

	public static readonly int _Random5 = Shader.PropertyToID("_Random5");

	public static readonly int _EarthRadius = Shader.PropertyToID("_EarthRadius");

	public static readonly int _StartHeight = Shader.PropertyToID("_StartHeight");

	public static readonly int _EndHeight = Shader.PropertyToID("_EndHeight");

	public static readonly int _AtmosphereThickness = Shader.PropertyToID("_AtmosphereThickness");

	public static readonly int _CameraPosition = Shader.PropertyToID("_CameraPosition");

	public static readonly int _SubFrameNumber = Shader.PropertyToID("_SubFrameNumber");

	public static readonly int _SubPixelSize = Shader.PropertyToID("_SubPixelSize");

	public static readonly int _SubFrameSize = Shader.PropertyToID("_SubFrameSize");

	public static readonly int _FrameSize = Shader.PropertyToID("_FrameSize");

	public static readonly int _PreviousProjection = Shader.PropertyToID("_PreviousProjection");

	public static readonly int _PreviousInverseProjection = Shader.PropertyToID("_PreviousInverseProjection");

	public static readonly int _PreviousRotation = Shader.PropertyToID("_PreviousRotation");

	public static readonly int _PreviousInverseRotation = Shader.PropertyToID("_PreviousInverseRotation");

	public static readonly int _Projection = Shader.PropertyToID("_Projection");

	public static readonly int _InverseProjection = Shader.PropertyToID("_InverseProjection");

	public static readonly int _Rotation = Shader.PropertyToID("_Rotation");

	public static readonly int _InverseRotation = Shader.PropertyToID("_InverseRotation");

	public static readonly int _CloudCoverage = Shader.PropertyToID("_CloudCoverage");

	public static readonly int _InvCamera = Shader.PropertyToID("_InvCamera");

	public static readonly int _InvProjection = Shader.PropertyToID("_InvProjection");

	public static readonly int _ShadowStrength = Shader.PropertyToID("_ShadowStrength");

	public static readonly int _WorldToWeatherClipMatrix = Shader.PropertyToID("_WorldToWeatherClipMatrix");

	public static readonly int _InverseWorldToWeatherClipMatrix = Shader.PropertyToID("_InverseWorldToWeatherClipMatrix");

	public static readonly int _WeatherClipTexture = Shader.PropertyToID("_WeatherClipTexture");

	public static readonly int _CoverageScalar = Shader.PropertyToID("_CoverageScalar");

	public static readonly int _RefractStrength = Shader.PropertyToID("_RefractStrength");

	public static readonly int _RefractMap = Shader.PropertyToID("_RefractMap");

	public static readonly int _WindForce = Shader.PropertyToID("_WindForce");

	public static readonly int _ShrinkIntensity = Shader.PropertyToID("_ShrinkIntensity");

	public static readonly int _FloatingIntensity = Shader.PropertyToID("_FloatingIntensity");

	public static readonly int _AnimFraction = Shader.PropertyToID("_AnimFraction");

	public static readonly int _FramesCount = Shader.PropertyToID("_FramesCount");

	public static readonly int _Duration = Shader.PropertyToID("_Duration");

	public static readonly int _UweFogHeightFallof = Shader.PropertyToID("_UweFogHeightFallof");

	public static readonly int _CloudsProjectionExtents = Shader.PropertyToID("_CloudsProjectionExtents");

	public static readonly int _CloudsRaymarchOffset = Shader.PropertyToID("_CloudsRaymarchOffset");

	public static readonly int _CloudsJitter = Shader.PropertyToID("_CloudsJitter");

	public static readonly int _LowresCloudTex = Shader.PropertyToID("_LowresCloudTex");

	public static readonly int _CloudsPrevVP = Shader.PropertyToID("_CloudsPrevVP");

	public static readonly int _CloudTex = Shader.PropertyToID("_CloudTex");

	public static readonly int _LowresCloudTexRT = Shader.PropertyToID("_LowresCloudTexRT");

	public static readonly int _CloudsPrevVPRT = Shader.PropertyToID("_CloudsPrevVPRT");

	public static readonly int _CloudTexRT = Shader.PropertyToID("_CloudTexRT");

	public static readonly int _BaseTex = Shader.PropertyToID("_BaseTex");

	public static readonly int _HeightDensity = Shader.PropertyToID("_HeightDensity");

	public static readonly int _DetailTex = Shader.PropertyToID("_DetailTex");

	public static readonly int _CurlNoise = Shader.PropertyToID("_CurlNoise");

	public static readonly int _BlueNoise = Shader.PropertyToID("_BlueNoise");

	public static readonly int _BaseTile = Shader.PropertyToID("_BaseTile");

	public static readonly int _DetailTile = Shader.PropertyToID("_DetailTile");

	public static readonly int _DetailStrength = Shader.PropertyToID("_DetailStrength");

	public static readonly int _CurlTile = Shader.PropertyToID("_CurlTile");

	public static readonly int _CurlStrength = Shader.PropertyToID("_CurlStrength");

	public static readonly int _CloudTopOffset = Shader.PropertyToID("_CloudTopOffset");

	public static readonly int _CloudSize = Shader.PropertyToID("_CloudSize");

	public static readonly int _CloudDensity = Shader.PropertyToID("_CloudDensity");

	public static readonly int _CloudTypeModifier = Shader.PropertyToID("_CloudTypeModifier");

	public static readonly int _CloudCoverageModifier = Shader.PropertyToID("_CloudCoverageModifier");

	public static readonly int _WindDirection = Shader.PropertyToID("_WindDirection");

	public static readonly int _WeatherTex = Shader.PropertyToID("_WeatherTex");

	public static readonly int _WeatherTexSize = Shader.PropertyToID("_WeatherTexSize");

	public static readonly int _Transmittance = Shader.PropertyToID("_Transmittance");

	public static readonly int _SilverIntensity = Shader.PropertyToID("_SilverIntensity");

	public static readonly int _SilverSpread = Shader.PropertyToID("_SilverSpread");

	public static readonly int _AtmosphereColor = Shader.PropertyToID("_AtmosphereColor");

	public static readonly int _AtmosphereColorSaturateDistance = Shader.PropertyToID("_AtmosphereColorSaturateDistance");

	public static readonly int _AmbientColor = Shader.PropertyToID("_AmbientColor");

	public static readonly int _ForceOutOfBound = Shader.PropertyToID("_ForceOutOfBound");

	public static readonly int _HeroCloudPos = Shader.PropertyToID("_HeroCloudPos");

	public static readonly int _HeroCloudMask = Shader.PropertyToID("_HeroCloudMask");

	public static readonly int _HeroCloudIntensity = Shader.PropertyToID("_HeroCloudIntensity");

	public static readonly int _SwirlScalar = Shader.PropertyToID("_SwirlScalar");

	public static readonly int _MultiColorMask = Shader.PropertyToID("_MultiColorMask");

	public static readonly int _Color2 = Shader.PropertyToID("_Color2");

	public static readonly int _Color3 = Shader.PropertyToID("_Color3");

	public static readonly int _SpecColor2 = Shader.PropertyToID("_SpecColor2");

	public static readonly int _SpecColor3 = Shader.PropertyToID("_SpecColor3");

	public static readonly int _ExchangerPosition = Shader.PropertyToID("_ExchangerPosition");

	public static readonly int _PointightPos = Shader.PropertyToID("_PointightPos");

	public static readonly int _PointLightColor = Shader.PropertyToID("_PointLightColor");

	public static readonly int _MainLightDir = Shader.PropertyToID("_MainLightDir");

	public static readonly int _MainLightColor = Shader.PropertyToID("_MainLightColor");

	public static readonly int _AOintensity = Shader.PropertyToID("_AOintensity");

	public static readonly int _OnOffSlider = Shader.PropertyToID("_OnOffSlider");

	public static readonly int _SpeakingSlider = Shader.PropertyToID("_SpeakingSlider");

	public static readonly int _ProximitySlider = Shader.PropertyToID("_ProximitySlider");

	public static readonly int _IntoBrainSlider = Shader.PropertyToID("_IntoBrainSlider");

	public static readonly int _MoonColorMultiplier = Shader.PropertyToID("_MoonColorMultiplier");

	public static readonly int _PlanetColorMultiplier = Shader.PropertyToID("_PlanetColorMultiplier");

	public static readonly int _SkyboxCubemap = Shader.PropertyToID("_SkyboxCubemap");

	public static readonly int _CreaturePositions = Shader.PropertyToID("_CreaturePositions");

	public static readonly int _Hide = Shader.PropertyToID("_Hide");

	public static readonly int _Gamma = Shader.PropertyToID("_Gamma");

	public static readonly int _InverseGamma = Shader.PropertyToID("_InverseGamma");

	public static readonly int _SpaceCubemap = Shader.PropertyToID("_SpaceCubemap");

	public static readonly int _GlitchIntensity = Shader.PropertyToID("_GlitchIntensity");

	public static readonly int _Seed = Shader.PropertyToID("_Seed");

	public static readonly int _HighlightingIntensity = Shader.PropertyToID("_HighlightingIntensity");

	public static readonly int _HighlightingCull = Shader.PropertyToID("_HighlightingCull");

	public static readonly int _HighlightingColor = Shader.PropertyToID("_HighlightingColor");

	public static readonly int _HighlightingBlurOffset = Shader.PropertyToID("_HighlightingBlurOffset");

	public static readonly int _HighlightingFillAlpha = Shader.PropertyToID("_HighlightingFillAlpha");

	public static readonly int _HighlightingBuffer = Shader.PropertyToID("_HighlightingBuffer");

	public static readonly int _HighlightingBlur1 = Shader.PropertyToID("_HighlightingBlur1");

	public static readonly int _HighlightingBlur2 = Shader.PropertyToID("_HighlightingBlur2");

	public static readonly int _WorldToScreenMatrix = Shader.PropertyToID("_WorldToScreenMatrix");

	public static readonly int _BoundingBoxes = Shader.PropertyToID("_BoundingBoxesBuffer");

	public static readonly int _VisibilityResults = Shader.PropertyToID("_VisibilityResultsBuffer");

	public static readonly int _DepthTexture = Shader.PropertyToID("_DepthTexture");

	public static readonly int _MipInfo = Shader.PropertyToID("_MipInfo");

	public static readonly int _CameraInfo = Shader.PropertyToID("_CameraInfo");

	public static readonly int _CameraClipPlanes = Shader.PropertyToID("_CameraClipPlanes");

	public static readonly int _UVRect = Shader.PropertyToID("_UVRect");

	public static readonly int _MainTex_Speed = Shader.PropertyToID("_MainTex_Speed");

	public static readonly int _MainTex2_Speed = Shader.PropertyToID("_MainTex2_Speed");

	public static readonly int _DeformMap_Speed = Shader.PropertyToID("_DeformMap_Speed");

	public static readonly int _TimeScale = Shader.PropertyToID("_TimeScale");

	public static readonly int _ScrollSpeed = Shader.PropertyToID("_ScrollSpeed");

	public static readonly int _RotationSpeed = Shader.PropertyToID("_RotationSpeed");

	public static readonly int _MainScrollSpeed = Shader.PropertyToID("_MainScrollSpeed");

	public static readonly int _DetailScrollSpeed = Shader.PropertyToID("_DetailScrollSpeed");

	public static readonly int _NervesScrollSpeed = Shader.PropertyToID("_NervesScrollSpeed");

	public static readonly int _RefractScrollSpeed = Shader.PropertyToID("_RefractScrollSpeed");

	public static readonly int _MainSpeed = Shader.PropertyToID("_MainSpeed");

	public static readonly int _DetailsSpeed = Shader.PropertyToID("_DetailsSpeed");

	public static readonly int _DeformSpeed = Shader.PropertyToID("_DeformSpeed");

	public static readonly int _SinWaveSpeed = Shader.PropertyToID("_SinWaveSpeed");

	public static readonly int _FresnelFade = Shader.PropertyToID("_FresnelFade");

	public static readonly int _ColorCycleSpeed = Shader.PropertyToID("_ColorCycleSpeed");

	public static readonly int _FlowIntensity = Shader.PropertyToID("_FlowIntensity");

	public static readonly int _Cutout1Matrix = Shader.PropertyToID("_Cutout1Matrix");

	public static readonly int _Cutout1Texture = Shader.PropertyToID("_Cutout1Texture");

	public static readonly int _Cutout1Min = Shader.PropertyToID("_Cutout1Min");

	public static readonly int _Cutout1SizeRcp = Shader.PropertyToID("_Cutout1SizeRcp");

	public static readonly int _Position = Shader.PropertyToID("_Position");

	public static readonly int _Alpha = Shader.PropertyToID("_Alpha");
}
