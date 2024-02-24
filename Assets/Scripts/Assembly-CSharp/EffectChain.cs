using UnityEngine;

public class EffectChain : MonoBehaviour
{
	public float scaleDelay;

	public Vector3 scaleAmount = new Vector3(0f, 0f, 0f);

	public float scaleTime;

	public AudioClip soundClip;

	public float soundVolume = 1f;

	public EffectChain playOnComplete;

	public void TriggerEffect()
	{
		if (scaleAmount.magnitude > 0.01f)
		{
			iTween.PunchScale(base.gameObject, iTween.Hash("amount", scaleAmount, "time", scaleTime, "delay", scaleDelay, "easetype", iTween.EaseType.easeInOutCubic, "oncomplete", "OnAnimComplete", "oncompletetarget", base.gameObject, "oncompleteparams", this));
		}
		if ((bool)soundClip)
		{
			AudioSource.PlayClipAtPoint(soundClip, base.gameObject.transform.position, soundVolume);
		}
	}

	private void OnAnimComplete(EffectChain source)
	{
		if ((bool)source.playOnComplete)
		{
			source.playOnComplete.TriggerEffect();
		}
	}
}
