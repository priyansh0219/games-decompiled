using UnityEngine;

public class Scareable : MonoBehaviour, IScheduledUpdateBehaviour, IManagedBehaviour
{
	public EcoTargetType targetType = EcoTargetType.Shark;

	[AssertNotNull]
	public CreatureFear creatureFear;

	[AssertNotNull]
	public Creature creature;

	[SerializeField]
	private CreatureAction fleeAction;

	public float scarePerSecond = 4f;

	public float maxRangeScalar = 10f;

	public float minMass = 50f;

	public float updateTargetInterval = 1f;

	public float updateRange = 100f;

	[SerializeField]
	private AnimationCurve daynightRangeMultiplier;

	private float timeNextSearch;

	private EcoRegion.TargetFilter isTargetValidFilter;

	public int scheduledUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "Scareable";
	}

	private void Awake()
	{
		timeNextSearch = updateTargetInterval + (Random.value * 2f - 1f) * updateTargetInterval * 0.5f;
		if (isTargetValidFilter == null)
		{
			isTargetValidFilter = IsTargetValid;
		}
	}

	public void ScheduledUpdate()
	{
		float time = Time.time;
		if (!(time > timeNextSearch) || !((Player.main.transform.position - base.transform.position).sqrMagnitude < updateRange * updateRange))
		{
			return;
		}
		timeNextSearch = time + updateTargetInterval;
		IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(targetType, base.transform.position, isTargetValidFilter);
		if (ecoTarget == null)
		{
			return;
		}
		GameObject gameObject = ecoTarget.GetGameObject();
		if (!(gameObject == null) && !(gameObject.GetComponent<Rigidbody>() == null))
		{
			float sqrMagnitude = (base.transform.position - ecoTarget.GetPosition()).sqrMagnitude;
			float num = DayNightUtils.Evaluate(maxRangeScalar, daynightRangeMultiplier);
			float num2 = 1f - sqrMagnitude / (num * num);
			float amount = scarePerSecond * num2 * updateTargetInterval;
			creature.Scared.Add(amount);
			creatureFear.SetTarget(gameObject);
			if (fleeAction != null)
			{
				creature.TryStartAction(fleeAction);
			}
		}
	}

	private bool IsTargetValid(IEcoTarget ecoTarget)
	{
		GameObject gameObject = ecoTarget.GetGameObject();
		if (gameObject == Player.main.gameObject && !Player.main.CanBeAttacked())
		{
			return false;
		}
		if (creature.IsFriendlyTo(gameObject))
		{
			return false;
		}
		if ((ecoTarget.GetPosition() - base.transform.position).sqrMagnitude > maxRangeScalar * maxRangeScalar)
		{
			return false;
		}
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		if (!component || component.mass < minMass)
		{
			return false;
		}
		return true;
	}

	private void OnEnable()
	{
		UpdateSchedulerUtils.Register(this);
	}

	private void OnDisable()
	{
		UpdateSchedulerUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		UpdateSchedulerUtils.Deregister(this);
	}
}
