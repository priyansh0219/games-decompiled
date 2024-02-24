using System;

namespace Story
{
	[Serializable]
	public class StoryGoal
	{
		public float delay;

		public string key;

		public GoalType goalType;

		public void Trigger()
		{
			StoryGoalScheduler.main.Schedule(this);
		}

		[Obsolete("Use a public member field of type StoryGoal instead")]
		public StoryGoal()
		{
		}

		public StoryGoal(string key, GoalType goalType, float delay)
		{
			this.key = key;
			this.goalType = goalType;
			this.delay = delay;
		}

		public static void Execute(string key, GoalType goalType)
		{
			bool flag = StoryGoalManager.main.OnGoalComplete(key);
			switch (goalType)
			{
			case GoalType.PDA:
				PDALog.Add(key);
				break;
			case GoalType.Radio:
				if (flag)
				{
					StoryGoalManager.main.AddPendingRadioMessage(key);
				}
				break;
			case GoalType.Encyclopedia:
				PDAEncyclopedia.AddAndPlaySound(key);
				break;
			}
		}

		public override string ToString()
		{
			return $"({key}, {goalType}, {delay}s)";
		}
	}
}
