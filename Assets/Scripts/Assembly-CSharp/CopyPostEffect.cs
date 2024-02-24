using UnityEngine;

[ExecuteInEditMode]
public class CopyPostEffect : MonoBehaviour
{
	private Texture copyTexture;

	public void SetCopyTexture(Texture _copyTexture)
	{
		copyTexture = _copyTexture;
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit(copyTexture, dst);
	}
}
