using UnityEngine;

public class PrecursorKeyTerminalTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.Equals(Player.main.gameObject))
		{
			SendMessageUpwards("OpenDeck");
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (col.gameObject.Equals(Player.main.gameObject))
		{
			SendMessageUpwards("CloseDeck");
		}
	}
}
