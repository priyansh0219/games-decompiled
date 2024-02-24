using System.Collections.Generic;

public interface IItemsContainer
{
	string label { get; }

	event OnAddItem onAddItem;

	event OnRemoveItem onRemoveItem;

	bool AddItem(InventoryItem item);

	bool RemoveItem(InventoryItem item, bool forced, bool verbose);

	bool AllowedToAdd(Pickupable pickupable, bool verbose);

	bool AllowedToRemove(Pickupable pickupable, bool verbose);

	void UpdateContainer();

	bool HasRoomFor(Pickupable pickupable, InventoryItem ignore);

	IEnumerator<InventoryItem> GetEnumerator();
}
