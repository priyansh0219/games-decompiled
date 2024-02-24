using ProtoBuf;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(SwimBehaviour))]
public class Roost : CreatureAction, IShouldSerialize
{
	private enum State
	{
		None = 0,
		FlyingTo = 1,
		Transition = 2,
		Roosting = 3
	}

	public float flyVelocity = 3f;

	public float flyingTime = 15f;

	public AnimationCurve flyingTimeMultiplier;

	public float roostingTime = 5f;

	public AnimationCurve roostingTimeMultiplier;

	public Vector3 offsetPosition = Vector3.zero;

	public float transitionTime = 3f;

	public float startTransitionDistance = 1.5f;

	private float startRoostingTime;

	private float lastRoostingTime;

	private float minFlyingTime = 4f;

	private float startTime;

	private Vector3 startPos;

	private Quaternion startRot;

	private Vector3 targetPos;

	private Quaternion targetRot;

	private State state;

	private void Start()
	{
		lastRoostingTime = Time.time - GetFlyingTime() * Random.value;
	}

	private float GetFlyingTime()
	{
		return DayNightUtils.Evaluate(flyingTime, flyingTimeMultiplier);
	}

	private float GetRoostingTime()
	{
		return DayNightUtils.Evaluate(roostingTime, roostingTimeMultiplier);
	}

	private Vector3 GetRoostSpot()
	{
		return creature.leashPosition - Vector3.up * 20f;
	}

	public override float Evaluate(Creature creature, float time)
	{
		float num = GetFlyingTime();
		float num2 = GetRoostingTime();
		if (creature.Scared.Value > 0.3f)
		{
			return 0f;
		}
		if (state == State.None)
		{
			if (Time.time <= lastRoostingTime + num)
			{
				return 0f;
			}
			Vector3 direction = GetRoostSpot() - base.transform.position;
			float magnitude = direction.magnitude;
			if (magnitude > 0.5f && Physics.Raycast(base.transform.position, direction, magnitude - 0.5f, Voxeland.GetTerrainLayerMask()))
			{
				return 0f;
			}
		}
		if (state == State.Roosting && num >= minFlyingTime && time > startRoostingTime + num2)
		{
			return 0f;
		}
		return GetEvaluatePriority();
	}

	public override void StartPerform(Creature creature, float time)
	{
		state = State.FlyingTo;
		targetPos = GetRoostSpot();
	}

	public override void StopPerform(Creature creature, float time)
	{
		state = State.None;
		SafeAnimator.SetBool(creature.GetAnimator(), "roosting", value: false);
		SafeAnimator.SetBool(creature.GetAnimator(), "flapping", value: false);
		lastRoostingTime = time;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (state == State.FlyingTo)
		{
			base.swimBehaviour.SwimTo(targetPos, 3f);
		}
	}

	private void Update()
	{
		if (state != 0 && !creature.isActiveAndEnabled)
		{
			StopPerform(creature, Time.time);
		}
	}

	private void LateUpdate()
	{
		switch (state)
		{
		case State.FlyingTo:
		{
			Vector3 roostSpot = GetRoostSpot();
			if (Vector3.Distance(base.transform.position, roostSpot) <= startTransitionDistance)
			{
				state = State.Transition;
				startPos = base.transform.position;
				startRot = base.transform.rotation;
				targetPos = roostSpot + offsetPosition * base.transform.localScale.x;
				if (Physics.Raycast(targetPos, Vector3.down, out var hitInfo, 25f, 1 << LayerMask.NameToLayer("Default"), QueryTriggerInteraction.Ignore) && hitInfo.collider.tag == "DenyBuilding" && targetPos.y - hitInfo.distance > 0.25f)
				{
					targetPos.y = targetPos.y - hitInfo.distance + 0.25f;
				}
				Vector3 euler = new Vector3(0f, base.transform.eulerAngles.y, 0f);
				targetRot = Quaternion.Euler(euler);
				SafeAnimator.SetBool(creature.GetAnimator(), "flapping", value: true);
				startTime = Time.time;
			}
			break;
		}
		case State.Transition:
		{
			float num = (Time.time - startTime) / transitionTime;
			base.transform.position = Vector3.Lerp(startPos, targetPos, num);
			base.transform.rotation = Quaternion.Slerp(startRot, targetRot, num);
			if (num >= 1f)
			{
				state = State.Roosting;
				SafeAnimator.SetBool(creature.GetAnimator(), "roosting", value: true);
				startRoostingTime = Time.time;
			}
			break;
		}
		case State.Roosting:
			base.transform.position = targetPos;
			base.transform.rotation = targetRot;
			break;
		}
	}

	private void OnKill()
	{
		if (state != 0)
		{
			StopPerform(creature, Time.time);
		}
	}

	public bool ShouldSerialize()
	{
		return true;
	}
}
