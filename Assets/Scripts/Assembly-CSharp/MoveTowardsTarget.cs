using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class MoveTowardsTarget : CreatureAction
{
	public EcoTargetType targetType = EcoTargetType.Coral;

	public float scanInterval = 5f;

	public float moveSpeed = 5f;

	public float swimInterval = 1f;

	public bool fleeInstead;

	public float requiredAggression;

	public float minDistanceToTarget;

	public float chanceToLoseTarget = 0.1f;

	private float timeNextScan;

	private float timeNextSwim;

	private IEcoTarget currentTarget;

	private EcoRegion.TargetFilter isTargetValidFilter;

	private void Start()
	{
		InvokeRepeating("LoseTarget", Random.value, 1f);
		if (isTargetValidFilter == null)
		{
			isTargetValidFilter = IsValidTarget;
		}
	}

	private bool IsValidTarget(IEcoTarget target)
	{
		Vector3 direction = target.GetPosition() - base.transform.position;
		if (!fleeInstead && direction.sqrMagnitude <= minDistanceToTarget * minDistanceToTarget)
		{
			return false;
		}
		float num = direction.magnitude - 0.5f;
		if (num > 0f && Physics.Raycast(base.transform.position, direction, num, Voxeland.GetTerrainLayerMask()))
		{
			return false;
		}
		return true;
	}

	private void UpdateCurrentTarget()
	{
		if (EcoRegionManager.main != null && (Mathf.Approximately(requiredAggression, 0f) || creature.Aggression.Value >= requiredAggression))
		{
			IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(targetType, base.transform.position, isTargetValidFilter);
			if (ecoTarget != null)
			{
				currentTarget = ecoTarget;
			}
			else
			{
				currentTarget = null;
			}
		}
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (timeNextScan < time)
		{
			UpdateCurrentTarget();
			timeNextScan = time + scanInterval;
		}
		if (currentTarget != null && !currentTarget.Equals(null))
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (currentTarget != null && !currentTarget.Equals(null) && time > timeNextSwim)
		{
			if (!fleeInstead && (currentTarget.GetPosition() - base.transform.position).sqrMagnitude <= minDistanceToTarget * minDistanceToTarget)
			{
				currentTarget = null;
				return;
			}
			timeNextSwim = time + swimInterval;
			Vector3 vector = currentTarget.GetPosition() - base.transform.position;
			Vector3 targetPosition = ((!fleeInstead) ? (currentTarget.GetPosition() - vector.normalized * minDistanceToTarget) : (base.transform.position - vector));
			base.swimBehaviour.SwimTo(targetPosition, moveSpeed);
		}
	}

	private void LoseTarget()
	{
		if (base.gameObject.activeInHierarchy && currentTarget != null && Random.value < chanceToLoseTarget)
		{
			currentTarget = null;
		}
	}

	public override string GetDebugString()
	{
		string text = base.GetDebugString();
		if (currentTarget != null && !currentTarget.Equals(null))
		{
			text = $"{text}: {currentTarget.GetName()}";
		}
		return text;
	}
}
