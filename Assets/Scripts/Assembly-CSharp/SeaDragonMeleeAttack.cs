using UWE;
using UnityEngine;

public class SeaDragonMeleeAttack : MeleeAttack
{
	[AssertNotNull]
	public SeaDragon seaDragon;

	[AssertNotNull]
	public PlayerCinematicController playerDeathCinematic;

	[AssertNotNull]
	public RangedAttackLastTarget rangedAttackLastTarget;

	public float swatAttackDamage = 20f;

	public float shoveAttackDamage = 40f;

	public FMODAsset swatAttackSound;

	public FMODAsset clawHitSound;

	public float swatKickVelocity = 20f;

	public float swatKickTorque;

	public float biteAttackChance = 0.25f;

	public Transform swatTargetAttachPoint;

	public float swatAttackInterval = 5f;

	public float swatAttackDuration = 2f;

	public float swatCurrentSpeed = 30f;

	public float swatCurrentRadius = 20f;

	public float swatCurrentLifeTime = 5f;

	private float timeLastSwatAttack;

	private bool isAttacking;

	private GameObject currentTarget;

	private Vector3 targetInitialPos;

	private float timeStartAttack;

	private float timeEndAttack;

	public float pushCyclopsForce = 10f;

	public override bool CanBite(GameObject target)
	{
		if (rangedAttackLastTarget.isActive)
		{
			return false;
		}
		return base.CanBite(target);
	}

	public override bool CanEat(BehaviourType behaviourType, bool holdingByPlayer)
	{
		if (behaviourType != BehaviourType.Shark && behaviourType != BehaviourType.MediumFish)
		{
			return behaviourType == BehaviourType.SmallFish;
		}
		return true;
	}

