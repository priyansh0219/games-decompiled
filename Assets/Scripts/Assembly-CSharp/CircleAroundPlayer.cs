using UnityEngine;

[RequireComponent(typeof(PlayerDistanceTracker))]
public class CircleAroundPlayer : CreatureAction
{
	public float swimInterval = 1f;

	public float swimForce = 6f;

	public float priority = 0.15f;

	private float direction = 1f;

	private float timeLastFlip;

	private float timeLastSwim;

	private float timeLastInterestedCheck;

	protected PlayerDistanceTracker tracker;

	protected bool interestedInPlayer;

	public override void Awake()
	{
		base.Awake();
		tracker = GetComponent<PlayerDistanceTracker>();
		direction = ((Random.value > 0.5f) ? 1 : (-1));
	}

	public override void StartPerform(Creature creature, float time)
	{
		creature.Happy.Add(1f);
	}

	public override void Perform(Creature b, float time, float deltaTime)
	{
		if (timeLastSwim + swimInterval < time)
		{
			Vector3 vector = Vector3.Normalize(Player.main.transform.position - base.transform.position);
			Vector3 vector2 = (Vector3.Cross(Vector3.up, vector) * direction + vector) * 0.5f;
			if (!Player.main.IsUnderwater())
			{
				vector2.y = 0f;
			}
			GetComponent<Rigidbody>().AddForce(vector2 * swimForce, ForceMode.VelocityChange);
			timeLastSwim = time;
		}
	}

	public virtual bool GetInterestedInPlayer()
	{
		return interestedInPlayer;
	}

	private void Update()
	{
		float time = Time.time;
		if (timeLastFlip + 30f < time)
		{
			if (direction == 1f)
			{
				direction = -1f;
			}
			else
			{
				direction = 1f;
			}
			timeLastFlip = time;
		}
		if (tracker.playerNearby && Player.main.CanBeAttacked())
		{
			if (timeLastInterestedCheck + 6f < time)
			{
				interestedInPlayer = Random.value > 0.3f;
				timeLastInterestedCheck = time;
			}
		}
		else
		{
			interestedInPlayer = false;
		}
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (!GetInterestedInPlayer())
		{
			return 0f;
		}
		return priority;
	}
}
