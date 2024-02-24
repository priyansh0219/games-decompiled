using UnityEngine;

[ExecuteInEditMode]
public class ExplosionScreenFX : MonoBehaviour
{
	public Shader shader;

	public float strength;

	public Color color;

	public float chromaticOffset = 0.1f;

	public float noiseFactor = 0.005f;

	private Material mat;

	private void Start()
	{
		mat = new Material(shader);
		mat.hideFlags = HideFlags.HideAndDontSave;
	}

	private void Update()
	{
		if (strength < 0.001f)
		{
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (mat != null)
		{
			mat.SetColor(ShaderPropertyID._Color, color);
			mat.SetFloat(ShaderPropertyID._NoiseFactor, noiseFactor);
			mat.SetFloat(ShaderPropertyID._time, Mathf.Sin(Time.time * Time.deltaTime));
			mat.SetFloat(ShaderPropertyID._Strength, strength);
			mat.SetFloat(ShaderPropertyID._ChromaticOffset, chromaticOffset);
			Graphics.Blit(source, destination, mat);
		}
		else
		{
			mat = new Material(shader);
			mat.hideFlags = HideFlags.HideAndDontSave;
		}
	}
}
