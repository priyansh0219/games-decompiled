using UnityEngine;

public class SeaDragonAggressiveTowardsSharks : AggressiveWhenSeeTarget
{
	[AssertNotNull]
	public SeaDragon seaDragon;

	public float playerAttackInterval = 10f;

	private float timeLastPlayerAttack;

	protected override GameObject GetAggressionTarget()
	{
		if (Time.time > timeLastPlayerAttack + playerAttackInterval && IsTargetValid(Player.main.gameObject))
		{
			return Player.main.gameObject;
		}
		return base.GetAggressionTarget();
	}

	public void OnMeleeAttack(GameObject target)
	{
		if (target == Player.main.gameObject)
		{
			timeLastPlayerAttack = Time.time;
		}
	}
}
