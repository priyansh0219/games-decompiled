using UnityEngine;

public class OnSurfaceMovement : MonoBehaviour
{
	private Vector3 targetPosition;

	private float currentVelocity;

	[SerializeField]
	[AssertNotNull]
	private OnSurfaceTracker onSurfaceTracker;

	[SerializeField]
	[AssertNotNull]
	private Locomotion locomotion;

	[SerializeField]
	private float targetRange = 1f;

	private bool driving;

	public void Start()
	{
		driving = false;
	}

	public void Idle()
	{
		driving = false;
		locomotion.Idle();
	}

	public void GoTo(Vector3 targetPos, float velocity)
	{
		targetPosition = targetPos;
		currentVelocity = velocity;
		driving = true;
		locomotion.maxVelocity = velocity;
	}

	public void Update()
	{
		if (!driving)
		{
			return;
		}
		if (!onSurfaceTracker.onSurface)
		{
			locomotion.Idle();
			return;
		}
		_ = base.transform.position;
		Vector3 vector = targetPosition - base.transform.position;
		vector = Vector3.ProjectOnPlane(vector, onSurfaceTracker.surfaceNormal);
		if (vector.sqrMagnitude < targetRange * targetRange)
		{
			Idle();
		}
		else
		{
			locomotion.ApplyVelocity(currentVelocity * vector.normalized);
		}
	}
}
