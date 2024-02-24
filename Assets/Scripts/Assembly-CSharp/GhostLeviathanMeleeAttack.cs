using UnityEngine;

public class GhostLeviathanMeleeAttack : MeleeAttack
{
	public float cyclopsDamage;

	protected override float GetBiteDamage(GameObject target)
	{
		if (target.GetComponent<SubControl>() != null)
		{
			return cyclopsDamage;
		}
		return base.GetBiteDamage(target);
	}
}
