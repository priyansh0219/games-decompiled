using System;
using UnityEngine;

public class RangedAttackLastTarget : AttackLastTarget
{
	[Serializable]
	public class RangedAttackType
	{
		public float attackChance = 1f;

		public float chargeTime;

		public float castDelay = 1.3f;

		public GameObject ammoPrefab;

		public float ammoVelocity = 5f;

		public int maxProjectiles = 1;

		public float castProjectileInterval = 1f;

		[Range(0f, 1f)]
		public float projectilesSpread;

		public string animParameter;

		public string animChargeParameter;

		public FMODAsset attackSound;

		public FMOD_CustomLoopingEmitter attackLoopSound;

		public VFXController fxControl;

		public int fxIndex = -1;

		public void PlayFX()
		{
			if (fxControl != null)
			{
				if (fxIndex == -1)
				{
					fxControl.Play();
				}
				else if (fxIndex < fxControl.emitters.Length)
				{
					fxControl.Play(fxIndex);
				}
			}
		}

		public void StopFX()
		{
			if (fxControl != null)
			{
				if (fxIndex == -1)
				{
					fxControl.Stop();
				}
				else if (fxIndex < fxControl.emitters.Length)
				{
					fxControl.Stop(fxIndex);
				}
			}
		}
	}

	private enum State
	{
		None = 0,
		Charge = 1,
		Cast = 2
	}

	public bool idleOnCasting = true;

	public float minDistanceToTarget = 10f;

	public float maxCastingDistance = 20f;

	public Transform ammoSpawnPoint;

	public Transform lookDirectionTransform;

	public RangedAttackType[] attackTypes;

	private RangedAttackType currentAttack;

	private State state;

	private float startCastingTime;

	private float ammoSpawnTime;

	private int projectilesCasted;

	public bool isActive { get; private set; }

	protected override bool CanAttackTarget(GameObject target)
	{
		if (base.CanAttackTarget(target))
		{
			if (state == State.None)
			{
				return Vector3.Distance(target.transform.position, base.transform.position) > minDistanceToTarget;
			}
			return true;
		}
		return false;
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (!isActive)
		{
			currentAttack = GetRandomAttack();
		}
		if (currentAttack == null)
		{
			return 0f;
		}
		return base.Evaluate(creature, time);
	}

	public override void StartPerform(Creature creature, float time)
	{
		isActive = true;
		projectilesCasted = 0;
		base.StartPerform(creature, time);
	}

	public override void StopPerform(Creature creature, float time)
	{
		isActive = false;
		StopAttack();
		base.StopPerform(creature, time);
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (currentTarget == null || currentAttack == null)
		{
			StopAttack();
			return;
		}
		if (state == State.None)
		{
			Vector3 position = currentTarget.transform.position;
			Vector3 lookDirection = GetLookDirection();
			Vector3 rhs = Vector3.Normalize(position - ammoSpawnPoint.position);
			if (Vector3.Dot(lookDirection, rhs) > 0.65f && Vector3.Distance(base.transform.position, position) <= maxCastingDistance)
			{
				StartCharging(currentAttack);
			}
		}
		else if (state == State.Charge && time > startCastingTime)
		{
			StartCasting(currentAttack);
		}
		if (state != State.Cast || !idleOnCasting)
		{
			base.Perform(creature, time, deltaTime);
		}
	}

	private void Update()
	{
		if (!isActive || !(currentTarget != null))
		{
			return;
		}
		while (state == State.Cast && Time.time >= ammoSpawnTime)
		{
			Cast(directionToTarget: (!(currentAttack.projectilesSpread > 0f)) ? (currentTarget.transform.position - ammoSpawnPoint.position).normalized : (GetLookDirection() + currentAttack.projectilesSpread * UnityEngine.Random.onUnitSphere).normalized, attackType: currentAttack);
			projectilesCasted++;
			if (projectilesCasted < currentAttack.maxProjectiles)
			{
				ammoSpawnTime += currentAttack.castProjectileInterval;
			}
			else
			{
				StopAttack();
			}
		}
	}

	private Vector3 GetLookDirection()
	{
		if (!(lookDirectionTransform != null))
		{
			return base.transform.forward;
		}
		return lookDirectionTransform.forward;
	}

	private RangedAttackType GetRandomAttack()
	{
		float num = 0f;
		float value = UnityEngine.Random.value;
		for (int i = 0; i < attackTypes.Length; i++)
		{
			num += attackTypes[i].attackChance;
			if (value <= num)
			{
				return attackTypes[i];
			}
		}
		return null;
	}

	private void StartCharging(RangedAttackType attackType)
	{
		if (attackType.chargeTime <= 0f)
		{
			StartCasting(attackType);
			return;
		}
		state = State.Charge;
		startCastingTime = Time.time + attackType.chargeTime;
		if (!string.IsNullOrEmpty(currentAttack.animChargeParameter))
		{
			SafeAnimator.SetBool(creature.GetAnimator(), currentAttack.animChargeParameter, value: true);
		}
	}

	private void StartCasting(RangedAttackType attackType)
	{
		if (idleOnCasting)
		{
			base.swimBehaviour.Idle();
		}
		state = State.Cast;
		ammoSpawnTime = Time.time + attackType.castDelay;
		if (!string.IsNullOrEmpty(currentAttack.animChargeParameter))
		{
			SafeAnimator.SetBool(creature.GetAnimator(), currentAttack.animChargeParameter, value: false);
		}
		if (!string.IsNullOrEmpty(attackType.animParameter))
		{
			SafeAnimator.SetBool(creature.GetAnimator(), attackType.animParameter, value: true);
		}
		attackType.PlayFX();
		if (attackType.attackSound != null)
		{
			Utils.PlayFMODAsset(attackType.attackSound, base.transform);
		}
		if (attackType.attackLoopSound != null)
		{
			attackType.attackLoopSound.Play();
		}
	}

	protected virtual void Cast(RangedAttackType attackType, Vector3 directionToTarget)
	{
		GameObject obj = Utils.SpawnPrefabAt(attackType.ammoPrefab, null, ammoSpawnPoint.position);
		obj.GetComponent<Rigidbody>().AddForce((1f + UnityEngine.Random.Range(-0.05f, 0.05f)) * attackType.ammoVelocity * directionToTarget, ForceMode.VelocityChange);
		obj.SendMessage("OnProjectileCasted", creature, SendMessageOptions.DontRequireReceiver);
	}

	protected override void StopAttack()
	{
		base.StopAttack();
		if (currentAttack != null)
		{
			if (!string.IsNullOrEmpty(currentAttack.animParameter))
			{
				SafeAnimator.SetBool(creature.GetAnimator(), currentAttack.animParameter, value: false);
			}
			if (!string.IsNullOrEmpty(currentAttack.animChargeParameter))
			{
				SafeAnimator.SetBool(creature.GetAnimator(), currentAttack.animChargeParameter, value: false);
			}
			currentAttack.StopFX();
			if (currentAttack.attackLoopSound != null)
			{
				currentAttack.attackLoopSound.Stop();
			}
			currentAttack = null;
		}
		state = State.None;
	}
}
