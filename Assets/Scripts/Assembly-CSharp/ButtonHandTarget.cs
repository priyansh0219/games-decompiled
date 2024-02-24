public class ButtonHandTarget : HandTarget, IHandTarget
{
	public delegate void ButtonHandler();

	public FMOD_StudioEventEmitter pressSound;

	public ButtonHandler buttonHandler;

	[AssertLocalization]
	private const string pressButtonHandText = "PressButton";

	public void OnHandHover(GUIHand hand)
	{
		if (hand.IsFreeToInteract())
		{
			HandReticle.main.SetIcon(HandReticle.IconType.Interact);
			HandReticle.main.SetText(HandReticle.TextType.Hand, "PressButton", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (hand.IsFreeToInteract())
		{
			if (pressSound != null)
			{
				Utils.PlayEnvSound(pressSound);
			}
			if (buttonHandler != null)
			{
				buttonHandler();
			}
		}
	}
}
