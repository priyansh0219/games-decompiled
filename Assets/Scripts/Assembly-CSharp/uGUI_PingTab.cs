using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_PingTab : uGUI_PDATab, uGUI_INavigableIconGrid
{
	private struct SortingHelper
	{
		public string id;

		public int pingType;

		public SortingHelper(string id, int pingType)
		{
			this.id = id;
			this.pingType = pingType;
		}
	}

	private const string pingManagerLabelKey = "PingManagerLabel";

	[AssertNotNull]
	public TextMeshProUGUI pingManagerLabel;

	[AssertNotNull]
	public Toggle visibilityToggle;

	[AssertNotNull]
	public Image visibilityToggleIndicator;

	[AssertNotNull]
	public CanvasGroup content;

	[AssertNotNull]
	public RectTransform pingCanvas;

	[AssertNotNull]
	public ScrollRect scrollRect;

	[AssertNotNull]
	public GameObject prefabEntry;

	[AssertNotNull]
	public Sprite spriteShowAll;

	[AssertNotNull]
	public Sprite spriteHideAll;

	private bool _isDirty = true;

	private Dictionary<string, uGUI_PingEntry> entries = new Dictionary<string, uGUI_PingEntry>();

	private List<SortingHelper> tempSort = new List<SortingHelper>();

	private PrefabPool<uGUI_PingEntry> poolPings;

	private ISelectable selectableVisibilityToggle;

	public override int notificationsCount
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<string, uGUI_PingEntry> entry in entries)
			{
				if (NotificationManager.main.Contains(NotificationManager.Group.Pings, entry.Key))
				{
					num++;
				}
			}
			return num;
		}
	}

	bool uGUI_INavigableIconGrid.ShowSelector => true;

	bool uGUI_INavigableIconGrid.EmulateRaycast => true;

	protected override void Awake()
	{
		selectableVisibilityToggle = new SelectableWrapper(visibilityToggle, delegate(GameInput.Button button)
		{
			if (button == GameInput.Button.UISubmit)
			{
				visibilityToggle.isOn = !visibilityToggle.isOn;
				return true;
			}
			return false;
		});
		poolPings = new PrefabPool<uGUI_PingEntry>(prefabEntry, pingCanvas, 8, 4, delegate(uGUI_PingEntry entry)
		{
			entry.Uninitialize();
		}, delegate(uGUI_PingEntry entry)
		{
			entry.Uninitialize();
		});
	}

	private void OnEnable()
	{
		PingManager.onAdd = (PingManager.OnAdd)Delegate.Combine(PingManager.onAdd, new PingManager.OnAdd(OnAdd));
		PingManager.onRemove = (PingManager.OnRemove)Delegate.Combine(PingManager.onRemove, new PingManager.OnRemove(OnRemove));
		PingManager.onRename = (PingManager.OnRename)Delegate.Combine(PingManager.onRename, new PingManager.OnRename(OnRename));
		PingManager.onIconChange = (PingManager.OnIconChange)Delegate.Combine(PingManager.onIconChange, new PingManager.OnIconChange(OnIconChange));
		PingManager.onVisit = (PingManager.OnVisit)Delegate.Combine(PingManager.onVisit, new PingManager.OnVisit(OnVisit));
	}

	private void OnDestroy()
	{
		PingManager.onAdd = (PingManager.OnAdd)Delegate.Remove(PingManager.onAdd, new PingManager.OnAdd(OnAdd));
		PingManager.onRemove = (PingManager.OnRemove)Delegate.Remove(PingManager.onRemove, new PingManager.OnRemove(OnRemove));
		PingManager.onRename = (PingManager.OnRename)Delegate.Remove(PingManager.onRename, new PingManager.OnRename(OnRename));
		PingManager.onIconChange = (PingManager.OnIconChange)Delegate.Remove(PingManager.onIconChange, new PingManager.OnIconChange(OnIconChange));
		PingManager.onVisit = (PingManager.OnVisit)Delegate.Remove(PingManager.onVisit, new PingManager.OnVisit(OnVisit));
	}

	public override void Open()
	{
		content.SetVisible(visible: true);
	}

	public override void Close()
	{
		scrollRect.velocity = Vector2.zero;
		content.SetVisible(visible: false);
	}

	public override void OnLateUpdate(bool isOpen)
	{
		UpdateEntries();
	}

	public override void OnLanguageChanged()
	{
		pingManagerLabel.text = Language.main.Get("PingManagerLabel");
		Dictionary<string, uGUI_PingEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, uGUI_PingEntry> current = enumerator.Current;
			PingInstance pingInstance = PingManager.Get(current.Key);
			if (pingInstance != null)
			{
				current.Value.UpdateLabel(pingInstance.pingType, pingInstance.GetLabel());
			}
		}
	}

	public override uGUI_INavigableIconGrid GetInitialGrid()
	{
		return this;
	}

	public void SetEntriesVisibility(bool visible)
	{
		visibilityToggleIndicator.sprite = (visible ? spriteHideAll : spriteShowAll);
		Dictionary<string, uGUI_PingEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.visibility.isOn = visible;
		}
	}

	public void OnHoverButtonAll()
	{
		PlayHoverSound();
	}

	private void PlayHoverSound()
	{
	}

	private void OnAdd(PingInstance instance)
	{
		if (instance.displayPingInManager)
		{
			_isDirty = true;
		}
	}

	private void OnRemove(string id)
	{
		if (entries.ContainsKey(id))
		{
			_isDirty = true;
		}
	}

	private void OnRename(PingInstance instance)
	{
		if (instance.displayPingInManager && entries.TryGetValue(instance.Id, out var value))
		{
			value.UpdateLabel(instance.pingType, instance.GetLabel());
		}
	}

	private void OnIconChange(PingInstance instance)
	{
		if (instance.displayPingInManager && entries.TryGetValue(instance.Id, out var value))
		{
			value.SetIcon(instance.pingType);
		}
	}

	private void OnVisit(PingInstance instance, float progress)
	{
		if (instance.displayPingInManager && entries.TryGetValue(instance.Id, out var value))
		{
			value.SetVisit(progress);
		}
	}

	private void UpdateEntries()
	{
		if (!_isDirty)
		{
			return;
		}
		_isDirty = false;
		using (ListPool<string> listPool = Pool<ListPool<string>>.Get())
		{
			List<string> list = listPool.list;
			list.AddRange(entries.Keys);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				string text = list[num];
				if (PingManager.Get(text) == null)
				{
					uGUI_PingEntry entry = entries[text];
					entries.Remove(text);
					poolPings.Release(entry);
				}
			}
			Dictionary<string, PingInstance>.Enumerator enumerator = PingManager.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, PingInstance> current = enumerator.Current;
				string key = current.Key;
				PingInstance value = current.Value;
				if (!entries.ContainsKey(key) && value != null && value.displayPingInManager)
				{
					uGUI_PingEntry uGUI_PingEntry2 = poolPings.Get();
					uGUI_PingEntry2.Initialize(key, value.visible, value.pingType, value.GetLabel(), value.colorIndex);
					bool flag = NotificationManager.main.Contains(NotificationManager.Group.Pings, value.Id);
					uGUI_PingEntry2.SetVisit(flag ? 1f : 0f);
					entries.Add(key, uGUI_PingEntry2);
				}
			}
		}
		tempSort.Clear();
		if (tempSort.Capacity < entries.Count)
		{
			tempSort.Capacity = entries.Count;
		}
		Dictionary<string, uGUI_PingEntry>.Enumerator enumerator2 = entries.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			string key2 = enumerator2.Current.Key;
			PingInstance pingInstance = PingManager.Get(key2);
			if (pingInstance != null)
			{
				tempSort.Add(new SortingHelper(key2, (int)pingInstance.pingType));
			}
		}
		tempSort.Sort((SortingHelper x, SortingHelper y) => x.pingType.CompareTo(y.pingType));
		int i = 0;
		for (int count = tempSort.Count; i < count; i++)
		{
			string id = tempSort[i].id;
			entries[id].rectTransform.SetSiblingIndex(i);
		}
	}

	object uGUI_INavigableIconGrid.GetSelectedItem()
	{
		return UISelection.selected;
	}

	Graphic uGUI_INavigableIconGrid.GetSelectedIcon()
	{
		ISelectable selected = UISelection.selected;
		if (selected != null)
		{
			RectTransform rect = selected.GetRect();
			if (rect != null)
			{
				return rect.GetComponent<Graphic>();
			}
		}
		return null;
	}

	void uGUI_INavigableIconGrid.SelectItem(object item)
	{
		((uGUI_INavigableIconGrid)this).DeselectItem();
		if (!(item is ISelectable selectable))
		{
			return;
		}
		UISelection.selected = selectable;
		RectTransform rect = selectable.GetRect();
		if (!(rect == null))
		{
			uGUI_PingEntry componentInParent = rect.GetComponentInParent<uGUI_PingEntry>();
			if (!(componentInParent == null))
			{
				scrollRect.ScrollTo(componentInParent.rectTransform, xRight: true, yUp: false, Vector4.zero);
				PlayHoverSound();
			}
		}
	}

	void uGUI_INavigableIconGrid.DeselectItem()
	{
		if (UISelection.selected != null)
		{
			UISelection.selected = null;
		}
	}

	bool uGUI_INavigableIconGrid.SelectFirstItem()
	{
		((uGUI_INavigableIconGrid)this).SelectItem((object)selectableVisibilityToggle);
		return true;
	}

	bool uGUI_INavigableIconGrid.SelectItemClosestToPosition(Vector3 worldPos)
	{
		return false;
	}

	bool uGUI_INavigableIconGrid.SelectItemInDirection(int dirX, int dirY)
	{
		if (UISelection.selected == null)
		{
			return ((uGUI_INavigableIconGrid)this).SelectFirstItem();
		}
		if (dirX == 0 && dirY == 0)
		{
			return false;
		}
		UISelection.sSelectables.Clear();
		UISelection.sSelectables.Add(selectableVisibilityToggle);
		Dictionary<string, uGUI_PingEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.GetSelectables(UISelection.sSelectables);
		}
		ISelectable selectable = UISelection.FindSelectable(pingCanvas, new Vector2(dirX, -dirY), UISelection.selected, UISelection.sSelectables, fromEdge: false);
		UISelection.sSelectables.Clear();
		if (selectable != null)
		{
			((uGUI_INavigableIconGrid)this).SelectItem((object)selectable);
			return true;
		}
		return false;
	}

	uGUI_INavigableIconGrid uGUI_INavigableIconGrid.GetNavigableGridInDirection(int dirX, int dirY)
	{
		return null;
	}
}
