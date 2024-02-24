using UnityEngine;

public class ShockerMeleeAttack : MeleeAttack
{
	public float electricalDamage = 15f;

	public float cyclopsDamage = 50f;

	public float electricalDamageInterval = 2f;

	public FMOD_StudioEventEmitter electricalAttackSound;

	private float timeLastElectricalDamage;

	public void OnMouthTouch(Collider collider)
	{
		OnTouch(collider);
		base.OnTouch(collider);
	}

	public override void OnTouch(Collider collider)
	{
		if (!base.enabled || frozen || Time.time < timeLastElectricalDamage + electricalDamageInterval || !liveMixin.IsAlive())
		{
			return;
		}
		GameObject target = GetTarget(collider);
		Player component = target.GetComponent<Player>();
		if (component != null && !component.CanBeAttacked())
		{
			return;
		}
		LiveMixin component2 = target.GetComponent<LiveMixin>();
		if (!(component2 != null) || !component2.IsAlive())
		{
			return;
		}
		bool flag = target.GetComponent<SubControl>() != null;
		if (!flag || !(target != lastTarget.target))
		{
			float originalDamage = (flag ? cyclopsDamage : electricalDamage);
			component2.TakeDamage(originalDamage, default(Vector3), DamageType.Electrical);
			component2.NotifyCreatureDeathsOfCreatureAttack();
			base.gameObject.SendMessage("OnMeleeAttack", component2.gameObject, SendMessageOptions.DontRequireReceiver);
			timeLastElectricalDamage = Time.time;
			if (electricalAttackSound != null)
			{
				Utils.PlayEnvSound(electricalAttackSound, target.transform.position);
			}
		}
	}
}
