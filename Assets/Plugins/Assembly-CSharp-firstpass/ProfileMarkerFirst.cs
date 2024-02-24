using UnityEngine;

public class ProfileMarkerFirst : MonoBehaviour
{
	private void Update()
	{
		StopwatchProfiler.Instance.NotifyFirstUpdate();
	}

	private void LateUpdate()
	{
		StopwatchProfiler.Instance.NotifyFirstLateUpdate();
	}

	private void FixedUpdate()
	{
		StopwatchProfiler.Instance.NotifyFirstFixedUpdate();
	}
}
