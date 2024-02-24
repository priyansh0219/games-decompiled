using UnityEngine;

namespace uSky
{
	[ExecuteInEditMode]
	[AddComponentMenu("uSky/uSky Light")]
	[RequireComponent(typeof(uSkyManager))]
	public class uSkyLight : MonoBehaviour
	{
		[Range(0f, 4f)]
		[Tooltip("Brightness of the Sun (directional light)")]
		public float SunIntensity = 1f;

		[Tooltip("The color of the both Sun and Moon light emitted")]
		public Gradient LightColor = new Gradient
		{
			colorKeys = new GradientColorKey[7]
			{
				new GradientColorKey(new Color32(55, 66, 77, byte.MaxValue), 0.23f),
				new GradientColorKey(new Color32(245, 173, 84, byte.MaxValue), 0.26f),
				new GradientColorKey(new Color32(249, 208, 144, byte.MaxValue), 0.32f),
				new GradientColorKey(new Color32(252, 222, 186, byte.MaxValue), 0.5f),
				new GradientColorKey(new Color32(249, 208, 144, byte.MaxValue), 0.68f),
				new GradientColorKey(new Color32(245, 173, 84, byte.MaxValue), 0.74f),
				new GradientColorKey(new Color32(55, 66, 77, byte.MaxValue), 0.77f)
			},
			alphaKeys = new GradientAlphaKey[2]
			{
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(1f, 1f)
			}
		};

		[Range(0f, 2f)]
		[Tooltip("Brightness of the Moon (directional light). If the Moon Intensity is at 0 (less then 0.01), the Moon light will auto disabled and always disabled at Day time")]
		public float MoonIntensity = 0.4f;

		[Tooltip("Ambient light that shines into the scene.")]
		public uSkyAmbient Ambient;

		public float ambientLight = 0.28f;

		private uSkyManager _uSM;

		private Light _sun_Light;

		private Light _moon_Light;

		private float currentTime
		{
			get
			{
				if (!(uSM != null))
				{
					return 1f;
				}
				return uSM.Timeline01;
			}
		}

		private float dayTime
		{
			get
			{
				if (!(uSM != null))
				{
					return 1f;
				}
				return uSM.DayTime;
			}
		}

		private float nightTime
		{
			get
			{
				if (!(uSM != null))
				{
					return 0f;
				}
				return uSM.NightTime;
			}
		}

		private float sunsetTime
		{
			get
			{
				if (!(uSM != null))
				{
					return 1f;
				}
				return uSM.SunsetTime;
			}
		}

		private uSkyManager uSM
		{
			get
			{
				if (_uSM == null)
				{
					_uSM = uSkyManager.main;
				}
				return _uSM;
			}
		}

		private GameObject sunLightObj
		{
			get
			{
				if (uSM != null)
				{
					if (!(uSM.SunLight != null))
					{
						return null;
					}
					return uSM.SunLight;
				}
				return null;
			}
		}

		private GameObject moonLightObj
		{
			get
			{
				if (uSM != null)
				{
					if (!(uSM.MoonLight != null))
					{
						return null;
					}
					return uSM.MoonLight;
				}
				return null;
			}
		}

		private Light sun_Light
		{
			get
			{
				if ((bool)_sun_Light)
				{
					return _sun_Light;
				}
				if ((bool)sunLightObj)
				{
					_sun_Light = sunLightObj.GetComponent<Light>();
				}
				if ((bool)_sun_Light)
				{
					return _sun_Light;
				}
				return null;
			}
		}

		private Light moon_Light
		{
			get
			{
				if ((bool)moonLightObj)
				{
					_moon_Light = moonLightObj.GetComponent<Light>();
				}
				if ((bool)_moon_Light)
				{
					return _moon_Light;
				}
				return null;
			}
		}

		private float exposure
		{
			get
			{
				if (!(uSM != null))
				{
					return 1f;
				}
				return uSM.Exposure;
			}
		}

		private float rayleighSlider
		{
			get
			{
				if (!(uSM != null))
				{
					return 1f;
				}
				return uSM.RayleighScattering;
			}
		}

		public Color CurrentLightColor => LightColor.Evaluate(currentTime);

		public Color CurrentSkyColor => colorOffset(Ambient.SkyColor.Evaluate(currentTime), 0.15f, 0.7f, IsGround: false);

		public Color CurrentEquatorColor => colorOffset(Ambient.EquatorColor.Evaluate(currentTime), 0.15f, 0.9f, IsGround: false);

