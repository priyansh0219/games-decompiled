using System.Collections.Generic;
using Gendarme;
using UnityEngine;

public class StorageSlot : IItemsContainer
{
	private string _label = "";

	private Transform root;

	public InventoryItem storedItem { get; private set; }

	public string label => _label;

	public event OnAddItem onAddItem;

	public event OnRemoveItem onRemoveItem;

	public StorageSlot(Transform newRoot, string label = "")
	{
		_label = label;
		root = newRoot;
	}

	public InventoryItem AddItem(Pickupable pickupable)
	{
		InventoryItem inventoryItem = new InventoryItem(pickupable);
		if (AddItem(inventoryItem))
		{
			return inventoryItem;
		}
		return null;
	}

	public void RemoveItem()
	{
		InventoryItem inventoryItem = storedItem;
		if (inventoryItem != null)
		{
			Pickupable item = inventoryItem.item;
			if (!item.isDestroyed)
			{
				item.GetComponent<Transform>().SetParent(null, worldPositionStays: true);
			}
			inventoryItem.container = null;
			storedItem = null;
			NotifyRemoveItem(inventoryItem);
		}
	}

	private void UnsafeAdd(InventoryItem item)
	{
		Pickupable item2 = item.item;
		if (root != null)
		{
			item2.GetComponent<Transform>().SetParent(root, worldPositionStays: false);
			item2.gameObject.SetActive(value: false);
		}
		item.container = this;
		storedItem = item;
		NotifyAddItem(item);
	}

	private void NotifyAddItem(InventoryItem item)
	{
		if (this.onAddItem != null)
		{
			this.onAddItem(item);
		}
	}

	private void NotifyRemoveItem(InventoryItem item)
	{
		if (this.onRemoveItem != null)
		{
			this.onRemoveItem(item);
		}
	}

	public IEnumerator<InventoryItem> GetEnumerator()
	{
		if (storedItem != null)
		{
			yield return storedItem;
		}
	}

	public bool AddItem(InventoryItem item)
	{
		if (item == null)
		{
			return false;
		}
		Pickupable item2 = item.item;
		if (!AllowedToAdd(item2, verbose: true) || !HasRoomFor(item2))
		{
			return false;
		}
		IItemsContainer container = item.container;
		if (container != null && !container.RemoveItem(item, forced: false, verbose: true))
		{
			return false;
		}
		UnsafeAdd(item);
		return true;
	}

	public bool RemoveItem(InventoryItem item, bool forced, bool verbose)
	{
		Pickupable item2 = item.item;
		if (!forced && !AllowedToRemove(item2, verbose))
		{
			return false;
		}
		RemoveItem();
		return true;
	}

	public bool AllowedToAdd(Pickupable pickupable, bool verbose)
	{
		if (storedItem == null)
		{
			return pickupable != null;
		}
		return false;
	}

	public bool AllowedToRemove(Pickupable pickupable, bool verbose)
	{
		if (storedItem != null)
		{
			return storedItem.item == pickupable;
		}
		return true;
	}

	[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidUnnecessarySpecializationRule")]
	public bool HasRoomFor(Pickupable pickupable, InventoryItem ignore = null)
	{
		if (pickupable == null)
		{
			return false;
		}
		if (storedItem != null)
		{
			if (ignore != null)
			{
				return ignore == storedItem;
			}
			return false;
		}
		return true;
	}

	public void UpdateContainer()
	{
	}

	public void Restore()
	{
		if (storedItem != null || !(root != null))
		{
			return;
		}
		int i = 0;
		for (int childCount = root.childCount; i < childCount; i++)
		{
			Pickupable component = root.GetChild(i).GetComponent<Pickupable>();
			if (component != null)
			{
				UnsafeAdd(new InventoryItem(component));
				break;
			}
		}
	}
}
