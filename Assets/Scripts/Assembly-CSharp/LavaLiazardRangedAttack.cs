using UnityEngine;

public class LavaLiazardRangedAttack : RangedAttackLastTarget
{
	[AssertNotNull]
	public LavaShell lavaShell;

	public float lavaArmorCost = 10f;

	public override float Evaluate(Creature creature, float time)
	{
		if (lavaShell.IsEnabled())
		{
			return Mathf.Lerp(0f, base.Evaluate(creature, time), lavaShell.GetArmorFraction());
		}
		return 0f;
	}

	protected override void Cast(RangedAttackType attackType, Vector3 directionToTarget)
	{
		base.Cast(attackType, directionToTarget);
		lavaShell.TakeDamage(lavaArmorCost);
	}
}
