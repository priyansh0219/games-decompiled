using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class uGUI_CanvasScaler : MonoBehaviour
{
	public enum ScaleMode
	{
		DontScale = 0,
		Immediate = 1,
		Postponed = 2
	}

	public enum Mode
	{
		Static = 0,
		Inversed = 1,
		Parented = 2,
		World = 3
	}

	private static List<Action<float>> uiScaleListeners = new List<Action<float>>();

	private static float _uiScale = 1f;

	private const float positionEpsilon = 0.0001f;

	private const float rotationThreshold = 0.9961947f;

	private const float logBase = 2f;

	public Vector2 referenceResolution = new Vector2(1920f, 1080f);

	public Mode mode;

	public Mode vrMode;

	public float distance = 1f;

	[Tooltip("How this canvas reacts to UI scaling in settings.")]
	public ScaleMode scaleMode;

	private RectTransform _rectTransform;

	private Canvas _canvas;

	private bool _active = true;

	private float _scaleX = -1f;

	private float _scaleY = -1f;

	private float _width = -1f;

	private float _height = -1f;

	private bool isDirty = true;

	private float prevScaleFactor = 1f;

	private Transform _anchor;

	private Vector3 _spawnPosition;

	private Quaternion _spawnRotation;

	public static float uiScale
	{
		get
		{
			return _uiScale;
		}
		set
		{
			if (Mathf.Approximately(_uiScale, value))
			{
				return;
			}
			_uiScale = value;
			for (int num = uiScaleListeners.Count - 1; num >= 0; num--)
			{
				Action<float> action = uiScaleListeners[num];
				if (action == null)
				{
					uiScaleListeners.RemoveAt(num);
				}
				else
				{
					try
					{
						action(_uiScale);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			}
		}
	}

	public bool active
	{
		get
		{
			return _active;
		}
		set
		{
			if (_active != value)
			{
				_active = value;
				if (_active)
				{
					OnUpdate();
				}
			}
		}
	}

	private RectTransform rectTransform
	{
		get
		{
			if (!(_rectTransform != null))
			{
				return _rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	private Canvas canvas
	{
		get
		{
			if (!(_canvas != null))
			{
				return _canvas = GetComponent<Canvas>();
			}
			return _canvas;
		}
	}

	private Mode currentMode
	{
		get
		{
			if (!XRSettings.enabled)
			{
				return mode;
			}
			return vrMode;
		}
	}

	private void OnEnable()
	{
		ManagedUpdate.Queue queue = ManagedUpdate.Queue.LateUpdateAfterInput;
		switch (currentMode)
		{
		case Mode.Inversed:
		case Mode.Parented:
			queue = ManagedUpdate.Queue.PreCanvasCanvasScaler;
			break;
		}
		ManagedUpdate.Subscribe(queue, OnUpdate);
		ManagedCanvasUpdate.AddUICameraChangeListener(OnUICameraChange);
		SetAnchor();
		AddUIScaleListener(OnUIScaleChange);
		DisplayManager.OnDisplayChanged += OnScreenChanged;
	}

	private void OnDisable()
	{
		DisplayManager.OnDisplayChanged -= OnScreenChanged;
		RemoveUIScaleListener(OnUIScaleChange);
		ManagedUpdate.Unsubscribe(OnUpdate);
		ManagedCanvasUpdate.RemoveUICameraChangeListener(OnUICameraChange);
		isDirty = true;
		SetScaleFactor(1f);
	}

	private void OnUpdate()
	{
		if (!isDirty || !_active)
		{
			return;
		}
		Camera camera = MainCamera.camera;
		if (camera != null)
		{
			UpdateTransform(camera);
			UpdateFrustum(camera);
			switch (currentMode)
			{
			case Mode.Static:
				isDirty = false;
				break;
			case Mode.World:
				isDirty = false;
				break;
			default:
				isDirty = false;
				break;
			case Mode.Inversed:
			case Mode.Parented:
				break;
			}
		}
	}

	private void OnUIScaleChange(float scale)
	{
		if (scaleMode == ScaleMode.Immediate)
		{
			isDirty = true;
		}
		SetTextScaleDirtyIfNecessary();
	}

	private void SetTextScaleDirtyIfNecessary()
	{
		if (currentMode == Mode.Static)
		{
			SetTextScaleDirty();
		}
	}

	private void SetTextScaleDirty()
	{
		using (ListPool<TextMeshProUGUI> listPool = Pool<ListPool<TextMeshProUGUI>>.Get())
		{
			List<TextMeshProUGUI> list = listPool.list;
			GetComponentsInChildren(includeInactive: false, list);
			for (int i = 0; i < list.Count; i++)
			{
				TextMeshProUGUI textMeshProUGUI = list[i];
				if (!textMeshProUGUI.isTextObjectScaleStatic)
				{
					textMeshProUGUI.SetScaleDirty();
				}
			}
		}
	}

	private void OnUICameraChange(Camera camera)
	{
		switch (currentMode)
		{
		case Mode.Static:
			isDirty = true;
			break;
		case Mode.Inversed:
		case Mode.Parented:
		case Mode.World:
			break;
		}
	}

	private void OnScreenChanged()
	{
		SetTextScaleDirtyIfNecessary();
	}

	private void UpdateTransform(Camera cam)
	{
		Transform component = cam.GetComponent<Transform>();
		Mode mode = currentMode;
		Vector3 vector = Vector3.zero;
		Quaternion quaternion = Quaternion.identity;
		switch (mode)
		{
		case Mode.Static:
		{
			Camera uICamera = ManagedCanvasUpdate.GetUICamera();
			if (uICamera != null)
			{
				Transform transform2 = uICamera.transform;
				vector = transform2.position + transform2.forward * distance;
				quaternion = transform2.rotation;
			}
			else
			{
				vector = new Vector3(0f, 0f, distance);
				quaternion = Quaternion.identity;
			}
			break;
		}
		case Mode.World:
		{
			Transform transform = ((component.parent != null) ? component.parent : component);
			if (transform != null)
			{
				vector = transform.position + transform.forward * distance;
				quaternion = transform.rotation;
			}
			else
			{
				vector = new Vector3(0f, 0f, distance);
				quaternion = Quaternion.identity;
			}
			break;
		}
		case Mode.Inversed:
		{
			Vector3 vector3;
			Quaternion quaternion3;
			if (_anchor != null)
			{
				Quaternion rotation = _anchor.rotation;
				vector3 = _anchor.position + rotation * _spawnPosition;
				quaternion3 = rotation * _spawnRotation;
			}
			else
			{
				vector3 = _spawnPosition;
				quaternion3 = _spawnRotation;
			}
			Vector3 vector4 = vector3 - component.position;
			Quaternion quaternion4 = Quaternion.Inverse(component.rotation);
			vector = quaternion4 * vector4;
			quaternion = quaternion4 * quaternion3;
			break;
		}
		case Mode.Parented:
		{
			Vector3 vector2 = _spawnPosition - component.localPosition;
			Quaternion quaternion2 = Quaternion.Inverse(component.localRotation);
			vector = quaternion2 * vector2;
			quaternion = quaternion2 * _spawnRotation;
			break;
		}
		}
		bool flag = true;
		if (!XRSettings.enabled || (mode != Mode.Inversed && mode != Mode.Parented))
		{
			float sqrMagnitude = (vector - rectTransform.position).sqrMagnitude;
			float num = Quaternion.Dot(quaternion, rectTransform.rotation);
			flag = sqrMagnitude > 9.999999E-09f || num < 0.9961947f;
		}
		if (flag)
		{
			rectTransform.SetPositionAndRotation(vector, quaternion);
		}
	}

	private void UpdateFrustum(Camera cam)
	{
		if (currentMode != Mode.Inversed || !(_anchor != null))
		{
			Vector2Int screenSize = GraphicsUtil.GetScreenSize();
			float num = (float)screenSize.x / (float)screenSize.y;
			float num2 = distance * Mathf.Tan(cam.fieldOfView * 0.5f * ((float)Math.PI / 180f));
			float num3 = num2 * num;
			num3 *= 2f;
			num2 *= 2f;
			if (XRSettings.enabled)
			{
				float num4 = 0.1f;
				num3 *= 1f + num4;
				num2 *= 1f + num4;
			}
			float num5 = num3 / (float)screenSize.x;
			float num6 = num2 / (float)screenSize.y;
			float a = (float)screenSize.x / referenceResolution.x;
			float b = (float)screenSize.y / referenceResolution.y;
			float num7 = Mathf.Min(a, b);
			if (scaleMode > ScaleMode.DontScale)
			{
				num7 *= _uiScale;
			}
			float num8 = 1f / num7;
			float num9 = (float)screenSize.x * num8;
			float num10 = (float)screenSize.y * num8;
			float num11 = num5 * num7;
			float num12 = num6 * num7;
			if (_width != num9)
			{
				_width = num9;
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _width);
			}
			if (_height != num10)
			{
				_height = num10;
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _height);
			}
			if (_scaleX != num11 || _scaleY != num12)
			{
				_scaleX = num11;
				_scaleY = num12;
				rectTransform.localScale = new Vector3(_scaleX, _scaleY, _scaleX);
			}
			SetScaleFactor(num7);
		}
	}

	private void SetScaleFactor(float scaleFactor)
	{
		if (prevScaleFactor != scaleFactor)
		{
			prevScaleFactor = scaleFactor;
			canvas.scaleFactor = scaleFactor;
		}
	}

	public void SetDirty()
	{
		isDirty = true;
	}

	public void SetAnchor(Transform anchor)
	{
		_anchor = anchor;
		_spawnPosition = Vector3.zero;
		_spawnRotation = Quaternion.identity;
	}

	public void SetAnchor()
	{
		Camera camera = MainCamera.camera;
		if (camera == null)
		{
			return;
		}
		Transform transform = camera.transform;
		Mode mode = currentMode;
		if (mode != Mode.Inversed)
		{
			if (mode == Mode.Parented)
			{
				_spawnPosition = transform.localPosition + transform.localRotation * Vector3.forward * distance;
				_spawnRotation = transform.localRotation;
				return;
			}
		}
		else
		{
			if (!(_anchor == null))
			{
				return;
			}
			if (base.transform.parent != null)
			{
				_anchor = base.transform.parent;
				_spawnPosition = base.transform.localPosition;
				_spawnRotation = base.transform.localRotation;
				return;
			}
		}
		_spawnPosition = transform.position + transform.forward * distance;
		_spawnRotation = transform.rotation;
	}

	public static void AddUIScaleListener(Action<float> listener)
	{
		if (!uiScaleListeners.Contains(listener))
		{
			uiScaleListeners.Add(listener);
		}
	}

	public static void RemoveUIScaleListener(Action<float> listener)
	{
		uiScaleListeners.Remove(listener);
	}

	public static float GetInverseScale(Transform transform)
	{
		float result = 1f;
		uGUI_CanvasScaler componentInParent = transform.GetComponentInParent<uGUI_CanvasScaler>();
		if (componentInParent != null && componentInParent.scaleMode > ScaleMode.DontScale)
		{
			result = 1f / uiScale;
		}
		return result;
	}
}
