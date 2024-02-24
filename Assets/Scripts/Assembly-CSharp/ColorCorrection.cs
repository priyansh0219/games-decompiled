using UnityEngine;

public class ColorCorrection : MonoBehaviour
{
	public Shader shader;

	private Material material;

	public float underWaterExposure = 1f;

	public float aboveWaterExposure = 0.5f;

	public float adaptationSpeed = 1f;

	public bool adjustWithDepth = true;

	public float depthAdjustment = 0.1f;

	private float exposure;

	private void Start()
	{
		material = new Material(shader);
		material.hideFlags = HideFlags.HideAndDontSave;
		exposure = aboveWaterExposure;
	}

	private void Update()
	{
		float target;
		if (MainCamera.camera.transform.position.y > 0f || Player.main == null || Player.main.IsInsideWalkable())
		{
			target = aboveWaterExposure;
		}
		else if (adjustWithDepth)
		{
			float num = 0f - MainCamera.camera.transform.position.y;
			target = underWaterExposure / Mathf.Exp((0f - num) * depthAdjustment);
		}
		else
		{
			target = underWaterExposure;
		}
		exposure = Mathf.MoveTowards(exposure, target, adaptationSpeed * Time.deltaTime);
		material.SetFloat(ShaderPropertyID._Exposure, exposure);
	}

	[ImageEffectTransformsToLDR]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, material);
	}
}
