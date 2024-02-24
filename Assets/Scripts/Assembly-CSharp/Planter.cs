using System.Collections;
using System.Collections.Generic;
using UWE;
using UnityEngine;

public class Planter : MonoBehaviour, IProtoTreeEventListener
{
	private class PlantSlot
	{
		public readonly int id;

		public readonly Transform slot;

		public bool isOccupied;

		public Plantable plantable;

		public GameObject plantModel;

		public PlantSlot(int id, Transform slot)
		{
			this.id = id;
			this.slot = slot;
		}

		public void Clear()
		{
			plantable = null;
			plantModel = null;
		}

		public void SetMaxPlantHeight(float height)
		{
			if (!(height <= 0f) && !(plantable == null))
			{
				plantable.SetMaxPlantHeight(height - slot.localPosition.y);
			}
		}
	}

	public enum PlantEnvironment
	{
		Air = 0,
		Water = 1,
		Dynamic = 2
	}

	public PlantEnvironment environment;

	public bool isIndoor;

	public Constructable constructable;

	public StorageContainer storageContainer;

	public FMODAsset plantSound;

	public Transform grownPlantsRoot;

	public Transform[] bigSlots;

	public Transform[] slots;

	private bool initialized;

	private bool subscribed;

	private PlantSlot[] bigPlantSlots;

	private PlantSlot[] smallPlantSlots;

	private float maxPlantsHeight = -1f;

	private static readonly List<SkyApplier> sSkyAppliers = new List<SkyApplier>();

	private static readonly List<InventoryItem> sItemsToRemove = new List<InventoryItem>();

	private static readonly List<InventoryItem> sAddedItems = new List<InventoryItem>();

	private void Initialize()
	{
		if (!initialized)
		{
			bigPlantSlots = new PlantSlot[bigSlots.Length];
			for (int i = 0; i < bigSlots.Length; i++)
			{
				PlantSlot plantSlot = new PlantSlot(i, bigSlots[i]);
				bigPlantSlots[i] = plantSlot;
			}
			smallPlantSlots = new PlantSlot[slots.Length];
			for (int j = 0; j < slots.Length; j++)
			{
				PlantSlot plantSlot2 = new PlantSlot(j + bigPlantSlots.Length, slots[j]);
				smallPlantSlots[j] = plantSlot2;
			}
			ItemsContainer container = storageContainer.container;
			if (container != null)
			{
				container.containerType = GetContainerType();
			}
			initialized = true;
		}
	}

	private void Start()
	{
		Initialize();
	}

	private void OnEnable()
	{
		storageContainer.enabled = true;
	}

	private void OnDisable()
	{
		Subscribe(state: false);
		storageContainer.enabled = false;
	}

	private void LateUpdate()
	{
		Subscribe(state: true);
	}

	private void Subscribe(bool state)
	{
		if (subscribed == state)
		{
			return;
		}
		if (storageContainer.container == null)
		{
			Debug.LogWarning("Planter.Subscribe(): container null; will retry next frame");
			return;
		}
		if (subscribed)
		{
			storageContainer.container.onAddItem -= AddItem;
			storageContainer.container.onRemoveItem -= RemoveItem;
			storageContainer.container.isAllowedToAdd = null;
			storageContainer.container.isAllowedToRemove = null;
		}
		else
		{
			storageContainer.container.onAddItem += AddItem;
			storageContainer.container.onRemoveItem += RemoveItem;
			storageContainer.container.isAllowedToAdd = IsAllowedToAdd;
			storageContainer.container.isAllowedToRemove = IsAllowedToRemove;
		}
		subscribed = state;
	}

	private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
	{
		Plantable component = pickupable.GetComponent<Plantable>();
		if (!component)
		{
			return false;
		}
		if (GetFreeSlotID(component.size != Plantable.PlantSize.Small) < 0)
		{
			return false;
		}
		switch (GetContainerType())
		{
		case ItemsContainerType.LandPlants:
			return component.aboveWater;
		case ItemsContainerType.WaterPlants:
			return component.underwater;
		default:
			return false;
		}
	}

	private ItemsContainerType GetContainerType()
	{
		switch (environment)
		{
		case PlantEnvironment.Air:
			return ItemsContainerType.LandPlants;
		case PlantEnvironment.Water:
			return ItemsContainerType.WaterPlants;
		case PlantEnvironment.Dynamic:
			if (!(base.transform.position.y < -1f))
			{
				return ItemsContainerType.LandPlants;
			}
			return ItemsContainerType.WaterPlants;
		default:
			return ItemsContainerType.Default;
		}
	}

	private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
	{
		return false;
	}

	private void AddItem(InventoryItem item)
	{
		Plantable component = item.item.GetComponent<Plantable>();
		item.item.SetTechTypeOverride(component.plantTechType);
		item.isEnabled = false;
		AddItem(component, GetFreeSlotID(component.size != Plantable.PlantSize.Small));
	}

