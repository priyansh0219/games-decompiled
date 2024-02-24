using UnityEngine;

public class CrawlerAvoidEdges : CreatureAction
{
	[AssertNotNull]
	public OnSurfaceTracker onSurfaceTracker;

	[AssertNotNull]
	public WalkBehaviour walkBehaviour;

	[AssertNotNull]
	public Rigidbody rgbody;

	public Vector3 scanOffset = new Vector3(0f, 2f, 0f);

	public Vector3 scanDirection = new Vector3(0f, -2f, 5f);

	public float scanDistance = 15f;

	public float scanInterval = 1f;

	public float walkInterval = 1f;

	public float avoidanceDuration = 2f;

	public float avoidanceDistance = 5f;

	public float moveVelocity = 10f;

	private Vector3 avoidancePosition;

	private float timeStartAvoidance;

	private float timeNextScan;

	private float timeNextWalk;

	public override float Evaluate(Creature creature, float time)
	{
		if (time < timeStartAvoidance + avoidanceDuration)
		{
			return GetEvaluatePriority();
		}
		if (!onSurfaceTracker.onSurface)
		{
			return 0f;
		}
		if (time < timeNextScan)
		{
			return 0f;
		}
		timeNextScan = time + scanInterval;
		Vector3 origin = base.transform.TransformPoint(scanOffset);
		Vector3 surfaceNormal = onSurfaceTracker.surfaceNormal;
		Vector3 normalized = Vector3.ProjectOnPlane(rgbody.velocity, surfaceNormal).normalized;
		Vector3 direction = scanDirection.y * surfaceNormal + scanDirection.z * normalized;
		if (Physics.Raycast(origin, direction, scanDistance))
		{
			return 0f;
		}
		avoidancePosition = base.transform.position - normalized * avoidanceDistance;
		return GetEvaluatePriority();
	}

	public override void StartPerform(Creature creature, float time)
	{
		timeStartAvoidance = time;
		walkBehaviour.WalkTo(avoidancePosition, moveVelocity);
	}
}
