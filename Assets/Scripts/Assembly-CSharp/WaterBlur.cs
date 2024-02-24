using UnityEngine;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class WaterBlur : PostEffectsBase
{
	public enum BlurSampleCount
	{
		Medium = 0,
		High = 1
	}

	private enum Pass
	{
		CaptureCoc = 0,
		Tap4BlurForLRSpawn = 1,
		BlurInsaneMQ = 2,
		BlurUpsampleCombineMQ = 3,
		Visualize = 4,
		BlurInsaneHQ = 5,
		BlurUpsampleCombineHQ = 6
	}

	public bool visualizeFocus;

	public float maxBlurSize = 2f;

	public bool highResolution;

	public float startDistance;

	public float endDistance = 100f;

	public float underWaterBoost = 0.004f;

	public WBOIT transparency;

	public BlurSampleCount blurSampleCount = BlurSampleCount.High;

	public Shader dofHdrShader;

	private Material dofHdrMaterial;

	public override bool CheckResources()
	{
		CheckSupport(needDepth: true);
		dofHdrMaterial = CheckShaderAndCreateMaterial(dofHdrShader, dofHdrMaterial);
		if (!isSupported)
		{
			ReportAutoDisable();
		}
		return isSupported;
	}

	private void OnEnable()
	{
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}

	private void OnDisable()
	{
		if ((bool)dofHdrMaterial)
		{
			Object.DestroyImmediate(dofHdrMaterial);
		}
		dofHdrMaterial = null;
	}

	private void WriteCoc(RenderTexture fromTo, bool fgDilate)
	{
		fromTo.MarkRestoreExpected();
		Graphics.Blit(fromTo, fromTo, dofHdrMaterial, 0);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!CheckResources())
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (maxBlurSize < 0.1f)
		{
			maxBlurSize = 0.1f;
		}
		float num = Mathf.Max(maxBlurSize, 0f);
		RenderTexture renderTexture = null;
		RenderTexture renderTexture2 = null;
		dofHdrMaterial.SetTexture(ShaderPropertyID._WBOITATexture, transparency.GetTextureA());
		dofHdrMaterial.SetTexture(ShaderPropertyID._WBOITBTexture, transparency.GetTextureB());
		dofHdrMaterial.SetFloat(ShaderPropertyID._UnderWaterBoost, underWaterBoost);
		if (visualizeFocus)
		{
			WriteCoc(source, fgDilate: true);
			Graphics.Blit(source, destination, dofHdrMaterial, 4);
		}
		else
		{
			source.filterMode = FilterMode.Bilinear;
			if (highResolution)
			{
				num *= 2f;
			}
			WriteCoc(source, fgDilate: true);
			renderTexture = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
			renderTexture2 = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
			dofHdrMaterial.SetVector(ShaderPropertyID._BlurParams, new Vector2(startDistance, 1f / (endDistance - startDistance)));
			int pass = ((blurSampleCount == BlurSampleCount.High || blurSampleCount == BlurSampleCount.Medium) ? 5 : 2);
			if (highResolution)
			{
				dofHdrMaterial.SetVector(ShaderPropertyID._Offsets, new Vector4(0f, num, 0.025f, num));
				Graphics.Blit(source, destination, dofHdrMaterial, pass);
			}
			else
			{
				dofHdrMaterial.SetVector(ShaderPropertyID._Offsets, new Vector4(0f, num, 0.1f, num));
				Graphics.Blit(source, renderTexture, dofHdrMaterial, 1);
				Graphics.Blit(renderTexture, renderTexture2, dofHdrMaterial, pass);
				dofHdrMaterial.SetTexture(ShaderPropertyID._LowRez, renderTexture2);
				dofHdrMaterial.SetVector(ShaderPropertyID._Offsets, Vector4.one * (1f * (float)source.width / (1f * (float)renderTexture2.width)) * num);
				Graphics.Blit(source, destination, dofHdrMaterial, (blurSampleCount == BlurSampleCount.High) ? 6 : 3);
			}
		}
		if ((bool)renderTexture)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		if ((bool)renderTexture2)
		{
			RenderTexture.ReleaseTemporary(renderTexture2);
		}
	}
}
