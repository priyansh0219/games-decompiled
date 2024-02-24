using System;
using System.Collections;
using System.Collections.Generic;
using UWE;
using UnityEngine;

public class PowerRelay : MonoBehaviour, IPowerInterface
{
	[NonSerialized]
	public PowerRelay outboundRelay;

	[AssertNotNull]
	public GameObject powerSystemPreviewPrefab;

	public int maxOutboundDistance;

	public PowerSource internalPowerSource;

	public bool dontConnectToRelays;

	private List<IPowerInterface> inboundPowerSources = new List<IPowerInterface>();

	private PowerSystem.Status powerStatus;

	public PowerFX powerFX;

	[NonSerialized]
	public Event<PowerRelay> powerDownEvent = new Event<PowerRelay>();

	[NonSerialized]
	public Event<PowerRelay> powerUpEvent = new Event<PowerRelay>();

	private bool isPowered;

	private bool lastPowered;

	private bool electronicsDisabled;

	private float enableElectonicsTime = -2f;

	private GameObject powerPreview;

	public static List<PowerRelay> relayList = new List<PowerRelay>();

	private Constructable constructable;

	private bool isDirty = true;

	private int curFindRelayIndex;

	private bool lastCanConnect;

	public virtual void Start()
	{
		InvokeRepeating("UpdatePowerState", UnityEngine.Random.value, 0.5f);
		InvokeRepeating("MonitorCurrentConnection", UnityEngine.Random.value, 1f);
		lastCanConnect = CanMakeConnection();
		StartCoroutine(UpdateConnectionAsync());
		constructable = GetComponent<Constructable>();
		RegisterRelay(this);
		powerPreview = UnityEngine.Object.Instantiate(powerSystemPreviewPrefab, Vector3.zero, Quaternion.identity);
		powerPreview.transform.position = GetConnectPoint();
		powerPreview.transform.parent = base.transform;
		powerPreview.SetActive(value: false);
		UpdatePowerState();
		if (WaitScreen.IsWaiting)
		{
			lastPowered = (isPowered = true);
			powerStatus = PowerSystem.Status.Normal;
		}
	}

	public static void MarkRelaySystemDirty()
	{
		foreach (PowerRelay relay in relayList)
		{
			relay.isDirty = true;
		}
	}

	public bool IsUnderConstruction()
	{
		if (constructable != null)
		{
			return !constructable.constructed;
		}
		return false;
	}

	public bool IsNearestValidRelay(PowerRelay otherRelay)
	{
		if (!CanMakeConnection())
		{
			return false;
		}
		float closestDistSq = maxOutboundDistance * maxOutboundDistance;
		if (outboundRelay != null)
		{
			Vector3 connectPoint = GetConnectPoint();
			closestDistSq = (outboundRelay.GetConnectPoint(connectPoint) - connectPoint).sqrMagnitude;
		}
		PowerRelay closestRelay = null;
		CheckRelayForValidCloserConnection(otherRelay, includeGhostModels: true, ref closestDistSq, ref closestRelay);
		if (closestRelay == otherRelay)
		{
			return true;
		}
		return false;
	}

	public PowerRelay FindNearestValidRelay(bool includeGhostModels)
	{
		PowerRelay closestRelay = null;
		float closestDistSq = maxOutboundDistance * maxOutboundDistance;
		for (int i = 0; i < relayList.Count; i++)
		{
			CheckRelayForValidCloserConnection(i, includeGhostModels, ref closestDistSq, ref closestRelay);
		}
		return closestRelay;
	}

	public bool FindNearestValidRelayAsync(bool includeGhostModels, ref float curClosestDistSq, ref PowerRelay relayToConnectTo)
	{
		if (curFindRelayIndex >= relayList.Count)
		{
			return true;
		}
		CheckRelayForValidCloserConnection(curFindRelayIndex, includeGhostModels, ref curClosestDistSq, ref relayToConnectTo);
		curFindRelayIndex++;
		return false;
	}

	private bool IsValidRelayForConnection(PowerRelay potentialRelay, bool includeGhostModels)
	{
		try
		{
			if (!potentialRelay.gameObject.activeInHierarchy)
			{
				return false;
			}
			if (!potentialRelay.enabled)
			{
				return false;
			}
			if (potentialRelay == this)
			{
				return false;
			}
			if (potentialRelay.dontConnectToRelays)
			{
				return false;
			}
			if (potentialRelay.internalPowerSource != null)
			{
				return false;
			}
			if (potentialRelay.IsUnderConstruction())
			{
				return false;
			}
			if (!includeGhostModels && Builder.GetGhostModel() == potentialRelay.gameObject)
			{
				return false;
			}
			if (potentialRelay.GetOutboundConnectsTo(this))
			{
				return false;
			}
			PowerRelay endpoint = potentialRelay.GetEndpoint();
			if (GetInboundHasSource(endpoint))
			{
				return false;
			}
			return true;
		}
		finally
		{
		}
	}

