using UnityEngine;

[ExecuteInEditMode]
public class RadiationsScreenFX : MonoBehaviour
{
	public Shader shader;

	public Color color;

	public float noiseFactor = 0.005f;

	private Material mat;

	private void Start()
	{
		mat = new Material(shader);
		mat.hideFlags = HideFlags.HideAndDontSave;
	}

	private void OnPreRender()
	{
		if ((double)noiseFactor < 0.0001)
		{
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		mat.SetColor(ShaderPropertyID._Color, color);
		mat.SetFloat(ShaderPropertyID._NoiseFactor, noiseFactor);
		mat.SetFloat(ShaderPropertyID._time, Mathf.Sin(Time.time * Time.deltaTime));
		Graphics.Blit(source, destination, mat);
	}
}
