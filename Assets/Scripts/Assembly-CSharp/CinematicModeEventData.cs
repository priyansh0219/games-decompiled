using UnityEngine.EventSystems;

public class CinematicModeEventData : BaseEventData
{
	public Player player;

	public PlayerCinematicController cinematicController;

	public CinematicModeEventData(EventSystem eventSystem)
		: base(eventSystem)
	{
	}
}
