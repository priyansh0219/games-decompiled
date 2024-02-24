using UnityEngine;

public class CrabsnakeMeleeAttack : MeleeAttack
{
	public float cinematicAttackAdditionalDamage = 10f;

	public float seamothDamage = 15f;

	public PlayerCinematicController playerAttackCinematicController;

	public PlayerCinematicController playerKillCinematicController;

	public FMOD_StudioEventEmitter cinematicAttackSound;

	public FMOD_StudioEventEmitter nonCineAttackSound;

	public GameObject seamothDamageFX;

	public override void OnTouch(Collider collider)
	{
		if (!base.enabled || !liveMixin.IsAlive())
		{
			return;
		}
		GameObject target = GetTarget(collider);
		if (!CanBite(target))
		{
			return;
		}
		CrabSnake component = GetComponent<CrabSnake>();
		Player component2 = target.GetComponent<Player>();
		LiveMixin component3 = target.GetComponent<LiveMixin>();
		if (component2 != null)
		{
			if (component.state != CrabSnake.State.Coil && !component.IsWaterParkCreature())
			{
				float num = DamageSystem.CalculateDamage(biteDamage + cinematicAttackAdditionalDamage, DamageType.Normal, component2.gameObject);
				if (component3.health - num > 0f)
				{
					playerAttackCinematicController.StartCinematicMode(component2);
				}
				else
				{
					playerKillCinematicController.StartCinematicMode(component2);
				}
				component.cinematicMode = true;
				component3.TakeDamage(cinematicAttackAdditionalDamage, default(Vector3), DamageType.Normal, base.gameObject);
				attackSound = cinematicAttackSound;
			}
			else
			{
				attackSound = nonCineAttackSound;
			}
			base.OnTouch(collider);
		}
		else if (target.GetComponent<Vehicle>() != null)
		{
			timeLastBite = Time.time;
			component3.TakeDamage(seamothDamage, default(Vector3), DamageType.Normal, base.gameObject);
			Vector3 position = collider.ClosestPointOnBounds(mouth.transform.position);
			if (seamothDamageFX != null)
			{
				Object.Instantiate(seamothDamageFX, position, seamothDamageFX.transform.rotation);
			}
			Utils.PlayEnvSound(nonCineAttackSound, position);
			component.Aggression.Add(0f - biteAggressionDecrement);
		}
		else
		{
			attackSound = nonCineAttackSound;
			base.OnTouch(collider);
		}
	}

	public void OnPlayerCinematicModeEnd(PlayerCinematicController sender)
	{
		GetComponent<CrabSnake>().cinematicMode = false;
	}
}
