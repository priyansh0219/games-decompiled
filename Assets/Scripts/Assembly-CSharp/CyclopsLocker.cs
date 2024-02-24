using UnityEngine;

public class CyclopsLocker : MonoBehaviour
{
	public GameObject rotateTarget;

	public Transform rotateTo;

	public FMODAsset openSound;

	public FMODAsset closeSound;

	private Quaternion closedRotation;

	private Quaternion openRotation;

	private bool openState;

	private void Start()
	{
		closedRotation = rotateTarget.transform.localRotation;
		openRotation = rotateTo.transform.localRotation;
	}

	public void ToggleDoor()
	{
		openState = !openState;
		if (openState && openSound != null)
		{
			Utils.PlayFMODAsset(openSound, base.transform, 0f);
		}
		else if (!openState && closeSound != null)
		{
			Utils.PlayFMODAsset(closeSound, base.transform, 0f);
		}
	}

	private void Update()
	{
		if (openState)
		{
			rotateTarget.transform.localRotation = Quaternion.Slerp(rotateTarget.transform.localRotation, openRotation, Time.deltaTime * 5f);
		}
		else
		{
			rotateTarget.transform.localRotation = Quaternion.Slerp(rotateTarget.transform.localRotation, closedRotation, Time.deltaTime * 5f);
		}
	}
}
