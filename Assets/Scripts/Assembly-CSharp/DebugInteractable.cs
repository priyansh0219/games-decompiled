using UnityEngine;

public class DebugInteractable : MonoBehaviour
{
	public void OnHandClick(GUIHand hand)
	{
		ErrorMessage.AddDebug("OnHandClick");
	}
}
