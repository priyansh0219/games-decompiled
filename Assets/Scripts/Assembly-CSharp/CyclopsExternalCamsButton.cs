using UnityEngine;

public class CyclopsExternalCamsButton : MonoBehaviour
{
	[AssertNotNull]
	public CyclopsExternalCams cyclopsExternalCams;

	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public LiveMixin liveMixin;

	private bool mouseHover;

	private void Update()
	{
		if (mouseHover)
		{
			HandReticle main = HandReticle.main;
			main.SetText(HandReticle.TextType.Hand, "CyclopsCameras", translate: true, GameInput.Button.LeftHand);
			main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	public void OnMouseEnter()
	{
		if (!(Player.main.currentSub != subRoot))
		{
			mouseHover = true;
		}
	}

	public void OnMouseExit()
	{
		if (!(Player.main.currentSub != subRoot))
		{
			HandReticle.main.SetIcon(HandReticle.IconType.Default);
			mouseHover = false;
		}
	}

	public void CameraButtonActivated()
	{
		if (!(Player.main.currentSub != subRoot) && liveMixin.IsAlive())
		{
			mouseHover = false;
			cyclopsExternalCams.SetActive(value: true);
		}
	}
}