	private void AddItem(Plantable plantable, int slotID)
	{
		PlantSlot slotByID = GetSlotByID(slotID);
		if (slotByID != null && !slotByID.isOccupied)
		{
			SetSlotOccupiedState(slotID, state: true);
			GameObject gameObject = plantable.Spawn(slotByID.slot, isIndoor);
			SetupRenderers(gameObject, isIndoor);
			SetupLighting(gameObject);
			TryToExcludeFromSubParentRigidbody();
			gameObject.SetActive(value: true);
			slotByID.plantable = plantable;
			slotByID.plantModel = gameObject;
			slotByID.SetMaxPlantHeight(maxPlantsHeight);
			plantable.currentPlanter = this;
			if (plantable.eatable != null)
			{
				plantable.eatable.SetDecomposes(value: false);
			}
			if ((bool)plantSound)
			{
				FMODUWE.PlayOneShot(plantSound, slotByID.slot.position);
			}
		}
	}

	public bool ReplaceItem(Plantable seed, Plantable plant)
	{
		Pickupable component = seed.GetComponent<Pickupable>();
		Pickupable component2 = plant.GetComponent<Pickupable>();
		int slotID = GetSlotID(seed);
		if (component == null || component2 == null || slotID < 0)
		{
			return false;
		}
		bool state = subscribed;
		Subscribe(state: false);
		component.ResetTechTypeOverride();
		if (!storageContainer.container.RemoveItem(component))
		{
			Debug.LogErrorFormat("Failed to replace {0} item: cannot remove {1}", base.transform.name, seed.transform.name);
			Subscribe(state);
			return false;
		}
		InventoryItem inventoryItem = storageContainer.container.AddItem(component2);
		if (inventoryItem == null)
		{
			Debug.LogErrorFormat("Failed to replace {0} item: cannot add {1}", base.transform.name, plant.transform.name);
			Subscribe(state);
			return false;
		}
		inventoryItem.isEnabled = false;
		RemoveItem(slotID);
		AddItem(plant, slotID);
		Subscribe(state);
		return true;
	}

	public void RemoveItem(Plantable plantable)
	{
		if (!(plantable.pickupable == null))
		{
			storageContainer.container.isAllowedToRemove = null;
			plantable.pickupable.ResetTechTypeOverride();
			storageContainer.container.RemoveItem(plantable.pickupable);
			storageContainer.container.isAllowedToRemove = IsAllowedToRemove;
		}
	}

	private void RemoveItem(InventoryItem item)
	{
		Plantable component = item.item.GetComponent<Plantable>();
		RemoveItem(GetSlotID(component));
	}

	private void RemoveItem(int slotID)
	{
		PlantSlot slotByID = GetSlotByID(slotID);
		if (slotByID != null)
		{
			Plantable plantable = slotByID.plantable;
			plantable.currentPlanter = null;
			if (plantable.eatable != null)
			{
				plantable.eatable.SetDecomposes(value: true);
			}
			if ((bool)plantable.linkedGrownPlant)
			{
				Object.Destroy(plantable.linkedGrownPlant.gameObject);
			}
			GameObject plantModel = slotByID.plantModel;
			slotByID.Clear();
			ResourceTracker component = plantModel.GetComponent<ResourceTracker>();
			if ((bool)component)
			{
				component.OnPickedUp(null);
			}
			Object.Destroy(plantModel);
			SetSlotOccupiedState(slotID, state: false);
		}
	}

	private void SetSlotOccupiedState(int slotID, bool state)
	{
		GetSlotByID(slotID).isOccupied = state;
		if (bigPlantSlots.Length == 0)
		{
			return;
		}
		if (slotID < bigPlantSlots.Length)
		{
			int num = slotID * 4;
			for (int i = num; i < num + 4; i++)
			{
				smallPlantSlots[i].isOccupied = state;
			}
			return;
		}
		int num2 = (slotID - bigPlantSlots.Length) / 4;
		if (state)
		{
			bigPlantSlots[num2].isOccupied = true;
			return;
		}
		bool flag = true;
		int num3 = num2 * 4;
		for (int j = num3; j < num3 + 4; j++)
		{
			if (smallPlantSlots[j].isOccupied)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			bigPlantSlots[num2].isOccupied = false;
		}
	}

	private PlantSlot GetSlotByID(int slotID)
	{
		if (slotID < 0 || slotID > bigPlantSlots.Length + smallPlantSlots.Length)
		{
			return null;
		}
		if (slotID < bigPlantSlots.Length)
		{
			return bigPlantSlots[slotID];
		}
		return smallPlantSlots[slotID - bigPlantSlots.Length];
	}

	private int GetFreeSlotID(bool isBigPlant = false)
	{
		if (isBigPlant)
		{
			for (int i = 0; i < bigPlantSlots.Length; i++)
			{
				if (!bigPlantSlots[i].isOccupied)
				{
					return i;
				}
			}
		}
		else
		{
			for (int j = 0; j < smallPlantSlots.Length; j++)
			{
				if (!smallPlantSlots[j].isOccupied)
				{
					return j + bigPlantSlots.Length;
				}
			}
		}
		return -1;
	}

