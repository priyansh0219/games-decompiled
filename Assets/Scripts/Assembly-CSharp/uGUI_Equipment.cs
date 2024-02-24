using System;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_Equipment : MonoBehaviour, uGUI_INavigableIconGrid
{
	private static readonly List<string> sSlotIDs = new List<string>();

	[HideInInspector]
	public uGUI_InventoryTab inventory;

	private GameObject go;

	private Dictionary<string, uGUI_EquipmentSlot> allSlots;

	private Equipment equipment;

	private Dictionary<uGUI_EquipmentSlot, InventoryItem> slots;

	private Dictionary<InventoryItem, uGUI_EquipmentSlot> items;

	private uGUI_EquipmentSlot selectedSlot
	{
		get
		{
			if (UISelection.HasSelection)
			{
				uGUI_EquipmentSlot uGUI_EquipmentSlot2 = UISelection.selected as uGUI_EquipmentSlot;
				if (uGUI_EquipmentSlot2 != null && slots.ContainsKey(uGUI_EquipmentSlot2))
				{
					return uGUI_EquipmentSlot2;
				}
			}
			return null;
		}
	}

	bool uGUI_INavigableIconGrid.ShowSelector => true;

	bool uGUI_INavigableIconGrid.EmulateRaycast => true;

	private void Awake()
	{
		go = base.gameObject;
		allSlots = new Dictionary<string, uGUI_EquipmentSlot>();
		uGUI_EquipmentSlot[] componentsInChildren = GetComponentsInChildren<uGUI_EquipmentSlot>(includeInactive: true);
		foreach (uGUI_EquipmentSlot uGUI_EquipmentSlot2 in componentsInChildren)
		{
			string slot = uGUI_EquipmentSlot2.slot;
			if (!string.IsNullOrEmpty(slot))
			{
				if (allSlots.ContainsKey(slot))
				{
					Debug.LogError("uGUI_EquipmentSlot with duplicate '" + slot + "' identifier found!", uGUI_EquipmentSlot2.gameObject);
					continue;
				}
				allSlots.Add(slot, uGUI_EquipmentSlot2);
				uGUI_EquipmentSlot2.manager = this;
				uGUI_EquipmentSlot2.SetActive(value: false);
			}
		}
		go.SetActive(value: false);
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	public void Init(Equipment equipment)
	{
		slots = new Dictionary<uGUI_EquipmentSlot, InventoryItem>();
		items = new Dictionary<InventoryItem, uGUI_EquipmentSlot>();
		this.equipment = equipment;
		Dictionary<string, InventoryItem>.Enumerator enumerator = equipment.GetEquipment();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, InventoryItem> current = enumerator.Current;
			string key = current.Key;
			InventoryItem value = current.Value;
			if (allSlots.TryGetValue(key, out var value2))
			{
				value2.SetActive(value: true);
				if (value != null)
				{
					OnEquip(key, value);
				}
			}
			else
			{
				Debug.Log("slot not found in pda screen" + key);
			}
		}
		ExtinguishSlots();
		equipment.onAddSlot += OnAddSlot;
		equipment.onEquip += OnEquip;
		equipment.onUnequip += OnUnequip;
		ItemDragManager.onItemDragStart = (ItemDragManager.OnItemDragStart)Delegate.Combine(ItemDragManager.onItemDragStart, new ItemDragManager.OnItemDragStart(OnItemDragStart));
		ItemDragManager.onItemDragStop = (ItemDragManager.OnItemDragStop)Delegate.Combine(ItemDragManager.onItemDragStop, new ItemDragManager.OnItemDragStop(OnItemDragStop));
		go.SetActive(value: true);
	}

	public void DoUpdate()
	{
		if (slots == null)
		{
			return;
		}
		foreach (KeyValuePair<uGUI_EquipmentSlot, InventoryItem> slot in slots)
		{
			slot.Key.icon.SetBarValue(TooltipFactory.GetBarValue(slot.Value));
		}
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	public void Uninit()
	{
		if (equipment != null)
		{
			DeselectItem();
			ItemDragManager.onItemDragStart = (ItemDragManager.OnItemDragStart)Delegate.Remove(ItemDragManager.onItemDragStart, new ItemDragManager.OnItemDragStart(OnItemDragStart));
			ItemDragManager.onItemDragStop = (ItemDragManager.OnItemDragStop)Delegate.Remove(ItemDragManager.onItemDragStop, new ItemDragManager.OnItemDragStop(OnItemDragStop));
			equipment.onAddSlot -= OnAddSlot;
			equipment.onEquip -= OnEquip;
			equipment.onUnequip -= OnUnequip;
			Dictionary<uGUI_EquipmentSlot, InventoryItem>.Enumerator enumerator = slots.GetEnumerator();
			while (enumerator.MoveNext())
			{
				uGUI_EquipmentSlot key = enumerator.Current.Key;
				NotificationManager.main.UnregisterTarget(key);
				key.ClearIcon();
			}
			Dictionary<string, uGUI_EquipmentSlot>.Enumerator enumerator2 = allSlots.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				enumerator2.Current.Value.SetActive(value: false);
			}
			slots = null;
			items = null;
			equipment = null;
			go.SetActive(value: false);
		}
	}

	private void OnItemDragStart(Pickupable p)
	{
		EquipmentType equipmentType = TechData.GetEquipmentType(p.GetTechType());
		HighlightSlots(equipmentType);
	}

	private void OnItemDragStop()
	{
		ExtinguishSlots();
	}

	public void HighlightSlots(EquipmentType itemType)
	{
		if (equipment == null)
		{
			return;
		}
		equipment.GetSlots(itemType, sSlotIDs);
		for (int i = 0; i < sSlotIDs.Count; i++)
		{
			string key = sSlotIDs[i];
			if (allSlots.TryGetValue(key, out var value))
			{
				value.MarkCompatible(state: true);
			}
		}
		sSlotIDs.Clear();
	}

	public void ExtinguishSlots()
	{
		Dictionary<string, uGUI_EquipmentSlot>.Enumerator enumerator = allSlots.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.MarkCompatible(state: false);
		}
	}

	private void OnAddSlot(string slot)
	{
		if (allSlots.TryGetValue(slot, out var value))
		{
			value.SetActive(value: true);
		}
	}

	private void OnEquip(string slot, InventoryItem item)
	{
		if (allSlots.TryGetValue(slot, out var value))
		{
			value.SetItem(item);
			NotificationManager.main.RegisterItemTarget(item, value);
			slots.Add(value, item);
			items.Add(item, value);
		}
	}

	private void OnUnequip(string slot, InventoryItem item)
	{
		if (!items.TryGetValue(item, out var value))
		{
			return;
		}
		bool flag = false;
		Vector3 worldPos = Vector3.zero;
		if (selectedSlot == value)
		{
			DeselectItem();
			RectTransform rectTransform = value.rectTransform;
			if (rectTransform != null)
			{
				worldPos = rectTransform.TransformPoint(rectTransform.rect.center);
				flag = true;
			}
		}
		items.Remove(item);
		slots.Remove(value);
		NotificationManager.main.UnregisterTarget(value);
		value.ClearIcon();
		inventory.ContainerItemRemoved(item);
		if (flag && !SelectSlotClosestToWorldPosition(worldPos))
		{
			GamepadInputModule.current.SetCurrentGrid(inventory.inventory);
		}
	}

	public void GetTooltip(uGUI_EquipmentSlot instance, TooltipData data)
	{
		if (slots.TryGetValue(instance, out var value))
		{
			inventory.GetTooltip(value, data);
		}
	}

	public void OnPointerEnter(uGUI_EquipmentSlot instance)
	{
		if (slots.TryGetValue(instance, out var value))
		{
			inventory.OnPointerEnter(value);
		}
	}

	public void OnPointerExit(uGUI_EquipmentSlot instance)
	{
		inventory.OnPointerExit();
	}

	public void OnPointerClick(uGUI_EquipmentSlot instance, int button)
	{
		if (slots.TryGetValue(instance, out var value))
		{
			inventory.OnPointerClick(value, button);
		}
	}

	public void OnBeginDrag(uGUI_EquipmentSlot instance)
	{
		if (slots.TryGetValue(instance, out var value))
		{
			int instanceID = instance.GetInstanceID();
			RectTransform iconRect = instance.iconRect;
			Rect rect = iconRect.rect;
			ItemDragManager.DragStart(value, instance.icon, instanceID, iconRect.TransformPoint(rect.center), iconRect.rotation, iconRect.lossyScale, rect.size, rect.size, instance.GetBackgroundRadius());
		}
	}

	public void OnEndDrag()
	{
		ItemDragManager.DragStop();
	}

	public ItemAction CanSwitchOrSwap(string slotB)
	{
		if (!ItemDragManager.isDragging)
		{
			return ItemAction.None;
		}
		InventoryItem draggedItem = ItemDragManager.draggedItem;
		if (draggedItem == null)
		{
			return ItemAction.None;
		}
		Pickupable item = draggedItem.item;
		if (item == null)
		{
			return ItemAction.None;
		}
		if (!Equipment.IsCompatible(TechData.GetEquipmentType(item.GetTechType()), Equipment.GetSlotType(slotB)))
		{
			return ItemAction.None;
		}
		InventoryItem itemInSlot = equipment.GetItemInSlot(slotB);
		if (itemInSlot != null)
		{
			if (Inventory.CanSwap(draggedItem, itemInSlot))
			{
				return ItemAction.Swap;
			}
			return ItemAction.None;
		}
		return ItemAction.Switch;
	}

	public void OnDrop(string slotB)
	{
		if (equipment != null)
		{
			InventoryItem draggedItem = ItemDragManager.draggedItem;
			ItemAction itemAction = CanSwitchOrSwap(slotB);
			ItemDragManager.DragStop();
			if (itemAction == ItemAction.Switch || itemAction == ItemAction.Swap)
			{
				Inventory.AddOrSwap(draggedItem, equipment, slotB);
			}
		}
	}

	public void OnDragHoverEnter(string slotB)
	{
	}

	public void OnDragHoverStay(string slotB)
	{
		if (ItemDragManager.isDragging)
		{
			allSlots.TryGetValue(slotB, out var value);
			switch (CanSwitchOrSwap(slotB))
			{
			case ItemAction.Switch:
			{
				RectTransform iconRect2 = value.iconRect;
				Rect rect2 = iconRect2.rect;
				ItemDragManager.DragSnap(value.GetInstanceID(), iconRect2.TransformPoint(rect2.center), iconRect2.rotation, iconRect2.lossyScale, rect2.size, rect2.size, value.GetBackgroundRadius());
				break;
			}
			case ItemAction.Swap:
			{
				RectTransform iconRect = value.iconRect;
				Rect rect = iconRect.rect;
				Vector2 draggedItemSize = ItemDragManager.draggedItemSize;
				ItemDragManager.DragSnap(value.GetInstanceID(), iconRect.TransformPoint(rect.center + new Vector2(draggedItemSize.x * 0.5f, (0f - draggedItemSize.y) * 0.5f)), iconRect.rotation, iconRect.lossyScale, draggedItemSize, draggedItemSize, 33f);
				value.icon.SetForegroundAlpha(0.3f);
				ItemDragManager.SetActionHint(ItemActionHint.Swap);
				break;
			}
			}
		}
	}

	public void OnDragHoverExit(string slotB)
	{
		if (!ItemDragManager.isDragging)
		{
			return;
		}
		InventoryItem itemInSlot = equipment.GetItemInSlot(slotB);
		if (itemInSlot == null)
		{
			return;
		}
		InventoryItem draggedItem = ItemDragManager.draggedItem;
		if (itemInSlot != draggedItem)
		{
			allSlots.TryGetValue(slotB, out var value);
			if (value != null)
			{
				value.icon.SetForegroundAlpha(1f);
			}
		}
	}

	public object GetSelectedItem()
	{
		return selectedSlot;
	}

	public Graphic GetSelectedIcon()
	{
		return selectedSlot;
	}

	public void SelectItem(object item)
	{
		DeselectItem();
		if (item != null)
		{
			uGUI_EquipmentSlot uGUI_EquipmentSlot2 = item as uGUI_EquipmentSlot;
			if (!(uGUI_EquipmentSlot2 == null) && slots.TryGetValue(uGUI_EquipmentSlot2, out var _))
			{
				UISelection.selected = uGUI_EquipmentSlot2;
				uGUI_EquipmentSlot2.SetSelected(state: true);
			}
		}
	}

	public void DeselectItem()
	{
		uGUI_EquipmentSlot uGUI_EquipmentSlot2 = selectedSlot;
		if (uGUI_EquipmentSlot2 != null)
		{
			uGUI_EquipmentSlot2.SetSelected(state: false);
		}
		UISelection.selected = null;
	}

	public bool SelectFirstItem()
	{
		Vector3 vector = new Vector3(float.MaxValue, float.MinValue, 0f);
		uGUI_EquipmentSlot uGUI_EquipmentSlot2 = null;
		Dictionary<uGUI_EquipmentSlot, InventoryItem>.Enumerator enumerator = slots.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<uGUI_EquipmentSlot, InventoryItem> current = enumerator.Current;
			if (current.Value == null)
			{
				continue;
			}
			uGUI_EquipmentSlot key = current.Key;
			RectTransform rectTransform = key.rectTransform;
			if (rectTransform == null)
			{
				continue;
			}
			Canvas canvas = key.canvas;
			if (canvas == null)
			{
				continue;
			}
			RectTransform component = canvas.GetComponent<RectTransform>();
			if (!(component == null))
			{
				Vector3 vector2 = component.InverseTransformPoint(rectTransform.TransformPoint(rectTransform.rect.center));
				if (vector2.x < vector.x || vector2.y > vector.y)
				{
					uGUI_EquipmentSlot2 = key;
					vector = vector2;
				}
			}
		}
		if (uGUI_EquipmentSlot2 != null)
		{
			SelectItem(uGUI_EquipmentSlot2);
			return true;
		}
		return false;
	}

	public bool SelectItemClosestToPosition(Vector3 worldPos)
	{
		return SelectSlotClosestToWorldPosition(worldPos);
	}

	public bool SelectItemInDirection(int dirX, int dirY)
	{
		uGUI_EquipmentSlot uGUI_EquipmentSlot2 = selectedSlot;
		if (uGUI_EquipmentSlot2 == null)
		{
			return SelectFirstItem();
		}
		if (dirX == 0 && dirY == 0)
		{
			return false;
		}
		Canvas canvas = uGUI_EquipmentSlot2.canvas;
		if (canvas == null)
		{
			return false;
		}
		RectTransform component = canvas.GetComponent<RectTransform>();
		UISelection.sSelectables.Clear();
		Dictionary<uGUI_EquipmentSlot, InventoryItem>.Enumerator enumerator = slots.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<uGUI_EquipmentSlot, InventoryItem> current = enumerator.Current;
			UISelection.sSelectables.Add(current.Key);
		}
		RectTransform rectTransform = uGUI_EquipmentSlot2.rectTransform;
		ISelectable selectable = UISelection.FindSelectable(rectTransform.TransformPoint(rectTransform.rect.center), component.TransformDirection(new Vector3(dirX, -dirY, 0f)), UISelection.sSelectables, UISelection.selected);
		UISelection.sSelectables.Clear();
		if (selectable != null)
		{
			SelectItem(selectable);
			return true;
		}
		return false;
	}

	public uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY)
	{
		return inventory.GetNavigableGridInDirection(this, dirX, dirY);
	}

	private bool SelectSlotClosestToWorldPosition(Vector3 worldPos)
	{
		Canvas componentInParent = GetComponentInParent<Canvas>();
		if (componentInParent == null)
		{
			return false;
		}
		RectTransform component = componentInParent.GetComponent<RectTransform>();
		if (component == null)
		{
			return false;
		}
		UISelection.sSelectables.Clear();
		Dictionary<uGUI_EquipmentSlot, InventoryItem>.Enumerator enumerator = slots.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<uGUI_EquipmentSlot, InventoryItem> current = enumerator.Current;
			if (current.Value != null)
			{
				uGUI_EquipmentSlot key = current.Key;
				UISelection.sSelectables.Add(key);
			}
		}
		ISelectable selectable = UISelection.FindSelectable(component, worldPos, UISelection.sSelectables);
		UISelection.sSelectables.Clear();
		if (selectable != null)
		{
			SelectItem(selectable);
			return true;
		}
		return false;
	}
}
