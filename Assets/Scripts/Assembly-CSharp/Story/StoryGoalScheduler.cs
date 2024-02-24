using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UWE;
using UnityEngine;

namespace Story
{
	[ProtoContract]
	public class StoryGoalScheduler : MonoBehaviour
	{
		private bool paused;

		private const int currentVersion = 1;

		[NonSerialized]
		[ProtoMember(1)]
		public int version = 1;

		[NonSerialized]
		[ProtoMember(2)]
		public readonly List<ScheduledGoal> schedule = new List<ScheduledGoal>();

		public static StoryGoalScheduler main { get; private set; }

		private void Awake()
		{
			main = this;
			DevConsole.RegisterConsoleCommand(this, "schedule");
		}

		private void Update()
		{
			if (GameApplication.isQuitting)
			{
				return;
			}
			DayNightCycle dayNightCycle = DayNightCycle.main;
			if (!dayNightCycle || paused)
			{
				return;
			}
			double timePassed = dayNightCycle.timePassed;
			for (int num = schedule.Count - 1; num >= 0; num--)
			{
				ScheduledGoal scheduledGoal = schedule[num];
				if (timePassed >= (double)scheduledGoal.timeExecute)
				{
					schedule[num] = schedule[schedule.Count - 1];
					schedule.RemoveAt(schedule.Count - 1);
					StoryGoal.Execute(scheduledGoal.goalKey, scheduledGoal.goalType);
				}
			}
		}

		public void Schedule(StoryGoal goal)
		{
			DayNightCycle dayNightCycle = DayNightCycle.main;
			float num = (dayNightCycle ? ((float)dayNightCycle.timePassed) : 0f);
			ScheduledGoal scheduledGoal = new ScheduledGoal();
			scheduledGoal.timeExecute = num + goal.delay;
			scheduledGoal.goalKey = goal.key;
			scheduledGoal.goalType = goal.goalType;
			schedule.Add(scheduledGoal);
		}

		private void OnConsoleCommand_schedule(NotificationCenter.Notification n)
		{
			DayNightCycle dayNightCycle = DayNightCycle.main;
			float num = (dayNightCycle ? ((float)dayNightCycle.timePassed) : 0f);
			foreach (ScheduledGoal item in schedule.OrderBy((ScheduledGoal p) => p.timeExecute))
			{
				TimeSpan timeSpan = TimeSpan.FromSeconds(item.timeExecute - num);
				ErrorMessage.AddDebug($"{item.goalKey} ({item.goalType}) in {timeSpan:g}");
			}
		}

		public IEnumerable<ScheduledGoal> GetSchedule()
		{
			return schedule;
		}

		public void Pause()
		{
			paused = true;
		}
	}
}
