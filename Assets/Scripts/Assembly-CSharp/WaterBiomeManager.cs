using System;
using System.Collections.Generic;
using System.Linq;
using UWE;
using UnityEngine;
using mset;

[ExecuteInEditMode]
public class WaterBiomeManager : MonoBehaviour, ICompileTimeCheckable
{
	private struct Region1D
	{
		public int minIndex;

		public int maxIndex;
	}

	private struct Region2D
	{
		public int xMin;

		public int yMin;

		public int xMax;

		public int yMax;
	}

	[Serializable]
	public class BiomeSettings
	{
		public string name;

		public WaterscapeVolume.Settings settings;

		public GameObject skyPrefab;
	}

	private WaterscapeVolume volume;

	public Shader blurShader;

	private Material blurMaterial;

	public Shader unwrapShader;

	private Material unwrapMaterial;

	public Shader lookupShader;

	private Material lookupMaterial;

	public Shader debugShader;

	private Material debugMaterial;

	public Shader atmosphereVolumeShader;

	private Material atmosphereVolumeMaterial;

	public Shader resizeShader;

	private Material resizeMaterial;

	public LargeWorld world;

	public List<BiomeSettings> biomeSettings = new List<BiomeSettings>();

	private Dictionary<string, int> biomeLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	private List<Sky> biomeSkies = new List<Sky>();

	private WaterscapeVolume.Settings defaultSettings = new WaterscapeVolume.Settings();

	private RenderTexture settingsTexture;

	private RenderTexture settingsTextureBlurred;

	private RenderTexture extinctionTextureBlurred;

	private RenderTexture scatteringTextureBlurred;

	private RenderTexture emissiveTextureBlurred;

	private RenderTexture blurTempTexture;

	private Texture2D biomeExtinctionTexture;

	private Texture2D biomeScatteringTexture;

	private Texture2D biomeEmissiveTexture;

	public int settingsTextureSize = 8;

	public int settingsTextureUpsampledSize = 32;

	public float regionBounds = 50f;

	public bool enableBlur = true;

	private Vector3 wsMin;

	private Color32[] biomeIndexMap;

	private Texture2D biomeIndexMapTexture;

	private RenderTexture biomeIndexMapUnwrappedTexture;

	private float waterTransmission;

	private Int3 scrollTextureMinIndex;

	private Int3 scrollTextureMin;

	private bool incrementalUpdate;

	private Region2D[] xzRegions = new Region2D[6];

	private Region1D[] yRegions = new Region1D[2];

	public bool debugBiomeMap;

	public bool continuousRefresh;

	private int atmosphereVolumesLayerMask;

	public static WaterBiomeManager main;

	private RenderBuffer[] colorTargets = new RenderBuffer[3];

	private static Region1D[] cxRegions = new Region1D[2];

	private static Region1D[] cyRegions = new Region1D[2];

	private void Awake()
	{
		main = this;
		base.useGUILayout = false;
	}

	private void Start()
	{
		volume = GetComponent<WaterscapeVolume>();
		waterTransmission = 0f;
		blurMaterial = new Material(blurShader);
		unwrapMaterial = new Material(unwrapShader);
		lookupMaterial = new Material(lookupShader);
		debugMaterial = new Material(debugShader);
		resizeMaterial = new Material(resizeShader);
		atmosphereVolumeMaterial = new Material(atmosphereVolumeShader);
		CreateTextures();
		for (int i = 0; i < biomeSettings.Count; i++)
		{
			Sky item = null;
			if (biomeSettings[i].skyPrefab != null && MarmoSkies.main != null)
			{
				item = MarmoSkies.main.GetSky(biomeSettings[i].skyPrefab);
			}
			biomeSkies.Add(item);
			string key = biomeSettings[i].name;
			if (biomeLookup.ContainsKey(key))
			{
				Debug.LogWarningFormat("WaterBiomeManager: biomeSettings contains multiple instances of the same biome name: {0}", biomeSettings[i].name);
			}
			else
			{
				biomeLookup.Add(biomeSettings[i].name, i);
			}
		}
		atmosphereVolumesLayerMask = 1 << AtmosphereVolume.GetRequiredLayer();
	}

	private void OnDestroy()
	{
		ReleaseTextures();
	}

