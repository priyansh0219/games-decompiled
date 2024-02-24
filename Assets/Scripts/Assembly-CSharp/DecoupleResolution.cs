using UnityEngine;

public class DecoupleResolution : MonoBehaviour
{
	[AssertNotNull]
	public Camera mainCamera;

	[AssertNotNull]
	public Camera uiCamera;

	[AssertNotNull]
	public Shader transferDepthShader;

	[AssertNotNull]
	public Shader blitAlphaShader;

	private Material transferDepthMaterial;

	private RenderTexture uiCameraTarget;

	private RenderTexture mainCameraTarget;

	private void Start()
	{
		if (!DynamicResolution.IsEnabled())
		{
			base.enabled = false;
			return;
		}
		transferDepthMaterial = new Material(transferDepthShader);
		CreateRenderTargets();
		mainCamera.depthTextureMode |= DepthTextureMode.Depth;
	}

	private void Update()
	{
		if (Screen.width != uiCameraTarget.width || Screen.height != uiCameraTarget.height)
		{
			DestroyRenderTargets();
			CreateRenderTargets();
		}
	}

	private void DestroyRenderTargets()
	{
		if (uiCamera != null)
		{
			uiCamera.SetTargetBuffers(default(RenderBuffer), default(RenderBuffer));
		}
		if (uiCameraTarget != null)
		{
			uiCameraTarget.Release();
			uiCameraTarget = null;
		}
		if (mainCamera != null)
		{
			mainCamera.targetTexture = null;
		}
		if (mainCameraTarget != null)
		{
			mainCameraTarget.Release();
			mainCameraTarget = null;
		}
	}

	private void CreateRenderTargets()
	{
		uiCameraTarget = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
		uiCamera.SetTargetBuffers(uiCameraTarget.colorBuffer, uiCameraTarget.depthBuffer);
		mainCameraTarget = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
		mainCameraTarget.useDynamicScale = true;
		mainCamera.targetTexture = mainCameraTarget;
	}

	private void OnPreRender()
	{
		Graphics.SetRenderTarget(uiCameraTarget);
		GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
		Graphics.Blit(mainCameraTarget, uiCameraTarget);
		Graphics.Blit(null, uiCameraTarget, transferDepthMaterial);
	}

	private void OnRenderImage(RenderTexture _src, RenderTexture dst)
	{
		Graphics.Blit(uiCameraTarget, dst);
	}

	private void OnDestroy()
	{
		DestroyRenderTargets();
	}
}
