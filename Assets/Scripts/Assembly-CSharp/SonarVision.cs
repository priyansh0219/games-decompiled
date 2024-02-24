using UnityEngine;

[ExecuteInEditMode]
public class SonarVision : MonoBehaviour
{
	public Shader shader;

	public Color color = new Color(0.05f, 0.5f, 0.4f, 1f);

	private Material mat;

	private void Awake()
	{
		shader = Shader.Find("Image Effects/Sonar");
		mat = new Material(shader);
		mat.SetColor(ShaderPropertyID._Color, color);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, mat);
	}
}