	private void CreateTextures()
	{
		int num = settingsTextureSize;
		int num2 = settingsTextureSize * settingsTextureSize;
		biomeIndexMapTexture = new Texture2D(num, num2, TextureFormat.RGBA32, mipChain: false, linear: true);
		biomeIndexMapTexture.name = "biomeIndexMapTexture";
		biomeIndexMapTexture.wrapMode = TextureWrapMode.Repeat;
		biomeIndexMapTexture.filterMode = FilterMode.Point;
		biomeIndexMapUnwrappedTexture = new RenderTexture(num, num2, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		biomeIndexMapUnwrappedTexture.wrapMode = TextureWrapMode.Repeat;
		biomeIndexMapUnwrappedTexture.filterMode = FilterMode.Point;
		biomeIndexMapUnwrappedTexture.name = "biomeIndexMapUnwrappedTexture";
		settingsTexture = new RenderTexture(num, num2, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		settingsTexture.wrapMode = TextureWrapMode.Clamp;
		settingsTexture.name = "settingsTexture";
		blurTempTexture = new RenderTexture(num, num2, 0, settingsTexture.format, RenderTextureReadWrite.Linear);
		blurTempTexture.wrapMode = TextureWrapMode.Clamp;
		blurTempTexture.name = "blurTemp";
		settingsTextureBlurred = new RenderTexture(num, num2, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		settingsTextureBlurred.wrapMode = TextureWrapMode.Clamp;
		settingsTextureBlurred.name = "settingsTextureBlurred";
		extinctionTextureBlurred = new RenderTexture(settingsTextureUpsampledSize, settingsTextureUpsampledSize * settingsTextureUpsampledSize, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		extinctionTextureBlurred.wrapMode = TextureWrapMode.Clamp;
		extinctionTextureBlurred.name = "extinctionTextureBlurred";
		scatteringTextureBlurred = new RenderTexture(settingsTextureUpsampledSize, settingsTextureUpsampledSize * settingsTextureUpsampledSize, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		scatteringTextureBlurred.wrapMode = TextureWrapMode.Clamp;
		scatteringTextureBlurred.name = "scatteringTextureBlurred";
		emissiveTextureBlurred = new RenderTexture(settingsTextureUpsampledSize, settingsTextureUpsampledSize * settingsTextureUpsampledSize, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		emissiveTextureBlurred.wrapMode = TextureWrapMode.Clamp;
		emissiveTextureBlurred.name = "emissiveTextureBlurred";
		Shader.SetGlobalTexture(ShaderPropertyID._UweExtinctionTexture, extinctionTextureBlurred);
		Shader.SetGlobalTexture(ShaderPropertyID._UweScatteringTexture, scatteringTextureBlurred);
		Shader.SetGlobalTexture(ShaderPropertyID._UweEmissiveTexture, emissiveTextureBlurred);
		Shader.SetGlobalFloat(ShaderPropertyID._UweVolumeTextureSlices, settingsTextureUpsampledSize);
		int num3 = num * num2;
		biomeIndexMap = new Color32[num3];
		incrementalUpdate = false;
	}

	private void ReleaseTextures()
	{
		if ((bool)biomeIndexMapUnwrappedTexture && biomeIndexMapUnwrappedTexture.IsCreated())
		{
			biomeIndexMapUnwrappedTexture.Release();
		}
		if ((bool)settingsTexture && settingsTexture.IsCreated())
		{
			settingsTexture.Release();
		}
		if ((bool)blurTempTexture && blurTempTexture.IsCreated())
		{
			blurTempTexture.Release();
		}
		if ((bool)settingsTextureBlurred && settingsTextureBlurred.IsCreated())
		{
			settingsTextureBlurred.Release();
		}
		if ((bool)extinctionTextureBlurred && extinctionTextureBlurred.IsCreated())
		{
			extinctionTextureBlurred.Release();
		}
		if ((bool)scatteringTextureBlurred && scatteringTextureBlurred.IsCreated())
		{
			scatteringTextureBlurred.Release();
		}
		if ((bool)emissiveTextureBlurred && emissiveTextureBlurred.IsCreated())
		{
			emissiveTextureBlurred.Release();
		}
	}

	private void Update()
	{
		if (LargeWorld.main == null || LargeWorld.main.worldMounted)
		{
			bool flag = !incrementalUpdate;
			if (waterTransmission != volume.GetTransmission())
			{
				flag = true;
				waterTransmission = volume.GetTransmission();
			}
			if (flag)
			{
				BuildSettingsArrayTextures();
			}
			UpdateForPosition(MainCamera.camera.transform.position, flag);
			incrementalUpdate = true;
		}
		if (continuousRefresh)
		{
			BuildSettingsTextures();
		}
	}

	public void Rebuild()
	{
		if (LargeWorld.main != null && LargeWorld.main.worldMounted)
		{
			BuildSettingsArrayTextures();
			UpdateForPosition(MainCamera.camera.transform.position, forceRebuild: true);
		}
	}

	private int UpdateBiomeRegion(int xMin, int y, int zMin, int xMax, int zMax)
	{
		float metersPerPixel = GetMetersPerPixel();
		Vector3 wsPos = default(Vector3);
		for (int i = zMin; i < zMax; i++)
		{
			for (int j = xMin; j < xMax; j++)
			{
				int num = (j - scrollTextureMinIndex.x + settingsTextureSize) % settingsTextureSize;
				int num2 = (y - scrollTextureMinIndex.y + settingsTextureSize) % settingsTextureSize;
				int num3 = (i - scrollTextureMinIndex.z + settingsTextureSize) % settingsTextureSize;
				wsPos.x = (float)(scrollTextureMin.x + num) * metersPerPixel;
				wsPos.y = (float)(scrollTextureMin.y + num2) * metersPerPixel;
				wsPos.z = (float)(scrollTextureMin.z + num3) * metersPerPixel;
				int num4 = j + (i + y * settingsTextureSize) * settingsTextureSize;
				if (world != null)
				{
					string biome = world.GetBiome(wsPos);
					biomeIndexMap[num4].r = (byte)(GetBiomeIndex(biome) + 1);
				}
				else
				{
					biomeIndexMap[num4].r = 0;
				}
			}
		}
		return (xMax - xMin) * (zMax - zMin);
	}

	private int UpdateBiomeRegions(int yMin, int yMax, Region2D[] xzRegions, int numRegions, bool yRegionFullUpdate)
	{
		int num = 0;
		for (int i = yMin; i < yMax; i++)
		{
			if (yRegionFullUpdate)
			{
				num += UpdateBiomeRegion(0, i, 0, settingsTextureSize, settingsTextureSize);
				continue;
			}
			for (int j = 0; j < numRegions; j++)
			{
				Region2D region2D = xzRegions[j];
				num += UpdateBiomeRegion(region2D.xMin, i, region2D.yMin, region2D.xMax, region2D.yMax);
			}
		}
		return num;
	}

	private float GetMetersPerPixel()
	{
		return 2f * regionBounds / (float)settingsTextureSize;
	}

	private void UpdateForPosition(Vector3 wsCenter, bool forceRebuild)
	{
		float metersPerPixel = GetMetersPerPixel();
		wsMin = wsCenter - new Vector3(regionBounds, regionBounds, regionBounds);
		Int3 @int = default(Int3);
		@int.x = Mathf.RoundToInt(wsMin.x / metersPerPixel);
		@int.y = Mathf.RoundToInt(wsMin.y / metersPerPixel);
		@int.z = Mathf.RoundToInt(wsMin.z / metersPerPixel);
		wsMin = @int.ToVector3() * metersPerPixel;
		Int3 int2 = @int - scrollTextureMin;
		Int3 int3 = scrollTextureMinIndex + int2;
		if (forceRebuild || int3.x != scrollTextureMinIndex.x || int3.y != scrollTextureMinIndex.y || int3.z != scrollTextureMinIndex.z)
		{
			int numRegions = 0;
			int numRegions2 = 0;
			if (forceRebuild)
			{
				numRegions = 1;
				xzRegions[0].xMin = 0;
				xzRegions[0].yMin = 0;
				xzRegions[0].xMax = settingsTextureSize;
				xzRegions[0].yMax = settingsTextureSize;
				numRegions2 = 1;
				yRegions[0].minIndex = 0;
				yRegions[0].maxIndex = settingsTextureSize;
				int3 = Int3.zero;
			}
			else
			{
				ComputeDirtyRegions(settingsTextureSize, scrollTextureMinIndex.x, scrollTextureMinIndex.z, ref int3.x, ref int3.z, xzRegions, out numRegions);
				ComputeDirtyRegions(settingsTextureSize, scrollTextureMinIndex.y, ref int3.y, yRegions, out numRegions2);
			}
			scrollTextureMinIndex = int3;
			scrollTextureMin = @int;
			int num = 0;
			switch (numRegions2)
			{
			case 0:
				num += UpdateBiomeRegions(0, settingsTextureSize, xzRegions, numRegions, yRegionFullUpdate: false);
				break;
			case 1:
				num += UpdateBiomeRegions(0, yRegions[0].minIndex, xzRegions, numRegions, yRegionFullUpdate: false);
				num += UpdateBiomeRegions(yRegions[0].minIndex, yRegions[0].maxIndex, xzRegions, numRegions, yRegionFullUpdate: true);
				num += UpdateBiomeRegions(yRegions[0].maxIndex, settingsTextureSize, xzRegions, numRegions, yRegionFullUpdate: false);
				break;
			case 2:
				num += UpdateBiomeRegions(yRegions[0].minIndex, yRegions[0].maxIndex, xzRegions, numRegions, yRegionFullUpdate: true);
				num += UpdateBiomeRegions(yRegions[0].maxIndex, yRegions[1].minIndex, xzRegions, numRegions, yRegionFullUpdate: false);
				num += UpdateBiomeRegions(yRegions[1].minIndex, yRegions[1].maxIndex, xzRegions, numRegions, yRegionFullUpdate: true);
				break;
			}
			biomeIndexMapTexture.SetPixels32(biomeIndexMap);
			biomeIndexMapTexture.Apply();
			BuildSettingsTextures();
		}
	}

	private Vector4 GetExtinctionTextureValue(WaterscapeVolume.Settings settings)
	{
		volume.GetTransmission();
		Vector4 extinctionAndScatteringCoefficients = settings.GetExtinctionAndScatteringCoefficients();
		Vector4 result = default(Vector4);
		result.x = extinctionAndScatteringCoefficients.x;
		result.y = extinctionAndScatteringCoefficients.y;
		result.z = extinctionAndScatteringCoefficients.z;
		result.w = settings.startDistance;
		return result;
	}

	private Vector4 GetScatteringTextureValue(WaterscapeVolume.Settings settings)
	{
		Vector4 extinctionAndScatteringCoefficients = settings.GetExtinctionAndScatteringCoefficients();
		Color linear = settings.scatteringColor.linear;
		Vector4 result = default(Vector4);
		result.x = linear.r * extinctionAndScatteringCoefficients.w;
		result.y = linear.g * extinctionAndScatteringCoefficients.w;
		result.z = linear.b * extinctionAndScatteringCoefficients.w;
		result.w = settings.sunlightScale * waterTransmission;
		return result;
	}

	private Vector4 GetEmissiveTextureValue(WaterscapeVolume.Settings settings)
	{
		Vector4 emissive = settings.GetEmissive();
		Vector4 result = default(Vector4);
		result.x = emissive.x;
		result.y = emissive.y;
		result.z = emissive.z;
		result.w = settings.ambientScale;
		return result;
	}

	private WaterscapeVolume.Settings GetBiomeIndexMapSettings(int biomeIndex)
	{
		WaterscapeVolume.Settings settings = defaultSettings;
		if (biomeIndex > 0)
		{
			settings = biomeSettings[biomeIndex - 1].settings;
		}
		return settings;
	}

	public void BuildSettingsTextures()
	{
		unwrapMaterial.SetFloat(ShaderPropertyID._TextureSize, settingsTextureSize);
		unwrapMaterial.SetFloat(ShaderPropertyID._InvTextureSize, 1f / (float)settingsTextureSize);
		unwrapMaterial.SetVector(ShaderPropertyID._TextureMinIndex, scrollTextureMinIndex.ToVector3());
		Graphics.Blit(biomeIndexMapTexture, biomeIndexMapUnwrappedTexture, unwrapMaterial);
		resizeMaterial.SetFloat(ShaderPropertyID._SrcTextureSize, settingsTextureSize);
		resizeMaterial.SetFloat(ShaderPropertyID._DstTextureSize, settingsTextureUpsampledSize);
		resizeMaterial.SetFloat(ShaderPropertyID._InvDstTextureSize, 1f / (float)settingsTextureUpsampledSize);
		Vector3 wsCenter = wsMin;
		wsCenter.x += regionBounds * 0.5f;
		wsCenter.y += regionBounds * 0.5f;
		wsCenter.z += regionBounds * 0.5f;
		RasterizeAtmosphereVolumes(wsCenter, highDetail: false);
		lookupMaterial.SetFloat(ShaderPropertyID._InvNumBiomes, 1f / (float)(biomeSettings.Count + 1));
		if (!AtmosphereDirector.ShadowsEnabled())
		{
			lookupMaterial.SetVector(ShaderPropertyID._Modifier, new Vector4(1f, 1f, 1f, 0f));
		}
		else
		{
			lookupMaterial.SetVector(ShaderPropertyID._Modifier, Vector4.one);
		}
		lookupMaterial.SetTexture(ShaderPropertyID._BiomeValueTex, biomeExtinctionTexture);
		Graphics.Blit(biomeIndexMapUnwrappedTexture, settingsTexture, lookupMaterial);
		Blur(settingsTexture, settingsTextureBlurred);
		Graphics.Blit(settingsTextureBlurred, extinctionTextureBlurred, resizeMaterial);
		lookupMaterial.SetVector(ShaderPropertyID._Modifier, Vector4.one);
		lookupMaterial.SetTexture(ShaderPropertyID._BiomeValueTex, biomeScatteringTexture);
		Graphics.Blit(biomeIndexMapUnwrappedTexture, settingsTexture, lookupMaterial);
		Blur(settingsTexture, settingsTextureBlurred);
		Graphics.Blit(settingsTextureBlurred, scatteringTextureBlurred, resizeMaterial);
		lookupMaterial.SetVector(ShaderPropertyID._Modifier, Vector4.one);
		lookupMaterial.SetTexture(ShaderPropertyID._BiomeValueTex, biomeEmissiveTexture);
		Graphics.Blit(biomeIndexMapUnwrappedTexture, settingsTexture, lookupMaterial);
		Blur(settingsTexture, settingsTextureBlurred);
		Graphics.Blit(settingsTextureBlurred, emissiveTextureBlurred, resizeMaterial);
		RasterizeAtmosphereVolumes(wsCenter, highDetail: true);
	}

	public Bounds GetVolumeBounds()
	{
		Bounds result = default(Bounds);
		result.SetMinMax(wsMin, wsMin + Vector3.one * regionBounds * 2f);
		return result;
	}

	private static void CanonicalizeCapsuleMatrix(ref Matrix4x4 worldToLocalMatrix, int axis, int canonicalAxis = 2)
	{
		if (axis != canonicalAxis)
		{
			for (int i = 0; i < 4; i++)
			{
				float value = worldToLocalMatrix[axis, i];
				worldToLocalMatrix[axis, i] = worldToLocalMatrix[canonicalAxis, i];
				worldToLocalMatrix[canonicalAxis, i] = value;
			}
		}
	}

	private void RasterizeAtmosphereVolumes(Vector3 wsCenter, bool highDetail)
	{
		if (AtmosphereDirector.main == null)
		{
			return;
		}
		List<AtmosphereVolume> volumes = AtmosphereDirector.main.GetVolumes();
		int num;
		if (highDetail)
		{
			colorTargets[0] = extinctionTextureBlurred.colorBuffer;
			colorTargets[1] = scatteringTextureBlurred.colorBuffer;
			colorTargets[2] = emissiveTextureBlurred.colorBuffer;
			Graphics.SetRenderTarget(colorTargets, extinctionTextureBlurred.depthBuffer);
			atmosphereVolumeMaterial.EnableKeyword("ENABLE_MRT");
			num = settingsTextureUpsampledSize;
		}
		else
		{
			Graphics.SetRenderTarget(biomeIndexMapUnwrappedTexture);
			atmosphereVolumeMaterial.DisableKeyword("ENABLE_MRT");
			num = settingsTextureSize;
		}
		GL.PushMatrix();
		GL.LoadOrtho();
		float value = 2f * regionBounds / (float)num;
		atmosphereVolumeMaterial.SetFloat(ShaderPropertyID._TextureSize, num);
		atmosphereVolumeMaterial.SetFloat(ShaderPropertyID._MetersPerPixel, value);
		Bounds volumeBounds = GetVolumeBounds();
		for (int i = 0; i < volumes.Count; i++)
		{
			AtmosphereVolume atmosphereVolume = volumes[i];
			if (atmosphereVolume.highDetail != highDetail)
			{
				continue;
			}
			Collider collider = atmosphereVolume.GetCollider();
			Bounds bounds = collider.bounds;
			if (!volumeBounds.Intersects(bounds))
			{
				continue;
			}
			SphereCollider sphereCollider = collider as SphereCollider;
			BoxCollider boxCollider = collider as BoxCollider;
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			int num2 = -1;
			Matrix4x4 worldToLocalMatrix = collider.transform.worldToLocalMatrix * Matrix4x4.TRS(wsMin, Quaternion.identity, Vector3.one);
			if (sphereCollider != null)
			{
				worldToLocalMatrix[0, 3] -= sphereCollider.center.x;
				worldToLocalMatrix[1, 3] -= sphereCollider.center.y;
				worldToLocalMatrix[2, 3] -= sphereCollider.center.z;
				atmosphereVolumeMaterial.SetMatrix(ShaderPropertyID._WorldToLocalMatrix, worldToLocalMatrix);
				atmosphereVolumeMaterial.SetFloat(ShaderPropertyID._SphereRadius, sphereCollider.radius);
				num2 = 0;
			}
			else if (boxCollider != null)
			{
				worldToLocalMatrix[0, 3] -= boxCollider.center.x;
				worldToLocalMatrix[1, 3] -= boxCollider.center.y;
				worldToLocalMatrix[2, 3] -= boxCollider.center.z;
				atmosphereVolumeMaterial.SetMatrix(ShaderPropertyID._WorldToLocalMatrix, worldToLocalMatrix);
				atmosphereVolumeMaterial.SetVector(ShaderPropertyID._BoxExtents, boxCollider.size * 0.5f);
				num2 = 1;
			}
			else
			{
				if (!(capsuleCollider != null))
				{
					continue;
				}
				worldToLocalMatrix[0, 3] -= capsuleCollider.center.x;
				worldToLocalMatrix[1, 3] -= capsuleCollider.center.y;
				worldToLocalMatrix[2, 3] -= capsuleCollider.center.z;
				CanonicalizeCapsuleMatrix(ref worldToLocalMatrix, capsuleCollider.direction);
				float radius = capsuleCollider.radius;
				float y = capsuleCollider.height * 0.5f - radius;
				atmosphereVolumeMaterial.SetMatrix(ShaderPropertyID._WorldToLocalMatrix, worldToLocalMatrix);
				atmosphereVolumeMaterial.SetVector(ShaderPropertyID._CapsuleRadiusExtent, new Vector2(radius, y));
				num2 = 2;
			}
			if (highDetail)
			{
				int biomeIndex = GetBiomeIndex(atmosphereVolume.overrideBiome) + 1;
				WaterscapeVolume.Settings biomeIndexMapSettings = GetBiomeIndexMapSettings(biomeIndex);
				atmosphereVolumeMaterial.SetVector(ShaderPropertyID._Value, GetExtinctionTextureValue(biomeIndexMapSettings));
				atmosphereVolumeMaterial.SetVector(ShaderPropertyID._Value1, GetScatteringTextureValue(biomeIndexMapSettings));
				atmosphereVolumeMaterial.SetVector(ShaderPropertyID._Value2, GetEmissiveTextureValue(biomeIndexMapSettings));
			}
			else
			{
				byte b = (byte)(GetBiomeIndex(atmosphereVolume.overrideBiome) + 1);
				float y2 = ((atmosphereVolume.sun.enabled && atmosphereVolume.sun.shadowed) ? 1f : 0f);
				Vector4 value2 = new Vector4((float)(int)b / 255f, y2, 0f, 1f);
				atmosphereVolumeMaterial.SetVector(ShaderPropertyID._Value, value2);
			}
			atmosphereVolumeMaterial.SetPass(num2);
			GL.Begin(4);
			GL.Vertex3(0f, 0f, 0f);
			GL.Vertex3(1f, 0f, 0f);
			GL.Vertex3(1f, 1f, 0f);
			GL.Vertex3(1f, 1f, 0f);
			GL.Vertex3(0f, 1f, 0f);
			GL.Vertex3(0f, 0f, 0f);
			GL.End();
		}
		GL.PopMatrix();
	}

	private int GetBiomeIndex(string name)
	{
		if (!string.IsNullOrEmpty(name) && biomeLookup.TryGetValue(name, out var value))
		{
			return value;
		}
		return -1;
	}

	public bool GetSettings(Vector3 wsPosition, bool onlyAffectsVisuals, out WaterscapeVolume.Settings settings)
	{
		int num = -1;
		if ((bool)LargeWorld.main)
		{
			num = GetBiomeIndex(GetBiome(wsPosition, onlyAffectsVisuals));
		}
		if (num == -1)
		{
			settings = defaultSettings;
			return false;
		}
		settings = biomeSettings[num].settings;
		return true;
	}

	public bool GetSettings(string biomeName, out WaterscapeVolume.Settings settings)
	{
		int num = -1;
		if ((bool)LargeWorld.main)
		{
			num = GetBiomeIndex(biomeName);
		}
		if (num == -1)
		{
			settings = defaultSettings;
			return false;
		}
		settings = biomeSettings[num].settings;
		return true;
	}

	public void SetupConstantsForCamera(Camera camera)
	{
		Matrix4x4 cameraToWorldMatrix = camera.cameraToWorldMatrix;
		Matrix4x4 identity = Matrix4x4.identity;
		float num = 1f / (2f * regionBounds);
		identity.SetColumn(3, new Vector4((0f - wsMin.x) * num, (0f - wsMin.y) * num, (0f - wsMin.z) * num, 1f));
		identity[0, 0] *= num;
		identity[1, 1] *= num;
		identity[2, 2] *= num;
		Matrix4x4 value = identity * cameraToWorldMatrix;
		Shader.SetGlobalMatrix(ShaderPropertyID._UweCameraToVolumeMatrix, value);
		Shader.SetGlobalMatrix(ShaderPropertyID._UweWorldToVolumeMatrix, identity);
	}

	private void Blur(RenderTexture texture, RenderTexture dst)
	{
		if (enableBlur)
		{
			RenderTexture renderTexture = blurTempTexture;
			blurMaterial.SetVector(ShaderPropertyID._TextureSize, new Vector3(settingsTextureSize, settingsTextureSize, settingsTextureSize));
			blurMaterial.SetVector(ShaderPropertyID._InvTextureSize, new Vector3(1f / (float)settingsTextureSize, 1f / (float)settingsTextureSize, 1f / (float)settingsTextureSize));
			Graphics.Blit(texture, dst, blurMaterial, 0);
			Graphics.Blit(dst, renderTexture, blurMaterial, 1);
			dst.DiscardContents();
			Graphics.Blit(renderTexture, dst, blurMaterial, 2);
			renderTexture.DiscardContents();
		}
		else
		{
			Graphics.Blit(texture, dst);
		}
	}

	private static void ComputeDirtyRegions(int textureSize, int minIndex, ref int newMinIndex, Region1D[] regions, out int numRegions)
	{
		numRegions = 0;
		int num = newMinIndex - minIndex;
		if (num >= textureSize || num <= -textureSize)
		{
			regions[numRegions].minIndex = 0;
			regions[numRegions].maxIndex = textureSize;
			numRegions++;
			newMinIndex = 0;
		}
		else if (num > 0)
		{
			regions[numRegions].minIndex = minIndex;
			regions[numRegions].maxIndex = newMinIndex;
			if (regions[numRegions].maxIndex > textureSize)
			{
				regions[numRegions].minIndex = 0;
				regions[numRegions].maxIndex = newMinIndex - textureSize;
				numRegions++;
				regions[numRegions].minIndex = minIndex;
				regions[numRegions].maxIndex = textureSize;
			}
			numRegions++;
			newMinIndex %= textureSize;
		}
		else
		{
			if (num >= 0)
			{
				return;
			}
			regions[numRegions].minIndex = newMinIndex;
			regions[numRegions].maxIndex = minIndex;
			if (regions[numRegions].minIndex < 0)
			{
				regions[numRegions].minIndex = 0;
				if (regions[numRegions].minIndex != regions[numRegions].maxIndex)
				{
					numRegions++;
				}
				regions[numRegions].minIndex = textureSize + newMinIndex;
				regions[numRegions].maxIndex = textureSize;
				newMinIndex = textureSize + newMinIndex;
			}
			numRegions++;
		}
	}

	private static void ComputeDirtyRegions(int textureSize, int xMinIndex, int yMinIndex, ref int xNewMinIndex, ref int yNewMinIndex, Region2D[] regions, out int numRegions)
	{
		numRegions = 0;
		int numRegions2 = 0;
		ComputeDirtyRegions(textureSize, xMinIndex, ref xNewMinIndex, cxRegions, out numRegions2);
		for (int i = 0; i < numRegions2; i++)
		{
			regions[numRegions].xMin = cxRegions[i].minIndex;
			regions[numRegions].yMin = 0;
			regions[numRegions].xMax = cxRegions[i].maxIndex;
			regions[numRegions].yMax = textureSize;
			numRegions++;
		}
		int numRegions3 = 0;
		ComputeDirtyRegions(textureSize, yMinIndex, ref yNewMinIndex, cyRegions, out numRegions3);
		for (int j = 0; j < numRegions3; j++)
		{
			Region1D region1D = cyRegions[j];
			switch (numRegions2)
			{
			case 1:
				if (cxRegions[0].maxIndex - cxRegions[0].minIndex != textureSize)
				{
					if (cxRegions[0].minIndex != 0)
					{
						regions[numRegions].xMin = 0;
						regions[numRegions].yMin = region1D.minIndex;
						regions[numRegions].xMax = cxRegions[0].minIndex;
						regions[numRegions].yMax = region1D.maxIndex;
						numRegions++;
					}
					if (cxRegions[0].maxIndex != textureSize)
					{
						regions[numRegions].xMin = cxRegions[0].maxIndex;
						regions[numRegions].yMin = region1D.minIndex;
						regions[numRegions].xMax = textureSize;
						regions[numRegions].yMax = region1D.maxIndex;
						numRegions++;
					}
				}
				break;
			case 2:
				if (cxRegions[0].minIndex < cxRegions[1].minIndex)
				{
					regions[numRegions].xMin = cxRegions[0].maxIndex;
					regions[numRegions].xMax = cxRegions[1].minIndex;
				}
				else
				{
					regions[numRegions].xMin = cxRegions[1].maxIndex;
					regions[numRegions].xMax = cxRegions[0].minIndex;
				}
				regions[numRegions].yMin = region1D.minIndex;
				regions[numRegions].yMax = region1D.maxIndex;
				numRegions++;
				break;
			default:
				regions[numRegions].xMin = 0;
				regions[numRegions].yMin = region1D.minIndex;
				regions[numRegions].xMax = textureSize;
				regions[numRegions].yMax = region1D.maxIndex;
				numRegions++;
				break;
			}
		}
	}

	public string GetBiome(Vector3 wsPosition, bool onlyAffectsVisuals = true)
	{
		if (world == null)
		{
			return null;
		}
		AtmosphereVolume enclosingAtmosphereVolume = GetEnclosingAtmosphereVolume(wsPosition, onlyAffectsVisuals);
		if (enclosingAtmosphereVolume != null)
		{
			return enclosingAtmosphereVolume.overrideBiome;
		}
		return world.GetBiome(wsPosition);
	}

	private AtmosphereVolume GetEnclosingAtmosphereVolume(Vector3 wsPosition, bool onlyAffectsVisuals = true)
	{
		float radius = 0f;
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(wsPosition, radius, atmosphereVolumesLayerMask, QueryTriggerInteraction.Collide);
		AtmosphereVolume result = null;
		if (num > 0)
		{
			ulong num2 = 0uL;
			for (int i = 0; i < num; i++)
			{
				AtmosphereVolume component = UWE.Utils.sharedColliderBuffer[i].GetComponent<AtmosphereVolume>();
				if (component != null && (!onlyAffectsVisuals || component.affectsVisuals))
				{
					ulong sortKey = component.GetSortKey();
					if (sortKey >= num2)
					{
						num2 = sortKey;
						result = component;
					}
				}
			}
		}
		return result;
	}

	private void OnDrawGizmos()
	{
		if (AtmosphereDirector.main == null)
		{
			return;
		}
		Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
		Gizmos.matrix = Matrix4x4.identity;
		Bounds volumeBounds = GetVolumeBounds();
		Gizmos.DrawWireCube(volumeBounds.center, volumeBounds.size);
		List<AtmosphereVolume> volumes = AtmosphereDirector.main.GetVolumes();
		int num = 0;
		for (int i = 0; i < volumes.Count; i++)
		{
			AtmosphereVolume atmosphereVolume = volumes[i];
			Collider collider = atmosphereVolume.GetCollider();
			Bounds bounds = collider.bounds;
			if (volumeBounds.Intersects(bounds))
			{
				SphereCollider sphereCollider = collider as SphereCollider;
				BoxCollider boxCollider = collider as BoxCollider;
				CapsuleCollider capsuleCollider = collider as CapsuleCollider;
				Gizmos.matrix = atmosphereVolume.transform.localToWorldMatrix;
				Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
				if (sphereCollider != null)
				{
					Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
					num++;
				}
				else if (boxCollider != null)
				{
					Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
					num++;
				}
				else if (capsuleCollider != null)
				{
					num++;
				}
			}
		}
	}

	private void OnGUI()
	{
		if (debugBiomeMap && Event.current.type == EventType.Repaint)
		{
			float x = Screen.height / settingsTextureSize;
			float y = Screen.height;
			debugMaterial.SetTexture(ShaderPropertyID._MainTex, biomeIndexMapUnwrappedTexture);
			debugMaterial.SetPass(0);
			GL.Begin(7);
			GL.TexCoord2(0f, 0f);
			GL.Vertex3(0f, 0f, 0f);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(x, 0f, 0f);
			GL.TexCoord2(1f, 1f);
			GL.Vertex3(x, y, 0f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(0f, y, 0f);
			GL.End();
		}
	}

	private void BuildSettingsArrayTextures()
	{
		if (biomeExtinctionTexture != null)
		{
			UWE.Utils.DestroyWrap(biomeExtinctionTexture);
			UWE.Utils.DestroyWrap(biomeScatteringTexture);
			UWE.Utils.DestroyWrap(biomeEmissiveTexture);
		}
		int num = biomeSettings.Count + 1;
		biomeExtinctionTexture = new Texture2D(1, num, TextureFormat.RGBAFloat, mipChain: false, linear: true);
		biomeExtinctionTexture.filterMode = FilterMode.Point;
		biomeScatteringTexture = new Texture2D(1, num, TextureFormat.RGBAFloat, mipChain: false, linear: true);
		biomeScatteringTexture.filterMode = FilterMode.Point;
		biomeEmissiveTexture = new Texture2D(1, num, TextureFormat.RGBAFloat, mipChain: false, linear: true);
		biomeEmissiveTexture.filterMode = FilterMode.Point;
		for (int i = 0; i < num; i++)
		{
			WaterscapeVolume.Settings biomeIndexMapSettings = GetBiomeIndexMapSettings(i);
			biomeExtinctionTexture.SetPixel(0, i, GetExtinctionTextureValue(biomeIndexMapSettings));
			biomeScatteringTexture.SetPixel(0, i, GetScatteringTextureValue(biomeIndexMapSettings));
			biomeEmissiveTexture.SetPixel(0, i, GetEmissiveTextureValue(biomeIndexMapSettings));
		}
		biomeExtinctionTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		biomeScatteringTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		biomeEmissiveTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
	}

	public Sky GetBiomeEnvironment(Vector3 wsPosition)
	{
		Sky result = null;
		int biomeIndex = GetBiomeIndex(GetBiome(wsPosition));
		if (biomeIndex != -1)
		{
			result = biomeSkies[biomeIndex];
		}
		else if (biomeSkies.Count > 0)
		{
			result = biomeSkies[0];
		}
		return result;
	}

	public string CompileTimeCheck()
	{
		foreach (IGrouping<string, BiomeSettings> item in from x in biomeSettings
			group x by x.name)
		{
			if (item.Count() > 1)
			{
				return $"Duplicate biome name {item.First().name}";
			}
		}
		return null;
	}
}
