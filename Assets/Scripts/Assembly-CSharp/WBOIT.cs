using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
public class WBOIT : PostEffectsBase
{
	private static Color clearColor = new Color(0f, 0f, 0f, 1f);

	private static readonly RenderTargetIdentifier[] renderTargetIdentifiers = new RenderTargetIdentifier[3];

	[Header("WBOIT")]
	[AssertNotNull]
	[SerializeField]
	private Camera camera;

	[AssertNotNull]
	[SerializeField]
	private Camera guiCamera;

	private RenderTexture wboitTexture1;

	private RenderTexture wboitTexture2;

	public Shader compositeShader;

	private Material compositeMaterial;

	[Header("Hot temperature refraction")]
	public float temperatureScalar;

	public Texture2D temperatureRefractTex;

	private bool temperatureRefractEnabled;

	private int texAPropertyID = -1;

	private int texBPropertyID = -1;

	private int weightTogglePropertyID = -1;

	private int weightSharpnessPropertyID = -1;

	private int temperatureTexPropertyID = -1;

	private int temperaturePropertyID = -1;

	public bool useDepthWeighting = true;

	public float depthWeightingSharpness = 0.1f;

	private float nextTemperatureUpdate;

	private CommandBuffer buffer;

	protected static HashSet<VFXOverlayMaterial> overlays = new HashSet<VFXOverlayMaterial>();

	public bool debug;

	private bool hadVisibleRenderers;

	public override bool CheckResources()
	{
		return true;
	}

	private void Awake()
	{
		compositeMaterial = new Material(compositeShader);
		InitProperties();
	}

	private void OnDestroy()
	{
		overlays.Clear();
		DestroyRenderTargets();
	}

	public Texture GetTextureA()
	{
		return wboitTexture1;
	}

	public Texture GetTextureB()
	{
		return wboitTexture2;
	}

	public static void RegisterOverlay(VFXOverlayMaterial instance)
	{
		overlays.Add(instance);
	}

	public static void UnregisterOverlay(VFXOverlayMaterial instance)
	{
		overlays.Remove(instance);
	}

	private void InitRenderTargets()
	{
		Vector2Int screenSize = GraphicsUtil.GetScreenSize();
		if (wboitTexture1 != null && (screenSize.x != wboitTexture1.width || screenSize.y != wboitTexture1.height))
		{
			DestroyRenderTargets();
		}
		if (!(wboitTexture1 != null))
		{
			wboitTexture1 = DynamicResolution.CreateRenderTexture(screenSize.x, screenSize.y, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
			wboitTexture1.name = "WBOIT TexA";
			wboitTexture2 = DynamicResolution.CreateRenderTexture(screenSize.x, screenSize.y, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
			wboitTexture2.name = "WBOIT TexB";
			EditorModifications.SetOITTargets(camera, wboitTexture1, wboitTexture2);
			renderTargetIdentifiers[0] = BuiltinRenderTextureType.CameraTarget;
			renderTargetIdentifiers[1] = new RenderTargetIdentifier(wboitTexture1);
			renderTargetIdentifiers[2] = new RenderTargetIdentifier(wboitTexture2);
			compositeMaterial.SetTexture(texAPropertyID, wboitTexture1);
			compositeMaterial.SetTexture(texBPropertyID, wboitTexture2);
		}
	}

	private void DestroyRenderTargets()
	{
		EditorModifications.SetOITTargets(camera, null, null);
		if (wboitTexture1 != null)
		{
			wboitTexture1.Release();
			wboitTexture1 = null;
		}
		if (wboitTexture2 != null)
		{
			wboitTexture2.Release();
			wboitTexture2 = null;
		}
	}

	private void UpdateGlobalShaderParameters()
	{
		Shader.SetGlobalFloat(weightTogglePropertyID, useDepthWeighting ? 1f : 0f);
		Shader.SetGlobalFloat(weightSharpnessPropertyID, depthWeightingSharpness);
	}

	private void UpdateMaterialShaderParameters()
	{
		if (!debug && Time.time > nextTemperatureUpdate)
		{
			float temperature = GetTemperature();
			temperatureScalar = Mathf.Clamp01((temperature - 40f) / 30f);
			nextTemperatureUpdate = Time.time + Random.value;
		}
		if (temperatureScalar > 0f && temperatureRefractTex != null)
		{
			if (!temperatureRefractEnabled)
			{
				compositeMaterial.EnableKeyword("FX_TEMPERATURE_REFRACT");
				temperatureRefractEnabled = true;
			}
			compositeMaterial.SetTexture(temperatureTexPropertyID, temperatureRefractTex);
			compositeMaterial.SetFloat(temperaturePropertyID, temperatureScalar);
		}
		else if (temperatureRefractEnabled)
		{
			compositeMaterial.DisableKeyword("FX_TEMPERATURE_REFRACT");
			temperatureRefractEnabled = false;
		}
	}

	private void InitProperties()
	{
		texAPropertyID = Shader.PropertyToID("_WBOIT_texA");
		texBPropertyID = Shader.PropertyToID("_WBOIT_texB");
		weightTogglePropertyID = Shader.PropertyToID("_WBOIT_WeightToggle");
		weightSharpnessPropertyID = Shader.PropertyToID("_WBOIT_WeightSharpness");
		temperatureTexPropertyID = Shader.PropertyToID("_TemperatureTex");
		temperaturePropertyID = Shader.PropertyToID("_TemperatureScalar");
	}

	private float GetTemperature()
	{
		WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
		if (!(main != null))
		{
			return 0f;
		}
		return main.GetTemperature(Utils.GetLocalPlayerPos());
	}

	public void SetLoadingScreenOptimizations(bool isLoading)
	{
		if ((bool)camera)
		{
			camera.enabled = !isLoading;
			camera.GetComponent<StreamingController>().SetPreloading();
		}
	}

	private void OnPreRender()
	{
		if (base.enabled)
		{
			InitRenderTargets();
			Graphics.SetRenderTarget(wboitTexture1);
			GL.Clear(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 1f));
			Graphics.SetRenderTarget(wboitTexture2);
			GL.Clear(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 1f));
			UpdateGlobalShaderParameters();
			UpdateMaterialShaderParameters();
			RebuildCommandBuffer();
		}
	}

	private void RebuildCommandBuffer()
	{
		if (buffer == null)
		{
			buffer = new CommandBuffer();
			buffer.name = "WBOIT Overlays";
		}
		buffer.Clear();
		buffer.BeginSample("WBOIT Overlays");
		bool flag = false;
		buffer.SetRenderTarget(renderTargetIdentifiers, BuiltinRenderTextureType.CameraTarget);
		foreach (VFXOverlayMaterial overlay in overlays)
		{
			flag |= overlay.FillBuffer(buffer);
		}
		buffer.EndSample("WBOIT Overlays");
		if (flag != hadVisibleRenderers)
		{
			hadVisibleRenderers = flag;
			if (flag)
			{
				camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, buffer);
			}
			else
			{
				camera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, buffer);
			}
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit(src, dst, compositeMaterial);
	}
}
