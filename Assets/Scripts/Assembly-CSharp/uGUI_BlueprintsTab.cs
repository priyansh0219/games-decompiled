using System;
using System.Collections.Generic;
using System.Text;
using FMODUnity;
using Gendarme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule")]
public class uGUI_BlueprintsTab : uGUI_PDATab, ICompileTimeCheckable, uGUI_INavigableIconGrid, uGUI_IScrollReceiver, ILocalizationCheckable
{
	private class CategoryEntry
	{
		public RectTransform title;

		public TextMeshProUGUI titleText;

		public RectTransform canvas;

		public Dictionary<TechType, uGUI_BlueprintEntry> entries = new Dictionary<TechType, uGUI_BlueprintEntry>(TechTypeExtensions.sTechTypeComparer);
	}

	private static readonly List<TechCategory> sTechCategories = new List<TechCategory>();

	private static readonly List<TechType> sTechTypes = new List<TechType>();

	private static readonly CachedEnumString<TechCategory> techCategoryStrings = new CachedEnumString<TechCategory>("TechCategory", CraftData.sTechCategoryComparer);

	public static readonly CachedEnumString<TechType> blueprintEntryStrings = new CachedEnumString<TechType>(string.Empty, ".BlueprintsTab", TechTypeExtensions.sTechTypeComparer);

	private const NotificationManager.Group notificationGroup = NotificationManager.Group.Blueprints;

	private static readonly List<TechGroup> groups = new List<TechGroup>
	{
		TechGroup.Resources,
		TechGroup.Survival,
		TechGroup.Personal,
		TechGroup.Machines,
		TechGroup.Constructor,
		TechGroup.VehicleUpgrades,
		TechGroup.Workbench,
		TechGroup.MapRoomUpgrades,
		TechGroup.Cyclops,
		TechGroup.BasePieces,
		TechGroup.ExteriorModules,
		TechGroup.InteriorPieces,
		TechGroup.InteriorModules,
		TechGroup.Miscellaneous,
		TechGroup.Uncategorized
	};

	[AssertLocalization]
	private const string blueprintsLabelKey = "BlueprintsLabel";

	[AssertLocalization]
	private const string clearPinsKey = "PDABlueprintsButtonClearPins";

	[AssertNotNull]
	public CanvasGroup content;

	[AssertNotNull]
	public RectTransform canvas;

	[AssertNotNull]
	public ScrollRect scrollRect;

	[AssertNotNull]
	public GameObject prefabTitle;

	[AssertNotNull]
	public GameObject prefabEntry;

	[AssertNotNull]
	public RectTransform prefabPin;

	[AssertNotNull]
	public TextMeshProUGUI blueprintsLabel;

	[AssertNotNull]
	public GameObject buttonPinsClear;

	[AssertNotNull]
	public TextMeshProUGUI buttonTextPinsClear;

	[AssertNotNull]
	public RectTransform pinHover;

	[AssertNotNull]
	public FMODAsset soundPin;

	[AssertNotNull]
	public FMODAsset soundUnpin;

	[AssertNotNull]
	public FMODAsset soundClearPins;

	private Dictionary<TechCategory, CategoryEntry> entries = new Dictionary<TechCategory, CategoryEntry>(CraftData.sTechCategoryComparer);

	private bool isDirty = true;

	private int _notificationsCount;

	private CanvasGroup pinHoverGroup;

	private PrefabPool<RectTransform> poolPins;

	private bool scrollChangedPosition;

	public override int notificationsCount => _notificationsCount;

	private uGUI_BlueprintEntry selectedEntry
	{
		get
		{
			if (UISelection.HasSelection)
			{
				return UISelection.selected as uGUI_BlueprintEntry;
			}
			return null;
		}
	}

	bool uGUI_INavigableIconGrid.ShowSelector => true;

	bool uGUI_INavigableIconGrid.EmulateRaycast => true;

	protected override void Awake()
	{
		pinHoverGroup = pinHover.GetComponent<CanvasGroup>();
		pinHover.SetParent(canvas, worldPositionStays: false);
		UpdatePinHover(null);
		poolPins = new PrefabPool<RectTransform>(prefabPin.gameObject, canvas, 10, 10, delegate(RectTransform entry)
		{
			entry.gameObject.SetActive(value: false);
		}, delegate(RectTransform entry)
		{
			entry.gameObject.SetActive(value: false);
		});
		Close();
	}

