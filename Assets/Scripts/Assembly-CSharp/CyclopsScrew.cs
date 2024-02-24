using UnityEngine;

public class CyclopsScrew : MonoBehaviour, ISubThrottleHandler
{
	public float turnSpeedMax = 19f;

	public float turnDampening = 5f;

	[AssertNotNull]
	public VFXController fxControl;

	private bool isPlaying;

	private float turnSpeed;

	public void OnSubAppliedThrottle()
	{
		turnSpeed = turnSpeedMax;
	}

	private void Update()
	{
		turnSpeed = Mathf.MoveTowards(turnSpeed, 0f, Time.deltaTime * turnDampening);
		base.transform.Rotate(new Vector3(0f, 0f - turnSpeed, 0f), Space.Self);
		if (turnSpeed < 1f)
		{
			if (isPlaying)
			{
				fxControl.Stop();
				isPlaying = false;
			}
		}
		else if (!isPlaying)
		{
			fxControl.Play();
			isPlaying = true;
		}
	}
}
