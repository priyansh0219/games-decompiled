using System;
using System.Collections;
using UnityEngine;

public class uGUI_Fader : MonoBehaviour
{
	[AssertNotNull]
	public CanvasGroup canvasGroup;

	public float fadeInTime = 1f;

	public float fadeOutTime = 0.5f;

	public bool useUnscaledTime;

	public bool initialState;

	private Sequence sequence = new Sequence();

	private void Awake()
	{
		SetState(initialState);
	}

	private void Update()
	{
		ApplyState();
	}

	private void ApplyState()
	{
		if (sequence.active)
		{
			sequence.Update(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
			canvasGroup.alpha = 0.5f * (1f - Mathf.Cos((float)Math.PI * sequence.t));
		}
	}

	public void SetState(bool enabled)
	{
		sequence.ForceState(enabled);
	}

	public void FadeOut(float duration, SequenceCallback callback)
	{
		sequence.Set(duration, target: false, callback);
	}

	public void FadeIn(float duration, SequenceCallback callback)
	{
		sequence.Set(duration, target: true, callback);
	}

	public void FadeOut(SequenceCallback callback = null)
	{
		FadeOut(fadeInTime, callback);
	}

	public void FadeIn(SequenceCallback callback = null)
	{
		FadeIn(fadeOutTime, callback);
	}

	public void DelayedFadeIn(float delay, SequenceCallback callback = null)
	{
		StopAllCoroutines();
		StartCoroutine(FadeWait(delay, callback));
	}

	private IEnumerator FadeWait(float delay, SequenceCallback callback)
	{
		if (useUnscaledTime)
		{
			yield return new WaitForSecondsRealtime(delay);
		}
		else
		{
			yield return new WaitForSeconds(delay);
		}
		FadeIn(callback);
	}
}