	private void Start()
	{
		KnownTech.onChanged += OnCompletedChanged;
		KnownTech.onCompoundAdd += OnCompoundAdd;
		KnownTech.onCompoundRemove += OnCompoundRemove;
		KnownTech.onCompoundProgress += OnCompoundProgress;
		PDAScanner.onAdd = (PDAScanner.OnEntryEvent)Delegate.Combine(PDAScanner.onAdd, new PDAScanner.OnEntryEvent(OnLockedAdd));
		PDAScanner.onRemove = (PDAScanner.OnEntryEvent)Delegate.Combine(PDAScanner.onRemove, new PDAScanner.OnEntryEvent(OnLockedRemove));
		PDAScanner.onProgress = (PDAScanner.OnEntryEvent)Delegate.Combine(PDAScanner.onProgress, new PDAScanner.OnEntryEvent(OnLockedProgress));
		PinManager.onAdd += OnPinAdd;
		PinManager.onRemove += OnPinRemove;
		GameInput.OnPrimaryDeviceChanged += UpdatePinsClearButtonState;
		UpdatePinsClearButtonState();
	}

	private void OnDestroy()
	{
		KnownTech.onChanged -= OnCompletedChanged;
		KnownTech.onCompoundAdd -= OnCompoundAdd;
		KnownTech.onCompoundRemove -= OnCompoundRemove;
		KnownTech.onCompoundProgress -= OnCompoundProgress;
		PDAScanner.onAdd = (PDAScanner.OnEntryEvent)Delegate.Remove(PDAScanner.onAdd, new PDAScanner.OnEntryEvent(OnLockedAdd));
		PDAScanner.onRemove = (PDAScanner.OnEntryEvent)Delegate.Remove(PDAScanner.onRemove, new PDAScanner.OnEntryEvent(OnLockedRemove));
		PDAScanner.onProgress = (PDAScanner.OnEntryEvent)Delegate.Remove(PDAScanner.onProgress, new PDAScanner.OnEntryEvent(OnLockedProgress));
		PinManager.onAdd -= OnPinAdd;
		PinManager.onRemove -= OnPinRemove;
		GameInput.OnPrimaryDeviceChanged -= UpdatePinsClearButtonState;
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
		UpdateNotificationsCount();
	}

