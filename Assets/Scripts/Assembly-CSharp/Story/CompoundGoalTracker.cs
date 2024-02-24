using System.Collections.Generic;
using UnityEngine;

namespace Story
{
	public class CompoundGoalTracker : MonoBehaviour, ICompileTimeCheckable
	{
		public CompoundGoalData goalData;

		private readonly List<CompoundGoal> goals = new List<CompoundGoal>();

		public void Initialize(HashSet<string> completedGoals)
		{
			for (int i = 0; i < goalData.goals.Length; i++)
			{
				CompoundGoal compoundGoal = goalData.goals[i];
				if (!completedGoals.Contains(compoundGoal.key))
				{
					goals.Add(compoundGoal);
				}
			}
			NotifyGoalComplete(completedGoals);
		}

		public void NotifyGoalComplete(HashSet<string> completedGoals)
		{
			for (int num = goals.Count - 1; num >= 0; num--)
			{
				if (goals[num].Trigger(completedGoals))
				{
					goals[num] = goals[goals.Count - 1];
					goals.RemoveAt(goals.Count - 1);
				}
			}
		}

		public string CompileTimeCheck()
		{
			return StoryGoalUtils.CheckStoryGoals(goalData.goals);
		}
	}
}
