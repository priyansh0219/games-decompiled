using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class StayAtLeashPosition : CreatureAction
{
	public float leashDistance = 15f;

	public Vector3 directionDistanceMultiplier = Vector3.one;

	public float swimVelocity = 3f;

	public float swimInterval = 1f;

	public float minSwimDuration = 3f;

	public static bool debugShortLeash;

	private float timeNextSwim;

	private float timeStartSwim = -1f;

	public override float Evaluate(Creature creature, float time)
	{
		if (debugShortLeash)
		{
			return float.MaxValue;
		}
		if (timeStartSwim > 0f && time < timeStartSwim + minSwimDuration)
		{
			return GetEvaluatePriority();
		}
		float magnitude = Vector3.Scale(creature.leashPosition - base.transform.position, directionDistanceMultiplier).magnitude;
		if (creature != null && magnitude > leashDistance)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time > timeNextSwim)
		{
			timeNextSwim = time + swimInterval;
			base.swimBehaviour.SwimTo(creature.leashPosition, swimVelocity);
			if (debugShortLeash)
			{
				Debug.DrawLine(base.transform.position, creature.leashPosition, Color.yellow);
			}
		}
	}

	public override void StartPerform(Creature creature, float time)
	{
		timeStartSwim = time;
	}

	public override void StopPerform(Creature creature, float time)
	{
		timeStartSwim = -1f;
	}

	private void OnDrawGizmos()
	{
		Vector3 pos = base.transform.position;
		if (Application.isPlaying && (bool)creature)
		{
			pos = creature.leashPosition;
		}
		Vector3 vector = directionDistanceMultiplier;
		Gizmos.matrix = Matrix4x4.TRS(pos, base.transform.rotation, new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z));
		Gizmos.color = Color.yellow.ToAlpha(0.5f);
		Gizmos.DrawWireSphere(Vector3.zero, leashDistance);
	}
}
