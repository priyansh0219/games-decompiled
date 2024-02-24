using UnityEngine;

[ExecuteInEditMode]
public class MesmerizedScreenFX : MonoBehaviour
{
	public Material mat;

	public float amount;

	private void Start()
	{
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
		Graphics.Blit(source, destination, mat);
	}
}
