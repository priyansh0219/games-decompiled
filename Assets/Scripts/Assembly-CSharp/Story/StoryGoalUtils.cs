using System.Collections.Generic;
using System.Linq;

namespace Story
{
	public static class StoryGoalUtils
	{
		private static PDAData pdaData;

		private static void Initialize()
		{
			_ = (bool)pdaData;
		}

		public static string CheckStoryGoals(IEnumerable<StoryGoal> goals)
		{
			Initialize();
			if (!pdaData)
			{
				return "Failed to load PDA data";
			}
			foreach (StoryGoal goal in goals)
			{
				string text = CheckStoryGoal(goal);
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
			return null;
		}

		public static string CheckStoryGoal(StoryGoal goal)
		{
			Initialize();
			if (!pdaData)
			{
				return "Failed to load PDA data";
			}
			if (goal == null)
			{
				return "Goal must not be null";
			}
			if (string.IsNullOrEmpty(goal.key))
			{
				return "Goal key must not be empty";
			}
			if (goal.delay < 0f)
			{
				return "Goal delay must not be negative";
			}
			switch (goal.goalType)
			{
			case GoalType.PDA:
			case GoalType.Radio:
				return CheckPDAGoal(goal.key);
			case GoalType.Encyclopedia:
				return CheckEncyclopediaGoal(goal.key);
			default:
				return null;
			}
		}

		public static string CheckPDAGoal(string key)
		{
			Initialize();
			if (string.IsNullOrEmpty(key))
			{
				return "Goal key must not be empty";
			}
			if (!pdaData.log.Any((PDALog.EntryData p) => p.key == key))
			{
				return $"Missing entry '{key}' in PDAData log";
			}
			return null;
		}

		public static string CheckEncyclopediaGoal(string key)
		{
			Initialize();
			if (string.IsNullOrEmpty(key))
			{
				return "Goal key must not be empty";
			}
			if (!pdaData.encyclopedia.Any((PDAEncyclopedia.EntryData p) => p.key == key))
			{
				return $"Missing entry '{key}' in PDAData encyclopedia";
			}
			return null;
		}
	}
}
