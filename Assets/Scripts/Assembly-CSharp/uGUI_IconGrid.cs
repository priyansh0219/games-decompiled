using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_IconGrid : MonoBehaviour, uGUI_IIconManager, ILayoutElement, uGUI_INavigableIconGrid
{
	public class IconData
	{
		public uGUI_ItemIcon icon;

		public int index;

		public IconData(uGUI_ItemIcon icon, int index)
		{
			this.icon = icon;
			this.index = index;
		}
	}

	private float _minHeight;

	private RectTransform _rt;

	private Vector2 _iconSize = new Vector2(64f, 64f);

	private Vector2 _iconBorder = new Vector2(4f, 4f);

	private float _minSpaceX = 20f;

	private float _minSpaceY = 20f;

	private bool sizeDirty = true;

	private bool positionDirty = true;

	private bool canvasDirty = true;

	private float canvasWidth;

	private int iconsByWidth;

	private float spaceX;

	private Sprite iconBackgroundSprite;

	private float iconBackgroundRadius = -1f;

	private Color colorBackgroundNormal = new Color(1f, 1f, 1f, 1f);

	private Color colorBackgroundHover = new Color(1f, 1f, 1f, 1f);

	private Color colorBackgroundPress = new Color(1f, 1f, 1f, 1f);

	private uGUI_IIconGridManager manager;

	private Dictionary<string, IconData> icons = new Dictionary<string, IconData>();

	private Dictionary<uGUI_ItemIcon, string> ids = new Dictionary<uGUI_ItemIcon, string>();

	public float flexibleWidth => -1f;

	public float flexibleHeight => -1f;

	public int layoutPriority => -1;

	public float minWidth => -1f;

	public float minHeight => _minHeight;

	public float preferredWidth => -1f;

	public float preferredHeight => _minHeight;

	public Vector2 iconSize
	{
		get
		{
			return _iconSize;
		}
		set
		{
			if (_iconSize != value)
			{
				_iconSize = value;
				sizeDirty = true;
				positionDirty = true;
				canvasDirty = true;
			}
		}
	}

	public Vector2 iconBorder
	{
		get
		{
			return _iconBorder;
		}
		set
		{
			if (_iconBorder != value)
			{
				_iconBorder = value;
				sizeDirty = true;
			}
		}
	}

	public float minSpaceX
	{
		get
		{
			return _minSpaceX;
		}
		set
		{
			if (_minSpaceX != value)
			{
				_minSpaceX = value;
				sizeDirty = true;
				positionDirty = true;
			}
		}
	}

	public float minSpaceY
	{
		get
		{
			return _minSpaceY;
		}
		set
		{
			if (_minSpaceY != value)
			{
				_minSpaceY = value;
				sizeDirty = true;
				positionDirty = true;
			}
		}
	}

	private RectTransform rt
	{
		get
		{
			if (_rt == null)
			{
				_rt = GetComponent<RectTransform>();
			}
			return _rt;
		}
	}

	private uGUI_ItemIcon selectedIcon
	{
		get
		{
			if (UISelection.HasSelection)
			{
				uGUI_ItemIcon uGUI_ItemIcon2 = UISelection.selected as uGUI_ItemIcon;
				if (uGUI_ItemIcon2 != null && ids.ContainsKey(uGUI_ItemIcon2))
				{
					return uGUI_ItemIcon2;
				}
			}
			return null;
		}
	}

	bool uGUI_INavigableIconGrid.ShowSelector => true;

	bool uGUI_INavigableIconGrid.EmulateRaycast => true;

	public void CalculateLayoutInputHorizontal()
	{
	}

	public void CalculateLayoutInputVertical()
	{
	}

	private void LateUpdate()
	{
		UpdateNow();
	}

	public void Initialize(uGUI_IIconGridManager manager)
	{
		this.manager = manager;
	}

	public void Clear()
	{
		Dictionary<string, IconData>.Enumerator enumerator = icons.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, IconData> current = enumerator.Current;
			_ = current.Key;
			IconData value = current.Value;
			if (value == null)
			{
				continue;
			}
			uGUI_ItemIcon icon = value.icon;
			if (icon != null)
			{
				NotificationManager.main.UnregisterTarget(value.icon);
				GameObject gameObject = icon.gameObject;
				if (gameObject != null)
				{
					Object.Destroy(gameObject);
				}
			}
		}
		icons.Clear();
		ids.Clear();
	}

	public bool AddItem(string id, Sprite foreground, Sprite background = null, bool locked = false, int index = -1)
	{
		if (icons.ContainsKey(id))
		{
			return false;
		}
		uGUI_ItemIcon uGUI_ItemIcon2 = CreateIcon(foreground, (background != null) ? background : iconBackgroundSprite);
		uGUI_ItemIcon2.SetChroma(locked ? 0f : 1f);
		if (index < 0)
		{
			index = icons.Count;
		}
		IconData value = new IconData(uGUI_ItemIcon2, index);
		icons.Add(id, value);
		ids.Add(uGUI_ItemIcon2, id);
		positionDirty = true;
		canvasDirty = true;
		return true;
	}

	public void RemoveItem(string id)
	{
		if (icons.TryGetValue(id, out var value))
		{
			uGUI_ItemIcon icon = value.icon;
			NotificationManager.main.UnregisterTarget(icon);
			Object.Destroy(value.icon.gameObject);
			icons.Remove(id);
			ids.Remove(icon);
			positionDirty = true;
			canvasDirty = true;
		}
	}

	public void SetSprite(string id, Sprite sprite)
	{
		if (icons.TryGetValue(id, out var value))
		{
			value.icon.SetForegroundSprite(sprite);
		}
	}

	public void SetIndex(string id, int index)
	{
		if (icons.TryGetValue(id, out var value) && value.index != index)
		{
			value.index = index;
			positionDirty = true;
			canvasDirty = true;
		}
	}

	public int GetCount()
	{
		return icons.Count;
	}

	public int GetIndex(string id)
	{
		if (id != null)
		{
			if (manager != null)
			{
				manager.OnSortRequested();
			}
			if (icons.TryGetValue(id, out var value))
			{
				return value.index;
			}
		}
		return -1;
	}

	public uGUI_ItemIcon GetIcon(string id)
	{
		if (id == null)
		{
			return null;
		}
		if (icons.TryGetValue(id, out var value))
		{
			return value.icon;
		}
		return null;
	}

	public string GetIdentifier(int index)
	{
		Dictionary<string, IconData>.Enumerator enumerator = icons.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, IconData> current = enumerator.Current;
			if (current.Value.index == index)
			{
				return current.Key;
			}
		}
		return null;
	}

	public void SetIconBackgroundImage(Sprite image)
	{
		bool num = iconBackgroundSprite != null;
		bool flag = image != null;
		iconBackgroundSprite = image;
		if (!(num || flag) || icons == null)
		{
			return;
		}
		Dictionary<string, IconData>.Enumerator enumerator = icons.GetEnumerator();
		while (enumerator.MoveNext())
		{
			uGUI_ItemIcon icon = enumerator.Current.Value.icon;
			if (icon != null)
			{
				icon.SetBackgroundSprite(flag ? iconBackgroundSprite : null);
			}
		}
	}

	public void SetIconBackgroundRadius(float radius)
	{
		if (iconBackgroundRadius == radius)
		{
			return;
		}
		iconBackgroundRadius = radius;
		Dictionary<string, IconData>.Enumerator enumerator = icons.GetEnumerator();
		while (enumerator.MoveNext())
		{
			uGUI_ItemIcon icon = enumerator.Current.Value.icon;
			if (icon != null)
			{
				icon.SetBackgroundRadius(iconBackgroundRadius);
			}
		}
	}

	public void SetIconBackgroundColors(Color normal, Color hover, Color press)
	{
		colorBackgroundNormal = normal;
		colorBackgroundHover = hover;
		colorBackgroundPress = press;
		if (icons == null)
		{
			return;
		}
		Dictionary<string, IconData>.Enumerator enumerator = icons.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, IconData> current = enumerator.Current;
			_ = current.Key;
			IconData value = current.Value;
			if (value != null)
			{
				UpdateIconColor(value.icon);
			}
		}
	}

	public void UpdateNow()
	{
		if (manager != null)
		{
			manager.OnSortRequested();
		}
		if (sizeDirty)
		{
			CalculateSize();
			ResizeAll();
			sizeDirty = false;
			RepositionAll();
			positionDirty = false;
		}
		else if (positionDirty)
		{
			RepositionAll();
			positionDirty = false;
		}
		if (canvasDirty)
		{
			ResizeCanvas();
			canvasDirty = false;
		}
	}

	public void RegisterNotificationTarget(string iconId, NotificationManager.Group group, string key)
	{
		if (icons.TryGetValue(iconId, out var value))
		{
			uGUI_ItemIcon icon = value.icon;
			NotificationManager.main.RegisterTarget(group, key, icon);
		}
	}

	public void GetTooltip(uGUI_ItemIcon icon, TooltipData data)
	{
		if (manager != null && GetIndex(icon, out var id))
		{
			manager.GetTooltip(id, data);
		}
	}

	public void OnPointerEnter(uGUI_ItemIcon icon)
	{
		if (manager != null && GetIndex(icon, out var id))
		{
			manager.OnPointerEnter(id);
		}
	}

	public void OnPointerExit(uGUI_ItemIcon icon)
	{
		if (manager != null && GetIndex(icon, out var id))
		{
			manager.OnPointerExit(id);
		}
	}

	public bool OnPointerClick(uGUI_ItemIcon icon, int button)
	{
		if (manager != null && GetIndex(icon, out var id))
		{
			manager.OnPointerClick(id, button);
		}
		return true;
	}

	public bool OnBeginDrag(uGUI_ItemIcon icon)
	{
		return false;
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

	private bool GetIndex(uGUI_ItemIcon icon, out string id)
	{
		if (ids.TryGetValue(icon, out id))
		{
			return true;
		}
		id = null;
		return false;
	}

	private void SetIconSize(uGUI_ItemIcon icon, float width, float height)
	{
		icon.SetActiveSize(width, height);
		icon.SetBackgroundSize(width, height);
		icon.SetBarSize(width, height);
		icon.SetBackgroundRadius((iconBackgroundRadius < 0f) ? Mathf.Min(width, height) : iconBackgroundRadius);
		icon.SetForegroundSize(width - _iconBorder.x, height - _iconBorder.y);
	}

	private void CalculateSize()
	{
		canvasWidth = rt.rect.width;
		iconsByWidth = Mathf.FloorToInt(canvasWidth / (_iconSize.x + _minSpaceX));
		if (((float)iconsByWidth + 1f) * _iconSize.x + _minSpaceX * (float)iconsByWidth <= canvasWidth)
		{
			iconsByWidth++;
		}
		spaceX = (canvasWidth - (float)iconsByWidth * _iconSize.x) / ((float)iconsByWidth - 1f);
	}

	private uGUI_ItemIcon CreateIcon(Sprite foreground, Sprite background)
	{
		uGUI_ItemIcon uGUI_ItemIcon2 = new GameObject("GridIcon")
		{
			layer = base.gameObject.layer
		}.AddComponent<uGUI_ItemIcon>();
		uGUI_ItemIcon2.Init(this, rt, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f));
		uGUI_ItemIcon2.SetForegroundSprite(foreground);
		uGUI_ItemIcon2.SetBackgroundSprite(background);
		UpdateIconColor(uGUI_ItemIcon2);
		SetIconSize(uGUI_ItemIcon2, _iconSize.x, _iconSize.y);
		return uGUI_ItemIcon2;
	}

	private void RepositionAll()
	{
		Dictionary<string, IconData>.Enumerator enumerator = icons.GetEnumerator();
		while (enumerator.MoveNext())
		{
			IconData value = enumerator.Current.Value;
			uGUI_ItemIcon icon = value.icon;
			Reposition(icon, value.index);
		}
	}

	private void ResizeAll()
	{
		Dictionary<string, IconData>.Enumerator enumerator = icons.GetEnumerator();
		while (enumerator.MoveNext())
		{
			uGUI_ItemIcon icon = enumerator.Current.Value.icon;
			SetIconSize(icon, _iconSize.x, _iconSize.y);
		}
	}

	private void ResizeCanvas()
	{
		int count = icons.Count;
		int num = count / iconsByWidth;
		if (count - num * iconsByWidth > 0)
		{
			num++;
		}
		_minHeight = (float)num * (_iconSize.y + _minSpaceY) - _minSpaceY;
		rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _minHeight);
		LayoutRebuilder.MarkLayoutForRebuild(rt);
	}

	private void Reposition(uGUI_ItemIcon icon, int index)
	{
		Vector2 canvasPositionFromIndex = GetCanvasPositionFromIndex(index);
		icon.SetPosition(canvasPositionFromIndex.x, canvasPositionFromIndex.y);
	}

	public Vector2int GetGridPositionFromIndex(int index)
	{
		int num = 0;
		if (iconsByWidth != 0)
		{
			num = index / iconsByWidth;
		}
		return new Vector2int(index - num * iconsByWidth, num);
	}

	public Vector2 GetCanvasPositionFromIndex(int index)
	{
		Vector2int gridPositionFromIndex = GetGridPositionFromIndex(index);
		int y = gridPositionFromIndex.y;
		int x = gridPositionFromIndex.x;
		return new Vector2(0.5f * _iconSize.x + (_iconSize.x + spaceX) * (float)x, -0.5f * _iconSize.y - (_iconSize.y + _minSpaceY) * (float)y);
	}

	public Vector2 GetCanvasIconSize()
	{
		return new Vector2(_iconSize.x, _iconSize.y);
	}

	private void UpdateIconColor(uGUI_ItemIcon icon)
	{
		if (!(icon == null))
		{
			icon.SetBackgroundColors(colorBackgroundNormal, colorBackgroundHover, colorBackgroundPress);
		}
	}

	public object GetSelectedItem()
	{
		return selectedIcon;
	}

	public Graphic GetSelectedIcon()
	{
		return selectedIcon;
	}

	public void SelectItem(object item)
	{
		DeselectItem();
		uGUI_ItemIcon uGUI_ItemIcon2 = item as uGUI_ItemIcon;
		if (!(uGUI_ItemIcon2 == null) && ids.TryGetValue(uGUI_ItemIcon2, out var _))
		{
			UISelection.selected = uGUI_ItemIcon2;
			ScrollRect componentInParent = rt.GetComponentInParent<ScrollRect>();
			if (componentInParent != null)
			{
				componentInParent.ScrollTo(UISelection.selected.GetRect(), xRight: true, yUp: false, new Vector4(10f, 10f, 10f, 10f));
			}
		}
	}

	public void DeselectItem()
	{
		UISelection.selected = null;
	}

	public bool SelectFirstItem()
	{
		if (manager != null)
		{
			manager.OnSortRequested();
		}
		uGUI_ItemIcon uGUI_ItemIcon2 = null;
		Dictionary<string, IconData>.Enumerator enumerator = icons.GetEnumerator();
		while (enumerator.MoveNext())
		{
			IconData value = enumerator.Current.Value;
			if (value.index == 0)
			{
				uGUI_ItemIcon2 = value.icon;
				break;
			}
		}
		if (uGUI_ItemIcon2 != null)
		{
			SelectItem(uGUI_ItemIcon2);
			return true;
		}
		return false;
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
		if (dirX == 0 && dirY == 0)
		{
			return false;
		}
		UISelection.sSelectables.Clear();
		Dictionary<uGUI_ItemIcon, string>.Enumerator enumerator = ids.GetEnumerator();
		while (enumerator.MoveNext())
		{
			uGUI_ItemIcon key = enumerator.Current.Key;
			UISelection.sSelectables.Add(key);
		}
		UISelection.selected.GetRect();
		ISelectable selectable = UISelection.FindSelectable(rt, new Vector2(dirX, -dirY), UISelection.selected, UISelection.sSelectables, fromEdge: false);
		UISelection.sSelectables.Clear();
		if (selectable != null)
		{
			SelectItem(selectable);
		}
		return false;
	}

	public uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY)
	{
		return null;
	}
}
