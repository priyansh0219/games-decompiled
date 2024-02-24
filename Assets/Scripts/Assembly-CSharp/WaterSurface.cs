using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.XR;
using uSky;

public class WaterSurface : MonoBehaviour
{
	public enum Quality
	{
		Medium = 0,
		High = 1
	}

	public enum Generation
	{
		Realtime = 0,
		FromDisk = 1,
		Disabled = 2
	}

	private static class StaticDisplacementTexture
	{
		public static RenderTexture displacementTexture;
	}

	private static class ClipRenderTextureContainer
	{
		public static RenderTexture clipTexture;
	}

	private static WaterSurface _instance = null;

	[Range(0f, 1.5f)]
	public float frustumDilation = 0.5f;

	public WaterscapeVolume settings;

	public Shader surfaceShader;

	private Material surfaceMaterial;

	public Shader updateFoamShader;

	private Material updateFoamMaterial;

	public Shader updateNormalsShader;

	private Material updateNormalsMaterial;

	public ComputeShader updateSpectrumShader;

	public Shader interpolateShader;

	private Material interpolateMaterial;

	public Shader packDisplacementShader;

	private Material packDisplacementMaterial;

	public Shader resizeShader;

	private Material resizeMaterial;

	public Shader copyDepthBufferShader;

	private Material copyDepthBufferMaterial;

	public AnimationCurve underWaterBrightnessCurve = new AnimationCurve();

	public bool useWaterWaterBrightnessCurve = true;

	public WaterDisplacementGenerator displacementGenerator = new WaterDisplacementGenerator();

	public WaterCausticsGenerator causticsGenerator = new WaterCausticsGenerator();

	public bool rebuild;

	[Range(1f, 2f)]
	public float refractionIndex = 1.33f;

	[Range(1f, 2f)]
	public float underWaterRefractionIndex = 1.33f;

	public float underWaterRefractionDepthScale = 0.01f;

	public float waterOffset;

	private float waterLevel;

	[Header("File:")]
	public float sequenceLength = 10f;

	private const int numFrames = 64;

	private const string displacementAddressablesName = "WaterDisplacement";

	private const string displacementAssetBundleName = "waterdisplacement";

	public Generation causticsGeneration = Generation.FromDisk;

	public int frameTextureSize = 64;

	public int numCausticsFrames = 32;

	public int causticsFramesPerSecond = 25;

	private const float maxCausticsValue = 6f;

	private Vector3 maxDisplacement = new Vector3(100f, 300f, 100f);

	private List<Texture2D> displacementTextures;

	public bool cubicInterpolation;

	private bool isLoadingDisplacementTextures;

	private AsyncOperationHandle<IList<Texture2D>> textureLoadHandle;

	private const int displacementMapSize = 512;

	public Color reflectionColor = Color.white;

	public Color refractionColor = Color.white;

	public Color backLightTint = new Color(0f, 1f, 1f);

	public float sunReflectionGloss = 200f;

	public float sunReflectionAmount = 0.8f;

	public float waveHeightThicknessScale = 0.05f;

	[Header("Foam:")]
	public Texture2D foamTexture;

	public Texture2D foamMaskTexture;

	public float foamSmoothing = 5f;

	public float foamRate = 1f;

	public float foamScale = 6f;

	public RenderTexture foamAmountTexture;

	public float foamDecay = 3f;

	public float foamDistance = 5f;

	public Color subSurfaceFoamColor = new Color(0.1f, 0.2f, 0.1f);

	public float subSurfaceFoamScale = 30f;

	public float displacementTextureFoamAmountMultiplier = 5f;

	private List<Texture2D> causticsTextures;

	public Light sunLight;

	private GameObject clipCameraObject;

	private Camera clipCamera;

	public float clipCameraSize = 128f;

	public int clipTextureSize = 512;

	private Matrix4x4 worldToClipMatrix;

	private Camera sceneCaptureCamera;

	private CommandBuffer sceneCaptureCommandBuffer;

	private RenderTexture capturedDepthBuffer;

	private RenderTexture refractionTexture;

	[Header("Downsample:")]
	[Range(0f, 8f)]
	public int ssrDownsample;

	[Header("Raycast:")]
	[Range(1f, 300f)]
	public int iterations = 20;

	[Range(0f, 32f)]
	public int binarySearchIterations;

	[Range(1f, 64f)]
	public int pixelStride = 1;

	public float pixelStrideZCutoff = 100f;

