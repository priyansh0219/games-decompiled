[SkipProtoContractCheck]
public class IntroLifepodWeldableWires : HandTarget, IHandTarget
{
	public LiveMixin liveMixin;

	[AssertLocalization]
	private const string damagedWiresHandText = "DamagedWires";

	[AssertLocalization]
	private const string weldToFixHandText = "WeldToFix";

	private void Start()
	{
	}

	public void OnHandHover(GUIHand hand)
	{
		if (liveMixin.health < liveMixin.maxHealth)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "DamagedWires", translate: true);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "WeldToFix", translate: true);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
	}
}