	private void CheckRelayForValidCloserConnection(PowerRelay potentialRelay, bool includeGhostModels, ref float closestDistSq, ref PowerRelay closestRelay)
	{
		if (IsValidRelayForConnection(potentialRelay, includeGhostModels))
		{
			Vector3 connectPoint = GetConnectPoint();
			float sqrMagnitude = (potentialRelay.GetConnectPoint(connectPoint) - connectPoint).sqrMagnitude;
			if (sqrMagnitude <= closestDistSq && (!outboundRelay || outboundRelay == potentialRelay || sqrMagnitude < (outboundRelay.GetConnectPoint(connectPoint) - connectPoint).sqrMagnitude))
			{
				closestDistSq = sqrMagnitude;
				closestRelay = potentialRelay;
			}
		}
	}

	private void CheckRelayForValidCloserConnection(int index, bool includeGhostModels, ref float closestDistSq, ref PowerRelay closestRelay)
	{
		PowerRelay potentialRelay = relayList[index];
		CheckRelayForValidCloserConnection(potentialRelay, includeGhostModels, ref closestDistSq, ref closestRelay);
	}

	public static void RegisterRelay(PowerRelay relay)
	{
		if (!relayList.Contains(relay))
		{
			relayList.Add(relay);
			MarkRelaySystemDirty();
		}
	}

	public static void UnregisterRelay(PowerRelay relay)
	{
		IPowerInterface component = relay.GetComponent<IPowerInterface>();
		foreach (PowerRelay relay2 in relayList)
		{
			relay2.RemoveInboundPower(component);
		}
		relayList.Remove(relay);
		MarkRelaySystemDirty();
	}

	public PowerRelay GetOutboundRelay()
	{
		return outboundRelay;
	}

	public virtual Vector3 GetConnectPoint(Vector3 fromPosition)
	{
		return GetConnectPoint();
	}

	public Vector3 GetConnectPoint()
	{
		Vector3 position = base.transform.position;
		PowerFX component = GetComponent<PowerFX>();
		if (component != null && component.attachPoint != null)
		{
			position = component.attachPoint.position;
		}
		return position;
	}

	public float GetPower()
	{
		float num = 0f;
		if (!electronicsDisabled)
		{
			num = ((!internalPowerSource) ? GetPowerFromInbound() : internalPowerSource.GetPower());
		}
		if (!electronicsDisabled && Time.time < enableElectonicsTime + 2f)
		{
			num *= Mathf.InverseLerp(enableElectonicsTime, enableElectonicsTime + 2f, Time.time);
		}
		return num;
	}

	public float GetMaxPower()
	{
		float num = 0f;
		if ((bool)internalPowerSource)
		{
			num = internalPowerSource.GetMaxPower();
		}
		else
		{
			for (int i = 0; i < inboundPowerSources.Count; i++)
			{
				num += inboundPowerSources[i].GetMaxPower();
			}
		}
		return num;
	}

	public bool ModifyPower(float amount, out float modified)
	{
		bool result = false;
		modified = 0f;
		if (!electronicsDisabled)
		{
			result = ((!internalPowerSource) ? ModifyPowerFromInbound(amount, out modified) : internalPowerSource.ModifyPower(amount, out modified));
		}
		return result;
	}

	public float GetPowerFromInbound()
	{
		float num = 0f;
		for (int i = 0; i < inboundPowerSources.Count; i++)
		{
			num += inboundPowerSources[i].GetPower();
		}
		return num;
	}

	public bool ModifyPowerFromInbound(float amount, out float modified)
	{
		bool flag = false;
		modified = 0f;
		for (int i = 0; i < inboundPowerSources.Count; i++)
		{
			float modified2 = 0f;
			flag = inboundPowerSources[i].ModifyPower(amount, out modified2);
			modified += modified2;
			amount -= modified2;
			if (flag)
			{
				break;
			}
		}
		return flag;
	}

	public void AddInboundPower(IPowerInterface powerInterface)
	{
		if (!inboundPowerSources.Contains(powerInterface))
		{
			inboundPowerSources.Add(powerInterface);
		}
	}

	public bool RemoveInboundPower(IPowerInterface powerInterface)
	{
		return inboundPowerSources.Remove(powerInterface);
	}

	public bool HasInboundPower(IPowerInterface powerInterface)
	{
		bool flag = this == powerInterface;
		if (!flag)
		{
			for (int i = 0; i < inboundPowerSources.Count; i++)
			{
				if (inboundPowerSources[i].HasInboundPower(powerInterface))
				{
					flag = true;
					break;
				}
			}
		}
		return flag;
	}

	public bool GetInboundHasSource(IPowerInterface powerInterface)
	{
		if (internalPowerSource != null && internalPowerSource.IsConnected() && internalPowerSource.connectedRelay == powerInterface)
		{
			return true;
		}
		for (int i = 0; i < inboundPowerSources.Count; i++)
		{
			if (inboundPowerSources[i].GetInboundHasSource(powerInterface))
			{
				return true;
			}
		}
		return false;
	}

	public PowerRelay GetEndpoint()
	{
		PowerRelay powerRelay = this;
		while (powerRelay != null && powerRelay.outboundRelay != null)
		{
			powerRelay = powerRelay.outboundRelay;
		}
		return powerRelay;
	}

