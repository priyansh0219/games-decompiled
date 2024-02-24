using UnityEngine;

public class ConstructorClimb : HandTarget, IHandTarget
{
	[AssertLocalization]
	public string handText = "Climb";

	[AssertNotNull]
	public Transform target;

	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, handText, translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnHandClick(GUIHand hand)
	{
		Player component = hand.gameObject.GetComponent<Player>();
		component.SetPosition(target.position);
		component.GetComponent<Rigidbody>().velocity = Vector3.zero;
		component.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
	}
}
