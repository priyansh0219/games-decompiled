using UnityEngine;

public class School : MonoBehaviour
{
	private Vector3 _startPosition;

	private Vector3 _targetPosition;

	private void Start()
	{
		_startPosition = base.transform.position;
		_startPosition.y = Mathf.Min(_startPosition.y, -5f);
	}

	private void FixedUpdate()
	{
		if ((base.transform.position - _targetPosition).sqrMagnitude <= 2f)
		{
			_targetPosition = Random.onUnitSphere * 5f + _startPosition;
		}
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(_targetPosition - base.transform.position), Time.deltaTime * 0.8f);
		base.transform.position += Time.deltaTime * 1.7f * base.transform.forward;
	}
}
