using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LensWater : MonoBehaviour
{
	public Shader shader;

	private Material material;

	public Texture noiseTexture;

	public float flowSpeed = 10f;

	public float duration = 1.5f;

	private float startTime = float.NegativeInfinity;

	public float waterLevel;

	private void Start()
	{
		material = new Material(shader);
		material.hideFlags = HideFlags.HideAndDontSave;
	}

	public void CreateSplash()
	{
		if (!base.enabled)
		{
			startTime = Time.time;
			base.enabled = true;
		}
	}

	private float GetAmount()
	{
		return 1f - Mathf.Min((Time.time - startTime) / duration, 1f);
	}

	private void OnPreRender()
	{
		if ((double)GetAmount() < 0.0001)
		{
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		float amount = GetAmount();
		material.SetTexture(ShaderPropertyID._NoiseTexture, noiseTexture);
		material.SetFloat(ShaderPropertyID._FlowSpeed, flowSpeed);
		material.SetFloat(ShaderPropertyID._Amount, amount);
		Graphics.Blit(source, destination, material);
	}
}
