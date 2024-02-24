using System.Collections.Generic;
using UnityEngine;

public class VoiceNotificationManager : MonoBehaviour
{
	[AssertNotNull]
	public SubRoot subRoot;

	public float voSpacing = 5f;

	private List<VoiceNotification> voQueue = new List<VoiceNotification>();

	private bool ready = true;

	public void ClearQueue()
	{
		voQueue.Clear();
	}

	public void PlayVoiceNotification(VoiceNotification vo, bool addToQueue = true, bool forcePlay = false)
	{
		if (vo == null || Player.main.currentSub != subRoot || !vo.GetCanPlay())
		{
			return;
		}
		if (forcePlay)
		{
			vo.Play();
		}
		else if (addToQueue || ready)
		{
			if (addToQueue && !ready && !voQueue.Contains(vo))
			{
				voQueue.Add(vo);
			}
			else if (ready && voQueue.Count == 0)
			{
				PlayVO(vo);
			}
		}
	}

	private void PlayVO(VoiceNotification vo)
	{
		vo.Play();
		ready = false;
		Invoke("TryPlayNext", voSpacing);
	}

	private void TryPlayNext()
	{
		if (Player.main.currentSub != subRoot)
		{
			voQueue.Clear();
			return;
		}
		if (voQueue.Count == 0)
		{
			ready = true;
			return;
		}
		voQueue[0].Play();
		voQueue.RemoveAt(0);
		if (voQueue.Count > 0)
		{
			Invoke("TryPlayNext", voSpacing);
		}
		else
		{
			ready = true;
		}
	}
}
