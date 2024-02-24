using UnityEngine;

public class VFXPlayOnVelocity : MonoBehaviour
{
	[AssertNotNull]
	public Rigidbody rb;

	[AssertNotNull]
	public VFXController fxControl;

	public float speedThreshold;

	public float currentSpeed;

	private bool isPlaying = true;

	private void Update()
	{
		currentSpeed = rb.velocity.magnitude;
		if (rb.isKinematic)
		{
			if (isPlaying)
			{
				fxControl.Stop();
				isPlaying = false;
			}
		}
		else if (currentSpeed > speedThreshold)
		{
			if (!isPlaying)
			{
				fxControl.Play();
				isPlaying = true;
			}
		}
		else if (isPlaying)
		{
			fxControl.Stop();
			isPlaying = false;
		}
	}
}
