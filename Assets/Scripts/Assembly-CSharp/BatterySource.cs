using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BatterySource : EnergyMixin, IPowerInterface
{
	private PowerRelay connectedRelay;

	protected override void Start()
	{
		base.Start();
		UpdateConnection();
		InvokeRepeating("UpdateConnectionCallback", Random.value, 1f);
	}

	private void UpdateConnectionCallback()
	{
		UpdateConnection();
	}

	public bool UpdateConnection()
	{
		PowerRelay powerRelay = PowerSource.FindRelay(base.transform);
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

	float IPowerInterface.GetPower()
	{
		return base.charge;
	}

	float IPowerInterface.GetMaxPower()
	{
		return base.capacity;
	}

	bool IPowerInterface.ModifyPower(float amount, out float modified)
	{
		bool flag = false;
		modified = 0f;
		if (amount >= 0f)
		{
			flag = amount <= base.capacity - base.charge;
			modified = ModifyCharge(amount);
		}
		else
		{
			flag = base.charge >= 0f - amount;
			if (GameModeUtils.RequiresPower())
			{
				modified = ModifyCharge(amount);
			}
			else
			{
				modified = amount;
			}
		}
		return flag;
	}

	bool IPowerInterface.HasInboundPower(IPowerInterface powerInterface)
	{
		return false;
	}

	public bool GetInboundHasSource(IPowerInterface powerInterface)
	{
		return false;
	}
}
