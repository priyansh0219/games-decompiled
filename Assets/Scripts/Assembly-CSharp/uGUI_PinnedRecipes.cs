using System.Collections.Generic;
using UnityEngine;

public class uGUI_PinnedRecipes : MonoBehaviour
{
	public enum Mode
	{
		Off = 0,
		Blueprints = 1,
		Full = 2
	}

	[AssertNotNull]
	public GameObject prefabEntry;

	[AssertNotNull]
	public RectTransform canvas;

	[AssertNotNull]
	public CanvasGroup canvasGroup;

	public Color colorGreen;

	public Color colorRed;

	public float entryHeight = 100f;

	private const ManagedUpdate.Queue queueUpdate = ManagedUpdate.Queue.UpdateAfterInput;

	private const ManagedUpdate.Queue queueUpdateMax = ManagedUpdate.Queue.PreCanvasRectTransform;

	private const float minAspect = 1.62f;

	private const float bottomGap = 330f;

	private const float dragLerpSpeed = 10f;

	private bool initialized;

	private bool ingredientsDirty;

	private List<uGUI_RecipeEntry> entries = new List<uGUI_RecipeEntry>();

	private Mode mode;

	private int dragIndex = -1;

	private Vector2 dragOffset;

	private Vector2 cursor;

	private bool pingOnClick = true;

	private PrefabPool<uGUI_RecipeEntry> pool;

	private void Start()
	{
		pool = new PrefabPool<uGUI_RecipeEntry>(prefabEntry, canvas, 10, 4, delegate(uGUI_RecipeEntry entry)
		{
			entry.manager = this;
			entry.Deinitialize();
		}, delegate(uGUI_RecipeEntry entry)
		{
			entry.Deinitialize();
		});
		OnUIScaleChange(uGUI_CanvasScaler.uiScale);
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
		uGUI_CanvasScaler.AddUIScaleListener(OnUIScaleChange);
	}

	private void OnDisable()
	{
		Deinitialize();
	}

	private void OnDestroy()
	{
		uGUI_CanvasScaler.RemoveUIScaleListener(OnUIScaleChange);
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
	}

	public void OnPointerClick(uGUI_RecipeEntry entry)
	{
		if (pingOnClick)
		{
			uGUI_PDA main = uGUI_PDA.main;
			if (main != null && main.currentTabType == PDATab.Journal)
			{
				uGUI_BlueprintsTab uGUI_BlueprintsTab2 = main.GetTab(PDATab.Journal) as uGUI_BlueprintsTab;
				if (uGUI_BlueprintsTab2 != null)
				{
					uGUI_BlueprintsTab2.Ping(entry.techType);
				}
			}
		}
		pingOnClick = true;
	}

	public void OnBeginDrag(uGUI_RecipeEntry entry)
	{
		int num = entries.IndexOf(entry);
		if (num >= 0)
		{
			dragIndex = num;
			entry.rectTransform.SetAsLastSibling();
			UpdateCursorPosition();
			dragOffset = entry.rectTransform.anchoredPosition - cursor;
			pingOnClick = false;
		}
	}

	public void OnEndDrag(uGUI_RecipeEntry entry)
	{
		if (entries.IndexOf(entry) >= 0 && dragIndex >= 0)
		{
			int snapIndex = GetSnapIndex();
			if (snapIndex >= 0)
			{
				PinManager.Move(dragIndex, snapIndex);
			}
			else
			{
				PinManager.SetPin(entry.techType, value: false);
			}
			dragIndex = -1;
			pingOnClick = true;
		}
	}

	public void OnDrop(uGUI_RecipeEntry entry)
	{
		entries.IndexOf(entry);
		_ = 0;
	}

