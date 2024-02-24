using UWE;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class VisualizeDepth : ImageEffectWithEvents
{
	public Shader shader;

	private Material material;

	public bool eventsOnly;

	private void Awake()
	{
		material = new Material(shader);
		material.hideFlags = HideFlags.HideAndDontSave;
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}

	public override bool CheckResources()
	{
		return true;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		using (new OnRenderImageWrapper(this, source, destination))
		{
			if (!eventsOnly)
			{
				Graphics.Blit(source, destination, material);
			}
			else
			{
				Graphics.Blit(source, destination);
			}
		}
	}
}
