using System.Collections.Generic;
using Gendarme;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Correctness", "CheckParametersNullityInVisibleMethodsRule")]
public class Equipment : IItemsContainer
{
	public class EquipmentTypeComparer : IEqualityComparer<EquipmentType>
	{
		public bool Equals(EquipmentType x, EquipmentType y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(EquipmentType equipmentType)
		{
			return (int)equipmentType;
		}
	}

	public delegate void OnEquip(string slot, InventoryItem item);

	public delegate void OnUnequip(string slot, InventoryItem item);

	public delegate void OnAddSlot(string slot);

	public delegate void OnRemoveSlot(string slot);

	public delegate bool DelegateGetCompatibleSlot(EquipmentType itemType, out string slot);

	public static readonly EquipmentTypeComparer sEquipmentTypeComparer = new EquipmentTypeComparer();

	private string _label = "";

	public static readonly Dictionary<string, EquipmentType> slotMapping = new Dictionary<string, EquipmentType>
	{
		{
			"Head",
			EquipmentType.Head
		},
		{
			"Body",
			EquipmentType.Body
		},
		{
			"Gloves",
			EquipmentType.Gloves
		},
		{
			"Foots",
			EquipmentType.Foots
		},
		{
			"Chip1",
			EquipmentType.Chip
		},
		{
			"Chip2",
			EquipmentType.Chip
		},
		{
			"Tank",
			EquipmentType.Tank
		},
		{
			"Module1",
			EquipmentType.CyclopsModule
		},
		{
			"Module2",
			EquipmentType.CyclopsModule
		},
		{
			"Module3",
			EquipmentType.CyclopsModule
		},
		{
			"Module4",
			EquipmentType.CyclopsModule
		},
		{
			"Module5",
			EquipmentType.CyclopsModule
		},
		{
			"Module6",
			EquipmentType.CyclopsModule
		},
		{
			"SeamothModule1",
			EquipmentType.SeamothModule
		},
		{
			"SeamothModule2",
			EquipmentType.SeamothModule
		},
		{
			"SeamothModule3",
			EquipmentType.SeamothModule
		},
		{
			"SeamothModule4",
			EquipmentType.SeamothModule
		},
		{
			"ExosuitModule1",
			EquipmentType.ExosuitModule
		},
		{
			"ExosuitModule2",
			EquipmentType.ExosuitModule
		},
		{
			"ExosuitModule3",
			EquipmentType.ExosuitModule
		},
		{
			"ExosuitModule4",
			EquipmentType.ExosuitModule
		},
		{
			"ExosuitArmLeft",
			EquipmentType.ExosuitArm
		},
		{
			"ExosuitArmRight",
			EquipmentType.ExosuitArm
		},
		{
			"NuclearReactor1",
			EquipmentType.NuclearReactor
		},
		{
			"NuclearReactor2",
			EquipmentType.NuclearReactor
		},
		{
			"NuclearReactor3",
			EquipmentType.NuclearReactor
		},
		{
			"NuclearReactor4",
			EquipmentType.NuclearReactor
		},
		{
			"BatteryCharger1",
			EquipmentType.BatteryCharger
		},
		{
			"BatteryCharger2",
			EquipmentType.BatteryCharger
		},
		{
			"BatteryCharger3",
			EquipmentType.BatteryCharger
		},
		{
			"BatteryCharger4",
			EquipmentType.BatteryCharger
		},
		{
			"PowerCellCharger1",
			EquipmentType.PowerCellCharger
		},
		{
			"PowerCellCharger2",
			EquipmentType.PowerCellCharger
		},
		{
			"DecoySlot1",
			EquipmentType.DecoySlot
		},
		{
			"DecoySlot2",
			EquipmentType.DecoySlot
		},
		{
			"DecoySlot3",
			EquipmentType.DecoySlot
		},
		{
			"DecoySlot4",
			EquipmentType.DecoySlot
		},
		{
			"DecoySlot5",
			EquipmentType.DecoySlot
		}
	};

	public IsAllowedToAdd isAllowedToAdd;

