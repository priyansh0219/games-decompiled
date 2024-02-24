using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(LiveMixin))]
[RequireComponent(typeof(SwimBehaviour))]
[RequireComponent(typeof(LastTarget))]
public class Crash : Creature, IPropulsionCannonAmmo
{
	public enum State
	{
		DetonateOnImpact = 0,
		Resting = 1,
		Agitated = 2,
		Angry = 3,
		Attacking = 4,
		Inflating = 5
	}

	private float inflateRange = 5f;

	private float detonateRadius = 5f;

	private float maxDamage = 50f;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter idleLoop;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter angryLoop;

	[AssertNotNull]
	public FMOD_CustomEmitter attackSound;

	[AssertNotNull]
	public FMOD_CustomEmitter inflateSound;

	public GameObject detonateParticlePrefab;

	[AssertNotNull]
	public Transform model;

	private bool calmingDown;

	private State state = State.Resting;

	private State requestedState = State.Resting;

	public float swimVelocity = 10f;

	public float maxAttackTime = 2f;

	public float calmDownDelay = 1f;

	[AssertNotNull]
	public ProtectCrashHome protectCrashHome;

	[AssertNotNull]
	public FleeWhenScared fleeWhenScared;

	[AssertNotNull]
	public CreatureFear creatureFear;

	[AssertNotNull]
	public SwimBehaviour swimBehaviour;

	[AssertNotNull]
	public Rigidbody useRigidbody;

	[AssertNotNull]
	public LastTarget lastTarget;

	[AssertNotNull]
	public WaterParkCreature waterParkCreature;

	void IPropulsionCannonAmmo.OnGrab()
	{
		CancelInvoke("Inflate");
		CancelInvoke("AnimateInflate");
		CancelInvoke("Detonate");
		CancelInvoke("OnCalmDown");
		OnState(State.Angry, forced: true);
		GetAnimator().SetBool(AnimatorHashID.attacking, value: true);
	}

	void IPropulsionCannonAmmo.OnShoot()
	{
		OnState(State.DetonateOnImpact, forced: true);
	}

	void IPropulsionCannonAmmo.OnRelease()
	{
		lastTarget.SetTarget(Player.main.gameObject);
		AttackLastTarget();
	}

	void IPropulsionCannonAmmo.OnImpact()
	{
		Detonate();
	}

	bool IPropulsionCannonAmmo.GetAllowedToGrab()
	{
		return !IsResting();
	}

	bool IPropulsionCannonAmmo.GetAllowedToShoot()
	{
		return !IsResting();
	}

	public override void Start()
	{
		if (waterParkCreature != null && waterParkCreature.bornInside)
		{
			fleeWhenScared.enabled = true;
			protectCrashHome.enabled = false;
			creatureFear.SetTarget(Player.main.gameObject, float.PositiveInfinity);
			Scared.Add(1f);
			if ((bool)LargeWorldStreamer.main)
			{
				LargeWorldStreamer.main.MakeEntityTransient(base.gameObject);
			}
			UWE.Utils.InvokeOnce(this, Inflate, 20f);
		}
		base.Start();
	}

	public void Update()
	{
		if (waterParkCreature.bornInside)
		{
			Scared.Add(1f);
		}
		else
		{
			if (!IsAttacking())
			{
				return;
			}
			GameObject target = lastTarget.target;
			if ((bool)target)
			{
				Vector3 position = target.transform.position;
				Vector3 vector = -target.transform.forward;
				position -= vector;
				swimBehaviour.Attack(position, vector, swimVelocity);
				if (CloseToPosition(position, inflateRange))
				{
					Inflate();
				}
			}
		}
	}

	public bool IsResting()
	{
		return state == State.Resting;
	}

	private bool IsAttacking()
	{
		return state >= State.Attacking;
	}

	public void AttackLastTarget()
	{
		if (!IsAttacking())
		{
			OnState(State.Attacking);
			Vector3 position = base.transform.position;
			base.transform.position = position + base.transform.forward;
			model.position = position;
			StartCoroutine(UWE.Utils.LerpTransform(model, Vector3.zero, model.localRotation, model.localScale, 0.5f));
			attackSound.Play();
			GetAnimator().SetBool(AnimatorHashID.attacking, value: true);
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(useRigidbody, isKinematic: false);
			useRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
			UWE.Utils.InvokeOnce(this, Inflate, maxAttackTime);
		}
	}

	public void RequestState(State newState)
	{
		if (newState > state)
		{
			StopCalmDown();
			OnState(newState);
		}
		else if (newState < state)
		{
			StartCalmDown(newState);
		}
	}

	private void OnState(State newState, bool forced = false)
	{
		if (newState != state && (state != State.Inflating || forced))
		{
			switch (newState)
			{
			case State.Agitated:
				angryLoop.Stop();
				idleLoop.Play();
				break;
			case State.DetonateOnImpact:
			case State.Angry:
			case State.Attacking:
				idleLoop.Stop();
				angryLoop.Play();
				break;
			default:
				idleLoop.Stop();
				angryLoop.Stop();
				break;
			}
			state = newState;
		}
	}

	private void StartCalmDown(State targetState)
	{
		requestedState = targetState;
		if (!calmingDown)
		{
			calmingDown = true;
			Invoke("OnCalmDown", calmDownDelay);
		}
	}

	private void StopCalmDown()
	{
		calmingDown = false;
		CancelInvoke("OnCalmDown");
	}

	private void OnCalmDown()
	{
		if (requestedState < state)
		{
			OnState(state - 1);
		}
		calmingDown = false;
		if (requestedState < state)
		{
			StartCalmDown(requestedState);
		}
	}

	private void Inflate()
	{
		if (state != State.Inflating)
		{
			OnState(State.Inflating);
			inflateSound.Play();
			UWE.Utils.InvokeOnce(this, AnimateInflate, 2f);
			UWE.Utils.InvokeOnce(this, Detonate, 2.5f);
		}
	}

	private void AnimateInflate()
	{
		SafeAnimator.SetBool(GetAnimator(), "explode", value: true);
	}

	private void Detonate()
	{
		if ((bool)detonateParticlePrefab)
		{
			Utils.PlayOneShotPS(detonateParticlePrefab, base.transform.position, base.transform.rotation);
		}
		DamageSystem.RadiusDamage(maxDamage, base.transform.position, detonateRadius, DamageType.Explosive, base.gameObject);
		base.gameObject.GetComponent<LiveMixin>().Kill();
	}

	private bool CloseToPosition(Vector3 position, float range)
	{
		return (base.transform.position - position).sqrMagnitude < range * range;
	}
}
