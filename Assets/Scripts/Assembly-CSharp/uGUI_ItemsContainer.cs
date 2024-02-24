using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class uGUI_ItemsContainer : MonoBehaviour, IDropHandler, IEventSystemHandler, uGUI_IIconManager, uGUI_INavigableIconGrid
{
	public const int CellWidth = 71;

	public const int CellHeight = 71;

	public const int IconWidth = 66;

	public const int IconHeight = 66;

	public const float IconRadius = 33f;

	public const int IconOffsetX = 1;

	public const int IconOffsetY = 1;

	[HideInInspector]
	public uGUI_InventoryTab inventory;

	[AssertNotNull]
	public RectTransform rectTransform;

	public RawImage grid;

	public Graphic background;

	private Dictionary<uGUI_ItemIcon, InventoryItem> icons = new Dictionary<uGUI_ItemIcon, InventoryItem>();

	private Dictionary<InventoryItem, uGUI_ItemIcon> items = new Dictionary<InventoryItem, uGUI_ItemIcon>();

	private ItemsContainer container;

	private bool selectLastItem;

	private Vector2 cachedItemPos = new Vector2(-1f, -1f);

	bool uGUI_INavigableIconGrid.ShowSelector => true;

	bool uGUI_INavigableIconGrid.EmulateRaycast => true;

	public static void GetIconSize(int cw, int ch, out int width, out int height)
	{
		width = cw * 66 + (cw - 1) * 5;
		height = ch * 66 + (ch - 1) * 5;
	}

	public static void GetIconPosition(int cx, int cy, out int posX, out int posY)
	{
		posX = cx * 71 + 1;
		posY = cy * 71 + 1;
	}

	private void Awake()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Init(ItemsContainer container)
	{
		Uninit();
		this.container = container;
		OnResize(container.sizeX, container.sizeY);
		foreach (InventoryItem item in (IEnumerable<InventoryItem>)container)
		{
			OnAddItem(item);
		}
		container.onAddItem += OnAddItem;
		container.onRemoveItem += OnRemoveItem;
		container.onChangeItemPosition += OnChangeItemPosition;
		container.onResize += OnResize;
		base.gameObject.SetActive(value: true);
	}

	public void DoUpdate()
	{
		foreach (KeyValuePair<uGUI_ItemIcon, InventoryItem> icon in icons)
		{
			icon.Key.SetBarValue(TooltipFactory.GetBarValue(icon.Value));
		}
	}

	public void Uninit()
	{
		if (container != null)
		{
			container.onAddItem -= OnAddItem;
			container.onRemoveItem -= OnRemoveItem;
			container.onChangeItemPosition -= OnChangeItemPosition;
			container.onResize -= OnResize;
			container = null;
		}
		DeselectItem();
		foreach (KeyValuePair<uGUI_ItemIcon, InventoryItem> icon in icons)
		{
			uGUI_ItemIcon key = icon.Key;
			NotificationManager.main.UnregisterTarget(key);
			Object.Destroy(key.gameObject);
		}
		icons.Clear();
		items.Clear();
		base.gameObject.SetActive(value: false);
	}

	private InventoryItem GetLastItem()
	{
		InventoryItem result = null;
		Vector2int vector2int = new Vector2int(int.MinValue, int.MinValue);
		foreach (KeyValuePair<InventoryItem, uGUI_ItemIcon> item in items)
		{
			InventoryItem key = item.Key;
			if (key != null)
			{
				int num = key.x + key.width - 1;
				int num2 = key.y + key.height - 1;
				if (num2 > vector2int.y || (num2 == vector2int.y && num > vector2int.x))
				{
					vector2int.x = num;
					vector2int.y = num2;
					result = key;
				}
			}
		}
		return result;
	}

	private void OnDrop(InventoryItem itemB)
	{
		if (container == null || !ItemDragManager.isDragging)
		{
			return;
		}
		InventoryItem draggedItem = ItemDragManager.draggedItem;
		ItemDragManager.DragStop();
		if (draggedItem != null)
		{
			IItemsContainer itemsContainer = draggedItem.container;
			IItemsContainer itemsContainer3;
			if (itemB == null)
			{
				IItemsContainer itemsContainer2 = container;
				itemsContainer3 = itemsContainer2;
			}
			else
			{
				itemsContainer3 = itemB.container;
			}
			IItemsContainer itemsContainer4 = itemsContainer3;
			if (itemsContainer4 != null && itemsContainer != itemsContainer4 && (itemB == null || !Inventory.AddOrSwap(draggedItem, itemB)))
			{
				Inventory.AddOrSwap(draggedItem, itemsContainer4);
			}
		}
	}

	private void OnAddItem(InventoryItem item)
	{
		TechType techType = item.item.GetTechType();
		GameObject gameObject = new GameObject("Item Icon");
		gameObject.layer = base.gameObject.layer;
		uGUI_ItemIcon uGUI_ItemIcon2 = gameObject.AddComponent<uGUI_ItemIcon>();
		uGUI_ItemIcon2.Init(this, rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f));
		uGUI_ItemIcon2.SetBackgroundSprite(SpriteManager.GetBackground(techType));
		float num = 33f;
		uGUI_ItemIcon2.SetBackgroundRadius(num);
		uGUI_ItemIcon2.SetBarRadius(num);
		uGUI_ItemIcon2.SetForegroundSprite(SpriteManager.Get(techType));
		uGUI_ItemIcon2.SetBarValue(TooltipFactory.GetBarValue(item));
		if (!item.isEnabled)
		{
			uGUI_ItemIcon2.SetChroma(0f);
		}
		Vector2int itemSize = TechData.GetItemSize(techType);
		GetIconSize(itemSize.x, itemSize.y, out var width, out var height);
		GetIconPosition(item.x, item.y, out var posX, out var posY);
		uGUI_ItemIcon2.SetSize(width, height);
		uGUI_ItemIcon2.SetPosition(posX, -posY);
		icons.Add(uGUI_ItemIcon2, item);
		items.Add(item, uGUI_ItemIcon2);
		NotificationManager.main.RegisterItemTarget(item, uGUI_ItemIcon2);
		if (item.x < 0 || item.y < 0)
		{
			gameObject.SetActive(value: false);
		}
	}

	private void OnRemoveItem(InventoryItem item)
	{
		if (!items.TryGetValue(item, out var value))
		{
			return;
		}
		if (value == UISelection.selected as uGUI_ItemIcon)
		{
			DeselectItem();
			if (item == GetLastItem())
			{
				selectLastItem = true;
			}
			else
			{
				cachedItemPos.x = (float)item.x + 0.5f * (float)item.width;
				cachedItemPos.y = (float)item.y + 0.5f * (float)item.height;
			}
		}
		items.Remove(item);
		icons.Remove(value);
		if (value != null)
		{
			NotificationManager.main.UnregisterTarget(value);
			Object.Destroy(value.gameObject);
			value = null;
		}
		inventory.ContainerItemRemoved(item);
	}

	private void OnChangeItemPosition(InventoryItem item)
	{
		if (items.TryGetValue(item, out var value))
		{
			GameObject gameObject = value.gameObject;
			if (!gameObject.activeSelf && item.x >= 0 && item.y >= 0)
			{
				gameObject.SetActive(value: true);
			}
			GetIconPosition(item.x, item.y, out var posX, out var posY);
			value.SetPosition(posX, -posY);
		}
	}

	private void OnResize(int width, int height)
	{
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * 71);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * 71);
		if (grid != null)
		{
			grid.uvRect = new Rect(0f, 0f, width, height);
		}
	}

	void IDropHandler.OnDrop(PointerEventData eventData)
	{
		OnDrop(null);
	}

	public void GetTooltip(uGUI_ItemIcon icon, TooltipData data)
	{
		if (icons.TryGetValue(icon, out var value))
		{
			inventory.GetTooltip(value, data);
		}
	}

	public void OnPointerEnter(uGUI_ItemIcon icon)
	{
		if (icons.TryGetValue(icon, out var value))
		{
			inventory.OnPointerEnter(value);
		}
	}

	public void OnPointerExit(uGUI_ItemIcon icon)
	{
		inventory.OnPointerExit();
	}

	public bool OnPointerClick(uGUI_ItemIcon icon, int button)
	{
		if (icons.TryGetValue(icon, out var value))
		{
			inventory.OnPointerClick(value, button);
		}
		return true;
	}

	public bool OnBeginDrag(uGUI_ItemIcon icon)
	{
		if (!icons.TryGetValue(icon, out var value))
		{
			return true;
		}
		int instanceID = icon.GetInstanceID();
		RectTransform rectTransform = icon.rectTransform;
		Rect rect = rectTransform.rect;
		Vector2int itemSize = TechData.GetItemSize(value.item.GetTechType());
		GetIconSize(itemSize.x, itemSize.y, out var width, out var height);
		Vector2 vector = new Vector2(width, height);
		ItemDragManager.DragStart(value, icon, instanceID, rectTransform.TransformPoint(rect.center), rectTransform.rotation, rectTransform.lossyScale, vector, vector, 33f);
		return true;
	}

	public void OnEndDrag(uGUI_ItemIcon icon)
	{
		ItemDragManager.DragStop();
	}

	void uGUI_IIconManager.OnDrop(uGUI_ItemIcon icon)
	{
		icons.TryGetValue(icon, out var value);
		OnDrop(value);
	}

	public void OnDragHoverEnter(uGUI_ItemIcon icon)
	{
	}

	public void OnDragHoverStay(uGUI_ItemIcon icon)
	{
		if (ItemDragManager.isDragging)
		{
			icons.TryGetValue(icon, out var value);
			if (Inventory.CanSwap(ItemDragManager.draggedItem, value))
			{
				icon.SetForegroundAlpha(0.3f);
				RectTransform rectTransform = icon.rectTransform;
				Rect rect = rectTransform.rect;
				Vector2 draggedItemSize = ItemDragManager.draggedItemSize;
				ItemDragManager.DragSnap(icon.GetInstanceID(), rectTransform.TransformPoint(rect.center + new Vector2(draggedItemSize.x * 0.5f, (0f - draggedItemSize.y) * 0.5f)), rectTransform.rotation, rectTransform.lossyScale, draggedItemSize, draggedItemSize, 33f);
				ItemDragManager.SetActionHint(ItemActionHint.Swap);
			}
		}
	}

	public void OnDragHoverExit(uGUI_ItemIcon icon)
	{
		if (ItemDragManager.isDragging)
		{
			icons.TryGetValue(icon, out var value);
			InventoryItem draggedItem = ItemDragManager.draggedItem;
			if (value != draggedItem)
			{
				icon.SetForegroundAlpha(1f);
			}
		}
	}

	public bool OnButtonDown(uGUI_ItemIcon icon, GameInput.Button button)
	{
		icons.TryGetValue(icon, out var value);
		if (button == GameInput.button2)
		{
			Inventory.main.ExecuteItemAction(value, 2);
			return true;
		}
		return false;
	}

	public uGUI_ItemIcon GetIcon(InventoryItem item)
	{
		items.TryGetValue(item, out var value);
		return value;
	}

	public object GetSelectedItem()
	{
		return UISelection.selected as uGUI_ItemIcon;
	}

	public Graphic GetSelectedIcon()
	{
		return UISelection.selected as uGUI_ItemIcon;
	}

	public void SelectItem(object item)
	{
		DeselectItem();
		uGUI_ItemIcon uGUI_ItemIcon2 = item as uGUI_ItemIcon;
		if (!(uGUI_ItemIcon2 == null) && icons.TryGetValue(uGUI_ItemIcon2, out var value))
		{
			UISelection.selected = uGUI_ItemIcon2;
			EquipmentType equipmentType = TechData.GetEquipmentType(value.item.GetTechType());
			inventory.equipment.HighlightSlots(equipmentType);
		}
	}

	public void DeselectItem()
	{
		if (UISelection.selected != null)
		{
			inventory.equipment.ExtinguishSlots();
			UISelection.selected = null;
		}
	}

	public bool SelectFirstItem()
	{
		uGUI_ItemIcon uGUI_ItemIcon2 = null;
		if (selectLastItem)
		{
			selectLastItem = false;
			InventoryItem lastItem = GetLastItem();
			if (lastItem != null && items.TryGetValue(lastItem, out var value))
			{
				uGUI_ItemIcon2 = value;
			}
		}
		else if (cachedItemPos.x >= 0f && cachedItemPos.y >= 0f)
		{
			Vector2 vector = cachedItemPos;
			cachedItemPos.x = -1f;
			cachedItemPos.y = -1f;
			Vector2int vector2int = new Vector2int(int.MaxValue, int.MaxValue);
			float num = float.MaxValue;
			foreach (KeyValuePair<InventoryItem, uGUI_ItemIcon> item in items)
			{
				InventoryItem key = item.Key;
				float num2 = (float)key.x + 0.5f * (float)key.width - vector.x;
				float num3 = (float)key.y + 0.5f * (float)key.height - vector.y;
				float num4 = num2 * num2 + num3 * num3;
				bool flag = false;
				if (num4 < num - 0.1f)
				{
					num = num4;
					flag = true;
				}
				else if (num4 < num + 0.1f && (key.x < vector2int.x || key.y < vector2int.y))
				{
					flag = true;
				}
				if (flag)
				{
					vector2int.x = key.x;
					vector2int.y = key.y;
					uGUI_ItemIcon2 = item.Value;
				}
			}
		}
		if (uGUI_ItemIcon2 == null && items != null)
		{
			foreach (KeyValuePair<InventoryItem, uGUI_ItemIcon> item2 in items)
			{
				InventoryItem key2 = item2.Key;
				if (key2.x == 0 && key2.y == 0)
				{
					uGUI_ItemIcon2 = item2.Value;
					break;
				}
			}
		}
		if (uGUI_ItemIcon2 != null)
		{
			SelectItem(uGUI_ItemIcon2);
			return true;
		}
		if (this != inventory.inventory)
		{
			GamepadInputModule.current.SetCurrentGrid(inventory.inventory);
		}
		return false;
	}

	public bool SelectItemClosestToPosition(Vector3 worldPos)
	{
		UISelection.sSelectables.Clear();
		foreach (KeyValuePair<InventoryItem, uGUI_ItemIcon> item in items)
		{
			uGUI_ItemIcon value = item.Value;
			UISelection.sSelectables.Add(value);
		}
		ISelectable selectable = UISelection.FindSelectable(rectTransform, worldPos, UISelection.sSelectables);
		UISelection.sSelectables.Clear();
		if (selectable != null)
		{
			SelectItem(selectable);
			return true;
		}
		return false;
	}

	public bool SelectItemInDirection(int dirX, int dirY)
	{
		if (UISelection.selected == null || !UISelection.selected.IsValid())
		{
			return SelectFirstItem();
		}
		if (dirX == 0 && dirY == 0)
		{
			return false;
		}
		if (!icons.TryGetValue(UISelection.selected as uGUI_ItemIcon, out var value))
		{
			return false;
		}
		ISelectable selectable = null;
		Vector2 vector = new Vector2(dirX, dirY);
		vector.Normalize();
		Vector2 pointOnRectEdge = RectTransformExtensions.GetPointOnRectEdge(new Rect(value.x, value.y, value.width, value.height), vector, 1f);
		Vector2int vector2int = new Vector2int(int.MaxValue, int.MaxValue);
		float num = float.NegativeInfinity;
		float num2 = float.PositiveInfinity;
		float num3 = 180f;
		foreach (KeyValuePair<InventoryItem, uGUI_ItemIcon> item in items)
		{
			InventoryItem key = item.Key;
			if (key == value)
			{
				continue;
			}
			Vector2 vector2 = new Rect((float)key.x + 0.05f, (float)key.y + 0.05f, (float)key.width - 0.1f, (float)key.height - 0.1f).ClosestPoint(pointOnRectEdge) - pointOnRectEdge;
			float magnitude = vector2.magnitude;
			Vector2 rhs = vector2 / magnitude;
			float num4 = Vector2.Dot(vector, rhs);
			if (!(num4 <= 0f))
			{
				float num5 = Mathf.Acos(num4) * 57.29578f;
				float num6 = (1f - num5 / 90f) / magnitude;
				if (!(num6 < num - 0.001f) && (!(Mathf.Abs(num2 - magnitude) < 0.1f) || !(Mathf.Abs(num3 - num5) < 2.5f) || (key.x <= vector2int.x && key.y <= vector2int.y)))
				{
					num = num6;
					num2 = magnitude;
					num3 = num5;
					vector2int.x = key.x;
					vector2int.y = key.y;
					selectable = item.Value;
				}
			}
		}
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
}
