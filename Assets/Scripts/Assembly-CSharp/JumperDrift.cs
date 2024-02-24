using UnityEngine;

public class JumperDrift : CreatureAction
{
	[AssertNotNull]
	public Jumper jumper;

	public float swimVelocity = 3f;

	public float swimInterval = 1f;

	public float maxDriftTime = 10f;

	public float driftInterval = 10f;

	public float driftDistance = 10f;

	public float jumpHeight = 3f;

	private Vector3 swimTarget;

	private float timeNextSwim;

	public float timeLastDrift { get; private set; }

	private void Start()
	{
		timeLastDrift = Time.time - Random.value * driftInterval;
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (jumper.IsWalking())
		{
			if (time > timeLastDrift + driftInterval)
			{
				return GetEvaluatePriority();
			}
			return 0f;
		}
		if (jumper.state == Jumper.State.Drift)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		swimTarget = base.transform.position + base.transform.forward * driftDistance;
		swimTarget.y = base.transform.position.y + jumpHeight;
		timeNextSwim = time;
		timeLastDrift = time;
		jumper.state = Jumper.State.Drift;
	}

	public override void StopPerform(Creature creature, float time)
	{
		timeLastDrift = time;
		jumper.state = Jumper.State.Swim;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time >= timeNextSwim)
		{
			base.swimBehaviour.SwimTo(swimTarget, swimVelocity);
			timeNextSwim = time + swimInterval;
		}
	}

	public bool IsTargetReached()
	{
		if (!(Time.time > timeLastDrift + maxDriftTime))
		{
			return (base.transform.position - swimTarget).sqrMagnitude < 9f;
		}
		return true;
	}
}
