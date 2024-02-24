using UnityEngine;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
public class MinimapShader : PostEffectsBase
{
	public Shader shader;

	private Material material;

	public override bool CheckResources()
	{
		CheckSupport(needDepth: true);
		material = CheckShaderAndCreateMaterial(shader, material);
		if (!isSupported)
		{
			ReportAutoDisable();
		}
		return isSupported;
	}

	private void SetCameraFlag()
	{
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!CheckResources())
		{
			Graphics.Blit(source, destination);
		}
		else
		{
			Graphics.Blit(source, destination, material);
		}
	}
}
