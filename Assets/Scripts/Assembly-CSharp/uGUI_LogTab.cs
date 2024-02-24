using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_LogTab : uGUI_PDATab, ICompileTimeCheckable, uGUI_INavigableIconGrid, uGUI_IScrollReceiver
{
	public enum SortBy
	{
		DateDescending = 0,
		DateAscending = 1,
		Topic = 2
	}

	private CachedEnumString<PDALog.EntryType> sEntryTypeStrings = new CachedEnumString<PDALog.EntryType>(PDALog.sEntryTypeComparer);

	[AssertLocalization]
	private const string logLabelKey = "LogLabel";

	[AssertLocalization]
	private const string dayLabelKey = "Day";

	[AssertNotNull]
	public CanvasGroup content;

	[AssertNotNull]
	public TextMeshProUGUI logLabel;

	[AssertNotNull]
	public GameObject prefabEntry;

	[AssertNotNull]
	public GameObject prefabGroupLabel;

	[AssertNotNull]
	public ScrollRect scrollRect;

	[AssertNotNull]
	public RectTransform logCanvas;

	private Comparison<int> groupComparerDateDescending = (int x, int y) => y.CompareTo(x);

	private Comparison<int> groupComparerDateAscending = (int x, int y) => x.CompareTo(y);

	private Comparison<int> groupComparerTopic = (int x, int y) => x.CompareTo(y);

	private Comparison<PDALog.Entry> entryComparerDateDescending = (PDALog.Entry x, PDALog.Entry y) => y.timestamp.CompareTo(x.timestamp);

	private Comparison<PDALog.Entry> entryComparerDateAscending = (PDALog.Entry x, PDALog.Entry y) => x.timestamp.CompareTo(y.timestamp);

	private Comparison<PDALog.Entry> entryComparerTopic = (PDALog.Entry x, PDALog.Entry y) => y.timestamp.CompareTo(x.timestamp);

	private SortBy sortBy;

	private bool _isDirty = true;

	private Dictionary<PDALog.Entry, uGUI_LogEntry> entries = new Dictionary<PDALog.Entry, uGUI_LogEntry>();

	private Dictionary<int, TextMeshProUGUI> groupLabels = new Dictionary<int, TextMeshProUGUI>();

	private Dictionary<int, List<PDALog.Entry>> tempSort = new Dictionary<int, List<PDALog.Entry>>();

	private List<int> tempGroups = new List<int>();

	private PrefabPool<uGUI_LogEntry> poolEntries;

	private PrefabPool<TextMeshProUGUI> poolGroups;

	private bool scrollChangedPosition;

	private Func<int, int, bool> comparerLess = (int x, int y) => x < y;

	private Func<int, int, bool> comparerGreater = (int x, int y) => x > y;

	public override int notificationsCount => NotificationManager.main.GetCount(NotificationManager.Group.Log);

	private uGUI_LogEntry selectedEntry
	{
		get
		{
			if (UISelection.HasSelection)
			{
				return UISelection.selected as uGUI_LogEntry;
			}
			return null;
		}
	}

	bool uGUI_INavigableIconGrid.ShowSelector => true;

	bool uGUI_INavigableIconGrid.EmulateRaycast => true;

	protected override void Awake()
	{
		PDALog.onAdd += OnAddEntry;
		PDALog.onRemove += OnRemoveEntry;
		PDALog.onSetTime += OnSetTime;
		poolEntries = new PrefabPool<uGUI_LogEntry>(prefabEntry, logCanvas, 8, 4, delegate(uGUI_LogEntry entry)
		{
			entry.Uninitialize();
		}, delegate(uGUI_LogEntry entry)
		{
			entry.Uninitialize();
		});
		poolGroups = new PrefabPool<TextMeshProUGUI>(prefabGroupLabel, logCanvas, 4, 4, UninitializeGroupLabel, UninitializeGroupLabel);
	}

	private void OnDestroy()
	{
		PDALog.onAdd -= OnAddEntry;
		PDALog.onRemove -= OnRemoveEntry;
		PDALog.onSetTime -= OnSetTime;
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

	public override void OnWarmUp()
	{
		_isDirty = true;
		base.OnWarmUp();
	}

	public override void OnLateUpdate(bool isOpen)
	{
		UpdateEntries();
		if (isOpen)
		{
			UpdatePlayButtonsState();
		}
	}

	public override void OnLanguageChanged()
	{
		logLabel.text = Language.main.Get("LogLabel");
		Dictionary<int, TextMeshProUGUI>.Enumerator enumerator = groupLabels.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<int, TextMeshProUGUI> current = enumerator.Current;
			int key = current.Key;
			current.Value.text = GetGroupText(key);
		}
		Dictionary<PDALog.Entry, uGUI_LogEntry>.Enumerator enumerator2 = entries.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			enumerator2.Current.Value.UpdateText();
		}
	}

	public override uGUI_INavigableIconGrid GetInitialGrid()
	{
		return this;
	}

	private void OnAddEntry(PDALog.Entry entry)
	{
		_isDirty = true;
	}

	private void OnRemoveEntry(PDALog.Entry entry)
	{
		if (entries.ContainsKey(entry))
		{
			uGUI_LogEntry uGUI_LogEntry2 = entries[entry];
			entries.Remove(entry);
			poolEntries.Release(uGUI_LogEntry2);
			NotificationManager.main.UnregisterTarget(uGUI_LogEntry2);
			_isDirty = true;
		}
	}

	private void OnSetTime()
	{
		_isDirty = true;
	}

	private void UpdateEntries()
	{
		if (!_isDirty)
		{
			return;
		}
		_isDirty = false;
		using (ListPool<PDALog.Entry> listPool = Pool<ListPool<PDALog.Entry>>.Get())
		{
			List<PDALog.Entry> list = listPool.list;
			list.AddRange(entries.Keys);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				PDALog.Entry entry = list[num];
				if (!PDALog.Contains(entry.data.key))
				{
					uGUI_LogEntry uGUI_LogEntry2 = entries[entry];
					entries.Remove(entry);
					poolEntries.Release(uGUI_LogEntry2);
					NotificationManager.main.UnregisterTarget(uGUI_LogEntry2);
				}
			}
		}
		foreach (KeyValuePair<string, PDALog.Entry> entry2 in PDALog.GetEntries())
		{
			string key = entry2.Key;
			PDALog.Entry value = entry2.Value;
			if (!entries.ContainsKey(value))
			{
				uGUI_LogEntry uGUI_LogEntry3 = poolEntries.Get();
				uGUI_LogEntry3.Initialize(value);
				entries.Add(value, uGUI_LogEntry3);
				PDALog.EntryData entryData = value.data;
				if (entryData == null)
				{
					entryData = new PDALog.EntryData();
					entryData.key = key;
					entryData.type = PDALog.EntryType.Invalid;
					value.data = entryData;
				}
				NotificationManager.main.RegisterTarget(NotificationManager.Group.Log, entryData.key, uGUI_LogEntry3);
			}
		}
		SortEntries();
	}

	private void UpdatePlayButtonsState()
	{
		SoundQueue queue = PDASounds.queue;
		if (queue == null)
		{
			return;
		}
		string current = queue.current;
		foreach (KeyValuePair<PDALog.Entry, uGUI_LogEntry> entry in entries)
		{
			PDALog.Entry key = entry.Key;
			uGUI_LogEntry value = entry.Value;
			if (((INotificationTarget)value).IsVisible())
			{
				FMODAsset sound = key.data.sound;
				value.SetPlaying(sound != null && sound.id == current);
			}
		}
	}

	private void SortEntries()
	{
		tempSort.Clear();
		Dictionary<PDALog.Entry, uGUI_LogEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PDALog.Entry key = enumerator.Current.Key;
			int entryCriteria = GetEntryCriteria(key);
			if (!tempSort.TryGetValue(entryCriteria, out var value))
			{
				value = new List<PDALog.Entry>();
				tempSort.Add(entryCriteria, value);
			}
			value.Add(key);
		}
		tempGroups.Clear();
		tempGroups.AddRange(groupLabels.Keys);
		int i = 0;
		for (int count = tempGroups.Count; i < count; i++)
		{
			int key2 = tempGroups[i];
			if (!tempSort.ContainsKey(key2))
			{
				poolGroups.Release(groupLabels[key2]);
			}
		}
		tempGroups.Clear();
		tempGroups.AddRange(tempSort.Keys);
		Comparison<int> comparison;
		switch (sortBy)
		{
		default:
			comparison = groupComparerDateDescending;
			break;
		case SortBy.DateAscending:
			comparison = groupComparerDateAscending;
			break;
		case SortBy.Topic:
			comparison = groupComparerTopic;
			break;
		}
		tempGroups.Sort(comparison);
		int num = 0;
		Comparison<PDALog.Entry> comparison2;
		switch (sortBy)
		{
		default:
			comparison2 = entryComparerDateDescending;
			break;
		case SortBy.DateAscending:
			comparison2 = entryComparerDateAscending;
			break;
		case SortBy.Topic:
			comparison2 = entryComparerTopic;
			break;
		}
		int j = 0;
		for (int count2 = tempGroups.Count; j < count2; j++)
		{
			int num2 = tempGroups[j];
			if (!groupLabels.TryGetValue(num2, out var value2))
			{
				value2 = poolGroups.Get();
				value2.gameObject.SetActive(value: true);
				value2.text = GetGroupText(num2);
				groupLabels.Add(num2, value2);
			}
			value2.rectTransform.SetSiblingIndex(num);
			num++;
			List<PDALog.Entry> list = tempSort[num2];
			list.Sort(comparison2);
			int k = 0;
			for (int count3 = list.Count; k < count3; k++)
			{
				PDALog.Entry key3 = list[k];
				entries[key3].rectTransform.SetSiblingIndex(num);
				num++;
			}
		}
		tempGroups.Clear();
		tempSort.Clear();
	}

	private void InitializeGroupLabel(TextMeshProUGUI groupLabel)
	{
		groupLabel.gameObject.SetActive(value: true);
	}

	private void UninitializeGroupLabel(TextMeshProUGUI groupLabel)
	{
		groupLabel.text = string.Empty;
		groupLabel.gameObject.SetActive(value: false);
	}

	private string GetGroupText(int index)
	{
		switch (sortBy)
		{
		case SortBy.DateDescending:
		case SortBy.DateAscending:
			return string.Format("{0} {1}", Language.main.Get("Day"), index);
		case SortBy.Topic:
			return sEntryTypeStrings.Get((PDALog.EntryType)index);
		default:
			return IntStringCache.GetStringForInt(index);
		}
	}

	private int GetEntryCriteria(PDALog.Entry entry)
	{
		int result = 0;
		switch (sortBy)
		{
		case SortBy.DateDescending:
		case SortBy.DateAscending:
			result = DayNightCycle.ToGameDays(entry.timestamp) + 1;
			break;
		case SortBy.Topic:
			result = (int)entry.data.type;
			break;
		}
		return result;
	}

	public string CompileTimeCheck()
	{
		if (prefabEntry.GetComponent<uGUI_LogEntry>() == null)
		{
			return "uGUI_LogTab : uGUI_LogEntry component is missing on prefabEntry prefab!";
		}
		if (prefabGroupLabel.GetComponent<TextMeshProUGUI>() == null)
		{
			return "uGUI_LogTab : Text component is missing on prefabgroupLabel prefab!";
		}
		return null;
	}

	public object GetSelectedItem()
	{
		return selectedEntry;
	}

	public Graphic GetSelectedIcon()
	{
		uGUI_LogEntry uGUI_LogEntry2 = selectedEntry;
		if (!(uGUI_LogEntry2 != null))
		{
			return null;
		}
		return uGUI_LogEntry2.background;
	}

	public void SelectItem(object item)
	{
		uGUI_LogEntry uGUI_LogEntry2 = item as uGUI_LogEntry;
		if (!(uGUI_LogEntry2 == null))
		{
			DeselectItem();
			UISelection.selected = uGUI_LogEntry2;
			scrollRect.ScrollTo(uGUI_LogEntry2.rectTransform, xRight: true, yUp: false, new Vector4(10f, 10f, 10f, 10f));
		}
	}

	public void DeselectItem()
	{
		if (!(selectedEntry == null))
		{
			UISelection.selected = null;
		}
	}

	public bool SelectFirstItem()
	{
		uGUI_LogEntry uGUI_LogEntry2 = null;
		RectTransform viewport = scrollRect.viewport;
		Rect rect = viewport.rect;
		float yMin = rect.yMin;
		float yMax = rect.yMax;
		float num = float.MinValue;
		Dictionary<PDALog.Entry, uGUI_LogEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			uGUI_LogEntry value = enumerator.Current.Value;
			if (value.isActiveAndEnabled)
			{
				RectTransform rectTransform = value.rectTransform;
				float y = viewport.InverseTransformPoint(rectTransform.TransformPoint(rectTransform.rect.center)).y;
				if (y >= yMin && y <= yMax && y > num)
				{
					num = y;
					uGUI_LogEntry2 = value;
				}
			}
		}
		if (uGUI_LogEntry2 != null)
		{
			SelectItem(uGUI_LogEntry2);
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
		uGUI_LogEntry uGUI_LogEntry2 = selectedEntry;
		if (uGUI_LogEntry2 == null)
		{
			return SelectFirstItem();
		}
		if (dirX == 0 && dirY == 0)
		{
			return false;
		}
		if (scrollChangedPosition)
		{
			scrollChangedPosition = false;
			RectTransform viewport = scrollRect.viewport;
			RectTransform rectTransform = selectedEntry.rectTransform;
			float y = viewport.InverseTransformPoint(rectTransform.TransformPoint(rectTransform.rect.center)).y;
			Rect rect = viewport.rect;
			if (y < rect.yMin || y > rect.yMax)
			{
				return ((uGUI_INavigableIconGrid)this).SelectFirstItem();
			}
		}
		if (dirY == 0)
		{
			return false;
		}
		uGUI_LogEntry uGUI_LogEntry3 = null;
		int siblingIndex = uGUI_LogEntry2.rectTransform.GetSiblingIndex();
		int arg = ((dirY < 0) ? int.MinValue : int.MaxValue);
		Func<int, int, bool> func = ((dirY < 0) ? comparerLess : comparerGreater);
		Dictionary<PDALog.Entry, uGUI_LogEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			uGUI_LogEntry value = enumerator.Current.Value;
			if (value.isActiveAndEnabled)
			{
				int siblingIndex2 = value.rectTransform.GetSiblingIndex();
				if (func(siblingIndex2, siblingIndex) && func(arg, siblingIndex2))
				{
					arg = siblingIndex2;
					uGUI_LogEntry3 = value;
				}
			}
		}
		if (uGUI_LogEntry3 != null)
		{
			SelectItem(uGUI_LogEntry3);
			return true;
		}
		return false;
	}

	public uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY)
	{
		return null;
	}

	bool uGUI_IScrollReceiver.OnScroll(float scrollDelta, float speedMultiplier)
	{
		scrollChangedPosition = true;
		scrollRect.Scroll(scrollDelta, speedMultiplier);
		return true;
	}
}
