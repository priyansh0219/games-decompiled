using System.Collections.Generic;
using UnityEngine;

namespace Story
{
	public class BiomeGoalTracker : MonoBehaviour, ICompileTimeCheckable
	{
		[AssertNotNull]
		public BiomeGoalData goalData;

		public float trackBiomeInterval = 3f;

		private readonly List<BiomeGoal> goals = new List<BiomeGoal>();

		private string lastBiome;

		private double timeLastBiomeChanged = -1.0;

		private bool tracking;

		public static BiomeGoalTracker main { get; private set; }

		private void Awake()
		{
			main = this;
		}

		private void Start()
		{
			for (int i = 0; i < goalData.goals.Length; i++)
			{
				BiomeGoal item = goalData.goals[i];
				goals.Add(item);
			}
			StartTracking();
		}

		private void StartTracking()
		{
			if (!tracking)
			{
				InvokeRepeating("TrackBiome", Random.value, trackBiomeInterval);
				tracking = true;
			}
		}

		private void StopTracking()
		{
			if (tracking)
			{
				CancelInvoke("TrackBiome");
				tracking = false;
			}
		}

		private void OnEnable()
		{
			StartTracking();
		}

		private void OnDisable()
		{
			StopTracking();
		}

		private void TrackBiome()
		{
			string biomeString = Player.main.GetBiomeString();
			double timePassed = DayNightCycle.main.timePassed;
			if (biomeString != lastBiome)
			{
				lastBiome = biomeString;
				timeLastBiomeChanged = timePassed;
			}
			for (int num = goals.Count - 1; num >= 0; num--)
			{
				if (goals[num].Trigger(biomeString, (float)(timePassed - timeLastBiomeChanged)))
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
