using UnityEngine;

public class InsideOutsideVisualizer : MonoBehaviour
{
	public Shader shader;

	private Material material;

	private void Start()
	{
		material = new Material(shader);
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(src, dest, material);
	}
}
