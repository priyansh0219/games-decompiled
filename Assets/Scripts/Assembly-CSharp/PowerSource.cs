using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PowerSource : MonoBehaviour, IPowerInterface
{
	private const int currentVersion = 1;

	[ProtoMember(1)]
	public float power;

	[ProtoMember(2)]
	public float maxPower = 10000f;

	[NonSerialized]
	[ProtoMember(3)]
	public int version = 1;

	[NonSerialized]
	public PowerRelay connectedRelay;

	private void Start()
	{
		InvokeRepeating("UpdateConnectionCallback", UnityEngine.Random.value, 1f);
	}

	public bool IsConnected()
	{
		return connectedRelay != null;
	}

	private void OnDestroy()
	{
		if (connectedRelay != null)
		{
			connectedRelay.RemoveInboundPower(this);
		}
	}

	private void UpdateConnectionCallback()
	{
		UpdateConnection();
	}

	public static PowerRelay FindRelay(Transform transform)
	{
		if (transform.parent != null)
		{
			return Utils.FindAncestorWithComponent<PowerRelay>(transform.parent.gameObject);
		}
		return null;
	}

	public bool UpdateConnection()
	{
		PowerRelay powerRelay = FindRelay(base.transform);
		if (powerRelay != null && powerRelay != connectedRelay)
		{
			if (connectedRelay != null)
			{
				connectedRelay.RemoveInboundPower(this);
			}
			connectedRelay = powerRelay;
			connectedRelay.AddInboundPower(this);
		}
		return powerRelay != null;
	}

	public void SetPower(float newPower)
	{
		power = Mathf.Clamp(newPower, 0f, maxPower);
	}

	public float GetPower()
	{
		return power;
	}

	public float GetMaxPower()
	{
		return maxPower;
	}

	public bool ModifyPower(float amount, out float consumed)
	{
		bool flag = false;
		consumed = 0f;
		float num = power;
		flag = ((!(amount >= 0f)) ? (power >= 0f - amount) : (amount <= maxPower - power));
		if (GameModeUtils.RequiresPower())
		{
			power = Mathf.Clamp(power + amount, 0f, maxPower);
			consumed = power - num;
		}
		else
		{
			consumed = amount;
		}
		return flag;
	}

	public bool HasInboundPower(IPowerInterface powerInterface)
	{
		return false;
	}

	public bool GetInboundHasSource(IPowerInterface powerInterface)
	{
		return false;
	}
}
