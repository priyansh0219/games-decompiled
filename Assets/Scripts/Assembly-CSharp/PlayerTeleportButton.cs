using UnityEngine;

[SkipProtoContractCheck]
public class PlayerTeleportButton : HandTarget, IHandTarget
{
	public Transform destination;

	public bool isForEscapePod;

	public bool enterOnly;

	[AssertNotNull]
	public string hoverText = "";

	public string customGoal = "";

	public void OnHandHover(GUIHand hand)
	{
		if (hand.IsFreeToInteract())
		{
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			HandReticle.main.SetText(HandReticle.TextType.Hand, hoverText, translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (!hand.IsFreeToInteract())
		{
			return;
		}
		Player component = hand.GetComponent<Player>();
		component.SetPosition(destination.position);
		if (isForEscapePod)
		{
			if (enterOnly)
			{
				component.escapePod.Update(newValue: true);
			}
			else
			{
				component.escapePod.Update(newValue: false);
			}
		}
		if (customGoal != "")
		{
			GoalManager.main.OnCustomGoalEvent(customGoal);
		}
	}
}
