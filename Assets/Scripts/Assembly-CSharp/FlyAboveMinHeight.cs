using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class FlyAboveMinHeight : CreatureAction
{
	public float minHeight = 2f;

	public float flyVelocity = 3f;

	public float flyInterval = 1f;

	public float flyUp = 0.3f;

	private float timeNextFly;

	public override float Evaluate(Creature creature, float time)
	{
		if (base.transform.position.y < minHeight)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		SafeAnimator.SetBool(creature.GetAnimator(), "flapping", value: true);
	}

	public override void StopPerform(Creature creature, float time)
	{
		SafeAnimator.SetBool(creature.GetAnimator(), "flapping", value: false);
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time > timeNextFly)
		{
			timeNextFly = time + flyInterval;
			Vector3 normalized = new Vector3(base.transform.forward.x, 0f, base.transform.forward.z).normalized;
			normalized.y = flyUp;
			Vector3 targetPosition = base.transform.position + normalized * 10f;
			base.swimBehaviour.SwimTo(targetPosition, flyVelocity);
		}
	}
}
