using System;
using UnityEngine;

[Serializable]
public class Goal
{
	public GoalType goalType;

	public TechType itemType;

	public string customGoalName = "";

	public string customGoalDisplayText = "";

	public bool showDirection;

	public Vector3 fallBackGoalPosition;

	public string gameObjectName = "";

	private bool displayed;

	private float timeCompleted = -1f;

	public string GetGoalDescription()
	{
		string result = goalType.ToString() + " " + itemType.AsString(lowercase: true);
		if (customGoalDisplayText != "")
		{
			result = customGoalDisplayText;
		}
		return result;
	}

	public float GetTimeCompleted()
	{
		return timeCompleted;
	}

	public bool IsDisplayed()
	{
		return displayed;
	}

	public void SetIsDisplayed(bool newDisplayState)
	{
		displayed = newDisplayState;
	}

	public bool IsCompleted()
	{
		return timeCompleted != -1f;
	}

	public void SetTimeCompleted(float t)
	{
		timeCompleted = t;
	}

	public override string ToString()
	{
		return $"type {goalType}, item {itemType}, name '{customGoalName}', text '{customGoalDisplayText}', direction {showDirection}, position {fallBackGoalPosition}, object '{gameObjectName}', displayed {displayed}, completed {timeCompleted}";
	}
}
