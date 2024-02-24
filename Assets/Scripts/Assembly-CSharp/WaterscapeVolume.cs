using System;
using UnityEngine;

public class WaterscapeVolume : MonoBehaviour
{
	[Serializable]
	public class Settings
	{
		[Tooltip("Attenuation coefficients of light (1/cm)")]
		public Vector3 absorption = new Vector3(100f, 18.29155f, 3.531373f);

		public float scattering = 1f;

		public Color scatteringColor = Color.white;

		[Range(0f, 20f)]
		public float murkiness = 1f;

		public Color emissive = Color.black;

		public float emissiveScale = 1f;

		public float startDistance = 25f;

		[Tooltip("Scale applied to the sunlight in this biome")]
		public float sunlightScale = 1f;

		[Tooltip("Scale applied to the ambient lighting in this biome")]
		public float ambientScale = 1f;

		[Tooltip("Temperature in Celsius")]
		public float temperature = 24f;

		public Vector4 GetExtinctionAndScatteringCoefficients()
		{
			float num = murkiness / 100f;
			Vector3 vector = absorption + scattering * Vector3.one;
			return new Vector4(vector.x, vector.y, vector.z, scattering) * num;
		}

		public Vector4 GetEmissive()
		{
			return emissive.linear * emissiveScale / 100f;
		}
	}

	[AssertNotNull]
	public Shader fogShader;

	public uSkyManager sky;

	public WaterBiomeManager biomeManager;

	public WaterSurface surface;

	private bool fogEnabled = true;

	private Material material;

	public GameObject waterPlane;

	public float waterOffset;

	[Tooltip("Percentage of the sun light that's transmist to below the water surface")]
	[Range(0f, 1f)]
	public float waterTransmission = 0.9f;

	[Tooltip("Scale applied to the water emission when computing the ambient lighting")]
	public float emissionAmbientScale = 0.5f;

	public float aboveWaterStartDistance = 5f;

	[Range(-1f, 0f)]
	public float scatteringPhase = -0.3f;

	[Header("Sun:")]
	[Range(0f, 1f)]
	public float sunAttenuation = 1f;

	[Tooltip("Scale on the sun used to compensate for the surface lighting calculations not using proper units")]
	public float sunLightAmount = 1f;

	public float colorCastDistanceFactor = 0.005f;

	public float colorCastDepthFactor;

	[Header("Caustics:")]
	public float causticsScale = 0.2f;

	public float causticsAmount = 0.5f;

	private float scatteringScale = 1f;

	public float aboveWaterDensityScale = 10f;

	public float aboveWaterMinHeight = -1f;

	public float aboveWaterMaxHeight = 1f;

	private void Awake()
	{
		CreateResources();
		Shader.SetGlobalFloat(ShaderPropertyID._UweFogEnabled, 0f);
		biomeManager = GetComponent<WaterBiomeManager>();
		fogEnabled = true;
	}

	private void CreateResources()
	{
		material = new Material(fogShader);
		material.hideFlags = HideFlags.HideAndDontSave;
	}

	private void SetupWaterPlane(Camera camera, GameObject waterPlane)
	{
		if (waterPlane != null)
		{
			Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
			Transform transform = waterPlane.transform;
			Plane plane = new Plane(transform.up, transform.position);
			Plane plane2 = worldToCameraMatrix.TransformPlane(plane);
			Vector3 normal = plane2.normal;
			Shader.SetGlobalVector(ShaderPropertyID._UweVsWaterPlane, new Vector4(normal.x, normal.y, normal.z, plane2.distance));
		}
		else
		{
			Shader.SetGlobalVector(ShaderPropertyID._UweVsWaterPlane, new Vector4(0f, 0f, 0f, 0f));
		}
	}

	private void OnEnable()
	{
		Shader.SetGlobalFloat(ShaderPropertyID._UweFogEnabled, 1f);
		fogEnabled = true;
	}

