using UnityEngine;

public class AvoidEscapePod : CreatureAction
{
	public float swimVelocity = 5f;

	public float maxDistanceToPod = 10f;

	public float swimInterval = 2f;

	private float timeNextSwim;

	public override float Evaluate(Creature creature, float time)
	{
		if (EscapePod.main != null && Vector3.Distance(base.transform.position, EscapePod.main.transform.position) < maxDistanceToPod)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StopPerform(Creature creature, float time)
	{
		if (EscapePod.main != null)
		{
			Vector3 position = EscapePod.main.transform.position;
			if (Vector3.Distance(position, base.transform.position) > Vector3.Distance(position, creature.leashPosition))
			{
				creature.leashPosition = base.transform.position;
			}
		}
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time > timeNextSwim && EscapePod.main != null)
		{
			Vector3 vector = base.transform.position - EscapePod.main.transform.position;
			vector.y = 0f;
			Vector3 targetPosition = base.transform.position + vector.normalized * maxDistanceToPod;
			base.swimBehaviour.SwimTo(targetPosition, swimVelocity);
		}
	}
}
