using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DummyBlit : MonoBehaviour
{
	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(src, dest);
	}
}
