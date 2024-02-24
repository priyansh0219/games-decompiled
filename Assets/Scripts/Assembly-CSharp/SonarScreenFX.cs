using UnityEngine;

[ExecuteInEditMode]
public class SonarScreenFX : MonoBehaviour
{
	[Range(0f, 1f)]
	public float pingDistance;

	public float waveDuration = 5f;

	[AssertNotNull]
	public Shader _shader;

	private Material _material;

	private int pingDistanceShaderID;

	public void Awake()
	{
		pingDistanceShaderID = Shader.PropertyToID("_SonarPingDistance");
	}

	private void Update()
	{
		pingDistance += Time.deltaTime / waveDuration;
		if (pingDistance > 1f)
		{
			base.enabled = false;
		}
		Shader.SetGlobalFloat(pingDistanceShaderID, pingDistance);
	}

	public void Ping()
	{
		if (!base.enabled)
		{
			base.enabled = true;
		}
		pingDistance = 0f;
	}

	private void OnEnable()
	{
		Ping();
		WaterscapeVolumeOnCamera.doWaterFogCullingAdjustments = false;
	}

	private void OnDisable()
	{
		pingDistance = 0f;
		WaterscapeVolumeOnCamera.doWaterFogCullingAdjustments = true;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_material == null)
		{
			_material = new Material(_shader);
			_material.hideFlags = HideFlags.DontSave;
		}
		_material.SetFloat(ShaderPropertyID._SonarPingDistance, pingDistance);
		Graphics.Blit(source, destination, _material);
	}
}
