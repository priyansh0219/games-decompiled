using UWE;
using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class Hide : CreatureAction
{
	public float searchDistance = 50f;

	public float searchInterval = 1f;

	public float swimVelocity = 1f;

	private Vector3 hideout;

	public override float Evaluate(Creature creature, float time)
	{
		return GetEvaluatePriority();
	}

	public override void StartPerform(Creature creature, float time)
	{
		Vector3 vector = Vector3.down + Random.insideUnitSphere;
		if (UWE.Utils.TraceForTerrain(new Ray(base.transform.position, vector.normalized), searchDistance, out var hitInfo))
		{
			hideout = hitInfo.point;
		}
		else
		{
			hideout = creature.leashPosition;
		}
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		base.swimBehaviour.SwimTo(hideout, swimVelocity);
	}

	public override void StopPerform(Creature creature, float time)
	{
		base.StopPerform(creature, time);
	}
}
