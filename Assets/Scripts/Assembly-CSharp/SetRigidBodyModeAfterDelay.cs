using UnityEngine;

public class SetRigidBodyModeAfterDelay : MonoBehaviour
{
	public CollisionDetectionMode previousDetectionMode;

	private float setOffTime;

	private bool started;

	public void TriggerStart(float timeDelay, CollisionDetectionMode wantedDetectionMode)
	{
		previousDetectionMode = wantedDetectionMode;
		setOffTime = Time.time + timeDelay;
		started = true;
	}

	private void Update()
	{
		if (started && Time.time >= setOffTime)
		{
			Rigidbody component = GetComponent<Rigidbody>();
			if ((bool)component)
			{
				component.collisionDetectionMode = previousDetectionMode;
			}
			Object.Destroy(this);
		}
	}
}
