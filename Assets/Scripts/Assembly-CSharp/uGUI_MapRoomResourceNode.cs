using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class uGUI_MapRoomResourceNode : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[HideInInspector]
	public uGUI_MapRoomScanner mainUI;

	[HideInInspector]
	public int index;

	public TextMeshProUGUI text;

	public GameObject hover;

	public GameObject background;

	public uGUI_Icon icon;

	[HideInInspector]
	public FMODAsset hoverSound;

	private void Start()
	{
		hover.SetActive(value: false);
		background.SetActive(value: true);
	}

	public void SetTechType(TechType techType)
	{
		text.text = Language.main.Get(techType.AsString());
		Sprite sprite = SpriteManager.Get(techType, null);
		if (sprite != null)
		{
			icon.sprite = sprite;
			icon.enabled = true;
		}
		else
		{
			icon.enabled = false;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hover.SetActive(value: true);
		background.SetActive(value: false);
		Utils.PlayFMODAsset(hoverSound, base.transform);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hover.SetActive(value: false);
		background.SetActive(value: true);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (uGUI_MapRoomScanner.IsAcceptedButton(eventData.button))
		{
			mainUI.OnStartScan(index);
			hover.SetActive(value: false);
			background.SetActive(value: true);
		}
	}
}
