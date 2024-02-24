using System;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class DayNightCycle : MonoBehaviour, IProtoEventListener
{
	public static DayNightCycle main;

	public static readonly DateTime dateOrigin = new DateTime(2287, 5, 7, 9, 36, 0);

	public static readonly DateTime dayOrigin = new DateTime(dateOrigin.Year, dateOrigin.Month, dateOrigin.Day);

	private const float kDayLengthSeconds = 1200f;

	public float kMinSurfaceAmbientScalar = 0.3f;

	public float kMinDeepAmbientScalar;

	private float _dayNightSpeed = 1f;

	public Gradient atmosphereColor;

	public bool debugFreeze;

	private Light sunAndCausticsLight;

	private DayNightLight sunlight;

	private Color fullAmbientColor;

	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public float timePassedDeprecated;

	[NonSerialized]
	[ProtoMember(3)]
	public double timePassedAsDouble;

	public const float secondsInDay = 86400f;

	public const float gameSecondMultiplier = 72f;

	private readonly float timePassedOrigin = 1200f * (float)(3600 * dateOrigin.Hour + 60 * dateOrigin.Minute + dateOrigin.Second) / 86400f;

	private bool dayLastFrame = true;

	public float sunRiseTime = 0.125f;

	public float sunSetTime = 0.875f;

	public Event<bool> dayNightCycleChangedEvent = new Event<bool>();

	private bool skipTimeMode;

	private double skipModeEndTime;

	public float dayNightSpeed => _dayNightSpeed;

	public float deltaTime => Time.deltaTime * _dayNightSpeed;

	public float timePassedAsFloat => (float)timePassed;

	public float timePassedSinceOrigin => timePassedAsFloat - timePassedOrigin;

	public double timePassed => timePassedAsDouble;

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (version < 2)
		{
			timePassedAsDouble = timePassedDeprecated;
		}
		PDALog.SetTime(timePassedAsFloat);
		version = 2;
	}

	[Obsolete("Do not mess with timePassed!")]
	public void SetTimePassed(float timePassedOverride)
	{
		timePassedAsDouble = timePassedOverride;
	}

	private void Awake()
	{
		if (main != null)
		{
			Debug.Log("DayNightCycle already exists - destroying self");
			UnityEngine.Object.DestroyImmediate(base.gameObject);
			return;
		}
		main = this;
		sunlight = GetComponentInChildren<DayNightLight>();
		sunAndCausticsLight = GetComponentInChildren<Light>();
		timePassedAsDouble = timePassedOrigin;
		fullAmbientColor = new Color(RenderSettings.ambientLight.r, RenderSettings.ambientLight.g, RenderSettings.ambientLight.b);
		DevConsole.RegisterConsoleCommand(this, "day");
		DevConsole.RegisterConsoleCommand(this, "night");
		DevConsole.RegisterConsoleCommand(this, "daynight");
		DevConsole.RegisterConsoleCommand(this, "daynightspeed");
	}

	public float GetDayNightCycleTime()
	{
		float num = GetDayScalar();
		float num2 = sunSetTime - sunRiseTime;
		if (num > sunRiseTime && num < sunSetTime)
		{
			return (num - sunRiseTime) / num2 * 0.5f + 0.25f;
		}
		float num3 = 1f - num2;
		if (num < sunSetTime)
		{
			num += 1f;
		}
		float num4 = (num - sunSetTime) / num3 * 0.5f + 0.75f;
		if (num4 > 1f)
		{
			num4 -= 1f;
		}
		return num4;
	}

	public bool IsDay()
	{
		float dayScalar = GetDayScalar();
		if (dayScalar > sunRiseTime)
		{
			return dayScalar < sunSetTime;
		}
		return false;
	}

	public void Update()
	{
		if (!debugFreeze)
		{
			timePassedAsDouble += deltaTime;
			if (skipTimeMode && timePassed >= skipModeEndTime)
			{
				skipTimeMode = false;
				_dayNightSpeed = 1f;
			}
			UpdateAtmosphere();
			UpdateDayNightMessage();
		}
	}

	public void Pause()
	{
		_dayNightSpeed = 0f;
	}

	public void Resume()
	{
		_dayNightSpeed = 1f;
	}

	public float GetTimeOfYear()
	{
		double num = 10.0 * 1200.0;
		return Mathf.Repeat((float)(UWE.Utils.Repeat(timePassed, num) / num), 1f);
	}

	private void UpdateAtmosphere()
	{
		float lightScalar = GetLightScalar();
		float value = Mathf.GammaToLinearSpace(GetLocalLightScalar());
		Shader.SetGlobalFloat(ShaderPropertyID._UweLightScalar, lightScalar);
		Shader.SetGlobalColor(ShaderPropertyID._AtmoColor, atmosphereColor.Evaluate(lightScalar));
		Shader.SetGlobalFloat(ShaderPropertyID._UweAtmoLightFade, sunlight.fade);
		Shader.SetGlobalFloat(ShaderPropertyID._UweLocalLightScalar, value);
	}

	private void UpdateDayNightMessage()
	{
		bool flag = IsDay();
		if (flag != dayLastFrame)
		{
			dayNightCycleChangedEvent.Trigger(flag);
			dayLastFrame = flag;
		}
	}

	public void SetLightEnabled(bool lightEnabled)
	{
		sunAndCausticsLight.enabled = lightEnabled;
	}

	public float GetLocalLightScalar()
	{
		Color color = sunAndCausticsLight.color;
		return Mathf.Clamp01(sunAndCausticsLight.intensity * (color.r + color.g + color.b) / 3f * 1.2f - 0.15f);
	}

	public float GetLightScalar()
	{
		float dayScalar = GetDayScalar();
		return sunlight.sunFraction.Evaluate(dayScalar);
	}

	public double GetDay()
	{
		return timePassed / 1200.0;
	}

	public float GetDayScalar()
	{
		return Mathf.Repeat((float)(UWE.Utils.Repeat(timePassed, 1200.0) / 1200.0), 1f);
	}

	public void SetDayNightTime(float scalar)
	{
		bool flag = IsDay();
		scalar = Mathf.Clamp01(scalar);
		timePassedAsDouble += 1200.0 - timePassedAsDouble % 1200.0 + (double)(scalar * 1200f);
		skipTimeMode = false;
		UpdateAtmosphere();
		bool flag2 = IsDay();
		if (flag2 != flag)
		{
			dayNightCycleChangedEvent.Trigger(flag2);
		}
	}

	public static DateTime ToGameDateTime(float realSeconds)
	{
		return dayOrigin.Add(new TimeSpan(0, 0, Mathf.FloorToInt(realSeconds * 72f)));
	}

	public static int ToGameDays(float realSeconds)
	{
		DateTime dateTime = ToGameDateTime(realSeconds);
		dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
		return (dateTime - dayOrigin).Days;
	}

	private void OnConsoleCommand_day(NotificationCenter.Notification n)
	{
		bool num = IsDay();
		ErrorMessage.AddDebug("Day cheat activated");
		timePassedAsDouble += 1200.0 - timePassed % 1200.0 + 600.0;
		skipTimeMode = false;
		_dayNightSpeed = 1f;
		UpdateAtmosphere();
		if (!num)
		{
			dayNightCycleChangedEvent.Trigger(parms: true);
		}
	}

	private void OnConsoleCommand_night(NotificationCenter.Notification n)
	{
		bool num = IsDay();
		ErrorMessage.AddDebug("Night cheat activated");
		timePassedAsDouble += 1200.0 - timePassed % 1200.0;
		skipTimeMode = false;
		_dayNightSpeed = 1f;
		UpdateAtmosphere();
		if (num)
		{
			dayNightCycleChangedEvent.Trigger(parms: false);
		}
	}

	private void OnConsoleCommand_daynight(NotificationCenter.Notification n)
	{
		bool flag = IsDay();
		if (DevConsole.ParseFloat(n, 0, out var value))
		{
			value = Mathf.Clamp01(value);
			ErrorMessage.AddDebug("Setting day/night scalar to " + value + ".");
			timePassedAsDouble += 1200.0 - timePassedAsDouble % 1200.0 + (double)(value * 1200f);
		}
		skipTimeMode = false;
		_dayNightSpeed = 1f;
		UpdateAtmosphere();
		bool flag2 = IsDay();
		if (flag2 != flag)
		{
			dayNightCycleChangedEvent.Trigger(flag2);
		}
	}

	private void OnConsoleCommand_daynightspeed(NotificationCenter.Notification n)
	{
		if (DevConsole.ParseFloat(n, 0, out var value))
		{
			value = Mathf.Clamp(value, 0f, 100f);
			ErrorMessage.AddDebug("Setting day/night speed to " + value + ".");
			_dayNightSpeed = value;
			skipTimeMode = false;
		}
		else
		{
			ErrorMessage.AddDebug("Must specify value from 0 to 100.");
		}
	}

	public bool IsInSkipTimeMode()
	{
		return skipTimeMode;
	}

	public bool SkipTime(float timeAmount, float skipDuration)
	{
		if (skipTimeMode)
		{
			return false;
		}
		if (timeAmount <= 0f || skipDuration <= 0f)
		{
			return false;
		}
		skipTimeMode = true;
		skipModeEndTime = timePassed + (double)timeAmount;
		_dayNightSpeed = timeAmount / skipDuration;
		return true;
	}

	public void StopSkipTimeMode()
	{
		skipTimeMode = false;
		_dayNightSpeed = 1f;
	}
}
