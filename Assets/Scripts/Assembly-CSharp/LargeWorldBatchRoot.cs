using System;
using System.Collections;
using Gendarme;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class LargeWorldBatchRoot : MonoBehaviour, IDeserialized, ISerializeInEditMode
{
	private const int currentVersion = 3;

	public const int atmospherePriority = 1;

	public static bool EditMode;

	[NonSerialized]
	public Int3 batchId;

	[NonSerialized]
	[DoNotSerialize]
	public LargeWorldStreamer streamer;

	[ProtoMember(2)]
	public string overrideBiome;

	[NonSerialized]
	[DoNotSerialize]
	public AtmosphereDirector.Settings atmosphereSettings;

	[ProtoMember(3)]
	public Color fogColor = new Color(0f, 44f / 85f, 0.6745098f);

	[ProtoMember(4)]
	public float fogStartDistance = 35f;

	[ProtoMember(5)]
	public float fogMaxDistance = 350f;

	[ProtoMember(6)]
	public float fadeDefaultLights = 1f;

	[ProtoMember(7)]
	public float fadeRate = 0.1f;

	[ProtoMember(8)]
	public FogSettings fog;

	[ProtoMember(9)]
	public SunlightSettings sun;

	[ProtoMember(10)]
	public AmbientLightSettings amb;

	[ProtoMember(11)]
	public int version;

	[ProtoMember(12)]
	public string atmospherePrefabClassId;

	private bool oldInBatch;

	[ProtoBeforeSerialization]
	public void OnBeforeSerialization()
	{
		version = 3;
	}

	[ProtoAfterDeserialization]
	public void OnAfterDeserialization()
	{
		UpgradeData();
	}

	public void Deserialized()
	{
		UpgradeData();
	}

	private void UpgradeData()
	{
		if (version < 2)
		{
			Debug.Log(string.Concat("Upgrading LargeWorldBatchRoot ", fog, ", ", sun, ", ", amb), base.gameObject);
			fog = fog ?? new FogSettings();
			fog.enabled |= !fogColor.ApproxEquals(fog.color, 0.003921569f) || !Mathf.Approximately(fogStartDistance, fog.startDistance) || !Mathf.Approximately(fogMaxDistance, fog.maxDistance);
			fog.dayNightColor = UWE.Utils.DayNightGradient(fogColor);
			fog.startDistance = fogStartDistance;
			fog.maxDistance = fogMaxDistance;
			sun = sun ?? new SunlightSettings();
			sun.enabled |= !Mathf.Approximately(fadeDefaultLights, sun.fade);
			sun.fade = fadeDefaultLights;
			version = 2;
		}
	}

	public void OnDisable()
	{
		CancelInvoke("UpdateAtmosphere");
	}

	[SuppressMessage("Subnautica.Rules", "AvoidStringConcatenation")]
	public IEnumerator Start()
	{
		yield return PullSettingsFromPrefab();
		atmosphereSettings = AtmosphereDirector.Settings.Create(1, base.gameObject.name, base.gameObject);
		atmosphereSettings.fadeRate = fadeRate;
		InvokeRepeating("UpdateAtmosphere", UnityEngine.Random.value, 1f);
	}

	public void OnDestroy()
	{
		if (atmosphereSettings != null && AtmosphereDirector.main != null)
		{
			AtmosphereDirector.main.PopSettings(atmosphereSettings);
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidStringConcatenation")]
	public IEnumerator PullSettingsFromPrefab()
	{
		if (string.IsNullOrEmpty(atmospherePrefabClassId))
		{
			yield break;
		}
		IPrefabRequest request = PrefabDatabase.GetPrefabAsync(atmospherePrefabClassId);
		yield return request;
		if (!request.TryGetPrefab(out var prefab))
		{
			yield break;
		}
		AtmosphereVolume component = prefab.GetComponent<AtmosphereVolume>();
		if (!component)
		{
			yield break;
		}
		fog.CopyFrom(component.fog);
		sun.CopyFrom(component.sun);
		amb.CopyFrom(component.amb);
		fadeRate = component.fadeRate;
		overrideBiome = component.overrideBiome;
		if (atmosphereSettings != null)
		{
			AtmosphereDirector.main.PopSettings(atmosphereSettings);
			atmosphereSettings.fog = fog;
			atmosphereSettings.sun = sun;
			atmosphereSettings.amb = amb;
			atmosphereSettings.fadeRate = fadeRate;
			if (oldInBatch)
			{
				AtmosphereDirector.main.PushSettings(atmosphereSettings);
			}
		}
	}

	private void UpdateAtmosphere()
	{
		if (AtmosphereDirector.main == null || MainCamera.camera == null || atmosphereSettings == null || !(streamer != null))
		{
			return;
		}
		bool flag = streamer.GetContainingBatch(MainCamera.camera.transform.position) == batchId;
		if (flag != oldInBatch)
		{
			if (flag)
			{
				AtmosphereDirector.main.PushSettings(atmosphereSettings);
			}
			else
			{
				AtmosphereDirector.main.PopSettings(atmosphereSettings);
			}
			oldInBatch = flag;
		}
	}
}
