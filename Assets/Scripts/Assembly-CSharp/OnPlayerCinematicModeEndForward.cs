using UnityEngine;

public class OnPlayerCinematicModeEndForward : MonoBehaviour
{
	public PlayerCinematicController[] forward;

	private void OnPlayerCinematicModeEnd(AnimationEvent e)
	{
		for (int i = 0; i < forward.Length; i++)
		{
			forward[i].OnPlayerCinematicModeEnd();
		}
	}
}
