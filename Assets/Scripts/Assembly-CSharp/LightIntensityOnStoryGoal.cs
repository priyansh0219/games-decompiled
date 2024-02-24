using System;
using Story;
using UnityEngine;

public class LightIntensityOnStoryGoal : MonoBehaviour, IStoryGoalListener, ICompileTimeCheckable
{
	[AssertNotNull]
	public StoryGoal disableOnGoal;

	public float intensity;

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
				SetIntensity();
			}
		}
	}

	public void NotifyGoalComplete(string key)
	{
		if (string.Equals(key, disableOnGoal.key, StringComparison.OrdinalIgnoreCase))
		{
			TriggerIntensityChange(disableOnGoal.delay);
		}
	}

	private void TriggerIntensityChange(float delay = 0f)
	{
		if (delay > 0f)
		{
			Invoke("SetIntensity", delay);
		}
		else
		{
			SetIntensity();
		}
	}

	private void SetIntensity()
	{
		Light[] componentsInChildren = GetComponentsInChildren<Light>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].intensity = intensity;
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

	public string CompileTimeCheck()
	{
		return StoryGoalUtils.CheckStoryGoal(disableOnGoal);
	}
}
