using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class uGUI_ButtonSound : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ISelectHandler
{
	[SerializeField]
	[AssertNotNull]
	private Button button;

	[SerializeField]
	private FMODAsset soundClick;

	[SerializeField]
	private FMODAsset soundHover;

	private void Start()
	{
		if ((bool)soundClick)
		{
			button.onClick.AddListener(OnClick);
		}
	}

	private void OnClick()
	{
		FMODUWE.PlayOneShot(soundClick, Vector3.zero);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if ((bool)soundHover)
		{
			FMODUWE.PlayOneShot(soundHover, Vector3.zero);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if ((bool)soundHover)
		{
			FMODUWE.PlayOneShot(soundHover, Vector3.zero);
		}
	}
}
