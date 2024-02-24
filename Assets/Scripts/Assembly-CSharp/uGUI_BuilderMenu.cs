using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class uGUI_BuilderMenu : uGUI_InputGroup, uGUI_IIconGridManager, uGUI_IToolbarManager, uGUI_IButtonReceiver, INotificationListener, ILocalizationCheckable
{
	private const string prefabPath = "uGUI_BuilderMenu.prefab";

	[AssertLocalization]
	private const string craftingLabelKey = "CraftingLabel";

	private static readonly List<TechGroup> groups = new List<TechGroup>
	{
		TechGroup.BasePieces,
		TechGroup.ExteriorModules,
		TechGroup.InteriorPieces,
		TechGroup.InteriorModules,
		TechGroup.Miscellaneous
	};

	private static readonly List<TechType> techTypesToCache = new List<TechType>
	{
		TechType.BaseHatch,
		TechType.BaseConnector,
		TechType.BaseUpgradeConsole,
		TechType.Aquarium,
		TechType.BaseBulkhead,
		TechType.BaseWindow,
		TechType.BaseReinforcement
	};

	private const NotificationManager.Group notificationGroup = NotificationManager.Group.Builder;

	private static uGUI_BuilderMenu singleton;

	private static readonly List<TechType>[] groupsTechTypes = new List<TechType>[groups.Count];

	private static Dictionary<TechType, int> techTypeToTechGroupIdx = new Dictionary<TechType, int>();

	private static bool groupsTechTypesInitialized = false;

	[AssertNotNull]
	public uGUI_CanvasScaler canvasScaler;

	[AssertNotNull]
	public TextMeshProUGUI title;

	[AssertNotNull]
	public uGUI_Toolbar toolbar;

	[AssertNotNull]
	public uGUI_IconGrid iconGrid;

	[AssertNotNull]
	public GameObject content;

	[Range(1f, 256f)]
	public float iconSize = 64f;

	[Range(0f, 64f)]
	public float minSpaceX = 20f;

	[Range(0f, 64f)]
	public float minSpaceY = 20f;

	private Dictionary<string, TechType> items = new Dictionary<string, TechType>();

	private int openInFrame = -1;

	private new int selected;

	private CachedEnumString<TechGroup> techGroupNames = new CachedEnumString<TechGroup>(CraftData.sTechGroupComparer);

	private List<string> toolbarTooltips = new List<string>();

	private int[] groupNotificationCounts = new int[groups.Count];

	private Dictionary<TechType, GameObject> techTypePrefabCache = new Dictionary<TechType, GameObject>();

	[SerializeField]
	[AssertNotNull]
	private uGUI_GraphicRaycaster interactionRaycaster;

	private Coroutine beginCoroutine;

	public bool state { get; private set; }

	public int TabOpen => selected;

	public int TabCount => groups.Count;

	void INotificationListener.OnAdd(NotificationManager.Group group, string key)
	{
		TechType techType = key.DecodeKey();
		if (KnownTech.Contains(techType))
		{
			int techTypeTechGroupIdx = GetTechTypeTechGroupIdx(techType);
			if (techTypeTechGroupIdx >= 0)
			{
				groupNotificationCounts[techTypeTechGroupIdx]++;
			}
		}
	}

	void INotificationListener.OnRemove(NotificationManager.Group group, string key)
	{
		TechType techType = key.DecodeKey();
		if (KnownTech.Contains(techType))
		{
			int techTypeTechGroupIdx = GetTechTypeTechGroupIdx(techType);
			if (techTypeTechGroupIdx >= 0)
			{
				groupNotificationCounts[techTypeTechGroupIdx]--;
			}
		}
	}

	public static bool IsOpen()
	{
		if (singleton != null)
		{
			return singleton.state;
		}
		return false;
	}

	protected override void Awake()
	{
		if (singleton != null)
		{
			Debug.LogError("Multiple uGUI_BuilderMenu instances found in scene!", this);
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		singleton = this;
		base.Awake();
		EnsureTechGroupTechTypeDataInitialized();
		ClearNotificationCounts();
		iconGrid.iconSize = new Vector2(iconSize, iconSize);
		iconGrid.minSpaceX = minSpaceX;
		iconGrid.minSpaceY = minSpaceY;
		iconGrid.Initialize(this);
		int count = groups.Count;
		Sprite[] array = new Sprite[count];
		for (int i = 0; i < count; i++)
		{
			TechGroup value = groups[i];
			string text = techGroupNames.Get(value);
			array[i] = SpriteManager.Get(SpriteManager.Group.Tab, "group" + text);
		}
		uGUI_Toolbar obj = toolbar;
		object[] array2 = array;
		obj.Initialize(this, array2);
		toolbar.Select(selected);
		UpdateItems();
		KnownTech.onChanged += OnChanged;
		PDAScanner.onAdd = (PDAScanner.OnEntryEvent)Delegate.Combine(PDAScanner.onAdd, new PDAScanner.OnEntryEvent(OnLockedAdd));
		PDAScanner.onRemove = (PDAScanner.OnEntryEvent)Delegate.Combine(PDAScanner.onRemove, new PDAScanner.OnEntryEvent(OnLockedRemove));
		NotificationManager.main.Subscribe(this, NotificationManager.Group.Builder, string.Empty);
		interactionRaycaster.updateRaycasterStatusDelegate = UpdateRaycasterStatus;
		StartCoroutine(InitializePrefabCache());
	}

	private IEnumerator InitializePrefabCache()
	{
		foreach (TechType techType in techTypesToCache)
		{
			CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
			yield return request;
			GameObject result = request.GetResult();
			if (result != null)
			{
				techTypePrefabCache.Add(techType, result);
				continue;
			}
			Debug.LogWarningFormat("Failed to cached prefab for Tech Type = {0}", techType);
		}
	}

	private List<TechType> GetTechTypesForGroup(int groupIdx)
	{
		return groupsTechTypes[groupIdx];
	}

	private void Start()
	{
		Language.OnLanguageChanged += OnLanguageChanged;
		OnLanguageChanged();
	}

	protected override void Update()
	{
		base.Update();
		if (state && openInFrame != Time.frameCount)
		{
			bool flag = GameInput.GetButtonDown(GameInput.Button.UICancel) || !base.focused;
			if (!flag && GameInput.PrimaryDevice == GameInput.Device.Keyboard && GameInput.GetButtonDown(GameInput.Button.RightHand))
			{
				bool flag2 = "<Mouse>/leftButton" == GameInput.GetBinding(GameInput.Device.Keyboard, GameInput.Button.RightHand, GameInput.BindingSet.Primary) || "<Mouse>/leftButton" == GameInput.GetBinding(GameInput.Device.Keyboard, GameInput.Button.RightHand, GameInput.BindingSet.Secondary);
				flag = GameInput.GetButtonDown(GameInput.Button.RightHand) && !flag2;
			}
			if (flag)
			{
				Close();
			}
		}
	}

	private void LateUpdate()
	{
		if (state)
		{
			UpdateToolbarNotificationNumbers();
		}
	}

	private void OnDestroy()
	{
		NotificationManager.main.Unsubscribe(this);
		KnownTech.onChanged -= OnChanged;
		PDAScanner.onAdd = (PDAScanner.OnEntryEvent)Delegate.Remove(PDAScanner.onAdd, new PDAScanner.OnEntryEvent(OnLockedAdd));
		PDAScanner.onRemove = (PDAScanner.OnEntryEvent)Delegate.Remove(PDAScanner.onRemove, new PDAScanner.OnEntryEvent(OnLockedRemove));
		Language.OnLanguageChanged -= OnLanguageChanged;
		singleton = null;
	}

	public override void OnSelect(bool lockMovement)
	{
		InterruptBeginAsync();
		base.OnSelect(lockMovement);
		GamepadInputModule.current.SetCurrentGrid(iconGrid);
	}

	public override void OnDeselect()
	{
		InterruptBeginAsync();
		base.OnDeselect();
		Close();
	}

	public static void Show()
	{
		uGUI_BuilderMenu instance = GetInstance();
		if (instance != null)
		{
			instance.Open();
		}
	}

	public static void Hide()
	{
		uGUI_BuilderMenu instance = GetInstance();
		if (instance != null)
		{
			instance.Close();
		}
	}

	public static IEnumerator EnsureCreatedAsync()
	{
		return CreateInstanceAsync();
	}

	public void Open()
	{
		if (!state)
		{
			UpdateToolbarNotificationNumbers();
			MainCameraControl.main.SaveLockedVRViewModelAngle();
			SetState(newState: true);
			openInFrame = Time.frameCount;
		}
	}

	public void Close()
	{
		if (state)
		{
			SetState(newState: false);
		}
	}

	private void OnChanged()
	{
		UpdateItems();
	}

	private void OnLockedAdd(PDAScanner.Entry entry)
	{
		UpdateItems();
	}

	private void OnLockedRemove(PDAScanner.Entry entry)
	{
		UpdateItems();
	}

	public void GetToolbarTooltip(int index, TooltipData data)
	{
		if (index >= 0 && index < toolbarTooltips.Count)
		{
			data.prefix.Append(toolbarTooltips[index]);
		}
	}

	public void OnToolbarClick(int index, int button)
	{
		if (button == 0)
		{
			SetCurrentTab(index);
		}
	}

	public void GetTooltip(string id, TooltipData data)
	{
		if (items.TryGetValue(id, out var value))
		{
			bool locked = !CrafterLogic.IsCraftRecipeUnlocked(value);
			TooltipFactory.BuilderItem(value, locked, data);
		}
	}

	public void OnPointerEnter(string id)
	{
	}

	public void OnPointerExit(string id)
	{
	}

	public void OnPointerClick(string id, int button)
	{
		if (button == 0 && items.TryGetValue(id, out var value) && KnownTech.Contains(value))
		{
			SetState(newState: false);
			beginCoroutine = StartCoroutine(BeginAsync(value));
		}
	}

	private void InterruptBeginAsync()
	{
		if (beginCoroutine != null)
		{
			StopCoroutine(beginCoroutine);
			beginCoroutine = null;
		}
	}

	private IEnumerator BeginAsync(TechType techType)
	{
		GameObject gameObject = TryGetCachedPrefab(techType);
		if (gameObject != null)
		{
			Builder.Begin(techType, gameObject);
		}
		else
		{
			yield return Builder.BeginAsync(techType);
		}
		beginCoroutine = null;
	}

	private GameObject TryGetCachedPrefab(TechType techType)
	{
		if (techTypePrefabCache.TryGetValue(techType, out var value))
		{
			return value;
		}
		return null;
	}

	public void OnSortRequested()
	{
	}

	private void UpdateToolbarNotificationNumbers()
	{
		for (int i = 0; i < groups.Count; i++)
		{
			toolbar.SetNotificationsAmount(i, groupNotificationCounts[i]);
		}
	}

	private static void EnsureTechGroupTechTypeDataInitialized()
	{
		if (groupsTechTypesInitialized)
		{
			return;
		}
		for (int i = 0; i < groups.Count; i++)
		{
			groupsTechTypes[i] = new List<TechType>();
			List<TechType> list = groupsTechTypes[i];
			CraftData.GetBuilderGroupTech(groups[i], list);
			for (int j = 0; j < list.Count; j++)
			{
				TechType key = list[j];
				techTypeToTechGroupIdx.Add(key, i);
			}
		}
		groupsTechTypesInitialized = true;
	}

	private void ClearNotificationCounts()
	{
		_ = NotificationManager.main;
		for (int i = 0; i < groups.Count; i++)
		{
			groupNotificationCounts[i] = 0;
		}
	}

	private int GetTechTypeTechGroupIdx(TechType inTechType)
	{
		if (!techTypeToTechGroupIdx.TryGetValue(inTechType, out var value))
		{
			return -1;
		}
		return value;
	}

	private void CacheToolbarTooltips()
	{
		StringBuilder stringBuilder = new StringBuilder();
		toolbarTooltips.Clear();
		for (int i = 0; i < groups.Count; i++)
		{
			stringBuilder.Length = 0;
			TechGroup value = groups[i];
			TooltipFactory.Label($"Group{techGroupNames.Get(value)}", stringBuilder);
			toolbarTooltips.Add(stringBuilder.ToString());
		}
	}

	private void OnLanguageChanged()
	{
		title.text = Language.main.Get("CraftingLabel");
		CacheToolbarTooltips();
	}

	private static uGUI_BuilderMenu GetInstance()
	{
		if (singleton == null)
		{
			throw new Exception("uGUI_BuilderMenu not created!");
		}
		return singleton;
	}

	private static IEnumerator CreateInstanceAsync()
	{
		AsyncOperationHandle<GameObject> loadRequest = AddressablesUtility.LoadAsync<GameObject>("uGUI_BuilderMenu.prefab");
		yield return loadRequest;
		if (loadRequest.Status == AsyncOperationStatus.Failed)
		{
			Debug.LogError("Cannot find main uGUI_BuilderMenu prefab in Resources folder at path 'uGUI_BuilderMenu.prefab'");
			Debug.Break();
		}
		else
		{
			UnityEngine.Object.Instantiate(loadRequest.Result);
			singleton.state = true;
			singleton.SetState(newState: false);
		}
	}

	private void SetState(bool newState)
	{
		if (state == newState)
		{
			return;
		}
		state = newState;
		if (state)
		{
			canvasScaler.SetAnchor();
			content.SetActive(value: true);
			if (!base.focused)
			{
				Select();
			}
		}
		else
		{
			if (base.focused)
			{
				Deselect();
			}
			iconGrid.DeselectItem();
			content.SetActive(value: false);
		}
	}

	private void SetCurrentTab(int index)
	{
		if (index >= 0 && index < groups.Count && index != selected)
		{
			toolbar.Select(index);
			selected = index;
			UpdateItems();
			iconGrid.UpdateNow();
			GamepadInputModule.current.SetCurrentGrid(iconGrid);
		}
	}

	private void UpdateItems()
	{
		iconGrid.Clear();
		items.Clear();
		_ = groups[selected];
		List<TechType> techTypesForGroup = GetTechTypesForGroup(selected);
		int num = 0;
		for (int i = 0; i < techTypesForGroup.Count; i++)
		{
			TechType techType = techTypesForGroup[i];
			TechUnlockState techUnlockState = KnownTech.GetTechUnlockState(techType);
			if (techUnlockState == TechUnlockState.Available || techUnlockState == TechUnlockState.Locked)
			{
				string stringForInt = IntStringCache.GetStringForInt(num);
				items.Add(stringForInt, techType);
				iconGrid.AddItem(stringForInt, SpriteManager.Get(techType), SpriteManager.GetBackground(techType), techUnlockState == TechUnlockState.Locked, num);
				iconGrid.RegisterNotificationTarget(stringForInt, NotificationManager.Group.Builder, techType.EncodeKey());
				num++;
			}
		}
	}

	private void UpdateRaycasterStatus(uGUI_GraphicRaycaster raycaster)
	{
		if (GameInput.IsPrimaryDeviceGamepad() && !VROptions.GetUseGazeBasedCursor())
		{
			raycaster.enabled = false;
		}
		else
		{
			raycaster.enabled = base.focused;
		}
	}

	public bool OnButtonDown(GameInput.Button button)
	{
		switch (button)
		{
		case GameInput.Button.UINextTab:
		{
			int currentTab2 = (TabOpen + 1) % TabCount;
			SetCurrentTab(currentTab2);
			return true;
		}
		case GameInput.Button.UIPrevTab:
		{
			int currentTab = (TabOpen - 1 + TabCount) % TabCount;
			SetCurrentTab(currentTab);
			return true;
		}
		case GameInput.Button.UICancel:
			Close();
			return true;
		default:
			return false;
		}
	}

	public string CompileTimeCheck(ILanguage language)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < groups.Count; i++)
		{
			TechGroup value = groups[i];
			string key = $"Group{techGroupNames.Get(value)}";
			string text = language.CheckKey(key);
			if (text != null)
			{
				stringBuilder.AppendLine(text);
			}
		}
		if (stringBuilder.Length != 0)
		{
			return stringBuilder.ToString();
		}
		return null;
	}
}
