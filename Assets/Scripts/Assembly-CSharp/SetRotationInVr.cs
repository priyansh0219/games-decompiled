using UnityEngine;
using UnityEngine.XR;

public class SetRotationInVr : MonoBehaviour
{
	public Vector3 vrEulerAngles = Vector3.zero;

	private Vector3 initEulerAngles;

	private bool setupForVr;

	private void Start()
	{
		initEulerAngles = base.transform.localEulerAngles;
	}

	private void Update()
	{
		if (XRSettings.enabled != setupForVr)
		{
			if (XRSettings.enabled)
			{
				base.transform.localEulerAngles = vrEulerAngles;
			}
			else
			{
				base.transform.localEulerAngles = initEulerAngles;
			}
			setupForVr = XRSettings.enabled;
		}
	}
}
