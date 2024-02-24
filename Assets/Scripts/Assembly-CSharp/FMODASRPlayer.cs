using UnityEngine;

public class FMODASRPlayer : MonoBehaviour
{
	public FMOD_StudioEventEmitter startLoopSound;

	public FMOD_StudioEventEmitter stopSound;

	public bool debug;

	private bool playingSounds;

	private float timePlayedSounds;

	public void Awake()
	{
	}

	public void Play()
	{
		if (!playingSounds)
		{
			if (debug)
			{
				Debug.Log("FMODASRPlayer." + startLoopSound.gameObject.name + ".Play()");
			}
			startLoopSound.PlayUI();
			playingSounds = true;
			timePlayedSounds = Time.time;
		}
	}

	public void Stop()
	{
		if (!playingSounds)
		{
			return;
		}
		if (debug)
		{
			Debug.Log("FMODASRPlayer." + startLoopSound.gameObject.name + ".Stop()");
		}
		startLoopSound.Stop();
		if (stopSound != null && Time.time > timePlayedSounds + 2f)
		{
			if (debug)
			{
				Debug.Log("FMODASRPlayer." + stopSound.gameObject.name + ".Play() - playing stop sound because startLoop sound playing for a bit");
			}
			Utils.PlayEnvSound(stopSound);
		}
		playingSounds = false;
	}
}
