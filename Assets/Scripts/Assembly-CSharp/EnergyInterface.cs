using UnityEngine;

public class EnergyInterface : MonoBehaviour
{
	public EnergyMixin[] sources;

	public bool hasCharge
	{
		get
		{
			if (!GameModeUtils.RequiresPower())
			{
				return true;
			}
			for (int i = 0; i < sources.Length; i++)
			{
				EnergyMixin energyMixin = sources[i];
				if (energyMixin != null && energyMixin.charge > 0f)
				{
					return true;
				}
			}
			return false;
		}
	}

	public float TotalCanProvide(out int sourceCount)
	{
		float num = 0f;
		sourceCount = 0;
		for (int i = 0; i < sources.Length; i++)
		{
			EnergyMixin energyMixin = sources[i];
			if (energyMixin != null && energyMixin.charge > 0f)
			{
				num += energyMixin.charge;
				sourceCount++;
			}
		}
		return num;
	}

	public float TotalCanConsume(out int sourceCount)
	{
		float num = 0f;
		sourceCount = 0;
		for (int i = 0; i < sources.Length; i++)
		{
			EnergyMixin energyMixin = sources[i];
			if (energyMixin != null && energyMixin.charge < energyMixin.capacity)
			{
				num += energyMixin.capacity - energyMixin.charge;
				sourceCount++;
			}
		}
		return num;
	}

	public float AddEnergy(float amount)
	{
		return ModifyCharge(Mathf.Abs(amount));
	}

	public float ConsumeEnergy(float amount)
	{
		return 0f - ModifyCharge(0f - Mathf.Abs(amount));
	}

	public float ModifyCharge(float amount)
	{
		float num = 0f;
		if (GameModeUtils.RequiresPower())
		{
			int sourceCount = 0;
			if (amount > 0f)
			{
				if (TotalCanConsume(out sourceCount) > 0f)
				{
					float amount2 = amount / (float)sourceCount;
					for (int i = 0; i < sources.Length; i++)
					{
						EnergyMixin energyMixin = sources[i];
						if (energyMixin != null && energyMixin.charge < energyMixin.capacity)
						{
							num += energyMixin.ModifyCharge(amount2);
						}
					}
				}
			}
			else
			{
				float num2 = TotalCanProvide(out sourceCount);
				if (sourceCount > 0)
				{
					amount = ((0f - amount > num2) ? (0f - num2) : amount);
					for (int j = 0; j < sources.Length; j++)
					{
						EnergyMixin energyMixin2 = sources[j];
						if (energyMixin2 != null && energyMixin2.charge > 0f)
						{
							float num3 = energyMixin2.charge / num2;
							num += energyMixin2.ModifyCharge(amount * num3);
						}
					}
				}
			}
		}
		return num;
	}

	public void GetValues(out float charge, out float capacity)
	{
		int num = 0;
		charge = 0f;
		capacity = 0f;
		for (int i = 0; i < sources.Length; i++)
		{
			EnergyMixin energyMixin = sources[i];
			if (energyMixin != null)
			{
				num++;
				charge += energyMixin.charge;
				capacity += energyMixin.capacity;
			}
		}
	}

	public void DisableElectronicsForTime(float time)
	{
		for (int i = 0; i < sources.Length; i++)
		{
			EnergyMixin energyMixin = sources[i];
			if (energyMixin != null)
			{
				energyMixin.DisableElectronicsForTime(time);
			}
		}
	}
}
