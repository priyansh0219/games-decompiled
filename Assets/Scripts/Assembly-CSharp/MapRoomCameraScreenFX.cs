using UnityEngine;

[ExecuteInEditMode]
public class MapRoomCameraScreenFX : MonoBehaviour
{
	public Shader shader;

	public Color color;

	private float _noiseFactor;

	private Material mat;

	public float noiseFactor
	{
		get
		{
			return _noiseFactor;
		}
		set
		{
			_noiseFactor = value;
			base.enabled = ((_noiseFactor > 0f) ? true : false);
		}
	}

	private void Awake()
	{
		mat = new Material(shader);
		mat.hideFlags = HideFlags.HideAndDontSave;
		noiseFactor = 0f;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		mat.SetColor(ShaderPropertyID._Color, color);
		mat.SetFloat(ShaderPropertyID._NoiseFactor, noiseFactor);
		mat.SetFloat(ShaderPropertyID._time, Mathf.Sin(Time.time * Time.deltaTime));
		Graphics.Blit(source, destination, mat);
	}
}
