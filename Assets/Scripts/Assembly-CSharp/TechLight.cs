using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class TechLight : MonoBehaviour, IConstructable, IObstacle
{
	[AssertNotNull]
	public GameObject lights;

	[AssertNotNull]
	public Constructable constructable;

	[AssertNotNull]
	public PowerFX powerFX;

	[AssertNotNull]
	public VFXTechLight vfxTechLight;

	private bool searching;

	private PowerRelay powerRelay;

	private bool searchingRelay;

	private static float connectionDistance = 20f;

	private static float powerPerSecond = 0.2f;

	private static float updateInterval = 3f;

	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public bool placedByPlayer;

	private void Start()
	{
		InvokeRepeating("UpdatePower", 0f, updateInterval);
		if (placedByPlayer)
		{
			SetLightsActive(active: false);
		}
		if (version < 2)
		{
			EnergyMixin component = GetComponent<EnergyMixin>();
			if ((bool)component)
			{
				UnityEngine.Object.Destroy(component);
			}
			ToggleLights component2 = GetComponent<ToggleLights>();
			if ((bool)component2)
			{
				UnityEngine.Object.Destroy(component2);
			}
			Battery[] componentsInChildren = GetComponentsInChildren<Battery>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
			}
			version = 2;
		}
	}

	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string reason)
	{
		reason = null;
		return placedByPlayer;
	}

	public void OnConstructedChanged(bool constructed)
	{
		if (!constructed)
		{
			SetLightsActive(active: false);
			vfxTechLight.placedByPlayer = true;
			placedByPlayer = true;
		}
	}

	private void SetLightsActive(bool active)
	{
		lights.SetActive(active);
		powerFX.SetVFXVisible(active);
		vfxTechLight.SetLightOnOff(active);
	}

	private void UpdatePower()
	{
		if (!placedByPlayer || !constructable.constructed)
		{
			return;
		}
		if ((bool)powerRelay)
		{
			if (powerRelay.GetPowerStatus() == PowerSystem.Status.Normal)
			{
				SetLightsActive(active: true);
				powerRelay.ConsumeEnergy(powerPerSecond * updateInterval, out var _);
			}
			else
			{
				SetLightsActive(active: false);
			}
			return;
		}
		SetLightsActive(active: false);
		if (!searching)
		{
			searching = true;
			InvokeRepeating("FindNearestValidRelay", 0f, 2f);
		}
	}

	public static PowerRelay GetNearestValidRelay(GameObject fromObject)
	{
		PowerRelay result = null;
		float num = 1000f;
		for (int i = 0; i < PowerRelay.relayList.Count; i++)
		{
			PowerRelay powerRelay = PowerRelay.relayList[i];
			if (powerRelay is BasePowerRelay && powerRelay.gameObject.activeInHierarchy && powerRelay.enabled && !powerRelay.dontConnectToRelays && !(Builder.GetGhostModel() == powerRelay.gameObject))
			{
				float magnitude = (powerRelay.GetConnectPoint(fromObject.transform.position) - fromObject.transform.position).magnitude;
				if (magnitude <= connectionDistance && magnitude < num)
				{
					num = magnitude;
					result = powerRelay;
				}
			}
		}
		return result;
	}

	public void FindNearestValidRelay()
	{
		PowerRelay nearestValidRelay = GetNearestValidRelay(base.gameObject);
		if ((bool)nearestValidRelay)
		{
			powerRelay = nearestValidRelay;
			powerFX.SetTarget(powerRelay.gameObject);
			searching = false;
			CancelInvoke("FindNearestValidRelay");
		}
	}
}
