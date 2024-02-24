using UnityEngine;

public class PreflightCheckSwitch : MonoBehaviour
{
	public RocketPreflightCheckManager preflightCheckManager;

	public PreflightCheck preflightCheck;

	public void CompletePreflightCheck()
	{
		if ((bool)preflightCheckManager)
		{
			preflightCheckManager.CompletePreflightCheck(preflightCheck);
		}
	}
}
