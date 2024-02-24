using UWE;
using UnityEngine;

public class DirectionIndicator : MonoBehaviour
{
	public GameObject _model;

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
				_model.SetActive(value: true);
				Vector3 v = Vector3.Normalize(zero - Player.main.viewModelCamera.transform.position);
				Vector3 euler = new Vector3(UWE.Utils.GetPitchDegFromVector(v), UWE.Utils.GetYawDegFromVector(v), 0f);
				base.transform.rotation = Quaternion.Euler(euler);
			}
			else
			{
				_model.SetActive(value: false);
			}
		}
		else
		{
			_model.SetActive(value: false);
		}
	}
}
