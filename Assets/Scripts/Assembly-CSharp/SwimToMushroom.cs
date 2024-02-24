using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class SwimToMushroom : CreatureAction
{
	public float maxSwimTime = 10f;

	public float swimToVelocity = 6f;

	public float startEnterDistance = 1.5f;

	private Vector3 mushroomPosition;

	private CrabSnake crabsnake;

	public override void OnEnable()
	{
		base.OnEnable();
		crabsnake = GetComponent<CrabSnake>();
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (crabsnake.HasMushroomToHide() && time > crabsnake.leaveMushroomTime + maxSwimTime)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (crabsnake.HasMushroomToHide())
		{
			mushroomPosition = crabsnake.GetSwimToMushroomPosition();
			base.swimBehaviour.SwimTo(mushroomPosition, swimToVelocity);
			if (Vector3.Distance(base.transform.position, mushroomPosition) <= startEnterDistance)
			{
				crabsnake.EnterMushroom();
			}
		}
	}
}
