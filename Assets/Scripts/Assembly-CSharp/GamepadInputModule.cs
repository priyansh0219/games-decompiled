using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GamepadInputModule : MonoBehaviour
{
	public struct ReceiverEnumerable<ReceiverType> where ReceiverType : class
	{
		private readonly GamepadInputModule inputModule;

		public ReceiverEnumerable(GamepadInputModule _inputModule)
		{
			inputModule = _inputModule;
		}

		public ReceiverEnumerator<ReceiverType> GetEnumerator()
		{
			return new ReceiverEnumerator<ReceiverType>(inputModule);
		}
	}

	public struct ReceiverEnumerator<ReceiverType> where ReceiverType : class
	{
		private int step;

		private GamepadInputModule inputModule;

		private ReceiverType current;

		public ReceiverType Current => current;

		public ReceiverEnumerator(GamepadInputModule _inputModule)
		{
			inputModule = _inputModule;
			current = null;
			step = 0;
		}

		public bool MoveNext()
		{
			while (true)
			{
				step++;
				if (step == 1)
				{
					if (inputModule.currentNavigableGrid != null)
					{
						object selectedItem = inputModule.currentNavigableGrid.GetSelectedItem();
						if (selectedItem is ReceiverType)
						{
							current = selectedItem as ReceiverType;
							return true;
						}
					}
				}
				else if (step == 2)
				{
					if (inputModule.currentNavigableGrid is ReceiverType)
					{
						current = inputModule.currentNavigableGrid as ReceiverType;
						return true;
					}
				}
				else
				{
					if (step != 3)
					{
						break;
					}
					if (inputModule.currentGroup is ReceiverType)
					{
						current = inputModule.currentGroup as ReceiverType;
						return true;
					}
				}
			}
			return false;
		}

		public void Reset()
		{
			step = 0;
		}
	}

	private const float repeatDelay = 0.5f;

	private const float inputActionsPerSecond = 9f;

	private const float deadZone = 0.6f;

	private const float scrollSpeedMultiplyThreshold = 0.75f;

	private const float scrollSpeedMultiplyRatio = 0.75f;

	private const float scrollSpeedMultiplyInterval = 1f;

	private const float scrollSpeedMultiplierMax = 5f;

	private const ManagedUpdate.Queue queueUpdate = ManagedUpdate.Queue.LateUpdateInput;

	private const ManagedUpdate.Queue queueSelection = ManagedUpdate.Queue.UILayoutCompleteSelection;

	public static GamepadInputModule current;

	public bool isControlling;

	public GameObject selectorPrefab;

	public float selectorPadding = 10f;

	public float selectorRectSpeed = 15f;

	public float selectorAlpha = 0.8f;

	private GameObject _selectorCanvasGO;

	private CanvasGroup _selectorCanvasGroup;

	private Canvas _selectorCanvas;

	private Image _selectorIcon;

	private RectTransform _selectorCanvasRT;

	private RectTransform _selectorRT;

	private TMP_InputField currentInputField;

	private uGUI_INavigableIconGrid currentNavigableGrid;

	private uGUI_InputGroup currentGroup;

	private bool skipOneInputFrame;

	private bool usingController;

	private GameInput.Button[] dispatchableButtons;

	private Vector2int moveDirection = new Vector2int(0, 0);

	private int consecutiveMoveCount;

	private Vector2 lastMoveVector = new Vector2(0f, 0f);

	private float prevActionTime;

	private float scrollSpeedMultiplyStart = -1f;

	public bool UsingController => usingController;

	private void Awake()
	{
		if (current != null)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			current = this;
		}
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateInput, OnUpdate);
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UILayoutCompleteSelection, UpdateSelectionIndicator);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UILayoutCompleteSelection, UpdateSelectionIndicator);
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdateInput, OnUpdate);
	}

	private void InitializeSelectionIndicator()
	{
		if (!(_selectorRT != null))
		{
			Vector2 vector = new Vector2(0f, 0f);
			_selectorCanvasGO = new GameObject("SelectorCanvas");
			_selectorCanvasGO.layer = LayerID.UI;
			_selectorCanvas = _selectorCanvasGO.AddComponent<Canvas>();
			_selectorCanvas.renderMode = RenderMode.WorldSpace;
			_selectorCanvasRT = _selectorCanvas.GetComponent<RectTransform>();
			_selectorCanvasRT.anchorMin = vector;
			_selectorCanvasRT.anchorMax = vector;
			_selectorCanvasRT.pivot = vector;
			_selectorCanvasRT.sizeDelta = vector;
			_selectorCanvasGroup = _selectorCanvasGO.AddComponent<CanvasGroup>();
			GameObject gameObject = UnityEngine.Object.Instantiate(selectorPrefab);
			_selectorIcon = gameObject.GetComponent<Image>();
			_selectorIcon.type = Image.Type.Sliced;
			_selectorIcon.fillCenter = false;
			StartCoroutine(SetupSpriteAsync());
			gameObject.layer = LayerID.UI;
			_selectorRT = gameObject.GetComponent<RectTransform>();
			_selectorRT.SetParent(_selectorCanvasRT, worldPositionStays: false);
			_selectorRT.anchorMin = vector;
			_selectorRT.anchorMax = vector;
			_selectorRT.pivot = vector;
		}
	}

	private IEnumerator SetupSpriteAsync()
	{
		while (!SpriteManager.hasInitialized)
		{
			yield return null;
		}
		_selectorIcon.sprite = SpriteManager.Get(SpriteManager.Group.None, "selector");
	}

	private void UpdateSelectionIndicator()
	{
		InitializeSelectionIndicator();
		if (isControlling && currentNavigableGrid != null && currentNavigableGrid.ShowSelector)
		{
			Graphic selectedIcon = currentNavigableGrid.GetSelectedIcon();
			if (selectedIcon != null && selectedIcon.isActiveAndEnabled && selectedIcon.gameObject.activeInHierarchy)
			{
				Canvas canvas = selectedIcon.canvas;
				RectTransform component = canvas.GetComponent<RectTransform>();
				CanvasGroup componentInParent = selectedIcon.GetComponentInParent<CanvasGroup>();
				Color color = _selectorIcon.color;
				color.a = selectorAlpha;
				if (componentInParent != null)
				{
					color.a *= componentInParent.alpha;
				}
				_selectorIcon.color = color;
				_selectorCanvasGO.layer = selectedIcon.gameObject.layer;
				_selectorCanvasRT.localScale = component.lossyScale;
				_selectorCanvasRT.position = component.position;
				_selectorCanvasRT.rotation = component.rotation;
				_selectorCanvas.sortingLayerID = canvas.sortingLayerID;
				_selectorCanvas.sortingOrder = canvas.sortingOrder + 1;
				RectTransformExtensions.GetCanvasRect(selectedIcon, out var rect);
				Vector3 vector = new Vector3(rect.x - selectorPadding, rect.y - selectorPadding);
				Vector3 vector2 = new Vector3(rect.x + rect.width + selectorPadding, rect.y + rect.height + selectorPadding);
				if (_selectorCanvasGroup.alpha > 0f)
				{
					Vector2 vector3 = _selectorRT.anchoredPosition3D;
					Vector2 a = vector3 + _selectorRT.sizeDelta;
					float t = selectorRectSpeed * Time.unscaledDeltaTime;
					Vector2 vector4 = Vector2.Lerp(vector3, vector, t);
					Vector2 vector5 = Vector2.Lerp(a, vector2, t);
					_selectorRT.anchoredPosition = new Vector2(vector4.x, vector4.y);
					_selectorRT.sizeDelta = new Vector2(vector5.x - vector4.x, vector5.y - vector4.y);
				}
				else
				{
					_selectorRT.anchoredPosition = new Vector2(vector.x, vector.y);
					_selectorRT.sizeDelta = new Vector2(vector2.x - vector.x, vector2.y - vector.y);
					_selectorCanvasGO.SetActive(value: true);
					_selectorCanvasGroup.alpha = 1f;
				}
				return;
			}
		}
		_selectorCanvasGroup.alpha = 0f;
	}

	private void Start()
	{
		dispatchableButtons = (GameInput.Button[])Enum.GetValues(typeof(GameInput.Button));
		skipOneInputFrame = true;
		isControlling = false;
	}

	private void OnUpdate()
	{
		isControlling = false;
		bool flag = usingController;
		usingController = !VROptions.GetUseGazeBasedCursor() && GameInput.PrimaryDevice == GameInput.Device.Controller;
		if (!usingController)
		{
			if (flag && currentNavigableGrid != null)
			{
				object selectedItem = currentNavigableGrid.GetSelectedItem();
				if (selectedItem == null || !(selectedItem is TMP_InputField))
				{
					currentNavigableGrid.DeselectItem();
				}
			}
			return;
		}
		if (!flag)
		{
			OnGroupChanged(currentGroup);
		}
		if (skipOneInputFrame)
		{
			skipOneInputFrame = false;
			return;
		}
		if (IsInputAllowed() && currentGroup != null)
		{
			isControlling = true;
			ProcessInput();
		}
		if ((bool)Inventory.main)
		{
			Inventory.main.UpdateContainers();
		}
	}

	private bool IsInputAllowed()
	{
		if (WaitScreen.IsWaiting)
		{
			return false;
		}
		return Application.isFocused;
	}

	public void SetCurrentGrid(uGUI_INavigableIconGrid currentGrid)
	{
		if (currentNavigableGrid != null)
		{
			currentNavigableGrid.DeselectItem();
		}
		currentNavigableGrid = currentGrid;
	}

	public uGUI_INavigableIconGrid GetCurrentGrid()
	{
		return currentNavigableGrid;
	}

	public void OnGroupChanged(uGUI_InputGroup newGroup)
	{
		if (newGroup == null)
		{
			currentGroup = null;
			currentNavigableGrid = null;
			return;
		}
		currentGroup = newGroup;
		if (usingController)
		{
			skipOneInputFrame = true;
			isControlling = true;
			FPSInputModule.current.lockMovement = true;
		}
	}

	private void UpdateMoveDirection()
	{
		moveDirection.x = 0;
		moveDirection.y = 0;
		float unscaledTime = Time.unscaledTime;
		bool buttonDown;
		Vector2 uIDirection = GetUIDirection(out buttonDown);
		bool flag = buttonDown && consecutiveMoveCount == 0;
		if (Mathf.Approximately(uIDirection.x, 0f) && Mathf.Approximately(uIDirection.y, 0f))
		{
			consecutiveMoveCount = 0;
			return;
		}
		bool flag2 = Vector2.Dot(uIDirection, lastMoveVector) > 0f;
		if (!flag)
		{
			if (flag2 && consecutiveMoveCount == 1)
			{
				flag = unscaledTime > prevActionTime + 0.5f;
			}
			else if (consecutiveMoveCount > 1)
			{
				flag = unscaledTime > prevActionTime + 1f / 9f;
			}
		}
		if (!flag)
		{
			return;
		}
		float magnitude = uIDirection.magnitude;
		if (magnitude >= 0.6f)
		{
			float num = uIDirection.x / magnitude;
			float num2 = uIDirection.y / magnitude;
			int num3 = ((!(num < 0f)) ? 1 : (-1));
			int num4 = ((!(num2 < 0f)) ? 1 : (-1));
			if (num * (float)num3 > 0.38268343f)
			{
				moveDirection.x = num3;
			}
			if (num2 * (float)num4 > 0.38268343f)
			{
				moveDirection.y = num4;
			}
		}
		if (moveDirection.x != 0 || moveDirection.y != 0)
		{
			if (!flag2)
			{
				consecutiveMoveCount = 0;
			}
			consecutiveMoveCount++;
			prevActionTime = unscaledTime;
			lastMoveVector = uIDirection;
		}
		else
		{
			consecutiveMoveCount = 0;
		}
	}

	private void HandleNavigation()
	{
		if (currentNavigableGrid == null)
		{
			return;
		}
		bool flag = currentNavigableGrid.SelectItemInDirection(moveDirection.x, moveDirection.y);
		if ((moveDirection.x != 0 || moveDirection.y != 0) && !flag)
		{
			uGUI_INavigableIconGrid navigableGridInDirection = currentNavigableGrid.GetNavigableGridInDirection(moveDirection.x, moveDirection.y);
			if (navigableGridInDirection != null)
			{
				Graphic graphic = (currentNavigableGrid.EmulateRaycast ? currentNavigableGrid.GetSelectedIcon() : null);
				if (graphic == null)
				{
					navigableGridInDirection.SelectFirstItem();
					currentNavigableGrid = navigableGridInDirection;
				}
				else
				{
					Vector3 position = graphic.rectTransform.position;
					object selectedItem = currentNavigableGrid.GetSelectedItem();
					currentNavigableGrid.DeselectItem();
					if (navigableGridInDirection.SelectItemClosestToPosition(position))
					{
						currentNavigableGrid = navigableGridInDirection;
					}
					else
					{
						currentNavigableGrid.SelectItem(selectedItem);
					}
				}
			}
		}
		if (TryEmulateRaycast(out var raycastResult))
		{
			CursorManager.SetRaycastResult(raycastResult);
		}
	}

	public bool TryEmulateRaycast(out RaycastResult raycastResult)
	{
		raycastResult = default(RaycastResult);
		if (currentNavigableGrid != null && currentNavigableGrid.EmulateRaycast)
		{
			Graphic selectedIcon = currentNavigableGrid.GetSelectedIcon();
			if (selectedIcon != null)
			{
				Canvas canvas = selectedIcon.canvas;
				if (canvas != null)
				{
					BaseRaycaster component = canvas.GetComponent<BaseRaycaster>();
					if (component != null)
					{
						RectTransform rectTransform = selectedIcon.rectTransform;
						Vector3 vector = rectTransform.TransformPoint(rectTransform.rect.center);
						raycastResult.gameObject = selectedIcon.gameObject;
						raycastResult.module = component;
						raycastResult.index = 0f;
						raycastResult.depth = selectedIcon.depth;
						raycastResult.sortingLayer = canvas.sortingLayerID;
						raycastResult.sortingOrder = canvas.sortingOrder;
						Camera eventCamera = component.eventCamera;
						if (eventCamera != null)
						{
							raycastResult.distance = (vector - eventCamera.transform.position).magnitude;
							Vector3 vector2 = eventCamera.WorldToScreenPoint(vector);
							raycastResult.screenPosition = new Vector2(vector2.x, vector2.y);
						}
						else
						{
							raycastResult.distance = 0f;
							raycastResult.screenPosition = Vector2.zero;
						}
						raycastResult.worldPosition = vector;
						raycastResult.worldNormal = rectTransform.up;
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool TranslateButtonEvent(object selectedItem, GameInput.Button button)
	{
		if (selectedItem is ISelectable)
		{
			return (selectedItem as ISelectable).OnButtonDown(button);
		}
		if (selectedItem is IPointerClickHandler && button == GameInput.Button.UISubmit)
		{
			FPSInputModule.current.GetPointerDataFromInputModule(out var evtData);
			evtData.button = PointerEventData.InputButton.Left;
			((IPointerClickHandler)selectedItem).OnPointerClick(evtData);
			return true;
		}
		if (selectedItem is uGUI_Choice)
		{
			uGUI_Choice uGUI_Choice2 = selectedItem as uGUI_Choice;
			switch (button)
			{
			case GameInput.Button.UIRightStickAdjustLeft:
				uGUI_Choice2.PreviousChoice();
				return true;
			case GameInput.Button.UISubmit:
			case GameInput.Button.UIRightStickAdjustRight:
				uGUI_Choice2.NextChoice();
				return true;
			}
		}
		if (selectedItem is uGUI_ColorChoice)
		{
			uGUI_ColorChoice uGUI_ColorChoice2 = selectedItem as uGUI_ColorChoice;
			switch (button)
			{
			case GameInput.Button.UIRightStickAdjustLeft:
				uGUI_ColorChoice2.Left();
				return true;
			case GameInput.Button.UISubmit:
			case GameInput.Button.UIRightStickAdjustRight:
				uGUI_ColorChoice2.Right();
				return true;
			}
		}
		return false;
	}

	private bool TranslateScrollEvent(object selectedItem, float scrollDelta)
	{
		if (selectedItem is Scrollbar)
		{
			Scrollbar scrollbar = selectedItem as Scrollbar;
			float num = scrollDelta * Time.unscaledDeltaTime * 10f;
			if (num != 0f)
			{
				scrollbar.value += num;
			}
			return true;
		}
		return false;
	}

	private bool TranslateAdjustEvent(object selectedItem, Vector2 adjustDelta)
	{
		if (selectedItem is uGUI_SnappingSlider)
		{
			uGUI_SnappingSlider uGUI_SnappingSlider2 = selectedItem as uGUI_SnappingSlider;
			float num = ((Mathf.Abs(adjustDelta.x) >= Mathf.Abs(adjustDelta.y)) ? adjustDelta.x : adjustDelta.y) * Time.unscaledDeltaTime;
			if (num != 0f)
			{
				uGUI_SnappingSlider2.normalizedUnsnappedValue += num;
			}
			return true;
		}
		if (selectedItem is Slider)
		{
			Slider slider = selectedItem as Slider;
			float num2 = ((Mathf.Abs(adjustDelta.x) >= Mathf.Abs(adjustDelta.y)) ? adjustDelta.x : adjustDelta.y) * Time.unscaledDeltaTime;
			if (num2 != 0f)
			{
				slider.normalizedValue += num2;
			}
			return true;
		}
		return false;
	}

	private void DispatchInputEvents()
	{
		object obj = null;
		if (currentNavigableGrid != null)
		{
			obj = currentNavigableGrid.GetSelectedItem();
		}
		GameInput.Button[] array = dispatchableButtons;
		foreach (GameInput.Button button in array)
		{
			if (GameInput.GetButtonDown(button))
			{
				if (obj != null && TranslateButtonEvent(obj, button))
				{
					break;
				}
				ReceiverEnumerator<uGUI_IButtonReceiver> enumerator = GetEventReceivers<uGUI_IButtonReceiver>().GetEnumerator();
				while (enumerator.MoveNext() && !enumerator.Current.OnButtonDown(button))
				{
				}
			}
		}
		Vector2 vector = GameInput.GetVector2(GameInput.Button.UIAdjust);
		float speedMultiplier = 1f;
		float num = ((Mathf.Abs(vector.y) < 0.05f) ? 0f : vector.y);
		if (Mathf.Abs(num) > 0.75f)
		{
			if (scrollSpeedMultiplyStart < 0f)
			{
				scrollSpeedMultiplyStart = Time.unscaledTime;
			}
			float num2 = Time.unscaledTime - scrollSpeedMultiplyStart;
			speedMultiplier = Mathf.Clamp(1f + num2 * 0.75f + Mathf.Floor(num2 / 1f), 1f, 5f);
		}
		else
		{
			scrollSpeedMultiplyStart = -1f;
		}
		if (num != 0f)
		{
			bool flag = false;
			if (obj != null)
			{
				flag = TranslateScrollEvent(obj, num);
			}
			if (!flag)
			{
				ReceiverEnumerator<uGUI_IScrollReceiver> enumerator2 = GetEventReceivers<uGUI_IScrollReceiver>().GetEnumerator();
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current.OnScroll(num, speedMultiplier))
					{
						flag = true;
						break;
					}
				}
			}
		}
		if ((!Mathf.Approximately(vector.x, 0f) || !Mathf.Approximately(vector.y, 0f)) && (obj == null || !TranslateAdjustEvent(obj, vector)))
		{
			ReceiverEnumerator<uGUI_IAdjustReceiver> enumerator3 = GetEventReceivers<uGUI_IAdjustReceiver>().GetEnumerator();
			while (enumerator3.MoveNext() && !enumerator3.Current.OnAdjust(vector))
			{
			}
		}
	}

	private void UpdateTooltip()
	{
		object obj = null;
		if (currentNavigableGrid != null)
		{
			obj = currentNavigableGrid.GetSelectedItem();
		}
		ITooltip tooltip = obj as ITooltip;
		if (tooltip == null || tooltip.Equals(null))
		{
			Component component = obj as Component;
			if (component != null)
			{
				tooltip = component.GetComponentInParent<ITooltip>();
			}
		}
		if (tooltip != null && !tooltip.Equals(null))
		{
			uGUI_Tooltip.Set(tooltip);
		}
		else
		{
			uGUI_Tooltip.Clear();
		}
	}

	private ReceiverEnumerable<ReceiverType> GetEventReceivers<ReceiverType>() where ReceiverType : class
	{
		return new ReceiverEnumerable<ReceiverType>(this);
	}

	private void ProcessInput()
	{
		if (currentNavigableGrid != null)
		{
			Selectable selectable = currentNavigableGrid.GetSelectedItem() as Selectable;
			if (selectable != null)
			{
				selectable.Select();
			}
		}
		if (!TouchScreenKeyboardManager.visible)
		{
			UpdateMoveDirection();
			HandleNavigation();
			DispatchInputEvents();
			UpdateTooltip();
		}
	}

	public void SelectItem(Selectable selectedItem)
	{
		if (currentNavigableGrid != null)
		{
			currentNavigableGrid.SelectItem(selectedItem);
		}
	}

	public static Vector2 GetUIDirection(out bool buttonDown)
	{
		Vector2 zero = Vector2.zero;
		buttonDown = false;
		if (GameInput.GetButtonDown(GameInput.Button.UIDown))
		{
			buttonDown = true;
			zero.y = 1f;
			return zero;
		}
		if (GameInput.GetButtonHeld(GameInput.Button.UIDown))
		{
			zero.y = 1f;
			return zero;
		}
		if (GameInput.GetButtonDown(GameInput.Button.UIUp))
		{
			buttonDown = true;
			zero.y = -1f;
			return zero;
		}
		if (GameInput.GetButtonHeld(GameInput.Button.UIUp))
		{
			zero.y = -1f;
			return zero;
		}
		if (GameInput.GetButtonDown(GameInput.Button.UIRight))
		{
			buttonDown = true;
			zero.x = 1f;
			return zero;
		}
		if (GameInput.GetButtonHeld(GameInput.Button.UIRight))
		{
			zero.x = 1f;
			return zero;
		}
		if (GameInput.GetButtonDown(GameInput.Button.UILeft))
		{
			buttonDown = true;
			zero.x = -1f;
			return zero;
		}
		if (GameInput.GetButtonHeld(GameInput.Button.UILeft))
		{
			zero.x = -1f;
			return zero;
		}
		return zero;
	}

	private void DrawDebug()
	{
		uGUI_INavigableIconGrid uGUI_INavigableIconGrid2 = currentNavigableGrid;
		if (uGUI_INavigableIconGrid2 != null && !uGUI_INavigableIconGrid2.Equals(null))
		{
			Type type = uGUI_INavigableIconGrid2.GetType();
			Component component = null;
			if (typeof(Component).IsAssignableFrom(type))
			{
				component = uGUI_INavigableIconGrid2 as Component;
			}
			Dbg.Write("currentNavigableGrid: {0} '{1}'", type, (component != null) ? component.gameObject.name : string.Empty);
			object selectedItem = uGUI_INavigableIconGrid2.GetSelectedItem();
			if (selectedItem != null)
			{
				Type type2 = selectedItem.GetType();
				Component component2 = null;
				if (typeof(Component).IsAssignableFrom(type))
				{
					component2 = selectedItem as Component;
				}
				Dbg.Write("selected: {0} '{1}'", type2, (component2 != null) ? component2.gameObject.name : string.Empty);
				if (component2 != null)
				{
					Dbg.Write(Dbg.LogHierarchy(component2.gameObject));
				}
			}
			else
			{
				Dbg.Write("selected: null");
			}
			Graphic graphic = (uGUI_INavigableIconGrid2.ShowSelector ? uGUI_INavigableIconGrid2.GetSelectedIcon() : null);
			if (graphic != null)
			{
				Type type3 = graphic.GetType();
				Dbg.Write("graphic: {0} '{1}' active:{2}", type3, graphic.gameObject.name, graphic.isActiveAndEnabled && graphic.gameObject.activeInHierarchy);
				Dbg.Write(Dbg.LogHierarchy(graphic.gameObject));
			}
			else
			{
				Dbg.Write("graphic: null");
			}
		}
		else
		{
			Dbg.Write("currentNavigableGrid: null");
		}
	}
}
