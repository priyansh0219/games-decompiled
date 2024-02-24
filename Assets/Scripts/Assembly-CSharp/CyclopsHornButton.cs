using UnityEngine;

public class CyclopsHornButton : MonoBehaviour
{
	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public FMOD_CustomEmitter hornSFX;

	private bool mouseHover;

	public void OnPress()
	{
		if (!(Player.main.currentSub != subRoot))
		{
			hornSFX.Play();
		}
	}

	private void Update()
	{
		if (mouseHover)
		{
			HandReticle main = HandReticle.main;
			main.SetText(HandReticle.TextType.Hand, "CyclopsHorn", translate: true, GameInput.Button.LeftHand);
			main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnMouseOver()
	{
		if (!(Player.main.currentSub != subRoot))
		{
			mouseHover = true;
		}
	}

	public void OnMouseLeave()
	{
		if (!(Player.main.currentSub != subRoot))
		{
			HandReticle.main.SetIcon(HandReticle.IconType.Default);
			mouseHover = false;
		}
	}
}
