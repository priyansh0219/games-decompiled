using UnityEngine;
using UnityEngine.XR;

public class VROptionsMenu : MonoBehaviour
{
	private void Update()
	{
		if (!XRSettings.enabled)
		{
			Debug.Log("Hiding VR menu");
			base.gameObject.SetActive(value: false);
		}
	}

	public void VR_SetDisableMousePitch(bool value)
	{
		VROptions.disableInputPitch = value;
	}

	public void VR_SetAimRightArmWithHead(bool value)
	{
		VROptions.aimRightArmWithHead = value;
	}
}
