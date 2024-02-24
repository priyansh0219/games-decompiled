using UnityEngine;

public class WarpOut : CreatureAction, IOnTakeDamage
{
	[AssertNotNull]
	public Warper warper;

	[AssertNotNull]
	public LastTarget lastTarget;

	public float damageThreshold = 10f;

	public float maxSwimTime = 30f;

	public float maxSearchTime = 10f;

	private float accumulatedDamage;

	private float spawnedTime;

	private void Start()
	{
		spawnedTime = Time.time;
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (accumulatedDamage >= damageThreshold || time > spawnedTime + maxSwimTime || time > Mathf.Max(spawnedTime, lastTarget.targetTime) + maxSearchTime)
		{
			return evaluatePriority;
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		warper.WarpOut();
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		float num = damageInfo.damage;
		if (damageInfo.type == DamageType.Electrical)
		{
			num *= 35f;
		}
		accumulatedDamage += num;
		if (accumulatedDamage >= damageThreshold)
		{
			creature.TryStartAction(this);
		}
	}
}
