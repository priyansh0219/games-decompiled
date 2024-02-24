using UnityEngine;

public class RocketPreflightSoundFX : MonoBehaviour
{
	public PreflightCheck preflightCheck;

	[AssertNotNull]
	public FMOD_CustomEmitter preflightSFX;

	public void PlayPreflightSound(PreflightCheck tryPlayPreflightCheck)
	{
		if (tryPlayPreflightCheck == preflightCheck)
		{
			preflightSFX.Play();
		}
	}
}
