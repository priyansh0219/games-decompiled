using UnityEngine;

public class PrecursorElevatorTrigger : MonoBehaviour
{
	public int index;

	[AssertNotNull]
	public PrecursorElevatorTrigger other;

	private int otherIndex;

	private void Start()
	{
		otherIndex = other.index;
	}

	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.Equals(Player.main.gameObject))
		{
			SendMessageUpwards("ActivateElevator", otherIndex, SendMessageOptions.RequireReceiver);
		}
	}
}
