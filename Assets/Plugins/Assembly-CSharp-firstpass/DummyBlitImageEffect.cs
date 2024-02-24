using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DummyBlitImageEffect : MonoBehaviour
{
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination);
	}
}
