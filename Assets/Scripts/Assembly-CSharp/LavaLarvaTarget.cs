using UnityEngine;

internal class LavaLarvaTarget : VehicleAccelerationModifier, ICompileTimeCheckable
{
	[AssertNotNull]
	public LavaLarvaAttachPoint[] attachPoints;

	public float distanceToStartAction = 10f;

	[AssertNotNull]
	public LiveMixin liveMixin;

	public EnergyInterface energyInterface;

	public Vehicle vehicle;

	public SubControl subControl;

	[AssertNotNull]
	public Transform larvaePrefabRoot;

	public float primiryPointsCount;

	public override void ModifyAcceleration(ref Vector3 acceleration)
	{
		acceleration *= Mathf.Lerp(1f, accelerationMultiplier, GetAtachedLarvaeCount() / (float)attachPoints.Length);
	}

	public bool GetAllowedToAttach()
	{
		if (vehicle != null && vehicle.docked)
		{
			return false;
		}
		if (!liveMixin.IsAlive() || liveMixin.shielded)
		{
			return false;
		}
		if (!HasCharge())
		{
			return false;
		}
		return true;
	}

	public LavaLarvaAttachPoint GetClosestAttachPoint(Vector3 position)
	{
		bool flag = false;
		float num = float.MaxValue;
		int num2 = -1;
		if (GetAllowedToAttach())
		{
			for (int i = 0; i < attachPoints.Length; i++)
			{
				if ((float)i < primiryPointsCount && attachPoints[i].occupied)
				{
					flag = true;
				}
				if (primiryPointsCount > 0f && (float)i > primiryPointsCount - 1f && !flag)
				{
					break;
				}
				if (!attachPoints[i].occupied)
				{
					float sqrMagnitude = (attachPoints[i].transform.position - position).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num2 = i;
						num = sqrMagnitude;
					}
				}
			}
		}
		if (num2 == -1)
		{
			return null;
		}
		return attachPoints[num2];
	}

	private float GetAtachedLarvaeCount()
	{
		int num = 0;
		LavaLarvaAttachPoint[] array = attachPoints;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].attached)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsCyclops()
	{
		return subControl != null;
	}

	public bool HasCharge()
	{
		if (energyInterface != null)
		{
			return energyInterface.hasCharge;
		}
		if (subControl != null)
		{
			return subControl.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline;
		}
		return false;
	}

	public float ConsumeEnergy(float amount)
	{
		float amountConsumed = 0f;
		if (energyInterface != null)
		{
			amountConsumed = energyInterface.ConsumeEnergy(amount);
		}
		else if (subControl != null)
		{
			subControl.powerRelay.ConsumeEnergy(amount, out amountConsumed);
		}
		return amountConsumed;
	}

	public string CompileTimeCheck()
	{
		if (subControl == null && vehicle == null)
		{
			return $"Either subControl or vehicle field must be set up on LavaLarvaTarget {base.name}";
		}
		if (vehicle != null && energyInterface == null)
		{
			return $"energyInterface field must be set up on LavaLarvaTarget {base.name} attached to Vehicle {vehicle.name}";
		}
		return null;
	}
}
