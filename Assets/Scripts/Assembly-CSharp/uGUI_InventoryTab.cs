using System.Collections.Generic;
using Gendarme;
using TMPro;
using UnityEngine;

public class uGUI_InventoryTab : uGUI_PDATab
{
	private const string defaultInventoryLabelKey = "InventoryLabel";

	private const string defaultStorageLabelKey = "StorageLabel";

	[AssertNotNull]
	public CanvasGroup content;

	public uGUI_ItemsContainer inventory;

	public uGUI_ItemsContainer storage;

	public uGUI_ItemsContainer[] torpedoStorage;

	public uGUI_Equipment equipment;

	[AssertNotNull]
	public TextMeshProUGUI inventoryLabel;

	[AssertNotNull]
	public TextMeshProUGUI storageLabel;

	private List<uGUI_INavigableIconGrid> usedStorageGrids = new List<uGUI_INavigableIconGrid>();

	private string storageLabelKey = "StorageLabel";

	public override int notificationsCount => NotificationManager.main.GetCount(NotificationManager.Group.Inventory);

	protected override void Awake()
	{
		inventory.inventory = this;
		storage.inventory = this;
		for (int i = 0; i < torpedoStorage.Length; i++)
		{
			uGUI_ItemsContainer uGUI_ItemsContainer2 = torpedoStorage[i];
			if (uGUI_ItemsContainer2 != null)
			{
				uGUI_ItemsContainer2.inventory = this;
			}
		}
		equipment.inventory = this;
	}

	private void Start()
	{
		Inventory main = Inventory.main;
		inventory.Init(main.container);
	}

