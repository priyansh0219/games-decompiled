using UnityEngine;

public class VoiceNotification : MonoBehaviour
{
	[AssertNotNull]
	[AssertLocalization]
	public string text;

	[AssertNotNull]
	public FMODAsset sound;

	public float minInterval;

	private float timeNextPlay = -1f;

	public void Play()
	{
		Play(null);
	}

	public bool GetCanPlay()
	{
		if (DayNightCycle.main.timePassedAsFloat < timeNextPlay)
		{
			return false;
		}
		return true;
	}

	public bool Play(params object[] args)
	{
		float timePassedAsFloat = DayNightCycle.main.timePassedAsFloat;
		if (timePassedAsFloat < timeNextPlay)
		{
			return false;
		}
		timeNextPlay = timePassedAsFloat + minInterval;
		if (!string.IsNullOrEmpty(text))
		{
			Subtitles.Add(text, args);
		}
		if ((bool)sound)
		{
			FMODUWE.PlayOneShot(sound, base.transform.position);
		}
		return true;
	}
}
