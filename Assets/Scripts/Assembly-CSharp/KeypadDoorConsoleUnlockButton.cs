using UnityEngine;

public class KeypadDoorConsoleUnlockButton : MonoBehaviour
{
	public void OnButtonPress()
	{
		SendMessageUpwards("UnlockDoorButtonPress", null, SendMessageOptions.RequireReceiver);
	}
}
