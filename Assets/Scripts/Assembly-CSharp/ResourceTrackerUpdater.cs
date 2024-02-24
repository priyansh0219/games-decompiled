using UnityEngine;

public class ResourceTrackerUpdater : MonoBehaviour
{
	private ResourceTracker tracker;

	private Vector3 lastPosition;

	public void Start()
	{
		lastPosition = base.transform.position;
		tracker = GetComponent<ResourceTracker>();
		InvokeRepeating("CheckSettled", 10f, 10f);
	}

	private void CheckSettled()
	{
		if ((lastPosition - base.transform.position).sqrMagnitude < 0.001f)
		{
			Object.Destroy(this);
		}
		lastPosition = base.transform.position;
	}

	private void Update()
	{
		tracker.UpdatePosition();
	}
}
