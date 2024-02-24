using UnityEngine;

public class PlayAnimation : CreatureAction
{
	public float actionInterval = 5f;

	public float maxDistanceToFriend = 6f;

	public string[] animationParams;

	private float lastActionTime;

	private string currentAnimation;

	private void Start()
	{
		lastActionTime = Time.time - Random.value * actionInterval;
	}

	private bool CanAnimate(Creature creature)
	{
		if (maxDistanceToFriend > 0f)
		{
			GameObject friend = creature.GetFriend();
			if (friend == null)
			{
				return false;
			}
			Vector3 rhs = ((Player.main.gameObject == friend) ? MainCameraControl.main.transform.forward : friend.transform.forward);
			Vector3 vector = base.transform.position - friend.transform.position;
			if (vector.magnitude > maxDistanceToFriend)
			{
				return false;
			}
			vector = vector.normalized;
			if (Vector3.Dot(vector, rhs) < 0.65f)
			{
				return false;
			}
		}
		return true;
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (time > lastActionTime + actionInterval && CanAnimate(creature))
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		lastActionTime = time;
		currentAnimation = animationParams.GetRandom();
		if (!string.IsNullOrEmpty(currentAnimation))
		{
			SafeAnimator.SetBool(creature.GetAnimator(), currentAnimation, value: true);
		}
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time > lastActionTime + 0.2f)
		{
			if (!string.IsNullOrEmpty(currentAnimation))
			{
				SafeAnimator.SetBool(creature.GetAnimator(), currentAnimation, value: false);
			}
			currentAnimation = "";
		}
	}

	public override void StopPerform(Creature creature, float time)
	{
		if (!string.IsNullOrEmpty(currentAnimation))
		{
			SafeAnimator.SetBool(creature.GetAnimator(), currentAnimation, value: false);
		}
	}
}