	public int GetSlotID(Plantable plantable)
	{
		if (plantable != null)
		{
			for (int i = 0; i < bigPlantSlots.Length; i++)
			{
				if (bigPlantSlots[i].plantable == plantable)
				{
					return i;
				}
			}
			for (int j = 0; j < smallPlantSlots.Length; j++)
			{
				if (smallPlantSlots[j].plantable == plantable)
				{
					return j + bigPlantSlots.Length;
				}
			}
		}
		return -1;
	}

	public void ResetStorage()
	{
		storageContainer.ResetContainer();
		PlantSlot[] array = bigPlantSlots;
		foreach (PlantSlot obj in array)
		{
			Plantable plantable = obj.plantable;
			if (plantable != null)
			{
				GrownPlant linkedGrownPlant = plantable.linkedGrownPlant;
				if (linkedGrownPlant != null)
				{
					Object.Destroy(linkedGrownPlant.gameObject);
				}
			}
			GameObject plantModel = obj.plantModel;
			obj.Clear();
			Object.Destroy(plantModel);
			obj.isOccupied = false;
		}
		array = smallPlantSlots;
		foreach (PlantSlot obj2 in array)
		{
			Plantable plantable2 = obj2.plantable;
			if (plantable2 != null)
			{
				GrownPlant linkedGrownPlant2 = plantable2.linkedGrownPlant;
				if (linkedGrownPlant2 != null)
				{
					Object.Destroy(linkedGrownPlant2.gameObject);
				}
			}
			GameObject plantModel2 = obj2.plantModel;
			obj2.Clear();
			Object.Destroy(plantModel2);
			obj2.isOccupied = false;
		}
	}

	public void SetupRenderers(GameObject gameObject, bool interior)
	{
		int newLayer = ((!interior) ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("Viewmodel"));
		Utils.SetLayerRecursively(gameObject, newLayer);
	}

	public void SetupLighting(GameObject plant)
	{
	}

	public void TryToExcludeFromSubParentRigidbody()
	{
		SubRoot componentInParent = GetComponentInParent<SubRoot>();
		if (componentInParent != null && componentInParent.isCyclops && constructable != null)
		{
			constructable.ExcludeFromSubParentRigidbody();
		}
	}

	public void SetMaxPlantsHeight(float height)
	{
		maxPlantsHeight = height;
		if (!(maxPlantsHeight <= 0f))
		{
			PlantSlot[] array = bigPlantSlots;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetMaxPlantHeight(maxPlantsHeight);
			}
			array = smallPlantSlots;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetMaxPlantHeight(maxPlantsHeight);
			}
		}
	}

	private IEnumerator DeserializeAsync()
	{
		Initialize();
		foreach (Transform item2 in grownPlantsRoot.transform)
		{
			GrownPlant component = item2.GetComponent<GrownPlant>();
			if (component != null)
			{
				component.FindSeed();
				continue;
			}
			item2.gameObject.SetActive(value: false);
			Debug.LogErrorFormat("Cannot find GrownPlant component on child {0} of {1} grownPlantRoot", item2.name, base.transform.name);
		}
		yield return null;
		ItemsContainer container = storageContainer.container;
		if (container == null)
		{
			yield break;
		}
		sItemsToRemove.Clear();
		sAddedItems.Clear();
		foreach (InventoryItem item3 in (IEnumerable<InventoryItem>)container)
		{
			Plantable component2 = item3.item.GetComponent<Plantable>();
			if (component2 == null)
			{
				sItemsToRemove.Add(item3);
				continue;
			}
			int slotID = component2.GetSlotID();
			PlantSlot slotByID = GetSlotByID(slotID);
			if (slotByID == null || slotByID.isOccupied || slotID < bigPlantSlots.Length == (component2.size == Plantable.PlantSize.Small))
			{
				sItemsToRemove.Add(item3);
				if (component2.linkedGrownPlant != null)
				{
					Object.Destroy(component2.linkedGrownPlant.gameObject);
				}
			}
			else
			{
				AddItem(component2, slotID);
				sAddedItems.Add(item3);
			}
		}
		foreach (InventoryItem sAddedItem in sAddedItems)
		{
			Pickupable item = sAddedItem.item;
			Plantable component3 = item.GetComponent<Plantable>();
			item.SetTechTypeOverride(component3.plantTechType);
			sAddedItem.isEnabled = false;
		}
		IItemsContainer container2 = storageContainer.container;
		for (int i = 0; i < sItemsToRemove.Count; i++)
		{
			container2.RemoveItem(sItemsToRemove[i], forced: true, verbose: false);
			Object.Destroy(sItemsToRemove[i].item.gameObject);
		}
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		CoroutineHost.StartCoroutine(DeserializeAsync());
	}
}
