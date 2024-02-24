using ProtoBuf;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(SwimBehaviour))]
public class Breach : CreatureAction
{
	public float swimVelocity = 10f;

	public float swimInterval = 5f;

	public float swimRadius = 10f;

	public float breachHeight = 2f;

	public float breachInterval = 100f;

	public float breachDuration = 5f;

	public float maxDepth = 10f;

	private bool isBreaching;

	private bool hasBreached;

	private float timeNextBreach;

	private float timeStopBreach;

	private float timeNextSwim;

	private void Start()
	{
		timeNextBreach = Time.time + Random.value * breachInterval;
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (base.transform.position.y < 0f - maxDepth)
		{
			return 0f;
		}
		if (isBreaching && hasBreached && time > timeStopBreach)
		{
			return 0f;
		}
		if (!isBreaching && time < timeNextBreach)
		{
			return 0f;
		}
		return GetEvaluatePriority();
	}

	public override void StartPerform(Creature creature, float time)
	{
		isBreaching = true;
		hasBreached = false;
		creature.Happy.Add(1f);
	}

	public override void StopPerform(Creature creature, float time)
	{
		isBreaching = false;
		timeNextBreach = time + breachInterval;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		Vector3 position = base.transform.position;
		if (!hasBreached && position.y > 0f)
		{
			hasBreached = true;
			timeStopBreach = time + breachDuration;
		}
		if (time > timeNextSwim)
		{
			timeNextSwim = time + swimInterval;
			Vector3 targetPosition = position + Random.insideUnitSphere * swimRadius;
			targetPosition.y = breachHeight;
			base.swimBehaviour.SwimTo(targetPosition, swimVelocity);
		}
	}
}
