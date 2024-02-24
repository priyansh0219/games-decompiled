using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
[RequireComponent(typeof(CreatureFear))]
public class FleeWhenScared : CreatureAction
{
	[AssertNotNull]
	public CreatureFear creatureFear;

	[SerializeField]
	private FMOD_StudioEventEmitter scaredSound;

	[Header("Swim configuration")]
	public float swimVelocity = 10f;

	[SerializeField]
	private float swimInterval = 1f;

	[SerializeField]
	private int avoidanceIterations = 10;

	[Header("Creature Tired trait")]
	[SerializeField]
	private float swimTiredness = 0.2f;

	[SerializeField]
	private float tiredVelocity = 3f;

	[Header("Flee exhaustion")]
	[SerializeField]
	[AssertNotNull]
	private CreatureTrait exhausted;

	[SerializeField]
	private float swimExhaustion = 0.25f;

	[SerializeField]
	private float exhaustedVelocity = 1f;

	private float timeNextSwim;

	private float timeLastAction;

	private Vector3 fleeDirectionModifier = Vector3.zero;

	public bool active { get; private set; }

	public override float Evaluate(Creature creature, float time)
	{
		Vector3 position = base.transform.position;
		float range = creatureFear.range;
		if ((creatureFear.lastScarePosition - position).sqrMagnitude > range * range)
		{
			return 0f;
		}
		return GetEvaluatePriority() * creature.Scared.Value;
	}

	private void Flee()
	{
		float time = Time.time;
		if (!(time > timeNextSwim))
		{
			return;
		}
		timeNextSwim = time + swimInterval;
		Vector3 position = base.transform.position;
		Vector3 lastScarePosition = creatureFear.lastScarePosition;
		Vector3 vector = position - lastScarePosition;
		float range = creatureFear.range;
		if ((position - lastScarePosition).sqrMagnitude > range * range)
		{
			return;
		}
		if (position.y > 0f - swimVelocity && vector.y > 0f)
		{
			vector.y = 0f;
		}
		else
		{
			vector.y *= 0.5f;
		}
		vector = vector.normalized;
		Vector3 normalized = (vector + fleeDirectionModifier).normalized;
		Vector3 targetPosition = position + vector * swimVelocity;
		for (int i = 0; i < avoidanceIterations; i++)
		{
			if (!Physics.Raycast(position, normalized, swimVelocity, -5, QueryTriggerInteraction.Ignore))
			{
				targetPosition = position + normalized * swimVelocity;
				break;
			}
			fleeDirectionModifier = Random.onUnitSphere;
			normalized = (vector + fleeDirectionModifier).normalized;
		}
		float a = Mathf.Lerp(swimVelocity, tiredVelocity, creature.Tired.Value);
		float b = Mathf.Lerp(swimVelocity, exhaustedVelocity, exhausted.Value);
		float velocity = Mathf.Min(a, b);
		base.swimBehaviour.SwimTo(targetPosition, velocity);
		exhausted.Add(swimExhaustion * swimInterval);
		creature.Tired.Add(swimTiredness * swimInterval);
		if (scaredSound != null && !scaredSound.GetIsPlaying())
		{
			scaredSound.StartEvent();
		}
	}

	public override void StartPerform(Creature creature, float time)
	{
		active = true;
		fleeDirectionModifier = Vector3.zero;
		exhausted.UpdateTrait(time - timeLastAction);
	}

	public override void StopPerform(Creature creature, float time)
	{
		active = false;
		timeLastAction = time;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		Flee();
		exhausted.UpdateTrait(deltaTime);
	}
}
