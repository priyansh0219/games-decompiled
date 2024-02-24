using UnityEngine.XR;

public class VROptions
{
	public static bool disableInputPitch = true;

	public static bool aimRightArmWithHead = true;

	public static bool gazeBasedCursor = false;

	public static bool enableCinematics = false;

	public static bool skipIntro = false;

	public static float groundMoveScale = 0.6f;

	public static bool GetUseGazeBasedCursor()
	{
		if (gazeBasedCursor)
		{
			return XRSettings.enabled;
		}
		return false;
	}
}
