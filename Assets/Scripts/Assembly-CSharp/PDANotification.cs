using UnityEngine;

public class PDANotification : MonoBehaviour
{
	public string text;

	public FMODAsset sound;

	public void Play()
	{
		Play(null);
	}

	public void Play(params object[] args)
	{
		if (!string.IsNullOrEmpty(text))
		{
			Subtitles.Add(text, args);
		}
		if ((bool)sound)
		{
			PDASounds.queue.PlayQueued(sound);
		}
	}
}
