using System;
using Story;
using UnityEngine;

public class PrecursorAquariumIncubatorTrigger : MonoBehaviour, IStoryGoalListener
{
	[AssertNotNull]
	public string listenForTeleporterActiveGoal = "PrecursorPrisonAquariumFinalTeleporterActive";

	[AssertNotNull]
	public string listenForBabiesHatchedGoal = "SeaEmperorBabiesHatched";

	[AssertNotNull]
	public Animator targetAnimator;

	[AssertNotNull]
	public string atIncubatorParameterName = "at_incubator";

	[AssertNotNull]
	public string hatchingTimeParameterName = "hatching_time";

	private void Start()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if (!main)
		{
			return;
		}
		if (main.IsGoalComplete(listenForBabiesHatchedGoal))
		{
			OnBabiesHatched();
			return;
		}
		if (main.IsGoalComplete(listenForTeleporterActiveGoal))
		{
			OnTeleporterActive();
		}
		main.AddListener(this);
	}

	private void OnDestroy()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			main.RemoveListener(this);
		}
	}

	public void NotifyGoalComplete(string key)
	{
		if (string.Equals(key, listenForTeleporterActiveGoal, StringComparison.OrdinalIgnoreCase))
		{
			OnTeleporterActive();
		}
		else if (string.Equals(key, listenForBabiesHatchedGoal, StringComparison.OrdinalIgnoreCase))
		{
			OnBabiesHatched();
		}
	}

	private void OnTeleporterActive()
	{
		SafeAnimator.SetBool(targetAnimator, hatchingTimeParameterName, value: true);
	}

	private void OnBabiesHatched()
	{
		SafeAnimator.SetBool(targetAnimator, atIncubatorParameterName, value: true);
		SafeAnimator.SetBool(targetAnimator, hatchingTimeParameterName, value: true);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
