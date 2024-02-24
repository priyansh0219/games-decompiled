using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class FleeOnDamage : CreatureAction, IOnTakeDamage
{
	private Vector3 moveTo;

	private float timeToFlee;

	private float accumulatedDamage;

	public float damageThreshold = 10f;

	public float fleeDuration = 2f;

	public float minFleeDistance = 5f;

	public bool breakLeash = true;

	public float swimVelocity = 10f;

	public float swimInterval = 1f;

	private Vector3 lastDamagePosition;

	private float timeNextSwim;

	public override float Evaluate(Creature creature, float time)
	{
		if (time < timeToFlee)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		creature.Scared.Add(1f);
		creature.Tired.Add(-1f);
		creature.Happy.Add(-1f);
	}

	public override void StopPerform(Creature creature, float time)
	{
		accumulatedDamage = 0f;
		if (breakLeash)
		{
			creature.leashPosition = base.transform.position;
		}
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time > timeNextSwim)
		{
			timeNextSwim = time + swimInterval;
			Vector3 position = base.transform.position;
			Vector3 vector = Vector3.Normalize(position - lastDamagePosition);
			Vector3 vector2 = position + vector * (minFleeDistance + accumulatedDamage / 30f);
			moveTo = new Vector3(vector2.x, Mathf.Min(vector2.y, Ocean.GetOceanLevel()), vector2.z);
			base.swimBehaviour.SwimTo(moveTo, swimVelocity);
		}
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		if (base.enabled)
		{
			float num = damageInfo.damage;
			if (damageInfo.type == DamageType.Electrical)
			{
				num *= 35f;
			}
			accumulatedDamage += num;
			lastDamagePosition = damageInfo.position;
			if (accumulatedDamage > damageThreshold)
			{
				timeToFlee = Time.time + fleeDuration;
				creature.Scared.Add(1f);
				creature.TryStartAction(this);
			}
		}
	}
}
