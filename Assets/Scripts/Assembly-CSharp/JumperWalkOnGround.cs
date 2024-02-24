using UnityEngine;

public class JumperWalkOnGround : CreatureAction, IManagedUpdateBehaviour, IManagedBehaviour
{
	private Vector3 foodSource;

	[AssertNotNull]
	public Jumper jumper;

	[AssertNotNull]
	public JumperDrift drift;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public OnSurfaceTracker onSurfaceTracker;

	public float swimVelocity = 2f;

	public float swimInterval = 1f;

	public float pauseInterval = 5f;

	public float minWalkTime = 5f;

	public float findFoodChance = 0.5f;

	public float eatHungerDecrement = 0.25f;

	public float eatHappyInrement = 0.25f;

	[AssertNotNull]
	public GameObject walkFX;

	private float timeNextSwim;

	private bool isWalking;

	private bool isDescending;

	private float startWalkTime;

	private float endWalkTime;

	private float timeLastJump;

	private float timeNextJump;

	public float minJumpInterval = 5f;

	public float maxJumpInterval = 8f;

	public float descendHeight = 2f;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "JumperWalkOnGround";
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (!IsGroundLoaded())
		{
			return 0f;
		}
		if (jumper.state == Jumper.State.Walk && time < startWalkTime + minWalkTime)
		{
			return GetEvaluatePriority();
		}
		if (jumper.state == Jumper.State.Drift && (onSurfaceTracker.onSurface || drift.IsTargetReached()))
		{
			return GetEvaluatePriority();
		}
		if (isDescending && foodSource.y > base.transform.position.y)
		{
			return 0f;
		}
		if (creature.Hunger.Value >= 0.5f && time > endWalkTime + pauseInterval && (!isWalking || onSurfaceTracker.onSurface))
		{
			return GetEvaluatePriority() * Mathf.Lerp(0.5f, 1f, creature.Hunger.Value);
		}
		return 0f;
	}

	private void FindFoodSource()
	{
		foodSource = (Random.onUnitSphere - Vector3.up) * 10f + base.transform.position;
		Vector3 direction = foodSource - base.transform.position;
		if (Physics.Raycast(base.transform.position, direction, out var hitInfo, 30f, Voxeland.GetTerrainLayerMask()))
		{
			foodSource = hitInfo.point;
		}
	}

	private void UpdateNextJumpTime(float time)
	{
		timeNextJump = time + Random.Range(minJumpInterval, maxJumpInterval);
	}

	private void Jump(float velocity)
	{
		walkFX.SetActive(value: false);
		animator.SetBool(AnimatorHashID.on_ground, value: false);
		isWalking = false;
		timeLastJump = Time.time;
		base.swimBehaviour.Idle();
		jumper.Jump(velocity);
	}

	public override void StartPerform(Creature creature, float time)
	{
		FindFoodSource();
		isWalking = false;
		if (drift.timeLastDrift == time)
		{
			isDescending = true;
			jumper.state = Jumper.State.Walk;
			base.swimBehaviour.Idle();
		}
		else
		{
			isDescending = false;
		}
		startWalkTime = time;
		BehaviourUpdateUtils.Register(this);
	}

	public override void StopPerform(Creature creature, float time)
	{
		if (isWalking)
		{
			Jump(10f);
		}
		isWalking = false;
		isDescending = false;
		endWalkTime = time;
		jumper.state = Jumper.State.Swim;
		BehaviourUpdateUtils.Deregister(this);
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (isWalking && time > timeNextJump)
		{
			FindFoodSource();
			if (Random.value < findFoodChance)
			{
				creature.Hunger.Add(0f - eatHungerDecrement);
				creature.Happy.Add(eatHappyInrement);
			}
			Jump(5f);
			isDescending = true;
			UpdateNextJumpTime(time);
		}
		else if (!isDescending && time > timeNextSwim)
		{
			timeNextSwim = time + swimInterval;
			Vector3 targetPosition = new Vector3(foodSource.x, base.transform.position.y - 1f, foodSource.z);
			base.swimBehaviour.SwimTo(targetPosition, swimVelocity);
		}
	}

	public void ManagedUpdate()
	{
		if (!IsGroundLoaded())
		{
			jumper.SwimRandom();
			return;
		}
		if (!isWalking && onSurfaceTracker.onSurface && Time.time > timeLastJump + 1f)
		{
			isWalking = true;
			isDescending = false;
			UpdateNextJumpTime(Time.time);
			animator.SetBool(AnimatorHashID.on_ground, value: true);
			walkFX.SetActive(value: true);
		}
		if (jumper.state != Jumper.State.Walk && (isWalking || (base.transform.position - foodSource).sqrMagnitude < descendHeight * descendHeight))
		{
			jumper.state = Jumper.State.Walk;
			isDescending = !isWalking;
		}
	}

	private bool IsGroundLoaded()
	{
		if (LargeWorldStreamer.main != null)
		{
			return LargeWorldStreamer.main.IsRangeActiveAndBuilt(new Bounds(base.transform.position, Vector3.zero));
		}
		return false;
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}
}
