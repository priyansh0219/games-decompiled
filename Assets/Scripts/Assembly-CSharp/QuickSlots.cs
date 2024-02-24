using System.Collections.Generic;
using System.Text;
using UWE;
using UnityEngine;

public class QuickSlots : IQuickSlots
{
	private enum ArmsState
	{
		None = 0,
		Draw = 1,
		Hold = 2,
		Holster = 3,
		Drop = 4
	}

	public delegate void OnBind(int slotID, TechType id, bool state);

	public delegate void OnToggle(int slotID, bool state);

	public delegate void OnSelect(int slotID);

	private static readonly string[] slotNames = new string[6] { "QuickSlot0", "QuickSlot1", "QuickSlot2", "QuickSlot3", "QuickSlot4", "QuickSlot5" };

	private GameObject owner;

	private ItemsContainer container;

	private Transform slotTransform;

	private Transform toolSocket;

	private Transform cameraSocket;

	private InventoryItem[] binding;

	private InventoryItem _heldItem;

	private int defaultLayer;

	private int viewModelLayer;

	private ArmsState state;

	private string activeToolName = string.Empty;

	private int desiredSlot = -1;

	private Sequence sequence = new Sequence();

	private TechType refillTechType;

	private int refillSlot = -1;

	private bool ignoreHotkeyInput;

	private bool ignoreScrollInput;

	private float timeLastUpdate;

	private bool suspendSlotActivation;

	private InventoryItem _assignItem;

	private int _assignSlot = -1;

	public InventoryItem heldItem => _heldItem;

	public int activeSlot => GetSlotByItem(_heldItem);

	public int assignSlot => _assignSlot;

	public InventoryItem assignItem => _assignItem;

	public int slotCount { get; private set; }

	public event OnBind onBind;

	public event OnToggle onToggle;

	public event OnSelect onSelect;

	public QuickSlots(GameObject owner, Transform toolSocket, Transform cameraSocket, Inventory inv, Transform slotTr, int slotCount)
	{
		this.owner = owner;
		this.toolSocket = toolSocket;
		this.cameraSocket = cameraSocket;
		defaultLayer = LayerMask.NameToLayer("Default");
		viewModelLayer = LayerMask.NameToLayer("Viewmodel");
		this.slotCount = slotCount;
		binding = new InventoryItem[slotCount];
		container = inv.container;
		container.onAddItem += OnAddItem;
		container.onRemoveItem += OnRemoveItem;
		slotTransform = slotTr;
	}

	public TechType[] GetSlotBinding()
	{
		TechType[] array = new TechType[slotCount];
		for (int i = 0; i < slotCount; i++)
		{
			array[i] = binding[i]?.item.GetTechType() ?? TechType.None;
		}
		return array;
	}

	public void SetIgnoreHotkeyInput(bool ignore)
	{
		ignoreHotkeyInput = ignore;
	}

	public void SetIgnoreScrollInput(bool ignore)
	{
		ignoreScrollInput = ignore;
	}

	public TechType GetSlotBinding(int slotID)
	{
		if (slotID < 0 || slotID >= slotCount)
		{
			return TechType.None;
		}
		return binding[slotID]?.item.GetTechType() ?? TechType.None;
	}

	public InventoryItem GetSlotItem(int slotID)
	{
		if (slotID < 0 || slotID >= slotCount)
		{
			return null;
		}
		return binding[slotID];
	}

	public int GetActiveSlotID()
	{
		return desiredSlot;
	}

	public bool IsToggled(int slotID)
	{
		if (slotID < 0 || slotID >= slotCount)
		{
			return false;
		}
		if (TechData.GetSlotType(GetSlotBinding(slotID)) == QuickSlotType.Passive)
		{
			return true;
		}
		return activeSlot == slotID;
	}

	public int GetSlotCount()
	{
		return slotCount;
	}

