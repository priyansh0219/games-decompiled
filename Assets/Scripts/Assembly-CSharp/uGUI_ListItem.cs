using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class uGUI_ListItem : Graphic, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	public uGUI_ListItemManager manager;

	public Image icon;

	public Text text;

	protected override void OnPopulateMesh(VertexHelper m)
	{
		m.Clear();
	}

	public void SetText(string value)
	{
		text.text = value;
	}

	public void SetIcon(Sprite sprite)
	{
		icon.sprite = sprite;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		manager.OnPointerEnter(this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		manager.OnPointerExit(this);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int button = -1;
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			button = 0;
			break;
		case PointerEventData.InputButton.Right:
			button = 1;
			break;
		case PointerEventData.InputButton.Middle:
			button = 2;
			break;
		}
		manager.OnPointerClick(this, button);
	}
}
