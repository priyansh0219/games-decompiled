using System;
using Story;
using UnityEngine;

public class PrecursorGunCentralColumn : MonoBehaviour, IStoryGoalListener
{
	[AssertNotNull]
	public StoryGoal disableGun;

	[AssertNotNull]
	public GameObject beam;

	[AssertNotNull]
	public Animator animator;

	private void Start()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			if (!main.IsGoalComplete(disableGun.key))
			{
				main.AddListener(this);
			}
			else
			{
				SetDisabledInstantly();
			}
		}
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
		if (string.Equals(key, disableGun.key, StringComparison.OrdinalIgnoreCase))
		{
			SetDisabled();
		}
	}

	private void SetDisabledInstantly()
	{
		animator.SetBool("disabledInstantly", value: true);
		beam.SetActive(value: false);
	}

	private void SetDisabled()
	{
		animator.SetBool("active", value: false);
		beam.SetActive(value: false);
	}
}