	public void BeginAssign(InventoryItem item)
	{
		EndAssign();
		if (item != null && item.isBindable)
		{
			int slotByItem = GetSlotByItem(item);
			_assignSlot = slotByItem;
			if (_assignSlot < 0)
			{
				_assignSlot = GetFirstEmpty();
			}
			if (_assignSlot < 0)
			{
				_assignSlot = 0;
			}
			_assignItem = item;
			uGUI_QuickSlots quickSlots = uGUI.main.quickSlots;
			GamepadInputModule.current.SetCurrentGrid(quickSlots);
			quickSlots.SelectFirstItem();
			uGUI_ItemIcon icon = quickSlots.GetIcon(_assignSlot);
			RectTransform rectTransform = icon.rectTransform;
			int instanceID = icon.GetInstanceID();
			Vector3 worldPosition = rectTransform.TransformPoint(rectTransform.rect.center);
			Quaternion rotation = rectTransform.rotation;
			Vector3 lossyScale = rectTransform.lossyScale;
			Vector2 foregroundSize = uGUI_QuickSlots.foregroundSize;
			Vector2 backgroundSize = uGUI_QuickSlots.backgroundSize;
			float radius = uGUI_QuickSlots.radius;
			ItemDragManager.DragStart(item, icon, instanceID, worldPosition, rotation, lossyScale, foregroundSize, backgroundSize, radius);
			ItemDragManager.DragSnap(icon.GetInstanceID(), worldPosition, rotation, lossyScale, foregroundSize, backgroundSize, radius);
		}
	}

	public void NavigateAssign(int direction)
	{
		if (_assignSlot < 0)
		{
			return;
		}
		if (direction > 0)
		{
			_assignSlot++;
			if (_assignSlot >= binding.Length)
			{
				_assignSlot = 0;
			}
		}
		else
		{
			_assignSlot--;
			if (_assignSlot < 0)
			{
				_assignSlot = binding.Length - 1;
			}
		}
	}

	public void EndAssign(bool apply = false)
	{
		if (_assignSlot < 0)
		{
			return;
		}
		if (apply && _assignItem != null)
		{
			int slotByItem = GetSlotByItem(_assignItem);
			if (slotByItem < 0)
			{
				InventoryItem slotItem = GetSlotItem(assignSlot);
				Unbind(assignSlot);
				Bind(assignSlot, _assignItem);
				if (slotItem != null)
				{
					int firstEmpty = GetFirstEmpty();
					if (firstEmpty >= 0)
					{
						Bind(firstEmpty, slotItem);
					}
				}
			}
			else if (slotByItem != assignSlot)
			{
				InventoryItem slotItem2 = GetSlotItem(assignSlot);
				if (slotItem2 != null)
				{
					Bind(slotByItem, slotItem2);
				}
				Bind(assignSlot, _assignItem);
			}
		}
		InventoryItem item = _assignItem;
		_assignItem = null;
		_assignSlot = -1;
		ItemDragManager.DragStop();
		PDA pda = Player.main.pda;
		if (pda.state == PDA.State.Opened)
		{
			uGUI_INavigableIconGrid initialGrid = pda.ui.currentTab.GetInitialGrid();
			GamepadInputModule.current.SetCurrentGrid(initialGrid);
			uGUI_ItemsContainer uGUI_ItemsContainer2 = initialGrid as uGUI_ItemsContainer;
			if (uGUI_ItemsContainer2 != null)
			{
				initialGrid.SelectItem(uGUI_ItemsContainer2.GetIcon(item));
			}
		}
	}

	public void Bind(int slotID, InventoryItem item)
	{
		if (slotID < 0 || slotID >= slotCount || item == null || !item.isBindable)
		{
			return;
		}
		InventoryItem inventoryItem = binding[slotID];
		if (inventoryItem != null && inventoryItem == item)
		{
			Unbind(slotID);
			return;
		}
		for (int i = 0; i < slotCount; i++)
		{
			if (binding[i] == item)
			{
				Unbind(i);
				break;
			}
		}
		binding[slotID] = item;
		NotifyBind(slotID, state: true);
	}

	public void Unbind(int slotID)
	{
		if (slotID >= 0 && slotID < slotCount && binding[slotID] != null)
		{
			binding[slotID] = null;
			NotifyBind(slotID, state: false);
		}
	}

