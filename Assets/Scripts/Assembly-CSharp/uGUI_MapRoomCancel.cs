using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class uGUI_MapRoomCancel : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	public uGUI_MapRoomScanner mainUI;

	public Image image;

	private void Start()
	{
		image.color = Color.grey;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		image.color = Color.yellow;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		image.color = Color.grey;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (uGUI_MapRoomScanner.IsAcceptedButton(eventData.button))
		{
			mainUI.OnCancelScan();
			image.color = Color.grey;
		}
	}
}
