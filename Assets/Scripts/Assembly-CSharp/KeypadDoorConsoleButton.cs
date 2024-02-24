using UnityEngine;
using UnityEngine.EventSystems;

public class KeypadDoorConsoleButton : MonoBehaviour, IPointerHoverHandler, IEventSystemHandler
{
	public int index;

	public void OnNumberButtonPress()
	{
		SendMessageUpwards("NumberButtonPress", index, SendMessageOptions.RequireReceiver);
	}

	public void OnBackspaceButtonPress()
	{
		SendMessageUpwards("BackspaceButtonPress", null, SendMessageOptions.RequireReceiver);
	}

	public void OnPointerHover(PointerEventData eventData)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, string.Empty, translate: false, GameInput.button0);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Interact);
	}
}
