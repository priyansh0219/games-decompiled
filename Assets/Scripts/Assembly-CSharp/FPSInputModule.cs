using System;
using System.Collections.Generic;
using UWE;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FPSInputModule : PointerInputModule
{
	private static readonly ExecuteEvents.EventFunction<IPointerHoverHandler> sPointerHoverHandler = ExecuteHover;

	private static readonly ExecuteEvents.EventFunction<IDragHoverHandler> sDragHoverEnterHandler = ExecuteEnter;

	private static readonly ExecuteEvents.EventFunction<IDragHoverHandler> sDragHoverStayHandler = ExecuteStay;

	private static readonly ExecuteEvents.EventFunction<IDragHoverHandler> sDragHoverExitHandler = ExecuteExit;

	public static FPSInputModule current;

	private bool _lockRotation;

	public bool lockPauseMenu;

	private int lockMovementFrame = -1;

	private bool _lockMovement;

	public float maxInteractionDistance = 3f;

	[AssertNotNull]
	public GameObject cursorPrefab;

	private GameObject[] dragHoverHandler = new GameObject[3];

	private GameObject lastPress;

	private Vector2 pointerPosition = Vector2.zero;

	private RaycastResult lastRaycastResult;

	private float lastValidRaycastTime;

	private GameObject _cursorCanvasGO;

	private Canvas _cursorCanvas;

	private Graphic _cursorGraphic;

	private RectTransform _cursorCanvasRT;

	private bool skipMouseEvent;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.LateUpdateInput;

	private readonly MouseState m_MouseState = new MouseState();

	private static readonly Comparison<RaycastResult> s_RaycastComparer = RaycastComparer;

	public bool lockRotation
	{
		get
		{
			return _lockRotation;
		}
		private set
		{
			if (_lockRotation != value)
			{
				_lockRotation = value;
				if (MainCameraControl.main != null)
				{
					MainCameraControl.main.SetEnabled(!_lockRotation);
				}
			}
		}
	}

	public bool lockMovement
	{
		get
		{
			return _lockMovement;
		}
		set
		{
			if (value)
			{
				lockMovementFrame = Time.frameCount;
				_lockMovement = true;
			}
			else if (lockMovementFrame != Time.frameCount)
			{
				_lockMovement = false;
			}
		}
	}

	public uGUI_InputGroup lastGroup { get; private set; }

	private static void ExecuteHover(IPointerHoverHandler handler, BaseEventData eventData)
	{
		handler.OnPointerHover(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
	}

	private static void ExecuteEnter(IDragHoverHandler handler, BaseEventData eventData)
	{
		handler.OnDragHoverEnter(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
	}

	private static void ExecuteStay(IDragHoverHandler handler, BaseEventData eventData)
	{
		handler.OnDragHoverStay(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
	}

	private static void ExecuteExit(IDragHoverHandler handler, BaseEventData eventData)
	{
		handler.OnDragHoverExit(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
	}

	protected override void Awake()
	{
		base.Awake();
		if (current != null)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			current = this;
		}
	}

	public override bool IsModuleSupported()
	{
		return true;
	}

	public override void ActivateModule()
	{
		base.ActivateModule();
	}

	public override void DeactivateModule()
	{
		base.DeactivateModule();
		ChangeGroup(null, lockMovement: false);
	}

	public override void UpdateModule()
	{
		base.UpdateModule();
	}

	public override bool ShouldActivateModule()
	{
		if (!base.ShouldActivateModule())
		{
			return false;
		}
		return true;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateInput, OnUpdate);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdateInput, OnUpdate);
	}

	public override void Process()
	{
	}

	public void OnUpdate()
	{
		if (!base.eventSystem.isFocused || TouchScreenKeyboardManager.visible || GameInput.IsRebinding)
		{
			return;
		}
		SendUpdateEventToSelectedObject();
		if (Input.GetKeyDown(KeyCode.F2))
		{
			VRUtil.Recenter();
		}
		bool flag = false;
		bool useGazeBasedCursor = VROptions.GetUseGazeBasedCursor();
		if (GameInput.PrimaryDevice == GameInput.Device.Controller || useGazeBasedCursor)
		{
			UWE.Utils.alwaysLockCursor = true;
		}
		else
		{
			UWE.Utils.alwaysLockCursor = false;
		}
		if (GameInput.PrimaryDevice == GameInput.Device.Keyboard)
		{
			flag = true;
		}
		if (!lockRotation)
		{
			flag = true;
		}
		if (useGazeBasedCursor)
		{
			flag = true;
		}
		if (flag)
		{
			if (skipMouseEvent)
			{
				skipMouseEvent = false;
				uGUI_Tooltip.Clear();
			}
			else
			{
				ProcessMouseEvent();
			}
			UpdateCursor();
		}
		if ((bool)Inventory.main)
		{
			Inventory.main.UpdateContainers();
		}
	}

	private void OnApplicationFocus(bool focusStatus)
	{
		if (focusStatus)
		{
			return;
		}
		MouseState mouseState = m_MouseState;
		for (int i = 0; i < 3; i++)
		{
			PointerEventData buttonData = mouseState.GetButtonState((PointerEventData.InputButton)i).eventData.buttonData;
			GameObject gameObject = dragHoverHandler[i];
			if (gameObject != null)
			{
				dragHoverHandler[i] = null;
				ExecuteEvents.Execute(gameObject, buttonData, sDragHoverExitHandler);
			}
			if (buttonData.dragging && buttonData.pointerDrag != null)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.endDragHandler);
			}
			buttonData.dragging = false;
			buttonData.pointerDrag = null;
		}
	}

	private Vector2 GetCursorScreenPosition()
	{
		if (VROptions.GetUseGazeBasedCursor() || !Input.mousePresent || Cursor.lockState == CursorLockMode.Locked)
		{
			return (Vector2)GraphicsUtil.GetScreenSize() * 0.5f;
		}
		return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
	}

	protected override MouseState GetMousePointerEventData()
	{
		GetPointerData(-1, out var data, create: true);
		data.Reset();
		Vector2 cursorScreenPosition = GetCursorScreenPosition();
		data.delta = Vector2.zero;
		data.position = cursorScreenPosition;
		data.scrollDelta = Input.mouseScrollDelta;
		data.button = PointerEventData.InputButton.Left;
		m_RaycastResultCache.Clear();
		RaycastResult raycastResult = default(RaycastResult);
		if (lastGroup == null || !lastGroup.Raycast(data, m_RaycastResultCache))
		{
			base.eventSystem.RaycastAll(data, m_RaycastResultCache);
		}
		m_RaycastResultCache.Sort(s_RaycastComparer);
		raycastResult = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
		raycastResult.screenPosition = cursorScreenPosition;
		m_RaycastResultCache.Clear();
		if (raycastResult.isValid)
		{
			Camera eventCamera = raycastResult.module.eventCamera;
			if (eventCamera != null)
			{
				raycastResult.worldPosition = eventCamera.ScreenPointToRay(raycastResult.screenPosition).GetPoint(raycastResult.distance);
			}
		}
		data.pointerCurrentRaycast = raycastResult;
		Vector2 canvasPosition = Vector2.zero;
		if (ScreenToCanvasPoint(raycastResult, cursorScreenPosition, ref canvasPosition))
		{
			data.delta = canvasPosition - pointerPosition;
			pointerPosition = canvasPosition;
			lastRaycastResult = raycastResult;
			lastValidRaycastTime = Time.unscaledTime;
		}
		else if (ScreenToCanvasPoint(lastRaycastResult, cursorScreenPosition, ref canvasPosition))
		{
			data.delta = canvasPosition - pointerPosition;
			pointerPosition = canvasPosition;
			lastRaycastResult.screenPosition = cursorScreenPosition;
		}
		else
		{
			lastRaycastResult = default(RaycastResult);
		}
		CursorManager.SetRaycastResult(lastRaycastResult);
		UpdateMouseState(data);
		return m_MouseState;
	}

	private static int RaycastComparer(RaycastResult lhs, RaycastResult rhs)
	{
		if (lhs.sortingLayer != rhs.sortingLayer)
		{
			int layerValueFromID = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
			int layerValueFromID2 = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
			return layerValueFromID.CompareTo(layerValueFromID2);
		}
		if (lhs.sortingOrder != rhs.sortingOrder)
		{
			return rhs.sortingOrder.CompareTo(lhs.sortingOrder);
		}
		if (lhs.depth != rhs.depth)
		{
			return rhs.depth.CompareTo(lhs.depth);
		}
		if (lhs.distance != rhs.distance)
		{
			return lhs.distance.CompareTo(rhs.distance);
		}
		return lhs.index.CompareTo(rhs.index);
	}

	public bool GetPointerDataFromInputModule(out PointerEventData evtData)
	{
		return GetPointerData(-1, out evtData, create: true);
	}

	public AxisEventData GetAxisEventDataFromInputModule(float x, float y, float moveDeadZone)
	{
		return GetAxisEventData(x, y, moveDeadZone);
	}

	private void UpdateMouseState(PointerEventData leftData)
	{
		GetPointerData(-2, out var data, create: true);
		CopyFromTo(leftData, data);
		data.button = PointerEventData.InputButton.Right;
		GetPointerData(-3, out var data2, create: true);
		CopyFromTo(leftData, data2);
		data2.button = PointerEventData.InputButton.Middle;
		m_MouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), leftData);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), data);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), data2);
		if (GameInput.PrimaryDevice == GameInput.Device.Controller)
		{
			bool buttonDown = GameInput.GetButtonDown(GameInput.button0);
			bool buttonUp = GameInput.GetButtonUp(GameInput.button0);
			if (m_MouseState.GetButtonState(PointerEventData.InputButton.Left).eventData.buttonState == PointerEventData.FramePressState.NotChanged)
			{
				m_MouseState.SetButtonState(PointerEventData.InputButton.Left, ConstructPressState(buttonDown, buttonUp), leftData);
			}
			buttonDown = GameInput.GetButtonDown(GameInput.button1);
			buttonUp = GameInput.GetButtonUp(GameInput.button1);
			if (m_MouseState.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonState == PointerEventData.FramePressState.NotChanged)
			{
				m_MouseState.SetButtonState(PointerEventData.InputButton.Right, ConstructPressState(buttonDown, buttonUp), data);
			}
		}
	}

	private static PointerEventData.FramePressState ConstructPressState(bool pressed, bool released)
	{
		PointerEventData.FramePressState result = PointerEventData.FramePressState.NotChanged;
		if (pressed && released)
		{
			result = PointerEventData.FramePressState.PressedAndReleased;
		}
		else if (pressed)
		{
			result = PointerEventData.FramePressState.Pressed;
		}
		else if (released)
		{
			result = PointerEventData.FramePressState.Released;
		}
		return result;
	}

	private void ProcessMouseEvent()
	{
		MouseState mousePointerEventData = GetMousePointerEventData();
		MouseButtonEventData eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
		MouseButtonEventData eventData2 = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData;
		MouseButtonEventData eventData3 = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData;
		PointerEventData buttonData = eventData.buttonData;
		PointerEventData buttonData2 = eventData2.buttonData;
		PointerEventData buttonData3 = eventData3.buttonData;
		RaycastResult pointerCurrentRaycast = buttonData.pointerCurrentRaycast;
		GameObject gameObject = pointerCurrentRaycast.gameObject;
		ProcessHover(buttonData);
		ProcessMousePress(eventData);
		ProcessMove(buttonData);
		ProcessDrag(buttonData);
		ProcessDragHover(buttonData, ref dragHoverHandler[0]);
		ProcessMousePress(eventData2);
		ProcessDrag(buttonData2);
		ProcessDragHover(buttonData2, ref dragHoverHandler[1]);
		ProcessMousePress(eventData3);
		ProcessDrag(buttonData3);
		ProcessDragHover(buttonData2, ref dragHoverHandler[2]);
		if (gameObject != null)
		{
			uGUI_InputGroup componentInParent = gameObject.GetComponentInParent<uGUI_InputGroup>();
			if (componentInParent != null && componentInParent != lastGroup && pointerCurrentRaycast.distance > maxInteractionDistance)
			{
				return;
			}
			ITooltip componentInParent2 = gameObject.GetComponentInParent<ITooltip>();
			if (componentInParent2 == null || (!componentInParent2.showTooltipOnDrag && (buttonData.dragging || buttonData2.dragging || buttonData3.dragging)))
			{
				uGUI_Tooltip.Clear();
			}
			else
			{
				uGUI_Tooltip.Set(componentInParent2);
			}
		}
		else
		{
			uGUI_Tooltip.Clear();
		}
		if (!Mathf.Approximately(buttonData.scrollDelta.sqrMagnitude, 0f))
		{
			ExecuteEvents.ExecuteHierarchy(ExecuteEvents.GetEventHandler<IScrollHandler>(pointerCurrentRaycast.gameObject), buttonData, ExecuteEvents.scrollHandler);
		}
	}

	private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
	{
		if (!useDragThreshold)
		{
			return true;
		}
		return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
	}

	protected override void ProcessDrag(PointerEventData pointerEvent)
	{
		if (!pointerEvent.IsPointerMoving() || pointerEvent.pointerDrag == null)
		{
			return;
		}
		if (!pointerEvent.dragging && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, base.eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
		{
			ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
			pointerEvent.dragging = true;
		}
		if (pointerEvent.dragging)
		{
			if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
			{
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;
			}
			ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
		}
	}

	private void ProcessDragHover(PointerEventData pointerEvent, ref GameObject dragHover)
	{
		GameObject gameObject = (pointerEvent.dragging ? pointerEvent.pointerCurrentRaycast.gameObject : null);
		GameObject gameObject2 = ((gameObject != null) ? ExecuteEvents.GetEventHandler<IDragHoverHandler>(gameObject) : null);
		if (gameObject2 != dragHover)
		{
			if (dragHover != null)
			{
				ExecuteEvents.Execute(dragHover, pointerEvent, sDragHoverExitHandler);
			}
			dragHover = gameObject2;
			if (dragHover != null)
			{
				ExecuteEvents.Execute(dragHover, pointerEvent, sDragHoverEnterHandler);
			}
		}
		if (dragHover != null)
		{
			ExecuteEvents.Execute(dragHover, pointerEvent, sDragHoverStayHandler);
		}
	}

	private new void CopyFromTo(PointerEventData src, PointerEventData dst)
	{
		dst.position = src.position;
		dst.delta = src.delta;
		dst.scrollDelta = src.scrollDelta;
		dst.pointerCurrentRaycast = src.pointerCurrentRaycast;
		dst.pointerEnter = src.pointerEnter;
	}

	private bool SendUpdateEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return baseEventData.used;
	}

	private void ProcessHover(PointerEventData pointerEvent)
	{
		GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
		if (!(gameObject == null))
		{
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerHoverHandler>(gameObject);
			if (!(eventHandler == null))
			{
				ExecuteEvents.Execute(eventHandler, pointerEvent, sPointerHoverHandler);
			}
		}
	}

	private void ProcessMousePress(MouseButtonEventData data)
	{
		PointerEventData buttonData = data.buttonData;
		GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;
		if (data.PressedThisFrame())
		{
			buttonData.eligibleForClick = true;
			buttonData.delta = Vector2.zero;
			buttonData.dragging = false;
			buttonData.useDragThreshold = true;
			buttonData.pressPosition = buttonData.position;
			buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
			DeselectIfSelectionChanged(gameObject, buttonData);
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			if (gameObject2 == null && gameObject != null)
			{
				ScrollRect componentInParent = gameObject.GetComponentInParent<ScrollRect>();
				if (componentInParent != null && componentInParent.content != null)
				{
					RectTransform content = componentInParent.content;
					if (gameObject.GetComponent<Transform>().IsChildOf(content))
					{
						gameObject2 = content.gameObject;
					}
				}
				if (gameObject2 == null && gameObject.GetComponentInParent<uGUI_InputGroup>() != null)
				{
					gameObject2 = gameObject;
				}
			}
			if (gameObject2 == null)
			{
				ChangeGroup(null, lockMovement: false);
			}
			else if (lastPress == null || gameObject2 != lastPress)
			{
				uGUI_InputGroup uGUI_InputGroup2 = ((gameObject2.GetComponentInParent<uGUI_QuickSlots>() != null) ? uGUI_PDA.main : ((!(gameObject2.GetComponentInParent<uGUI_PinnedRecipes>() != null)) ? gameObject2.GetComponentInParent<uGUI_InputGroup>() : lastGroup));
				if (uGUI_InputGroup2 != null && !uGUI_InputGroup2.IsAcceptedToOpenWithButton(buttonData.button))
				{
					ChangeGroup(null, lockMovement: false);
					gameObject2 = null;
				}
				else
				{
					ChangeGroup(uGUI_InputGroup2, lockMovement: false);
				}
			}
			lastPress = gameObject2;
			float unscaledTime = Time.unscaledTime;
			if (gameObject2 == buttonData.lastPress)
			{
				if (unscaledTime - buttonData.clickTime < 0.3f)
				{
					int clickCount = buttonData.clickCount + 1;
					buttonData.clickCount = clickCount;
				}
				else
				{
					buttonData.clickCount = 1;
				}
				buttonData.clickTime = unscaledTime;
			}
			else
			{
				buttonData.clickCount = 1;
			}
			buttonData.pointerPress = gameObject2;
			buttonData.rawPointerPress = gameObject;
			buttonData.clickTime = unscaledTime;
			buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (buttonData.pointerDrag != null)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (data.ReleasedThisFrame())
		{
			lastRaycastResult = default(RaycastResult);
			ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if (buttonData.pointerPress == eventHandler && buttonData.eligibleForClick)
			{
				ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerClickHandler);
			}
			else if (buttonData.pointerDrag != null)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.dropHandler);
			}
			buttonData.eligibleForClick = false;
			buttonData.pointerPress = null;
			buttonData.rawPointerPress = null;
			if (buttonData.pointerDrag != null && buttonData.dragging)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.endDragHandler);
			}
			buttonData.dragging = false;
			buttonData.pointerDrag = null;
			if (gameObject != buttonData.pointerEnter)
			{
				HandlePointerExitAndEnter(buttonData, null);
				HandlePointerExitAndEnter(buttonData, gameObject);
			}
		}
	}

	protected override void ProcessMove(PointerEventData pointerEvent)
	{
		GameObject newEnterTarget = pointerEvent.pointerCurrentRaycast.gameObject;
		HandlePointerExitAndEnter(pointerEvent, newEnterTarget);
	}

	private static RectTransform GetCanvasRectTransform(RaycastResult raycastResult)
	{
		GameObject gameObject = raycastResult.gameObject;
		if (gameObject == null)
		{
			return null;
		}
		Graphic component = gameObject.GetComponent<Graphic>();
		if (component == null)
		{
			return null;
		}
		if (raycastResult.module == null)
		{
			return null;
		}
		Canvas canvas = component.canvas;
		if (canvas == null)
		{
			return null;
		}
		return canvas.GetComponent<RectTransform>();
	}

	private static bool ScreenToCanvasPoint(RaycastResult raycastResult, Vector2 screenPosition, ref Vector2 canvasPosition)
	{
		RectTransform canvasRectTransform = GetCanvasRectTransform(raycastResult);
		if (canvasRectTransform == null)
		{
			return false;
		}
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, raycastResult.module.eventCamera, out canvasPosition);
	}

	private static bool ScreenToWorldPoint(RaycastResult raycastResult, Vector2 screenPosition, ref Vector3 worldPosition)
	{
		RectTransform canvasRectTransform = GetCanvasRectTransform(raycastResult);
		if (canvasRectTransform == null)
		{
			return false;
		}
		return RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRectTransform, screenPosition, raycastResult.module.eventCamera, out worldPosition);
	}

	public void EscapeMenu()
	{
		if (!lockPauseMenu && GameInput.GetButtonDown(GameInput.Button.UIMenu) && IngameMenu.main != null && !IngameMenu.main.selected)
		{
			IngameMenu.main.Open();
			GameInput.ClearInput();
		}
	}

	protected void ChangeGroup(uGUI_InputGroup newGroup, bool lockMovement)
	{
		if (lastGroup == null)
		{
			if (newGroup != null)
			{
				GameInput.ClearInput();
				newGroup.OnSelect(lockMovement);
				lastGroup = newGroup;
				lockRotation = true;
				skipMouseEvent = true;
			}
		}
		else if (newGroup == null)
		{
			GameInput.ClearInput();
			lastGroup.OnDeselect();
			lastGroup = null;
			lastPress = null;
			lockRotation = false;
			skipMouseEvent = true;
		}
		else if (lastGroup != newGroup)
		{
			GameInput.ClearInput();
			lastGroup.OnDeselect();
			newGroup.OnSelect(lockMovement);
			lastGroup = newGroup;
			skipMouseEvent = true;
		}
		else
		{
			lastGroup.OnReselect(lockMovement);
		}
		GamepadInputModule gamepadInputModule = GamepadInputModule.current;
		if (gamepadInputModule != null)
		{
			gamepadInputModule.OnGroupChanged(newGroup);
		}
	}

	public static uGUI_InputGroup SelectGroup(uGUI_InputGroup group, bool lockMovement = false)
	{
		if (current == null || !current.enabled)
		{
			return null;
		}
		uGUI_InputGroup result = current.lastGroup;
		current.ChangeGroup(group, lockMovement);
		return result;
	}

	public static void DeselectGroup(uGUI_InputGroup group, uGUI_InputGroup restoreGroup)
	{
		if (!(current == null) && current.enabled && group == current.lastGroup)
		{
			SelectGroup(restoreGroup);
			if (!EventSystem.current.alreadySelecting)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
	}

	public static GameObject BubbleEvent<T>(GameObject currentReceiver, BaseEventData eventData, ExecuteEvents.EventFunction<T> functor) where T : IEventSystemHandler
	{
		if (currentReceiver != null)
		{
			GameObject gameObject = null;
			Transform parent = currentReceiver.transform.parent;
			while (parent != null)
			{
				T component = parent.GetComponent<T>();
				if (!EqualityComparer<T>.Default.Equals(component, default(T)))
				{
					gameObject = parent.gameObject;
					break;
				}
				parent = parent.parent;
			}
			if (gameObject != null)
			{
				ExecuteEvents.Execute(gameObject, eventData, functor);
				return gameObject;
			}
		}
		return null;
	}

	private void InitializeCursor()
	{
		if (!(_cursorCanvasGO != null))
		{
			_cursorCanvasGO = UnityEngine.Object.Instantiate(cursorPrefab);
			_cursorCanvasGO.name = "WorldCursor";
			_cursorCanvas = _cursorCanvasGO.GetComponent<Canvas>();
			_cursorCanvasRT = _cursorCanvas.GetComponent<RectTransform>();
			_cursorGraphic = _cursorCanvasGO.GetComponentInChildren<Graphic>();
		}
	}

	private void UpdateCursor()
	{
		float num = 0.5f;
		float num2 = 0.1f;
		float num3 = Time.unscaledTime - lastValidRaycastTime;
		bool flag = lastGroup != null;
		if (!VROptions.GetUseGazeBasedCursor())
		{
			flag = false;
		}
		Vector3 worldPosition = lastRaycastResult.worldPosition;
		if (num3 > 0f)
		{
			if (num3 > num + num2)
			{
				flag = false;
			}
			else
			{
				Vector2 cursorScreenPosition = GetCursorScreenPosition();
				if (!ScreenToWorldPoint(lastRaycastResult, cursorScreenPosition, ref worldPosition))
				{
					flag = false;
				}
			}
		}
		InitializeCursor();
		if (flag)
		{
			GameObject gameObject = lastRaycastResult.gameObject;
			if (gameObject != null)
			{
				Graphic component = gameObject.GetComponent<Graphic>();
				if (component != null)
				{
					Canvas canvas = component.canvas;
					if (canvas != null)
					{
						RectTransform component2 = canvas.GetComponent<RectTransform>();
						_cursorCanvasGO.layer = gameObject.layer;
						_cursorCanvasRT.localScale = component2.lossyScale;
						_cursorCanvasRT.position = worldPosition;
						_cursorCanvasRT.rotation = component2.rotation;
						_cursorCanvas.sortingLayerID = canvas.sortingLayerID;
						_cursorCanvas.sortingOrder = canvas.sortingOrder + 1;
						Color color = _cursorGraphic.color;
						color.a = 1f - Mathf.Clamp01((num3 - num) / num2);
						_cursorGraphic.color = color;
					}
				}
			}
		}
		if (_cursorCanvasGO.activeSelf != flag)
		{
			_cursorCanvasGO.SetActive(flag);
		}
	}
}
