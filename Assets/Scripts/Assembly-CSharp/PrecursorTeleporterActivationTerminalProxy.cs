using UnityEngine;

public class PrecursorTeleporterActivationTerminalProxy : MonoBehaviour, IHandTarget
{
	[AssertNotNull]
	public PrecursorTeleporterActivationTerminal activationTerminal;

	public void OnHandHover(GUIHand hand)
	{
		activationTerminal.OnProxyHandHover(hand);
	}

	public void OnHandClick(GUIHand hand)
	{
		activationTerminal.OnProxyHandClick(hand);
	}
}
