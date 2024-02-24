using UnityEngine;

public class MoveOnSurface : CreatureAction
{
	[AssertNotNull]
	public OnSurfaceTracker onSurfaceTracker;

	[AssertNotNull]
	public WalkBehaviour walkBehaviour;

	public float updateTargetInterval = 5f;

	public float moveVelocity = 13f;

	public float moveRadius = 7f;

	private float timeNextTarget;

	private Vector3 desiredPosition = Vector3.zero;

	private bool actionActive;

	private Vector3 FindRandomPosition()
	{
		return Random.onUnitSphere * moveRadius + base.transform.position;
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (onSurfaceTracker.onSurface)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (timeNextTarget <= time)
		{
			desiredPosition = FindRandomPosition();
			timeNextTarget = time + updateTargetInterval + 6f * Random.value;
			walkBehaviour.WalkTo(desiredPosition, moveVelocity);
		}
	}
}
