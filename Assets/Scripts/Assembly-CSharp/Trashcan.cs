using System.Collections.Generic;
using UnityEngine;

public class Trashcan : MonoBehaviour
{
	private class Waste
	{
		public InventoryItem inventoryItem;

		public double timeAdded;

		public Waste(InventoryItem inventoryItem, double timeAdded)
		{
			this.inventoryItem = inventoryItem;
			this.timeAdded = timeAdded;
		}
	}

	[AssertNotNull]
	public StorageContainer storageContainer;

	public float startDestroyTimeOut = 5f;

	public float destroyInterval = 1f;

	public bool biohazard;

	private static readonly List<TechType> nuclearWaste = new List<TechType>
	{
		TechType.ReactorRod,
		TechType.DepletedReactorRod
	};

	private bool subscribed;

	private List<Waste> wasteList = new List<Waste>();

	private double timeLastWasteDestroyed;

	[AssertLocalization]
	private const string notNuclearWaterMessage = "TrashcanErrorNotNuclearWaste";

	[AssertLocalization]
	private const string nuclearWasterMessage = "TrashcanErrorNuclearWaste";

	private void OnEnable()
	{
		if (!subscribed)
		{
			storageContainer.enabled = true;
			storageContainer.container.containerType = ItemsContainerType.Trashcan;
			storageContainer.container.onAddItem += AddItem;
			storageContainer.container.onRemoveItem += RemoveItem;
			storageContainer.container.isAllowedToAdd = IsAllowedToAdd;
			subscribed = true;
		}
	}

	private void OnDisable()
	{
		if (subscribed)
		{
			storageContainer.container.onAddItem -= AddItem;
			storageContainer.container.onRemoveItem -= RemoveItem;
			storageContainer.container.isAllowedToAdd = null;
			subscribed = false;
			storageContainer.enabled = false;
		}
	}

	private void Update()
	{
		if (wasteList.Count <= 0 || !(timeLastWasteDestroyed + (double)destroyInterval < DayNightCycle.main.timePassed))
		{
			return;
		}
		Waste waste = wasteList[0];
		if (ItemDragManager.isDragging && waste.inventoryItem == ItemDragManager.draggedItem)
		{
			waste = ((wasteList.Count > 1) ? wasteList[1] : null);
		}
		if (waste != null && waste.timeAdded + (double)startDestroyTimeOut < DayNightCycle.main.timePassed)
		{
			timeLastWasteDestroyed = DayNightCycle.main.timePassed;
			Pickupable item = waste.inventoryItem.item;
			if (storageContainer.container.RemoveItem(item, forced: true))
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
	{
		TechType techType = pickupable.GetTechType();
		bool flag = nuclearWaste.Contains(techType);
		if (biohazard == flag)
		{
			return true;
		}
		if (verbose)
		{
			string key = (biohazard ? "TrashcanErrorNotNuclearWaste" : "TrashcanErrorNuclearWaste");
			ErrorMessage.AddMessage(Language.main.Get(key));
		}
		return false;
	}

	private void AddItem(InventoryItem item)
	{
		wasteList.Add(new Waste(item, DayNightCycle.main.timePassed));
	}

	private void RemoveItem(InventoryItem item)
	{
		for (int i = 0; i < wasteList.Count; i++)
		{
			if (wasteList[i].inventoryItem == item)
			{
				wasteList.RemoveAt(i);
				break;
			}
		}
	}
}
