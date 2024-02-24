using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
[RequireComponent(typeof(EcoTargetDistanceTracker))]
public class Coil : CreatureAction
{
	public float lowHealth = 50f;

	public float targetMinDistance = 10f;

	public float swimVelocity = 5f;

	public float mushroomTimeout = 10f;

	public float pauseInterval = 5f;

	private float lastCoilTime;

	private LiveMixin liveMixin;

	private CrabSnake snakeBehaviour;

	private EcoTargetDistanceTracker tracker;

	public override void OnEnable()
	{
		base.OnEnable();
		liveMixin = GetComponent<LiveMixin>();
		snakeBehaviour = GetComponent<CrabSnake>();
		tracker = GetComponent<EcoTargetDistanceTracker>();
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (liveMixin.health <= lowHealth && tracker.distanceToTarget <= targetMinDistance && time > snakeBehaviour.leaveMushroomTime + mushroomTimeout && time > lastCoilTime + pauseInterval && Player.main.CanBeAttacked())
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		snakeBehaviour.EnterCoil();
	}

	public override void StopPerform(Creature creature, float time)
	{
		snakeBehaviour.ExitCoil();
		lastCoilTime = time;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		Vector3 position = base.transform.position;
		position.y = Player.main.transform.position.y;
		base.swimBehaviour.SwimTo(position, swimVelocity);
	}
}
