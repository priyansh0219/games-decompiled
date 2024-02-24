using UnityEngine;

public class IncubatorEggAnimation : MonoBehaviour
{
	[AssertNotNull]
	public Animator eggAnimator;

	[AssertNotNull]
	public Transform babyEmperorAttachPoint;

	private bool animationActive;

	private SeaEmperorBaby baby;

	public void SetHatched(string animParameter)
	{
		SafeAnimator.SetBool(eggAnimator, animParameter, value: true);
		SafeAnimator.SetBool(eggAnimator, "hatched", value: true);
	}

	public void StartHatchAnimation(int babyIdentifier, string animParameter, GameObject babyGO)
	{
		if (!animationActive)
		{
			baby = babyGO.GetComponent<SeaEmperorBaby>();
			baby.SetId(babyIdentifier);
			SafeAnimator.SetBool(eggAnimator, animParameter, value: true);
			SafeAnimator.SetBool(baby.GetAnimator(), animParameter, value: true);
			baby.cinematicController.SetCinematicMode(cinematicOn: true);
			animationActive = true;
		}
	}

	public void OnHatchAnimationEnd()
	{
		if (animationActive)
		{
			baby.cinematicController.SetCinematicMode(cinematicOn: false);
			animationActive = false;
			baby.SwimToMother();
		}
	}

	private void OnDisable()
	{
		if (animationActive)
		{
			OnHatchAnimationEnd();
		}
	}
}
