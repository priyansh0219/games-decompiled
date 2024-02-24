using UnityEngine;

public class TreaderStayAtLeashPosition : CreatureAction
{
	public float velocity = 1f;

	public float updateTargetInterval = 1f;

	private SeaTreader treader;

	private float timeNextMove;

	private void Start()
	{
		treader = GetComponent<SeaTreader>();
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (Vector3.Distance(treader.leashPosition, base.transform.position) > treader.leashDistance)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		timeNextMove = time;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time >= timeNextMove)
		{
			timeNextMove = time + updateTargetInterval;
			treader.MoveTo(treader.leashPosition, velocity);
		}
	}
}