	private void Initialize()
	{
		if (initialized)
		{
			return;
		}
		ItemsContainer container = GetContainer();
		if (container == null)
		{
			return;
		}
		initialized = true;
		container.onAddItem += OnInventoryChange;
		container.onRemoveItem += OnInventoryChange;
		PinManager.onAdd += OnPinAdd;
		PinManager.onRemove += OnPinRemove;
		PinManager.onMove += OnMove;
		using (IEnumerator<TechType> enumerator = PinManager.GetPins())
		{
			while (enumerator.MoveNext())
			{
				OnPinAdd(enumerator.Current);
			}
		}
	}

	private void Deinitialize()
	{
		if (initialized)
		{
			initialized = false;
			ItemsContainer container = GetContainer();
			if (container != null)
			{
				container.onAddItem -= OnInventoryChange;
				container.onRemoveItem -= OnInventoryChange;
			}
			PinManager.onAdd -= OnPinAdd;
			PinManager.onRemove -= OnPinRemove;
			PinManager.onMove -= OnMove;
			Clear();
		}
	}

	private void OnInventoryChange(InventoryItem item)
	{
		ingredientsDirty = true;
	}

	private void OnPinAdd(TechType techType)
	{
		uGUI_RecipeEntry uGUI_RecipeEntry2 = pool.Get();
		uGUI_RecipeEntry2.Initialize(techType);
		uGUI_RecipeEntry2.UpdateIngredients(GetContainer(), ping: false);
		uGUI_RecipeEntry2.SetMode(mode);
		uGUI_RecipeEntry2.rectTransform.anchoredPosition = new Vector2(0f, (0f - entryHeight) * (float)entries.Count);
		entries.Add(uGUI_RecipeEntry2);
	}

	private void OnPinRemove(TechType techType)
	{
		for (int num = entries.Count - 1; num >= 0; num--)
		{
			uGUI_RecipeEntry uGUI_RecipeEntry2 = entries[num];
			if (uGUI_RecipeEntry2.techType == techType)
			{
				entries.RemoveAt(num);
				pool.Release(uGUI_RecipeEntry2);
			}
		}
	}

	private void OnMove(int oldIndex, int newIndex)
	{
		uGUI_RecipeEntry item = entries[oldIndex];
		entries.RemoveAt(oldIndex);
		entries.Insert(newIndex, item);
	}

	private ItemsContainer GetContainer()
	{
		Inventory main = Inventory.main;
		if (main == null)
		{
			return null;
		}
		return main.container;
	}

	private void OnUpdate()
	{
		Initialize();
		if (initialized)
		{
			if (GetContainer() == null)
			{
				Deinitialize();
				return;
			}
			bool flag = IsInteractable();
			canvasGroup.interactable = flag;
			canvasGroup.blocksRaycasts = flag;
			UpdateMode();
			UpdateIngredients();
			UpdatePositions();
		}
	}

