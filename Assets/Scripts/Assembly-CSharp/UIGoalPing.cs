using UWE;
using UnityEngine;

public class UIGoalPing : MonoBehaviour
{
	private GoalObject _nearestGoalObject;

	private Goal _goal;

	private void Start()
	{
		GoalManager.main.onNewGoalEvent.AddHandler(base.gameObject, OnNewGoal);
		GoalManager.main.onCompleteGoalEvent.AddHandler(base.gameObject, OnCompleteGoal);
	}

	private void OnNewGoal(Goal goal)
	{
		_goal = goal;
	}

	private void OnCompleteGoal(Goal goal)
	{
		_goal = null;
		_nearestGoalObject = null;
	}

	private void Update()
	{
		if (_goal != null && _goal.showDirection)
		{
			_nearestGoalObject = GoalManager.main.GetClosestGoalObject(_goal);
			Vector3 zero = Vector3.zero;
			zero = ((!(_nearestGoalObject != null)) ? _goal.fallBackGoalPosition : _nearestGoalObject.transform.position);
			if (zero != Vector3.zero)
			{
				GetComponent<Renderer>().enabled = true;
				Vector3 vector = Vector3.Normalize(zero - Player.main.viewModelCamera.transform.position);
				new Vector3(UWE.Utils.GetPitchDegFromVector(vector), UWE.Utils.GetYawDegFromVector(vector), 0f);
				base.transform.position = Player.main.viewModelCamera.transform.position + vector * 0.5f;
			}
			else
			{
				GetComponent<Renderer>().enabled = false;
			}
		}
		else
		{
			GetComponent<Renderer>().enabled = false;
		}
	}
}
