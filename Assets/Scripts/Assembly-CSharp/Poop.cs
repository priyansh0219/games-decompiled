using UnityEngine;

public class Poop : CreatureAction
{
	public SeaTreader treader;

	public string animationParameterName;

	public float actionInterval = 100f;

	public float animationDuration = 5.3f;

	public GameObject recourcePrefab;

	public Transform recourceSpawnPoint;

	public float spawnDelay;

	private bool isActive;

	private float nextActionTime;

	private float endActionTime;

	private bool recourceSpawned;

	private float recourceSpawnTime;

	public override void Awake()
	{
		base.Awake();
		nextActionTime = Time.time + Random.value * actionInterval;
	}

	public override float Evaluate(Creature creature, float time)
	{
		if ((!isActive && time >= nextActionTime && !treader.cinematicMode) || (isActive && time < endActionTime))
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		isActive = true;
		endActionTime = time + animationDuration;
		recourceSpawned = false;
		recourceSpawnTime = time + spawnDelay;
		treader.cinematicMode = true;
		treader.Idle();
		SafeAnimator.SetBool(creature.GetAnimator(), animationParameterName, value: true);
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (!recourceSpawned && time >= recourceSpawnTime)
		{
			SafeAnimator.SetBool(creature.GetAnimator(), animationParameterName, value: false);
			recourceSpawned = true;
			Object.Instantiate(recourcePrefab, recourceSpawnPoint.position, recourceSpawnPoint.rotation);
		}
	}

	public override void StopPerform(Creature creature, float time)
	{
		isActive = false;
		nextActionTime = time + actionInterval * (1f + 0.2f * Random.value);
		treader.cinematicMode = false;
	}
}