	public float pixelZSizeOffset = 0.1f;

	public float maxRayDistance = 10f;

	public bool enableReflection;

	[Header("Reflection Fading:")]
	[Range(0f, 1f)]
	public float screenEdgeFadeStart = 0.75f;

	[Range(0f, 1f)]
	public float eyeFadeStart;

	[Range(0f, 1f)]
	public float eyeFadeEnd = 1f;

	[Tooltip("Index of refraction used for screen space refraction rays")]
	[Range(1f, 2f)]
	public float screenSpaceRefractionIndex = 1.05f;

	[Range(0f, 1f)]
	public float screenSpaceInternalReflectionFlatness;

	[Range(1f, 256f)]
	public float screenSpaceReflectionMaxDistance = 32f;

	[Range(1f, 256f)]
	public int screenSpaceReflectionNumSteps = 16;

	[Range(1f, 256f)]
	public float consoleScreenSpaceReflectionMaxDistance = 32f;

	[Range(1f, 256f)]
	public int consoleScreenSpaceReflectionNumSteps = 8;

	private RenderTexture normalsTexture;

	private Mesh quadMesh;

	private HeightFieldMesh surfaceMesh;

	[Header("Level Of Detail:")]
	public int numTrianglesPerPatch = 5000;

	public float minPatchSize = 2f;

	public float errorThreshold = 1f;

	[Header("Debug:")]
	[Range(0f, 1f)]
	public float timeScale = 1f;

	public bool visible = true;

	public uSkymapRenderer sky;

	public uSkyManager skyManager;

	private float time;

	[NonSerialized]
	public bool causticsEnabled = true;

	private Quality currentQuality;

	private static Quality quality = RestrictQualityForSystem(Quality.Medium);

	private const bool PlatformRequiresDepthBufferCopy = false;

	public static WaterSurface Get()
	{
		if (_instance == null)
		{
			_instance = UnityEngine.Object.FindObjectOfType<WaterSurface>();
		}
		return _instance;
	}

	public static void SetQuality(Quality _quality)
	{
		quality = RestrictQualityForSystem(_quality);
	}

	public static Quality GetQuality()
	{
		return quality;
	}

	public static Quality RestrictQualityForSystem(Quality quality)
	{
		if (quality == Quality.High && !PlatformUtils.SupportsComputeShaders())
		{
			return Quality.Medium;
		}
		return quality;
	}

	public static Quality[] GetQualityOptions()
	{
		if (PlatformUtils.SupportsComputeShaders())
		{
			return new Quality[2]
			{
				Quality.Medium,
				Quality.High
			};
		}
		return new Quality[1];
	}

