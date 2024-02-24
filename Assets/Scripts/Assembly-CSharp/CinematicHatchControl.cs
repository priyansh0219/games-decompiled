using UnityEngine;

public class CinematicHatchControl : MonoBehaviour
{
	public Openable hatch;

	private void OnCyclopsHatchOpen(AnimationEvent e)
	{
		hatch.Open();
	}
}
