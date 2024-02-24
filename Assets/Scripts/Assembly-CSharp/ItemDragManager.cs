using Gendarme;
using UWE;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Performance", "AvoidUnneededFieldInitializationRule")]
[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule")]
[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
public class ItemDragManager : MonoBehaviour
{
	public struct SnapProperties
	{
		public int instanceId;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public Vector2 foregroundSize;

		public Vector2 backgroundSize;

		public float radius;
	}

	public delegate void OnItemDragStart(Pickupable p);

	public delegate void OnItemDragStop();

	public const float swapAlpha = 0.3f;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.PreCanvasDrag;

	private static ItemDragManager _instance;

	public static InventoryItem hoveredItem;

	public static OnItemDragStart onItemDragStart;

	public static OnItemDragStop onItemDragStop;

	private static ItemActionHint currentHint;

	public float dragSnapSpeed = 15f;

	private InventoryItem _draggedItem;

	private uGUI_ItemIcon draggedIcon;

	private uGUI_Icon actionHint;

	private ItemActionHint cachedHint;

	private Vector2 _draggedItemSize;

	private uGUI_ILockable lockedIcon;

	private RectTransform rt;

	private int dragSnapInstanceId;

	private bool dragSnap;

	private float dragSnapT;

	private Vector3 dragSnapPosition;

	private Quaternion dragSnapRotation;

	private Vector2 dragSnapScale;

	private Vector2 dragSnapForegroundSize;

	private Vector2 dragSnapBackgroundSize;

	private float dragSnapRadius;

	private bool initialized;

	private Vector3 cursorPosition;

	private Quaternion cursorRotation;

	private Vector3 cursorScale;

	private Vector3 aimingPosition;

	private Vector3 aimingForward;

	private static ItemDragManager instance
	{
		get
		{
			if (_instance == null)
			{
				new GameObject("DragManager").AddComponent<ItemDragManager>();
			}
			return _instance;
		}
	}

	public static bool isDragging => draggedItem != null;

	public static InventoryItem draggedItem
	{
		get
		{
			if (!(_instance != null))
			{
				return null;
			}
			return _instance._draggedItem;
		}
	}

	public static Vector2 draggedItemSize
	{
		get
		{
			if (!(_instance != null))
			{
				return Vector2.zero;
			}
			return _instance._draggedItemSize;
		}
	}

	private void Awake()
	{
		if (_instance != null)
		{
			Debug.LogError("Multiple uGUI_DragManager instances found in scene!");
			Debug.Break();
			UWE.Utils.DestroyWrap(base.gameObject);
			return;
		}
		_instance = this;
		Vector2 vector = new Vector2(0f, 0f);
		base.gameObject.layer = LayerID.UI;
		Canvas canvas = base.gameObject.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;
		canvas.sortingLayerName = "ItemDragManager";
		rt = canvas.GetComponent<RectTransform>();
		rt.anchorMin = vector;
		rt.anchorMax = vector;
		rt.pivot = vector;
		rt.sizeDelta = vector;
		rt.localScale = new Vector3(1f, 1f, 1f);
		base.gameObject.AddComponent<CanvasGroup>().blocksRaycasts = false;
		GameObject gameObject = new GameObject("DraggedIcon");
		gameObject.layer = LayerID.UI;
		draggedIcon = gameObject.AddComponent<uGUI_ItemIcon>();
		draggedIcon.raycastTarget = false;
		draggedIcon.Init(null, rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
		draggedIcon.SetActive(active: false);
		GameObject gameObject2 = new GameObject("ActionHint");
		gameObject2.layer = LayerID.UI;
		actionHint = gameObject2.AddComponent<uGUI_Icon>();
		actionHint.raycastTarget = false;
		actionHint.material = new Material(uGUI_ItemIcon.iconMaterial);
		actionHint.rectTransform.SetParent(rt, worldPositionStays: false);
		actionHint.enabled = false;
	}

	private void OnEnable()
	{
		if (!initialized)
		{
			ManagedUpdate.Subscribe(ManagedUpdate.Queue.PreCanvasDrag, OnLateUpdate);
			initialized = true;
		}
	}

	private void OnDisable()
	{
		if (initialized)
		{
			ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.PreCanvasDrag, OnLateUpdate);
			initialized = false;
		}
	}

	public static void Deinitialize()
	{
		hoveredItem = null;
		onItemDragStart = null;
		onItemDragStop = null;
		currentHint = ItemActionHint.None;
		_instance = null;
	}

	public static bool DragStart(InventoryItem item, uGUI_ILockable icon, int instanceId, Vector3 worldPosition, Quaternion worldRotation, Vector2 worldScale, Vector2 foregroundSize, Vector2 backgroundSize, float backgroundRadius)
	{
		return instance.InternalDragStart(item, icon, instanceId, worldPosition, worldRotation, worldScale, foregroundSize, backgroundSize, backgroundRadius);
	}

	public static void DragSnap(int instanceId, Vector3 worldPosition, Quaternion worldRotation, Vector2 worldScale, Vector2 foregroundSize, Vector2 backgroundSize, float backgroundRadius)
	{
		instance.InternalDragSnap(instanceId, worldPosition, worldRotation, worldScale, foregroundSize, backgroundSize, backgroundRadius);
	}

	public static void DragStop()
	{
		instance.InternalDragStop();
	}

	public static void SetActionHint(ItemActionHint hint)
	{
		currentHint = hint;
	}

	private bool InternalDragStart(InventoryItem item, uGUI_ILockable icon, int instanceId, Vector3 worldPosition, Quaternion worldRotation, Vector2 worldScale, Vector2 foregroundSize, Vector2 backgroundSize, float backgroundRadius)
	{
		if (isDragging || item == null)
		{
			return false;
		}
		if (!item.CanDrag(verbose: true))
		{
			return false;
		}
		dragSnapT = 1f;
		dragSnapInstanceId = instanceId;
		dragSnapPosition = worldPosition;
		dragSnapRotation = worldRotation;
		dragSnapScale = worldScale;
		dragSnapForegroundSize = foregroundSize;
		dragSnapBackgroundSize = backgroundSize;
		dragSnapRadius = backgroundRadius;
		UpdateIconTransform();
		Pickupable item2 = item.item;
		TechType techType = item2.GetTechType();
		Vector2int itemSize = TechData.GetItemSize(techType);
		draggedIcon.SetActive(active: true);
		draggedIcon.SetForegroundSprite(SpriteManager.Get(techType));
		draggedIcon.SetBackgroundSprite(SpriteManager.GetBackground(techType));
		draggedIcon.SetBarValue(TooltipFactory.GetBarValue(item));
		uGUI_ItemsContainer.GetIconSize(itemSize.x, itemSize.y, out var width, out var height);
		_draggedItemSize = new Vector2(width, height);
		_draggedItem = item;
		lockedIcon = icon;
		LockIcon(state: true);
		NotifyItemDragStart(item2);
		return true;
	}

	private void InternalDragSnap(int instanceId, Vector3 worldPosition, Quaternion worldRotation, Vector2 worldScale, Vector2 foregroundSize, Vector2 backgroundSize, float worldRadius)
	{
		if (isDragging)
		{
			dragSnap = true;
			dragSnapPosition = worldPosition;
			dragSnapRotation = worldRotation;
			dragSnapScale = worldScale;
			dragSnapForegroundSize = foregroundSize;
			dragSnapBackgroundSize = backgroundSize;
			dragSnapRadius = worldRadius;
			_ = dragSnapInstanceId;
			dragSnapInstanceId = instanceId;
		}
	}

	private void InternalDragStop()
	{
		bool num = _draggedItem != null;
		dragSnapInstanceId = 0;
		dragSnapT = 0f;
		_draggedItem = null;
		draggedIcon.SetActive(active: false);
		LockIcon(state: false);
		lockedIcon = null;
		if (num)
		{
			NotifyItemDragStop();
		}
	}

	private bool ExtractParams()
	{
		cursorPosition = Vector3.zero;
		cursorRotation = Quaternion.identity;
		cursorScale = Vector3.one;
		aimingPosition = Vector3.zero;
		aimingForward = Vector3.forward;
		RectTransform rectTransform = null;
		if (CursorManager.GetPointerInfo(ref rectTransform, out var _, out cursorPosition, out var aimingTransform, out var _))
		{
			aimingPosition = aimingTransform.position;
			aimingForward = aimingTransform.forward;
			cursorRotation = rectTransform.rotation;
			cursorScale = rectTransform.lossyScale;
			return true;
		}
		return false;
	}

	private void OnLateUpdate()
	{
		if (cachedHint != currentHint)
		{
			cachedHint = currentHint;
			switch (cachedHint)
			{
			case ItemActionHint.None:
				actionHint.enabled = false;
				break;
			case ItemActionHint.Drop:
				actionHint.enabled = true;
				actionHint.sprite = SpriteManager.Get(SpriteManager.Group.ItemActions, "Drop");
				actionHint.SetNativeSize();
				break;
			case ItemActionHint.Swap:
				actionHint.enabled = true;
				actionHint.sprite = SpriteManager.Get(SpriteManager.Group.ItemActions, "Swap");
				actionHint.SetNativeSize();
				break;
			}
		}
		currentHint = ItemActionHint.None;
		if (!isDragging)
		{
			return;
		}
		dragSnapT = MathExtensions.StableLerp(dragSnapT, dragSnap ? 1f : 0f, dragSnapSpeed, PDA.deltaTime);
		UpdateIconTransform();
		draggedIcon.SetBarValue(TooltipFactory.GetBarValue(_draggedItem));
		if (actionHint.enabled)
		{
			RectTransform rectTransform = actionHint.rectTransform;
			rectTransform.anchorMax = rectTransform.anchorMin;
			Vector2 vector = Vector2.zero;
			switch (cachedHint)
			{
			case ItemActionHint.Drop:
				vector = new Vector2((0f - _draggedItemSize.x) * 0.5f * 0.9f, 0f);
				break;
			case ItemActionHint.Swap:
				vector = new Vector2((0f - _draggedItemSize.x) * 0.5f * 0.9f, _draggedItemSize.y * 0.5f * 0.9f);
				break;
			}
			float num = Vector3.Dot(aimingForward, rt.position - aimingPosition) * 0.001080918f;
			rectTransform.localScale = new Vector3(num / rt.localScale.x, num / rt.localScale.y, 1f);
			rectTransform.localPosition = vector;
		}
		dragSnap = false;
	}

	private void UpdateIconTransform()
	{
		if (ExtractParams())
		{
			rt.position = Vector3.Lerp(cursorPosition, dragSnapPosition, dragSnapT);
			rt.rotation = Quaternion.Lerp(cursorRotation, dragSnapRotation, dragSnapT);
			rt.localScale = new Vector3(Mathf.Lerp(cursorScale.x, dragSnapScale.x, dragSnapT), Mathf.Lerp(cursorScale.y, dragSnapScale.y, dragSnapT), 1f);
			draggedIcon.SetForegroundSize(Vector2.Lerp(_draggedItemSize, dragSnapForegroundSize, dragSnapT));
			Vector2 vector = Vector2.Lerp(_draggedItemSize, dragSnapBackgroundSize, dragSnapT);
			draggedIcon.SetBackgroundSize(vector);
			draggedIcon.SetBarSize(vector);
			float num = Mathf.Lerp(33f, dragSnapRadius, dragSnapT);
			draggedIcon.SetBackgroundRadius(num);
			draggedIcon.SetBarRadius(num);
		}
	}

	private void LockIcon(bool state)
	{
		if (lockedIcon != null)
		{
			if (state)
			{
				lockedIcon.OnLock();
			}
			else
			{
				lockedIcon.OnUnlock();
			}
		}
	}

	private void NotifyItemDragStart(Pickupable p)
	{
		if (onItemDragStart != null)
		{
			onItemDragStart(p);
		}
	}

	private void NotifyItemDragStop()
	{
		if (onItemDragStop != null)
		{
			onItemDragStop();
		}
	}
}
