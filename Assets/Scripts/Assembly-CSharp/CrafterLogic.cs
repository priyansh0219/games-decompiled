using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public sealed class CrafterLogic : MonoBehaviour, IProtoEventListener
{
	public delegate void OnItemChanged(TechType techType);

	public delegate void OnProgress(float progress);

	public delegate void OnDone();

	public delegate void OnItemPickup(GameObject item);

	public OnItemChanged onItemChanged;

	public OnProgress onProgress;

	public OnDone onDone;

	public OnItemPickup onItemPickup;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public float timeCraftingBegin = -1f;

	[NonSerialized]
	[ProtoMember(3)]
	public float timeCraftingEnd = -1f;

	[NonSerialized]
	[ProtoMember(4)]
	public TechType craftingTechType;

	[NonSerialized]
	[ProtoMember(5)]
	public int linkedIndex = -1;

	[NonSerialized]
	[ProtoMember(6)]
	public int numCrafted;

	private double lastTime;

	private Coroutine pickupRoutine;

	private bool pickingUp;

	[AssertLocalization]
	private const string missingIngredientsMessage = "DontHaveNeededIngredients";

	public bool inProgress
	{
		get
		{
			if (currentTechType != 0)
			{
				return DayNightCycle.main.timePassed < (double)timeCraftingEnd;
			}
			return false;
		}
	}

	public float progress
	{
		get
		{
			double timePassed = DayNightCycle.main.timePassed;
			double num = timeCraftingEnd - timeCraftingBegin;
			float value = (float)((timePassed - (double)timeCraftingBegin) / num);
			if (!(timeCraftingEnd > timeCraftingBegin))
			{
				return -1f;
			}
			return Mathf.Clamp01(value);
		}
	}

	public TechType currentTechType
	{
		get
		{
			if (linkedIndex > -1)
			{
				ReadOnlyCollection<TechType> linkedItems = TechData.GetLinkedItems(craftingTechType);
				if (linkedItems != null && linkedIndex < linkedItems.Count)
				{
					return linkedItems[linkedIndex];
				}
			}
			return craftingTechType;
		}
	}

	private void Update()
	{
		if (craftingTechType != 0 && lastTime < (double)timeCraftingEnd)
		{
			NotifyProgress(progress);
			lastTime = DayNightCycle.main.timePassed;
			if (lastTime >= (double)timeCraftingEnd)
			{
				NotifyEnd();
			}
		}
	}

	private void OnDisable()
	{
		if (pickupRoutine != null)
		{
			StopCoroutine(pickupRoutine);
			pickupRoutine = null;
		}
		pickingUp = false;
	}

	public bool Craft(TechType techType, float craftTime)
	{
		if (craftTime > 0f)
		{
			timeCraftingBegin = DayNightCycle.main.timePassedAsFloat;
			timeCraftingEnd = timeCraftingBegin + craftTime + 0.1f;
			craftingTechType = techType;
			linkedIndex = -1;
			numCrafted = TechData.GetCraftAmount(techType);
			NotifyChanged(craftingTechType);
			NotifyProgress(0f);
			return true;
		}
		return false;
	}

	public void ResetCrafter()
	{
		timeCraftingBegin = -1f;
		timeCraftingEnd = -1f;
		craftingTechType = TechType.None;
		linkedIndex = -1;
		numCrafted = 0;
		NotifyChanged(TechType.None);
		NotifyProgress(0f);
	}

	public void TryPickup()
	{
		if (!pickingUp)
		{
			pickupRoutine = StartCoroutine(TryPickupAsync());
		}
	}

	private IEnumerator TryPickupAsync()
	{
		if (craftingTechType == TechType.None || progress < 1f)
		{
			yield break;
		}
		ReadOnlyCollection<TechType> linkedItems = TechData.GetLinkedItems(craftingTechType);
		int linkedItemsCount = linkedItems?.Count ?? 0;
		pickingUp = true;
		bool interrupt = false;
		while (!interrupt)
		{
			TechType techType2 = craftingTechType;
			if (linkedIndex != -1)
			{
				techType2 = ((linkedIndex < linkedItemsCount) ? linkedItems[linkedIndex] : TechType.None);
			}
			while (numCrafted > 0)
			{
				TaskResult<bool> result = new TaskResult<bool>();
				yield return TryPickupSingleAsync(techType2, result);
				if (result.Get())
				{
					numCrafted--;
					continue;
				}
				pickingUp = false;
				yield break;
			}
			if (numCrafted == 0)
			{
				linkedIndex++;
				if (linkedIndex < linkedItemsCount)
				{
					numCrafted = 1;
					techType2 = linkedItems[linkedIndex];
					NotifyChanged(techType2);
				}
				else
				{
					interrupt = true;
				}
			}
		}
		pickingUp = false;
		ResetCrafter();
	}

	private IEnumerator TryPickupSingleAsync(TechType techType, IOut<bool> result)
	{
		Inventory inventory = Inventory.main;
		bool overrideTech = false;
		CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
		yield return request;
		GameObject gameObject = request.GetResult();
		if (gameObject == null)
		{
			gameObject = Utils.genericLootPrefab;
			overrideTech = true;
		}
		if (gameObject != null)
		{
			Pickupable component = gameObject.GetComponent<Pickupable>();
			if (component != null)
			{
				Vector2int itemSize = TechData.GetItemSize(component.GetTechType());
				if (inventory.HasRoomFor(itemSize.x, itemSize.y))
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
					component = gameObject2.GetComponent<Pickupable>();
					if (overrideTech)
					{
						component.SetTechTypeOverride(techType, lootCube: true);
					}
					NotifyCraftEnd(gameObject2, craftingTechType);
					inventory.ForcePickup(component);
					Player.main.PlayGrab();
					NotifyPickup(gameObject2);
					result.Set(value: true);
				}
				else
				{
					ErrorMessage.AddMessage(Language.main.Get("InventoryFull"));
					result.Set(value: false);
				}
			}
			else
			{
				Debug.LogErrorFormat("Can't find Pickupable component on prefab for TechType.{0}", techType);
				result.Set(value: true);
			}
		}
		else
		{
			Debug.LogErrorFormat("Can't find prefab for TechType.{0}", techType);
			result.Set(value: true);
		}
	}

	private void NotifyChanged(TechType techType)
	{
		if (onItemChanged != null)
		{
			onItemChanged(techType);
		}
	}

	private void NotifyProgress(float progress)
	{
		if (onProgress != null)
		{
			onProgress(progress);
		}
	}

	private void NotifyEnd()
	{
		CraftingAnalytics.main.OnCraft(craftingTechType, base.transform.position);
		if (onDone != null)
		{
			onDone();
		}
	}

	private void NotifyPickup(GameObject item)
	{
		if (onItemPickup != null)
		{
			onItemPickup(item);
		}
	}

	public static bool IsCraftRecipeUnlocked(TechType techType)
	{
		if (GameModeUtils.RequiresBlueprints())
		{
			return KnownTech.Contains(techType);
		}
		return true;
	}

	public static bool IsCraftRecipeFulfilled(TechType techType)
	{
		if (Inventory.main == null)
		{
			return false;
		}
		if (!GameModeUtils.RequiresIngredients())
		{
			return true;
		}
		Inventory main = Inventory.main;
		ReadOnlyCollection<Ingredient> ingredients = TechData.GetIngredients(techType);
		int num = ingredients?.Count ?? 0;
		int i = 0;
		for (int num2 = num; i < num2; i++)
		{
			Ingredient ingredient = ingredients[i];
			if (main.GetPickupCount(ingredient.techType) < ingredient.amount)
			{
				return false;
			}
		}
		return true;
	}

	public static bool ConsumeEnergy(PowerRelay powerRelay, float amount)
	{
		if (!GameModeUtils.RequiresPower())
		{
			return true;
		}
		if (powerRelay == null)
		{
			return false;
		}
		float amountConsumed;
		return powerRelay.ConsumeEnergy(amount, out amountConsumed);
	}

	public static bool ConsumeResources(TechType techType)
	{
		if (IsCraftRecipeFulfilled(techType))
		{
			Inventory.main.ConsumeResourcesForRecipe(techType);
			return true;
		}
		ErrorMessage.AddWarning(Language.main.Get("DontHaveNeededIngredients"));
		return false;
	}

	public static void NotifyCraftEnd(GameObject target, TechType techType)
	{
		if (target == null)
		{
			return;
		}
		using (ListPool<ICraftTarget> listPool = Pool<ListPool<ICraftTarget>>.Get())
		{
			List<ICraftTarget> list = listPool.list;
			target.GetComponentsInChildren(includeInactive: true, list);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				list[i].OnCraftEnd(techType);
			}
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (craftingTechType != 0)
		{
			if (TechData.TryGetValue(craftingTechType, out var _))
			{
				if (linkedIndex != -1)
				{
					int num = TechData.GetLinkedItems(craftingTechType)?.Count ?? 0;
					if (linkedIndex >= num)
					{
						ResetCrafter();
					}
				}
			}
			else
			{
				ResetCrafter();
			}
		}
		DayNightCycle main = DayNightCycle.main;
		if (main != null)
		{
			lastTime = main.timePassed;
		}
		NotifyChanged(currentTechType);
		NotifyProgress(progress);
	}
}
