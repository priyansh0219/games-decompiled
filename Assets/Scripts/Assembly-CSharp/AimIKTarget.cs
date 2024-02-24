using UnityEngine;
using UnityEngine.XR;

public class AimIKTarget : MonoBehaviour
{
	private Vector3 origLocalPos;

	private void Awake()
	{
		origLocalPos = base.transform.localPosition;
	}

	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (XRSettings.enabled && VROptions.aimRightArmWithHead)
		{
			Transform aimingTransform = SNCameraRoot.main.GetAimingTransform();
			base.transform.position = aimingTransform.position + aimingTransform.forward * 5f;
		}
		else
		{
			base.transform.localPosition = origLocalPos;
		}
	}
}
