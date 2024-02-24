using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_PlayerSleep : MonoBehaviour
{
	private enum State
	{
		Disabled = 0,
		FadeIn = 1,
		Enabled = 2,
		FadeOut = 3
	}

	public static uGUI_PlayerSleep main;

	public Image blackOverlay;

	public TextMeshProUGUI text;

	private State state;

	public float fadeInSpeed = 5f;

	public float fadeOutSpeed = 2f;

	private void Start()
	{
		main = this;
		blackOverlay.color = new Color(0f, 0f, 0f, 0f);
	}

	public void StartSleepScreen()
	{
		if (state != State.FadeIn && state != State.Enabled)
		{
			BeginFadeIn();
		}
	}

	public void StopSleepScreen()
	{
		if (state != 0 && state != State.FadeOut)
		{
			BeginFadeOut();
		}
	}

	private void BeginFadeIn()
	{
		state = State.FadeIn;
		if ((bool)FPSInputModule.current)
		{
			FPSInputModule.current.lockPauseMenu = true;
		}
		blackOverlay.color = new Color(0f, 0f, 0f, 0f);
	}

	private void BeginFadeOut()
	{
		state = State.FadeOut;
		if ((bool)FPSInputModule.current)
		{
			FPSInputModule.current.lockPauseMenu = false;
		}
	}

	private void Update()
	{
		if (state == State.FadeIn)
		{
			Color b = new Color(0f, 0f, 0f, 1f);
			Color color = blackOverlay.color;
			blackOverlay.color = Color.Lerp(color, b, Time.deltaTime * fadeInSpeed);
			blackOverlay.enabled = true;
			if (color.a > 0.98f)
			{
				blackOverlay.color = new Color(0f, 0f, 0f, 1f);
				state = State.Enabled;
			}
		}
		else if (state == State.FadeOut)
		{
			Color b2 = new Color(0f, 0f, 0f, 0f);
			Color color2 = blackOverlay.color;
			blackOverlay.color = Color.Lerp(color2, b2, Time.deltaTime * fadeOutSpeed);
			if (color2.a < 0.02f)
			{
				blackOverlay.color = new Color(0f, 0f, 0f, 0f);
				blackOverlay.enabled = false;
				state = State.Disabled;
			}
		}
	}
}