	private bool SetupCameraCommandBuffer(Camera cam)
	{
		if (sceneCaptureCamera != cam)
		{
			if (sceneCaptureCamera != null)
			{
				sceneCaptureCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, sceneCaptureCommandBuffer);
				sceneCaptureCamera = null;
			}
			if (sceneCaptureCommandBuffer == null)
			{
				sceneCaptureCommandBuffer = new CommandBuffer();
			}
			else
			{
				sceneCaptureCommandBuffer.Clear();
			}
			capturedDepthBuffer = null;
			RenderTextureDescriptor desc = DynamicResolution.CreateRenderTextureDescriptor(cam.pixelWidth, cam.pixelHeight, RenderTextureFormat.ARGB32);
			refractionTexture = new RenderTexture(desc);
			sceneCaptureCommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, refractionTexture);
			cam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, sceneCaptureCommandBuffer);
			sceneCaptureCamera = cam;
			return true;
		}
		return false;
	}

	private bool ShouldUseDisplacementTextures(Quality quality)
	{
		return quality == Quality.Medium;
	}

	private void UpdateQuality()
	{
		if (quality != currentQuality)
		{
			SetupForQuality(quality);
		}
	}

	private void SetupForQuality(Quality quality)
	{
		if (ShouldUseDisplacementTextures(quality))
		{
			LoadDisplacementTextures();
			displacementGenerator.Destroy();
		}
		else
		{
			displacementGenerator.Create();
			displacementGenerator.FillComputeBuffers(0f);
			UnloadDisplacementTextures();
		}
		currentQuality = quality;
	}

	private void Start()
	{
		surfaceMaterial = new Material(surfaceShader);
		skyManager.SetConstantMaterialProperties(surfaceMaterial);
		updateFoamMaterial = new Material(updateFoamShader);
		updateNormalsMaterial = new Material(updateNormalsShader);
		interpolateMaterial = new Material(interpolateShader);
		packDisplacementMaterial = new Material(packDisplacementShader);
		resizeMaterial = new Material(resizeShader);
		copyDepthBufferMaterial = new Material(copyDepthBufferShader);
		SetupForQuality(RestrictQualityForSystem(quality));
		if (causticsGeneration == Generation.FromDisk)
		{
			LoadCausticsTextures();
		}
		else if (causticsGeneration == Generation.Realtime)
		{
			causticsGenerator.Create();
		}
		CreateDisplacementTexture();
		foamAmountTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.RHalf)
		{
			wrapMode = TextureWrapMode.Repeat,
			name = "FoamAmount"
		};
		normalsTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.RGFloat)
		{
			wrapMode = TextureWrapMode.Repeat,
			autoGenerateMips = true,
			useMipMap = true,
			anisoLevel = 8,
			name = "WaterNormals"
		};
		quadMesh = CreateQuadMesh();
		InitializeFoamTexture();
		CreateClipCamera();
		SetupSurfaceMaterialConstantValues(surfaceMaterial);
		DevConsole.RegisterConsoleCommand(this, "togglewatershadowrecieve");
		DevConsole.RegisterConsoleCommand(this, "togglewatershadowcast");
		DevConsole.RegisterConsoleCommand(this, "togglewaterfrustumcull");
	}

	private void OnDestroy()
	{
		UnloadDisplacementTextures();
		displacementGenerator.Destroy();
		if ((bool)foamAmountTexture)
		{
			foamAmountTexture.Release();
			foamAmountTexture = null;
		}
		if ((bool)normalsTexture)
		{
			normalsTexture.Release();
			normalsTexture = null;
		}
		if ((bool)capturedDepthBuffer)
		{
			capturedDepthBuffer.Release();
			capturedDepthBuffer = null;
		}
		if ((bool)refractionTexture)
		{
			refractionTexture.Release();
			refractionTexture = null;
		}
		if (surfaceMesh != null)
		{
			surfaceMesh.Dispose();
			surfaceMesh = null;
		}
	}

	private void OnEnable()
	{
		if (surfaceMesh != null)
		{
			surfaceMesh.Dispose();
		}
		surfaceMesh = new HeightFieldMesh(numTrianglesPerPatch);
	}

	private void OnDisable()
	{
		surfaceMesh.Dispose();
		surfaceMesh = null;
	}

	private void CreateClipCamera()
	{
		if (!(clipCameraObject != null))
		{
			clipCameraObject = new GameObject("Clip Camera");
			RenderTexture renderTexture = ClipRenderTextureContainer.clipTexture;
			if (renderTexture == null)
			{
				renderTexture = (ClipRenderTextureContainer.clipTexture = new RenderTexture(clipTextureSize, clipTextureSize, 0, RenderTextureFormat.RGHalf)
				{
					name = "ClipCamera"
				});
			}
			clipCamera = clipCameraObject.AddComponent<Camera>();
			clipCamera.enabled = true;
			clipCamera.cullingMask = 1 << LayerID.BaseClipProxy;
			clipCamera.orthographic = true;
			clipCamera.targetTexture = renderTexture;
			clipCamera.clearFlags = CameraClearFlags.Color;
			clipCamera.backgroundColor = new Color(0f, 1f, 0f, 0f);
			clipCamera.allowHDR = false;
			clipCamera.allowMSAA = false;
			clipCamera.useOcclusionCulling = false;
			clipCamera.renderingPath = RenderingPath.Forward;
			clipCamera.depthTextureMode = DepthTextureMode.None;
			clipCamera.pixelRect = new Rect(1f, 1f, renderTexture.width - 2, renderTexture.width - 2);
			if (XRSettings.enabled)
			{
				XRDevice.DisableAutoXRCameraTracking(clipCamera, disabled: true);
				Transform obj = clipCamera.transform;
				obj.localPosition = Vector3.zero;
				obj.localRotation = Quaternion.identity;
			}
			clipCameraObject.AddComponent<DetachRenderTargetOnDestroy>();
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = renderTexture;
			GL.Clear(clearDepth: false, clearColor: true, clipCamera.backgroundColor);
			RenderTexture.active = active;
			if (surfaceMaterial != null)
			{
				surfaceMaterial.SetTexture(ShaderPropertyID._ClipTexture, renderTexture);
			}
		}
	}

	private Mesh CreateQuadMesh()
	{
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, 0f),
			new Vector3(1f, -1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(-1f, 1f, 0f)
		};
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
		return mesh;
	}

	private void CreateDisplacementTexture()
	{
		if (StaticDisplacementTexture.displacementTexture == null)
		{
			RenderTextureFormat format = RenderTextureFormat.ARGBFloat;
			StaticDisplacementTexture.displacementTexture = new RenderTexture(512, 512, 0, format, RenderTextureReadWrite.Linear)
			{
				wrapMode = TextureWrapMode.Repeat,
				name = "WaterDisplacement"
			};
		}
	}

	private string GetResourceTextureDirectory()
	{
		return "Data/";
	}

	private string GetDisplacementTextureName(int frameIndex, string extension = "")
	{
		return $"WaterFrame{frameIndex:00}{extension}";
	}

	private string GetCausticsTextureName(int frameIndex, string extension = "")
	{
		return $"WaterCaustics{frameIndex:00}{extension}";
	}

	public bool IsLoadingDisplacementTextures()
	{
		return isLoadingDisplacementTextures;
	}

	private bool GetAreDisplacementTexturesAvailable()
	{
		if (displacementTextures != null && displacementTextures.Count > 0)
		{
			return !IsLoadingDisplacementTextures();
		}
		return false;
	}

	private void LoadDisplacementTextures()
	{
		if (!isLoadingDisplacementTextures)
		{
			isLoadingDisplacementTextures = true;
			AsyncOperationHandle<IList<Texture2D>> asyncOperationHandle = AddressablesUtility.LoadAllAsync<Texture2D>("WaterDisplacement");
			asyncOperationHandle.Completed += LoadDisplacementTexturesAsync;
		}
	}

	private void LoadDisplacementTexturesAsync(AsyncOperationHandle<IList<Texture2D>> handle)
	{
		textureLoadHandle = handle;
		if (textureLoadHandle.Status == AsyncOperationStatus.Failed)
		{
			Debug.LogError("[WaterSurface] Water displacement textures were unable to be found");
			UnloadDisplacementTextures();
			return;
		}
		IList<Texture2D> result = handle.Result;
		if (result.Count != 64)
		{
			Debug.LogErrorFormat("[WaterSurface] Only {0}/{1} of displacement textures have been loaded", result.Count, 64);
			UnloadDisplacementTextures();
			return;
		}
		displacementTextures = new List<Texture2D>(64);
		for (int i = 0; i < 64; i++)
		{
			Texture2D item = result[i];
			displacementTextures.Add(item);
		}
		isLoadingDisplacementTextures = false;
	}

	private void UnloadDisplacementTextures()
	{
		isLoadingDisplacementTextures = false;
		displacementTextures = null;
		if (textureLoadHandle.IsValid())
		{
			AddressablesUtility.QueueRelease(ref textureLoadHandle);
		}
	}

	private void LoadCausticsTextures()
	{
		string resourceTextureDirectory = GetResourceTextureDirectory();
		causticsTextures = new List<Texture2D>();
		for (int i = 0; i < numCausticsFrames; i++)
		{
			string causticsTextureName = GetCausticsTextureName(i);
			Texture2D item = Resources.Load<Texture2D>(resourceTextureDirectory + causticsTextureName);
			causticsTextures.Add(item);
		}
	}

	private void OnValidate()
	{
		if (surfaceMaterial != null)
		{
			SetupSurfaceMaterialConstantValues(surfaceMaterial);
		}
	}

	private void SetupSurfaceMaterialConstantValues(Material material)
	{
		float transmission = settings.GetTransmission();
		float patchLength = displacementGenerator.GetPatchLength();
		Shader.SetGlobalTexture(ShaderPropertyID._WaterDisplacementMap, StaticDisplacementTexture.displacementTexture);
		Shader.SetGlobalFloat(ShaderPropertyID._WaterPatchLength, patchLength);
		material.SetFloat(ShaderPropertyID._TexelLength2, patchLength / 512f * 2f);
		material.SetTexture(ShaderPropertyID._NormalsTex, normalsTexture);
		float value = Mathf.Pow((1f - refractionIndex) / (1f + refractionIndex), 2f);
		material.SetFloat(ShaderPropertyID._Refraction0, value);
		material.SetColor(ShaderPropertyID._ReflectionColor, reflectionColor);
		material.SetColor(ShaderPropertyID._RefractionColor, refractionColor);
		float value2 = 1f;
		if (ShouldUseDisplacementTextures(currentQuality))
		{
			value2 = displacementTextureFoamAmountMultiplier;
		}
		material.SetTexture(ShaderPropertyID._FoamTexture, foamTexture);
		material.SetTexture(ShaderPropertyID._FoamMaskTexture, foamMaskTexture);
		material.SetFloat(ShaderPropertyID._FoamSmoothing, foamSmoothing);
		material.SetTexture(ShaderPropertyID._FoamAmountTexture, foamAmountTexture);
		material.SetFloat(ShaderPropertyID._FoamScale, foamScale);
		material.SetFloat(ShaderPropertyID._FoamDistance, foamDistance);
		material.SetColor(ShaderPropertyID._SubSurfaceFoamColor, subSurfaceFoamColor * subSurfaceFoamColor.a);
		material.SetFloat(ShaderPropertyID._SubSurfaceFoamScale, subSurfaceFoamScale);
		material.SetFloat(ShaderPropertyID._FoamAmountMultiplier, value2);
		material.SetVector(ShaderPropertyID._BackLightTint, backLightTint.linear * transmission);
		material.SetFloat(ShaderPropertyID._WaveHeightThicknessScale, waveHeightThicknessScale);
		material.SetTexture(ShaderPropertyID._ClipTexture, ClipRenderTextureContainer.clipTexture);
		material.SetFloat(ShaderPropertyID._SunReflectionGloss, sunReflectionGloss);
		material.SetFloat(ShaderPropertyID._SunReflectionAmount, sunReflectionAmount);
		material.SetFloat(ShaderPropertyID._ScreenSpaceRefractionRatio, 1f / screenSpaceRefractionIndex);
		material.SetFloat(ShaderPropertyID._ScreenSpaceInternalReflectionFlatness, screenSpaceInternalReflectionFlatness);
		material.SetFloat(ShaderPropertyID._PixelStride, pixelStride);
		material.SetFloat(ShaderPropertyID._PixelStrideZCuttoff, pixelStrideZCutoff);
		material.SetFloat(ShaderPropertyID._PixelZSize, pixelZSizeOffset);
		material.SetFloat(ShaderPropertyID._Iterations, iterations);
		material.SetFloat(ShaderPropertyID._BinarySearchIterations, binarySearchIterations);
		material.SetFloat(ShaderPropertyID._MaxRayDistance, maxRayDistance);
		material.SetFloat(ShaderPropertyID._ScreenEdgeFadeStart, screenEdgeFadeStart);
		material.SetFloat(ShaderPropertyID._EyeFadeStart, eyeFadeStart);
		material.SetFloat(ShaderPropertyID._EyeFadeEnd, eyeFadeEnd);
		material.SetInt(ShaderPropertyID._ZWriteMode, 1);
		material.SetTexture(ShaderPropertyID._CapturedDepthSurface, capturedDepthBuffer);
		material.SetTexture(ShaderPropertyID._RefractionTexture, refractionTexture);
		if (enableReflection && AtmosphereDirector.ShadowsEnabled())
		{
			material.EnableKeyword("ENABLE_SCREEN_SPACE_REFLECTION");
		}
		else
		{
			material.DisableKeyword("ENABLE_SCREEN_SPACE_REFLECTION");
		}
	}

	private void SetupSurfaceMaterial(Camera camera, Material material, Matrix4x4 worldToClipMatrix)
	{
		if (SetupCameraCommandBuffer(camera))
		{
			material.SetTexture(ShaderPropertyID._RefractionTexture, refractionTexture);
			material.SetTexture(ShaderPropertyID._CapturedDepthSurface, capturedDepthBuffer);
		}
		float depthOf = Ocean.GetDepthOf(camera.transform.position);
		float value = underWaterRefractionIndex + depthOf * underWaterRefractionDepthScale;
		material.SetFloat(ShaderPropertyID._UnderWaterRefraction, value);
		material.SetMatrix(ShaderPropertyID._WorldToClipMatrix, worldToClipMatrix);
		Color meanSkyColor = skyManager.GetMeanSkyColor();
		material.SetVector(ShaderPropertyID._MeanSkyColor, meanSkyColor.linear);
		if (skyManager != null)
		{
			skyManager.SetVaryingMaterialProperties(material);
		}
		if (sky != null)
		{
			Shader.SetGlobalTexture(ShaderPropertyID._SkyMap, sky.GetSkyTexture());
		}
		SetupSSRMaterialProperties(camera, material);
		if (useWaterWaterBrightnessCurve)
		{
			material.SetFloat(ShaderPropertyID._UnderWaterSkyBrightness, underWaterBrightnessCurve.Evaluate(depthOf));
		}
		else
		{
			material.SetFloat(ShaderPropertyID._UnderWaterSkyBrightness, 1f);
		}
	}

	private void Update()
	{
		time += Time.deltaTime * timeScale;
	}

	public void DoUpdate(bool isSurfaceVisible)
	{
		RenderBuffer activeColorBuffer = Graphics.activeColorBuffer;
		RenderBuffer activeDepthBuffer = Graphics.activeDepthBuffer;
		UpdateQuality();
		if (sunLight != null)
		{
			if (causticsEnabled)
			{
				sunLight.cookie = GetCausticsTexture();
			}
			else
			{
				sunLight.cookie = null;
			}
		}
		UpdateClipCamera(isSurfaceVisible);
		waterLevel = base.transform.position.y + waterOffset;
		if (isSurfaceVisible)
		{
			if (!ShouldUseDisplacementTextures(currentQuality))
			{
				displacementGenerator.GenerateDisplacementMap(StaticDisplacementTexture.displacementTexture, time);
			}
			else if (GetAreDisplacementTexturesAvailable())
			{
				int count = displacementTextures.Count;
				float num = (float)count / sequenceLength;
				float num2 = time * num;
				int num3 = Mathf.FloorToInt(num2);
				int num4 = num3 + 1;
				float fraction = num2 - (float)num3;
				int num5 = num3 + count - 1;
				int num6 = num4 + 1;
				num3 %= count;
				num4 %= count;
				num5 %= count;
				num6 %= count;
				InterpolateDisplacementMaps(StaticDisplacementTexture.displacementTexture, displacementTextures[num5], displacementTextures[num3], displacementTextures[num4], displacementTextures[num6], fraction);
			}
			else
			{
				Graphics.SetRenderTarget(StaticDisplacementTexture.displacementTexture);
				GL.Viewport(new Rect(0f, 0f, StaticDisplacementTexture.displacementTexture.width, StaticDisplacementTexture.displacementTexture.height));
				GL.Clear(clearDepth: false, clearColor: true, Color.black);
			}
			UpdateNormalsTexture(normalsTexture);
		}
		if (causticsGeneration == Generation.Realtime)
		{
			causticsGenerator.GenerateCaustics(StaticDisplacementTexture.displacementTexture, normalsTexture, GetPatchLength() / 100f);
		}
		else if (causticsGeneration == Generation.FromDisk && causticsTextures == null)
		{
			LoadCausticsTextures();
		}
		if (isSurfaceVisible)
		{
			UpdateFoamTexture(worldToClipMatrix);
		}
		Graphics.SetRenderTarget(activeColorBuffer, activeDepthBuffer);
	}

	public Texture GetCausticsTexture()
	{
		if (causticsGeneration == Generation.FromDisk && causticsTextures != null && causticsTextures.Count == numCausticsFrames)
		{
			int num = Mathf.FloorToInt(time * (float)causticsFramesPerSecond);
			return causticsTextures[num % numCausticsFrames];
		}
		if (causticsGeneration == Generation.Realtime)
		{
			return causticsGenerator.GetCausticsTexture();
		}
		return null;
	}

	public float GetCausticsTextureScale()
	{
		if (causticsGeneration == Generation.FromDisk && causticsTextures != null)
		{
			return 6f;
		}
		return 1f;
	}

	public float GetCausticsWorldToTextureScale()
	{
		return 1f / causticsGenerator.GetCausticsSize();
	}

	private void InterpolateDisplacementMaps(RenderTexture target, Texture frame0, Texture frame1, Texture frame2, Texture frame3, float fraction)
	{
		Graphics.SetRenderTarget(target);
		GL.Viewport(new Rect(0f, 0f, target.width, target.height));
		Vector3 vector = new Vector3(fraction, fraction * fraction, fraction * fraction * fraction);
		Vector3 vector2 = 2f * maxDisplacement;
		Vector3 vector3 = -maxDisplacement;
		if (cubicInterpolation)
		{
			interpolateMaterial.EnableKeyword("CUBIC_INTERPOLATION");
		}
		else
		{
			interpolateMaterial.DisableKeyword("CUBIC_INTERPOLATION");
		}
		interpolateMaterial.SetVector(ShaderPropertyID._DisplacementLoadScale, vector2);
		interpolateMaterial.SetVector(ShaderPropertyID._DisplacementLoadOffset, vector3);
		interpolateMaterial.SetTexture(ShaderPropertyID._Frame0Tex, frame0);
		interpolateMaterial.SetTexture(ShaderPropertyID._Frame1Tex, frame1);
		interpolateMaterial.SetTexture(ShaderPropertyID._Frame2Tex, frame2);
		interpolateMaterial.SetTexture(ShaderPropertyID._Frame3Tex, frame3);
		interpolateMaterial.SetVector(ShaderPropertyID._Fraction, vector);
		DrawQuad(interpolateMaterial);
	}

	public Texture GetDisplacementTexture()
	{
		return StaticDisplacementTexture.displacementTexture;
	}

	public float GetPatchLength()
	{
		return displacementGenerator.GetPatchLength();
	}

	public void PreRender(Camera camera)
	{
		if (visible)
		{
			SetupSurfaceMaterial(camera, surfaceMaterial, worldToClipMatrix);
			settings.PreRender(camera);
		}
	}

	public void BeginRenderWaterSurface(Camera camera)
	{
		if (visible)
		{
			float farClipPlane = camera.farClipPlane;
			Vector3 position = camera.transform.position;
			surfaceMesh.BeginRender(position, new Vector3(position.x, waterLevel, position.z), farClipPlane, minPatchSize, errorThreshold, camera, frustumDilation);
		}
	}

	public bool RenderWaterSurface(Camera camera)
	{
		if (!visible)
		{
			return false;
		}
		bool result = surfaceMesh.FinalizeRender(surfaceMaterial, camera);
		settings.PostRender(camera);
		return result;
	}

	private void PackDisplacementMap(RenderTexture displacementTexture, RenderTexture packedTexture)
	{
		Vector3 vector = new Vector3(0.5f / maxDisplacement.x, 0.5f / maxDisplacement.y, 0.5f / maxDisplacement.z);
		Vector3 vector2 = 0.5f * Vector3.one;
		packDisplacementMaterial.SetTexture(ShaderPropertyID._DisplacementTex, displacementTexture);
		packDisplacementMaterial.SetVector(ShaderPropertyID._DisplacementStoreScale, vector);
		packDisplacementMaterial.SetVector(ShaderPropertyID._DisplacementStoreOffset, vector2);
		Graphics.SetRenderTarget(packedTexture);
		GL.Viewport(new Rect(0f, 0f, 512f, 512f));
		DrawQuad(packDisplacementMaterial);
	}

	private void PackCausticsMap(RenderTexture causticsTexture, RenderTexture packedTexture)
	{
		resizeMaterial.SetTexture(ShaderPropertyID._Texture, causticsTexture);
		resizeMaterial.SetFloat(ShaderPropertyID._Scale, 1f / 6f);
		Graphics.SetRenderTarget(packedTexture);
		GL.Viewport(new Rect(0f, 0f, packedTexture.width, packedTexture.height));
		DrawQuad(resizeMaterial);
	}

	private void UpdateNormalsTexture(RenderTexture normalsTexture)
	{
		Graphics.SetRenderTarget(normalsTexture);
		GL.Viewport(new Rect(0f, 0f, 512f, 512f));
		updateNormalsMaterial.SetTexture(ShaderPropertyID._WaterDisplacementMap, StaticDisplacementTexture.displacementTexture);
		DrawQuad(updateNormalsMaterial);
	}

	private void InitializeFoamTexture()
	{
		Graphics.SetRenderTarget(foamAmountTexture);
		GL.Clear(clearDepth: false, clearColor: true, Color.black);
	}

	private void UpdateFoamTexture(Matrix4x4 worldToClipMatrix)
	{
		Graphics.SetRenderTarget(foamAmountTexture);
		float num = Time.deltaTime * timeScale;
		float patchLength = displacementGenerator.GetPatchLength();
		updateFoamMaterial.SetTexture(ShaderPropertyID._MainTex, StaticDisplacementTexture.displacementTexture);
		updateFoamMaterial.SetFloat(ShaderPropertyID._WaterPatchLength, patchLength);
		updateFoamMaterial.SetFloat(ShaderPropertyID._DisplacementTexel, 0.001953125f);
		updateFoamMaterial.SetFloat(ShaderPropertyID._FoamDecay, Mathf.Max(1f - num * foamDecay, 0f));
		updateFoamMaterial.SetFloat(ShaderPropertyID._FoamRate, foamRate * num / 0.008333f);
		updateFoamMaterial.SetTexture(ShaderPropertyID._ClipTexture, ClipRenderTextureContainer.clipTexture);
		updateFoamMaterial.SetMatrix(ShaderPropertyID._WorldToClipMatrix, worldToClipMatrix);
		DrawQuad(updateFoamMaterial);
	}

	private void DrawQuad(Material material)
	{
		material.SetPass(0);
		Graphics.DrawMeshNow(quadMesh, Matrix4x4.identity);
	}

	private void UpdateClipCamera(bool isSurfaceVisible)
	{
		clipCamera.enabled = isSurfaceVisible;
		if (isSurfaceVisible)
		{
			float num = 30f;
			Vector3 position = MainCamera.camera.transform.position;
			position.y = num;
			float num2 = clipCameraSize * 2f / (float)ClipRenderTextureContainer.clipTexture.width;
			position.x = Mathf.Floor(position.x / num2) * num2;
			position.z = Mathf.Floor(position.z / num2) * num2;
			clipCamera.transform.SetPositionAndRotation(position, Quaternion.Euler(new Vector3(90f, 0f, 0f)));
			clipCamera.orthographicSize = clipCameraSize;
			clipCamera.nearClipPlane = 1f;
			clipCamera.farClipPlane = num * 2f;
			Matrix4x4 identity = Matrix4x4.identity;
			identity[0, 0] = 0.5f;
			identity[1, 1] = (GraphicsUtil.GetUVStartsAtTop() ? (-0.5f) : 0.5f);
			identity[0, 3] = 0.5f;
			identity[1, 3] = 0.5f;
			worldToClipMatrix = identity * GL.GetGPUProjectionMatrix(clipCamera.projectionMatrix, renderIntoTexture: true) * clipCamera.worldToCameraMatrix;
		}
	}

	private void SetupSSRMaterialProperties(Camera camera, Material material)
	{
		int num = ssrDownsample + 1;
		int num2 = camera.pixelWidth / num;
		int num3 = camera.pixelHeight / num;
		Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0f), Quaternion.identity, new Vector3(0.5f, 0.5f, 1f));
		Matrix4x4 matrix4x2 = Matrix4x4.Scale(new Vector3(num2, num3, 1f));
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		Matrix4x4 value = matrix4x2 * matrix4x * projectionMatrix;
		int value2 = screenSpaceReflectionNumSteps;
		float value3 = screenSpaceReflectionMaxDistance;
		material.SetFloat(ShaderPropertyID._ScreenSpaceReflectionMaxDistance, value3);
		material.SetInt(ShaderPropertyID._ScreenSpaceReflectionMaxSteps, value2);
		material.SetVector(ShaderPropertyID._RenderBufferSize, new Vector4(num2, num3, 0f, 0f));
		material.SetVector(ShaderPropertyID._OneDividedByRenderBufferSize, new Vector4(1f / (float)num2, 1f / (float)num3, 0f, 0f));
		material.SetMatrix(ShaderPropertyID._CameraProjectionMatrix, value);
		material.SetMatrix(ShaderPropertyID._CameraInverseProjectionMatrix, projectionMatrix.inverse);
	}

	public void OnConsoleCommand_togglewaterfrustumcull(NotificationCenter.Notification n)
	{
		surfaceMesh.frustumCull = !surfaceMesh.frustumCull;
		Debug.Log($"WaterSurface: FrustumCull : {surfaceMesh.frustumCull.ToString()}");
	}

	public void OnConsoleCommand_togglewatershadowcast(NotificationCenter.Notification n)
	{
		surfaceMesh.castShadows = !surfaceMesh.castShadows;
		Debug.Log($"WaterSurface: CastShadows : {surfaceMesh.castShadows.ToString()}");
	}

	public void OnConsoleCommand_togglewatershadowrecieve(NotificationCenter.Notification n)
	{
		surfaceMesh.receiveShadows = !surfaceMesh.receiveShadows;
		Debug.Log($"WaterSurface: ReceiveShadows : {surfaceMesh.receiveShadows.ToString()}");
	}
}
