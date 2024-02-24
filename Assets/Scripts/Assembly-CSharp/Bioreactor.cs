using ProtoBuf;

[ProtoContract]
public class Bioreactor : PowerCrafter, IHandTarget
{
	[AssertLocalization]
	private const string deprecatedHandText = "Deprecated";

	[AssertLocalization]
	private const string deprecatedInstructionsHandText = "DeprecatedBuildableInstructions";

	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, "Deprecated", translate: true);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "DeprecatedBuildableInstructions", translate: true);
	}

	public void OnHandClick(GUIHand hand)
	{
	}
}
