using UnityEngine;

public class RocketElevatorBlocker : MonoBehaviour
{
	[AssertNotNull]
	public Rocket rocket;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.Equals(Player.main.gameObject))
		{
			rocket.elevatorBlocked = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.Equals(Player.main.gameObject))
		{
			rocket.elevatorBlocked = false;
		}
	}
}
