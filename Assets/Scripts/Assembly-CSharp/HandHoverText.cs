using UnityEngine;

public class HandHoverText : MonoBehaviour, IHandTarget
{
	[AssertLocalization]
	public string text;

	public virtual void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: true);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
	}

	public virtual void OnHandClick(GUIHand hand)
	{
	}
}
