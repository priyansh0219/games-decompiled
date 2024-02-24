using System;
using UnityEngine;
using mset;

public class SkyApplier : MonoBehaviour
{
	private const float updateDistance = 2f;

	public Skies anchorSky;

	public GameObject customSkyPrefab;

	public bool dynamic;

	public bool emissiveFromPower;

	[AssertNotNull]
	public Renderer[] renderers;

	private Sky environmentSky;

	private Vector3 applyPosition;

	private Sky applySky;

	private LightingController lightControl;

	[NonSerialized]
	public int updaterIndex = -1;

	public Sky InspectorEnvironmentSky => environmentSky;

	public Sky InspectorApplySky => applySky;

	public LightingController InspectorLightControl => lightControl;

	private void Awake()
	{
		base.enabled = true;
	}

	private void Start()
	{
		if (anchorSky == Skies.Custom && customSkyPrefab != null)
		{
			environmentSky = MarmoSkies.main.GetSky(customSkyPrefab);
		}
		if (applySky == null)
		{
			GameObject environment = GetEnvironment(base.gameObject, anchorSky);
			GetAndApplySkybox(environment);
		}
		if (emissiveFromPower)
		{
			lightControl = GetComponentInParent<LightingController>();
			if ((bool)lightControl)
			{
				lightControl.RegisterSkyApplier(this);
			}
		}
		if (!dynamic)
		{
			base.enabled = false;
		}
	}

	private void ApplySkybox()
	{
		applyPosition = base.transform.position;
		Sky biomeEnvironment = environmentSky;
		if (biomeEnvironment == null)
		{
			biomeEnvironment = WaterBiomeManager.main.GetBiomeEnvironment(applyPosition);
		}
		if (!(biomeEnvironment != applySky))
		{
			return;
		}
		if (applySky != null)
		{
			applySky.UnregisterSkyApplier(this);
		}
		applySky = biomeEnvironment;
		if (!(biomeEnvironment != null))
		{
			return;
		}
		for (int i = 0; i < renderers.Length; i++)
		{
			Renderer renderer = renderers[i];
			if ((bool)renderer)
			{
				biomeEnvironment.ApplyFast(renderer, 0);
			}
		}
		biomeEnvironment.RegisterSkyApplier(this);
	}

	public void SetSky(Skies skyMode)
	{
		if (skyMode != Skies.Custom)
		{
			anchorSky = skyMode;
			GameObject environment = GetEnvironment(base.gameObject, anchorSky);
			GetAndApplySkybox(environment);
		}
	}

	public void SetCustomSky(Sky customSky)
	{
		if (customSky != null)
		{
			anchorSky = Skies.Custom;
			dynamic = false;
			environmentSky = customSky;
			ApplySkybox();
		}
	}

	private bool HasMoved()
	{
		return (applyPosition - base.transform.position).sqrMagnitude >= 4f;
	}

	public void UpdateSkyIfNecessary()
	{
		if (HasMoved())
		{
			ApplySkybox();
		}
	}

	public void RefreshDirtySky()
	{
		if (!(environmentSky != null))
		{
			return;
		}
		for (int i = 0; i < renderers.Length; i++)
		{
			Renderer renderer = renderers[i];
			if ((bool)renderer)
			{
				environmentSky.ApplyFast(renderer, 0);
			}
		}
	}

	public void OnEnvironmentChanged(SkyEnvironmentChanged.Parameters parameters)
	{
		GameObject environment = parameters.environment;
		GetAndApplySkybox(environment);
	}

	private static Sky GetSkyForEnvironment(GameObject environment, Skies anchorMode)
	{
		if (environment != null)
		{
			MarmoLifepodSky component = environment.GetComponent<MarmoLifepodSky>();
			if (component != null)
			{
				return component.anchorSky;
			}
			SubRoot component2 = environment.GetComponent<SubRoot>();
			if (component2 != null)
			{
				if (anchorMode == Skies.BaseGlass)
				{
					return component2.glassSky;
				}
				return component2.interiorSky;
			}
		}
		return null;
	}

	private static GameObject GetEnvironment(GameObject gameObject, Skies anchorMode)
	{
		SubRoot componentInParent = gameObject.GetComponentInParent<SubRoot>();
		Constructable componentInParent2 = gameObject.GetComponentInParent<Constructable>();
		if (componentInParent2 != null)
		{
			bool flag = false;
			if (componentInParent != null)
			{
				flag = componentInParent.isCyclops;
			}
			if (!componentInParent2.IsInside() && !flag)
			{
				return null;
			}
		}
		MarmoLifepodSky componentInParent3 = gameObject.GetComponentInParent<MarmoLifepodSky>();
		if (componentInParent3 != null)
		{
			return componentInParent3.gameObject;
		}
		if (gameObject.GetComponentInParent<BaseCell>() != null && anchorMode != Skies.BaseGlass && anchorMode != Skies.BaseInterior)
		{
			return null;
		}
		if (componentInParent != null && (componentInParent.gameObject != gameObject || anchorMode == Skies.BaseGlass || anchorMode == Skies.BaseInterior))
		{
			return componentInParent.gameObject;
		}
		return null;
	}

	private void GetAndApplySkybox(GameObject environment)
	{
		if (anchorSky != Skies.Custom)
		{
			if (anchorSky == Skies.Auto || anchorSky == Skies.BaseInterior || anchorSky == Skies.BaseGlass)
			{
				environmentSky = GetSkyForEnvironment(environment, anchorSky);
			}
			else
			{
				environmentSky = MarmoSkies.main.GetSky(anchorSky);
			}
		}
		ApplySkybox();
	}

	private void OnDestroy()
	{
		if (applySky != null)
		{
			applySky.UnregisterSkyApplier(this);
		}
		if (lightControl != null)
		{
			lightControl.UnregisterSkyApplier(this);
		}
	}

	public void DebugRefreshSky()
	{
		if (!(applySky != null))
		{
			return;
		}
		for (int i = 0; i < renderers.Length; i++)
		{
			Renderer renderer = renderers[i];
			if ((bool)renderer)
			{
				applySky.ApplyFast(renderer, 0);
			}
		}
	}

	private void OnEnable()
	{
		if ((bool)SkyApplierUpdater.main)
		{
			SkyApplierUpdater.main.Add(this);
		}
	}

	private void OnDisable()
	{
		if ((bool)SkyApplierUpdater.main)
		{
			SkyApplierUpdater.main.Remove(this);
		}
	}
}
