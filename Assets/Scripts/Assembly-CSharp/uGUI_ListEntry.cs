using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class uGUI_ListEntry : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, INotificationTarget, ISelectable
{
	private static readonly UIAnchor notificationLabelAnchor = UIAnchor.MiddleRight;

	private static readonly Vector2 notificationLabelOffset = new Vector2(-20f, 0f);

	[AssertNotNull]
	public Image icon;

	[AssertNotNull]
	public LayoutElement layoutElement;

	[AssertNotNull]
	public TextMeshProUGUI text;

	[AssertNotNull]
	public Image background;

	private uGUI_NotificationLabel notification;

	private string key;

	private uGUI_IListEntryManager manager;

	private float minIndent;

	private bool hovered;

	private bool selected;

	private UISpriteData spriteData;

	public string Key => key;

	public RectTransform rectTransform { get; private set; }

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		minIndent = layoutElement.minWidth;
	}

	private void OnEnable()
	{
		UpdateSprite();
	}

	private void OnRectTransformDimensionsChange()
	{
		UpdateNotificationPosition();
	}

	public void Initialize(uGUI_IListEntryManager manager, string key, UISpriteData spriteData)
	{
		base.gameObject.SetActive(value: true);
		this.manager = manager;
		this.key = key;
		this.spriteData = spriteData;
		if (spriteData != null)
		{
			background.sprite = spriteData.normal;
		}
	}

	public void Uninitialize()
	{
		base.gameObject.SetActive(value: false);
		manager = null;
		key = null;
	}

	public void SetSelected(bool state)
	{
		if (selected != state)
		{
			selected = state;
			UpdateSprite();
		}
	}

	public void SetIcon(Sprite icon)
	{
		this.icon.sprite = icon;
		this.icon.enabled = icon != null;
	}

	public void SetIndent(float indent)
	{
		float a = (icon.enabled ? minIndent : 0f);
		layoutElement.minWidth = Mathf.Max(a, indent);
	}

	public void SetText(string text)
	{
		this.text.text = text;
	}

	public bool SetNotificationAlpha(float alpha)
	{
		bool result = false;
		alpha = Mathf.Clamp01(alpha);
		if (alpha > 0f)
		{
			if (notification == null)
			{
				notification = uGUI_NotificationLabel.CreateInstance(rectTransform);
				notification.rectTransform.SetAsLastSibling();
				UpdateNotificationPosition();
				result = true;
			}
			notification.SetAlpha(alpha);
		}
		else if (notification != null)
		{
			UnityEngine.Object.Destroy(notification.gameObject);
		}
		return result;
	}

	public void SetNotificationBackgroundColor(Color color)
	{
		if (notification != null)
		{
			notification.SetBackgroundColor(color);
		}
	}

	public void SetNotificationText(string text)
	{
		if (notification != null)
		{
			notification.SetText(text);
		}
	}

	private void UpdateSprite()
	{
		if (spriteData != null)
		{
			Sprite sprite = ((!hovered) ? (selected ? spriteData.selected : spriteData.normal) : (selected ? spriteData.selectedHovered : spriteData.hovered));
			background.sprite = sprite;
		}
	}

	private void UpdateNotificationPosition()
	{
		if (notification != null)
		{
			notification.rectTransform.localPosition = Vector2.Scale(rectTransform.rect.size, RectTransformExtensions.GetAnchorPoint(notificationLabelAnchor) - rectTransform.pivot) + notificationLabelOffset;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hovered = true;
		UpdateSprite();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hovered = false;
		UpdateSprite();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		eventData?.Use();
		if (manager != null)
		{
			switch (eventData.button)
			{
			case PointerEventData.InputButton.Left:
				manager.OnButtonDown(key, GameInput.Button.UISubmit);
				break;
			case PointerEventData.InputButton.Right:
				manager.OnButtonDown(key, GameInput.Button.UICancel);
				break;
			}
		}
	}

	public bool IsVisible()
	{
		if (this != null && base.isActiveAndEnabled)
		{
			return selected;
		}
		return false;
	}

	public bool IsDestroyed()
	{
		return this == null;
	}

	public void Progress(float progress)
	{
		float notificationAlpha = Mathf.Sin(progress * ((float)Math.PI / 2f));
		if (SetNotificationAlpha(notificationAlpha))
		{
			SetNotificationBackgroundColor(NotificationManager.notificationColor);
			SetNotificationText("+");
		}
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
		return rectTransform;
	}

	bool ISelectable.OnButtonDown(GameInput.Button button)
	{
		if (manager != null)
		{
			return manager.OnButtonDown(key, button);
		}
		return false;
	}
}
