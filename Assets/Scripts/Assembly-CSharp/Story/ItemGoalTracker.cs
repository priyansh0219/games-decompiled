using System.Collections.Generic;
using UnityEngine;

namespace Story
{
	public class ItemGoalTracker : MonoBehaviour, ICompileTimeCheckable
	{
		private static ItemGoalTracker main;

		[AssertNotNull]
		public ItemGoalData goalData;

		private readonly Dictionary<TechType, List<ItemGoal>> goals = new Dictionary<TechType, List<ItemGoal>>(TechTypeExtensions.sTechTypeComparer);

		public static void OnConstruct(TechType techType)
		{
			if ((bool)main)
			{
				main.TriggerGoal(techType);
			}
		}

		private void Start()
		{
			main = this;
			Inventory.main.container.onAddItem += OnInventoryAddItem;
			Inventory.main.equipment.onAddItem += OnInventoryAddItem;
			for (int i = 0; i < goalData.goals.Length; i++)
			{
				ItemGoal itemGoal = goalData.goals[i];
				goals.GetOrAddNew(itemGoal.techType).Add(itemGoal);
			}
		}

		private void OnDestroy()
		{
			Inventory.main.container.onAddItem -= OnInventoryAddItem;
			Inventory.main.equipment.onAddItem -= OnInventoryAddItem;
		}

		private void OnInventoryAddItem(InventoryItem item)
		{
			if (item != null && !(item.item == null))
			{
				TechType techType = item.item.GetTechType();
				TriggerGoal(techType);
			}
		}

		private void TriggerGoal(TechType techType)
		{
			if (goals.TryGetValue(techType, out var value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					value[i].Trigger();
				}
				goals.Remove(techType);
			}
		}

		public string CompileTimeCheck()
		{
			return StoryGoalUtils.CheckStoryGoals(goalData.goals);
		}
	}
}
