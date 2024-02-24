using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Camera))]
public class MenuSceneFadeIn : MonoBehaviour
{
	public WaterSurface waterSurface;

	public Shader overlayShader;

	public float fadeTime = 1f;

	private float overlayFadeValue;

	private float mainFadeValue;

	public LayerMask overlayMask;

	private Material overlayMaterial;

	public RenderTexture targetTexture;

	private Camera camera;

	private bool renderingOverlay;

	private void Start()
	{
		overlayMaterial = new Material(overlayShader);
		overlayMaterial.hideFlags = HideFlags.HideAndDontSave;
		camera = GetComponent<Camera>();
		targetTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
	}

	private void Update()
	{
		overlayFadeValue = Mathf.Clamp01(overlayFadeValue + Time.deltaTime / fadeTime);
		if (!waterSurface.IsLoadingDisplacementTextures())
		{
			mainFadeValue = Mathf.Clamp01(mainFadeValue + Time.deltaTime / fadeTime);
		}
	}

	private void LateUpdate()
	{
		if (mainFadeValue < 1f)
		{
			renderingOverlay = true;
			RenderOverlay();
			renderingOverlay = false;
		}
	}

	private RenderTexture RenderOverlay()
	{
		RenderTexture active = RenderTexture.active;
		if (XRSettings.enabled)
		{
			RenderTexture.active = targetTexture;
			GL.Clear(clearDepth: true, clearColor: true, Color.black);
		}
		else
		{
			Color backgroundColor = camera.backgroundColor;
			camera.backgroundColor = Color.black;
			CameraClearFlags clearFlags = camera.clearFlags;
			camera.clearFlags = CameraClearFlags.Color;
			RenderTexture renderTexture = camera.targetTexture;
			camera.targetTexture = targetTexture;
			LayerMask layerMask = camera.cullingMask;
			camera.cullingMask = overlayMask;
			camera.Render();
			camera.cullingMask = layerMask;
			camera.targetTexture = renderTexture;
			camera.clearFlags = clearFlags;
			camera.backgroundColor = backgroundColor;
		}
		RenderTexture.active = active;
		return targetTexture;
	}

	public void OnPostRender()
	{
		if (!renderingOverlay && mainFadeValue < 1f)
		{
			GL.PushMatrix();
			GL.LoadOrtho();
			overlayMaterial.mainTexture = targetTexture;
			overlayMaterial.SetPass(0);
			float num = Mathf.Pow(overlayFadeValue, 5f);
			GL.Begin(7);
			GL.Color(new Color(num, num, num, 1f - mainFadeValue));
			GL.TexCoord2(0f, 0f);
			GL.Vertex3(0f, 0f, 0f);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(1f, 0f, 0f);
			GL.TexCoord2(1f, 1f);
			GL.Vertex3(1f, 1f, 0f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(0f, 1f, 0f);
			GL.End();
			GL.PopMatrix();
		}
	}
}