		public Color CurrentGroundColor => colorOffset(Ambient.GroundColor.Evaluate(currentTime), 0.25f, 0.85f, IsGround: true);

		private void OnEnable()
		{
			if (uSM != null)
			{
				InitUpdate();
			}
		}

		private void Update()
		{
			if (uSM != null && uSM.SkyUpdate)
			{
				InitUpdate();
			}
		}

		private void InitUpdate()
		{
			SunAndMoonLightUpdate();
			float indirectLightFraction = uSM.GetIndirectLightFraction();
			float num = uSM.Eclipse();
			indirectLightFraction *= 1f - num;
			Shader.SetGlobalVector(ShaderPropertyID._UweBottomAmbientColor, CurrentGroundColor.linear * indirectLightFraction * ambientLight);
			Shader.SetGlobalVector(ShaderPropertyID._UweTopAmbientColor, CurrentSkyColor.linear * indirectLightFraction * ambientLight);
		}

		public Vector3 GetLightDirection()
		{
			return sun_Light.transform.forward;
		}

		public Color GetLightColor()
		{
			return sun_Light.color.linear * sun_Light.intensity;
		}

		private void SunAndMoonLightUpdate()
		{
			uSkyManager uSkyManager = uSM;
			float directLightFraction = uSkyManager.GetDirectLightFraction();
			float num = uSkyManager.Eclipse();
			directLightFraction *= 1f - num;
			directLightFraction = Mathf.Max(directLightFraction, 0.01f);
			if (sunLightObj != null && sun_Light != null)
			{
				float num2 = uSkyManager.Exposure * (SunIntensity * dayTime + MoonIntensity * nightTime) * directLightFraction;
				Color color = CurrentLightColor * (dayTime + nightTime);
				if (uSkyManager.spaceTransition > 0f)
				{
					num2 = Mathf.Lerp(num2, uSkyManager.endSequenceLightIntensity, uSkyManager.spaceTransition);
					color = Color.Lerp(sun_Light.color, uSkyManager.endSequenceLightColor, uSkyManager.spaceTransition);
				}
				sun_Light.intensity = num2;
				sun_Light.color = color;
			}
		}

		private void AmbientGradientUpdate()
		{
			float num = uSM.Eclipse();
			RenderSettings.ambientSkyColor = CurrentSkyColor * (1f - num);
			RenderSettings.ambientEquatorColor = CurrentEquatorColor * (1f - num);
			RenderSettings.ambientGroundColor = CurrentGroundColor * (1f - num);
		}

		private Color colorOffset(Color currentColor, float offsetRange, float rayleighOffsetRange, bool IsGround)
		{
			Vector3 vector = ((uSM != null) ? (uSM.BetaR * 1000f) : new Vector3(5.81f, 13.57f, 33.13f));
			Vector3 vector2 = new Vector3(0.5f, 0.5f, 0.5f);
			vector2 = new Vector3(vector.x / 5.81f * 0.5f, vector.y / 13.57f * 0.5f, vector.z / 33.13f * 0.5f);
			if (!IsGround)
			{
				vector2 = Vector3.Lerp(new Vector3(Mathf.Abs(1f - vector2.x), Mathf.Abs(1f - vector2.y), Mathf.Abs(1f - vector2.z)), vector2, sunsetTime);
			}
			vector2 = Vector3.Lerp(new Vector3(0.5f, 0.5f, 0.5f), vector2, dayTime);
			Vector3 vector3 = default(Vector3);
			vector3 = new Vector3(Mathf.Lerp(currentColor.r - offsetRange, currentColor.r + offsetRange, vector2.x), Mathf.Lerp(currentColor.g - offsetRange, currentColor.g + offsetRange, vector2.y), Mathf.Lerp(currentColor.b - offsetRange, currentColor.b + offsetRange, vector2.z));
			Vector3 b = new Vector3(vector3.x / vector.x, vector3.y / vector.y, vector3.z / vector.z) * 4f;
			vector3 = ((rayleighSlider < 1f) ? Vector3.Lerp(Vector3.zero, vector3, rayleighSlider) : Vector3.Lerp(vector3, b, Mathf.Max(0f, rayleighSlider - 1f) / 4f * rayleighOffsetRange));
			return new Color(vector3.x, vector3.y, vector3.z, 1f) * exposure;
		}
	}
}
