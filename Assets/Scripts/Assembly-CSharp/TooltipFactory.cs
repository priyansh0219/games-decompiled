using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Gendarme;
using UnityEngine;

public static class TooltipFactory
{
	private static bool initialized;

	private static string stringBatteryNotInserted;

	private static string stringLockedRecipeHint;

	private static string stringUse;

	private static string stringEat;

	private static string stringEquip;

	private static string stringUnequip;

	private static string stringAssignQuickSlot;

	private static string stringBindQuickSlot;

	private static string stringPinRecipe;

	private static string stringUnpinRecipe;

	private static string stringUnpinAll;

	private static string stringNodeEnter;

	private static string stringNodeExit;

	private static string stringCraft;

	private static string stringSwitchContainer;

	private static string stringSwapItems;

	private static string stringDrop;

	private static string stringPlace;

	private static string stringSelect;

	private static string stringCancel;

	private static string stringKeyRange15;

	private static string stringLeftMouseButton;

	private static string stringRightMouseButton;

	private static string stringButton0;

	private static string stringButton1;

	private static string stringButton2;

	private static string stringButton3;

	public static readonly CachedEnumString<TechType> techTypeTooltipStrings = new CachedEnumString<TechType>("Tooltip_", TechTypeExtensions.sTechTypeComparer);

	public static readonly CachedEnumString<TechType> techTypeIngredientStrings = new CachedEnumString<TechType>(string.Empty, ".TooltipIngredient", TechTypeExtensions.sTechTypeComparer);

	public static bool debug;

