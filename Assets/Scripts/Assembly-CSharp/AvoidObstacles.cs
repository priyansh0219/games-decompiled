using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class AvoidObstacles : CreatureAction
{
	public LastTarget lastTarget;

	public bool avoidTerrainOnly = true;

	public float avoidanceIterations = 10f;

	public float avoidanceDistance = 5f;

	public float avoidanceDuration = 2f;

	public float scanInterval = 1f;

	public float scanDistance = 2f;

	public float scanRadius;

	public float swimVelocity = 3f;

	public float swimInterval = 1f;

	private Vector3 avoidancePosition;

	private float timeStartAvoidance;

	private float timeNextScan;

	private bool swimDirectionFound;

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
			bool flag = false;
			RaycastHit hitInfo;
			if (scanRadius > 0f)
			{
				if (Physics.SphereCast(transform.position, scanRadius, transform.forward, out hitInfo, scanDistance, GetLayerMask(), QueryTriggerInteraction.Ignore))
				{
					flag = IsObstacle(hitInfo.collider);
				}
			}
			else if (Physics.Raycast(transform.position, transform.forward, out hitInfo, scanDistance, GetLayerMask(), QueryTriggerInteraction.Ignore))
			{
				flag = IsObstacle(hitInfo.collider);
			}
			if (flag)
			{
				swimDirectionFound = false;
				return GetEvaluatePriority();
			}
		}
		return 0f;
	}

	public override void StopPerform(Creature creature, float time)
	{
		timeStartAvoidance = 0f;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time > timeNextSwim)
		{
			if (!swimDirectionFound)
			{
				FindSwimDirection();
			}
			timeNextSwim = time + swimInterval;
			float velocity = Mathf.Lerp(swimVelocity, 0f, creature.Tired.Value);
			base.swimBehaviour.SwimTo(avoidancePosition, velocity);
		}
	}

	private void FindSwimDirection()
	{
		Vector3 vector = (avoidancePosition = creature.transform.position);
		timeStartAvoidance = Time.time;
		swimDirectionFound = false;
		int layerMask = GetLayerMask();
		for (int i = 0; (float)i < avoidanceIterations; i++)
		{
			Vector3 onUnitSphere = Random.onUnitSphere;
			if (!Physics.Raycast(vector, onUnitSphere, out var hitInfo, avoidanceDistance, layerMask, QueryTriggerInteraction.Ignore) || !IsObstacle(hitInfo.collider))
			{
				avoidancePosition = vector + onUnitSphere * avoidanceDistance;
				swimDirectionFound = true;
				return;
			}
		}
		timeStartAvoidance = 0f;
	}

	private int GetLayerMask()
	{
		if (!avoidTerrainOnly)
		{
			return -5;
		}
		return Voxeland.GetTerrainLayerMask();
	}

	protected virtual bool IsObstacle(Collider collider)
	{
		GameObject gameObject = ((lastTarget != null) ? lastTarget.target : null);
		if (!avoidTerrainOnly && gameObject != null)
		{
			Rigidbody attachedRigidbody = collider.attachedRigidbody;
			if (((attachedRigidbody != null) ? attachedRigidbody.gameObject : collider.gameObject) == gameObject)
			{
				return false;
			}
		}
		return true;
	}

	private void OnDrawGizmosSelected()
	{
		if (base.enabled)
		{
			Transform transform = creature.transform;
			Vector3 position = transform.position;
			bool flag = false;
			if (Physics.Raycast(position, transform.forward, out var hitInfo, scanDistance, GetLayerMask(), QueryTriggerInteraction.Ignore))
			{
				flag = IsObstacle(hitInfo.collider);
			}
			Gizmos.color = (flag ? Color.red : Color.green);
			Gizmos.DrawLine(position, position + transform.forward * scanDistance);
		}
	}
}
