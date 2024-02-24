using System;
using System.Collections;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
public class uGUI_CraftingMenu : uGUI_InputGroup, uGUI_IIconManager, uGUI_INavigableIconGrid
{
	[SuppressMessage("Subnautica.Rules", "ValueTypeEnumeratorRule")]
	[SuppressMessage("Gendarme.Rules.Naming", "UseCorrectSuffixRule")]
	private sealed class Node : TreeNode, IEnumerable<Node>, IEnumerable
	{
		public TreeAction action;

		public TechType techType;

		public uGUI_ItemIcon icon;

		public bool expanded;

		public Node(string id, TreeAction action, TechType techType)
			: base(id)
		{
			this.action = action;
			this.techType = techType;
		}

		public new IEnumerator<Node> GetEnumerator()
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				yield return (Node)nodes[i];
			}
		}
	}

	private const float expandPunchAmplitude = 1f;

	private const float clickPunchAmplitude = 0.5f;

	private const float punchDuration = 1f;

	private const float punchDurationScatter = 0.2f;

	private const float clickPunchFrequency = 5f;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.LateUpdateAfterInput;

	private static Sprite _expandNormal;

	private static Sprite _expandHovered;

	private static Sprite _craftNormal;

	private static Sprite _craftHovered;

	[AssertNotNull]
	public uGUI_CanvasScaler canvasScaler;

	[AssertNotNull]
	public CanvasGroup canvasGroup;

	[AssertNotNull]
	public RectTransform iconsCanvas;

	[AssertNotNull]
	public FMODAsset soundOpen;

	[AssertNotNull]
	public FMODAsset soundExpand;

	[AssertNotNull]
	public FMODAsset soundAccept;

	[AssertNotNull]
	public FMODAsset soundDeny;

	private CraftTree.Type treeType;

	private string id;

	private ITreeActionReceiver _client;

	private Node craftedNode;

	private Node tree;

	private Dictionary<uGUI_ItemIcon, Node> icons = new Dictionary<uGUI_ItemIcon, Node>();

	[SerializeField]
	[AssertNotNull]
	private uGUI_GraphicRaycaster interactionRaycaster;

	private bool interactable = true;

	private bool isDirty;

	private bool resync;

	private bool locked;

	private static Sprite expandNormal
	{
		get
		{
			if (_expandNormal == null)
			{
				_expandNormal = SpriteManager.Get(SpriteManager.Group.Background, "categoryNormal");
			}
			return _expandNormal;
		}
	}

	private static Sprite expandHovered
	{
		get
		{
			if (_expandHovered == null)
			{
				_expandHovered = SpriteManager.Get(SpriteManager.Group.Background, "categoryHovered");
			}
			return _expandHovered;
		}
	}

	private static Sprite craftNormal
	{
		get
		{
			if (_craftNormal == null)
			{
				_craftNormal = SpriteManager.Get(SpriteManager.Group.Background, "craftNormal");
			}
			return _craftNormal;
		}
	}

	private static Sprite craftHovered
	{
		get
		{
			if (_craftHovered == null)
			{
				_craftHovered = SpriteManager.Get(SpriteManager.Group.Background, "craftHovered");
			}
			return _craftHovered;
		}
	}

	public ITreeActionReceiver client => _client;

	public uGUI_ItemIcon selectedIcon
	{
		get
		{
			ISelectable selectable = UISelection.selected;
			if (selectable != null && selectable.IsValid())
			{
				uGUI_ItemIcon uGUI_ItemIcon2 = selectable as uGUI_ItemIcon;
				if (icons.ContainsKey(uGUI_ItemIcon2))
				{
					return uGUI_ItemIcon2;
				}
			}
			return null;
		}
	}

	bool uGUI_INavigableIconGrid.ShowSelector => true;

	bool uGUI_INavigableIconGrid.EmulateRaycast => true;

	public void Open(CraftTree.Type treeType, ITreeActionReceiver receiver)
	{
		if (base.selected)
		{
			return;
		}
		CraftTree craftTree = CraftTree.GetTree(treeType);
		if (craftTree == null)
		{
			return;
		}
		if (craftTree.nodes.childCount == 0)
		{
			Debug.LogError("Provided CraftTree is empty!");
			return;
		}
		this.treeType = treeType;
		id = craftTree.id;
		canvasGroup.alpha = 1f;
		UpdateTree(craftTree.nodes, ref tree);
		Expand(tree);
		_client = receiver;
		if (_client.inProgress)
		{
			SetLocked(locked: true);
		}
		ItemsContainer container = Inventory.main.container;
		container.onAddItem += InventoryChanged;
		container.onRemoveItem += InventoryChanged;
		KnownTech.onChanged += OnCompletedChanged;
		KnownTech.onCompoundAdd += OnCompoundAdd;
		KnownTech.onCompoundRemove += OnCompoundRemove;
		KnownTech.onCompoundProgress += OnCompoundProgress;
		PDAScanner.onAdd = (PDAScanner.OnEntryEvent)Delegate.Combine(PDAScanner.onAdd, new PDAScanner.OnEntryEvent(OnLockedAdd));
		PDAScanner.onRemove = (PDAScanner.OnEntryEvent)Delegate.Combine(PDAScanner.onRemove, new PDAScanner.OnEntryEvent(OnLockedRemove));
		PDAScanner.onProgress = (PDAScanner.OnEntryEvent)Delegate.Combine(PDAScanner.onProgress, new PDAScanner.OnEntryEvent(OnLockedProgress));
		Select();
		GamepadInputModule.current.SetCurrentGrid(this);
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateAfterInput, UpdateVisuals);
		FMODUWE.PlayOneShot(soundOpen, MainCamera.camera.transform.position);
	}

	public void Close(ITreeActionReceiver receiver)
	{
		if (_client != null && !_client.Equals(null) && _client == receiver)
		{
			ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdateAfterInput, UpdateVisuals);
			Deselect();
		}
	}

	public void Lock(ITreeActionReceiver receiver)
	{
		if (_client != null && !_client.Equals(null) && _client == receiver)
		{
			SetInteractable(value: false);
		}
	}

	private bool ActionAvailable(Node sender)
	{
		switch (sender.action)
		{
		case TreeAction.Expand:
			return true;
		case TreeAction.Craft:
			if (CrafterLogic.IsCraftRecipeUnlocked(sender.techType))
			{
				return CrafterLogic.IsCraftRecipeFulfilled(sender.techType);
			}
			return false;
		default:
			return false;
		}
	}

	public override void OnSelect(bool lockMovement)
	{
		base.OnSelect(lockMovement);
		canvasScaler.SetAnchor();
	}

	public override void OnDeselect()
	{
		((uGUI_INavigableIconGrid)this).DeselectItem();
		SetInteractable(value: true);
		for (int num = iconsCanvas.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(iconsCanvas.GetChild(num).gameObject);
		}
		icons.Clear();
		if (tree != null)
		{
			tree.OnDestroy();
			tree = null;
		}
		treeType = CraftTree.Type.None;
		id = null;
		_client = null;
		craftedNode = null;
		ItemsContainer container = Inventory.main.container;
		if (container != null)
		{
			container.onAddItem -= InventoryChanged;
			container.onRemoveItem -= InventoryChanged;
		}
		KnownTech.onChanged -= OnCompletedChanged;
		KnownTech.onCompoundAdd -= OnCompoundAdd;
		KnownTech.onCompoundRemove -= OnCompoundRemove;
		KnownTech.onCompoundProgress -= OnCompoundProgress;
		PDAScanner.onAdd = (PDAScanner.OnEntryEvent)Delegate.Remove(PDAScanner.onAdd, new PDAScanner.OnEntryEvent(OnLockedAdd));
		PDAScanner.onRemove = (PDAScanner.OnEntryEvent)Delegate.Remove(PDAScanner.onRemove, new PDAScanner.OnEntryEvent(OnLockedRemove));
		PDAScanner.onProgress = (PDAScanner.OnEntryEvent)Delegate.Remove(PDAScanner.onProgress, new PDAScanner.OnEntryEvent(OnLockedProgress));
		base.OnDeselect();
	}

	private void Action(Node sender)
	{
		Vector3 position = MainCamera.camera.transform.position;
		if (_client != null && !_client.Equals(null) && interactable && ActionAvailable(sender))
		{
			switch (sender.action)
			{
			case TreeAction.Expand:
				Expand(sender);
				FMODUWE.PlayOneShot(soundExpand, position);
				break;
			case TreeAction.Craft:
				if (_client.PerformAction(sender.techType))
				{
					if (tree != null)
					{
						SetLocked(locked: true);
					}
					craftedNode = sender;
				}
				FMODUWE.PlayOneShot(soundAccept, position);
				break;
			}
		}
		else
		{
			FMODUWE.PlayOneShot(soundDeny, position);
		}
		Punch(sender.icon);
	}

	private void InventoryChanged(InventoryItem item)
	{
		SetDirty();
	}

	private void Resync()
	{
		resync = true;
	}

	private void SetDirty()
	{
		isDirty = true;
	}

	private void UpdateVisuals()
	{
		if (resync)
		{
			resync = false;
			if (tree != null)
			{
				UpdateTree(CraftTree.GetTree(treeType).nodes, ref tree);
				UpdateIcons(tree);
			}
			isDirty = true;
		}
		if (isDirty)
		{
			isDirty = false;
			if (tree != null)
			{
				int craftables = 0;
				int notifications = 0;
				UpdateNotifications(tree, ref craftables, ref notifications);
			}
		}
		if (_client == null || _client.Equals(null) || !locked)
		{
			return;
		}
		if (_client.inProgress)
		{
			float progress = _client.progress;
			if (!interactable)
			{
				SetAlpha(1f - progress / 0.3f);
			}
			SetProgress(craftedNode, progress);
		}
		else
		{
			SetInteractable(value: true);
			SetProgress(craftedNode, 1f);
			craftedNode = null;
			SetLocked(locked: false);
		}
	}

	private void OnCompletedChanged()
	{
		Resync();
	}

	private void OnCompoundAdd(TechType techType, int unlocked, int total)
	{
		Resync();
	}

	private void OnCompoundRemove(TechType techType)
	{
		Resync();
	}

	private void OnCompoundProgress(TechType techType, int unlocked, int total)
	{
		Resync();
	}

	private void OnLockedAdd(PDAScanner.Entry entry)
	{
		Resync();
	}

	private void OnLockedRemove(PDAScanner.Entry entry)
	{
		Resync();
	}

	private void OnLockedProgress(PDAScanner.Entry entry)
	{
		Resync();
	}

	private bool UpdateTree(CraftNode src, ref Node current, bool available = true)
	{
		if (available && src.action == TreeAction.Expand)
		{
			available = Filter(src.id);
		}
		bool result = false;
		foreach (CraftNode item in src)
		{
			Node current3 = ((current != null) ? (current[item.id] as Node) : null);
			if (UpdateTree(item, ref current3, available))
			{
				if (current == null)
				{
					current = new Node(src.id, src.action, src.techType0);
					result = true;
				}
				current.AddNode(current3);
			}
		}
		if (available)
		{
			if (src.action == TreeAction.Expand)
			{
				available = current != null && current.childCount > 0;
			}
			else if (src.action == TreeAction.Craft)
			{
				available = Filter(src.techType0);
			}
		}
		if (available != (current != null))
		{
			if (available)
			{
				current = new Node(src.id, src.action, src.techType0);
				result = true;
			}
			else
			{
				uGUI_ItemIcon icon = current.icon;
				if (icon != null)
				{
					icons.Remove(icon);
					UnityEngine.Object.Destroy(icon.gameObject);
					current.icon = null;
				}
				if (current.parent != null)
				{
					current.parent.RemoveNode(current);
				}
				current.OnDestroy();
				current = null;
			}
		}
		return result;
	}

	private void UpdateIcons(Node node)
	{
		if (!node.expanded && !node.icon)
		{
			return;
		}
		bool grid = IsGrid(node);
		using (IEnumerator<Node> enumerator = node.GetEnumerator())
		{
			int num = 0;
			int childCount = node.childCount;
			while (enumerator.MoveNext())
			{
				Node current = enumerator.Current;
				RectTransform rectTransform = GetRectTransform(node);
				GetIconMetrics(rectTransform, current, num, childCount, grid, out var size, out var x, out var y);
				if (current.icon != null)
				{
					current.icon.SetPosition(x, y);
				}
				else if (node.expanded)
				{
					CreateIcon(current, rectTransform, size, x, y);
					SetDirty();
				}
				UpdateIcons(current);
				num++;
			}
		}
	}

	private bool Filter(string id)
	{
		if (id == "Survival" && !GameModeUtils.RequiresSurvival())
		{
			return false;
		}
		if (id == "Submarine" && !Player.main.IsInSubmarine())
		{
			return false;
		}
		return true;
	}

	private bool Filter(TechType techType)
	{
		if (GameModeUtils.RequiresBlueprints())
		{
			if (TechData.GetIngredients(techType) == null)
			{
				Debug.LogErrorFormat("CraftData.GetIngredients returned null for '{0}'", techType.AsString());
				return false;
			}
			TechUnlockState techUnlockState = KnownTech.GetTechUnlockState(techType);
			if (techUnlockState != TechUnlockState.Available && techUnlockState != TechUnlockState.Locked)
			{
				return false;
			}
		}
		return true;
	}

	private void Expand(Node node)
	{
		if (node.parent is Node node2)
		{
			foreach (Node item in node2)
			{
				Collapse(item);
			}
		}
		SetExpanded(node, expanded: true);
		bool grid = IsGrid(node);
		using (IEnumerator<Node> enumerator2 = node.GetEnumerator())
		{
			int num = 0;
			int childCount = node.childCount;
			while (enumerator2.MoveNext())
			{
				Node current = enumerator2.Current;
				RectTransform rectTransform = GetRectTransform(node);
				GetIconMetrics(rectTransform, current, num, childCount, grid, out var size, out var x, out var y);
				if (current.icon == null)
				{
					CreateIcon(current, rectTransform, size, x, y);
					SetDirty();
				}
				else
				{
					current.icon.SetActive(active: true);
				}
				Punch(current.icon, 0.1f * ((float)num / (float)childCount), 1f);
				num++;
			}
		}
	}

	private void Collapse(Node parent)
	{
		SetExpanded(parent, expanded: false);
		if (parent.childCount == 0)
		{
			return;
		}
		using (IEnumerator<TreeNode> enumerator = parent.Traverse(includeSelf: false))
		{
			while (enumerator.MoveNext())
			{
				Node node = enumerator.Current as Node;
				SetExpanded(node, expanded: false);
				if (node.icon != null)
				{
					node.icon.SetActive(active: false);
				}
			}
		}
	}

	private bool IsGrid(Node node)
	{
		foreach (Node item in node)
		{
			if (item.action == TreeAction.Expand)
			{
				return false;
			}
		}
		return true;
	}

	private void GetIconMetrics(RectTransform canvas, Node node, int index, int siblings, bool grid, out float size, out float x, out float y)
	{
		int depth = node.depth;
		float width = canvas.rect.width;
		float num = 1f / Mathf.Pow(1.18f, depth - 1);
		size = Mathf.Max(40f, 92f * num);
		int num2 = ((!grid) ? 1 : Mathf.CeilToInt(Mathf.Sqrt(siblings)));
		int num3 = (siblings - 1) / num2 + 1;
		int num4 = index / num2;
		int num5 = index - num4 * num2;
		x = ((float)num5 + 0.5f) * size;
		y = (0.5f * (float)(num3 - 1) - (float)num4) * size;
		if (width > 0f)
		{
			x += 0.5f * width;
		}
	}

	private void CreateIcon(Node node, RectTransform canvas, float size, float x, float y)
	{
		TreeNode parent = node.parent;
		TreeAction action = node.action;
		if (parent != null)
		{
			TechType techType = ((action == TreeAction.Craft) ? node.techType : TechType.None);
			float num = 0.674f * size;
			Vector2 anchor = new Vector2(0.5f, 0.5f);
			Vector2 pivot = new Vector2(0.5f, 0.5f);
			Sprite foregroundSprite = SpriteManager.defaultSprite;
			Sprite backgroundSprite = null;
			switch (action)
			{
			case TreeAction.Expand:
				foregroundSprite = SpriteManager.Get(SpriteManager.Group.Category, $"{id}_{node.id}");
				backgroundSprite = expandNormal;
				break;
			case TreeAction.Craft:
				foregroundSprite = SpriteManager.Get(techType);
				backgroundSprite = craftNormal;
				break;
			}
			uGUI_ItemIcon uGUI_ItemIcon2 = new GameObject(id)
			{
				layer = LayerID.UI
			}.AddComponent<uGUI_ItemIcon>();
			uGUI_ItemIcon2.Init(this, canvas, anchor, pivot);
			uGUI_ItemIcon2.SetActiveSize(size, size);
			uGUI_ItemIcon2.SetBackgroundSize(size, size);
			uGUI_ItemIcon2.SetBarSize(size, size);
			uGUI_ItemIcon2.SetForegroundSize(num, num);
			uGUI_ItemIcon2.SetForegroundSprite(foregroundSprite);
			uGUI_ItemIcon2.SetBackgroundSprite(backgroundSprite);
			uGUI_ItemIcon2.SetBackgroundRadius(size * 0.5f);
			uGUI_ItemIcon2.SetPosition(x, y);
			node.icon = uGUI_ItemIcon2;
			icons.Add(uGUI_ItemIcon2, node);
			if (action == TreeAction.Craft)
			{
				NotificationManager.main.RegisterTarget(NotificationManager.Group.CraftTree, techType.EncodeKey(), uGUI_ItemIcon2);
			}
		}
	}

	private void UpdateNotifications(Node node, ref int craftables, ref int notifications)
	{
		int craftables2 = 0;
		int notifications2 = 0;
		foreach (Node item in node)
		{
			UpdateNotifications(item, ref craftables2, ref notifications2);
		}
		bool available = false;
		if (node.action == TreeAction.Craft)
		{
			if (ActionAvailable(node))
			{
				craftables++;
				available = true;
			}
			if (NotificationManager.main.Contains(NotificationManager.Group.CraftTree, node.techType.EncodeKey()))
			{
				notifications++;
			}
		}
		else if (node.action == TreeAction.Expand)
		{
			available = craftables2 > 0;
		}
		craftables += craftables2;
		notifications += notifications2;
		UpdateNotification(node, available, notifications2);
	}

	private void UpdateNotification(Node node, bool available, int notifications)
	{
		uGUI_ItemIcon icon = node.icon;
		if (icon == null)
		{
			return;
		}
		if ((node.parent as Node).expanded)
		{
			icon.SetChroma((available && !locked) ? 1f : 0f);
		}
		if (node.expanded)
		{
			icon.SetNotificationAlpha(0f);
			return;
		}
		if (icon.SetNotificationAlpha((notifications > 0) ? 1f : 0f))
		{
			icon.SetNotificationBackgroundColor(NotificationManager.notificationColor);
			icon.SetNotificationAnchor(UIAnchor.UpperRight);
			Vector2 vector = icon.rectTransform.rect.size * 0.5f;
			Vector2 vector2 = 0.8f * vector;
			icon.SetNotificationOffset(vector2 * 0.7071068f - vector);
		}
		icon.SetNotificationNumber(notifications);
	}

	private RectTransform GetRectTransform(Node node)
	{
		if (node.parent != null)
		{
			return node.icon.rectTransform;
		}
		return iconsCanvas;
	}

	private void Punch(uGUI_ItemIcon icon, float delay = 0f, float amplitude = 0.5f)
	{
		if (!(icon == null))
		{
			float duration = 1f + UnityEngine.Random.Range(-0.2f, 0.2f);
			icon.PunchScale(5f, amplitude, duration, delay);
		}
	}

	private void SetExpanded(Node node, bool expanded)
	{
		if (node.expanded != expanded)
		{
			node.expanded = expanded;
			SetDirty();
		}
	}

	private void SetLocked(bool locked)
	{
		if (this.locked != locked)
		{
			this.locked = locked;
			SetDirty();
		}
	}

	private void SetProgress(Node node, float progress)
	{
		if (node != null)
		{
			if (node.icon != null)
			{
				node.icon.SetProgress(progress);
			}
			SetProgress(node.parent as Node, progress);
		}
	}

	private void SetAlpha(float alpha)
	{
		alpha = Mathf.Clamp01(alpha);
		canvasGroup.alpha = MathExtensions.EaseInSine(alpha);
	}

	private void SetInteractable(bool value)
	{
		if (interactable != value)
		{
			interactable = value;
			canvasGroup.interactable = interactable;
			canvasGroup.blocksRaycasts = interactable;
			if (interactable)
			{
				SetAlpha(1f);
			}
		}
	}

	public override bool Raycast(PointerEventData eventData, List<RaycastResult> raycastResults)
	{
		bool num = base.Raycast(eventData, raycastResults);
		if (num)
		{
			BaseRaycaster componentInParent = uGUI.main.pinnedRecipes.GetComponentInParent<BaseRaycaster>();
			if (componentInParent != null)
			{
				componentInParent.Raycast(eventData, raycastResults);
			}
		}
		return num;
	}

	protected override void Awake()
	{
		base.Awake();
		interactionRaycaster.updateRaycasterStatusDelegate = SetRaycasterStatus;
	}

	private void SetRaycasterStatus(uGUI_GraphicRaycaster raycaster)
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

	private Node GetNode(uGUI_ItemIcon icon)
	{
		if (icon != null && icons.TryGetValue(icon, out var value))
		{
			return value;
		}
		return null;
	}

	void uGUI_IIconManager.GetTooltip(uGUI_ItemIcon icon, TooltipData data)
	{
		Node node = GetNode(icon);
		if (node != null)
		{
			switch (node.action)
			{
			case TreeAction.Expand:
				TooltipFactory.CraftNode($"{id}Menu_{node.id}", node.expanded, data);
				break;
			case TreeAction.Craft:
			{
				bool flag = !CrafterLogic.IsCraftRecipeUnlocked(node.techType);
				TooltipFactory.CraftRecipe(node.techType, flag, data);
				break;
			}
			}
		}
	}

	void uGUI_IIconManager.OnPointerEnter(uGUI_ItemIcon icon)
	{
		Node node = GetNode(icon);
		if (node != null)
		{
			icon.SetBackgroundSprite((node.action == TreeAction.Craft) ? craftHovered : expandHovered);
		}
	}

	void uGUI_IIconManager.OnPointerExit(uGUI_ItemIcon icon)
	{
		Node node = GetNode(icon);
		if (node != null)
		{
			icon.SetBackgroundSprite((node.action == TreeAction.Craft) ? craftNormal : expandNormal);
		}
	}

	bool uGUI_IIconManager.OnPointerClick(uGUI_ItemIcon icon, int button)
	{
		bool flag = GameInput.PrimaryDevice == GameInput.Device.Controller && !VROptions.GetUseGazeBasedCursor();
		if (!interactable)
		{
			if (flag && button == 1)
			{
				Deselect();
				return true;
			}
			return false;
		}
		Node node = GetNode(icon);
		if (node != null)
		{
			if (flag)
			{
				switch (button)
				{
				case 0:
					if (node.action == TreeAction.Craft)
					{
						Action(node);
					}
					else
					{
						((uGUI_INavigableIconGrid)this).SelectItemInDirection(1, 0);
					}
					break;
				case 1:
					Out(node.parent as Node);
					break;
				case 2:
					if (node.action == TreeAction.Craft)
					{
						TechType techType = node.techType;
						if (CrafterLogic.IsCraftRecipeUnlocked(techType))
						{
							PinManager.TogglePin(techType);
						}
					}
					break;
				}
			}
			else
			{
				switch (button)
				{
				case 0:
					Action(node);
					break;
				case 1:
					if (node.action == TreeAction.Craft)
					{
						TechType techType2 = node.techType;
						if (CrafterLogic.IsCraftRecipeUnlocked(techType2))
						{
							PinManager.TogglePin(techType2);
						}
					}
					break;
				}
			}
		}
		return true;
	}

	bool uGUI_IIconManager.OnButtonDown(uGUI_ItemIcon icon, GameInput.Button button)
	{
		if (interactable)
		{
			Node node = GetNode(icon);
			if (node == null)
			{
				return false;
			}
			if (button == GameInput.button2)
			{
				TechType techType = node.techType;
				if (CrafterLogic.IsCraftRecipeUnlocked(techType))
				{
					PinManager.TogglePin(techType);
				}
				return true;
			}
		}
		return false;
	}

	bool uGUI_IIconManager.OnBeginDrag(uGUI_ItemIcon icon)
	{
		return true;
	}

	void uGUI_IIconManager.OnEndDrag(uGUI_ItemIcon icon)
	{
	}

	void uGUI_IIconManager.OnDrop(uGUI_ItemIcon icon)
	{
	}

	void uGUI_IIconManager.OnDragHoverEnter(uGUI_ItemIcon icon)
	{
	}

	void uGUI_IIconManager.OnDragHoverStay(uGUI_ItemIcon icon)
	{
	}

	void uGUI_IIconManager.OnDragHoverExit(uGUI_ItemIcon icon)
	{
	}

	object uGUI_INavigableIconGrid.GetSelectedItem()
	{
		return selectedIcon;
	}

	Graphic uGUI_INavigableIconGrid.GetSelectedIcon()
	{
		return selectedIcon;
	}

	void uGUI_INavigableIconGrid.SelectItem(object item)
	{
		uGUI_ItemIcon uGUI_ItemIcon2 = item as uGUI_ItemIcon;
		if (!(uGUI_ItemIcon2 == null) && icons.ContainsKey(uGUI_ItemIcon2))
		{
			NavigationSelect(uGUI_ItemIcon2);
		}
	}

	void uGUI_INavigableIconGrid.DeselectItem()
	{
		if (!(((uGUI_INavigableIconGrid)this).GetSelectedItem() as uGUI_ItemIcon == null))
		{
			UISelection.selected = null;
		}
	}

	bool uGUI_INavigableIconGrid.SelectFirstItem()
	{
		if (tree != null)
		{
			foreach (Node item in tree)
			{
				uGUI_ItemIcon icon = item.icon;
				if (!(icon == null))
				{
					((uGUI_INavigableIconGrid)this).SelectItem((object)icon);
					return true;
				}
			}
		}
		return false;
	}

	bool uGUI_INavigableIconGrid.SelectItemClosestToPosition(Vector3 worldPos)
	{
		return false;
	}

	bool uGUI_INavigableIconGrid.SelectItemInDirection(int dirX, int dirY)
	{
		if (!interactable)
		{
			return true;
		}
		uGUI_ItemIcon uGUI_ItemIcon2 = selectedIcon;
		if (uGUI_ItemIcon2 == null)
		{
			return ((uGUI_INavigableIconGrid)this).SelectFirstItem();
		}
		if (dirX == 0 && dirY == 0)
		{
			return false;
		}
		Node node = GetNode(uGUI_ItemIcon2);
		if (node == null)
		{
			return false;
		}
		Node node2 = node.parent as Node;
		UISelection.sSelectables.Clear();
		for (int i = 0; i < node2.childCount; i++)
		{
			Node node3 = (Node)node2[i];
			if (node3 != null && !(node3.icon == null))
			{
				UISelection.sSelectables.Add(node3.icon);
			}
		}
		uGUI_ItemIcon uGUI_ItemIcon3 = (uGUI_ItemIcon)UISelection.FindSelectable((node2.isRoot || node2.icon == null) ? iconsCanvas : node2.icon.rectTransform, new Vector2(dirX, -dirY), uGUI_ItemIcon2, UISelection.sSelectables, fromEdge: false);
		UISelection.sSelectables.Clear();
		if (dirX < 0)
		{
			if (node.action == TreeAction.Expand || uGUI_ItemIcon3 == null)
			{
				Out(node2);
				return true;
			}
		}
		else if (dirX > 0 && node.action == TreeAction.Expand)
		{
			Action(node);
			if (node[0] is Node node4)
			{
				NavigationSelect(node4.icon);
			}
			return true;
		}
		if (uGUI_ItemIcon3 != null)
		{
			NavigationSelect(uGUI_ItemIcon3);
		}
		return true;
	}

	private void Out(Node parent)
	{
		if (!parent.isRoot)
		{
			NavigationSelect(parent.icon);
			Collapse(parent);
		}
		else
		{
			Deselect();
		}
	}

	uGUI_INavigableIconGrid uGUI_INavigableIconGrid.GetNavigableGridInDirection(int dirX, int dirY)
	{
		return null;
	}

	private void NavigationSelect(uGUI_ItemIcon icon)
	{
		UISelection.selected = icon;
	}
}
