using System.Text;
using Gendarme;
using UnityEngine;
using UnityEngine.XR;

[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
public class SNCameraRoot : MonoBehaviour, ICompileTimeCheckable
{
	public static SNCameraRoot main;

	[Tooltip("Minimum near clip distance allowed (shadows are flickering below 0.3)")]
	public float minNearClip = 0.2f;

	[AssertNotNull]
	[SerializeField]
	private Camera mainCamera;

	[AssertNotNull]
	[SerializeField]
	private Camera guiCamera;

	[AssertNotNull]
	[SerializeField]
	public Camera imguiCamera;

	private const float interpupillaryDistanceThreshold = 1E-05f;

	private float stereoSeparation = float.MinValue;

	private Matrix4x4 matrixLeftEye = Matrix4x4.identity;

	private Matrix4x4 matrixRightEye = Matrix4x4.identity;

	public Camera mainCam => mainCamera;

	public Camera guiCam => guiCamera;

	public float CurrentFieldOfView { get; private set; }

	public Transform GetForwardTransform()
	{
		return base.transform;
	}

	public Transform GetAimingTransform()
	{
		return mainCamera.transform;
	}

	public void SonarPing()
	{
		mainCamera.GetComponent<SonarScreenFX>().Ping();
	}

	private void Awake()
	{
		main = this;
		guiCamera.nearClipPlane = mainCamera.nearClipPlane;
		guiCamera.farClipPlane = mainCamera.farClipPlane;
		guiCamera.fieldOfView = mainCamera.fieldOfView;
		imguiCamera.nearClipPlane = mainCamera.nearClipPlane;
		imguiCamera.farClipPlane = mainCamera.farClipPlane;
		imguiCamera.fieldOfView = mainCamera.fieldOfView;
	}

	private void Start()
	{
		Transform obj = guiCamera.transform;
		obj.SetParent(null, worldPositionStays: false);
		obj.localPosition = new Vector3(0f, 0f, 0f);
		obj.localRotation = Quaternion.identity;
		if (XRSettings.enabled)
		{
			XRDevice.DisableAutoXRCameraTracking(imguiCamera, disabled: true);
		}
		Transform obj2 = imguiCamera.transform;
		obj2.SetParent(null, worldPositionStays: false);
		obj2.localPosition = Vector3.zero;
		obj2.localRotation = Quaternion.identity;
		DevConsole.RegisterConsoleCommand(this, "fov");
		DevConsole.RegisterConsoleCommand(this, "farplane");
		DevConsole.RegisterConsoleCommand(this, "nearplane");
		SyncFieldOfView();
	}

	private void Update()
	{
		UpdateVR();
	}

	private void OnDestroy()
	{
		main = null;
	}

	private void UpdateVR()
	{
		if (XRSettings.enabled)
		{
			float num = mainCamera.stereoSeparation;
			if (!(Mathf.Abs(stereoSeparation - num) < 1E-05f))
			{
				stereoSeparation = num;
				matrixLeftEye.m03 = stereoSeparation * 0.5f;
				matrixLeftEye.m22 = -1f;
				matrixRightEye.m03 = (0f - stereoSeparation) * 0.5f;
				matrixRightEye.m22 = -1f;
				guiCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Left, matrixLeftEye);
				guiCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Right, matrixRightEye);
				imguiCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Left, matrixLeftEye);
				imguiCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Right, matrixRightEye);
			}
		}
	}

	public void SetFov(float fov)
	{
		SyncFieldOfView(fov);
	}

	public void SyncFieldOfView()
	{
		SyncFieldOfView(MiscSettings.fieldOfView);
	}

	public void SetNearClip(float value)
	{
		float nearClipPlane = Mathf.Clamp(value, minNearClip, mainCamera.farClipPlane - minNearClip);
		mainCamera.nearClipPlane = nearClipPlane;
		guiCamera.nearClipPlane = nearClipPlane;
		imguiCamera.nearClipPlane = nearClipPlane;
	}

	private void SyncFieldOfView(float fieldOfView)
	{
		if (mainCamera != null)
		{
			mainCamera.fieldOfView = fieldOfView;
		}
		if (guiCamera != null)
		{
			guiCamera.fieldOfView = fieldOfView;
		}
		if (imguiCamera != null)
		{
			imguiCamera.fieldOfView = fieldOfView;
		}
		CurrentFieldOfView = fieldOfView;
	}

	public void SetFarPlaneDistance(float dist)
	{
		mainCamera.farClipPlane = dist;
		guiCamera.farClipPlane = dist;
		imguiCamera.farClipPlane = dist;
	}

	public void OnConsoleCommand_farplane(NotificationCenter.Notification n)
	{
		if (DevConsole.ParseFloat(n, 0, out var value))
		{
			SetFarPlaneDistance(value);
		}
		else
		{
			ErrorMessage.AddError("Invalid distance argument");
		}
	}

	public void OnConsoleCommand_nearplane(NotificationCenter.Notification n)
	{
		if (DevConsole.ParseFloat(n, 0, out var value))
		{
			SetNearClip(value);
		}
		else
		{
			ErrorMessage.AddError("Invalid distance argument");
		}
	}

	public string CompileTimeCheck()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int cullingMask = mainCamera.cullingMask;
		int cullingMask2 = guiCamera.cullingMask;
		int cullingMask3 = imguiCamera.cullingMask;
		for (int i = 0; i < 32; i++)
		{
			bool num = (cullingMask & (1 << i)) != 0;
			bool flag = (cullingMask2 & (1 << i)) != 0;
			bool flag2 = (cullingMask3 & (1 << i)) != 0;
			if (num && (flag || flag2))
			{
				string arg = LayerMask.LayerToName(i);
				stringBuilder.AppendFormat("Both Main and GUI Cameras are setup to render the {0} (\"{1}\") layer. That means all objects with this Layer will be rendered twice. \n", i, arg);
			}
			if (flag && flag2)
			{
				string arg2 = LayerMask.LayerToName(i);
				stringBuilder.AppendFormat("Both GUI Cameras are setup to render the {0} (\"{1}\") layer. That means all objects with this Layer will be rendered twice. \n", i, arg2);
			}
		}
		if (stringBuilder.Length <= 0)
		{
			return null;
		}
		return stringBuilder.ToString();
	}
}
