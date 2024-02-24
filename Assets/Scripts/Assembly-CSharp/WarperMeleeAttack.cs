using UnityEngine;

public class WarperMeleeAttack : MeleeAttack
{
	public AnimationCurve attackDamage;

	public override bool CanBite(GameObject target)
	{
		if (!base.CanBite(target))
		{
			return false;
		}
		Vehicle component = target.GetComponent<Vehicle>();
		if (component != null)
		{
			Player componentInChildren = component.playerPosition.GetComponentInChildren<Player>();
			if (componentInChildren == null)
			{
				return false;
			}
			target = componentInChildren.gameObject;
		}
		InfectedMixin component2 = target.GetComponent<InfectedMixin>();
		if (component2 != null)
		{
			return component2.infectedAmount > 0.33f;
		}
		return false;
	}

	protected override float GetBiteDamage(GameObject target)
	{
		Vehicle component = target.GetComponent<Vehicle>();
		if (component != null)
		{
			Player componentInChildren = component.playerPosition.GetComponentInChildren<Player>();
			if (componentInChildren != null)
			{
				target = componentInChildren.gameObject;
			}
		}
		InfectedMixin component2 = target.GetComponent<InfectedMixin>();
		float time = ((component2 != null) ? component2.GetInfectedAmount() : 0f);
		return attackDamage.Evaluate(time);
	}
}