	public override void OnOpenPDA(PDATab tab, bool explicitly)
	{
		Inventory main = Inventory.main;
		int usedStorageCount = main.GetUsedStorageCount();
		usedStorageGrids.Clear();
		storageLabelKey = null;
		if (usedStorageCount < 2)
		{
			IItemsContainer itemsContainer;
			if (usedStorageCount >= 1)
			{
				itemsContainer = main.GetUsedStorage(0);
			}
			else
			{
				IItemsContainer itemsContainer2 = Inventory.main.equipment;
				itemsContainer = itemsContainer2;
			}
			IItemsContainer itemsContainer3 = itemsContainer;
			if (itemsContainer3 != null)
			{
				storageLabelKey = itemsContainer3.label;
				if (itemsContainer3 is ItemsContainer)
				{
					storage.Init(itemsContainer3 as ItemsContainer);
					usedStorageGrids.Add(storage);
				}
				else if (itemsContainer3 is Equipment)
				{
					equipment.Init(itemsContainer3 as Equipment);
					usedStorageGrids.Add(equipment);
				}
			}
		}
		else
		{
			int num = Mathf.Min(torpedoStorage.Length, usedStorageCount);
			if (usedStorageGrids.Capacity < num)
			{
				usedStorageGrids.Capacity = num;
			}
			for (int i = 0; i < num; i++)
			{
				uGUI_ItemsContainer uGUI_ItemsContainer2 = torpedoStorage[i];
				IItemsContainer usedStorage = main.GetUsedStorage(i);
				usedStorageGrids.Add(uGUI_ItemsContainer2);
				if (uGUI_ItemsContainer2 != null && usedStorage != null)
				{
					if (string.IsNullOrEmpty(storageLabelKey))
					{
						storageLabelKey = usedStorage.label;
					}
					if (usedStorage is ItemsContainer)
					{
						uGUI_ItemsContainer2.Init(usedStorage as ItemsContainer);
					}
				}
			}
		}
		if (string.IsNullOrEmpty(storageLabelKey))
		{
			storageLabelKey = "StorageLabel";
		}
		UpdateStorageLabelText();
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	public override void OnClosePDA()
	{
		ItemDragManager.DragStop();
		ItemDragManager.hoveredItem = null;
		storage.Uninit();
		int i = 0;
		for (int num = torpedoStorage.Length; i < num; i++)
		{
			uGUI_ItemsContainer uGUI_ItemsContainer2 = torpedoStorage[i];
			if (uGUI_ItemsContainer2 != null)
			{
				uGUI_ItemsContainer2.Uninit();
			}
		}
		equipment.Uninit();
		Inventory.main.ClearUsedStorage();
		usedStorageGrids.Clear();
	}

	public override void Open()
	{
		content.SetVisible(visible: true);
	}

	public override void Close()
	{
		content.SetVisible(visible: false);
	}

	public override void OnUpdate(bool isOpen)
	{
		if (isOpen)
		{
			inventory.DoUpdate();
			storage.DoUpdate();
			equipment.DoUpdate();
			if (GameInput.GetButtonDown(GameInput.button2))
			{
				Inventory.main.ExecuteItemAction(ItemDragManager.hoveredItem, 2);
			}
		}
	}

	public override void OnLanguageChanged()
	{
		inventoryLabel.text = Language.main.Get("InventoryLabel");
		UpdateStorageLabelText();
	}

	public override bool OnButtonDown(GameInput.Button button)
	{
		if (Inventory.main.IsUsingStorage(PlayerTimeCapsule.main.container))
		{
			if (button == GameInput.button1)
			{
				pda.OpenTab(PDATab.TimeCapsule);
				return true;
			}
			return false;
		}
		return base.OnButtonDown(button);
	}

	public void GetTooltip(InventoryItem item, TooltipData data)
	{
		TooltipFactory.InventoryItem(item, data);
	}

	public void OnPointerEnter(InventoryItem item)
	{
		if (!ItemDragManager.isDragging && Player.main.GetPDA().isInUse)
		{
			ItemDragManager.hoveredItem = item;
			EquipmentType equipmentType = TechData.GetEquipmentType(item.item.GetTechType());
			equipment.HighlightSlots(equipmentType);
		}
	}

	public void OnPointerExit()
	{
		if (!ItemDragManager.isDragging)
		{
			ItemDragManager.hoveredItem = null;
			equipment.ExtinguishSlots();
		}
	}

	public void OnPointerClick(InventoryItem item, int button)
	{
		if (!ItemDragManager.isDragging)
		{
			bool num = Inventory.main.GetItemAction(item, button) != ItemAction.None;
			if (num)
			{
				Inventory.main.ExecuteItemAction(item, button);
			}
			equipment.ExtinguishSlots();
			if (!num && GameInput.PrimaryDevice == GameInput.Device.Controller && button == 1)
			{
				ClosePDA();
			}
		}
	}

	public void ContainerItemRemoved(InventoryItem item)
	{
		if (ItemDragManager.hoveredItem != null && ItemDragManager.hoveredItem == item)
		{
			ItemDragManager.hoveredItem = null;
		}
	}

	private void UpdateStorageLabelText()
	{
		storageLabel.text = Language.main.Get(storageLabelKey);
	}

	public override uGUI_INavigableIconGrid GetInitialGrid()
	{
		return inventory;
	}

	public uGUI_INavigableIconGrid GetNavigableGridInDirection(uGUI_INavigableIconGrid grid, int dirX, int dirY)
	{
		bool flag = false;
		uGUI_ItemsContainer[] array = torpedoStorage;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject.activeInHierarchy)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			uGUI_ItemsContainer uGUI_ItemsContainer2 = grid as uGUI_ItemsContainer;
			if (uGUI_ItemsContainer2 != null)
			{
				Vector3 vector = base.transform.TransformDirection(new Vector3(dirX, -dirY, 0f));
				Vector3 vector2 = Quaternion.Inverse(base.transform.rotation) * vector;
				Vector3 currentPos = uGUI_ItemsContainer2.transform.TransformPoint(GetPointOnRectEdge(uGUI_ItemsContainer2.transform as RectTransform, vector2));
				float num = GetMoveBetweenScore(uGUI_ItemsContainer2, currentPos, inventory, vector);
				uGUI_ItemsContainer result = inventory;
				array = torpedoStorage;
				foreach (uGUI_ItemsContainer uGUI_ItemsContainer3 in array)
				{
					float moveBetweenScore = GetMoveBetweenScore(uGUI_ItemsContainer2, currentPos, uGUI_ItemsContainer3, vector);
					if (moveBetweenScore > num)
					{
						result = uGUI_ItemsContainer3;
						num = moveBetweenScore;
					}
				}
				return result;
			}
		}
		if (grid == inventory)
		{
			if (dirX > 0 && dirY == 0 && usedStorageGrids.Count > 0)
			{
				return usedStorageGrids[0];
			}
		}
		else if (dirX < 0 && dirY == 0)
		{
			return inventory;
		}
		return null;
	}

	private float GetMoveBetweenScore(uGUI_ItemsContainer current, Vector3 currentPos, uGUI_ItemsContainer target, Vector3 dir)
	{
		if (current == target)
		{
			return 0f;
		}
		if (!target.gameObject.activeInHierarchy)
		{
			return 0f;
		}
		Vector3 position = target.rectTransform.rect.center;
		Vector3 rhs = target.transform.TransformPoint(position) - currentPos;
		float num = Vector3.Dot(dir, rhs);
		if (num <= 0f)
		{
			return float.NegativeInfinity;
		}
		return num / rhs.sqrMagnitude;
	}

	private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
	{
		if (rect == null)
		{
			return Vector3.zero;
		}
		if (dir != Vector2.zero)
		{
			dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
		}
		dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
		return dir;
	}
}
