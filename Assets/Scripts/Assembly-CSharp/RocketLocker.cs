using UnityEngine;

public class RocketLocker : MonoBehaviour
{
	[AssertNotNull]
	public GameObject rotateTarget;

	[AssertNotNull]
	public Transform rotateTo;

	public FMOD_CustomEmitter doorOpenSFX;

	public FMOD_CustomEmitter doorCloseSFX;

	public BoxCollider collider;

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
		if (openState && doorOpenSFX != null)
		{
			if ((bool)collider)
			{
				collider.enabled = false;
			}
			doorOpenSFX.Play();
		}
		else if (!openState && doorCloseSFX != null)
		{
			Invoke("EnableCollider", 1f);
			doorCloseSFX.Play();
		}
	}

	private void EnableCollider()
	{
		if ((bool)collider)
		{
			collider.enabled = true;
		}
	}

	private void Update()
	{
		if (openState)
		{
			rotateTarget.transform.localRotation = Quaternion.Slerp(rotateTarget.transform.localRotation, openRotation, Time.deltaTime * 2f);
		}
		else
		{
			rotateTarget.transform.localRotation = Quaternion.Slerp(rotateTarget.transform.localRotation, closedRotation, Time.deltaTime * 5f);
		}
	}
}
