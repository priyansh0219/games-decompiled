using UnityEngine;

public class uGUI_PopupNotificationSkin : MonoBehaviour
{
	public FMODAsset soundSlideIn;

	public FMODAsset soundSlideOut;

	public FMODAsset defaultSound;

	public bool adjustPositionOnLargeScale;

	public virtual void SetVisible(bool visible)
	{
	}

	public virtual void OnShow(uGUI_PopupNotification.Entry current)
	{
	}

	public virtual void SetTransition(float value)
	{
	}

	public virtual void OnUpdate()
	{
	}
}
