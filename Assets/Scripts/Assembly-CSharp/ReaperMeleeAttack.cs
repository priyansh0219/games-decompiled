using UnityEngine;

public class ReaperMeleeAttack : MeleeAttack
{
	[AssertNotNull]
	public ReaperLeviathan reaper;

	[AssertNotNull]
	public PlayerCinematicController playerDeathCinematic;

	public FMOD_CustomEmitter playerAttackSound;

	public float cyclopsDamage = 160f;

	public override bool CanEat(BehaviourType behaviourType, bool holdingByPlayer = false)
	{
		if (behaviourType != BehaviourType.Shark && behaviourType != BehaviourType.MediumFish)
		{
			return behaviourType == BehaviourType.SmallFish;
		}
		return true;
	}

	protected override float GetBiteDamage(GameObject target)
	{
		if (target.GetComponent<SubControl>() != null)
		{
			return cyclopsDamage;
		}
		return base.GetBiteDamage(target);
	}

	public override void OnTouch(Collider collider)
	{
		if (!liveMixin.IsAlive() || !(Time.time > timeLastBite + biteInterval) || !(reaper.Aggression.Value >= 0.5f))
		{
			return;
		}
		GameObject target = GetTarget(collider);
		if (reaper.IsHoldingVehicle() || playerDeathCinematic.IsCinematicModeActive())
		{
			return;
		}
		Player component = target.GetComponent<Player>();
		if (component != null)
		{
			if (component.CanBeAttacked() && !component.cinematicModeActive)
			{
				float num = DamageSystem.CalculateDamage(biteDamage, DamageType.Normal, component.gameObject);
				if (component.GetComponent<LiveMixin>().health - num <= 0f)
				{
					playerDeathCinematic.StartCinematicMode(component);
					if ((bool)playerAttackSound)
					{
						playerAttackSound.Play();
					}
					reaper.OnGrabPlayer();
				}
			}
		}
		else if (reaper.GetCanGrabVehicle())
		{
			SeaMoth component2 = target.GetComponent<SeaMoth>();
			if ((bool)component2 && !component2.docked)
			{
				reaper.GrabSeamoth(component2);
			}
			Exosuit component3 = target.GetComponent<Exosuit>();
			if ((bool)component3 && !component3.docked)
			{
				reaper.GrabExosuit(component3);
			}
		}
		base.OnTouch(collider);
		reaper.Aggression.Value -= 0.25f;
	}

	public void OnPlayerCinematicModeEnd(PlayerCinematicController sender)
	{
		if (sender == playerDeathCinematic)
		{
			reaper.OnReleasePlayer();
		}
	}
}
