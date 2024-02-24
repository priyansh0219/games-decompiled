using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class AvoidTerrain : CreatureAction
{
	[Range(0f, 1f)]
	public float avoidanceForward = 0.5f;

	public float avoidanceIterations = 10f;

	public float avoidanceDistance = 50f;

	public float avoidanceDuration = 2f;

	public float scanInterval = 1f;

	public float scanDistance = 30f;

	public float swimVelocity = 15f;

	public float swimInterval = 1f;

	private Vector3 avoidancePosition;

	private float timeStartAvoidance;

	private float timeNextScan;

	private float timeNextSwim;

	public override float Evaluate(Creature creature, float time)
	{
		if (time < timeStartAvoidance + avoidanceDuration)
		{
			return GetEvaluatePriority();
		}
		if (time > timeNextScan)
		{
			timeNextScan = time + scanInterval;
			Transform transform = creature.transform;
			if (OctreeRaycast(transform.position, transform.forward, scanDistance))
			{
				return GetEvaluatePriority();
			}
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		Vector3 vector = (avoidancePosition = creature.transform.position);
		timeStartAvoidance = time;
		for (int i = 0; (float)i < avoidanceIterations; i++)
		{
			Vector3 onUnitSphere = Random.onUnitSphere;
			onUnitSphere += base.transform.forward * avoidanceForward;
			if (!OctreeRaycastSkipCurrent(vector, onUnitSphere, avoidanceDistance))
			{
				avoidancePosition = vector + onUnitSphere * avoidanceDistance;
				return;
			}
		}
		timeStartAvoidance = 0f;
	}

	public override void StopPerform(Creature creature, float time)
	{
		timeStartAvoidance = 0f;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time > timeNextSwim)
		{
			timeNextSwim = time + swimInterval;
			base.swimBehaviour.SwimTo(avoidancePosition, swimVelocity);
		}
	}

	private static bool OctreeRaycastSkipCurrent(Vector3 origin, Vector3 direction, float maxDistance)
	{
		LargeWorldStreamer main = LargeWorldStreamer.main;
		if (!main)
		{
			return false;
		}
		int blocksPerTree = main.blocksPerTree;
		Vector3 startPoint = origin + direction * blocksPerTree;
		Vector3 endPoint = origin + direction * Mathf.Max(maxDistance, blocksPerTree + 1);
		return OctreeRaycast(startPoint, endPoint);
	}

	private static bool OctreeRaycast(Vector3 origin, Vector3 direction, float maxDistance)
	{
		Vector3 endPoint = origin + direction * maxDistance;
		return OctreeRaycast(origin, endPoint);
	}

	private static bool OctreeRaycast(Vector3 startPoint, Vector3 endPoint)
	{
		LargeWorldStreamer main = LargeWorldStreamer.main;
		if (!main)
		{
			return false;
		}
		if (main.OctreeRaycast(startPoint, endPoint, out var _))
		{
			Debug.DrawLine(startPoint, endPoint, Color.red, 1f, depthTest: true);
			return true;
		}
		Debug.DrawLine(startPoint, endPoint, Color.white, 1f, depthTest: true);
		return false;
	}
}
