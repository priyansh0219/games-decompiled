using System;
using System.Collections.Generic;
using UnityEngine;

namespace Story
{
	public class OnGoalUnlockTracker : MonoBehaviour, ICompileTimeCheckable
	{
		public OnGoalUnlockData unlockData;

		public GameObject signalPrefab;

		private readonly Dictionary<string, OnGoalUnlock> goalUnlocks = new Dictionary<string, OnGoalUnlock>(StringComparer.OrdinalIgnoreCase);

		public void Initialize(HashSet<string> completedGoals)
		{
			for (int i = 0; i < unlockData.onGoalUnlocks.Length; i++)
			{
				OnGoalUnlock onGoalUnlock = unlockData.onGoalUnlocks[i];
				if (completedGoals.Contains(onGoalUnlock.goal))
				{
					onGoalUnlock.TriggerBlueprints();
					onGoalUnlock.TriggerAchievements();
				}
				else
				{
					goalUnlocks.Add(onGoalUnlock.goal, onGoalUnlock);
				}
			}
		}

		public void NotifyGoalComplete(string completedGoal)
		{
			if (goalUnlocks.TryGetValue(completedGoal, out var value))
			{
				value.Trigger(this);
				goalUnlocks.Remove(completedGoal);
			}
		}

		public string CompileTimeCheck()
		{
			GameObject gameObject = new GameObject();
			Language language = gameObject.AddComponent<Language>();
			language.Initialize("English");
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			OnGoalUnlock[] onGoalUnlocks = unlockData.onGoalUnlocks;
			foreach (OnGoalUnlock onGoalUnlock in onGoalUnlocks)
			{
				if (!hashSet.Add(onGoalUnlock.goal))
				{
					return $"OnGoalUnlockData contains duplicate goals '{onGoalUnlock.goal}' ({onGoalUnlock}).";
				}
				UnlockSignalData[] signals = onGoalUnlock.signals;
				foreach (UnlockSignalData unlockSignalData in signals)
				{
					if (!language.Contains(unlockSignalData.targetDescription))
					{
						return $"OnGoalUnlockData contains a missing translation for signal '{unlockSignalData.targetDescription}' in goal unlock '{onGoalUnlock.goal}'";
					}
				}
			}
			UnityEngine.Object.DestroyImmediate(gameObject);
			return null;
		}

		protected void Awake()
		{
			DevConsole.RegisterConsoleCommand(this, "ongoal");
		}

		protected void OnConsoleCommand_ongoal(NotificationCenter.Notification n)
		{
			if (n == null || n.data == null || n.data.Count <= 0)
			{
				return;
			}
			string text = (string)n.data[0];
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (string.Equals(text, "all", StringComparison.OrdinalIgnoreCase))
			{
				OnGoalUnlock[] onGoalUnlocks = unlockData.onGoalUnlocks;
				foreach (OnGoalUnlock onGoalUnlock in onGoalUnlocks)
				{
					NotifyGoalComplete(onGoalUnlock.goal);
				}
			}
			else
			{
				NotifyGoalComplete(text);
			}
		}
	}
}
