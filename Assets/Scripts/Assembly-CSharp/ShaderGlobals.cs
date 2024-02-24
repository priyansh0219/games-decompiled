using UnityEngine;

[ExecuteInEditMode]
public class ShaderGlobals : MonoBehaviour
{
	public bool unlitMode;

	[Header("INFECTION:")]
	[AssertNotNull]
	public Texture2D infectionAlbedoMap;

	[AssertNotNull]
	public Texture2D infectionNormalMap;

	[Range(0f, 1f)]
	public float debugInfectedAmount;

	[Header("_GlobalSeaLevel:")]
	public float globalSeaLevel;

	[Header("Misc:")]
	[AssertNotNull]
	public Texture[] blueNoiseTextures;

	private int blueNoiseTextureIndex;

	private Vector4 randomVector = Vector4.zero;

	private void SetInfectionGlobals()
	{
		Shader.SetGlobalTexture(ShaderPropertyID._InfectionAlbedomap, infectionAlbedoMap);
		Shader.SetGlobalTexture(ShaderPropertyID._InfectionNormalMap, infectionNormalMap);
	}

	private void Start()
	{
		SetInfectionGlobals();
		Shader.SetGlobalFloat(ShaderPropertyID._UweLocalLightScalar, 1f);
	}

	private void Update()
	{
		Shader.SetGlobalFloat(ShaderPropertyID._UWE_CTime, Time.time);
		if (Player.main != null)
		{
			globalSeaLevel = (Player.main.displaySurfaceWater ? 0f : (-999999f));
			Shader.SetGlobalFloat(ShaderPropertyID._GlobalSeaLevel, globalSeaLevel);
		}
	}

	private void OnPreCull()
	{
		Shader.SetGlobalMatrix(ShaderPropertyID._Camera2World, GetComponent<Camera>().cameraToWorldMatrix);
	}

	private void OnPreRender()
	{
		if (unlitMode)
		{
			Shader.EnableKeyword("UWE_EDITOR_UNLIT");
		}
		else
		{
			Shader.DisableKeyword("UWE_EDITOR_UNLIT");
		}
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		Shader.SetGlobalFloat(ShaderPropertyID._CameraFOVDegs, GetComponent<Camera>().fieldOfView);
		randomVector.x = Random.value;
		randomVector.y = Random.value;
		randomVector.z = Random.value;
		randomVector.w = Random.value;
		if (++blueNoiseTextureIndex >= blueNoiseTextures.Length)
		{
			blueNoiseTextureIndex = 0;
		}
		Shader.SetGlobalTexture(ShaderPropertyID._Uwe_BlueNoiseMap, blueNoiseTextures[blueNoiseTextureIndex]);
		Shader.SetGlobalVector(ShaderPropertyID._Uwe_RandomVector, randomVector);
	}
}
