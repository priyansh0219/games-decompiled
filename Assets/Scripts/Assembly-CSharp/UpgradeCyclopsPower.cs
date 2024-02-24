using System.Collections;
using UnityEngine;

public class UpgradeCyclopsPower : MonoBehaviour
{
	private IEnumerator Start()
	{
		PowerSource powerSource = GetComponent<PowerSource>();
		if (!(powerSource != null))
		{
			yield break;
		}
		BatterySource component = GetComponent<BatterySource>();
		bool destroySelf = false;
		if (component != null)
		{
			TaskResult<bool> result = new TaskResult<bool>();
			yield return component.SpawnDefaultAsync(powerSource.power / powerSource.maxPower, result);
			if (result.Get())
			{
				destroySelf = true;
			}
		}
		Object.Destroy(powerSource);
		if (destroySelf)
		{
			Object.Destroy(this);
		}
	}
}
