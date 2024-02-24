using UnityEngine;
using UnityEngine.EventSystems;

public class ColorChangeEventData : BaseEventData
{
	public Color color;

	public Vector3 hsb;

	public ColorChangeEventData(EventSystem eventSystem)
		: base(eventSystem)
	{
		color = new Color(0f, 0f, 0f, 0f);
		hsb = new Vector3(0f, 0f, 0f);
	}
}
