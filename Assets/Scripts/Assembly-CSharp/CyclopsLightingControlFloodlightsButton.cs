using UnityEngine;

[SkipProtoContractCheck]
public class CyclopsLightingControlFloodlightsButton : HandTarget, IHandTarget
{
	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, "CyclopsFloodlightsToggle", translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Interact);
	}

	public void OnHandClick(GUIHand guiHand)
	{
		SendMessageUpwards("ToggleFloodlights", null, SendMessageOptions.RequireReceiver);
	}
}
