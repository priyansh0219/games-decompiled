using UnityEngine;

public class WarperInspectPlayer : CreatureAction
{
	[AssertNotNull]
	public Warper warper;

	public float swimVelocity = 3f;

	public float swimInterval = 1f;

	public float maxDistance = 30f;

	public float inspectDistance = 10f;

	public float warpOutDistance = 3f;

	private float timeNextSwim;

	public float backVel = 3f;

	public float backDir = 0.9f;

	private bool GetCanInspect(GameObject target)
	{
		if (Vector3.Distance(target.transform.position, base.transform.position) > maxDistance)
		{
			return false;
		}
		if (!warper.GetCanSeeObject(target))
		{
			return false;
		}
		InfectedMixin component = target.GetComponent<InfectedMixin>();
		if (component != null && component.GetInfectedAmount() > 0.33f)
		{
			return false;
		}
		return true;
	}

	public override float Evaluate(Creature creature, float time)
	{
		Player main = Player.main;
		if (main != null && main.CanBeAttacked() && GetCanInspect(main.gameObject))
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		base.swimBehaviour.LookAt(Player.main.transform);
	}

	public override void StopPerform(Creature creature, float time)
	{
		base.swimBehaviour.LookForward();
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		Transform transform = Player.main.transform;
		Vector3 vector = transform.position - base.transform.position;
		float magnitude = vector.magnitude;
		if (magnitude < warpOutDistance)
		{
			warper.WarpOut();
		}
		else if (time > timeNextSwim)
		{
			if (magnitude > inspectDistance)
			{
				base.swimBehaviour.SwimTo(transform.position, vector.normalized, swimVelocity);
			}
			else
			{
				base.swimBehaviour.SwimTo(base.transform.position - base.transform.forward * backDir, vector.normalized, backVel);
			}
		}
	}
}
