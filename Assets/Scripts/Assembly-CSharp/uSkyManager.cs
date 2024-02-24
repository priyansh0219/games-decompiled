using System;
using Gendarme;
using UnityEngine;
using uSky;

[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
[ExecuteInEditMode]
[AddComponentMenu("uSky/uSky Manager")]
public class uSkyManager : MonoBehaviour
{
	public enum NightModes
	{
		Off = 1,
		Static = 2,
		Rotation = 3
	}

	[HideInInspector]
	public static uSkyManager main;

	private uSkyLight m_uSL;

	[Tooltip("Update of the sky calculations in each frame.")]
	public bool SkyUpdate = true;

	[Range(0f, 24f)]
	[Tooltip("This value controls the light vertically. It represents sunrise/day and sunset/night time( Rotation X )")]
	public float Timeline = 17f;

	public bool UseTimeOfDay = true;

	[Range(-180f, 180f)]
	[Space(5f)]
	[Tooltip("This value controls the light horizionally.( Rotation Y )")]
	public float SunDirection;

	[Range(0f, 90f)]
	[Space(5f)]
	[Tooltip("Controls SunAndCaustic light maximum angle")]
	public float sunMaxAngle = 65f;

	[Range(-60f, 60f)]
	public float NorthPoleOffset;

	[Space(10f)]
	[Tooltip("This value sets the brightness of the sky.(for day time only)")]
	[Range(0f, 5f)]
	public float Exposure = 1f;

	[Range(0f, 5f)]
	[Tooltip("Rayleigh scattering is caused by particles in the atmosphere (up to 8 km). It produces typical earth-like sky colors (reddish/yellowish colors at sun set, and the like).")]
	public float RayleighScattering = 1f;

	[Range(0f, 5f)]
	[Tooltip("Mie scattering is caused by aerosols in the lower atmosphere (up to 1.2 km). It is for haze and halos around the sun on foggy days.")]
	public float MieScattering = 1f;

	[Range(0f, 0.9995f)]
	[Tooltip("The anisotropy factor controls the sun's appearance in the sky.The closer this value gets to 1.0, the sharper and smaller the sun spot will be. Higher values cause more fuzzy and bigger sun spots.")]
	public float SunAnisotropyFactor = 0.76f;

	[Range(0.001f, 10f)]
	[Tooltip("Size of the sun spot in the sky")]
	public float SunSize = 1f;

	public Texture2D sunBurstTexture;

	[Tooltip("It is visible spectrum light waves. Tweaking these values will shift the colors of the resulting gradients and produce different kinds of atmospheres.")]
	public Vector3 Wavelengths = new Vector3(680f, 550f, 440f);

	[Tooltip("It is wavelength dependent. Tweaking these values will shift the colors of sky color.")]
	public Color SkyTint = new Color(0.5f, 0.5f, 0.5f, 1f);

	[Tooltip("It is the bottom half color of the skybox")]
	public Color m_GroundColor = new Color(0.369f, 0.349f, 0.341f, 1f);

	[Tooltip("It is a Directional Light from the scene, it represents Sun Ligthing")]
	public GameObject SunLight;

	[Tooltip("How dense the fog on objects in the sky is")]
	public float skyFogDensity = 0.00015f;

	public Gradient skyFogColor = new Gradient();

	[Header("Planet:")]
	public float planetRadius = 500f;

	public Texture2D planetTexture;

	public Texture2D planetNormalMap;

	public float planetZenith;

	public float planetDistance = 10000f;

	public Color planetRimColor = Color.white;

	public Color planetAmbientLight = Color.white;

	public float planetOrbitSpeed = 1f;

	[Range(0f, 1f)]
	public float planetLightWrap;

	[Tooltip("The color of the planet's inner corona. This Alpha value controls the size and blurriness corona.")]
	public Color planetInnerCorona = new Color(1f, 1f, 1f, 0.5f);

	[Tooltip("The color of the planet's outer corona. This Alpha value controls the size and blurriness corona.")]
	public Color planetOuterCorona = new Color(0.25f, 0.39f, 0.5f, 0.5f);

	[Header("Clouds")]
	public Texture2D cloudsTexture;

	public float cloudsRotateSpeed = 1f;

	public float cloudNightBrightness = 0.25f;

	public float cloudsAttenuation = 0.6f;

	[Range(1f, 10f)]
	public float cloudsAlphaSaturation = 2f;

	[Range(0f, 8f)]
	public float sunColorMultiplier = 4f;

	[Range(0f, 8f)]
	public float skyColorMultiplier = 1.5f;

	[Range(0f, 10f)]
	public float cloudsScatteringMultiplier = 4f;

	[Range(0f, 109f)]
	public float cloudsScatteringExponent = 15f;

	public Gradient meanSkyColor = new Gradient();

	[Header("SecondaryLight")]
	public Vector3 secondaryLightDir = Vector3.zero;

	public Color secondaryLightColor = Color.black;

	public float secondaryLightPow = 4f;

	[Space(10f)]
	public NightModes NightSky = NightModes.Static;

	[Tooltip("The zenith color of the night sky gradient. (Top of the night sky)")]
	public Gradient NightZenithColor = new Gradient
	{
		colorKeys = new GradientColorKey[4]
		{
			new GradientColorKey(new Color32(50, 71, 99, byte.MaxValue), 0.225f),
			new GradientColorKey(new Color32(74, 107, 148, byte.MaxValue), 0.25f),
			new GradientColorKey(new Color32(74, 107, 148, byte.MaxValue), 0.75f),
			new GradientColorKey(new Color32(50, 71, 99, byte.MaxValue), 0.775f)
		},
		alphaKeys = new GradientAlphaKey[2]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		}
	};

	[Tooltip("The horizon color of the night sky gradient.")]
	public Color NightHorizonColor = new Color(0.43f, 0.47f, 0.5f, 1f);

	[Range(0f, 10f)]
	[Tooltip("This controls the intensity of the Stars field in night sky.")]
	public float StarIntensity = 1f;

	[Tooltip("The color of the moon's inner corona. This Alpha value controls the size and blurriness corona.")]
	public Color MoonInnerCorona = new Color(1f, 1f, 1f, 0.5f);

	[Tooltip("The color of the moon's outer corona. This Alpha value controls the size and blurriness corona.")]
	public Color MoonOuterCorona = new Color(0.25f, 0.39f, 0.5f, 0.5f);

	[Range(0f, 1f)]
	[Tooltip("This controls the moon texture size in the night sky.")]
	public float MoonSize = 0.15f;

	public Texture2D MoonTexture;

	[Range(-90f, 90f)]
	public float MoonPositionOffset;

	[Tooltip("It is additional Directional Light from the scene, it represents Moon Ligthing.")]
	public GameObject MoonLight;

	[Tooltip("It is the uSkybox Material of the uSky.")]
	public Material SkyboxMaterial;

	[SerializeField]
	[Tooltip("It will automatically assign the current skybox material to Render Settings.")]
	private bool _AutoApplySkybox = true;

	[HideInInspector]
	public bool LinearSpace;

	[Tooltip("Toggle it if the Main Camera is using HDR mode and Tonemapping image effect.")]
	public bool SkyboxHDR;

	private Quaternion sunEuler;

	private Matrix4x4 moon_wtl;

	public float directLightFraction = 1f;

	public float indirectLightFraction = 1f;

	[Header("LensFlare:")]
	public Transform lensFlareTransform;

	public LensFlare lensFlareComp;

	[Header("EndSequence cinematic (Debug):")]
	public Transform rocketTrajectoryHelper;

	public Transform endSequenceSunHelper;

	public Transform endSequencePlanetHelper;

	public float spaceTransition;

	public float endSequenceLightIntensity = 1f;

	public float endSequenceSunSizeMultiplier = 1f;

	public float endSequencePlanetRadius = 80000f;

	private float endSequenceSunSize;

	public Color endSequenceLightColor = new Color(1f, 0.88f, 0.6f, 1f);

	public Color endSequenceSunBurstColor = new Color(1f, 0.88f, 0.6f, 1f);

	public float endSequenceDaytimeTransition;

	private Material m_starMaterial;

	[SerializeField]
	[HideInInspector]
	private Mesh _starsMesh;

	private readonly Vector3 BetaM = new Vector3(0.004f, 0.004f, 0.004f) * 0.9f;

	private bool EnableNightSky
	{
		get
		{
			if (NightSky != NightModes.Off)
			{
				return true;
			}
			return false;
		}
	}

	public bool AutoApplySkybox
	{
		get
		{
			return _AutoApplySkybox;
		}
		set
		{
			if (value && (bool)SkyboxMaterial && RenderSettings.skybox != SkyboxMaterial)
			{
				RenderSettings.skybox = SkyboxMaterial;
			}
			_AutoApplySkybox = value;
		}
	}

	protected Material starMaterial
	{
		get
		{
			if (m_starMaterial == null)
			{
				m_starMaterial = new Material(ShaderManager.preloadedShaders.uSkyManagerStar);
				m_starMaterial.hideFlags = HideFlags.DontSave;
			}
			return m_starMaterial;
		}
	}

	public Mesh starsMesh
	{
		get
		{
			return _starsMesh;
		}
		set
		{
			_starsMesh = value;
		}
	}

	protected uSkyLight uSL
	{
		get
		{
			if (m_uSL == null)
			{
				m_uSL = base.gameObject.GetComponent<uSkyLight>();
				if (m_uSL == null)
				{
					Debug.Log("Can't not find uSkyLight Component, Please apply DistanceCloud in uSkyManager gameobject");
				}
			}
			return m_uSL;
		}
	}

	public float Timeline01 => Timeline / 24f;

	public Vector3 SunDir
	{
		get
		{
			if (!(SunLight != null))
			{
				return new Vector3(0.321f, 0.766f, -0.557f);
			}
			return GetLightDirection() * -1f;
		}
	}

	private Matrix4x4 getMoonWorldToLocalMatrix
	{
		get
		{
			if (!(MoonLight != null))
			{
				return Matrix4x4.identity;
			}
			return MoonLight.transform.worldToLocalMatrix;
		}
	}

	private Matrix4x4 getSunLocalToWorldMatrix
	{
		get
		{
			if (!(MoonLight != null))
			{
				return Matrix4x4.identity;
			}
			return MoonLight.transform.localToWorldMatrix;
		}
	}

	private Matrix4x4 getMoonMatrix
	{
		get
		{
			if (MoonLight == null)
			{
				moon_wtl = Matrix4x4.TRS(Vector3.zero, new Quaternion(-0.9238795f, 8.817204E-08f, 8.817204E-08f, 0.3826835f), Vector3.one);
			}
			else if (MoonLight != null)
			{
				moon_wtl = MoonLight.transform.worldToLocalMatrix;
				moon_wtl.SetColumn(2, Vector4.Scale(new Vector4(1f, 1f, 1f, -1f), moon_wtl.GetColumn(2)));
			}
			return moon_wtl;
		}
	}

	private Vector3 variableRangeWavelengths => new Vector3(Mathf.Lerp(Wavelengths.x + 150f, Wavelengths.x - 150f, SkyTint.r), Mathf.Lerp(Wavelengths.y + 150f, Wavelengths.y - 150f, SkyTint.g), Mathf.Lerp(Wavelengths.z + 150f, Wavelengths.z - 150f, SkyTint.b));

	public Vector3 BetaR
	{
		get
		{
			Vector3 vector = variableRangeWavelengths * 1E-09f;
			Vector3 vector2 = new Vector3(Mathf.Pow(vector.x, 4f), Mathf.Pow(vector.y, 4f), Mathf.Pow(vector.z, 4f));
			Vector3 vector3 = 7.635E+25f * vector2 * 5.755f;
			float num = 8f * Mathf.Pow((float)Math.PI, 3f) * Mathf.Pow(0.0006002188f, 2f) * 6.105f;
			return 1000f * new Vector3(num / vector3.x, num / vector3.y, num / vector3.z);
		}
	}

	private Vector3 betaR_RayleighOffset => BetaR * Mathf.Max(0.001f, RayleighScattering);

	public float uMuS => Mathf.Atan(Mathf.Max(SunDir.y, -0.1975f) * 5.35f) / 1.1f + 0.739f;

	public float DayTime => Mathf.Clamp01(uMuS);

	public float SunsetTime => Mathf.Clamp01((uMuS - 1f) * (1.5f / Mathf.Pow(RayleighScattering, 4f)));

	public float NightTime => Mathf.Max(spaceTransition, 1f - DayTime);

	public Vector3 miePhase_g
	{
		get
		{
			float num = SunAnisotropyFactor * SunAnisotropyFactor;
			return new Vector3(((LinearSpace && SkyboxHDR) ? 2f : 1f) * ((1f - num) / (2f + num)), 1f + num, 2f * SunAnisotropyFactor);
		}
	}

	public Vector3 mieConst => new Vector3(1f, BetaR.x / BetaR.y, BetaR.x / BetaR.z) * BetaM.x * MieScattering;

	public Vector3 skyMultiplier => new Vector3(SunsetTime, Exposure * 4f * DayTime * Mathf.Sqrt(RayleighScattering), NightTime);

	private Vector3 bottomTint
	{
		get
		{
			float num = (LinearSpace ? 0.01f : 0.02f);
			return new Vector3(betaR_RayleighOffset.x / (m_GroundColor.r * num), betaR_RayleighOffset.y / (m_GroundColor.g * num), betaR_RayleighOffset.z / (m_GroundColor.b * num));
		}
	}

	public Vector2 ColorCorrection
	{
		get
		{
			if (!LinearSpace || !SkyboxHDR)
			{
				if (!LinearSpace)
				{
					return Vector2.one;
				}
				return new Vector2(1f, 2f);
			}
			return new Vector2(0.38317f, 1.413f);
		}
	}

	public Color getNightHorizonColor => NightHorizonColor * NightTime;

	public Color getNightZenithColor => NightZenithColor.Evaluate(Timeline01) * 0.01f;

	private float moonCoronaFactor
	{
		get
		{
			float num = 0f;
			float num2 = ((SunLight != null) ? GetLightDirection().y : 0f);
			if (NightSky == NightModes.Rotation)
			{
				return NightTime * num2;
			}
			return NightTime;
		}
	}

	private Vector4 getMoonInnerCorona => new Vector4(MoonInnerCorona.r * moonCoronaFactor, MoonInnerCorona.g * moonCoronaFactor, MoonInnerCorona.b * moonCoronaFactor, 400f / MoonInnerCorona.a);

	private Vector4 getMoonOuterCorona
	{
		get
		{
			float num = ((!LinearSpace) ? 8f : (SkyboxHDR ? 16f : 12f));
			return new Vector4(MoonOuterCorona.r * 0.25f * moonCoronaFactor, MoonOuterCorona.g * 0.25f * moonCoronaFactor, MoonOuterCorona.b * 0.25f * moonCoronaFactor, num / MoonOuterCorona.a);
		}
	}

	private Vector4 getPlanetInnerCorona => new Vector4(planetInnerCorona.r * moonCoronaFactor, planetInnerCorona.g * moonCoronaFactor, planetInnerCorona.b * moonCoronaFactor, 400f / planetInnerCorona.a);

	private Vector4 getPlanetOuterCorona
	{
		get
		{
			float num = ((!LinearSpace) ? 8f : (SkyboxHDR ? 16f : 12f));
			return new Vector4(planetOuterCorona.r * 0.25f * moonCoronaFactor, planetOuterCorona.g * 0.25f * moonCoronaFactor, planetOuterCorona.b * 0.25f * moonCoronaFactor, num / planetOuterCorona.a);
		}
	}

	private float starBrightness
	{
		get
		{
			float num = (LinearSpace ? 1f : 1.5f);
			return StarIntensity * NightTime * num + spaceTransition;
		}
	}

	public void SetEndSequenceVariables(float transitionValue, float lensFlareBrightness)
	{
		endSequenceDaytimeTransition = transitionValue;
		lensFlareComp.brightness = lensFlareBrightness;
	}

	private void Awake()
	{
		main = this;
	}

	private Matrix4x4 GetRocketInverseMatrix()
	{
		if (!(rocketTrajectoryHelper != null))
		{
			return Matrix4x4.identity;
		}
		return rocketTrajectoryHelper.localToWorldMatrix.inverse;
	}

	private Quaternion GetRocketInverseRotation()
	{
		if (!(rocketTrajectoryHelper != null))
		{
			return Quaternion.identity;
		}
		return rocketTrajectoryHelper.rotation;
	}

	public void LaunchRocket(Transform trajectoryHelper, Transform sunHelper, Transform planetHelper)
	{
		rocketTrajectoryHelper = trajectoryHelper;
		rocketTrajectoryHelper.rotation = Quaternion.identity;
		endSequenceSunHelper = sunHelper;
		endSequencePlanetHelper = planetHelper;
		lensFlareComp.gameObject.SetActive(value: true);
		SkyboxMaterial.EnableKeyword("ROCKETLAUNCH");
		starMaterial.EnableKeyword("ROCKETLAUNCH");
	}

	public void RevertRocketLaunch()
	{
		rocketTrajectoryHelper = null;
		endSequenceSunHelper = null;
		endSequencePlanetHelper = null;
		lensFlareComp.gameObject.SetActive(value: false);
		SkyboxMaterial.DisableKeyword("ROCKETLAUNCH");
		starMaterial.DisableKeyword("ROCKETLAUNCH");
		spaceTransition = 0f;
		endSequenceDaytimeTransition = 0f;
	}

	public void SetDirectLightFraction(float _directLightFraction)
	{
		directLightFraction = _directLightFraction;
	}

	public float GetDirectLightFraction()
	{
		return directLightFraction;
	}

	public void SetIndirectLightFraction(float _indirectLightFraction)
	{
		indirectLightFraction = _indirectLightFraction;
	}

	public float GetIndirectLightFraction()
	{
		return indirectLightFraction;
	}

	public void InitStarsMesh()
	{
		StarField starField = new StarField();
		if (starsMesh == null)
		{
			starsMesh = new Mesh();
		}
		starsMesh = starField.InitializeStarfield();
	}

	private void OnEnable()
	{
		if (SunLight == null)
		{
			SunLight = GameObject.Find("Directional Light");
		}
		if (SkyboxMaterial != null)
		{
			SetConstantMaterialProperties(SkyboxMaterial);
			SetVaryingMaterialProperties(SkyboxMaterial);
		}
		if (starMaterial != null)
		{
			SetConstantMaterialProperties(starMaterial);
			SetVaryingMaterialProperties(starMaterial);
		}
		if (EnableNightSky && starsMesh == null)
		{
			InitStarsMesh();
		}
	}

	private void OnDisable()
	{
		if ((bool)starsMesh)
		{
			UnityEngine.Object.DestroyImmediate(starsMesh);
		}
		if ((bool)m_starMaterial)
		{
			UnityEngine.Object.DestroyImmediate(m_starMaterial);
		}
	}

	private void detectColorSpace()
	{
	}

	private void Start()
	{
		RevertRocketLaunch();
		InitSunAndMoon();
		if (SkyboxMaterial != null)
		{
			SetConstantMaterialProperties(SkyboxMaterial);
			SetVaryingMaterialProperties(SkyboxMaterial);
		}
		if (starMaterial != null)
		{
			SetConstantMaterialProperties(starMaterial);
			SetVaryingMaterialProperties(starMaterial);
		}
		AutoApplySkybox = _AutoApplySkybox;
	}

	public float Eclipse()
	{
		return Mathf.Pow(Mathf.Max(Vector3.Dot(PlanetPos().normalized, SunDir), 0f), 50f);
	}

	public void SetConstantMaterialProperties(Material mat)
	{
		mat.SetVector(ShaderPropertyID._betaR, betaR_RayleighOffset);
		mat.SetVector(ShaderPropertyID._betaM, BetaM);
		mat.SetTexture(ShaderPropertyID._SunBurstTexture, sunBurstTexture);
		mat.SetVector(ShaderPropertyID._mieConst, mieConst);
		mat.SetVector(ShaderPropertyID._miePhase_g, miePhase_g);
		mat.SetVector(ShaderPropertyID._GroundColor, bottomTint);
		mat.SetVector(ShaderPropertyID._NightHorizonColor, getNightHorizonColor);
		mat.SetVector(ShaderPropertyID._NightZenithColor, getNightZenithColor);
		mat.SetVector(ShaderPropertyID._MoonInnerCorona, getMoonInnerCorona);
		mat.SetVector(ShaderPropertyID._MoonOuterCorona, getMoonOuterCorona);
		mat.SetFloat(ShaderPropertyID._MoonSize, MoonSize);
		mat.SetVector(ShaderPropertyID._colorCorrection, ColorCorrection);
		mat.SetTexture(ShaderPropertyID._PlanetTexture, planetTexture);
		mat.SetTexture(ShaderPropertyID._PlanetNormalMap, planetNormalMap);
		mat.SetColor(ShaderPropertyID._PlanetRimColor, planetRimColor);
		mat.SetColor(ShaderPropertyID._PlanetAmbientLight, planetAmbientLight);
		mat.SetFloat(ShaderPropertyID._PlanetLightWrap, planetLightWrap);
		mat.SetVector(ShaderPropertyID._PlanetInnerCorona, getPlanetInnerCorona);
		mat.SetVector(ShaderPropertyID._PlanetOuterCorona, getPlanetOuterCorona);
		if (SkyboxHDR)
		{
			mat.DisableKeyword("USKY_HDR_OFF");
			mat.EnableKeyword("USKY_HDR_ON");
		}
		else
		{
			mat.EnableKeyword("USKY_HDR_OFF");
			mat.DisableKeyword("USKY_HDR_ON");
		}
		mat.SetTexture(ShaderPropertyID._MoonSampler, MoonTexture);
		mat.SetTexture(ShaderPropertyID._CloudsTexture, cloudsTexture);
		mat.SetFloat(ShaderPropertyID._CloudsAlphaSaturation, cloudsAlphaSaturation);
		mat.SetFloat(ShaderPropertyID._CloudsAttenuation, cloudsAttenuation);
		mat.SetFloat(ShaderPropertyID._CloudsScatteringMultiplier, cloudsScatteringMultiplier);
		mat.SetFloat(ShaderPropertyID._CloudsScatteringExponent, cloudsScatteringExponent);
		mat.SetFloat(ShaderPropertyID._SunColorMultiplier, sunColorMultiplier);
		mat.SetFloat(ShaderPropertyID._SkyColorMultiplier, skyColorMultiplier);
	}

	public void SetVaryingMaterialProperties(Material mat)
	{
		mat.SetColor(ShaderPropertyID._EndSequenceSunBurstColor, endSequenceSunBurstColor);
		if (rocketTrajectoryHelper != null)
		{
			mat.SetFloat(ShaderPropertyID._SunSize, Mathf.Lerp(32f / SunSize, endSequenceSunSizeMultiplier / endSequenceSunSize, endSequenceDaytimeTransition));
			mat.SetFloat(ShaderPropertyID._PlanetRadius, Mathf.Lerp(planetRadius, endSequencePlanetRadius, endSequenceDaytimeTransition));
		}
		else
		{
			mat.SetFloat(ShaderPropertyID._SunSize, 32f / SunSize);
			mat.SetFloat(ShaderPropertyID._PlanetRadius, planetRadius);
		}
		Matrix4x4 value = getMoonMatrix * base.transform.worldToLocalMatrix;
		mat.SetMatrix(ShaderPropertyID._WorldToMoonMatrix, value);
		Shader.SetGlobalMatrix(ShaderPropertyID._RocketMatrix, GetRocketInverseMatrix());
		mat.SetVector(ShaderPropertyID._SkyMultiplier, skyMultiplier);
		Vector3 vector = PlanetPos();
		mat.SetVector(ShaderPropertyID._PlanetPos, vector);
		if (NightSky == NightModes.Rotation)
		{
			mat.SetMatrix(ShaderPropertyID._WorldToSpaceMatrix, getMoonWorldToLocalMatrix * base.transform.worldToLocalMatrix);
		}
		else
		{
			mat.SetMatrix(ShaderPropertyID._WorldToSpaceMatrix, base.transform.worldToLocalMatrix);
		}
		mat.SetFloat(ShaderPropertyID.StarIntensity, starBrightness);
		Shader.SetGlobalFloat(ShaderPropertyID._SpaceTransition, spaceTransition);
		if (NightSky == NightModes.Rotation)
		{
			mat.SetMatrix(ShaderPropertyID.rotationMatrix, getSunLocalToWorldMatrix);
		}
		else
		{
			mat.SetMatrix(ShaderPropertyID.rotationMatrix, Matrix4x4.identity);
		}
		Quaternion q = Quaternion.AngleAxis(cloudsRotateSpeed * Time.time, Vector3.up);
		mat.SetMatrix(ShaderPropertyID._WorldToCloudsMatrix, Matrix4x4.TRS(Vector3.zero, q, Vector3.one));
		if (VFXSunbeam.main != null)
		{
			Color cloudsColor = VFXSunbeam.main.GetCloudsColor();
			if (cloudsColor != Color.black)
			{
				secondaryLightDir = Vector3.Normalize(base.transform.InverseTransformPoint(VFXSunbeam.main.targetInitPos));
				secondaryLightColor = cloudsColor;
				mat.SetVector(ShaderPropertyID._SecondaryLightDir, secondaryLightDir);
				mat.SetColor(ShaderPropertyID._SecondaryLightColor, secondaryLightColor);
				mat.SetFloat(ShaderPropertyID._SecondaryLightPow, secondaryLightPow);
			}
			else
			{
				mat.SetColor(ShaderPropertyID._SecondaryLightColor, Color.black);
			}
		}
	}

	private void UpdateGlobalShaderProperties()
	{
		float num = Eclipse();
		Shader.SetGlobalVector(ShaderPropertyID._SunDir, SunDir);
		Shader.SetGlobalFloat(ShaderPropertyID._SkyFogDensity, skyFogDensity * (1f - num));
		Shader.SetGlobalColor(ShaderPropertyID._SkyFogColor, skyFogColor.Evaluate(Timeline01));
		Shader.SetGlobalFloat(ShaderPropertyID._Eclipse, num);
		float num2 = Mathf.Max(Mathf.Pow(cloudNightBrightness, LinearSpace ? 1.5f : 1f), DayTime);
		num2 *= Mathf.Sqrt(Exposure);
		Shader.SetGlobalVector(ShaderPropertyID._ShadeColorFromSun, LinearSpace ? (uSL.CurrentLightColor.linear * num2) : (uSL.CurrentLightColor * num2));
		Shader.SetGlobalVector(ShaderPropertyID._ShadeColorFromSky, LinearSpace ? (uSL.CurrentSkyColor.linear * num2) : (uSL.CurrentSkyColor * num2));
	}

	private void Update()
	{
		if (SkyUpdate)
		{
			if (UseTimeOfDay && DayNightCycle.main != null)
			{
				Timeline = DayNightCycle.main.GetDayNightCycleTime() * 24f;
			}
			if (Timeline >= 24f)
			{
				Timeline = 0f;
			}
			if (Timeline < 0f)
			{
				Timeline = 24f;
			}
			if (SkyboxMaterial != null)
			{
				InitSunAndMoon();
				SetVaryingMaterialProperties(SkyboxMaterial);
				SetVaryingMaterialProperties(starMaterial);
			}
			UpdateGlobalShaderProperties();
		}
		if (EnableNightSky && starsMesh != null && starMaterial != null && (SunDir.y < 0.2f || spaceTransition > 0f))
		{
			Graphics.DrawMesh(starsMesh, (spaceTransition > 0f) ? MainCamera.camera.transform.position : Vector3.zero, GetRocketInverseRotation(), starMaterial, 0);
		}
	}

	private Vector3 PlanetPos()
	{
		double num = 0.0;
		if (DayNightCycle.main != null)
		{
			num = (double)planetOrbitSpeed * DayNightCycle.main.GetDay();
		}
		double num2 = planetZenith * ((float)Math.PI / 180f);
		double num3 = num * 0.01745329238474369;
		double num4 = Math.Sin(num2);
		double num5 = Math.Cos(num2);
		double num6 = Math.Sin(num3);
		double num7 = Math.Cos(num3);
		Vector3 vector = default(Vector3);
		vector.x = (float)(num4 * num7);
		vector.y = (float)num5;
		vector.z = (float)(num4 * num6);
		vector *= planetDistance;
		if (endSequencePlanetHelper != null)
		{
			Vector3 vector2 = endSequencePlanetHelper.position - MainCamera.camera.transform.position;
			vector2 = Quaternion.Inverse(rocketTrajectoryHelper.rotation) * vector2;
			vector = Vector3.Lerp(vector, vector2, endSequenceDaytimeTransition);
		}
		return vector;
	}

	private void InitSunAndMoon()
	{
		float num = Timeline / 24f;
		float num2 = 0f;
		if (Timeline < 6f)
		{
			num2 = Mathf.Clamp(num * 4f, 0f, 1f);
			num = Mathf.Lerp(0f, 0f - sunMaxAngle, num2);
		}
		else if (Timeline > 18f)
		{
			num2 = Mathf.Clamp((num - 0.75f) * 4f, 0f, 1f);
			num = Mathf.Lerp(sunMaxAngle, 0f, num2);
		}
		else
		{
			num2 = Mathf.Clamp(num * 2f - 0.5f, 0f, 1f);
			num = Mathf.Lerp(0f - sunMaxAngle, sunMaxAngle, num2);
		}
		sunEuler = Quaternion.Euler(new Vector3(0f, SunDirection, NorthPoleOffset)) * Quaternion.Euler(Timeline * 360f / 24f - 90f, 0f, 0f);
		Quaternion quaternion = Quaternion.identity;
		Vector3 zero = Vector3.zero;
		float num3 = 0f;
		if (endSequenceSunHelper != null && rocketTrajectoryHelper != null)
		{
			zero = MainCamera.camera.transform.position - endSequenceSunHelper.position;
			num3 = zero.magnitude;
			float num4 = 2f * Mathf.Tan(MainCamera.camera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
			endSequenceSunSize = num4 / num3;
			quaternion = Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(zero) * 360f) * Quaternion.Euler(new Vector3(-90f, 0f, 0f));
			Quaternion b = Quaternion.Inverse(rocketTrajectoryHelper.rotation) * quaternion;
			sunEuler = Quaternion.Lerp(sunEuler, b, endSequenceDaytimeTransition);
		}
		if (SunLight != null)
		{
			SunLight.transform.rotation = Quaternion.Euler(new Vector3(0f, SunDirection, NorthPoleOffset)) * Quaternion.Euler(num + 90f, 0f, 0f);
			if (endSequenceSunHelper != null && rocketTrajectoryHelper != null)
			{
				SunLight.transform.rotation = Quaternion.Lerp(SunLight.transform.rotation, quaternion, endSequenceDaytimeTransition);
				lensFlareTransform.position = -SunLight.transform.forward * num3;
			}
		}
	}

	public Color GetMeanSkyColor()
	{
		return meanSkyColor.Evaluate(Timeline01);
	}

	public Vector3 GetLightDirection()
	{
		return sunEuler * Vector3.forward;
	}

	public Vector3 GetRealLightDirection()
	{
		return uSL.GetLightDirection();
	}

	public Color GetLightColor()
	{
		return uSL.GetLightColor();
	}

	public void OnValidate()
	{
		Timeline = Mathf.Clamp(Timeline, 0f, 24f);
		SunDirection = Mathf.Clamp(SunDirection, -180f, 180f);
		NorthPoleOffset = Mathf.Clamp(NorthPoleOffset, -60f, 60f);
		SunAnisotropyFactor = Mathf.Clamp01(SunAnisotropyFactor);
		Wavelengths.x = Mathf.Clamp(Wavelengths.x, 380f, 780f);
		Wavelengths.y = Mathf.Clamp(Wavelengths.y, 380f, 780f);
		Wavelengths.z = Mathf.Clamp(Wavelengths.z, 380f, 780f);
		if (EnableNightSky && starsMesh == null)
		{
			InitStarsMesh();
		}
		AutoApplySkybox = _AutoApplySkybox;
	}
}
