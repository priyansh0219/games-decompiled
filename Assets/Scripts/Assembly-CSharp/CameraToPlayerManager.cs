using UnityEngine;
using UnityEngine.XR;

public class CameraToPlayerManager : MonoBehaviour
{
	[AssertNotNull]
	public Transform headCameraBone;

	private bool isEnabled;

	public void EnableHeadCameraController()
	{
		if (!XRSettings.enabled)
		{
			MainCameraControl.main.enabled = false;
			isEnabled = true;
		}
	}

	public void DisableHeadCameraController()
	{
		MainCameraControl.main.enabled = true;
		isEnabled = false;
	}

	private void Update()
	{
		if (isEnabled)
		{
			MainCameraControl.main.transform.position = headCameraBone.position;
			MainCameraControl.main.transform.rotation = headCameraBone.rotation;
		}
	}
}
