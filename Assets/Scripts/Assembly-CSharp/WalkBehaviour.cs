using UnityEngine;

[RequireComponent(typeof(OnSurfaceMovement))]
public class WalkBehaviour : SwimBehaviour
{
	[AssertNotNull]
	public OnSurfaceTracker onSurfaceTracker;

	[AssertNotNull]
	public OnSurfaceMovement onSurfaceMovement;

	public bool allowSwimming;

	public override void Idle()
	{
		base.Idle();
		onSurfaceMovement.Idle();
	}

	public void WalkTo(Vector3 targetPosition, float velocity)
	{
		SwimTo(targetPosition, velocity);
	}

	protected override void GoToInternal(Vector3 targetPosition, Vector3 targetDirection, float velocity)
	{
		if (allowSwimming && !onSurfaceTracker.onSurface && base.transform.position.y < 0f)
		{
			onSurfaceMovement.enabled = false;
			splineFollowing.enabled = true;
			splineFollowing.GoTo(targetPosition, targetDirection, velocity);
		}
		else
		{
			splineFollowing.enabled = false;
			onSurfaceMovement.enabled = true;
			onSurfaceMovement.GoTo(targetPosition, velocity);
		}
	}
}
