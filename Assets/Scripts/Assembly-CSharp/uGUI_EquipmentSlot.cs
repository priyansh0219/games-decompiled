using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class uGUI_EquipmentSlot : Graphic, ITooltip, IDropHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDragHoverHandler, INotificationTarget, ISelectable
{
	public enum State
	{
		Invalid = -1,
		Default = 0,
		Highlighted = 1,
		Allowed = 2,
		Denied = 3,
		Selected = 4
	}

	private Color32 tintDefault = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 127);

	private Color32 tintHighlighted = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private Color32 tintSelected = new Color32(0, 250, byte.MaxValue, byte.MaxValue);

	private Color32 tintAllowed = new Color32(0, byte.MaxValue, 0, byte.MaxValue);

	private Color32 tintDenied = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	public string slot;

	[AssertNotNull]
	public RectTransform iconRect;

	[AssertNotNull]
	public Image background;

	public GameObject hint;

	[HideInInspector]
	public uGUI_Equipment manager;

	private bool active = true;

	private uGUI_ItemIcon _icon;

	private State state = State.Invalid;

	private bool compatible;

	private bool hovered;

	private bool selected;

	public uGUI_ItemIcon icon
	{
		get
		{
			if (_icon == null)
			{
				GameObject gameObject = new GameObject("Equipment Slot Icon");
				gameObject.layer = base.gameObject.layer;
				_icon = gameObject.AddComponent<uGUI_ItemIcon>();
				_icon.Init(null, iconRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
				_icon.raycastTarget = false;
				_icon.SetPosition(Vector2.zero);
				_icon.SetAsLastSibling();
			}
			return _icon;
		}
	}

	public bool showTooltipOnDrag => false;

	protected override void Awake()
	{
		UpdateFeedbackState();
	}

	public void SetActive(bool value)
	{
		if (active != value)
		{
			active = value;
			base.gameObject.SetActive(active);
		}
	}

	public float GetBackgroundRadius()
	{
		Vector2 size = iconRect.rect.size;
		return Mathf.Min(size.x, size.y) * 0.5f;
	}

	public void SetItem(InventoryItem item)
	{
		if (item == null)
		{
			return;
		}
		Pickupable item2 = item.item;
		if (!(item2 == null))
		{
			TechType techType = item2.GetTechType();
			uGUI_ItemIcon obj = icon;
			obj.SetForegroundSprite(SpriteManager.Get(techType));
			obj.SetBackgroundSprite(SpriteManager.GetBackground(techType));
			Vector2 size = iconRect.rect.size;
			float backgroundRadius = GetBackgroundRadius();
			obj.SetBackgroundRadius(backgroundRadius);
			obj.SetBarRadius(backgroundRadius);
			obj.SetForegroundAlpha(1f);
			obj.SetSize(size);
			obj.SetActive(active: true);
			obj.SetBarValue(TooltipFactory.GetBarValue(item));
			if (hint != null)
			{
				hint.SetActive(value: false);
			}
		}
	}

	public void ClearIcon()
	{
		if (_icon != null)
		{
			_icon.SetActive(active: false);
		}
		if (hint != null)
		{
			hint.SetActive(value: true);
		}
	}

	public void MarkCompatible(bool state)
	{
		if (compatible != state)
		{
			compatible = state;
			UpdateFeedbackState();
		}
	}

	public void SetSelected(bool state)
	{
		selected = state;
		UpdateFeedbackState();
	}

	private void OnPointerClick(int button)
	{
		if (manager != null)
		{
			manager.OnPointerClick(this, button);
		}
		UpdateFeedbackState();
	}

	private void UpdateFeedbackState()
	{
		State state = State.Default;
		bool isDragging = ItemDragManager.isDragging;
		state = (selected ? State.Selected : (compatible ? ((!isDragging || !hovered) ? State.Highlighted : State.Allowed) : ((isDragging && hovered) ? State.Denied : State.Default)));
		SetState(state);
	}

	private void SetState(State newState)
	{
		if (state != newState)
		{
			state = newState;
			Color32 color;
			switch (state)
			{
			case State.Selected:
				color = tintSelected;
				break;
			case State.Highlighted:
				color = tintHighlighted;
				break;
			case State.Allowed:
				color = tintAllowed;
				break;
			case State.Denied:
				color = tintDenied;
				break;
			default:
				color = tintDefault;
				break;
			}
			background.color = color;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hovered = true;
		if (manager != null)
		{
			manager.OnPointerEnter(this);
		}
		UpdateFeedbackState();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hovered = false;
		if (manager != null)
		{
			manager.OnPointerExit(this);
		}
		UpdateFeedbackState();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int button = -1;
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			button = 0;
			break;
		case PointerEventData.InputButton.Right:
			button = 1;
			break;
		case PointerEventData.InputButton.Middle:
			button = 2;
			break;
		}
		OnPointerClick(button);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnBeginDrag(this);
		}
		UpdateFeedbackState();
	}

	public void OnDrag(PointerEventData eventData)
	{
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnEndDrag();
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnDrop(slot);
		}
		UpdateFeedbackState();
	}

	public void OnDragHoverEnter(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnDragHoverEnter(slot);
		}
	}

	public void OnDragHoverStay(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnDragHoverStay(slot);
		}
	}

	public void OnDragHoverExit(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.OnDragHoverExit(slot);
		}
	}

	void ITooltip.GetTooltip(TooltipData data)
	{
		if (manager != null)
		{
			manager.GetTooltip(this, data);
		}
	}

	bool INotificationTarget.IsVisible()
	{
		return ((INotificationTarget)icon).IsVisible();
	}

	void INotificationTarget.Progress(float progress)
	{
		if (icon.SetNotificationProgress(progress))
		{
			icon.SetNotificationBackgroundColor(NotificationManager.notificationColor);
			icon.SetNotificationOffset(iconRect.rect.size * -0.1464466f);
		}
	}

	bool INotificationTarget.IsDestroyed()
	{
		return this == null;
	}

	bool ISelectable.IsValid()
	{
		if (this != null)
		{
			return base.isActiveAndEnabled;
		}
		return false;
	}

	RectTransform ISelectable.GetRect()
	{
		return base.rectTransform;
	}

	bool ISelectable.OnButtonDown(GameInput.Button button)
	{
		if (manager == null)
		{
			return false;
		}
		switch (button)
		{
		case GameInput.Button.UISubmit:
			manager.OnPointerClick(this, 0);
			return true;
		case GameInput.Button.UICancel:
			manager.OnPointerClick(this, 1);
			return true;
		default:
			return false;
		}
	}
}
