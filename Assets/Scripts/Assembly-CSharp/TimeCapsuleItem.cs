using System;
using System.Collections;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class TimeCapsuleItem
{
	[NonSerialized]
	[ProtoMember(1)]
	public TechType techType;

	[NonSerialized]
	[ProtoMember(2)]
	public bool hasBattery;

	[NonSerialized]
	[ProtoMember(3)]
	public TechType batteryType;

	[NonSerialized]
	[ProtoMember(4)]
	public float batteryCharge = -1f;

	public bool IsValid()
	{
		return true & (techType != TechType.None);
	}

	public IEnumerator SpawnAsync(IOut<Pickupable> result)
	{
		TaskResult<GameObject> prefabResult = new TaskResult<GameObject>();
		yield return CraftData.InstantiateFromPrefabAsync(techType, prefabResult);
		GameObject gameObject = prefabResult.Get();
		if (gameObject == null)
		{
			yield break;
		}
		Pickupable pickupable = gameObject.GetComponent<Pickupable>();
		if (pickupable != null)
		{
			if (hasBattery)
			{
				EnergyMixin component = gameObject.GetComponent<EnergyMixin>();
				if (component != null)
				{
					yield return component.SetBatteryAsync(batteryType, batteryCharge, DiscardTaskResult<InventoryItem>.Instance);
				}
				else
				{
					Debug.LogErrorFormat("Time Capsule item deserialization error - deserialized item TechType.{0} is supposed to have battery but EnergyMixin component was not found on spawned GameObject", techType.AsString());
				}
			}
		}
		else
		{
			UnityEngine.Object.Destroy(gameObject);
			Debug.LogErrorFormat("Time Capsule item deserialization failed - Pickupable component missing on spawned prefab for TechType.{0}", techType.AsString());
		}
		result.Set(pickupable);
	}
}
