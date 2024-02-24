using UnityEngine;

public class CyclopsEntryHatch : MonoBehaviour
{
	public Transform leftHatchTransform;

	public Transform rightHatchTransform;

	public Transform leftHatchGoToTransform;

	public Transform rightHatchGoToTransform;

	[AssertNotNull]
	public FMOD_CustomEmitter openSFX;

	[AssertNotNull]
	public FMOD_CustomEmitter closeSFX;

	public float hatchSpeed = 3f;

	private Quaternion leftClosedQuat;

	private Quaternion rightClosedQuat;

	private bool hatchOpen;

	private void Start()
	{
		leftClosedQuat = leftHatchTransform.localRotation;
		rightClosedQuat = rightHatchTransform.localRotation;
		CloseHatch(warpClosed: true);
	}

	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.Equals(Player.main.gameObject) && !hatchOpen)
		{
			if (Player.main.IsUnderwater())
			{
				openSFX.Play();
			}
			hatchOpen = true;
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (col.gameObject.Equals(Player.main.gameObject) && hatchOpen)
		{
			if (Player.main.IsUnderwater())
			{
				closeSFX.Play();
			}
			hatchOpen = false;
		}
	}

	private void Update()
	{
		if (hatchOpen)
		{
			OpenHatch();
		}
		else
		{
			CloseHatch();
		}
	}

	private void OpenHatch()
	{
		leftHatchTransform.localRotation = Quaternion.Slerp(leftHatchTransform.localRotation, leftHatchGoToTransform.localRotation, Time.deltaTime * hatchSpeed);
		rightHatchTransform.localRotation = Quaternion.Slerp(rightHatchTransform.localRotation, rightHatchGoToTransform.localRotation, Time.deltaTime * hatchSpeed);
	}

	private void CloseHatch(bool warpClosed = false)
	{
		if (warpClosed)
		{
			leftHatchTransform.localRotation = leftClosedQuat;
			rightHatchTransform.localRotation = rightClosedQuat;
		}
		else
		{
			leftHatchTransform.localRotation = Quaternion.Slerp(leftHatchTransform.localRotation, leftClosedQuat, Time.deltaTime * hatchSpeed);
			rightHatchTransform.localRotation = Quaternion.Slerp(rightHatchTransform.localRotation, rightClosedQuat, Time.deltaTime * hatchSpeed);
		}
	}
}
