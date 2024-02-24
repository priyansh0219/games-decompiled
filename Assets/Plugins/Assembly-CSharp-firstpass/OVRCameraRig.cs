using System;
using Gendarme;
using UnityEngine;
using UnityEngine.XR;

[ExecuteInEditMode]
public class OVRCameraRig : MonoBehaviour
{
	public bool usePerEyeCameras;

	private readonly string trackingSpaceName = "TrackingSpace";

	private readonly string trackerAnchorName = "TrackerAnchor";

	private readonly string eyeAnchorName = "EyeAnchor";

	private readonly string handAnchorName = "HandAnchor";

	private readonly string legacyEyeAnchorName = "Camera";

	private Camera _centerEyeCamera;

	private Camera _leftEyeCamera;

	private Camera _rightEyeCamera;

	public Camera leftEyeCamera
	{
		get
		{
			if (!usePerEyeCameras)
			{
				return _centerEyeCamera;
			}
			return _leftEyeCamera;
		}
	}

	public Camera rightEyeCamera
	{
		get
		{
			if (!usePerEyeCameras)
			{
				return _centerEyeCamera;
			}
			return _rightEyeCamera;
		}
	}

	public Transform trackingSpace { get; private set; }

	public Transform leftEyeAnchor { get; private set; }

	public Transform centerEyeAnchor { get; private set; }

	public Transform rightEyeAnchor { get; private set; }

	public Transform leftHandAnchor { get; private set; }

	public Transform rightHandAnchor { get; private set; }

	public Transform trackerAnchor { get; private set; }

	public event Action<OVRCameraRig> UpdatedAnchors;

	private void Awake()
	{
		EnsureGameObjectIntegrity();
	}

	private void Start()
	{
		EnsureGameObjectIntegrity();
		if (Application.isPlaying)
		{
			UpdateAnchors();
		}
	}

	private void Update()
	{
		EnsureGameObjectIntegrity();
		if (Application.isPlaying)
		{
			UpdateAnchors();
		}
	}

	private void UpdateAnchors()
	{
		bool monoscopic = OVRManager.instance.monoscopic;
		OVRPose pose = OVRManager.tracker.GetPose();
		trackerAnchor.localRotation = pose.orientation;
		centerEyeAnchor.localRotation = InputTracking.GetLocalRotation(XRNode.CenterEye);
		leftEyeAnchor.localRotation = (monoscopic ? centerEyeAnchor.localRotation : InputTracking.GetLocalRotation(XRNode.LeftEye));
		rightEyeAnchor.localRotation = (monoscopic ? centerEyeAnchor.localRotation : InputTracking.GetLocalRotation(XRNode.RightEye));
		leftHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
		rightHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
		trackerAnchor.localPosition = pose.position;
		centerEyeAnchor.localPosition = InputTracking.GetLocalPosition(XRNode.CenterEye);
		leftEyeAnchor.localPosition = (monoscopic ? centerEyeAnchor.localPosition : InputTracking.GetLocalPosition(XRNode.LeftEye));
		rightEyeAnchor.localPosition = (monoscopic ? centerEyeAnchor.localPosition : InputTracking.GetLocalPosition(XRNode.RightEye));
		leftHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
		rightHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
		if (this.UpdatedAnchors != null)
		{
			this.UpdatedAnchors(this);
		}
	}

