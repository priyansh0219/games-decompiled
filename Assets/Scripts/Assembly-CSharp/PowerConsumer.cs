using UnityEngine;

public class PowerConsumer : MonoBehaviour
{
	private float _consumed;

	private PowerRelay powerRelay;

	private Base baseComp;

	private float consumptionRate;

	private Vector3 cellPosition;

	private void Start()
	{
		powerRelay = base.gameObject.GetComponentInParent<PowerRelay>();
		baseComp = base.gameObject.GetComponentInParent<Base>();
		if ((bool)baseComp)
		{
			baseComp.GetClosestCell(base.transform.position, out var _, out cellPosition, out var _);
		}
	}

	public void ConsumePower(float powerToConsume, out float consumed)
	{
		consumed = 0f;
		if ((bool)powerRelay)
		{
			powerRelay.ConsumeEnergy(powerToConsume, out consumed);
			_consumed += consumed;
		}
	}

	public Base GetBaseComp()
	{
		return baseComp;
	}

	public bool HasPower(float power)
	{
		if (powerRelay != null)
		{
			return powerRelay.GetPower() >= power;
		}
		return false;
	}

	public bool IsPowered()
	{
		if (powerRelay != null)
		{
			return powerRelay.IsPowered();
		}
		return false;
	}

	public float GetConsumptionRate()
	{
		return consumptionRate;
	}

	public float PollPowerConsumption()
	{
		consumptionRate = _consumed;
		_consumed = 0f;
		return consumptionRate;
	}
}
