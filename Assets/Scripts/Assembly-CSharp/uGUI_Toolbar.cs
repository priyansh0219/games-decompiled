using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class uGUI_Toolbar : MonoBehaviour, uGUI_IIconManager, uGUI_INavigableIconGrid
{
	[AssertNotNull]
	public Sprite backgroundSpriteNormal;

	[AssertNotNull]
	public Sprite backgroundSpriteSelected;

	public RectTransform selectionMarker;

	public Vector2 backgroundSize = new Vector2(100f, 50f);

	public Vector2 foregroundSize = new Vector2(50f, 50f);

	public Vector2 buttonSize = new Vector2(90f, 90f);

	public float buttonStep = 4f;

	public Vector2 selectionMarkerOffset = new Vector2(0f, 0f);

	public UIAnchor notificationAnchor = UIAnchor.UpperRight;

	public Vector2 notificationOffset = new Vector2(-10f, -10f);

	public TextMeshProUGUI cycleRightBtnText;

	public TextMeshProUGUI cycleLeftBtnText;

	public Font font;

	public int fontSize;

	public uGUI_InterGridNavigation interGridNavigation;

	private int selected = -1;

	private int navigationSelected = -1;

	private RectTransform rt;

	private GameObject go;

	private uGUI_IToolbarManager manager;

	private List<uGUI_ItemIcon> icons;

	bool uGUI_INavigableIconGrid.ShowSelector => true;

	bool uGUI_INavigableIconGrid.EmulateRaycast => true;

	public void OnEnable()
	{
		GameInput.OnPrimaryDeviceChanged += OnPrimaryDeviceChanged;
		GameInput.OnBindingsChanged += OnBindingsChanged;
		UpdateCycleBtnText();
	}

	public void OnDisable()
	{
		GameInput.OnBindingsChanged -= OnBindingsChanged;
		GameInput.OnPrimaryDeviceChanged -= OnPrimaryDeviceChanged;
	}

	public void Initialize(uGUI_IToolbarManager manager, object[] content, Font font = null, int fontSize = 15)
	{
		go = base.gameObject;
		rt = GetComponent<RectTransform>();
		this.manager = manager;
		this.font = font;
		this.fontSize = fontSize;
		selected = -1;
		navigationSelected = -1;
		if (icons == null)
		{
			icons = new List<uGUI_ItemIcon>();
		}
		else
		{
			for (int num = icons.Count - 1; num >= 0; num--)
			{
				uGUI_ItemIcon uGUI_ItemIcon2 = icons[num];
				if (uGUI_ItemIcon2 != null)
				{
					Object.Destroy(uGUI_ItemIcon2.gameObject);
				}
				icons.RemoveAt(num);
			}
		}
		int num2 = content.Length;
		for (int i = 0; i < num2; i++)
		{
			object content2 = content[i];
			float x = 0.5f * backgroundSize.x + (float)i * (backgroundSize.x + buttonStep);
			float y = 0.5f * backgroundSize.y;
			uGUI_ItemIcon item = CreateIcon(content2, backgroundSize, foregroundSize, buttonSize, x, y);
			icons.Add(item);
		}
		float size = Mathf.Max(0f, (float)num2 * backgroundSize.x + (float)(num2 - 1) * buttonStep);
		rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		if (selectionMarker != null)
		{
			selectionMarker.SetAsLastSibling();
			selectionMarker.gameObject.SetActive(value: false);
		}
	}

	public void Select(int index)
	{
		if (index >= 0 && index < icons.Count)
		{
			uGUI_ItemIcon uGUI_ItemIcon2;
			if (selected >= 0)
			{
				uGUI_ItemIcon2 = icons[selected];
				uGUI_ItemIcon2.SetBackgroundSprite(backgroundSpriteNormal);
			}
			selected = index;
			uGUI_ItemIcon2 = icons[selected];
			uGUI_ItemIcon2.SetBackgroundSprite(backgroundSpriteSelected);
			if (selectionMarker != null)
			{
				selectionMarker.localPosition = uGUI_ItemIcon2.rectTransform.localPosition + new Vector3(selectionMarkerOffset.x, selectionMarkerOffset.y, 0f);
				selectionMarker.gameObject.SetActive(value: true);
			}
		}
	}

	public void SetNotificationsAmount(int index, int count)
	{
		if (index >= 0 && index < icons.Count)
		{
			uGUI_ItemIcon uGUI_ItemIcon2 = icons[index];
			if (uGUI_ItemIcon2.SetNotificationAlpha((count > 0) ? 1f : 0f))
			{
				uGUI_ItemIcon2.SetNotificationBackgroundColor(NotificationManager.notificationColor);
				uGUI_ItemIcon2.SetNotificationAnchor(notificationAnchor);
				uGUI_ItemIcon2.SetNotificationOffset(notificationOffset);
			}
			uGUI_ItemIcon2.SetNotificationNumber(count);
		}
	}

	public void GetTooltip(uGUI_ItemIcon icon, TooltipData data)
	{
		int num = icons.IndexOf(icon);
		if (num >= 0 && manager != null)
		{
			manager.GetToolbarTooltip(num, data);
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
		int num = icons.IndexOf(icon);
		if (num >= 0 && manager != null)
		{
			manager.OnToolbarClick(num, button);
		}
		return true;
	}

	public bool OnBeginDrag(uGUI_ItemIcon icon)
	{
		return true;
	}

	public void OnEndDrag(uGUI_ItemIcon icon)
	{
	}

	public void OnDrop(uGUI_ItemIcon icon)
	{
	}

	public void OnDragHoverEnter(uGUI_ItemIcon icon)
	{
	}

	public void OnDragHoverStay(uGUI_ItemIcon icon)
	{
	}

	public void OnDragHoverExit(uGUI_ItemIcon icon)
	{
	}

	public bool OnButtonDown(uGUI_ItemIcon icon, GameInput.Button button)
	{
		return false;
	}

	private uGUI_ItemIcon CreateIcon(object content, Vector2 backgroundSize, Vector2 foregroundSize, Vector2 buttonSize, float x, float y)
	{
		Vector2 pivot = new Vector2(0.5f, 0.5f);
		uGUI_ItemIcon uGUI_ItemIcon2 = new GameObject("ToolbarIcon")
		{
			layer = go.layer
		}.AddComponent<uGUI_ItemIcon>();
		uGUI_ItemIcon2.Init(this, rt, new Vector2(0f, 0f), pivot);
		if (content is Sprite)
		{
			uGUI_ItemIcon2.SetForegroundSprite(content as Sprite);
		}
		else if (content is string)
		{
			uGUI_ItemIcon2.SetLabelFont(font, fontSize);
			uGUI_ItemIcon2.SetLabelText(content as string);
		}
		uGUI_ItemIcon2.SetBackgroundSprite(backgroundSpriteNormal);
		uGUI_ItemIcon2.SetActiveSize(buttonSize.x, buttonSize.y);
		uGUI_ItemIcon2.SetBackgroundSize(backgroundSize.x, backgroundSize.y, keepAspect: true);
		uGUI_ItemIcon2.SetForegroundSize(foregroundSize.x, foregroundSize.y);
		uGUI_ItemIcon2.SetPosition(x, y);
		return uGUI_ItemIcon2;
	}

	private void SetNavigationSelection(int index)
	{
		if (navigationSelected != -1)
		{
			OnPointerExit(icons[navigationSelected]);
		}
		navigationSelected = index;
		if (navigationSelected != -1)
		{
			OnPointerEnter(icons[navigationSelected]);
		}
	}

	public object GetSelectedItem()
	{
		if (navigationSelected != -1)
		{
			return icons[navigationSelected];
		}
		return null;
	}

	public Graphic GetSelectedIcon()
	{
		if (navigationSelected != -1)
		{
			return icons[navigationSelected];
		}
		return null;
	}

	public void SelectItem(object item)
	{
		SetNavigationSelection(icons.IndexOf(item as uGUI_ItemIcon));
	}

	public void DeselectItem()
	{
		SetNavigationSelection(-1);
	}

	public bool SelectFirstItem()
	{
		if (icons.Count > 0)
		{
			SetNavigationSelection(0);
			return true;
		}
		return false;
	}

	public bool SelectItemClosestToPosition(Vector3 worldPos)
	{
		float num = float.PositiveInfinity;
		int num2 = -1;
		for (int i = 0; i < icons.Count; i++)
		{
			RectTransform rectTransform = icons[i].rectTransform;
			float sqrMagnitude = (rectTransform.TransformPoint(rectTransform.rect.center) - worldPos).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				num2 = i;
			}
		}
		if (num2 != -1)
		{
			SetNavigationSelection(num2);
			return true;
		}
		return false;
	}

	public bool SelectItemInDirection(int dirX, int dirY)
	{
		if (dirX != 0)
		{
			int num = navigationSelected + dirX;
			if (num >= 0 && num < icons.Count)
			{
				SetNavigationSelection(num);
				return true;
			}
		}
		return false;
	}

	public virtual uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY)
	{
		return interGridNavigation.GetNavigableGridInDirection(dirX, dirY);
	}

	private void OnPrimaryDeviceChanged()
	{
		UpdateCycleBtnText();
	}

	private void OnBindingsChanged()
	{
		UpdateCycleBtnText();
	}

	private void UpdateCycleBtnText()
	{
		if (GameInput.PrimaryDevice == GameInput.Device.Keyboard)
		{
			if (cycleLeftBtnText != null)
			{
				cycleLeftBtnText.gameObject.SetActive(value: false);
			}
			if (cycleRightBtnText != null)
			{
				cycleRightBtnText.gameObject.SetActive(value: false);
			}
			return;
		}
		if (cycleLeftBtnText != null)
		{
			cycleLeftBtnText.text = GameInput.FormatButton(GameInput.Button.UIPrevTab);
			cycleLeftBtnText.gameObject.SetActive(value: true);
		}
		if (cycleRightBtnText != null)
		{
			cycleRightBtnText.text = GameInput.FormatButton(GameInput.Button.UINextTab);
			cycleRightBtnText.gameObject.SetActive(value: true);
		}
	}
}
