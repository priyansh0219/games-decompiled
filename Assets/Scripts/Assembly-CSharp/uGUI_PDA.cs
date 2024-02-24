using System.Collections;
using System.Collections.Generic;
using System.Text;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class uGUI_PDA : uGUI_InputGroup, uGUI_IToolbarManager, uGUI_IButtonReceiver, ILocalizationCheckable
{
	public class PDATabComparer : IEqualityComparer<PDATab>
	{
		public bool Equals(PDATab x, PDATab y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(PDATab tab)
		{
			return (int)tab;
		}
	}

	private static PDATabComparer sPDATabComparer = new PDATabComparer();

	private static readonly List<PDATab> regularTabs = new List<PDATab>
	{
		PDATab.Inventory,
		PDATab.Journal,
		PDATab.Ping,
		PDATab.Gallery,
		PDATab.Log,
		PDATab.Encyclopedia
	};

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.UpdateAfterInput;

	private const ManagedUpdate.Queue lateUpdateQueue = ManagedUpdate.Queue.LateUpdateAfterInput;

	[AssertNotNull]
	public uGUI_CanvasScaler canvasScaler;

	[AssertNotNull]
	public CanvasGroup canvasGroup;

	[AssertNotNull]
	public CanvasGroup content;

	[AssertNotNull]
	public uGUI_Toolbar toolbar;

	[AssertNotNull]
	public CanvasGroup toolbarCanvasGroup;

	[AssertNotNull]
	public Image pdaBackground;

	[AssertNotNull]
	public uGUI_PDATab tabIntro;

	[AssertNotNull]
	public uGUI_PDATab tabInventory;

	[AssertNotNull]
	public uGUI_PDATab tabJournal;

	[AssertNotNull]
	public uGUI_PDATab tabPing;

	[AssertNotNull]
	public uGUI_PDATab tabGallery;

	[AssertNotNull]
	public uGUI_PDATab tabLog;

	[AssertNotNull]
	public uGUI_PDATab tabEncyclopedia;

	[AssertNotNull]
	public uGUI_PDATab tabTimeCapsule;

	[AssertNotNull]
	public FMODAsset soundOpen;

	[AssertNotNull]
	public FMODAsset soundClose;

	[AssertNotNull]
	public Button backButton;

	[AssertNotNull]
	public TextMeshProUGUI backButtonText;

	[AssertNotNull]
	public uGUI_Dialog dialog;

	public SoundQueue soundQueue = new SoundQueue();

	private bool initialized;

	private Dictionary<PDATab, uGUI_PDATab> tabs;

	private PDATab tabPrev = regularTabs[0];

	private PDATab tabOpen = PDATab.None;

	private Coroutine revealBackgroundRoutine;

	private Coroutine revealContentRoutine;

	private List<PDATab> currentTabs = new List<PDATab>();

	private List<string> toolbarTooltips = new List<string>();

	private BaseRaycaster quickSlotsParentRaycaster;

	private List<RectMask2D> rectMasks = new List<RectMask2D>();

	private List<ScrollRect> scrollRects = new List<ScrollRect>();

	[SerializeField]
	[AssertNotNull]
	private uGUI_GraphicRaycaster interactionRaycaster;

	public static uGUI_PDA main { get; private set; }

	public bool introActive => tabOpen == PDATab.Intro;

	public uGUI_PDATab currentTab => GetTab(tabOpen);

	public PDATab currentTabType => tabOpen;

	protected override void Awake()
	{
		if (main != null)
		{
			Debug.LogError("uGUI_PDA : Awake() : Multiple instances of uGUI_PDA found in scene!");
			Object.Destroy(base.gameObject);
			return;
		}
		main = this;
		base.Awake();
		quickSlotsParentRaycaster = uGUI.main.quickSlots.GetComponentInParent<BaseRaycaster>();
		DevConsole.RegisterConsoleCommand(this, "pdaintro");
		GetComponentsInChildren(includeInactive: true, rectMasks);
		GetComponentsInChildren(includeInactive: true, scrollRects);
		content.SetVisible(visible: true);
		SetCanvasVisible(visible: false);
		interactionRaycaster.updateRaycasterStatusDelegate = UpdateRaycasterStatus;
	}

	private void SetCanvasVisible(bool visible)
	{
		canvasGroup.SetVisible(visible);
		canvasScaler.active = visible;
		foreach (RectMask2D rectMask in rectMasks)
		{
			rectMask.enabled = visible;
		}
		foreach (ScrollRect scrollRect in scrollRects)
		{
			scrollRect.enabled = visible;
		}
	}

	private void Start()
	{
		Language.OnLanguageChanged += OnLanguageChanged;
		GameInput.OnBindingsChanged += OnBindingsChanged;
		OnLanguageChanged();
		OnBindingsChanged();
	}

	protected override void Update()
	{
		soundQueue.Update();
		if (!base.selected && Player.main.GetPDA().isOpen && AvatarInputHandler.main.IsEnabled())
		{
			Select();
		}
		if (!uGUI.isIntro)
		{
			FPSInputModule.current.EscapeMenu();
		}
	}

	private void OnDestroy()
	{
		Language.OnLanguageChanged -= OnLanguageChanged;
		GameInput.OnBindingsChanged -= OnBindingsChanged;
	}

	private void OnLanguageChanged()
	{
		CacheToolbarTooltips();
		foreach (KeyValuePair<PDATab, uGUI_PDATab> tab in tabs)
		{
			tab.Value.OnLanguageChanged();
		}
	}

	private void OnBindingsChanged()
	{
		foreach (KeyValuePair<PDATab, uGUI_PDATab> tab in tabs)
		{
			tab.Value.OnBindingsChanged();
		}
	}

	private void CacheToolbarTooltips()
	{
		StringBuilder stringBuilder = new StringBuilder();
		toolbarTooltips.Clear();
		for (int i = 0; i < currentTabs.Count; i++)
		{
			stringBuilder.Length = 0;
			TooltipFactory.Label($"Tab{currentTabs[i].ToString()}", stringBuilder);
			toolbarTooltips.Add(stringBuilder.ToString());
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

	public void Initialize()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		tabs = new Dictionary<PDATab, uGUI_PDATab>(sPDATabComparer)
		{
			{
				PDATab.Intro,
				tabIntro
			},
			{
				PDATab.Inventory,
				tabInventory
			},
			{
				PDATab.Journal,
				tabJournal
			},
			{
				PDATab.Ping,
				tabPing
			},
			{
				PDATab.Gallery,
				tabGallery
			},
			{
				PDATab.Log,
				tabLog
			},
			{
				PDATab.Encyclopedia,
				tabEncyclopedia
			},
			{
				PDATab.TimeCapsule,
				tabTimeCapsule
			}
		};
		foreach (KeyValuePair<PDATab, uGUI_PDATab> tab in tabs)
		{
			uGUI_PDATab value = tab.Value;
			value.Register(this);
			value.Close();
		}
		backButton.gameObject.SetActive(value: false);
		SetTabs(regularTabs);
	}

	public void WarmUp()
	{
		foreach (KeyValuePair<PDATab, uGUI_PDATab> tab in tabs)
		{
			tab.Value.OnWarmUp();
		}
	}

	public void SetTabs(List<PDATab> tabs)
	{
		int num = tabs?.Count ?? 0;
		Sprite[] array = new Sprite[num];
		currentTabs.Clear();
		for (int i = 0; i < num; i++)
		{
			PDATab item = tabs[i];
			array[i] = SpriteManager.Get(SpriteManager.Group.Tab, $"Tab{item.ToString()}");
			currentTabs.Add(item);
		}
		uGUI_Toolbar obj = toolbar;
		object[] array2 = array;
		obj.Initialize(this, array2);
		CacheToolbarTooltips();
	}

	public void OnOpenPDA(PDATab tabId)
	{
		SetCanvasVisible(visible: true);
		content.interactable = false;
		content.blocksRaycasts = false;
		content.alpha = 1f;
		if (!introActive)
		{
			RuntimeManager.PlayOneShot(soundOpen.path);
		}
		bool flag = tabId == PDATab.None;
		if (flag)
		{
			uGUI_PopupNotification uGUI_PopupNotification2 = uGUI_PopupNotification.main;
			if (uGUI_PopupNotification2.isShowingMessage && !string.IsNullOrEmpty(uGUI_PopupNotification2.id))
			{
				string id = uGUI_PopupNotification2.id;
				if (id == "PDAEncyclopediaTab")
				{
					tabId = PDATab.Encyclopedia;
				}
			}
			if (tabId == PDATab.None && tabOpen == PDATab.None)
			{
				tabId = tabPrev;
			}
		}
		if (tabId == PDATab.TimeCapsule)
		{
			SetTabs(null);
			Inventory.main.SetUsedStorage(PlayerTimeCapsule.main.container);
			uGUI_GalleryTab obj = GetTab(PDATab.Gallery) as uGUI_GalleryTab;
			uGUI_TimeCapsuleTab @object = GetTab(PDATab.TimeCapsule) as uGUI_TimeCapsuleTab;
			obj.SetSelectListener(@object.SelectImage, "ScreenshotSelect", "ScreenshotSelectTooltip");
		}
		foreach (KeyValuePair<PDATab, uGUI_PDATab> tab in tabs)
		{
			tab.Value.OnOpenPDA(tabId, flag);
		}
		OpenTab(tabId);
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateAfterInput, OnLateUpdate);
	}

	public void OnClosePDA()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdateAfterInput, OnLateUpdate);
		RuntimeManager.PlayOneShot(soundClose.path);
		if (tabOpen != PDATab.None)
		{
			tabs[tabOpen].Close();
			tabOpen = PDATab.None;
		}
		foreach (KeyValuePair<PDATab, uGUI_PDATab> tab in tabs)
		{
			tab.Value.OnClosePDA();
		}
		Deselect();
		SetTabs(regularTabs);
		content.SetVisible(visible: false);
	}

	public void OnPDAOpened()
	{
		content.interactable = true;
		content.blocksRaycasts = true;
	}

	public void OnPDAClosed()
	{
		SetCanvasVisible(visible: false);
	}

	private void OnUpdate()
	{
		foreach (KeyValuePair<PDATab, uGUI_PDATab> tab in tabs)
		{
			tab.Value.OnUpdate(tab.Key == tabOpen);
		}
	}

	private void OnLateUpdate()
	{
		foreach (KeyValuePair<PDATab, uGUI_PDATab> tab in tabs)
		{
			tab.Value.OnLateUpdate(tab.Key == tabOpen);
		}
		for (int i = 0; i < currentTabs.Count; i++)
		{
			PDATab key = currentTabs[i];
			uGUI_PDATab uGUI_PDATab2 = tabs[key];
			toolbar.SetNotificationsAmount(i, uGUI_PDATab2.notificationsCount);
		}
	}

	public void OpenTab(PDATab tabId)
	{
		if (tabId != tabOpen && tabId >= PDATab.Intro && tabs.TryGetValue(tabId, out var value))
		{
			if (tabOpen != PDATab.None)
			{
				tabs[tabOpen].Close();
			}
			if (backButton.gameObject.activeSelf)
			{
				backButton.onClick.RemoveAllListeners();
				backButton.gameObject.SetActive(value: false);
			}
			tabOpen = tabId;
			value.Open();
			if (regularTabs.Contains(tabId))
			{
				tabPrev = tabId;
			}
			int num = currentTabs.IndexOf(tabId);
			if (num != -1)
			{
				toolbarCanvasGroup.alpha = 1f;
				toolbarCanvasGroup.interactable = true;
				toolbarCanvasGroup.blocksRaycasts = true;
				toolbar.Select(num);
			}
			else
			{
				toolbarCanvasGroup.alpha = 0f;
				toolbarCanvasGroup.interactable = false;
				toolbarCanvasGroup.blocksRaycasts = false;
			}
			GamepadInputModule.current.SetCurrentGrid(currentTab.GetInitialGrid());
		}
	}

	public uGUI_PDATab GetTab(PDATab tabId)
	{
		if (tabId == PDATab.None)
		{
			return null;
		}
		if (tabs.TryGetValue(tabId, out var value))
		{
			return value;
		}
		return null;
	}

	public PDATab GetNextTab()
	{
		int num = currentTabs.IndexOf(tabOpen);
		int count = currentTabs.Count;
		int num2 = ((num >= 0) ? (num + 1) : 0);
		if (num2 >= count)
		{
			num2 = 0;
		}
		for (int num3 = num2; num3 != num; num3 = ((num3 + 1 < count) ? (num3 + 1) : 0))
		{
			PDATab pDATab = currentTabs[num3];
			if (tabs.ContainsKey(pDATab))
			{
				return pDATab;
			}
		}
		return PDATab.None;
	}

	public PDATab GetPreviousTab()
	{
		int num = currentTabs.IndexOf(tabOpen);
		int count = currentTabs.Count;
		int num2 = ((num < 0) ? (count - 1) : (num - 1));
		if (num2 < 0)
		{
			num2 = count - 1;
		}
		for (int num3 = num2; num3 != num; num3 = ((num3 - 1 < 0) ? (count - 1) : (num3 - 1)))
		{
			PDATab pDATab = currentTabs[num3];
			if (tabs.ContainsKey(pDATab))
			{
				return pDATab;
			}
		}
		return PDATab.None;
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
		if (button == 0 && index >= 0 && index < currentTabs.Count)
		{
			OpenTab(currentTabs[index]);
		}
	}

	private void OnConsoleCommand_pdaintro(NotificationCenter.Notification n)
	{
		OpenTab(PDATab.Intro);
	}

	public void SetBackgroundAlpha(float alpha)
	{
		Color color = pdaBackground.color;
		color.a = Mathf.Clamp01(alpha);
		pdaBackground.color = color;
	}

	public void RevealBackground()
	{
		if (revealBackgroundRoutine == null)
		{
			revealBackgroundRoutine = StartCoroutine(RevealBackgroundRoutine());
		}
	}

	public void RevealContent()
	{
		if (revealContentRoutine == null)
		{
			revealContentRoutine = StartCoroutine(RevealContentRoutine());
		}
	}

	private IEnumerator RevealBackgroundRoutine()
	{
		SetBackgroundAlpha(0f);
		for (float alpha = 0f; alpha < 1f; alpha += PDA.deltaTime / 0.333333f)
		{
			SetBackgroundAlpha(alpha);
			yield return null;
		}
		SetBackgroundAlpha(1f);
		revealBackgroundRoutine = null;
	}

	private IEnumerator RevealContentRoutine()
	{
		content.SetVisible(visible: false);
		for (float alpha = 0f; alpha < 1f; alpha += PDA.deltaTime * 5f)
		{
			content.alpha = Mathf.Clamp01(alpha);
			yield return null;
		}
		content.SetVisible(visible: true);
		revealContentRoutine = null;
	}

	public override void OnSelect(bool lockMovement)
	{
		base.OnSelect(lockMovement);
		uGUI_INavigableIconGrid currentGrid = null;
		if (currentTab != null)
		{
			currentGrid = currentTab.GetInitialGrid();
		}
		GamepadInputModule.current.SetCurrentGrid(currentGrid);
	}

	public override void OnDeselect()
	{
		base.OnDeselect();
		GamepadInputModule.current.SetCurrentGrid(null);
		Inventory.main.quickSlots.EndAssign();
	}

	public override bool Raycast(PointerEventData eventData, List<RaycastResult> raycastResults)
	{
		bool num = base.Raycast(eventData, raycastResults);
		if (num && quickSlotsParentRaycaster != null)
		{
			quickSlotsParentRaycaster.Raycast(eventData, raycastResults);
		}
		return num;
	}

	public bool OnButtonDown(GameInput.Button button)
	{
		uGUI_PDATab uGUI_PDATab2 = currentTab;
		if (uGUI_PDATab2 != null)
		{
			return uGUI_PDATab2.OnButtonDown(button);
		}
		return false;
	}

	public string CompileTimeCheck(ILanguage language)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < regularTabs.Count; i++)
		{
			string key = $"Tab{regularTabs[i].ToString()}";
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
