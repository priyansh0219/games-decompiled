using System;
using System.Collections.Generic;
using System.Linq;
using UWE;
using UnityEngine;

public class AtmosphereDirector : MonoBehaviour
{
	[Serializable]
	public class Settings
	{
		public int priority;

		public string name;

		public List<Light> lights;

		public FogSettings fog;

		public SunlightSettings sun;

		public AmbientLightSettings amb;

		public float fadeRate = 0.5f;

		public string overrideBiome;

		public static Settings Create(int priority, string name, GameObject zoneRoot)
		{
			Settings settings = new Settings();
			settings.priority = priority;
			settings.name = name;
			List<Light> list = new List<Light>(GetComponentsInZone<Light>(zoneRoot));
			for (int i = 0; i < list.Count; i++)
			{
				Light light = list[i];
				light.enabled = false;
				if (!ShadowsEnabled())
				{
					light.shadows = LightShadows.None;
				}
			}
			settings.lights = list;
			return settings;
		}

		public override string ToString()
		{
			return string.Concat("(", name, ", prio ", priority, ", fog ", fog, ", sun ", sun, ", amb ", amb, ", rate ", fadeRate, ", ", lights.Count, " lights)");
		}

		private static bool IsSubzoneManaged(Component c, GameObject zoneRoot)
		{
			AtmosphereVolume componentInParent = c.GetComponentInParent<AtmosphereVolume>();
			if ((bool)componentInParent && componentInParent.gameObject != zoneRoot)
			{
				return true;
			}
			return false;
		}

		private static T GetComponentInZone<T>(GameObject zoneRoot) where T : Component
		{
			return zoneRoot.GetComponentsInChildren<T>().FirstOrDefault((T c) => !IsSubzoneManaged(c, zoneRoot));
		}

		private static IEnumerable<T> GetComponentsInZone<T>(GameObject zoneRoot) where T : Component
		{
			if (!zoneRoot)
			{
				return Enumerable.Empty<T>();
			}
			return from c in zoneRoot.GetComponentsInChildren<T>()
				where !IsSubzoneManaged(c, zoneRoot)
				select c;
		}
	}

	public static AtmosphereDirector main;

	public bool debug;

	private float intensityFadeRate = 0.5f;

	private WaterscapeVolume waterVolume;

	private uSkyManager skyManager;

	public GameObject defaultLightSet;

	private Settings defaultSettings;

	private FogSettings targetFog;

	private AmbientLightSettings targetAmbient;

	private FadeLightController targetLightController;

	private SunlightSettings targetSun;

	private string targetBiome;

	private float shadowed;

	private float directLightScale = 1f;

	private float indirectLightScale = 1f;

	private float scatteringScale = 1f;

	private List<AtmosphereVolume> volumes = new List<AtmosphereVolume>();

	private List<Settings> priorityQueue = new List<Settings>();

	private Settings lastSettings;

	public static event Action<AtmosphereVolume> onVolumeAdded;

	public static event Action<AtmosphereVolume> onVolumeRemoved;

	public static bool ShadowsEnabled()
	{
		return QualitySettings.shadowDistance > 0f;
	}

	private void Awake()
	{
		main = this;
	}

	private static ulong GetSortKey(AtmosphereVolume volume)
	{
		return (ulong)(((long)volume.priority << 32) + (uint)volume.GetInstanceID());
	}

	public void AddVolume(AtmosphereVolume volume)
	{
		try
		{
			if (!(volume.GetComponent<Collider>() == null) && (volume.overrideBiome == null || volume.overrideBiome.Length != 0))
			{
				ulong sortKey = volume.GetSortKey();
				int i;
				for (i = 0; i < volumes.Count && sortKey >= volumes[i].GetSortKey(); i++)
				{
				}
				volumes.Insert(i, volume);
				AtmosphereDirector.onVolumeAdded?.Invoke(volume);
			}
		}
		finally
		{
		}
	}

	public void RemoveVolume(AtmosphereVolume volume)
	{
		volumes.Remove(volume);
		AtmosphereDirector.onVolumeRemoved?.Invoke(volume);
	}

