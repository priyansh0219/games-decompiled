using UnityEngine;

[SkipProtoContractCheck]
public class BaseEntranceLadder : HandTarget, IHandTarget
{
	public Transform targetTransform;

	[AssertLocalization]
	private const string climbUpHandText = "ClimbUp";

	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, "ClimbUp", translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnHandClick(GUIHand hand)
	{
		if (base.enabled)
		{
			hand.player.SetPosition(targetTransform.position);
			hand.player.SetCurrentSub(GetComponentInParent<SubRoot>());
		}
	}
}
