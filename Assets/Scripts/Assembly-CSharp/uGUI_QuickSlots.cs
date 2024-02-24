using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class uGUI_QuickSlots : MonoBehaviour, uGUI_IIconManager, uGUI_INavigableIconGrid
{
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

	private static readonly GameInput.Button[] buttonsTemp = new GameInput.Button[2];

	[AssertLocalization]
	private const string previousKey = "QuickslotPrevious";

	[AssertLocalization]
	private const string nextKey = "QuickslotNext";

	[AssertLocalization]
	private const string assignKey = "QuickslotAssign";

	[AssertLocalization]
	private const string clearKey = "QuickslotClear";

	[AssertLocalization]
	private const string backKey = "QuickslotBack";

	private static readonly Vector2 iconStep = new Vector2(58f, 58f);

	public static readonly Vector2 foregroundSize = new Vector2(58f, 58f);

	public static readonly Vector2 backgroundSize = new Vector2(60f, 60f);

	private static readonly Vector2 activeSize = new Vector2(66f, 66f);

	private const float iconSpace = 8f;

	[AssertNotNull]
	public Material materialBackground;

	[AssertNotNull]
	public Sprite spriteLeft;

	[AssertNotNull]
	public Sprite spriteCenter;

	[AssertNotNull]
	public Sprite spriteRight;

	[AssertNotNull]
	public Sprite spriteNormal;

	[AssertNotNull]
	public Sprite spriteHighlighted;

	[AssertNotNull]
	public Sprite spriteExosuitArm;

	[AssertNotNull]
	public Sprite spriteSelected;

	private RectTransform _rectTransform;

	private IQuickSlots overrideTarget;

	private IQuickSlots target;

	private uGUI_ItemIcon[] icons;

	private Image[] backgrounds;

	private bool unbindOnEndDrag;

	private Image _selector;

	private static readonly GameInput.Button[] quickSlotButtons = new GameInput.Button[5]
	{
		GameInput.Button.Slot1,
		GameInput.Button.Slot2,
		GameInput.Button.Slot3,
		GameInput.Button.Slot4,
		GameInput.Button.Slot5
	};

	private RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	private Image selector
	{
		get
		{
			if (_selector == null)
			{
				GameObject gameObject = new GameObject("QuickSlot Selector");
				gameObject.layer = base.gameObject.layer;
				_selector = gameObject.AddComponent<Image>();
				_selector.sprite = spriteSelected;
				RectTransform obj = _selector.rectTransform;
				RectTransformExtensions.SetParams(obj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), rectTransform);
				obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, spriteSelected.rect.width);
				obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, spriteSelected.rect.height);
				_selector.enabled = false;
			}
			return _selector;
		}
	}

	public static float radius => Mathf.Min(backgroundSize.x, backgroundSize.y) * 0.5f;

	bool uGUI_INavigableIconGrid.ShowSelector => true;

	bool uGUI_INavigableIconGrid.EmulateRaycast => true;

	private void Start()
	{
		GameInput.OnPrimaryDeviceChanged += OnPrimaryDeviceChanged;
	}

	private void OnDestroy()
	{
		GameInput.OnPrimaryDeviceChanged -= OnPrimaryDeviceChanged;
	}

	private void Update()
	{
		IQuickSlots quickSlots = GetTarget();
		if (target != quickSlots)
		{
			target = quickSlots;
			Init(target);
		}
		if (target != null)
		{
			HandleInput();
			int i = 0;
			for (int num = icons.Length; i < num; i++)
			{
				uGUI_ItemIcon uGUI_ItemIcon2 = icons[i];
				if (!(uGUI_ItemIcon2 == null))
				{
					float slotProgress = target.GetSlotProgress(i);
					float slotCharge = target.GetSlotCharge(i);
					InventoryItem slotItem = target.GetSlotItem(i);
					uGUI_ItemIcon2.SetBarValue(TooltipFactory.GetBarValue(slotItem));
					if (slotProgress < 1f)
					{
						uGUI_ItemIcon2.SetProgress(slotProgress);
					}
					else if (slotCharge > 0f)
					{
						uGUI_ItemIcon2.SetProgress(slotCharge, FillMethod.Vertical);
					}
					else
					{
						uGUI_ItemIcon2.SetProgress(1f, FillMethod.None);
					}
				}
			}
		}
		Inventory main = Inventory.main;
		if (!(main != null))
		{
			return;
		}
		int assignSlot = main.quickSlots.assignSlot;
		if (assignSlot >= 0)
		{
			uGUI_ItemIcon icon = GetIcon(assignSlot);
			RectTransform rectTransform = icon.rectTransform;
			ItemDragManager.DragSnap(icon.GetInstanceID(), rectTransform.TransformPoint(rectTransform.rect.center), rectTransform.rotation, rectTransform.lossyScale, foregroundSize, backgroundSize, radius);
			using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
			{
				StringBuilder sb = stringBuilderPool.sb;
				Language main2 = Language.main;
				sb.Append(main2.Get("QuickslotPrevious"));
				sb.Append(' ');
				GameInput.AppendDisplayText(buttonsPrevious, sb);
				sb.Append(Language.main.Get("InputSeparator"));
				sb.Append(main2.Get("QuickslotNext"));
				sb.Append(' ');
				GameInput.AppendDisplayText(buttonsNext, sb);
				sb.Append('\n');
				sb.Append(main2.Get("QuickslotAssign"));
				sb.Append(' ');
				buttonsTemp[0] = GameInput.button0;
				buttonsTemp[1] = GameInput.button3;
				GameInput.AppendDisplayText(buttonsTemp, sb);
				sb.Append(Language.main.Get("InputSeparator"));
				sb.Append(main2.Get("QuickslotClear"));
				sb.Append(' ');
				GameInput.AppendDisplayText(GameInput.button2, sb);
				sb.Append(Language.main.Get("InputSeparator"));
				sb.Append(main2.Get("QuickslotBack"));
				sb.Append(' ');
				GameInput.AppendDisplayText(GameInput.button1, sb);
				HandReticle main3 = HandReticle.main;
				main3.SetTextRaw(HandReticle.TextType.Use, string.Empty);
				main3.SetTextRaw(HandReticle.TextType.UseSubscript, sb.ToString());
			}
		}
	}

	public void SetTarget(IQuickSlots newTarget)
	{
		overrideTarget = newTarget;
	}

	private void Init(IQuickSlots newTarget)
	{
		Uninit();
		if (newTarget != null)
		{
			target = newTarget;
			TechType[] slotBinding = target.GetSlotBinding();
			int num = slotBinding.Length;
			backgrounds = new Image[num];
			icons = new uGUI_ItemIcon[num];
			for (int i = 0; i < num; i++)
			{
				TechType techType = slotBinding[i];
				InventoryItem slotItem = target.GetSlotItem(i);
				Vector2 position = GetPosition(i);
				Image image = new GameObject("QuickSlot Background")
				{
					layer = base.gameObject.layer
				}.AddComponent<Image>();
				Sprite sprite = null;
				sprite = ((num != 1) ? ((i != 0) ? ((i != num - 1) ? spriteCenter : spriteRight) : spriteLeft) : spriteCenter);
				image.material = materialBackground;
				image.sprite = sprite;
				RectTransform obj = image.rectTransform;
				RectTransformExtensions.SetParams(obj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), rectTransform);
				obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sprite.rect.width);
				obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sprite.rect.height);
				obj.anchoredPosition = position;
				backgrounds[i] = image;
				uGUI_ItemIcon uGUI_ItemIcon2 = new GameObject("QuickSlot Icon")
				{
					layer = base.gameObject.layer
				}.AddComponent<uGUI_ItemIcon>();
				uGUI_ItemIcon2.Init(this, rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
				SetForeground(uGUI_ItemIcon2, techType);
				SetBackground(uGUI_ItemIcon2, techType, target.IsToggled(i));
				uGUI_ItemIcon2.SetBackgroundBlending(Blending.Additive);
				uGUI_ItemIcon2.SetActiveSize(activeSize.x, activeSize.y);
				uGUI_ItemIcon2.SetForegroundSize(foregroundSize);
				uGUI_ItemIcon2.SetBackgroundSize(-1f, -1f);
				uGUI_ItemIcon2.SetBarSize(backgroundSize);
				uGUI_ItemIcon2.SetBarRadius(radius);
				uGUI_ItemIcon2.SetBarValue(TooltipFactory.GetBarValue(slotItem));
				uGUI_ItemIcon2.SetPosition(position.x, position.y);
				icons[i] = uGUI_ItemIcon2;
			}
			int num2 = 0;
			for (int j = 0; j < num; j++)
			{
				backgrounds[j].rectTransform.SetSiblingIndex(num2);
				num2++;
			}
			selector.rectTransform.SetSiblingIndex(num2);
			num2++;
			for (int k = 0; k < num; k++)
			{
				icons[k].rectTransform.SetSiblingIndex(num2);
				num2++;
			}
			OnSelect(target.GetActiveSlotID());
			target.onBind += OnBind;
			target.onToggle += OnToggle;
			target.onSelect += OnSelect;
		}
	}

	private void Uninit()
	{
		if (target != null)
		{
			target.onBind -= OnBind;
			target.onToggle -= OnToggle;
			target.onSelect -= OnSelect;
			target = null;
		}
		selector.enabled = false;
		if (backgrounds != null)
		{
			for (int num = backgrounds.Length - 1; num >= 0; num--)
			{
				Image image = backgrounds[num];
				if (!(image == null))
				{
					Object.Destroy(image.gameObject);
				}
			}
			backgrounds = null;
		}
		if (icons == null)
		{
			return;
		}
		for (int num2 = icons.Length - 1; num2 >= 0; num2--)
		{
			uGUI_ItemIcon uGUI_ItemIcon2 = icons[num2];
			if (!(uGUI_ItemIcon2 == null))
			{
				Object.Destroy(uGUI_ItemIcon2.gameObject);
			}
		}
		icons = null;
	}

	private void HandleInput()
	{
		if (!Player.main.GetCanItemBeUsed())
		{
			return;
		}
		bool flag = uGUI.isIntro || IntroLifepodDirector.IsActive;
		if (!flag)
		{
			int i = 0;
			for (int num = quickSlotButtons.Length; i < num; i++)
			{
				GameInput.Button action = quickSlotButtons[i];
				if (GameInput.GetButtonDown(action))
				{
					target.SlotKeyDown(i);
				}
				else if (GameInput.GetButtonHeld(action))
				{
					target.SlotKeyHeld(i);
				}
				if (GameInput.GetButtonUp(action))
				{
					target.SlotKeyUp(i);
				}
			}
			if (GameInput.GetButtonDown(GameInput.Button.CycleNext))
			{
				target.SlotNext();
			}
			else if (GameInput.GetButtonDown(GameInput.Button.CyclePrev))
			{
				target.SlotPrevious();
			}
		}
		if (AvatarInputHandler.main != null && AvatarInputHandler.main.IsEnabled())
		{
			if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
			{
				target.SlotLeftDown();
			}
			else if (GameInput.GetButtonHeld(GameInput.Button.LeftHand))
			{
				target.SlotLeftHeld();
			}
			if (GameInput.GetButtonUp(GameInput.Button.LeftHand))
			{
				target.SlotLeftUp();
			}
			if (GameInput.GetButtonDown(GameInput.Button.RightHand))
			{
				target.SlotRightDown();
			}
			else if (GameInput.GetButtonHeld(GameInput.Button.RightHand))
			{
				target.SlotRightHeld();
			}
			if (GameInput.GetButtonUp(GameInput.Button.RightHand))
			{
				target.SlotRightUp();
			}
			if (!flag && GameInput.GetButtonDown(GameInput.Button.Exit))
			{
				target.DeselectSlots();
			}
		}
	}

	private void OnBind(int slotID, TechType techType, bool state)
	{
		if (target != null)
		{
			uGUI_ItemIcon icon = GetIcon(slotID);
			if (state)
			{
				SetForeground(icon, techType);
				SetBackground(icon, techType, target.IsToggled(slotID));
				icon.SetBarValue(TooltipFactory.GetBarValue(target.GetSlotItem(slotID)));
			}
			else
			{
				SetForeground(icon, TechType.None);
				SetBackground(icon, TechType.None, highlighted: false);
			}
		}
	}

	private void OnToggle(int slotID, bool state)
	{
		if (target != null)
		{
			SetBackground(GetIcon(slotID), target.GetSlotBinding(slotID), state);
		}
	}

	private void OnSelect(int slotID)
	{
		if (target != null)
		{
			if (slotID < 0)
			{
				selector.enabled = false;
				return;
			}
			selector.rectTransform.anchoredPosition = GetPosition(slotID);
			selector.enabled = true;
		}
	}

	private void SetForeground(uGUI_ItemIcon icon, TechType techType)
	{
		if (!(icon == null))
		{
			bool flag = techType != TechType.None;
			icon.SetForegroundSprite(flag ? SpriteManager.Get(techType) : null);
		}
	}

	private void SetBackground(uGUI_ItemIcon icon, TechType techType, bool highlighted)
	{
		if (!(icon == null))
		{
			Sprite backgroundSprite = (highlighted ? spriteHighlighted : spriteNormal);
			if (techType != 0 && TechData.GetEquipmentType(techType) == EquipmentType.ExosuitArm)
			{
				backgroundSprite = spriteExosuitArm;
			}
			icon.SetBackgroundSprite(backgroundSprite);
		}
	}

	public uGUI_ItemIcon GetIcon(int slotID)
	{
		if (icons == null || slotID < 0 || slotID >= icons.Length)
		{
			return null;
		}
		return icons[slotID];
	}

	private int GetSlot(uGUI_ItemIcon icon)
	{
		if (icons != null)
		{
			for (int i = 0; i < icons.Length; i++)
			{
				if (icons[i] == icon)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public void GetTooltip(uGUI_ItemIcon icon, TooltipData data)
	{
		if (target == null)
		{
			return;
		}
		int slot = GetSlot(icon);
		TechType slotBinding = target.GetSlotBinding(slot);
		if (slotBinding != 0)
		{
			InventoryItem slotItem = target.GetSlotItem(slot);
			if (slotItem != null && slotItem.item != null)
			{
				TooltipFactory.QuickSlot(slotBinding, slotItem.item.gameObject, data);
			}
		}
	}

	public void OnPointerEnter(uGUI_ItemIcon icon)
	{
	}

	public void OnPointerExit(uGUI_ItemIcon icon)
	{
	}

	public bool OnPointerClick(uGUI_ItemIcon icon, int button)
	{
		unbindOnEndDrag = false;
		QuickSlots quickSlots = Inventory.main.quickSlots;
		int assignSlot = quickSlots.assignSlot;
		if (assignSlot >= 0)
		{
			switch (button)
			{
			case 0:
			case 3:
				quickSlots.EndAssign(apply: true);
				return true;
			case 2:
			{
				bool flag = quickSlots.GetSlotByItem(quickSlots.assignItem) == assignSlot;
				quickSlots.Unbind(quickSlots.assignSlot);
				if (flag)
				{
					quickSlots.EndAssign();
				}
				return true;
			}
			case 1:
				quickSlots.EndAssign();
				return true;
			}
		}
		return true;
	}

	public bool OnBeginDrag(uGUI_ItemIcon icon)
	{
		if (target == null)
		{
			return true;
		}
		int slot = GetSlot(icon);
		InventoryItem slotItem = target.GetSlotItem(slot);
		int instanceID = icon.GetInstanceID();
		RectTransform rectTransform = icon.rectTransform;
		if (ItemDragManager.DragStart(slotItem, icon, instanceID, rectTransform.position, rectTransform.rotation, rectTransform.lossyScale, foregroundSize, backgroundSize, radius))
		{
			unbindOnEndDrag = true;
		}
		return true;
	}

	public void OnEndDrag(uGUI_ItemIcon icon)
	{
		ItemDragManager.DragStop();
		if (unbindOnEndDrag)
		{
			unbindOnEndDrag = false;
			if (target != null)
			{
				int slot = GetSlot(icon);
				target.Unbind(slot);
			}
		}
	}

	public void OnDrop(uGUI_ItemIcon icon)
	{
		if (target == null || !ItemDragManager.isDragging)
		{
			return;
		}
		unbindOnEndDrag = false;
		InventoryItem draggedItem = ItemDragManager.draggedItem;
		ItemDragManager.DragStop();
		if (draggedItem == null || !Inventory.main.GetCanBindItem(draggedItem))
		{
			return;
		}
		int slot = GetSlot(icon);
		InventoryItem slotItem = target.GetSlotItem(slot);
		if (draggedItem == slotItem)
		{
			return;
		}
		if (slotItem == null)
		{
			target.Bind(slot, draggedItem);
			return;
		}
		int slotByItem = target.GetSlotByItem(draggedItem);
		if (slotByItem == -1)
		{
			target.Bind(slot, draggedItem);
		}
		else if (slotByItem != slot)
		{
			target.Bind(slot, draggedItem);
			target.Bind(slotByItem, slotItem);
		}
	}

	public void OnDragHoverEnter(uGUI_ItemIcon icon)
	{
	}

	public void OnDragHoverStay(uGUI_ItemIcon icon)
	{
		if (target == null)
		{
			return;
		}
		bool flag = false;
		if (ItemDragManager.isDragging)
		{
			InventoryItem draggedItem = ItemDragManager.draggedItem;
			if (draggedItem != null && Inventory.main.GetCanBindItem(draggedItem) && Inventory.main.quickSlots.GetSlotByItem(draggedItem) != GetSlot(icon))
			{
				flag = true;
			}
		}
		if (flag)
		{
			RectTransform rectTransform = icon.rectTransform;
			ItemDragManager.DragSnap(icon.GetInstanceID(), rectTransform.position, rectTransform.rotation, rectTransform.lossyScale, foregroundSize, backgroundSize, radius);
		}
	}

	public void OnDragHoverExit(uGUI_ItemIcon icon)
	{
	}

	public bool OnButtonDown(uGUI_ItemIcon icon, GameInput.Button button)
	{
		return false;
	}

	public void OnPrimaryDeviceChanged()
	{
		Inventory main = Inventory.main;
		if (main != null)
		{
			main.quickSlots.EndAssign();
		}
	}

	private Vector2 GetPosition(int slotID)
	{
		if (icons == null)
		{
			return new Vector2(0f, 0f);
		}
		float num = iconStep.x + 8f;
		return new Vector2(-0.5f * (float)(icons.Length - 1) * num + (float)slotID * num, 0f);
	}

	private IQuickSlots GetTarget()
	{
		if (!uGUI.isMainLevel)
		{
			return null;
		}
		if (uGUI.isIntro)
		{
			return null;
		}
		if (LaunchRocket.isLaunching)
		{
			return null;
		}
		Player main = Player.main;
		if (main == null)
		{
			return null;
		}
		if (main.GetMode() == Player.Mode.Piloting || main.cinematicModeActive)
		{
			return null;
		}
		uGUI_CameraDrone main2 = uGUI_CameraDrone.main;
		if (main2 != null && main2.GetCamera() != null)
		{
			return null;
		}
		if (overrideTarget != null)
		{
			return overrideTarget;
		}
		Inventory main3 = Inventory.main;
		if (main3 != null)
		{
			return main3.quickSlots;
		}
		return null;
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
		if (!(uGUI_ItemIcon2 == null))
		{
			UISelection.selected = uGUI_ItemIcon2;
		}
	}

	public void DeselectItem()
	{
		if (UISelection.selected != null)
		{
			UISelection.selected = null;
		}
	}

	public bool SelectFirstItem()
	{
		QuickSlots quickSlots = Inventory.main.quickSlots;
		int assignSlot = quickSlots.assignSlot;
		if (assignSlot < 0)
		{
			quickSlots.EndAssign();
			return false;
		}
		SelectItem(GetIcon(assignSlot));
		return true;
	}

	public bool SelectItemClosestToPosition(Vector3 worldPos)
	{
		return false;
	}

	public bool SelectItemInDirection(int dirX, int dirY)
	{
		if (UISelection.selected == null || !UISelection.selected.IsValid())
		{
			return SelectFirstItem();
		}
		int num = ((dirX != 0) ? dirX : (-dirY));
		if (num == 0)
		{
			return false;
		}
		Inventory.main.quickSlots.NavigateAssign(num);
		return SelectFirstItem();
	}

	public uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY)
	{
		return null;
	}
}
