using UnityEngine;
using UnityEngine.EventSystems;

public class HandTargetEventData : BaseEventData
{
	public GUIHand guiHand;

	public Transform transform;

	public HandTargetEventData(EventSystem eventSystem)
		: base(eventSystem)
	{
	}
}
