using UnityEngine;

public class PrecursorTeleporterExitCinematicController : MonoBehaviour
{
	[AssertNotNull]
	public PlayerCinematicController controller;

	private void OnEnable()
	{
		PrecursorTeleporter.TeleportEventEnd += TeleportComplete;
	}

	private void OnDisable()
	{
		PrecursorTeleporter.TeleportEventEnd -= TeleportComplete;
	}

	private void TeleportComplete()
	{
		Invoke("BeginAnimation", 0.5f);
	}

	private void BeginAnimation()
	{
		controller.StartCinematicMode(Player.main);
	}

	public void OnPlayerCinematicModeEnd(PlayerCinematicController controller)
	{
		Object.Destroy(base.gameObject);
	}
}
