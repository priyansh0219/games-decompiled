using System;
using Story;
using UnityEngine;

public class DisableEmissiveOnStoryGoal : MonoBehaviour, IStoryGoalListener, ICompileTimeCheckable
{
	[AssertNotNull]
	public StoryGoal disableOnGoal;

	private Renderer[] renderers;

	private MaterialPropertyBlock block;

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
				DisableEmissive();
			}
		}
	}

	public void NotifyGoalComplete(string key)
	{
		if (string.Equals(key, disableOnGoal.key, StringComparison.OrdinalIgnoreCase))
		{
			DisableEmissive();
		}
	}

	private void DisableEmissive()
	{
		block = new MaterialPropertyBlock();
		renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < renderers.Length; i++)
		{
			block.Clear();
			renderers[i].GetPropertyBlock(block);
			block.SetFloat(ShaderPropertyID._UwePowerLoss, 1f);
			bool num = block.GetFloat(ShaderPropertyID._AffectedByDayNightCycle) < 0.5f;
			Vector4 vector = block.GetVector(ShaderPropertyID._ExposureIBL);
			if (num)
			{
				vector.x *= 0.3f;
				vector.y *= 0.3f;
			}
			block.SetVector(ShaderPropertyID._ExposureIBL, vector);
			renderers[i].SetPropertyBlock(block);
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
