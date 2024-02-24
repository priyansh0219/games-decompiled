using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[DoNotSerialize]
[ProtoContract]
[DisallowMultipleComponent]
public class LargeWorldEntity : MonoBehaviour, IShouldSerialize, ICompileTimeCheckable
{
	public enum CellLevel
	{
		Near = 0,
		Medium = 1,
		Far = 2,
		VeryFar = 3,
		Batch = 10,
		Global = 100
	}

	public const float maxDistSqr = 256f;

	[NonSerialized]
	private Int3 lastGlobalCell = new Int3(-1);

	[ProtoMember(2)]
	public CellLevel cellLevel;

	private CellLevel initialCellLevel = (CellLevel)(-1);

	public const bool FadingEnabled = true;

	private const float minFadeAmount = 0.001f;

	private const float maxFadeTime = 0.5f;

	private const float maxFadeDistance = 30f;

	private float fadeTime = 1000f;

	private List<Renderer> fadeRenderers;

	private IEnumerator fadingCoroutine;

	[NonSerialized]
	public int updaterIndex = -1;

	private void OnEnable()
	{
		LargeWorldEntityUpdater main = LargeWorldEntityUpdater.main;
		if ((bool)main)
		{
			main.Add(this);
		}
		StartFading();
	}

	private void OnDisable()
	{
		LargeWorldEntityUpdater main = LargeWorldEntityUpdater.main;
		if ((bool)main)
		{
			main.Remove(this);
		}
		StopFading();
	}

	private void Awake()
	{
		initialCellLevel = cellLevel;
	}

	public void Start()
	{
		LargeWorldStreamer main = LargeWorldStreamer.main;
		if (!main || !main.IsReady())
		{
			Debug.LogWarningFormat(base.gameObject, "Streamer not ready for {0}", base.name);
		}
		else if (!base.transform.parent)
		{
			Debug.LogWarningFormat(this, "Registering stray entity {0}", base.name);
			main.cellManager.RegisterEntity(this);
		}
		else if ((bool)base.transform.parent.GetComponentInParent<LargeWorldEntity>())
		{
			main.cellManager.UnregisterEntity(this);
		}
	}

	[ContextMenu("Start Fading")]
	private void StartFading()
	{
		if (!WaitScreen.IsWaiting && !(base.transform.GetComponent<SeaMoth>() != null) && !(base.transform.GetComponent<SubRoot>() != null) && !(base.transform.GetComponent<Constructable>() != null) && !(base.transform.GetComponent<PropulseCannonAmmoHandler>() != null))
		{
			fadeRenderers = new List<Renderer>();
			fadeTime = 0f;
			GetComponentsInChildren(fadeRenderers);
			UpdateFadeRenderers(0.001f);
			fadingCoroutine = UpdateFading();
			StartCoroutine(fadingCoroutine);
		}
	}

	[ContextMenu("Stop Fading")]
	private void StopFading()
	{
		if (fadeRenderers != null)
		{
			StopCoroutine(fadingCoroutine);
			if (fadeRenderers != null)
			{
				UpdateFadeRenderers(1f);
			}
			fadingCoroutine = null;
			fadeRenderers = null;
		}
	}

	private IEnumerator UpdateFading()
	{
		while (fadeTime < 0.5f)
		{
			fadeTime += Time.unscaledDeltaTime;
			float opacity = Mathf.Clamp01(fadeTime / 0.5f);
			UpdateFadeRenderers(opacity);
			yield return null;
		}
		UpdateFadeRenderers(1f);
		fadeRenderers = null;
		fadingCoroutine = null;
	}

	public void UpdateCell(LargeWorldStreamer streamer)
	{
		if (IsCellManaged())
		{
			Int3 globalCell = streamer.cellManager.GetGlobalCell(base.transform.position, (int)cellLevel);
			if (globalCell != lastGlobalCell)
			{
				lastGlobalCell = globalCell;
				streamer.cellManager.RegisterEntity(this);
			}
		}
		if (streamer.GetDisableFarColliders())
		{
			Rigidbody component = GetComponent<Rigidbody>();
			if (cellLevel == CellLevel.Near && component != null && component.isKinematic)
			{
				component.detectCollisions = (base.transform.position - MainCamera.camera.transform.position).sqrMagnitude < 256f;
			}
		}
	}

	private void UpdateFadeRenderers(float opacity)
	{
		foreach (Renderer fadeRenderer in fadeRenderers)
		{
			if (fadeRenderer != null)
			{
				fadeRenderer.SetFadeAmount(opacity);
			}
		}
	}

	public void OnAddToCell()
	{
	}

	public bool IsCellManaged()
	{
		return cellLevel < CellLevel.Batch;
	}

	public static void Register(GameObject go)
	{
		LargeWorldStreamer main = LargeWorldStreamer.main;
		if (!main || main.cellManager == null)
		{
			Debug.LogWarningFormat(go, "Streamer not ready for '{0}'", go.name);
		}
		else
		{
			main.cellManager.RegisterEntity(go);
		}
	}

	public bool ShouldSerialize()
	{
		return cellLevel != initialCellLevel;
	}

	public string CompileTimeCheck()
	{
		switch (cellLevel)
		{
		case CellLevel.Near:
		case CellLevel.Medium:
		case CellLevel.Far:
		case CellLevel.VeryFar:
			return null;
		case CellLevel.Batch:
		{
			if ((bool)GetComponentInChildren<AtmosphereVolume>() || (bool)GetComponent<TerrainColliderMarker>())
			{
				return null;
			}
			Light component = GetComponent<Light>();
			if ((bool)component && component.type == LightType.Directional)
			{
				return null;
			}
			if (GetComponentsInChildren<Component>(includeInactive: true).All((Component p) => p is Transform || p is PrefabIdentifier || p is LargeWorldEntity))
			{
				return null;
			}
			return "Batch level entity which is not an AtmosphereVolume or directional light";
		}
		case CellLevel.Global:
			if ((bool)GetComponent<Base>() || (bool)GetComponent<ConstructableBase>() || (bool)GetComponent<MapRoomCamera>() || (bool)GetComponent<Exosuit>() || (bool)GetComponent<Rocket>() || (bool)GetComponent<SeaMoth>())
			{
				return null;
			}
			if ((bool)GetComponent<Accumulator>() || (bool)GetComponent<SolarPanel>() || (bool)GetComponent<ThermalPlant>() || (bool)GetComponent<PowerRelay>() || (bool)GetComponent<Planter>())
			{
				return null;
			}
			if ((bool)GetComponent<Beacon>() || (bool)GetComponent<DiveReel>() || (bool)GetComponent<DiveReelAnchor>() || (bool)GetComponent<Constructor>() || (bool)GetComponent<PipeSurfaceFloater>() || (bool)GetComponent<BasePipeConnector>() || (bool)GetComponent<Pipe>() || (bool)GetComponent<PlayerSoundTrigger>() || (bool)GetComponent<RegeneratePowerSource>() || (bool)GetComponent<SignalPing>() || (bool)GetComponent<DeployableStorage>() || (bool)GetComponent<LEDLight>())
			{
				return null;
			}
			return "Global level entity";
		default:
			return $"Unexpected cell level {cellLevel}";
		}
	}
}
