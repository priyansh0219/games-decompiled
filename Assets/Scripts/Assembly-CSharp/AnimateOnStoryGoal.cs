using System;
using Story;
using UnityEngine;

public class AnimateOnStoryGoal : MonoBehaviour, IStoryGoalListener, ICompileTimeCheckable
{
	[AssertNotNull]
	public StoryGoal disableOnGoal;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public string paramName;

	private void Start()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			if (!main.IsGoalComplete(disableOnGoal.key))
			{
				main.AddListener(this);
			}
			else
			{
				AnimateObject();
			}
		}
	}

	public void NotifyGoalComplete(string key)
	{
		if (string.Equals(key, disableOnGoal.key, StringComparison.OrdinalIgnoreCase))
		{
			AnimateObject();
		}
	}

	private void AnimateObject()
	{
		animator.SetBool(paramName, value: true);
	}

	private void OnDestroy()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			main.RemoveListener(this);
		}
	}

	public string CompileTimeCheck()
	{
		return StoryGoalUtils.CheckStoryGoal(disableOnGoal);
	}
}
