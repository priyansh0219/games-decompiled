using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_ItemSelector : MonoBehaviour, IInputHandler
{
	private const float spaceBetweenIcons = 0.5f;

	private const float timeout = 5f;

	private const float fadeTime = 1f;

	private static readonly GameInput.Button[] buttonsNext = new GameInput.Button[3]
	{
		GameInput.Button.CycleNext,
		GameInput.Button.MoveForward,
		GameInput.Button.MoveRight
	};

	private static readonly GameInput.Button[] buttonsPrevious = new GameInput.Button[3]
	{
		GameInput.Button.CyclePrev,
		GameInput.Button.MoveBackward,
		GameInput.Button.MoveLeft
	};

	public TextMeshProUGUI text;

	public RectTransform canvas;

	public CanvasGroup canvasGroup;

	public AnimationCurve alphaCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	private List<IItemsContainer> srcContainers;

	private IItemSelectorManager manager;

	private Sprite defaultSprite;

	private List<uGUI_ItemIcon> icons = new List<uGUI_ItemIcon>();

	private int used = -1;

	private int selected = -1;

	private List<InventoryItem> items = new List<InventoryItem>();

	private Sequence sequence = new Sequence();

	[SerializeField]
	private RectMask2D rectMask;

	private static readonly List<Graphic> sGraphic = new List<Graphic>();

	private void Awake()
	{
		text.text = "";
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		SetState(active: false, immediately: true);
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(OnUpdate);
	}

	private void OnUpdate()
	{
		Vector2 size = canvas.rect.size;
		int count = icons.Count;
		for (int i = 0; i < count; i++)
		{
			InventoryItem item = ((i > 0) ? items[i - 1] : null);
			uGUI_ItemIcon obj = icons[i];
			RectTransform rectTransform = obj.rectTransform;
			GetIconPositionScale(i, out var x, out var s);
			obj.SetBarValue(TooltipFactory.GetBarValue(item));
			float t = 7f * PDA.deltaTime;
			x = Mathf.Lerp(rectTransform.anchoredPosition.x, x, t);
			s = Mathf.Lerp(rectTransform.localScale.x, s, t);
			obj.SetPosition(x, 0f);
			obj.SetScale(s, s);
			float num = alphaCurve.Evaluate(x / (0.5f * size.x));
			obj.SetAlpha(num, num, num);
		}
		if (sequence.active)
		{
			sequence.Update(PDA.deltaTime);
			float time = sequence.time;
			float num2 = sequence.t * time;
			float alpha = Mathf.Clamp01((time - num2) / 1f);
			canvasGroup.alpha = alpha;
			if (!sequence.active)
			{
				ResetSelector();
			}
		}
		if (manager != null)
		{
			using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
			{
				StringBuilder sb = stringBuilderPool.sb;
				Language main = Language.main;
				sb.Append(main.Get("ItemSelectorPrevious"));
				sb.Append(' ');
				GameInput.AppendDisplayText(buttonsPrevious, sb);
				sb.Append(Language.main.Get("InputSeparator"));
				sb.Append(main.Get("ItemSelectorNext"));
				sb.Append(' ');
				GameInput.AppendDisplayText(buttonsNext, sb);
				sb.Append('\n');
				sb.Append(main.Get("ItemSelectorSelect"));
				sb.Append(' ');
				GameInput.AppendDisplayText(GameInput.button0, sb, allBindingSets: true);
				sb.Append(Language.main.Get("InputSeparator"));
				sb.Append(main.Get("ItemSelectorCancel"));
				sb.Append(' ');
				GameInput.AppendDisplayText(GameInput.button1, sb, allBindingSets: true);
				HandReticle main2 = HandReticle.main;
				main2.SetTextRaw(HandReticle.TextType.Use, string.Empty);
				main2.SetTextRaw(HandReticle.TextType.UseSubscript, sb.ToString());
			}
		}
	}

	public void Initialize(IItemSelectorManager manager, Sprite defaultSprite, List<IItemsContainer> containers)
	{
		ResetSelector();
		if (manager == null || containers == null || containers.Count == 0)
		{
			return;
		}
		this.manager = manager;
		this.defaultSprite = defaultSprite;
		srcContainers = containers;
		for (int i = 0; i < srcContainers.Count; i++)
		{
			IItemsContainer itemsContainer = srcContainers[i];
			foreach (InventoryItem item in itemsContainer)
			{
				if (manager.Filter(item))
				{
					items.Add(item);
				}
			}
			itemsContainer.onAddItem += OnAddItem;
			itemsContainer.onRemoveItem += OnRemoveItem;
		}
		used = 1 + manager.Sort(items);
		selected = used;
		CreateIcons();
		UpdateInfoText();
		SetState(active: true, immediately: true);
		InputHandlerStack.main.Push(this);
		FPSInputModule.current.lockMovement = true;
		sequence.Set(5f, current: false, target: true);
	}

	public static bool HasCompatibleItems(IItemSelectorManager manager, List<IItemsContainer> containers)
	{
		for (int i = 0; i < containers.Count; i++)
		{
			foreach (InventoryItem item in containers[i])
			{
				if (manager.Filter(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ResetSelector()
	{
		sequence.Reset();
		ClearIcons();
		items.Clear();
		selected = -1;
		if (srcContainers != null)
		{
			for (int i = 0; i < srcContainers.Count; i++)
			{
				IItemsContainer itemsContainer = srcContainers[i];
				itemsContainer.onAddItem -= OnAddItem;
				itemsContainer.onRemoveItem -= OnRemoveItem;
			}
			srcContainers = null;
		}
		manager = null;
		defaultSprite = null;
		FPSInputModule.current.lockMovement = false;
	}

	private void ClearIcons()
	{
		int i = 0;
		for (int count = icons.Count; i < count; i++)
		{
			Object.Destroy(icons[i].gameObject);
		}
		icons.Clear();
	}

	private void OnAddItem(InventoryItem item)
	{
		if (manager.Filter(item))
		{
			ClearIcons();
			items.Add(item);
			used = 1 + manager.Sort(items);
			selected = used;
			CreateIcons();
		}
	}

	private void OnRemoveItem(InventoryItem item)
	{
		int num = items.IndexOf(item);
		if (num != -1)
		{
			ClearIcons();
			items.RemoveAt(num);
			used = 1 + manager.Sort(items);
			selected = used;
			CreateIcons();
		}
	}

	private void CreateIcons()
	{
		uGUI_ItemIcon item = CreateIcon((defaultSprite != null) ? defaultSprite : SpriteManager.defaultSprite, SpriteManager.GetBackground(CraftData.BackgroundType.Normal));
		icons.Add(item);
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			InventoryItem inventoryItem = items[i];
			TechType techType = inventoryItem.item.GetTechType();
			item = CreateIcon(SpriteManager.Get(techType), SpriteManager.GetBackground(techType));
			item.SetBarValue(TooltipFactory.GetBarValue(inventoryItem));
			icons.Add(item);
		}
		int j = 0;
		for (int count2 = icons.Count; j < count2; j++)
		{
			item = icons[j];
			GetIconPositionScale(j, out var x, out var s);
			item.SetPosition(x, 0f);
			item.SetScale(s, s);
		}
		if (used >= 0)
		{
			icons[used].SetForegroundColors(Color.green, Color.green, Color.green);
		}
	}

	private void GetIconPositionScale(int i, out float x, out float s)
	{
		float height = canvas.rect.height;
		float num = 1.5f * height;
		x = (float)(i - selected) * num;
		s = Mathf.Lerp(1f, 0.3f, Mathf.Abs(x / (3f * num)));
	}

	private uGUI_ItemIcon CreateIcon(Sprite foreground, Sprite background)
	{
		uGUI_ItemIcon obj = new GameObject("ItemIcon").AddComponent<uGUI_ItemIcon>();
		obj.Init(null, canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
		obj.SetForegroundSprite(foreground);
		obj.SetBackgroundSprite(background);
		float height = canvas.rect.height;
		obj.SetSize(height, height);
		float num = height * 0.5f;
		obj.SetBackgroundRadius(num);
		obj.SetBarRadius(num);
		return obj;
	}

	private InventoryItem GetSelectedItem()
	{
		if (selected >= 1)
		{
			return items[selected - 1];
		}
		return null;
	}

	private void UpdateInfoText()
	{
		text.text = manager.GetText(GetSelectedItem());
	}

	private void SetState(bool active, bool immediately)
	{
		if (active)
		{
			if (immediately)
			{
				canvasGroup.alpha = 1f;
			}
		}
		else if (immediately)
		{
			canvasGroup.alpha = 0f;
		}
		rectMask.enabled = active;
	}

	public bool HandleInput()
	{
		if (manager == null || GameInput.GetButtonDown(GameInput.Button.Reload))
		{
			ResetSelector();
			SetState(active: false, immediately: true);
			return false;
		}
		if (GameInputExtensions.GetButtonDown(buttonsNext))
		{
			selected++;
			if (selected >= icons.Count)
			{
				selected = 0;
			}
			UpdateInfoText();
			sequence.Set(5f, current: false, target: true);
		}
		else if (GameInputExtensions.GetButtonDown(buttonsPrevious))
		{
			selected--;
			if (selected < 0)
			{
				selected = icons.Count - 1;
			}
			UpdateInfoText();
			sequence.Set(5f, current: false, target: true);
		}
		else
		{
			if (GameInput.GetButtonDown(GameInput.button0))
			{
				manager.Select(GetSelectedItem());
				ResetSelector();
				GameInput.ClearInput();
				return false;
			}
			if (GameInput.GetButtonDown(GameInput.button1))
			{
				ResetSelector();
				GameInput.ClearInput();
				return false;
			}
		}
		return true;
	}

	public bool HandleLateInput()
	{
		return true;
	}

	public void OnFocusChanged(InputFocusMode mode)
	{
		switch (mode)
		{
		case InputFocusMode.Remove:
			ResetSelector();
			SetState(active: false, immediately: true);
			break;
		case InputFocusMode.Add:
		case InputFocusMode.Suspend:
		case InputFocusMode.Restore:
			break;
		}
	}
}