	private void OnUIScaleChange(float scale)
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.PreCanvasRectTransform, UpdateMaxPins);
	}

	private void UpdateMaxPins()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.PreCanvasRectTransform, UpdateMaxPins);
		uGUI_CanvasScaler componentInParent = GetComponentInParent<uGUI_CanvasScaler>();
		if (componentInParent != null)
		{
			float height = componentInParent.GetComponent<RectTransform>().rect.height;
			float num = 0f - canvas.anchoredPosition.y;
			PinManager.max = Mathf.FloorToInt((height - num - 330f) / entryHeight + 0.5f);
		}
	}

	private void UpdateMode()
	{
		Mode mode = GetMode();
		if (this.mode != mode)
		{
			this.mode = mode;
			for (int i = 0; i < entries.Count; i++)
			{
				entries[i].SetMode(this.mode);
			}
		}
	}

	private void UpdateIngredients()
	{
		if (ingredientsDirty)
		{
			ingredientsDirty = false;
			ItemsContainer container = GetContainer();
			for (int i = 0; i < entries.Count; i++)
			{
				entries[i].UpdateIngredients(container, ping: true);
			}
		}
	}

	private void UpdatePositions()
	{
		int snapIndex = GetSnapIndex();
		for (int i = 0; i < entries.Count; i++)
		{
			uGUI_RecipeEntry obj = entries[i];
			int num = i;
			if (dragIndex >= 0)
			{
				if (snapIndex >= 0)
				{
					if (snapIndex > dragIndex)
					{
						if (i > snapIndex)
						{
							num++;
						}
					}
					else if (i >= snapIndex)
					{
						num++;
					}
				}
				if (i >= dragIndex)
				{
					num--;
				}
			}
			Vector2 anchoredPosition = obj.rectTransform.anchoredPosition;
			if (i == dragIndex)
			{
				dragOffset = Vector2.Lerp(dragOffset, Vector2.zero, 10f * PDA.deltaTime);
				anchoredPosition = cursor + dragOffset;
			}
			else
			{
				anchoredPosition = Vector2.Lerp(anchoredPosition, new Vector2(0f, (0f - entryHeight) * (float)num), 10f * PDA.deltaTime);
			}
			obj.rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	private int GetSnapIndex()
	{
		int result = -1;
		if (dragIndex >= 0)
		{
			UpdateCursorPosition();
			if (Mathf.Abs(cursor.x) < entryHeight)
			{
				result = Mathf.Clamp(Mathf.FloorToInt((0.5f * entryHeight - cursor.y) / entryHeight), 0, entries.Count - 1);
			}
		}
		return result;
	}

	private void UpdateCursorPosition()
	{
		RectTransform rt = canvas;
		if (CursorManager.GetPointerInfo(ref rt, out var localPosition, out var _, out var _, out var _))
		{
			cursor = localPosition;
		}
	}

	private void Clear()
	{
		for (int num = entries.Count - 1; num >= 0; num--)
		{
			uGUI_RecipeEntry entry = entries[num];
			pool.Release(entry);
		}
		entries.Clear();
	}

	private float GetScreenAspect()
	{
		Vector2Int screenSize = GraphicsUtil.GetScreenSize();
		return (float)screenSize.x / (float)screenSize.y;
	}

	private bool IsInteractable()
	{
		if (!initialized)
		{
			return false;
		}
		if (GetScreenAspect() < 1.62f)
		{
			return false;
		}
		Player main = Player.main;
		if (main == null)
		{
			return false;
		}
		PDA pDA = main.GetPDA();
		if (pDA == null)
		{
			return false;
		}
		if (pDA.isInUse)
		{
			return true;
		}
		if (uGUI.main.craftingMenu.selected)
		{
			return true;
		}
		return false;
	}

	private Mode GetMode()
	{
		if (!initialized)
		{
			return Mode.Off;
		}
		if (!uGUI.isMainLevel)
		{
			return Mode.Off;
		}
		if (LaunchRocket.isLaunching)
		{
			return Mode.Off;
		}
		Player main = Player.main;
		if (main == null || main.cinematicModeActive)
		{
			return Mode.Off;
		}
		PDA pDA = main.GetPDA();
		if (pDA == null)
		{
			return Mode.Off;
		}
		if (uGUI.isIntro && pDA.ui.currentTabType == PDATab.Intro)
		{
			return Mode.Off;
		}
		uGUI_CameraDrone main2 = uGUI_CameraDrone.main;
		if (main2 != null && main2.GetCamera() != null)
		{
			return Mode.Off;
		}
		if (uGUI.main.craftingMenu.selected)
		{
			return Mode.Full;
		}
		if (uGUI_BuilderMenu.IsOpen())
		{
			return Mode.Blueprints;
		}
		if (main.GetMode() == Player.Mode.Piloting)
		{
			return Mode.Blueprints;
		}
		if (main.GetVehicle() != null)
		{
			return Mode.Full;
		}
		if (pDA.isInUse)
		{
			if (!(GetScreenAspect() < 1.62f))
			{
				return Mode.Blueprints;
			}
			return Mode.Off;
		}
		return Mode.Full;
	}
}
