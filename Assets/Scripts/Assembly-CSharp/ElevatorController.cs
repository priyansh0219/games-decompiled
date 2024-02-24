[SkipProtoContractCheck]
public class ElevatorController : HandTarget, IHandTarget
{
	[AssertNotNull]
	public Rocket rocket;

	[AssertLocalization]
	private const string startElevatorHandText = "StartElevator";

	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, "StartElevator", translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Interact);
	}

	public void OnHandClick(GUIHand hand)
	{
		rocket.ElevatorControlButtonActivate();
	}
}
