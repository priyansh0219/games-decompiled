using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Stillsuit : MonoBehaviour, IEquippable
{
	public Eatable waterPrefab;

	private Pickupable pickupableWaterPrefab;

	[NonSerialized]
	[ProtoMember(1)]
	public float waterCaptured;

	private void Start()
	{
		pickupableWaterPrefab = waterPrefab.gameObject.GetComponent<Pickupable>();
	}

	void IEquippable.OnEquip(GameObject sender, string slot)
	{
	}

	void IEquippable.OnUnequip(GameObject sender, string slot)
	{
	}

	void IEquippable.UpdateEquipped(GameObject sender, string slot)
	{
		if (GameModeUtils.RequiresSurvival() && !Player.main.IsFrozenStats())
		{
			float num = Time.deltaTime / 1800f * 100f;
			waterCaptured += num * 0.75f;
			if (waterCaptured >= waterPrefab.waterValue)
			{
				Pickupable component = UnityEngine.Object.Instantiate(waterPrefab.gameObject).GetComponent<Pickupable>();
				Inventory.main.ForcePickup(component);
				waterCaptured -= waterPrefab.waterValue;
			}
		}
	}
}
