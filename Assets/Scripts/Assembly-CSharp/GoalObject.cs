using UnityEngine;

public class GoalObject : MonoBehaviour
{
	public TechType type;

	public string customGoal;

	public float findRadius = 7f;

	private void Awake()
	{
	}

	private void Start()
	{
		GoalManager.main.RegisterGoalObject(this);
	}

	private void OnDestroy()
	{
		GoalManager.main.UnregisterGoalObject(this);
	}

	public string GetPickupName()
	{
		if (type != 0)
		{
			return type.AsString();
		}
		return "";
	}

	public string GetCustomGoalName()
	{
		return customGoal;
	}
}
