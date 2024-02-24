public interface IQuickSlots
{
	event QuickSlots.OnBind onBind;

	event QuickSlots.OnToggle onToggle;

	event QuickSlots.OnSelect onSelect;

	TechType[] GetSlotBinding();

	TechType GetSlotBinding(int slotID);

	InventoryItem GetSlotItem(int slotID);

	int GetSlotByItem(InventoryItem item);

	float GetSlotProgress(int slotID);

	float GetSlotCharge(int slotID);

	void SlotKeyDown(int slotID);

	void SlotKeyHeld(int slotID);

	void SlotKeyUp(int slotID);

	void SlotNext();

	void SlotPrevious();

	void SlotLeftDown();

	void SlotLeftHeld();

	void SlotLeftUp();

	void SlotRightDown();

	void SlotRightHeld();

	void SlotRightUp();

	void DeselectSlots();

	int GetActiveSlotID();

	bool IsToggled(int slotID);

	int GetSlotCount();

	void Bind(int slotID, InventoryItem item);

	void Unbind(int slotID);
}
