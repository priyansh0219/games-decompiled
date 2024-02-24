using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class FlyAwayWhenScared : CreatureAction
{
	[AssertNotNull]
	public CreatureFear creatureFear;

	public float flyVelocity = 5f;

	public float flyInterval = 2f;

	public float flyUp = 0.3f;

	public float flyFromSource = 1.3f;

	public float scaredThreshold = 0.5f;

	private float timeNextFly;

	public override float Evaluate(Creature creature, float time)
	{
		if (creature.Scared.Value > scaredThreshold && creatureFear.lastScarePosition.y > 0f)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		timeNextFly = time;
		SafeAnimator.SetBool(creature.GetAnimator(), "flapping", value: true);
	}

	public override void StopPerform(Creature creature, float time)
	{
		SafeAnimator.SetBool(creature.GetAnimator(), "flapping", value: false);
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time >= timeNextFly)
		{
			timeNextFly = time + flyInterval;
			Vector3 lastScarePosition = creatureFear.lastScarePosition;
			Vector3 normalized = (base.transform.position - lastScarePosition).normalized;
			Vector3 insideUnitSphere = Random.insideUnitSphere;
			insideUnitSphere += normalized * flyFromSource;
			insideUnitSphere.y = flyUp;
			Vector3 targetPosition = base.transform.position + insideUnitSphere * 10f;
			base.swimBehaviour.SwimTo(targetPosition, flyVelocity);
		}
	}

	public void OnFearTriggerEnter(Collider collider)
	{
		if (base.enabled)
		{
			Rigidbody attachedRigidbody = collider.attachedRigidbody;
			GameObject gameObject = ((attachedRigidbody != null) ? attachedRigidbody.gameObject : collider.gameObject);
			bool flag = false;
			Player main = Player.main;
			if (gameObject == main.gameObject)
			{
				flag = !main.IsInsideWalkable() && !GameModeUtils.IsInvisible();
			}
			else if (gameObject.GetComponent<Creature>() != null)
			{
				flag = CreatureData.GetBehaviourType(gameObject) == BehaviourType.Shark;
			}
			if (flag)
			{
				creatureFear.SetTarget(gameObject);
				creature.Scared.Add(1f);
				creature.TryStartAction(this);
			}
		}
	}
}
