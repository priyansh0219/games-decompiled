using UnityEngine;
using UnityEngine.EventSystems;

public sealed class GenericHandTarget : MonoBehaviour, IHandTarget
{
	[AssertNotNull]
	public HandTargetEvent onHandHover;

	[AssertNotNull]
	public HandTargetEvent onHandClick;

	public void OnHandHover(GUIHand hand)
	{
		if (base.enabled && onHandHover != null)
		{
			HandTargetEventData handTargetEventData = new HandTargetEventData(EventSystem.current);
			handTargetEventData.guiHand = hand;
			handTargetEventData.transform = base.transform;
			onHandHover.Invoke(handTargetEventData);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (base.enabled && onHandHover != null)
		{
			HandTargetEventData handTargetEventData = new HandTargetEventData(EventSystem.current);
			handTargetEventData.guiHand = hand;
			handTargetEventData.transform = base.transform;
			onHandClick.Invoke(handTargetEventData);
		}
	}
}
