using UnityEngine;

public class OnPlayerNearby : MonoBehaviour
{
	public GameObject rootGameObject;

	public GameObject player;

	private void OnTriggerStay(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			player = other.gameObject;
			rootGameObject.SendMessage("OnPlayerNearby", player, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			player = null;
			rootGameObject.SendMessage("OnPlayerLeft");
		}
	}
}
