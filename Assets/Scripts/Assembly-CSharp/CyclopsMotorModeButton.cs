using UWE;
using UnityEngine;
using UnityEngine.UI;

public class CyclopsMotorModeButton : MonoBehaviour
{
	[AssertNotNull]
	public Image image;

	[AssertNotNull]
	public Sprite inactiveSprite;

	[AssertNotNull]
	public Sprite activeSprite;

	public string tooltipKey;

	private SubRoot subRoot;

	public CyclopsMotorMode.CyclopsMotorModes motorModeIndex;

	private void Start()
	{
		subRoot = UWE.Utils.GetComponentInHierarchy<SubRoot>(base.gameObject);
	}

	public void SetCyclopsMotorMode(CyclopsMotorMode.CyclopsMotorModes setToMotorMode)
	{
		if (setToMotorMode == motorModeIndex)
		{
			SendMessageUpwards("ChangeCyclopsMotorMode", motorModeIndex, SendMessageOptions.RequireReceiver);
			image.sprite = activeSprite;
			VoiceNotification vo = subRoot.silentRunningNotification;
			switch (setToMotorMode)
			{
			case CyclopsMotorMode.CyclopsMotorModes.Slow:
				vo = subRoot.aheadSlowNotification;
				break;
			case CyclopsMotorMode.CyclopsMotorModes.Standard:
				vo = subRoot.aheadStandardNotification;
				break;
			case CyclopsMotorMode.CyclopsMotorModes.Flank:
				vo = subRoot.aheadFlankNotification;
				break;
			}
			subRoot.voiceNotificationManager.PlayVoiceNotification(vo, addToQueue: false, forcePlay: true);
		}
		else
		{
			image.sprite = inactiveSprite;
		}
	}

	public void OnMouseOver()
	{
	}

	public void OnClick()
	{
		if (!(Player.main.currentSub != subRoot))
		{
			base.transform.parent.BroadcastMessage("SetCyclopsMotorMode", motorModeIndex, SendMessageOptions.RequireReceiver);
		}
	}
}
