using UnityEngine;

[SkipProtoContractCheck]
public class BeaconLabel : HandTarget, IHandTarget
{
	private const int maxChars = 25;

	[AssertLocalization]
	private const string labelKey = "BeaconLabel";

	[AssertLocalization]
	private const string submitKey = "BeaconSubmit";

	[AssertLocalization]
	private const string labelEditKey = "BeaconLabelEdit";

	public Collider hitCollider;

	[AssertNotNull]
	public PingInstance pingInstance;

	private string stringBeaconLabel;

	private string stringBeaconSubmit;

	private string labelName;

	private void Start()
	{
		Language main = Language.main;
		stringBeaconLabel = main.Get("BeaconLabel");
		stringBeaconSubmit = main.Get("BeaconSubmit");
	}

	public void OnHandHover(GUIHand hand)
	{
		HandReticle main = HandReticle.main;
		main.SetText(HandReticle.TextType.Hand, "BeaconLabelEdit", translate: true, GameInput.Button.LeftHand);
		main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		main.SetIcon(HandReticle.IconType.Rename);
	}

	public void OnHandClick(GUIHand hand)
	{
		uGUI.main.userInput.RequestString(stringBeaconLabel, stringBeaconSubmit, labelName, 25, SetLabel);
	}

	public void SetLabel(string label)
	{
		labelName = label;
		if ((bool)pingInstance)
		{
			pingInstance.SetLabel(label);
		}
	}

	public string GetLabel()
	{
		return labelName;
	}

	public void OnPickedUp()
	{
		hitCollider.enabled = false;
	}

	public void OnDropped()
	{
		hitCollider.enabled = true;
	}
}