	public void SlotKeyDown(int slotID)
	{
		if (Player.main.GetPDA().isInUse)
		{
			InventoryItem hoveredItem = ItemDragManager.hoveredItem;
			if (hoveredItem != null && Inventory.main.GetCanBindItem(hoveredItem))
			{
				Bind(slotID, hoveredItem);
			}
		}
		else if (AvatarInputHandler.main.IsEnabled() && Player.main.GetMode() == Player.Mode.Normal && !ignoreHotkeyInput)
		{
			Select(slotID);
		}
	}

	public void SlotKeyHeld(int slotID)
	{
	}

	public void SlotKeyUp(int slotID)
	{
	}

	public void SlotNext()
	{
		if (!AvatarInputHandler.main.IsEnabled() || Player.main.GetMode() != 0 || ignoreHotkeyInput || ignoreScrollInput)
		{
			return;
		}
		int activeSlotID = GetActiveSlotID();
		int num = GetSlotCount();
		int num2 = ((activeSlotID < 0) ? (-1) : activeSlotID);
		for (int i = 0; i < num; i++)
		{
			num2++;
			if (num2 >= num)
			{
				Deselect();
				break;
			}
			TechType slotBinding = GetSlotBinding(num2);
			if (slotBinding != 0)
			{
				QuickSlotType slotType = TechData.GetSlotType(slotBinding);
				if (slotType == QuickSlotType.Selectable || slotType == QuickSlotType.SelectableChargeable)
				{
					Select(num2);
					break;
				}
			}
		}
	}

	public void SlotPrevious()
	{
		if (!AvatarInputHandler.main.IsEnabled() || Player.main.GetMode() != 0 || ignoreHotkeyInput || ignoreScrollInput)
		{
			return;
		}
		int activeSlotID = GetActiveSlotID();
		int num = GetSlotCount();
		int num2 = ((activeSlotID < 0) ? num : activeSlotID);
		for (int i = 0; i < num; i++)
		{
			num2--;
			if (num2 < 0)
			{
				Deselect();
				break;
			}
			TechType slotBinding = GetSlotBinding(num2);
			if (slotBinding != 0)
			{
				QuickSlotType slotType = TechData.GetSlotType(slotBinding);
				if (slotType == QuickSlotType.Selectable || slotType == QuickSlotType.SelectableChargeable)
				{
					Select(num2);
					break;
				}
			}
		}
	}

	public void SlotLeftDown()
	{
	}

	public void SlotLeftHeld()
	{
	}

	public void SlotLeftUp()
	{
	}

	public void SlotRightDown()
	{
	}

	public void SlotRightHeld()
	{
	}

	public void SlotRightUp()
	{
	}

	public void DeselectSlots()
	{
		Deselect();
	}

	public float GetSlotProgress(int slotID)
	{
		return 1f;
	}

	public float GetSlotCharge(int slotID)
	{
		return 1f;
	}

	public void SetSuspendSlotActivation(bool value)
	{
		suspendSlotActivation = value;
	}

	public void Select(int slotID)
	{
		if (slotID >= 0 && slotID < slotCount)
		{
			if (desiredSlot != slotID)
			{
				desiredSlot = slotID;
			}
			else
			{
				desiredSlot = -1;
			}
		}
	}

	public void Deselect()
	{
		desiredSlot = -1;
	}

	public void SelectImmediate(int slotID)
	{
		if (slotID < 0 || slotID >= slotCount)
		{
			return;
		}
		if (activeSlot != slotID)
		{
			DisposeAnimationState();
			sequence.Reset();
			sequence.ForceState(state: true);
			desiredSlot = slotID;
			if (suspendSlotActivation)
			{
				DeselectInternal();
				return;
			}
			SelectInternal(desiredSlot);
			if (_heldItem != null)
			{
				PlayerTool component = _heldItem.item.GetComponent<PlayerTool>();
				if (component != null && component.hasAnimations)
				{
					string animToolName = component.animToolName;
					if (!string.IsNullOrEmpty(animToolName))
					{
						SetAnimationState(animToolName);
						state = ArmsState.Draw;
					}
				}
				else
				{
					state = ArmsState.Hold;
				}
			}
			else
			{
				desiredSlot = -1;
				state = ArmsState.None;
			}
		}
		else
		{
			DeselectImmediate();
		}
	}

