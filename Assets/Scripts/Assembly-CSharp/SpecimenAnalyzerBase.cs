public class SpecimenAnalyzerBase : HandTarget, IHandTarget
{
	public SpecimenAnalyzer specimenAnalyzer;

	public override void Awake()
	{
		base.Awake();
	}

	public void OnHandHover(GUIHand hand)
	{
		Constructable component = specimenAnalyzer.gameObject.GetComponent<Constructable>();
		if (!component.constructed)
		{
			GUIHand.Send(component.gameObject, HandTargetEventType.Hover, hand);
			return;
		}
		string statusText = specimenAnalyzer.GetStatusText();
		HandReticle.main.SetText(HandReticle.TextType.Hand, statusText, translate: false);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnHandClick(GUIHand hand)
	{
	}
}
