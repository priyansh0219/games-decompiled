using System;
using System.Collections.Generic;
using UWE;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
	public static GoalManager main;

	[NonSerialized]
	public Event<Goal> onNewGoalEvent = new Event<Goal>();

	[NonSerialized]
	public Event<Goal> onCompleteGoalEvent = new Event<Goal>();

	private List<Goal> goals = new List<Goal>();

	private List<GoalObject> goalObjects = new List<GoalObject>();

	private List<string> completedGoalNames = new List<string>();

	private bool activeState;

	private void Awake()
	{
		main = this;
	}

	private Goal AddGoal(GoalType goalType, TechType itemType, bool showDirection, string customGoalDisplayText = "")
	{
		Goal goal = new Goal();
		goal.goalType = goalType;
		goal.itemType = itemType;
		goal.customGoalDisplayText = customGoalDisplayText;
		goal.showDirection = showDirection;
		goals.Add(goal);
		return goal;
	}

	private Goal AddGoal(GoalType goalType, TechType itemType, string customGoalDisplayText = "")
	{
		return AddGoal(goalType, itemType, showDirection: false, customGoalDisplayText);
	}

	public Goal AddCustomGoal(string customGoalName, string customGoalDisplayText, string gameObjectName = "", bool showDirection = false)
	{
		Goal goal = new Goal();
		goal.goalType = GoalType.Custom;
		goal.customGoalName = customGoalName;
		goal.customGoalDisplayText = customGoalDisplayText;
		goal.showDirection = showDirection;
		goal.gameObjectName = gameObjectName;
		goals.Add(goal);
		return goal;
	}

	private void Start()
	{
		ItemsContainer container = Inventory.main.container;
		if (container != null)
		{
			container.onAddItem += OnPickupEvent;
		}
		InvokeRepeating("UpdateFindGoal", 1f, 3f);
	}

	private void OnDestroy()
	{
		ItemsContainer container = Inventory.main.container;
		if (container != null)
		{
			container.onAddItem -= OnPickupEvent;
		}
	}

	public void UnregisterGoalObject(GoalObject goalObject)
	{
		goalObjects.Remove(goalObject);
	}

	public void RegisterGoalObject(GoalObject goalObject)
	{
		goalObjects.Add(goalObject);
	}

	public GoalObject GetClosestGoalObject(Goal goal)
	{
		Vector3 position = GameObject.FindGameObjectWithTag("Player").transform.position;
		GoalObject result = null;
		float num = 1000000f;
		for (int i = 0; i < goalObjects.Count; i++)
		{
			GoalObject goalObject = goalObjects[i];
			if (goalObject.gameObject.activeInHierarchy && ((goal.goalType != GoalType.Custom && goalObject.type == goal.itemType) || goal.gameObjectName == goalObject.gameObject.name))
			{
				float sqrMagnitude = (position - goalObject.transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = goalObject;
				}
			}
		}
		return result;
	}

	public void OnCraftEvent(string recipeName)
	{
		OnCompleteGoal(GoalType.Craft, recipeName);
	}

	public void OnPickupEvent(InventoryItem item)
	{
		Pickupable item2 = item.item;
		OnCompleteGoal(GoalType.Gather, item2.GetTechName());
	}

	public void OnCustomGoalEvent(string customGoalText)
	{
		OnCompleteGoal(GoalType.Custom, customGoalText);
	}

	public void OnCompleteGoal(GoalType goalType, string goalIdentifier)
	{
		if (completedGoalNames.Contains(goalIdentifier))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < goals.Count; i++)
		{
			Goal goal = goals[i];
			if (goal.goalType == goalType && !goal.IsCompleted() && ((goalType == GoalType.Custom && goalIdentifier == goal.customGoalName) || goal.itemType.AsString() == goalIdentifier))
			{
				goal.SetTimeCompleted(Time.time);
				onCompleteGoalEvent.Trigger(goal);
				completedGoalNames.Add(goalIdentifier);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			completedGoalNames.Add(goalIdentifier);
		}
	}

	private void ChooseNewGoal()
	{
		int num = 0;
		for (int i = 0; i < goals.Count; i++)
		{
			if (goals[i].IsDisplayed() && !goals[i].IsCompleted())
			{
				num++;
			}
		}
		if (num != 0)
		{
			return;
		}
		for (int j = 0; j < goals.Count; j++)
		{
			Goal goal = goals[j];
			if (!goal.IsCompleted())
			{
				onNewGoalEvent.Trigger(goal);
				break;
			}
		}
	}

	private void SetActiveState(bool newActiveState)
	{
		activeState = newActiveState;
	}

	private void UpdateFindGoal()
	{
		for (int i = 0; i < goalObjects.Count; i++)
		{
			GoalObject goalObject = goalObjects[i];
			float sqrMagnitude = (Player.main.transform.position - goalObject.transform.position).sqrMagnitude;
			float findRadius = goalObject.findRadius;
			if (sqrMagnitude < findRadius * findRadius)
			{
				if (goalObject.GetPickupName() != "")
				{
					OnCompleteGoal(GoalType.Find, goalObject.GetPickupName());
				}
				else
				{
					OnCompleteGoal(GoalType.Custom, "Find_" + goalObject.GetCustomGoalName());
				}
			}
		}
	}

	private void Update()
	{
		UpdateFindGoal();
		if (activeState)
		{
			ChooseNewGoal();
		}
	}
}
