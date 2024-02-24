using UnityEngine;

public class WarperLastTarget : LastTarget
{
	[AssertNotNull]
	public AttackLastTarget attackLastTarget;

	[AssertNotNull]
	public RangedAttackLastTarget rangedAttack;

	public AnimationCurve attackPause;

	public AnimationCurve rangedAttackPause;

	public AnimationCurve attackSwimVelocity;

	protected override void SetTargetInternal(GameObject newTarget)
	{
		base.SetTargetInternal(newTarget);
		if (base.target != null)
		{
			InfectedMixin component = base.target.GetComponent<InfectedMixin>();
			float time = ((component != null) ? component.GetInfectedAmount() : 0f);
			attackLastTarget.pauseInterval = attackPause.Evaluate(time);
			attackLastTarget.swimVelocity = attackSwimVelocity.Evaluate(time);
			rangedAttack.pauseInterval = rangedAttackPause.Evaluate(time);
		}
	}
}
