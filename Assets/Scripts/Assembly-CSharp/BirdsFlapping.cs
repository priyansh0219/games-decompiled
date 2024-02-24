using UnityEngine;

public class BirdsFlapping : CreatureAction
{
	public float flyVelocity = 3f;

	public float flyInterval = 1f;

	public float flyUp = 0.3f;

	public float flappingDuration = 3f;

	public float flappingInterval = 5f;

	public Animator animator;

	private float timeNextFly;

	private bool flapping;

	private float timeFlappingStart;

	private float timeLastFlapping;

	public override float Evaluate(Creature creature, float time)
	{
		if (time > timeLastFlapping + flappingInterval)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		timeFlappingStart = time;
		SafeAnimator.SetBool(animator, "flapping", value: true);
	}

	public override void StopPerform(Creature creature, float time)
	{
		SafeAnimator.SetBool(animator, "flapping", value: false);
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
			if (time > timeFlappingStart + flappingDuration)
			{
				timeLastFlapping = time;
			}
		}
	}
}
