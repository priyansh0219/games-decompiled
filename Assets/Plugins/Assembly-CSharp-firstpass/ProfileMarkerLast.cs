using UnityEngine;

public class ProfileMarkerLast : MonoBehaviour
{
	private void Update()
	{
		StopwatchProfiler.Instance.NotifyLastUpdate();
	}

	private void LateUpdate()
	{
		StopwatchProfiler.Instance.NotifyLastLateUpdate();
	}

	private void FixedUpdate()
	{
		StopwatchProfiler.Instance.NotifyLastFixedUpdate();
	}
}