	public void EnsureGameObjectIntegrity()
	{
		if (trackingSpace == null)
		{
			trackingSpace = ConfigureRootAnchor(trackingSpaceName);
		}
		if (leftEyeAnchor == null)
		{
			leftEyeAnchor = ConfigureEyeAnchor(trackingSpace, XRNode.LeftEye);
		}
		if (centerEyeAnchor == null)
		{
			centerEyeAnchor = ConfigureEyeAnchor(trackingSpace, XRNode.CenterEye);
		}
		if (rightEyeAnchor == null)
		{
			rightEyeAnchor = ConfigureEyeAnchor(trackingSpace, XRNode.RightEye);
		}
		if (leftHandAnchor == null)
		{
			leftHandAnchor = ConfigureHandAnchor(trackingSpace, OVRPlugin.Node.HandLeft);
		}
		if (rightHandAnchor == null)
		{
			rightHandAnchor = ConfigureHandAnchor(trackingSpace, OVRPlugin.Node.HandRight);
		}
		if (trackerAnchor == null)
		{
			trackerAnchor = ConfigureTrackerAnchor(trackingSpace);
		}
		if (_centerEyeCamera == null || _leftEyeCamera == null || _rightEyeCamera == null)
		{
			_centerEyeCamera = centerEyeAnchor.GetComponent<Camera>();
			_leftEyeCamera = leftEyeAnchor.GetComponent<Camera>();
			_rightEyeCamera = rightEyeAnchor.GetComponent<Camera>();
			if (_centerEyeCamera == null)
			{
				_centerEyeCamera = centerEyeAnchor.gameObject.AddComponent<Camera>();
				_centerEyeCamera.tag = "MainCamera";
			}
			if (_leftEyeCamera == null)
			{
				_leftEyeCamera = leftEyeAnchor.gameObject.AddComponent<Camera>();
				_leftEyeCamera.tag = "MainCamera";
			}
			if (_rightEyeCamera == null)
			{
				_rightEyeCamera = rightEyeAnchor.gameObject.AddComponent<Camera>();
				_rightEyeCamera.tag = "MainCamera";
			}
			_centerEyeCamera.stereoTargetEye = StereoTargetEyeMask.Both;
			_leftEyeCamera.stereoTargetEye = StereoTargetEyeMask.Left;
			_rightEyeCamera.stereoTargetEye = StereoTargetEyeMask.Right;
		}
		_centerEyeCamera.enabled = !usePerEyeCameras;
		_leftEyeCamera.enabled = usePerEyeCameras;
		_rightEyeCamera.enabled = usePerEyeCameras;
	}

	private Transform ConfigureRootAnchor(string name)
	{
		Transform transform = base.transform.Find(name);
		if (transform == null)
		{
			transform = new GameObject(name).transform;
		}
		transform.parent = base.transform;
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		return transform;
	}

	[SuppressMessage("Subnautica.Rules", "AvoidStringConcatenation")]
	private Transform ConfigureEyeAnchor(Transform root, XRNode eye)
	{
		object obj;
		switch (eye)
		{
		default:
			obj = "Right";
			break;
		case XRNode.LeftEye:
			obj = "Left";
			break;
		case XRNode.CenterEye:
			obj = "Center";
			break;
		}
		string text = (string)obj + eyeAnchorName;
		Transform transform = base.transform.Find(root.name + "/" + text);
		if (transform == null)
		{
			transform = base.transform.Find(text);
		}
		if (transform == null)
		{
			string n = legacyEyeAnchorName + eye;
			transform = base.transform.Find(n);
		}
		if (transform == null)
		{
			transform = new GameObject(text).transform;
		}
		transform.name = text;
		transform.parent = root;
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		return transform;
	}

	private Transform ConfigureHandAnchor(Transform root, OVRPlugin.Node hand)
	{
		string text = ((hand == OVRPlugin.Node.HandLeft) ? "Left" : "Right") + handAnchorName;
		Transform transform = base.transform.Find(root.name + "/" + text);
		if (transform == null)
		{
			transform = base.transform.Find(text);
		}
		if (transform == null)
		{
			transform = new GameObject(text).transform;
		}
		transform.name = text;
		transform.parent = root;
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		return transform;
	}

	private Transform ConfigureTrackerAnchor(Transform root)
	{
		string text = trackerAnchorName;
		Transform transform = base.transform.Find(root.name + "/" + text);
		if (transform == null)
		{
			transform = new GameObject(text).transform;
		}
		transform.parent = root;
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		return transform;
	}
}
