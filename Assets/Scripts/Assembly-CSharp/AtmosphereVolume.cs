using System;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
[SkipProtoContractCheck]
public class AtmosphereVolume : MonoBehaviour, IDeserialized, ICompileTimeCheckable
{
	private const int currentVersion = 3;

	[Range(0f, 1000f)]
	[ProtoMember(1)]
	public int priority = 10;

	[ProtoMember(2)]
	public Color fogColor = new Color(0f, 44f / 85f, 0.6745098f);

	[ProtoMember(3)]
	public float fogStartDistance = 35f;

	[ProtoMember(4)]
	public float fogMaxDistance = 350f;

	[ProtoMember(5)]
	public float fadeDefaultLights = 1f;

	[Obsolete]
	[ProtoMember(6)]
	public float fadeRate = 0.5f;

	[Obsolete]
	[ProtoMember(7)]
	public FogSettings fog;

	[Obsolete]
	[ProtoMember(8)]
	public SunlightSettings sun;

	[Obsolete]
	[ProtoMember(9)]
	public AmbientLightSettings amb;

	[ProtoMember(10)]
	public int version;

	[ProtoMember(11)]
	public string overrideBiome;

	[ProtoMember(13)]
	public bool highDetail = true;

	[ProtoMember(14)]
	public bool affectsVisuals = true;

	private AtmosphereDirector.Settings settings;

	private bool settingsActive;

	private Collider collider;

	public static int GetRequiredLayer()
	{
		return LayerMask.NameToLayer("Trigger");
	}

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
			Debug.Log(string.Concat("Upgrading AtmosphereVolume data ", fog, ", ", sun, ", ", amb), base.gameObject);
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

	public Collider GetCollider()
	{
		return collider;
	}

	public bool Contains(Vector3 pos)
	{
		using (new ProfilingUtils.Sample("AtmosphereVolume.Contains"))
		{
			CacheCollider();
			return UWE.Utils.IsInsideCollider(collider, pos);
		}
	}

	private void PushSettings()
	{
		AtmosphereDirector main = AtmosphereDirector.main;
		if (settings != null && main != null)
		{
			main.PushSettings(settings);
			settingsActive = true;
		}
	}

	private void PopSettings()
	{
		AtmosphereDirector main = AtmosphereDirector.main;
		if (settings != null && main != null)
		{
			main.PopSettings(settings);
			settingsActive = false;
		}
	}

	private void Awake()
	{
		base.gameObject.layer = GetRequiredLayer();
	}

	private void Start()
	{
		if (affectsVisuals)
		{
			AtmosphereDirector.main.AddVolume(this);
			settings = AtmosphereDirector.Settings.Create(priority, base.gameObject.name, base.gameObject);
			settings.fog = fog;
			settings.sun = sun;
			settings.amb = amb;
			settings.fadeRate = fadeRate;
			settings.overrideBiome = overrideBiome;
			InvokeRepeating("CheckTriggerExit", UnityEngine.Random.value, 10f);
		}
		CacheCollider();
		if (!(collider is SphereCollider) && !(collider is BoxCollider) && !(collider is CapsuleCollider))
		{
			Debug.LogFormat("{0} uses a collider type that's unsupported by the AtmosphereVolume", base.name);
		}
	}

	private void OnDestroy()
	{
		if (affectsVisuals)
		{
			AtmosphereDirector.main.RemoveVolume(this);
			PopSettings();
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		if (c.gameObject == Player.mainObject)
		{
			PushSettings();
		}
	}

	private void OnTriggerExit(Collider c)
	{
		if (c.gameObject == Player.mainObject)
		{
			PopSettings();
		}
	}

	private void CheckTriggerExit()
	{
		if (!settingsActive)
		{
			return;
		}
		GameObject mainObject = Player.mainObject;
		if ((bool)mainObject)
		{
			Vector3 position = mainObject.transform.position;
			if (!Contains(position))
			{
				Debug.LogWarning("OnTriggerExit failed. Fixing.", this);
				PopSettings();
			}
		}
	}

	private void CacheCollider()
	{
		if (!collider)
		{
			collider = GetComponent<Collider>();
		}
	}

	private void OnDrawGizmos()
	{
		if (!Event.current.shift || !Event.current.control)
		{
			Gizmos.color = (settingsActive ? Color.green.ToAlpha(0.25f) : Color.red.ToAlpha(0.25f));
			Gizmos.matrix = base.transform.localToWorldMatrix;
			BoxCollider boxCollider = GetComponent<Collider>() as BoxCollider;
			if ((bool)boxCollider)
			{
				Gizmos.DrawCube(boxCollider.center, boxCollider.size);
			}
			SphereCollider sphereCollider = GetComponent<Collider>() as SphereCollider;
			if ((bool)sphereCollider)
			{
				Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
			}
			MeshCollider meshCollider = GetComponent<Collider>() as MeshCollider;
			if ((bool)meshCollider)
			{
				Gizmos.DrawCube(meshCollider.sharedMesh.bounds.center, meshCollider.sharedMesh.bounds.size);
			}
			Gizmos.matrix = Matrix4x4.identity;
		}
	}

	public ulong GetSortKey()
	{
		return (ulong)(((long)priority << 32) + (uint)GetInstanceID());
	}

	public string CompileTimeCheck()
	{
		Collider[] components = GetComponents<Collider>();
		if (components.Length != 1)
		{
			return $"Atmosphere volumes must have exactly one Collider";
		}
		Collider collider = components[0];
		if (!(collider is BoxCollider) && !(collider is SphereCollider) && !(collider is CapsuleCollider))
		{
			return $"Atmosphere volumes only support Box, Capsule and SphereColliders (has {collider.GetType()})";
		}
		return null;
	}
}
