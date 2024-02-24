using UnityEngine;

[ExecuteInEditMode]
public class RadialBlurScreenFX : MonoBehaviour
{
	public Material mat;

	public float amount;

	private bool isOpenGL;

	private void Start()
	{
		isOpenGL = GraphicsUtil.IsOpenGL();
	}

	private void OnPreRender()
	{
		if (amount <= 0f)
		{
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		mat.SetFloat(ShaderPropertyID._Amount, amount);
		float value = 1f;
		float value2 = 1f;
		if (isOpenGL)
		{
			value = source.width;
			value2 = source.height;
		}
		mat.SetFloat(ShaderPropertyID._imgHeight, value);
		mat.SetFloat(ShaderPropertyID._imgWidth, value2);
		Graphics.Blit(source, destination, mat);
	}
}
