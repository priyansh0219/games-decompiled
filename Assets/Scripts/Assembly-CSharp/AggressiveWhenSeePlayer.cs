using UnityEngine;

public class AggressiveWhenSeePlayer : AggressiveWhenSeeTarget
{
	public float playerAttackInterval = 10f;

	private float timeLastPlayerAttack;

	protected override GameObject GetAggressionTarget()
	{
		if (Time.time > timeLastPlayerAttack + playerAttackInterval && IsTargetValid(Player.main.gameObject))
		{
			return Player.main.gameObject;
		}
		if (targetType != 0)
		{
			return base.GetAggressionTarget();
		}
		return null;
	}

	public void OnMeleeAttack(GameObject target)
	{
		if (target == Player.main.gameObject)
		{
			timeLastPlayerAttack = Time.time;
		}
	}
}
