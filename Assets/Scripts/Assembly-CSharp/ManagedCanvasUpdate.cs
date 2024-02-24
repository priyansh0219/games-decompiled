using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ManagedCanvasUpdate : MonoBehaviour
{
	public delegate void OnUICameraChange(Camera camera);

	private static ManagedCanvasUpdate _main;

	private static Camera[] allCameras = new Camera[2];

	private static Camera uiCamera = null;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.LateUpdateCamera;

	private List<OnUICameraChange> uiCameraChangeListeners = new List<OnUICameraChange>();

	private Vector2Int lastScreenSize;

	private float lastFieldOfView;

	public static ManagedCanvasUpdate main
	{
		get
		{
			if (_main == null)
			{
				GameObject obj = new GameObject("ManagedCanvasUpdate");
				_main = obj.AddComponent<ManagedCanvasUpdate>();
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			return _main;
		}
	}

	private void Awake()
	{
		lastScreenSize = GraphicsUtil.GetScreenSize();
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateCamera, CheckForCameraChanges);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdateCamera, CheckForCameraChanges);
	}

	public void CheckForCameraChanges()
	{
		Camera camera = uiCamera;
		Camera uICamera = GetUICamera();
		if (uICamera == null)
		{
			return;
		}
		Vector2Int screenSize = GraphicsUtil.GetScreenSize();
		float fieldOfView = uICamera.fieldOfView;
		if (!(camera != uICamera) && !(lastScreenSize != screenSize) && Mathf.Approximately(lastFieldOfView, fieldOfView))
		{
			return;
		}
		lastScreenSize = screenSize;
		lastFieldOfView = fieldOfView;
		for (int num = uiCameraChangeListeners.Count - 1; num >= 0; num--)
		{
			OnUICameraChange onUICameraChange = uiCameraChangeListeners[num];
			if (onUICameraChange != null)
			{
				try
				{
					onUICameraChange(uICamera);
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.ToString());
				}
			}
			else
			{
				uiCameraChangeListeners.RemoveAt(num);
			}
		}
	}

	public static void AddUICameraChangeListener(OnUICameraChange listener)
	{
		if (listener != null && !main.uiCameraChangeListeners.Contains(listener))
		{
			main.uiCameraChangeListeners.Add(listener);
		}
	}

	public static void RemoveUICameraChangeListener(OnUICameraChange listener)
	{
		if (listener != null)
		{
			main.uiCameraChangeListeners.Remove(listener);
		}
	}

	public static Camera GetUICamera()
	{
		int num = 1 << LayerID.UI;
		if (uiCamera == null || (uiCamera.cullingMask & num) == 0)
		{
			uiCamera = null;
			int allCamerasCount = Camera.allCamerasCount;
			if (allCameras.Length < allCamerasCount)
			{
				allCameras = new Camera[allCamerasCount];
			}
			int num2 = Camera.GetAllCameras(allCameras);
			for (int i = 0; i < num2; i++)
			{
				Camera camera = allCameras[i];
				if ((camera.cullingMask & num) != 0)
				{
					uiCamera = camera;
					if (XRSettings.enabled && uGUI.isMainLevel)
					{
						XRDevice.DisableAutoXRCameraTracking(uiCamera, disabled: true);
						Transform obj = uiCamera.transform;
						obj.localPosition = Vector3.zero;
						obj.localRotation = Quaternion.identity;
					}
					break;
				}
			}
			for (int j = 0; j < num2; j++)
			{
				allCameras[j] = null;
			}
		}
		return uiCamera;
	}
}
