using UnityEngine;

public class BirdFlapSound : MonoBehaviour
{
	public FMOD_StudioEventEmitter flapSound;

	public void OnFlightAnimationStarted()
	{
		if (flapSound != null)
		{
			Utils.PlayEnvSound(flapSound, base.transform.position);
		}
	}
}