	private static void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			Language.OnLanguageChanged += OnLanguageChanged;
			RefreshActionStrings();
			GameInput.OnBindingsChanged += OnBindingsChanged;
			RefreshBindingStrings();
		}
	}

	private static void OnLanguageChanged()
	{
		RefreshActionStrings();
		RefreshBindingStrings();
	}

	private static void RefreshActionStrings()
	{
		Language main = Language.main;
		stringBatteryNotInserted = main.Get("BatteryNotInserted");
		stringLockedRecipeHint = main.Get("LockedRecipeHint");
		stringUse = main.Get("Use");
		stringEat = main.Get("Eat");
		stringEquip = main.Get("Equip");
		stringUnequip = main.Get("Unequip");
		stringAssignQuickSlot = main.Get("AssignQuickSlot");
		stringBindQuickSlot = main.Get("BindQuickSlot");
		stringPinRecipe = main.Get("PinRecipe");
		stringUnpinRecipe = main.Get("UnpinRecipe");
		stringUnpinAll = main.Get("UnpinAll");
		stringNodeEnter = main.Get("NodeEnter");
		stringNodeExit = main.Get("NodeExit");
		stringCraft = main.Get("Craft");
		stringSwitchContainer = main.Get("SwitchContainer");
		stringSwapItems = main.Get("SwapItems");
		stringDrop = main.Get("Drop");
		stringPlace = main.Get("Place");
		stringSelect = main.Get("ActionSelect");
		stringCancel = main.Get("ActionCancel");
		stringKeyRange15 = main.Get("KeyRange15");
	}

	private static void OnBindingsChanged()
	{
		RefreshBindingStrings();
	}

	private static void RefreshBindingStrings()
	{
		stringButton0 = GameInput.FormatButton(GameInput.button0);
		stringButton1 = GameInput.FormatButton(GameInput.button1);
		stringButton2 = GameInput.FormatButton(GameInput.button2);
		stringButton3 = GameInput.FormatButton(GameInput.button3);
	}

	public static void Label(string label, StringBuilder sb)
	{
		Initialize();
		WriteTitle(sb, Language.main.Get(label));
	}

	public static void InventoryItem(InventoryItem item, TooltipData data)
	{
		Initialize();
		Pickupable item2 = item.item;
		ItemCommons(data.prefix, item2.GetTechType(), item2.gameObject);
		ItemActions(data.postfix, item);
	}

	public static void InventoryItemView(InventoryItem item, TooltipData data)
	{
		Initialize();
		Pickupable item2 = item.item;
		ItemCommons(data.prefix, item2.GetTechType(), item2.gameObject);
	}

	public static void QuickSlot(TechType techType, GameObject obj, TooltipData data)
	{
		Initialize();
		ItemCommons(data.prefix, techType, obj);
	}

	public static void CraftNode(string label, bool expanded, TooltipData data)
	{
		Initialize();
		WriteTitle(data.prefix, Language.main.Get(label));
		if (GameInput.PrimaryDevice == GameInput.Device.Controller)
		{
			if (!expanded)
			{
				WriteAction(data.postfix, stringButton0, stringNodeEnter);
			}
			WriteAction(data.postfix, stringButton1, stringNodeExit);
		}
		else if (!expanded)
		{
			WriteAction(data.postfix, stringButton0, stringNodeEnter);
		}
	}

	public static void CraftRecipe(TechType techType, bool locked, TooltipData data)
	{
		Initialize();
		if (locked)
		{
			WriteTitle(data.prefix, Language.main.Get(techType));
			WriteDescription(data.prefix, stringLockedRecipeHint);
			return;
		}
		string text = Language.main.Get(techType);
		int craftAmount = TechData.GetCraftAmount(techType);
		if (craftAmount > 1)
		{
			text = Language.main.GetFormat("CraftMultipleFormat", text, craftAmount);
		}
		WriteTitle(data.prefix, text);
		WriteDescription(data.prefix, Language.main.Get(techTypeTooltipStrings.Get(techType)));
		WriteIngredients(TechData.GetIngredients(techType), data.icons);
		if (!locked)
		{
			bool flag = GameInput.PrimaryDevice == GameInput.Device.Controller;
			if (CrafterLogic.IsCraftRecipeFulfilled(techType))
			{
				WriteAction(data.postfix, stringButton0, stringCraft);
			}
			bool pin = PinManager.GetPin(techType);
			string key = (flag ? stringButton2 : stringButton1);
			if (pin)
			{
				WriteAction(data.postfix, key, stringUnpinRecipe);
			}
			else if (PinManager.Count < PinManager.max)
			{
				WriteAction(data.postfix, key, stringPinRecipe);
			}
			if (flag)
			{
				WriteAction(data.postfix, stringButton1, stringNodeExit);
			}
		}
	}

	public static void Blueprint(TechType techType, bool locked, TooltipData data)
	{
		Initialize();
		BuildTech(techType, locked, data);
		if (!locked)
		{
			bool num = GameInput.PrimaryDevice == GameInput.Device.Controller;
			bool pin = PinManager.GetPin(techType);
			string key = stringButton0;
			if (pin)
			{
				WriteAction(data.postfix, key, stringUnpinRecipe);
			}
			else if (PinManager.Count < PinManager.max)
			{
				WriteAction(data.postfix, key, stringPinRecipe);
			}
			if (num && PinManager.Count > 0)
			{
				WriteAction(data.postfix, stringButton3, stringUnpinAll);
			}
		}
	}

	public static void Ingredient(TechType techType, TooltipData data)
	{
		Initialize();
		ReadOnlyCollection<Ingredient> ingredients = TechData.GetIngredients(techType);
		if (ingredients != null && ingredients.Count > 0)
		{
			bool locked = !CrafterLogic.IsCraftRecipeUnlocked(techType);
			BuildTech(techType, locked, data);
		}
		else
		{
			Language main = Language.main;
			WriteTitle(data.prefix, main.Get(techType));
			WriteDescription(data.prefix, main.Get(techTypeTooltipStrings.Get(techType)));
		}
	}

	public static void BuilderItem(TechType techType, bool locked, TooltipData data)
	{
		Initialize();
		BuildTech(techType, locked, data);
		if (GameInput.PrimaryDevice == GameInput.Device.Controller)
		{
			if (!locked)
			{
				WriteAction(data.postfix, stringButton0, stringSelect);
			}
			WriteAction(data.postfix, stringButton1, stringCancel);
		}
	}

	public static void BuildTech(TechType techType, bool locked, TooltipData data)
	{
		Initialize();
		string key = techType.AsString();
		if (locked)
		{
			WriteTitle(data.prefix, Language.main.Get(key));
			WriteDescription(data.prefix, stringLockedRecipeHint);
			return;
		}
		WriteTitle(data.prefix, Language.main.Get(key));
		WriteDebug(data.prefix, techType);
		WriteDescription(data.prefix, Language.main.Get(techTypeTooltipStrings.Get(techType)));
		WriteIngredients(TechData.GetIngredients(techType), data.icons);
	}

	public static float GetBarValue(InventoryItem item)
	{
		if (item == null)
		{
			return -1f;
		}
		if (!item.isBarVisible)
		{
			return -1f;
		}
		return GetBarValue(item.item);
	}

	public static float GetBarValue(Pickupable pickupable)
	{
		if (pickupable == null)
		{
			return -1f;
		}
		pickupable.GetTechType();
		GameObject gameObject = pickupable.gameObject;
		EnergyMixin component = gameObject.GetComponent<EnergyMixin>();
		if (component != null)
		{
			return component.GetBatteryChargeValue();
		}
		IBattery component2 = gameObject.GetComponent<IBattery>();
		if (component2 != null)
		{
			return Clamp01(component2.charge / component2.capacity);
		}
		Eatable component3 = gameObject.GetComponent<Eatable>();
		if (component3 != null)
		{
			if (GameModeUtils.RequiresSurvival())
			{
				if (!component3.decomposes)
				{
					return -1f;
				}
				float foodValue = component3.foodValue;
				float waterValue = component3.waterValue;
				float num = Mathf.Max(0f, component3.GetFoodValue());
				float num2 = Mathf.Max(0f, component3.GetWaterValue());
				if (foodValue > 0f)
				{
					if (waterValue > 0f)
					{
						return Clamp01((num + num2) / (foodValue + waterValue));
					}
					return Clamp01(num / foodValue);
				}
				if (waterValue > 0f)
				{
					return Clamp01(num2 / waterValue);
				}
				return -1f;
			}
			return -1f;
		}
		IOxygenSource component4 = gameObject.GetComponent<IOxygenSource>();
		if (component4 != null)
		{
			return Clamp01(component4.GetOxygenAvailable() / component4.GetOxygenCapacity());
		}
		FireExtinguisher component5 = gameObject.GetComponent<FireExtinguisher>();
		if (component5 != null)
		{
			return Clamp01(component5.fuel / component5.maxFuel);
		}
		return -1f;
	}

	public static Color GetBarColor(float value)
	{
		float num = 0.2f;
		float num2 = 0.5f;
		float num3 = 0.8f;
		Color color = new Color(0.816f, 0.447f, 0.325f, 1f);
		Color color2 = new Color(0.976f, 0.839f, 0.341f, 1f);
		Color color3 = new Color(0.643f, 0.843f, 0.412f, 1f);
		return new Color(0f, 0f, 0f, 0f) + color * ColorClamp((num2 - value) / (num2 - num), 0f, 1f) + color2 * ColorClamp((num3 - value) / (num3 - num2), 0f, (value - num) / (num2 - num)) + color3 * ColorClamp((value - num2) / (num3 - num2), 0f, 1f);
	}

	public static Color GetBarColor(float value, Color red, Color yellow, Color green)
	{
		float num = 0.2f;
		float num2 = 0.5f;
		float num3 = 0.8f;
		return new Color(0f, 0f, 0f, 0f) + red * ColorClamp((num2 - value) / (num2 - num), 0f, 1f) + yellow * ColorClamp((num3 - value) / (num3 - num2), 0f, (value - num) / (num2 - num)) + green * ColorClamp((value - num2) / (num3 - num2), 0f, 1f);
	}

	private static float ColorClamp(float x, float a, float b)
	{
		return Mathf.Max(a, Mathf.Min(b, x));
	}

	private static float Clamp01(float value)
	{
		if (float.IsNaN(value))
		{
			return -1f;
		}
		return Mathf.Clamp01(value);
	}

	private static void ItemCommons(StringBuilder sb, TechType techType, GameObject obj)
	{
		Language main = Language.main;
		string text = main.Get(techType);
		Creature component = obj.GetComponent<Creature>();
		if (component != null)
		{
			LiveMixin liveMixin = component.liveMixin;
			if (liveMixin != null && !liveMixin.IsAlive())
			{
				text = main.GetFormat("DeadFormat", text);
			}
		}
		Eatable component2 = obj.GetComponent<Eatable>();
		if (component2 != null)
		{
			string secondaryTooltip = component2.GetSecondaryTooltip();
			if (!string.IsNullOrEmpty(secondaryTooltip))
			{
				text = main.GetFormat("DecomposingFormat", secondaryTooltip, text);
			}
		}
		WriteTitle(sb, text);
		WriteDebug(sb, techType);
		bool flag = true;
		EnergyMixin component3 = obj.GetComponent<EnergyMixin>();
		if (component3 != null)
		{
			IBattery battery = component3.GetBattery();
			if (battery != null)
			{
				WriteDescription(sb, battery.GetChargeValueText());
			}
			else
			{
				WriteDescription(sb, stringBatteryNotInserted);
			}
		}
		IBattery component4 = obj.GetComponent<IBattery>();
		if (component4 != null)
		{
			WriteDescription(sb, component4.GetChargeValueText());
			flag = false;
		}
		if (techType == TechType.FirstAidKit)
		{
			WriteDescription(sb, main.GetFormat("HealthFormat", 50f));
		}
		if (component2 != null && GameModeUtils.RequiresSurvival())
		{
			int num = Mathf.CeilToInt(component2.GetFoodValue());
			if (num != 0)
			{
				WriteDescription(sb, main.GetFormat("FoodFormat", num));
			}
			int num2 = Mathf.CeilToInt(component2.GetWaterValue());
			if (num2 != 0)
			{
				WriteDescription(sb, main.GetFormat("WaterFormat", num2));
			}
		}
		IOxygenSource component5 = obj.GetComponent<IOxygenSource>();
		if (component5 != null)
		{
			WriteDescription(sb, component5.GetSecondaryTooltip());
		}
		if (flag)
		{
			WriteDescription(sb, main.Get(techTypeTooltipStrings.Get(techType)));
		}
		Signal component6 = obj.GetComponent<Signal>();
		if (component6 != null)
		{
			WriteDescription(sb, main.Get(component6.targetDescription));
		}
		FireExtinguisher component7 = obj.GetComponent<FireExtinguisher>();
		if (component7 != null)
		{
			WriteDescription(sb, component7.GetFuelValueText());
		}
	}

	private static void ItemActions(StringBuilder sb, InventoryItem item)
	{
		bool flag = item != null && item == ItemDragManager.hoveredItem && Inventory.main.GetCanBindItem(item);
		ItemAction itemAction = Inventory.main.GetItemAction(item, 0);
		ItemAction itemAction2 = Inventory.main.GetItemAction(item, 1);
		ItemAction itemAction3 = Inventory.main.GetItemAction(item, 2);
		ItemAction itemAction4 = Inventory.main.GetItemAction(item, 3);
		bool flag2 = GameInput.PrimaryDevice == GameInput.Device.Controller;
		if (flag || (itemAction | itemAction2 | itemAction3 | itemAction4) != 0)
		{
			if (flag && !flag2)
			{
				WriteAction(sb, stringKeyRange15, stringBindQuickSlot);
			}
			if (itemAction4 != 0)
			{
				WriteAction(sb, stringButton3, GetUseActionString(itemAction4));
			}
			if (itemAction != 0)
			{
				WriteAction(sb, stringButton0, GetUseActionString(itemAction));
			}
			if (itemAction3 != 0)
			{
				WriteAction(sb, stringButton2, GetUseActionString(itemAction3));
			}
			if (itemAction2 != 0)
			{
				WriteAction(sb, stringButton1, GetUseActionString(itemAction2));
			}
		}
	}

	[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidUnnecessarySpecializationRule")]
	private static void WriteIngredients(IList<Ingredient> ingredients, List<TooltipIcon> icons)
	{
		if (ingredients == null)
		{
			return;
		}
		int count = ingredients.Count;
		Inventory main = Inventory.main;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < count; i++)
		{
			stringBuilder.Length = 0;
			Ingredient ingredient = ingredients[i];
			TechType techType = ingredient.techType;
			int pickupCount = main.GetPickupCount(techType);
			int amount = ingredient.amount;
			bool num = pickupCount >= amount || !GameModeUtils.RequiresIngredients();
			Sprite sprite = SpriteManager.Get(techType);
			if (num)
			{
				stringBuilder.Append("<color=#94DE00FF>");
			}
			else
			{
				stringBuilder.Append("<color=#DF4026FF>");
			}
			string orFallback = Language.main.GetOrFallback(techTypeIngredientStrings.Get(techType), techType);
			stringBuilder.Append(orFallback);
			if (amount > 1)
			{
				stringBuilder.Append(" x");
				stringBuilder.Append(amount);
			}
			if (pickupCount > 0 && pickupCount < amount)
			{
				stringBuilder.Append(" (");
				stringBuilder.Append(pickupCount);
				stringBuilder.Append(")");
			}
			stringBuilder.Append("</color>");
			icons.Add(new TooltipIcon(sprite, stringBuilder.ToString()));
		}
	}

	private static void WriteTitle(StringBuilder sb, string title)
	{
		sb.AppendFormat("<size=25><color=#ffffffff>{0}</color></size>", title);
	}

	private static void WriteDescription(StringBuilder sb, string description)
	{
		if (sb.Length > 0)
		{
			sb.Append('\n');
		}
		sb.AppendFormat("<size=20><color=#DDDEDEFF>{0}</color></size>", description);
	}

	private static void WriteAction(StringBuilder sb, string key, string action)
	{
		if (sb.Length > 0)
		{
			sb.Append('\n');
		}
		sb.AppendFormat("<size=20><color=#ffffffff>{0}</color> - <color=#00ffffff>{1}</color></size>", key, action);
	}

	private static string GetUseActionString(ItemAction action)
	{
		switch (action)
		{
		case ItemAction.Use:
			return stringUse;
		case ItemAction.Eat:
			return stringEat;
		case ItemAction.Equip:
			return stringEquip;
		case ItemAction.Unequip:
			return stringUnequip;
		case ItemAction.Switch:
			return stringSwitchContainer;
		case ItemAction.Swap:
			return stringSwapItems;
		case ItemAction.Drop:
			return stringDrop;
		case ItemAction.Assign:
			return stringAssignQuickSlot;
		default:
			return null;
		}
	}

	private static void WriteDebug(StringBuilder sb, TechType techType)
	{
		if (debug)
		{
			if (sb.Length > 0)
			{
				sb.Append('\n');
			}
			sb.Append("<size=14><color=#DFBF30>");
			sb.AppendFormat("id: {0}", techType.AsString());
			sb.Append("</color></size>");
		}
	}
}