	private void OnDisable()
	{
		Shader.SetGlobalFloat(ShaderPropertyID._UweFogEnabled, 0f);
		fogEnabled = false;
	}

	public float GetTransmission()
	{
		return Mathf.Clamp01(waterTransmission);
	}

	public void SetScatteringScale(float _scatteringScale)
	{
		scatteringScale = _scatteringScale;
	}

	public float GetScatteringScale()
	{
		return scatteringScale;
	}

	public void PreRender(Camera camera)
	{
		SetupWaterPlane(camera, waterPlane);
		biomeManager.SetupConstantsForCamera(camera);
		if (fogEnabled)
		{
			Shader.SetGlobalFloat(ShaderPropertyID._UweFogEnabled, 1f);
		}
		float transmission = GetTransmission();
		Shader.SetGlobalFloat(ShaderPropertyID._UweCausticsScale, causticsScale * surface.GetCausticsWorldToTextureScale());
		Shader.SetGlobalVector(ShaderPropertyID._UweCausticsAmount, new Vector3(causticsAmount, surface.GetCausticsTextureScale() * causticsAmount, surface.GetCausticsTextureScale()));
		Shader.SetGlobalFloat(ShaderPropertyID._UweWaterTransmission, transmission);
		Shader.SetGlobalFloat(ShaderPropertyID._UweWaterEmissionAmbientScale, emissionAmbientScale);
		float t = (camera.transform.position.y - aboveWaterMinHeight) / (aboveWaterMaxHeight - aboveWaterMinHeight);
		float value = Mathf.Lerp(1f, aboveWaterDensityScale, t);
		Shader.SetGlobalFloat(ShaderPropertyID._UweExtinctionAndScatteringScale, value);
		if (sky != null)
		{
			Vector3 lightDirection = sky.GetLightDirection();
			lightDirection.y = Mathf.Min(lightDirection.y, -0.01f);
			Vector3 vector = -camera.worldToCameraMatrix.MultiplyVector(lightDirection);
			Color lightColor = sky.GetLightColor();
			Vector4 value2 = lightColor;
			value2.w = sunLightAmount * transmission;
			float value3 = lightColor.r * 0.3f + lightColor.g * 0.59f + lightColor.b * 0.11f;
			Shader.SetGlobalVector(ShaderPropertyID._UweFogVsLightDirection, vector);
			Shader.SetGlobalVector(ShaderPropertyID._UweFogWsLightDirection, lightDirection);
			Shader.SetGlobalVector(ShaderPropertyID._UweFogLightColor, value2);
			Shader.SetGlobalFloat(ShaderPropertyID._UweFogLightGreyscaleColor, value3);
		}
		else
		{
			Shader.SetGlobalFloat(ShaderPropertyID._UweFogLightAmount, 0f);
		}
		Shader.SetGlobalVector(ShaderPropertyID._UweColorCastFactor, new Vector2(colorCastDistanceFactor, colorCastDepthFactor));
		Shader.SetGlobalFloat(ShaderPropertyID._UweAboveWaterFogStartDistance, aboveWaterStartDistance);
		Vector3 vector2 = default(Vector3);
		vector2.x = (1f - scatteringPhase * scatteringPhase) / ((float)Math.PI * 4f);
		vector2.y = 1f + scatteringPhase * scatteringPhase;
		vector2.z = 2f * scatteringPhase;
		Shader.SetGlobalVector(ShaderPropertyID._UweFogMiePhaseConst, vector2);
		Shader.SetGlobalFloat(ShaderPropertyID._UweSunAttenuationFactor, sunAttenuation);
	}

	public void RenderImage(Camera camera, bool cameraInside, RenderTexture source, RenderTexture destination)
	{
		material.SetFloat(ShaderPropertyID._CameraInside, cameraInside ? 1f : 0f);
		Graphics.Blit(source, destination, material);
	}

	public void PostRender(Camera camera)
	{
		Shader.SetGlobalFloat(ShaderPropertyID._UweFogEnabled, 0f);
	}
}
