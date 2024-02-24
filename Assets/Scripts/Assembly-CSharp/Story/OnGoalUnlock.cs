using System;
using UWE;

namespace Story
{
	[Serializable]
	public class OnGoalUnlock
	{
		public string goal;

		public UnlockBlueprintData[] blueprints;

		public UnlockSignalData[] signals;

		public UnlockItemData[] items;

		public GameAchievements.Id[] achievements;

		public void Trigger(OnGoalUnlockTracker tracker)
		{
			TriggerBlueprints();
			TriggerAchievements();
			for (int i = 0; i < signals.Length; i++)
			{
				signals[i].Trigger(tracker);
			}
			for (int j = 0; j < items.Length; j++)
			{
				items[j].Trigger();
			}
		}

		public void TriggerAchievements()
		{
			for (int i = 0; i < achievements.Length; i++)
			{
				GameAchievements.Unlock(achievements[i]);
			}
		}

		public void TriggerBlueprints()
		{
			for (int i = 0; i < blueprints.Length; i++)
			{
				blueprints[i].Trigger();
			}
		}

		public override string ToString()
		{
			return $"OnGoalUnlock {goal} (blueprints {UWE.Utils.ToString(blueprints)}, signals {UWE.Utils.ToString(signals)}, items {UWE.Utils.ToString(items)})";
		}
	}
}
