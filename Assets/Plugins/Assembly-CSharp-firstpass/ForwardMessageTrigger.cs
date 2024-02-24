using UnityEngine;

public class ForwardMessageTrigger : MonoBehaviour
{
	public string messageName = "ForwardOnTrigger";

	public GameObject target;

	private const string exit = "Exit";

	private const string enter = "Enter";

	private string enterMessage;

	private string exitMessage;

	private void Start()
	{
		enterMessage = messageName + "Enter";
		exitMessage = messageName + "Exit";
	}

	private void OnTriggerEnter(Collider other)
	{
		if (target != null)
		{
			target.SendMessage(enterMessage, other.gameObject, SendMessageOptions.DontRequireReceiver);
			return;
		}
		Debug.Log("Warning: use of ForwardMessageTrigger on " + base.gameObject.name + " with specifying a target game object.");
		Object.Destroy(base.gameObject);
	}

	private void OnTriggerExit(Collider other)
	{
		if (target != null)
		{
			target.SendMessage(exitMessage, other.gameObject, SendMessageOptions.DontRequireReceiver);
			return;
		}
		Debug.Log("Warning: use of ForwardMessageTrigger on " + base.gameObject.name + " with specifying a target game object.");
		Object.Destroy(base.gameObject);
	}
}
