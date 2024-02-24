using UnityEngine;

public class ForwardOnPlayerEntered : MonoBehaviour
{
	[AssertNotNull]
	public GameObject forwardTo;

	public void OnPlayerEntered()
	{
		forwardTo.SendMessage("OnPlayerEntered", null, SendMessageOptions.DontRequireReceiver);
	}
}
