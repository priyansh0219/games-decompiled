using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class uGUI_ItemsContainerView : MonoBehaviour, uGUI_IIconManager
{
	public RectTransform rectTransform;

	public RawImage grid;

	private Dictionary<uGUI_ItemIcon, InventoryItem> icons = new Dictionary<uGUI_ItemIcon, InventoryItem>();

	private Dictionary<InventoryItem, uGUI_ItemIcon> items = new Dictionary<InventoryItem, uGUI_ItemIcon>();

	private ItemsContainer container;

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
		foreach (KeyValuePair<InventoryItem, uGUI_ItemIcon> item in items)
		{
			Object.Destroy(item.Value.gameObject);
		}
		items.Clear();
		icons.Clear();
		base.gameObject.SetActive(value: false);
	}

	private void OnAddItem(InventoryItem item)
	{
		TechType techType = item.item.GetTechType();
		SpriteManager.GetBackground(techType);
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
		uGUI_ItemsContainer.GetIconSize(itemSize.x, itemSize.y, out var width, out var height);
		uGUI_ItemsContainer.GetIconPosition(item.x, item.y, out var posX, out var posY);
		uGUI_ItemIcon2.SetSize(width, height);
		uGUI_ItemIcon2.SetPosition(posX, -posY);
		icons.Add(uGUI_ItemIcon2, item);
		items.Add(item, uGUI_ItemIcon2);
		if (item.x < 0 || item.y < 0)
		{
			gameObject.SetActive(value: false);
		}
	}

	private void OnRemoveItem(InventoryItem item)
	{
		if (items.TryGetValue(item, out var value))
		{
			items.Remove(item);
			icons.Remove(value);
			if (value != null)
			{
				Object.Destroy(value.gameObject);
				value = null;
			}
		}
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
			uGUI_ItemsContainer.GetIconPosition(item.x, item.y, out var posX, out var posY);
			value.SetPosition(posX, -posY);
		}
	}

	private void OnResize(int width, int height)
	{
		if (rectTransform != null)
		{
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * 71);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * 71);
		}
		if (grid != null)
		{
			grid.uvRect = new Rect(0f, 0f, width, height);
		}
	}

	public void GetTooltip(uGUI_ItemIcon icon, TooltipData data)
	{
		if (icons.TryGetValue(icon, out var value))
		{
			TooltipFactory.InventoryItemView(value, data);
		}
	}

	public bool OnPointerClick(uGUI_ItemIcon icon, int button)
	{
		return false;
	}

	public void OnPointerEnter(uGUI_ItemIcon icon)
	{
	}

	public void OnPointerExit(uGUI_ItemIcon icon)
	{
	}

	public bool OnBeginDrag(uGUI_ItemIcon icon)
	{
		return false;
	}

	public void OnEndDrag(uGUI_ItemIcon icon)
	{
	}

	public void OnDrop(uGUI_ItemIcon icon)
	{
	}

	public void OnDragHoverEnter(uGUI_ItemIcon icon)
	{
	}

	public void OnDragHoverStay(uGUI_ItemIcon icon)
	{
	}

	public void OnDragHoverExit(uGUI_ItemIcon icon)
	{
	}

	public bool OnButtonDown(uGUI_ItemIcon icon, GameInput.Button button)
	{
		return false;
	}
}
