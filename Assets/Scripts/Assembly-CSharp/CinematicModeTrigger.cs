[SkipProtoContractCheck]
public class CinematicModeTrigger : CinematicModeTriggerBase
{
	[AssertLocalization(AssertLocalizationAttribute.Options.AllowEmptyString)]
	public string handText;

	public override void OnHandHover(GUIHand hand)
	{
		if (showIconOnHandHover && PlayerCinematicController.cinematicModeCount <= 0 && !string.IsNullOrEmpty(handText))
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, handText, translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}
}
