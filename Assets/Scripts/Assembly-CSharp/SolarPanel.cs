using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class SolarPanel : MonoBehaviour, IHandTarget, IConstructable, IObstacle
{
	public PowerSource powerSource;

	[AssertNotNull]
	public PowerRelay relay;

	public float maxDepth = 200f;

	[AssertNotNull]
	public AnimationCurve depthCurve;

	[AssertLocalization(3)]
	private const string solarPanelStatusFormatKey = "SolarPanelStatus";

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public float biomeSunlightScale = 1f;

	private float GetDepthScalar()
	{
		float time = Mathf.Clamp01((maxDepth - Ocean.GetDepthOf(base.gameObject)) / maxDepth);
		return depthCurve.Evaluate(time);
	}

	private float GetSunScalar()
	{
		return DayNightCycle.main.GetLocalLightScalar() * biomeSunlightScale;
	}

	private float GetRechargeScalar()
	{
		return GetDepthScalar() * GetSunScalar();
	}

	private void Start()
	{
		AtmosphereDirector.onVolumeAdded += OnAtmosphereVolumeAdded;
	}

	private void OnDestroy()
	{
		AtmosphereDirector.onVolumeAdded -= OnAtmosphereVolumeAdded;
	}

	private void Update()
	{
		if (base.gameObject.GetComponent<Constructable>().constructed)
		{
			float amount = GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.25f * 5f;
			relay.ModifyPower(amount, out var _);
		}
	}

	private void OnAtmosphereVolumeAdded(AtmosphereVolume volume)
	{
		if (volume.Contains(base.transform.position) && WaterBiomeManager.main.GetSettings(volume.overrideBiome, out var settings))
		{
			biomeSunlightScale = Mathf.Clamp01(settings.sunlightScale);
		}
	}

	void IConstructable.OnConstructedChanged(bool constructed)
	{
		if (constructed && WaterBiomeManager.main.GetSettings(base.transform.position, onlyAffectsVisuals: true, out var settings))
		{
			biomeSunlightScale = Mathf.Clamp01(settings.sunlightScale);
		}
	}

	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	bool IObstacle.CanDeconstruct(out string reason)
	{
		reason = null;
		return true;
	}

	public void OnHandHover(GUIHand hand)
	{
		if (base.gameObject.GetComponent<Constructable>().constructed)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat("SolarPanelStatus", Mathf.RoundToInt(GetRechargeScalar() * 100f), Mathf.RoundToInt(powerSource.GetPower()), Mathf.RoundToInt(powerSource.GetMaxPower())), translate: false);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
	}
}
