using System.Collections.Generic;
using UnityEngine;

namespace Story
{
	public class LocationGoalTracker : MonoBehaviour, ICompileTimeCheckable
	{
		[AssertNotNull]
		public LocationGoalData goalData;

		public float trackLocationInterval = 3f;

		private readonly List<LocationGoal> goals = new List<LocationGoal>();

		private void Start()
		{
			for (int i = 0; i < goalData.goals.Length; i++)
			{
				LocationGoal item = goalData.goals[i];
				goals.Add(item);
			}
			InvokeRepeating("TrackLocation", Random.value, trackLocationInterval);
		}

		private void TrackLocation()
		{
			Vector3 position = Player.main.transform.position;
			double timePassed = DayNightCycle.main.timePassed;
			for (int num = goals.Count - 1; num >= 0; num--)
			{
				if (goals[num].Trigger(position, (float)timePassed))
				{
					goals.RemoveFast(num);
				}
			}
		}

		public string CompileTimeCheck()
		{
			return StoryGoalUtils.CheckStoryGoals(goalData.goals);
		}
	}
}
