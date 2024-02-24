using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_PlayerDeath : MonoBehaviour
{
	public enum DeathTypes
	{
		FadeToBlack = 0,
		CutToBlack = 1
	}

	public static uGUI_PlayerDeath main;

	public Image blackOverlay;

	public TextMeshProUGUI text;

	private bool active;

	private bool fadeIn;

	private void Start()
	{
		main = this;
		blackOverlay.color = new Color(0f, 0f, 0f, 0f);
		if ((bool)FPSInputModule.current)
		{
			FPSInputModule.current.lockPauseMenu = false;
		}
	}

	public void TriggerDeathVignette(DeathTypes deathType = DeathTypes.FadeToBlack)
	{
		if (!active)
		{
			if ((bool)FPSInputModule.current)
			{
				FPSInputModule.current.lockPauseMenu = true;
			}
			if (deathType != 0 && deathType == DeathTypes.CutToBlack)
			{
				CutToBlack();
				Invoke("BeginFadeOut", 6f);
			}
			else
			{
				Invoke("BeginDeathFade", 0f);
				Invoke("BeginFadeOut", 6f);
			}
		}
	}

	private void BeginDeathFade()
	{
		fadeIn = true;
		active = true;
		blackOverlay.enabled = true;
	}

	private void BeginFadeOut()
	{
		fadeIn = false;
		if ((bool)FPSInputModule.current)
		{
			FPSInputModule.current.lockPauseMenu = false;
		}
	}

	private void CutToBlack()
	{
		fadeIn = true;
		active = true;
		blackOverlay.color = new Color(0f, 0f, 0f, 1f);
		blackOverlay.enabled = true;
	}

	private void Update()
	{
		if (!active)
		{
			return;
		}
		if (fadeIn)
		{
			Color color = blackOverlay.color;
			if (color.a < 0.98f)
			{
				blackOverlay.enabled = true;
				Color b = new Color(0f, 0f, 0f, 1f);
				blackOverlay.color = Color.Lerp(color, b, Time.deltaTime * 5f);
			}
			else
			{
				blackOverlay.color = new Color(0f, 0f, 0f, 1f);
			}
		}
		else
		{
			Color b2 = new Color(0f, 0f, 0f, 0f);
			Color color2 = blackOverlay.color;
			blackOverlay.color = Color.Lerp(color2, b2, Time.deltaTime * 2f);
			if (color2.a < 0.02f)
			{
				ResetOverlay();
			}
		}
	}

	public void ResetOverlay()
	{
		active = false;
		fadeIn = false;
		blackOverlay.enabled = false;
		blackOverlay.color = new Color(0f, 0f, 0f, 0f);
	}
}