	public void DeselectImmediate()
	{
		if (_heldItem != null)
		{
			desiredSlot = -1;
			DisposeAnimationState();
			sequence.Reset();
			sequence.ForceState(state: false);
			state = ArmsState.None;
			DeselectInternal();
		}
	}

	public int GetSlotByGameObject(GameObject go)
	{
		if (go != null)
		{
			for (int i = 0; i < slotCount; i++)
			{
				InventoryItem inventoryItem = binding[i];
				if (inventoryItem != null && inventoryItem.item != null && inventoryItem.item.gameObject == go)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public int GetSlotByItem(InventoryItem item)
	{
		if (item != null)
		{
			for (int i = 0; i < slotCount; i++)
			{
				if (binding[i] == item)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public bool SelectSlotByGameObject(GameObject go)
	{
		int slotByGameObject = GetSlotByGameObject(go);
		if (slotByGameObject < 0)
		{
			return false;
		}
		Select(slotByGameObject);
		return true;
	}

	public void Update()
	{
		float time = Time.time;
		if (time != timeLastUpdate)
		{
			sequence.Update();
		}
		timeLastUpdate = time;
		UpdateState();
		if (_heldItem != null)
		{
			int slotByItem = GetSlotByItem(_heldItem);
			if (slotByItem == -1)
			{
				DeselectImmediate();
			}
			else
			{
				Equipment.SendEquipmentEvent(_heldItem.item, 2, owner, slotNames[slotByItem]);
			}
		}
	}

	private void UpdateState()
	{
		int slotByItem = GetSlotByItem(_heldItem);
		if (refillTechType != 0 && refillSlot != -1)
		{
			TryRefill(refillTechType, refillSlot);
			refillTechType = TechType.None;
			refillSlot = -1;
		}
		switch (state)
		{
		case ArmsState.None:
			if (desiredSlot == -1 || suspendSlotActivation)
			{
				break;
			}
			SelectInternal(desiredSlot);
			if (_heldItem != null)
			{
				PlayerTool component2 = _heldItem.item.GetComponent<PlayerTool>();
				if (component2 != null && component2.hasAnimations)
				{
					string animToolName2 = component2.animToolName;
					if (!string.IsNullOrEmpty(animToolName2))
					{
						SetAnimationState(animToolName2);
						state = ArmsState.Draw;
						sequence.Set(GetTransitionTime(), current: false, target: true, TransitionEnd);
					}
				}
				else
				{
					state = ArmsState.Hold;
				}
			}
			else
			{
				desiredSlot = -1;
			}
			break;
		case ArmsState.Draw:
			if (slotByItem != desiredSlot)
			{
				DisposeAnimationState();
				state = ArmsState.Holster;
				float transitionTime = GetTransitionTime();
				sequence.Set(transitionTime, current: true, target: false, TransitionEnd);
			}
			break;
		case ArmsState.Hold:
			if (slotByItem != desiredSlot && !HeldToolIsInUse() && !PlayerIsBashing())
			{
				DisposeAnimationState();
				state = ArmsState.Holster;
				sequence.Set(GetTransitionTime(), current: true, target: false, TransitionEnd);
			}
			break;
		case ArmsState.Holster:
			if (slotByItem != desiredSlot)
			{
				break;
			}
			if (_heldItem != null)
			{
				PlayerTool component = _heldItem.item.GetComponent<PlayerTool>();
				if (component != null && component.hasAnimations)
				{
					string animToolName = component.animToolName;
					if (!string.IsNullOrEmpty(animToolName))
					{
						SetAnimationState(animToolName);
						state = ArmsState.Draw;
						sequence.Set(GetTransitionTime(), current: false, target: true, TransitionEnd);
					}
				}
				else
				{
					state = ArmsState.Hold;
				}
			}
			else
			{
				desiredSlot = -1;
			}
			break;
		}
	}

	private float GetTransitionTime()
	{
		float value = 0.5f;
		float value2 = 0.35f;
		float value3 = 1f;
		if (_heldItem != null)
		{
			PlayerTool component = _heldItem.item.GetComponent<PlayerTool>();
			if (component != null)
			{
				value = component.drawTime;
				value2 = component.holsterTime;
				value3 = component.dropTime;
			}
		}
		value = Mathf.Clamp(value, 0f, 10f);
		value2 = Mathf.Clamp(value2, 0f, 10f);
		value3 = Mathf.Clamp(value3, 0f, 10f);
		switch (state)
		{
		case ArmsState.Draw:
			return value;
		case ArmsState.Holster:
			return value2;
		case ArmsState.Drop:
			return value3;
		default:
			return 0f;
		}
	}

	private void TransitionEnd()
	{
		switch (state)
		{
		case ArmsState.Draw:
			state = ArmsState.Hold;
			break;
		case ArmsState.Holster:
			state = ArmsState.None;
			DeselectInternal();
			break;
		case ArmsState.Drop:
			state = ArmsState.None;
			break;
		case ArmsState.Hold:
			break;
		}
	}

	private void SetAnimationState(string toolName)
	{
		DisposeAnimationState();
		if (!string.IsNullOrEmpty(toolName))
		{
			activeToolName = toolName;
			SafeAnimator.SetBool(Player.main.armsController.GetComponent<Animator>(), "holding_" + activeToolName, value: true);
		}
	}

	private void DisposeAnimationState()
	{
		if (!string.IsNullOrEmpty(activeToolName))
		{
			SafeAnimator.SetBool(Player.main.armsController.GetComponent<Animator>(), "holding_" + activeToolName, value: false);
			activeToolName = null;
		}
	}

	public void Drop(Vector3 force)
	{
		if (_heldItem != null && Inventory.CanDropItemHere(_heldItem.item, notify: true))
		{
			Pickupable item = _heldItem.item;
			refillTechType = item.GetTechType();
			refillSlot = GetSlotByItem(_heldItem);
			desiredSlot = refillSlot;
			item.Drop(item.transform.position, force);
		}
	}

	private bool HeldToolIsInUse()
	{
		if (_heldItem == null)
		{
			return false;
		}
		PlayerTool component = _heldItem.item.GetComponent<PlayerTool>();
		if (component == null)
		{
			return false;
		}
		return component.isInUse;
	}

	private bool PlayerIsBashing()
	{
		float num = 0.7f;
		if (_heldItem != null)
		{
			PlayerTool component = _heldItem.item.GetComponent<PlayerTool>();
			if (component != null)
			{
				num = component.bashTime;
			}
		}
		float timeBashed = Player.main.timeBashed;
		if (timeBashed != 0f)
		{
			return Time.time < timeBashed + num;
		}
		return false;
	}

	private void SelectInternal(int slotID)
	{
		DeselectInternal();
		InventoryItem inventoryItem = binding[slotID];
		if (inventoryItem != null)
		{
			Pickupable item = inventoryItem.item;
			PlayerTool component = item.GetComponent<PlayerTool>();
			if (component != null)
			{
				DrawAsTool(component);
			}
			else
			{
				DrawAsItem(inventoryItem);
			}
			_heldItem = inventoryItem;
			NotifyToggle(slotID, state: true);
			NotifySelect(slotID);
			Equipment.SendEquipmentEvent(item, 0, owner, slotNames[slotID]);
		}
	}

	private void DeselectInternal()
	{
		if (_heldItem != null)
		{
			InventoryItem inventoryItem = _heldItem;
			int slotByItem = GetSlotByItem(inventoryItem);
			Pickupable item = inventoryItem.item;
			PlayerTool component = item.GetComponent<PlayerTool>();
			if (component != null)
			{
				HolsterAsTool(component);
			}
			else
			{
				HolsterAsItem(inventoryItem);
			}
			_heldItem = null;
			NotifyToggle(slotByItem, state: false);
			NotifySelect(-1);
			Equipment.SendEquipmentEvent(slot: (slotByItem == -1) ? string.Empty : slotNames[slotByItem], pickupable: item, eventType: 1, owner: owner);
		}
	}

	public void SetViewModelVis(bool state)
	{
		if (_heldItem == null)
		{
			return;
		}
		PlayerTool component = _heldItem.item.GetComponent<PlayerTool>();
		if (component != null)
		{
			Player main = Player.main;
			if (state)
			{
				component.OnDraw(main);
			}
			else
			{
				component.OnHolster();
			}
			component.gameObject.SetActive(state);
		}
	}

	private void HolsterAsTool(PlayerTool tool)
	{
		tool.gameObject.SetActive(value: false);
		Utils.SetLayerRecursively(tool.gameObject, defaultLayer);
		if (tool.mainCollider != null)
		{
			tool.mainCollider.enabled = true;
		}
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(tool.GetComponent<Rigidbody>(), isKinematic: false);
		ItemsContainer itemsContainer = (ItemsContainer)_heldItem.container;
		if (itemsContainer != null)
		{
			_heldItem.item.Reparent(itemsContainer.tr);
		}
		Animator[] componentsInChildren = tool.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].cullingMode = AnimatorCullingMode.CullUpdateTransforms;
		}
		tool.OnHolster();
		TechType techType = _heldItem.item.GetTechType();
		GoalManager.main.OnCustomGoalEvent("Equip_" + techType.AsString());
	}

	private void DrawAsTool(PlayerTool tool)
	{
		Transform socket = ((tool.socket == PlayerTool.Socket.Camera) ? cameraSocket : toolSocket);
		ModelPlug.PlugIntoSocket(tool, socket);
		Utils.SetLayerRecursively(tool.gameObject, viewModelLayer);
		Animator[] componentsInChildren = tool.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].cullingMode = AnimatorCullingMode.AlwaysAnimate;
		}
		if (tool.mainCollider != null)
		{
			tool.mainCollider.enabled = false;
		}
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(tool.GetComponent<Rigidbody>(), isKinematic: true);
		tool.gameObject.SetActive(value: true);
		Player localPlayerComp = Utils.GetLocalPlayerComp();
		tool.OnDraw(localPlayerComp);
	}

	private void HolsterAsItem(InventoryItem item)
	{
		ItemsContainer itemsContainer = (ItemsContainer)item.container;
		if (itemsContainer != null)
		{
			item.item.Reparent(itemsContainer.tr);
		}
		item.item.SetVisible(visible: false);
		Utils.SetLayerRecursively(item.item.gameObject, defaultLayer);
	}

	private void DrawAsItem(InventoryItem item)
	{
		item.item.Reparent(toolSocket);
		item.item.SetVisible(visible: true);
		Utils.SetLayerRecursively(item.item.gameObject, viewModelLayer);
	}

	private int GetFirstEmpty()
	{
		for (int i = 0; i < binding.Length; i++)
		{
			if (binding[i] == null)
			{
				return i;
			}
		}
		return -1;
	}

	private int BindToEmpty(InventoryItem item)
	{
		int firstEmpty = GetFirstEmpty();
		if (firstEmpty == -1)
		{
			return -1;
		}
		Bind(firstEmpty, item);
		return firstEmpty;
	}

	private bool TryRefill(TechType techType, int slotID)
	{
		IList<InventoryItem> items = container.GetItems(techType);
		if (items == null)
		{
			return false;
		}
		for (int i = 0; i < items.Count; i++)
		{
			InventoryItem item = items[i];
			if (GetSlotByItem(item) == -1)
			{
				Bind(slotID, item);
				return true;
			}
		}
		return false;
	}

	public string[] SaveBinding()
	{
		string[] array = new string[binding.Length];
		for (int i = 0; i < binding.Length; i++)
		{
			array[i] = string.Empty;
			InventoryItem inventoryItem = binding[i];
			if (inventoryItem != null && !(inventoryItem.item == null))
			{
				UniqueIdentifier component = inventoryItem.item.GetComponent<UniqueIdentifier>();
				if (!(component == null))
				{
					array[i] = component.Id;
				}
			}
		}
		return array;
	}

	public void RestoreBinding(string[] uids)
	{
		for (int i = 0; i < binding.Length; i++)
		{
			Unbind(i);
		}
		Deselect();
		foreach (InventoryItem item in (IEnumerable<InventoryItem>)container)
		{
			UniqueIdentifier component = item.item.GetComponent<UniqueIdentifier>();
			if (component == null)
			{
				continue;
			}
			int num = Mathf.Min(uids.Length, binding.Length);
			for (int j = 0; j < num; j++)
			{
				if (uids[j] == component.Id)
				{
					Bind(j, item);
					break;
				}
			}
		}
	}

	private void OnAddItem(InventoryItem item)
	{
		if (item == null || item.item == null || !item.isBindable)
		{
			return;
		}
		Pickupable item2 = item.item;
		TechType techType = item2.GetTechType();
		if (techType == TechType.ScrapMetal)
		{
			return;
		}
		Inventory main = Inventory.main;
		ItemAction allItemActions = main.GetAllItemActions(item);
		ItemAction itemAction = ItemAction.None;
		if ((allItemActions & ItemAction.Eat) != 0)
		{
			itemAction = ItemAction.Eat;
		}
		else if ((allItemActions & ItemAction.Use) != 0)
		{
			itemAction = ItemAction.Use;
		}
		if (item2.GetComponent<PlayerTool>() == null && itemAction != ItemAction.Use && itemAction != ItemAction.Eat)
		{
			return;
		}
		if (itemAction == ItemAction.Eat)
		{
			LiveMixin component = item2.GetComponent<LiveMixin>();
			if (item2.GetComponent<Plantable>() != null || component == null)
			{
				return;
			}
		}
		if (main.container.GetCount(techType) <= 1)
		{
			int num = BindToEmpty(item);
			if (_heldItem == null && num >= 0 && AvatarInputHandler.main.IsEnabled())
			{
				Select(num);
			}
		}
	}

	private void OnRemoveItem(InventoryItem item)
	{
		int slotByItem = GetSlotByItem(item);
		if (item == _heldItem)
		{
			state = ArmsState.Drop;
			float transitionTime = GetTransitionTime();
			DeselectInternal();
			DisposeAnimationState();
			sequence.Set(transitionTime, current: true, target: false, TransitionEnd);
			desiredSlot = slotByItem;
		}
		if (slotByItem != -1)
		{
			Unbind(slotByItem);
			refillTechType = item.item.GetTechType();
			refillSlot = slotByItem;
		}
	}

	private void NotifyBind(int slotID, bool state)
	{
		if (this.onBind != null)
		{
			TechType id = (state ? binding[slotID].item.GetTechType() : TechType.None);
			this.onBind(slotID, id, state);
		}
	}

	private void NotifyToggle(int slotID, bool state)
	{
		if (this.onToggle != null)
		{
			this.onToggle(slotID, state);
		}
	}

	private void NotifySelect(int slotID)
	{
		if (this.onSelect != null)
		{
			this.onSelect(slotID);
		}
	}

	public void OnGUI()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("HeldItem: {0}\n", (_heldItem == null) ? "None" : _heldItem.item.name);
		stringBuilder.AppendFormat("activeSlot: {0}\ndesiredSlot: {1}\naction: {2}\nsequence.t: {3:f1}\nanimation: {4}\nrefillTechType: {5}\nrefillSlot: {6}\n", activeSlot, desiredSlot, state, sequence.t, string.IsNullOrEmpty(activeToolName) ? string.Empty : ("holding_" + activeToolName), refillTechType, refillSlot);
		GUI.Label(new Rect(10f, 10f, 500f, 500f), stringBuilder.ToString());
	}
}
