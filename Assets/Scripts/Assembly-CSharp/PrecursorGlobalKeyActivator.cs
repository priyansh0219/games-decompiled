using System;
using Gendarme;
using Story;
using UnityEngine;

[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
public class PrecursorGlobalKeyActivator : MonoBehaviour, IStoryGoalListener
{
	public string doorActivationKey;

	private bool isOpen;

	private void ToggleDoor(bool doorOpen)
	{
		if (!string.IsNullOrEmpty(doorActivationKey) && doorOpen && !isOpen)
		{
			isOpen = true;
			if ((bool)StoryGoalManager.main)
			{
				StoryGoalManager.main.OnGoalComplete(doorActivationKey);
			}
		}
	}

	private void OnEnable()
	{
		if (string.IsNullOrEmpty(doorActivationKey))
		{
			return;
		}
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			if (StoryGoalManager.main.IsGoalComplete(doorActivationKey))
			{
				BroadcastMessage("ToggleDoor", true, SendMessageOptions.RequireReceiver);
			}
			else
			{
				main.AddListener(this);
			}
		}
	}

	private void OnDisable()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			main.RemoveListener(this);
		}
	}

	public void NotifyGoalComplete(string key)
	{
		if (string.Equals(key, doorActivationKey, StringComparison.Ordinal))
		{
			BroadcastMessage("ToggleDoor", true, SendMessageOptions.RequireReceiver);
		}
	}

	public void NotifyGoalReset(string key)
	{
	}

	public void NotifyGoalsDeserialized()
	{
	}
}