	public List<AtmosphereVolume> GetVolumes()
	{
		return volumes;
	}

	private void Start()
	{
		WaterscapeVolume[] array = UnityEngine.Object.FindObjectsOfType<WaterscapeVolume>();
		int num = 0;
		if (num < array.Length)
		{
			WaterscapeVolume waterscapeVolume = array[num];
			waterVolume = waterscapeVolume;
		}
		skyManager = uSkyManager.main;
		defaultLightSet.AddComponent<SunlightController>().Initialize(1f);
		GameObject gameObject = new GameObject("default atmospherics");
		gameObject.transform.parent = base.transform;
		defaultSettings = Settings.Create(-1, gameObject.name, gameObject);
		defaultSettings.fog = new FogSettings();
		defaultSettings.fog.enabled = true;
		defaultSettings.fog.sunGlowAmount = waterVolume.sunLightAmount;
		defaultSettings.sun = new SunlightSettings();
		defaultSettings.sun.enabled = true;
		defaultSettings.sun.fade = 1f;
		defaultSettings.sun.color = SunlightSettings.defaultColor;
		defaultSettings.sun.replaceFraction = 0f;
		defaultSettings.amb = new AmbientLightSettings();
		defaultSettings.amb.enabled = true;
		defaultSettings.amb.dayNightColor = UWE.Utils.DayNightGradient(RenderSettings.ambientLight);
		defaultSettings.fadeRate = 0.1f;
		PushSettings(defaultSettings);
	}

	public void ResetDirector()
	{
		if (debug)
		{
			DebugSettings("reset settings\n");
		}
		priorityQueue.Clear();
		PushSettings(defaultSettings);
	}

	public void PushSettings(Settings s)
	{
		if (debug)
		{
			DebugSettings(string.Concat("push settings ", s, "to queue\n"));
		}
		priorityQueue.Remove(s);
		int index = priorityQueue.TakeWhile((Settings c) => c.priority > s.priority).Count();
		priorityQueue.Insert(index, s);
		Settings settings = priorityQueue[0];
		RequestSettingsInternal(settings, settings.fadeRate);
	}

	public void PopSettings(Settings s)
	{
		if (debug)
		{
			DebugSettings(string.Concat("pop settings ", s, "from queue\n"));
		}
		float fadeRate = priorityQueue[0].fadeRate;
		priorityQueue.Remove(s);
		RequestSettingsInternal(priorityQueue[0], fadeRate);
	}

	public string GetBiomeOverride()
	{
		return targetBiome;
	}

	[Obsolete("Use Push/PopSettings instead.")]
	public void RequestSettings(Settings s)
	{
		if (s != lastSettings)
		{
			PopSettings(lastSettings);
			PushSettings(s);
		}
	}

	private void RequestSettingsInternal(Settings s, float fadeRate)
	{
		intensityFadeRate = fadeRate;
		targetFog = priorityQueue.Select((Settings p) => p.fog).First((FogSettings p) => p?.enabled ?? false);
		targetAmbient = priorityQueue.Select((Settings p) => p.amb).First((AmbientLightSettings p) => p?.enabled ?? false);
		targetSun = priorityQueue.Select((Settings p) => p.sun).First((SunlightSettings p) => p?.enabled ?? false);
		targetBiome = priorityQueue.Select((Settings p) => p.overrideBiome).FirstOrDefault((string p) => !string.IsNullOrEmpty(p));
		if (debug)
		{
			DebugSettings(string.Concat("blending to fog ", targetFog, ", amb ", targetAmbient, ", sun ", targetSun, ", in ", fadeRate, "s\n"));
		}
		if (targetLightController != null)
		{
			targetLightController.FadeDestroy(intensityFadeRate);
		}
		GameObject gameObject = new GameObject("light set");
		gameObject.transform.parent = base.transform;
		for (int i = 0; i < s.lights.Count; i++)
		{
			Utils.SpawnFromPrefab(s.lights[i].gameObject, gameObject.transform);
		}
	}

	public void DebugSettings(string message = "")
	{
		Debug.Log(priorityQueue.Aggregate(message, (string str, Settings s) => string.Concat(str, s, "\n")), this);
	}
}