	public IsAllowedToRemove isAllowedToRemove;

	public DelegateGetCompatibleSlot compatibleSlotDelegate;

	private Dictionary<string, InventoryItem> equipment;

	private Dictionary<TechType, int> equippedCount;

	private Dictionary<EquipmentType, List<string>> typeToSlots;

	private static List<IEquippable> equippables = new List<IEquippable>();

	private static readonly List<string> sSlots = new List<string>();

	string IItemsContainer.label => _label;

	public Transform tr { get; private set; }

	public GameObject owner { get; private set; }

	public event OnAddItem onAddItem;

	public event OnRemoveItem onRemoveItem;

	public event OnEquip onEquip;

	public event OnUnequip onUnequip;

	public event OnAddSlot onAddSlot;

	public event OnRemoveSlot onRemoveSlot;

	public static EquipmentType GetSlotType(string slot)
	{
		if (slotMapping.TryGetValue(slot, out var value))
		{
			return value;
		}
		return EquipmentType.None;
	}

	public Equipment(GameObject owner, Transform tr)
	{
		this.tr = tr;
		this.owner = owner;
		equipment = new Dictionary<string, InventoryItem>();
		typeToSlots = new Dictionary<EquipmentType, List<string>>(sEquipmentTypeComparer);
		equippedCount = new Dictionary<TechType, int>(TechTypeExtensions.sTechTypeComparer);
	}

	public void SetLabel(string l)
	{
		_label = l;
	}

	public void GetSlots(EquipmentType itemType, List<string> results)
	{
		results.Clear();
		if (typeToSlots == null)
		{
			return;
		}
		Dictionary<EquipmentType, List<string>>.Enumerator enumerator = typeToSlots.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<EquipmentType, List<string>> current = enumerator.Current;
			if (IsCompatible(itemType, current.Key))
			{
				results.AddRange(current.Value);
			}
		}
	}

	public TechType GetTechTypeInSlot(string slot)
	{
		InventoryItem itemInSlot = GetItemInSlot(slot);
		if (itemInSlot != null)
		{
			Pickupable item = itemInSlot.item;
			if (item != null)
			{
				return item.GetTechType();
			}
		}
		return TechType.None;
	}

	public InventoryItem GetItemInSlot(string slot)
	{
		if (equipment.TryGetValue(slot, out var value))
		{
			return value;
		}
		return null;
	}

	public Dictionary<string, InventoryItem>.Enumerator GetEquipment()
	{
		return equipment.GetEnumerator();
	}

	public bool GetCompatibleSlot(EquipmentType itemType, out string result)
	{
		if (compatibleSlotDelegate == null)
		{
			return GetCompatibleSlotDefault(itemType, out result);
		}
		return compatibleSlotDelegate(itemType, out result);
	}