	public void OnTouchFront(Collider collider)
	{
		if (!liveMixin.IsAlive() || isAttacking || playerDeathCinematic.IsCinematicModeActive() || seaDragon.IsHoldingExosuit() || Time.time < Mathf.Max(timeLastBite, seaDragon.GetTimeExoReleased()) + biteInterval)
		{
			return;
		}
		GameObject target = GetTarget(collider);
		if (!CanBite(target))
		{
			return;
		}
		if (target.GetComponent<SubControl>() != null)
		{
			LiveMixin component = target.GetComponent<LiveMixin>();
			if (component != null)
			{
				component.TakeDamage(shoveAttackDamage, default(Vector3), DamageType.Normal, base.gameObject);
			}
			Rigidbody component2 = target.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				component2.AddForceAtPosition(base.transform.forward * pushCyclopsForce, mouth.transform.position, ForceMode.VelocityChange);
			}
			animator.SetTrigger("shove");
			seaDragon.Aggression.Add(0f - biteAggressionDecrement);
			base.gameObject.SendMessage("OnMeleeAttack", target, SendMessageOptions.DontRequireReceiver);
			timeLastBite = Time.time;
		}
		else
		{
			if (CreatureData.GetBehaviourType(target) != BehaviourType.Shark)
			{
				return;
			}
			if (seaDragon.Aggression.Value >= 0.5f && Random.value < biteAttackChance)
			{
				Player component3 = target.GetComponent<Player>();
				if (component3 != null)
				{
					if (component3.CanBeAttacked() && !component3.cinematicModeActive)
					{
						float num = DamageSystem.CalculateDamage(biteDamage, DamageType.Normal, component3.gameObject);
						if (component3.GetComponent<LiveMixin>().health - num <= 0f)
						{
							timeLastBite = Time.time;
							playerDeathCinematic.StartCinematicMode(component3);
							if (attackSound != null)
							{
								Utils.PlayEnvSound(attackSound, collider.transform.position);
							}
							component3.gameObject.GetComponent<LiveMixin>().TakeDamage(biteDamage, default(Vector3), DamageType.Normal, base.gameObject);
						}
					}
				}
				else
				{
					Exosuit component4 = target.GetComponent<Exosuit>();
					if (component4 != null)
					{
						if (seaDragon.GetCanGrabExosuit() && !component4.docked)
						{
							seaDragon.GrabExosuit(component4);
						}
						return;
					}
				}
				base.OnTouch(collider);
			}
			else
			{
				SwatAttack(target, base.transform.InverseTransformPoint(collider.transform.position).x > 0f);
			}
		}
	}

	public void OnTouchLeft(Collider collider)
	{
		SwatAttack(GetTarget(collider), isRightHand: false);
	}

	public void OnTouchRight(Collider collider)
	{
		SwatAttack(GetTarget(collider), isRightHand: true);
	}

	public void OnThrowExosuit(Exosuit exosuit)
	{
		SwatAttack(exosuit.gameObject, isRightHand: false);
	}

	private void SwatAttack(GameObject target, bool isRightHand)
	{
		if (liveMixin.IsAlive() && !isAttacking && !(Time.time < timeLastSwatAttack + swatAttackInterval) && !playerDeathCinematic.IsCinematicModeActive() && !seaDragon.IsHoldingExosuit() && target != null && CreatureData.GetBehaviourType(target) == BehaviourType.Shark && CanBite(target))
		{
			CaptureSwatTarget(target);
			string trigger = (isRightHand ? "attack_right" : "attack_left");
			animator.SetTrigger(trigger);
			if (swatAttackSound != null)
			{
				Utils.PlayFMODAsset(swatAttackSound, target.transform);
			}
		}
	}

	private void LateUpdate()
	{
		if (isAttacking && Time.time >= timeStartAttack)
		{
			if (currentTarget == null || swatTargetAttachPoint == null || (base.transform.position - swatTargetAttachPoint.position).sqrMagnitude > 2500f || Time.time > timeStartAttack + 4f)
			{
				ReleaseSwatTarget(currentTarget);
				return;
			}
			float t = Mathf.InverseLerp(timeStartAttack, timeEndAttack, Time.time);
			currentTarget.transform.position = Vector3.Lerp(targetInitialPos, swatTargetAttachPoint.position, t);
		}
	}

	public void OnSwatAttackHit()
	{
		if (!isAttacking)
		{
			return;
		}
		ReleaseSwatTarget(currentTarget);
		if (!(currentTarget != null))
		{
			return;
		}
		LiveMixin component = currentTarget.GetComponent<LiveMixin>();
		if (component != null)
		{
			component.TakeDamage(swatAttackDamage, default(Vector3), DamageType.Normal, base.gameObject);
		}
		Rigidbody component2 = currentTarget.GetComponent<Rigidbody>();
		if (component2 != null)
		{
			WorldForces.AddCurrent(swatTargetAttachPoint.position, DayNightCycle.main.timePassed, swatCurrentRadius, swatTargetAttachPoint.forward, swatCurrentSpeed, swatCurrentLifeTime);
			component2.AddForce(swatKickVelocity * swatTargetAttachPoint.forward, ForceMode.VelocityChange);
			if (currentTarget.gameObject != Player.main.gameObject && currentTarget.GetComponent<Exosuit>() == null)
			{
				component2.AddTorque(swatKickTorque * Random.onUnitSphere, ForceMode.VelocityChange);
			}
		}
		if (clawHitSound != null)
		{
			Utils.PlayFMODAsset(clawHitSound, currentTarget.transform);
		}
		base.gameObject.SendMessage("OnMeleeAttack", currentTarget, SendMessageOptions.DontRequireReceiver);
	}

	private void CaptureSwatTarget(GameObject target)
	{
		currentTarget = target;
		targetInitialPos = target.transform.position;
		timeStartAttack = Time.time;
		timeEndAttack = Time.time + swatAttackDuration;
		isAttacking = true;
		Vehicle component = currentTarget.GetComponent<Vehicle>();
		if (component != null)
		{
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(component.GetComponent<Rigidbody>(), isKinematic: true);
			component.collisionModel.SetActive(value: false);
		}
	}

	private void ReleaseSwatTarget(GameObject target)
	{
		if (target != null)
		{
			Vehicle component = currentTarget.GetComponent<Vehicle>();
			if (component != null)
			{
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(component.GetComponent<Rigidbody>(), isKinematic: false);
				component.collisionModel.SetActive(value: true);
			}
		}
		isAttacking = false;
		timeLastSwatAttack = Time.time;
	}

	protected override void OnDisable()
	{
		if (isAttacking)
		{
			ReleaseSwatTarget(currentTarget);
		}
		base.OnDisable();
	}
}
