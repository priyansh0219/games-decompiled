using UnityEngine;

public class RocketPDANotification : MonoBehaviour
{
	public PreflightCheck preflightCheck;

	[AssertNotNull]
	public PDANotification pdaNotification;

	public void PlayPDANotification(PreflightCheck tryPlayPreflightCheck)
	{
		if (tryPlayPreflightCheck == preflightCheck)
		{
			pdaNotification.Play();
		}
	}
}