	public bool GetCompatibleSlotDefault(EquipmentType itemType, out string result)
	{
		result = null;
		if (typeToSlots != null)
		{
			Dictionary<EquipmentType, List<string>>.Enumerator enumerator = typeToSlots.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<EquipmentType, List<string>> current = enumerator.Current;
				if (!IsCompatible(itemType, current.Key))
				{
					continue;
				}
				List<string> value = current.Value;
				int count = value.Count;
				for (int i = 0; i < count; i++)
				{
					string text = value[i];
					if (GetItemInSlot(text) == null)
					{
						result = text;
						return true;
					}
				}
				if (string.IsNullOrEmpty(result) && count > 0)
				{
					result = value[0];
				}
			}
		}
		return !string.IsNullOrEmpty(result);
	}

	public bool GetFreeSlot(EquipmentType type, out string result)
	{
		result = null;
		Dictionary<EquipmentType, List<string>>.Enumerator enumerator = typeToSlots.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<EquipmentType, List<string>> current = enumerator.Current;
			if (IsCompatible(type, current.Key))
			{
				sSlots.AddRange(current.Value);
			}
		}
		for (int i = 0; i < sSlots.Count; i++)
		{
			string text = sSlots[i];
			equipment.TryGetValue(text, out var value);
			if (value == null || value.ignoreForSorting)
			{
				result = text;
				break;
			}
		}
		sSlots.Clear();
		return result != null;
	}

	public bool GetItemSlot(Pickupable pickupable, ref string slot)
	{
		Dictionary<string, InventoryItem>.Enumerator enumerator = equipment.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, InventoryItem> current = enumerator.Current;
			InventoryItem value = current.Value;
			if (value != null)
			{
				Pickupable item = value.item;
				if (item != null && item == pickupable)
				{
					slot = current.Key;
					return true;
				}
			}
		}
		return false;
	}

	public bool GetItemSlot(InventoryItem item, ref string slot)
	{
		Dictionary<string, InventoryItem>.Enumerator enumerator = equipment.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, InventoryItem> current = enumerator.Current;
			if (item == current.Value)
			{
				slot = current.Key;
				return true;
			}
		}
		return false;
	}

	public bool AddSlot(string slot)
	{
		EquipmentType slotType = GetSlotType(slot);
		if (slotType == EquipmentType.None)
		{
			Debug.LogError("Equipment.AddSlot() - Failed attempt to add slot '" + slot + "'. Slot type is not defined in Equipment.slotMapping dictionary.");
			return false;
		}
		if (equipment.ContainsKey(slot))
		{
			return false;
		}
		equipment.Add(slot, null);
		if (typeToSlots.TryGetValue(slotType, out var value))
		{
			value.Add(slot);
		}
		else
		{
			value = new List<string> { slot };
			typeToSlots.Add(slotType, value);
		}
		NotifyAddSlot(slot);
		return true;
	}

	public void RemoveSlot(string slot)
	{
		if (!equipment.TryGetValue(slot, out var value))
		{
			return;
		}
		if (value != null)
		{
			Pickupable item = value.item;
			if (item != null)
			{
				Object.Destroy(item.gameObject);
			}
		}
		equipment.Remove(slot);
		EquipmentType slotType = GetSlotType(slot);
		if (typeToSlots.TryGetValue(slotType, out var value2))
		{
			value2.Remove(slot);
			if (value2.Count == 0)
			{
				typeToSlots.Remove(slotType);
			}
		}
		NotifyRemoveSlot(slot);
	}

	public void AddSlots(IEnumerable<string> slots)
	{
		foreach (string slot in slots)
		{
			AddSlot(slot);
		}
	}

	public void AddSlots(string[] slots)
	{
		for (int i = 0; i < slots.Length; i++)
		{
			AddSlot(slots[i]);
		}
	}

	IEnumerator<InventoryItem> IItemsContainer.GetEnumerator()
	{
		Dictionary<string, InventoryItem>.Enumerator e = equipment.GetEnumerator();
		while (e.MoveNext())
		{
			InventoryItem value = e.Current.Value;
			if (value != null)
			{
				yield return value;
			}
		}
	}

	bool IItemsContainer.AddItem(InventoryItem newItem)
	{
		EquipmentType equipmentType = TechData.GetEquipmentType(newItem.item.GetTechType());
		if (!GetFreeSlot(equipmentType, out var result))
		{
			return false;
		}
		return AddItem(result, newItem);
	}

	public bool AddItem(string slot, InventoryItem newItem, bool forced = false)
	{
		if (newItem == null)
		{
			return false;
		}
		if (!equipment.TryGetValue(slot, out var value))
		{
			return false;
		}
		if (value != null)
		{
			return false;
		}
		if (!forced && !AllowedToAdd(slot, newItem.item, verbose: true))
		{
			return false;
		}
		IItemsContainer container = newItem.container;
		if (container != null && !container.RemoveItem(newItem, forced: false, verbose: true))
		{
			return false;
		}
		newItem.container = this;
		newItem.item.Reparent(tr);
		equipment[slot] = newItem;
		TechType techType = newItem.item.GetTechType();
		UpdateCount(techType, increase: true);
		SendEquipmentEvent(newItem.item, 0, owner, slot);
		NotifyEquip(slot, newItem);
		return true;
	}

	bool IItemsContainer.RemoveItem(InventoryItem item, bool forced, bool verbose)
	{
		string slot = string.Empty;
		if (GetItemSlot(item, ref slot))
		{
			return RemoveItem(slot, forced, verbose) != null;
		}
		return false;
	}

	public bool AllowedToAdd(string slot, Pickupable pickupable, bool verbose)
	{
		EquipmentType equipmentType = TechData.GetEquipmentType(pickupable.GetTechType());
		EquipmentType slotType = GetSlotType(slot);
		if (!IsCompatible(equipmentType, slotType))
		{
			return false;
		}
		return ((IItemsContainer)this).AllowedToAdd(pickupable, verbose);
	}

	bool IItemsContainer.AllowedToAdd(Pickupable pickupable, bool verbose)
	{
		if (isAllowedToAdd != null)
		{
			return isAllowedToAdd(pickupable, verbose);
		}
		return true;
	}

	bool IItemsContainer.AllowedToRemove(Pickupable pickupable, bool verbose)
	{
		if (isAllowedToRemove != null)
		{
			return isAllowedToRemove(pickupable, verbose);
		}
		return true;
	}

	public bool RemoveItem(Pickupable pickupable)
	{
		if (pickupable == null)
		{
			return false;
		}
		string slot = string.Empty;
		if (GetItemSlot(pickupable, ref slot))
		{
			return RemoveItem(slot, forced: false, verbose: true) != null;
		}
		return false;
	}

	public InventoryItem RemoveItem(string slot, bool forced, bool verbose)
	{
		if (!equipment.TryGetValue(slot, out var value))
		{
			return null;
		}
		if (value == null)
		{
			return null;
		}
		if (!forced && !((IItemsContainer)this).AllowedToRemove(value.item, verbose))
		{
			return null;
		}
		equipment[slot] = null;
		TechType techType = value.item.GetTechType();
		UpdateCount(techType, increase: false);
		SendEquipmentEvent(value.item, 1, owner, slot);
		NotifyUnequip(slot, value);
		value.container = null;
		return value;
	}

	public void Clear()
	{
		List<string> list = new List<string>(equipment.Keys);
		for (int i = 0; i < list.Count; i++)
		{
			string slot = list[i];
			RemoveSlot(slot);
		}
	}

	public void ClearItems()
	{
		List<string> list = new List<string>(equipment.Keys);
		for (int i = 0; i < list.Count; i++)
		{
			InventoryItem inventoryItem = RemoveItem(list[i], forced: true, verbose: false);
			if (inventoryItem != null)
			{
				Pickupable item = inventoryItem.item;
				if (item != null)
				{
					Object.Destroy(item.gameObject);
				}
			}
		}
	}

	public bool SecureItems()
	{
		bool result = false;
		Dictionary<string, InventoryItem>.Enumerator enumerator = equipment.GetEnumerator();
		while (enumerator.MoveNext())
		{
			InventoryItem value = enumerator.Current.Value;
			if (value != null)
			{
				Pickupable item = value.item;
				if (item.destroyOnDeath)
				{
					result = true;
					item.destroyOnDeath = false;
				}
			}
		}
		return result;
	}

	void IItemsContainer.UpdateContainer()
	{
		Dictionary<string, InventoryItem>.Enumerator enumerator = equipment.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, InventoryItem> current = enumerator.Current;
			string key = current.Key;
			InventoryItem value = current.Value;
			if (value != null)
			{
				Pickupable item = value.item;
				if (!(item == null))
				{
					SendEquipmentEvent(item, 2, owner, key);
				}
			}
		}
	}

	bool IItemsContainer.HasRoomFor(Pickupable pickupable, InventoryItem ignore)
	{
		if (ignore != null)
		{
			ignore.ignoreForSorting = true;
		}
		EquipmentType equipmentType = TechData.GetEquipmentType(pickupable.GetTechType());
		string result;
		bool freeSlot = GetFreeSlot(equipmentType, out result);
		if (ignore != null)
		{
			ignore.ignoreForSorting = false;
		}
		return freeSlot;
	}

	public int GetCount(TechType techType)
	{
		equippedCount.TryGetValue(techType, out var value);
		return value;
	}

	public Dictionary<string, string> SaveEquipment()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (KeyValuePair<string, InventoryItem> item2 in equipment)
		{
			string key = item2.Key;
			InventoryItem value = item2.Value;
			string value2 = string.Empty;
			if (value != null)
			{
				Pickupable item = value.item;
				if (item != null)
				{
					UniqueIdentifier component = item.GetComponent<UniqueIdentifier>();
					if (component != null)
					{
						value2 = component.Id;
					}
				}
			}
			dictionary.Add(key, value2);
		}
		return dictionary;
	}

	public void RestoreEquipment(Dictionary<string, string> slots, Dictionary<string, InventoryItem> items)
	{
		Clear();
		foreach (KeyValuePair<string, string> slot in slots)
		{
			string key = slot.Key;
			string value = slot.Value;
			if (!AddSlot(key))
			{
				Debug.LogError("Equipment.RestoreEquipment() error - failed to add slot '" + key + "'");
			}
			else
			{
				if (string.IsNullOrEmpty(value))
				{
					continue;
				}
				if (!items.TryGetValue(value, out var value2))
				{
					Debug.LogError("Equipment.RestoreEquipment() error - failed to find serialized item with uid '" + value + "'");
					continue;
				}
				if (!AllowedToAdd(key, value2.item, verbose: false))
				{
					Debug.LogErrorFormat(value2.item, "Equipment.RestoreEquipment() warning - item {0} is no longer compatible with slot {1}, but still added to prevent data loss.", value2.item.GetTechType(), key);
				}
				AddItem(key, value2, forced: true);
			}
		}
	}

	private void NotifyEquip(string slot, InventoryItem item)
	{
		if (this.onEquip != null)
		{
			this.onEquip(slot, item);
		}
		if (this.onAddItem != null)
		{
			this.onAddItem(item);
		}
	}

	private void NotifyUnequip(string slot, InventoryItem item)
	{
		if (this.onUnequip != null)
		{
			this.onUnequip(slot, item);
		}
		if (this.onRemoveItem != null)
		{
			this.onRemoveItem(item);
		}
	}

	private void NotifyAddSlot(string slot)
	{
		if (this.onAddSlot != null)
		{
			this.onAddSlot(slot);
		}
	}

	private void NotifyRemoveSlot(string slot)
	{
		if (this.onRemoveSlot != null)
		{
			this.onRemoveSlot(slot);
		}
	}

	public static bool IsCompatible(EquipmentType itemType, EquipmentType slotType)
	{
		if (itemType == slotType)
		{
			return true;
		}
		if (itemType == EquipmentType.VehicleModule)
		{
			if (slotType != EquipmentType.SeamothModule)
			{
				return slotType == EquipmentType.ExosuitModule;
			}
			return true;
		}
		return false;
	}

	private void UpdateCount(TechType techType, bool increase)
	{
		if (equippedCount.TryGetValue(techType, out var value))
		{
			if (increase)
			{
				equippedCount[techType] = value + 1;
				return;
			}
			value--;
			if (value == 0)
			{
				equippedCount.Remove(techType);
			}
			else
			{
				equippedCount[techType] = value;
			}
		}
		else if (increase)
		{
			equippedCount.Add(techType, 1);
		}
	}

	public static void SendEquipmentEvent(Pickupable pickupable, int eventType, GameObject owner, string slot)
	{
		if (pickupable == null)
		{
			return;
		}
		pickupable.GetComponents(equippables);
		for (int i = 0; i < equippables.Count; i++)
		{
			IEquippable equippable = equippables[i];
			if (equippable != null)
			{
				switch (eventType)
				{
				case 0:
					equippable.OnEquip(owner, slot);
					break;
				case 1:
					equippable.OnUnequip(owner, slot);
					break;
				case 2:
					equippable.UpdateEquipped(owner, slot);
					break;
				}
			}
		}
		equippables.Clear();
	}
}
