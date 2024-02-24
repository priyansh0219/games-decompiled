using UnityEngine;
using UnityEngine.XR;

public class OVRDebugHeadController : MonoBehaviour
{
	[SerializeField]
	public bool AllowPitchLook;

	[SerializeField]
	public bool AllowYawLook = true;

	[SerializeField]
	public bool InvertPitch;

	[SerializeField]
	public float GamePad_PitchDegreesPerSec = 90f;

	[SerializeField]
	public float GamePad_YawDegreesPerSec = 90f;

	[SerializeField]
	public bool AllowMovement;

	[SerializeField]
	public float ForwardSpeed = 2f;

	[SerializeField]
	public float StrafeSpeed = 2f;

	protected OVRCameraRig CameraRig;

	private void Awake()
	{
		OVRCameraRig[] componentsInChildren = base.gameObject.GetComponentsInChildren<OVRCameraRig>();
		if (componentsInChildren.Length == 0)
		{
			Debug.LogWarning("OVRCamParent: No OVRCameraRig attached.");
		}
		else if (componentsInChildren.Length > 1)
		{
			Debug.LogWarning("OVRCamParent: More then 1 OVRCameraRig attached.");
		}
		else
		{
			CameraRig = componentsInChildren[0];
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (AllowMovement)
		{
			float y = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).y;
			float x = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).x;
			Vector3 vector = CameraRig.centerEyeAnchor.rotation * Vector3.forward * y * Time.deltaTime * ForwardSpeed;
			Vector3 vector2 = CameraRig.centerEyeAnchor.rotation * Vector3.right * x * Time.deltaTime * StrafeSpeed;
			base.transform.position += vector + vector2;
		}
		if (XRDevice.isPresent || (!AllowYawLook && !AllowPitchLook))
		{
			return;
		}
		Quaternion quaternion = base.transform.rotation;
		if (AllowYawLook)
		{
			quaternion = Quaternion.AngleAxis(OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x * Time.deltaTime * GamePad_YawDegreesPerSec, Vector3.up) * quaternion;
		}
		if (AllowPitchLook)
		{
			float num = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y;
			if (Mathf.Abs(num) > 0.0001f)
			{
				if (InvertPitch)
				{
					num *= -1f;
				}
				Quaternion quaternion2 = Quaternion.AngleAxis(num * Time.deltaTime * GamePad_PitchDegreesPerSec, Vector3.left);
				quaternion *= quaternion2;
			}
		}
		base.transform.rotation = quaternion;
	}
}