	public override void OnLanguageChanged()
	{
		blueprintsLabel.text = Language.main.Get("BlueprintsLabel");
		buttonTextPinsClear.text = Language.main.Get("PDABlueprintsButtonClearPins");
		Dictionary<TechCategory, CategoryEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TechCategory, CategoryEntry> current = enumerator.Current;
			TechCategory key = current.Key;
			CategoryEntry value = current.Value;
			SetCategoryTitle(value, key);
			Dictionary<TechType, uGUI_BlueprintEntry>.Enumerator enumerator2 = value.entries.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<TechType, uGUI_BlueprintEntry> current2 = enumerator2.Current;
				TechType key2 = current2.Key;
				uGUI_BlueprintEntry value2 = current2.Value;
				SetEntryText(value2, key2);
			}
		}
	}

	public override uGUI_INavigableIconGrid GetInitialGrid()
	{
		return this;
	}

	public override bool OnButtonDown(GameInput.Button button)
	{
		if (base.OnButtonDown(button))
		{
			return true;
		}
		uGUI_BlueprintEntry uGUI_BlueprintEntry2 = selectedEntry;
		if (uGUI_BlueprintEntry2 != null)
		{
			switch (button)
			{
			case GameInput.Button.UISubmit:
			{
				TechType techType = GetTechType(uGUI_BlueprintEntry2);
				if (IsUnlocked(techType))
				{
					PinManager.TogglePin(techType);
					PlayPinStatusSound(techType);
					UpdatePinHover(uGUI_BlueprintEntry2);
				}
				return true;
			}
			case GameInput.Button.UIAssign:
				ClearPins();
				return true;
			}
		}
		return false;
	}

	private void OnCompletedChanged()
	{
		isDirty = true;
	}

	private void OnCompoundAdd(TechType techType, int unlocked, int total)
	{
		isDirty = true;
	}

	private void OnCompoundRemove(TechType techType)
	{
		isDirty = true;
	}

	private void OnCompoundProgress(TechType techType, int unlocked, int total)
	{
		isDirty = true;
	}

	private void OnLockedAdd(PDAScanner.Entry entry)
	{
		isDirty = true;
	}

	private void OnLockedRemove(PDAScanner.Entry entry)
	{
		isDirty = true;
	}

	private void OnLockedProgress(PDAScanner.Entry entry)
	{
		isDirty = true;
	}

	private bool IsUnlocked(TechType techType)
	{
		return CrafterLogic.IsCraftRecipeUnlocked(techType);
	}

	private void UpdateEntries()
	{
		if (!isDirty)
		{
			return;
		}
		isDirty = false;
		for (int i = 0; i < groups.Count; i++)
		{
			TechGroup group = groups[i];
			CraftData.GetBuilderCategories(group, sTechCategories);
			for (int j = 0; j < sTechCategories.Count; j++)
			{
				TechCategory techCategory = sTechCategories[j];
				CraftData.GetBuilderTech(group, techCategory, sTechTypes);
				for (int k = 0; k < sTechTypes.Count; k++)
				{
					TechType techType = sTechTypes[k];
					uGUI_BlueprintEntry value = null;
					if (entries.TryGetValue(techCategory, out var value2))
					{
						value2.entries.TryGetValue(techType, out value);
					}
					int unlocked;
					int total;
					TechUnlockState techUnlockState = KnownTech.GetTechUnlockState(techType, out unlocked, out total);
					bool num = value != null;
					bool flag = techUnlockState == TechUnlockState.Available || techUnlockState == TechUnlockState.Locked;
					if (num != flag)
					{
						if (flag)
						{
							if (value2 == null)
							{
								value2 = new CategoryEntry();
								GameObject obj = UnityEngine.Object.Instantiate(prefabTitle);
								RectTransform component = obj.GetComponent<RectTransform>();
								component.SetParent(canvas, worldPositionStays: false);
								value2.title = component;
								TextMeshProUGUI componentInChildren = obj.GetComponentInChildren<TextMeshProUGUI>();
								value2.titleText = componentInChildren;
								SetCategoryTitle(value2, techCategory);
								GameObject obj2 = new GameObject("CategoryCanvas");
								RectTransform rectTransform = obj2.AddComponent<RectTransform>();
								rectTransform.SetParent(canvas, worldPositionStays: false);
								value2.canvas = rectTransform;
								obj2.AddComponent<FlexibleGridLayout>();
								entries.Add(techCategory, value2);
							}
							GameObject obj3 = UnityEngine.Object.Instantiate(prefabEntry);
							obj3.transform.SetParent(value2.canvas, worldPositionStays: false);
							value = obj3.GetComponent<uGUI_BlueprintEntry>();
							value.Initialize(this);
							value.SetIcon(techType);
							SetPin(value, PinManager.GetPin(techType));
							SetEntryText(value, techType);
							value2.entries.Add(techType, value);
							NotificationManager.main.RegisterTarget(NotificationManager.Group.Blueprints, techType.EncodeKey(), value);
						}
						else
						{
							NotificationManager.main.UnregisterTarget(value);
							value2.entries.Remove(techType);
							UnityEngine.Object.Destroy(value.gameObject);
							if (value2.entries.Count == 0)
							{
								UnityEngine.Object.Destroy(value2.title.gameObject);
								UnityEngine.Object.Destroy(value2.canvas.gameObject);
								entries.Remove(techCategory);
							}
						}
					}
					if (value != null)
					{
						value.SetValue(unlocked, total);
					}
				}
			}
		}
		UpdateOrder();
	}

	private void UpdateNotificationsCount()
	{
		_notificationsCount = 0;
		NotificationManager main = NotificationManager.main;
		Dictionary<TechCategory, CategoryEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Dictionary<TechType, uGUI_BlueprintEntry>.Enumerator enumerator2 = enumerator.Current.Value.entries.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				if (main.Contains(NotificationManager.Group.Blueprints, enumerator2.Current.Key.EncodeKey()))
				{
					_notificationsCount++;
				}
			}
		}
	}

	private void UpdateOrder()
	{
		List<TechCategory> list = new List<TechCategory>();
		int num = 0;
		for (int i = 0; i < groups.Count; i++)
		{
			TechGroup techGroup = groups[i];
			CraftData.GetBuilderCategories(techGroup, list);
			for (int j = 0; j < list.Count; j++)
			{
				TechCategory techCategory = list[j];
				if (entries.TryGetValue(techCategory, out var value))
				{
					value.title.SetSiblingIndex(num);
					num++;
					value.canvas.SetSiblingIndex(num);
					num++;
					UpdateOrder(techGroup, techCategory, value.entries);
				}
			}
		}
	}

	private void UpdateOrder(TechGroup techGroup, TechCategory techCategory, Dictionary<TechType, uGUI_BlueprintEntry> entries)
	{
		List<TechType> list = new List<TechType>();
		int num = 0;
		CraftData.GetBuilderTech(techGroup, techCategory, list);
		for (int i = 0; i < list.Count; i++)
		{
			TechType key = list[i];
			if (entries.TryGetValue(key, out var value))
			{
				value.rectTransform.SetSiblingIndex(num);
				num++;
			}
		}
	}

	private void SetCategoryTitle(CategoryEntry categoryEntry, TechCategory techCategory)
	{
		categoryEntry.titleText.text = Language.main.Get(techCategoryStrings.Get(techCategory));
	}

	private void SetEntryText(uGUI_BlueprintEntry blueprintEntry, TechType techType)
	{
		string orFallback = Language.main.GetOrFallback(blueprintEntryStrings.Get(techType), techType);
		blueprintEntry.SetText(orFallback);
	}

	public void GetTooltip(uGUI_BlueprintEntry entry, TooltipData data)
	{
		TechType techType = GetTechType(entry);
		if (techType != 0)
		{
			TooltipFactory.Blueprint(techType, !IsUnlocked(techType), data);
		}
	}

	public void OnPointerEnter(uGUI_BlueprintEntry entry)
	{
		if (entry != null && IsUnlocked(GetTechType(entry)))
		{
			UpdatePinHover(entry);
		}
	}

	public void OnPointerExit(uGUI_BlueprintEntry entry)
	{
		if (entry != null)
		{
			UpdatePinHover(null);
		}
	}

	public bool OnPointerClick(uGUI_BlueprintEntry entry, int button)
	{
		if (entry != null)
		{
			TechType techType = GetTechType(entry);
			if (IsUnlocked(techType))
			{
				PinManager.TogglePin(techType);
				PlayPinStatusSound(techType);
				UpdatePinHover(entry);
			}
		}
		return true;
	}

	private void OnPinAdd(TechType techType)
	{
		UpdatePins(techType, pin: true);
		UpdatePinsClearButtonState();
	}

	private void OnPinRemove(TechType techType)
	{
		UpdatePins(techType, pin: false);
		UpdatePinsClearButtonState();
	}

	private void UpdatePins(TechType techType, bool pin)
	{
		Dictionary<TechCategory, CategoryEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Dictionary<TechType, uGUI_BlueprintEntry>.Enumerator enumerator2 = enumerator.Current.Value.entries.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<TechType, uGUI_BlueprintEntry> current = enumerator2.Current;
				if (current.Key == techType)
				{
					SetPin(current.Value, pin);
				}
			}
		}
	}

	private void SetPin(uGUI_BlueprintEntry entry, bool state)
	{
		RectTransform pin = entry.pin;
		if (pin != null != state)
		{
			if (state)
			{
				pin = poolPins.Get();
				pin.gameObject.SetActive(value: true);
				pin.SetParent(entry.rectTransform, worldPositionStays: false);
				entry.pin = pin;
			}
			else
			{
				poolPins.Release(pin);
				entry.pin = null;
			}
		}
	}

	private void UpdatePinHover(uGUI_BlueprintEntry entry)
	{
		if (entry == null || entry.pin != null)
		{
			pinHoverGroup.alpha = 0f;
			return;
		}
		pinHoverGroup.alpha = 1f;
		Matrix4x4 matrix4x = Matrix4x4.identity;
		Transform transform = entry.rectTransform;
		while (transform != null && transform != canvas)
		{
			matrix4x = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale) * matrix4x;
			transform = transform.parent;
		}
		Vector2 min = entry.rectTransform.rect.min;
		min.y = 0f - min.y;
		pinHover.localPosition = matrix4x.MultiplyPoint(min);
	}

	private void UpdatePinsClearButtonState()
	{
		buttonPinsClear.SetActive(PinManager.Count > 0 && !GameInput.IsPrimaryDeviceGamepad());
	}

	public void ClearPins()
	{
		RuntimeManager.PlayOneShot(soundClearPins.id);
		PinManager.Clear();
	}

	public void Ping(TechType techType)
	{
		uGUI_BlueprintEntry entry = GetEntry(techType);
		if (entry != null)
		{
			ScrollTo(entry);
			entry.icon.PunchScale();
		}
	}

	private void PlayPinStatusSound(TechType techType)
	{
		RuntimeManager.PlayOneShot((PinManager.GetPin(techType) ? soundPin : soundUnpin).id);
	}

	private uGUI_BlueprintEntry GetEntry(TechType techType)
	{
		Dictionary<TechCategory, CategoryEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Dictionary<TechType, uGUI_BlueprintEntry>.Enumerator enumerator2 = enumerator.Current.Value.entries.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<TechType, uGUI_BlueprintEntry> current = enumerator2.Current;
				if (current.Key == techType)
				{
					return current.Value;
				}
			}
		}
		return null;
	}

	private TechType GetTechType(uGUI_BlueprintEntry blueprintEntry)
	{
		if (blueprintEntry != null)
		{
			Dictionary<TechCategory, CategoryEntry>.Enumerator enumerator = entries.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Dictionary<TechType, uGUI_BlueprintEntry>.Enumerator enumerator2 = enumerator.Current.Value.entries.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					KeyValuePair<TechType, uGUI_BlueprintEntry> current = enumerator2.Current;
					if (current.Value == blueprintEntry)
					{
						return current.Key;
					}
				}
			}
		}
		return TechType.None;
	}

	private uGUI_BlueprintEntry GetFirstEntry()
	{
		List<TechCategory> list = new List<TechCategory>();
		List<TechType> list2 = new List<TechType>();
		int i = 0;
		for (int count = groups.Count; i < count; i++)
		{
			TechGroup group = groups[i];
			CraftData.GetBuilderCategories(group, list);
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				TechCategory techCategory = list[j];
				if (!entries.TryGetValue(techCategory, out var value))
				{
					continue;
				}
				Dictionary<TechType, uGUI_BlueprintEntry> dictionary = value.entries;
				CraftData.GetBuilderTech(group, techCategory, list2);
				int k = 0;
				for (int count3 = list2.Count; k < count3; k++)
				{
					TechType key = list2[k];
					if (dictionary.TryGetValue(key, out var value2))
					{
						return value2;
					}
				}
			}
		}
		return null;
	}

	private CategoryEntry GetCategoryEntry(uGUI_BlueprintEntry blueprintEntry)
	{
		Dictionary<TechCategory, CategoryEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			CategoryEntry value = enumerator.Current.Value;
			Dictionary<TechType, uGUI_BlueprintEntry>.Enumerator enumerator2 = value.entries.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				if (enumerator2.Current.Value == blueprintEntry)
				{
					return value;
				}
			}
		}
		return null;
	}

	private void ScrollTo(uGUI_BlueprintEntry entry)
	{
		if (!(entry == null))
		{
			bool xRight = true;
			bool yUp = false;
			CategoryEntry categoryEntry = GetCategoryEntry(entry);
			if (categoryEntry != null)
			{
				scrollRect.ScrollTo(categoryEntry.title, xRight, yUp, Vector4.zero);
			}
			scrollRect.ScrollTo(((ISelectable)entry).GetRect(), xRight, yUp, new Vector4(10f, 10f, 10f, 10f));
		}
	}

	object uGUI_INavigableIconGrid.GetSelectedItem()
	{
		return selectedEntry;
	}

	Graphic uGUI_INavigableIconGrid.GetSelectedIcon()
	{
		if (!(selectedEntry != null))
		{
			return null;
		}
		return selectedEntry.icon;
	}

	void uGUI_INavigableIconGrid.SelectItem(object item)
	{
		((uGUI_INavigableIconGrid)this).DeselectItem();
		UISelection.selected = item as ISelectable;
		if (!(selectedEntry == null))
		{
			ScrollTo(selectedEntry);
			UpdatePinHover(selectedEntry);
		}
	}

	void uGUI_INavigableIconGrid.DeselectItem()
	{
		if (!(selectedEntry == null))
		{
			UpdatePinHover(null);
			UISelection.selected = null;
		}
	}

	bool uGUI_INavigableIconGrid.SelectFirstItem()
	{
		uGUI_BlueprintEntry uGUI_BlueprintEntry2 = null;
		RectTransform viewport = scrollRect.viewport;
		Rect rect = viewport.rect;
		float yMin = rect.yMin;
		float yMax = rect.yMax;
		Vector2 vector = new Vector2(float.MaxValue, float.MinValue);
		Dictionary<TechCategory, CategoryEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Dictionary<TechType, uGUI_BlueprintEntry>.Enumerator enumerator2 = enumerator.Current.Value.entries.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				uGUI_BlueprintEntry value = enumerator2.Current.Value;
				RectTransform rectTransform = value.rectTransform;
				Vector3 vector2 = viewport.InverseTransformPoint(rectTransform.TransformPoint(rectTransform.rect.center));
				if (vector2.y >= yMin && vector2.y <= yMax)
				{
					bool num = vector2.x < vector.x && vector2.y > vector.y - 5f;
					bool flag = vector2.y > vector.y && vector2.x < vector.x + 5f;
					if (num || flag)
					{
						vector = vector2;
						uGUI_BlueprintEntry2 = value;
					}
				}
			}
		}
		if (uGUI_BlueprintEntry2 == null)
		{
			uGUI_BlueprintEntry2 = GetFirstEntry();
		}
		if (uGUI_BlueprintEntry2 != null)
		{
			((uGUI_INavigableIconGrid)this).SelectItem((object)uGUI_BlueprintEntry2);
			return true;
		}
		return false;
	}

	bool uGUI_INavigableIconGrid.SelectItemClosestToPosition(Vector3 worldPos)
	{
		return false;
	}

	bool uGUI_INavigableIconGrid.SelectItemInDirection(int dirX, int dirY)
	{
		if (selectedEntry == null)
		{
			return ((uGUI_INavigableIconGrid)this).SelectFirstItem();
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
		UISelection.sSelectables.Clear();
		Dictionary<TechCategory, CategoryEntry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Dictionary<TechType, uGUI_BlueprintEntry>.Enumerator enumerator2 = enumerator.Current.Value.entries.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				uGUI_BlueprintEntry value = enumerator2.Current.Value;
				if (!(value == null))
				{
					UISelection.sSelectables.Add(value);
				}
			}
		}
		ISelectable selectable = UISelection.FindSelectable(canvas, new Vector2(dirX, -dirY), UISelection.selected, UISelection.sSelectables, fromEdge: false);
		UISelection.sSelectables.Clear();
		uGUI_BlueprintEntry uGUI_BlueprintEntry2 = selectable as uGUI_BlueprintEntry;
		if (uGUI_BlueprintEntry2 != null)
		{
			((uGUI_INavigableIconGrid)this).SelectItem((object)uGUI_BlueprintEntry2);
			return true;
		}
		return false;
	}

	uGUI_INavigableIconGrid uGUI_INavigableIconGrid.GetNavigableGridInDirection(int dirX, int dirY)
	{
		return null;
	}

	public string CompileTimeCheck()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (prefabTitle.GetComponent<RectTransform>() == null)
		{
			stringBuilder.AppendFormat("RectTransform component is expected on {0} prefab assigned to prefabTitle field\n", prefabTitle.name);
		}
		if (prefabTitle.GetComponentInChildren<TextMeshProUGUI>() == null)
		{
			stringBuilder.AppendFormat("Text component is expected on {0} prefab assigned to prefabTitle field\n", prefabTitle.name);
		}
		if (prefabEntry.GetComponent<uGUI_BlueprintEntry>() == null)
		{
			stringBuilder.AppendFormat("uGUI_BlueprintEntry component is expected on {0} prefab assigned to prefabEntry field\n", prefabEntry.name);
		}
		if (pinHover.GetComponent<CanvasGroup>() == null)
		{
			stringBuilder.AppendFormat("CanvasGroup component is expected on {0} GameObject assigned to pinHover field\n", pinHover.name);
		}
		if (stringBuilder.Length != 0)
		{
			return stringBuilder.ToString();
		}
		return null;
	}

	public string CompileTimeCheck(ILanguage language)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (TechCategory value in Enum.GetValues(typeof(TechCategory)))
		{
			string text = language.CheckKey(techCategoryStrings.Get(value));
			if (text != null)
			{
				stringBuilder.Append(text);
			}
		}
		if (stringBuilder.Length <= 0)
		{
			return null;
		}
		return stringBuilder.ToString();
	}

	bool uGUI_IScrollReceiver.OnScroll(float scrollDelta, float speedMultiplier)
	{
		scrollChangedPosition = true;
		scrollRect.Scroll(scrollDelta, speedMultiplier);
		return true;
	}
}
