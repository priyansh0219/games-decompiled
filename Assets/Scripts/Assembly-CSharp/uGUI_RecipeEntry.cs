using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class uGUI_RecipeEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, ITooltip
{
	private static readonly Vector2 iconSize = new Vector2(80f, 80f);

	private static readonly Vector2 itemIconOffset = new Vector2(0f, 3f);

	private static readonly Vector2 itemIconSize = new Vector2(55f, 55f);

	[AssertNotNull]
	public uGUI_ItemIcon icon;

	[AssertNotNull]
	public TextMeshProUGUI text;

	[AssertNotNull]
	public RectTransform canvas;

	[AssertNotNull]
	public uGUI_RecipeItem prefabItem;

	[AssertNotNull]
	public GameObject background;

	[HideInInspector]
	public uGUI_PinnedRecipes manager;

	private List<uGUI_RecipeItem> items = new List<uGUI_RecipeItem>();

	private PrefabPool<uGUI_RecipeItem> pool;

	private int min = int.MinValue;

	public TechType techType { get; private set; }

	public RectTransform rectTransform { get; private set; }

	public bool showTooltipOnDrag => false;

	private void Awake()
	{
		pool = new PrefabPool<uGUI_RecipeItem>(prefabItem.gameObject, canvas, 2, 1, delegate(uGUI_RecipeItem e)
		{
			e.icon.SetPosition(itemIconOffset);
			e.icon.SetSize(itemIconSize);
			e.Deinitialize();
		}, delegate(uGUI_RecipeItem e)
		{
			e.Deinitialize();
		});
		rectTransform = GetComponent<RectTransform>();
		icon.SetSize(iconSize);
	}

	public void Initialize(TechType techType)
	{
		this.techType = techType;
		base.gameObject.SetActive(value: true);
		icon.SetBackgroundSprite(SpriteManager.GetBackground(techType));
		icon.SetForegroundSprite(SpriteManager.Get(techType));
		icon.SetBackgroundRadius(0.5f * Mathf.Min(iconSize.x, iconSize.y));
	}

	public void Deinitialize()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetMode(uGUI_PinnedRecipes.Mode mode)
	{
		bool active = false;
		bool active2 = false;
		switch (mode)
		{
		case uGUI_PinnedRecipes.Mode.Off:
			active = false;
			active2 = false;
			break;
		case uGUI_PinnedRecipes.Mode.Blueprints:
			active = true;
			active2 = false;
			break;
		case uGUI_PinnedRecipes.Mode.Full:
			active = true;
			active2 = true;
			break;
		}
		icon.gameObject.SetActive(active);
		text.gameObject.SetActive(active);
		canvas.gameObject.SetActive(active2);
	}

	public void UpdateIngredients(ItemsContainer container, bool ping)
	{
		ReadOnlyCollection<Ingredient> ingredients = TechData.GetIngredients(this.techType);
		int craftAmount = TechData.GetCraftAmount(this.techType);
		int num = -1;
		int num2 = ingredients?.Count ?? 0;
		while (items.Count < num2)
		{
			uGUI_RecipeItem uGUI_RecipeItem2 = pool.Get();
			uGUI_RecipeItem2.Initialize();
			items.Add(uGUI_RecipeItem2);
		}
		while (items.Count > num2)
		{
			int index = items.Count - 1;
			uGUI_RecipeItem entry = items[index];
			items.RemoveAt(index);
			pool.Release(entry);
		}
		for (int i = 0; i < num2; i++)
		{
			Ingredient ingredient = ingredients[i];
			TechType techType = ingredient.techType;
			int count = container.GetCount(techType);
			int amount = ingredient.amount;
			int num3 = count / amount;
			if (num < 0 || num3 < num)
			{
				num = num3;
			}
			uGUI_RecipeItem obj = items[i];
			obj.text.color = ((count >= amount) ? manager.colorGreen : manager.colorRed);
			obj.Set(techType, count, amount, ping);
		}
		background.SetActive(num2 > 0);
		num *= craftAmount;
		if (num > 0)
		{
			if (min != num)
			{
				min = num;
				text.text = $"x{IntStringCache.GetStringForInt(min)}";
			}
		}
		else
		{
			min = int.MinValue;
			text.text = string.Empty;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnPointerClick(this);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnBeginDrag(this);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnEndDrag(this);
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnDrop(this);
		}
	}

	public void GetTooltip(TooltipData data)
	{
		if (techType != 0)
		{
			bool locked = !CrafterLogic.IsCraftRecipeUnlocked(techType);
			TooltipFactory.BuildTech(techType, locked, data);
		}
	}
}
