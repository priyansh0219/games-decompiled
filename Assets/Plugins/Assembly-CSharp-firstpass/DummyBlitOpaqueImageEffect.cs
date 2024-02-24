using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DummyBlitOpaqueImageEffect : MonoBehaviour
{
	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination);
	}
}