	public bool GetOutboundConnectsTo(PowerRelay relay)
	{
		PowerRelay powerRelay = this;
		while (powerRelay != null && powerRelay.outboundRelay != null)
		{
			if (powerRelay.outboundRelay == relay)
			{
				return true;
			}
			powerRelay = powerRelay.outboundRelay;
		}
		return false;
	}

	protected virtual void UpdatePowerState()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (electronicsDisabled && Time.time > enableElectonicsTime)
		{
			electronicsDisabled = false;
		}
		if (GameModeUtils.RequiresPower())
		{
			float power = GetPower();
			float maxPower = GetMaxPower();
			if (maxPower != 0f)
			{
				float num = power / maxPower;
				if (num == 0f || !isPowered)
				{
					powerStatus = PowerSystem.Status.Offline;
				}
				else if (num <= 0.2f)
				{
					powerStatus = PowerSystem.Status.Emergency;
				}
				else
				{
					powerStatus = PowerSystem.Status.Normal;
				}
			}
			else
			{
				powerStatus = PowerSystem.Status.Offline;
			}
			isPowered = power > 0f;
		}
		else
		{
			isPowered = true;
			powerStatus = PowerSystem.Status.Normal;
		}
		if (isPowered != lastPowered)
		{
			if (isPowered)
			{
				powerUpEvent.Trigger(this);
			}
			else
			{
				powerDownEvent.Trigger(this);
			}
			lastPowered = isPowered;
		}
	}

	public PowerRelay FindRelayToConnectTo()
	{
		PowerRelay result = null;
		if (base.gameObject.activeInHierarchy && !dontConnectToRelays && !Mathf.Approximately(GetMaxPower(), 0f) && !IsUnderConstruction())
		{
			result = FindNearestValidRelay(includeGhostModels: false);
		}
		return result;
	}

	private void MonitorCurrentConnection()
	{
		if (outboundRelay != null && !IsValidRelayForConnection(outboundRelay, includeGhostModels: false))
		{
			DisconnectFromRelay();
			isDirty = true;
		}
		bool flag = CanMakeConnection();
		if (flag != lastCanConnect)
		{
			isDirty = true;
		}
		lastCanConnect = flag;
	}

	public bool CanMakeConnection()
	{
		if (!base.gameObject.activeInHierarchy || dontConnectToRelays || Mathf.Approximately(GetMaxPower(), 0f) || IsUnderConstruction())
		{
			return false;
		}
		return true;
	}

	private IEnumerator UpdateConnectionAsync()
	{
		YieldInstruction delayForOneSecond = new WaitForSeconds(1f);
		while (true)
		{
			if (IsUnderConstruction())
			{
				yield return CoroutineUtils.waitForNextFrame;
				continue;
			}
			if (!CanMakeConnection())
			{
				yield return delayForOneSecond;
				continue;
			}
			PowerRelay relayToConnectTo = null;
			float curMaxDistSq = maxOutboundDistance * maxOutboundDistance;
			curFindRelayIndex = 0;
			int numChecksPerFrame = 10;
			while (!FindNearestValidRelayAsync(includeGhostModels: false, ref curMaxDistSq, ref relayToConnectTo))
			{
				if (numChecksPerFrame-- <= 0)
				{
					numChecksPerFrame = 10;
					yield return CoroutineUtils.waitForNextFrame;
				}
			}
			if (!(relayToConnectTo != null) || IsValidRelayForConnection(relayToConnectTo, includeGhostModels: false))
			{
				TryConnectToRelay(relayToConnectTo);
				isDirty = false;
				while (!isDirty)
				{
					yield return CoroutineUtils.waitForNextFrame;
				}
			}
		}
	}

	private bool TryConnectToRelay(PowerRelay relay)
	{
		if (relay != null)
		{
			if (relay != outboundRelay)
			{
				DisconnectFromRelay();
				outboundRelay = relay;
				outboundRelay.AddInboundPower(this);
				outboundRelay.isDirty = true;
				if ((bool)powerFX)
				{
					powerFX.SetTarget(outboundRelay.gameObject);
				}
			}
			return true;
		}
		DisconnectFromRelay();
		return false;
	}

	public bool UpdateConnection()
	{
		if (IsUnderConstruction())
		{
			return false;
		}
		PowerRelay relay = FindRelayToConnectTo();
		return TryConnectToRelay(relay);
	}

	private void DisconnectFromRelay()
	{
		if (outboundRelay != null)
		{
			outboundRelay.RemoveInboundPower(this);
			outboundRelay = null;
			isDirty = true;
		}
		if ((bool)powerFX)
		{
			powerFX.SetTarget(null);
		}
	}

	public bool IsPowered()
	{
		return isPowered;
	}

	public PowerSystem.Status GetPowerStatus()
	{
		return powerStatus;
	}

	private void OnDestroy()
	{
		UnregisterRelay(this);
	}

	public void DisableElectronicsForTime(float time)
	{
		enableElectonicsTime = Mathf.Max(enableElectonicsTime, Time.time + time);
		electronicsDisabled = true;
	}
}
