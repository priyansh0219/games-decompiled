using UnityEngine;

public class PrecursorDoorTrigger : MonoBehaviour
{
	[AssertNotNull]
	public PrecursorDoorway door;

	private void OnTriggerEnter(Collider other)
	{
		if ((bool)other && other.gameObject == Player.mainObject)
		{
			door.ToggleDoor(open: true);
		}
	}
}
