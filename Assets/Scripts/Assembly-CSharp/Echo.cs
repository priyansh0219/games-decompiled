using UnityEngine;

public class Echo : MonoBehaviour
{
	public FMOD_StudioEventEmitter lastHeardClip;

	public int timesToPlay;

	public float timeLastListen;

	private void PlaySound()
	{
		timesToPlay--;
		Utils.PlayEnvSound(lastHeardClip, base.transform.position, 10f);
		if (timesToPlay > 0)
		{
			Invoke("PlaySound", Random.Range(lastHeardClip.GetLength() * 2f, lastHeardClip.GetLength() * 10f));
		}
	}
}
