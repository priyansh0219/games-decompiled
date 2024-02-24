using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class SwimInSchool : CreatureAction
{
	private float baseRange = 20f;

	private float sizeRangeScalar = 5f;

	private float kBreakDistance = 20f;

	private float percentFindLeaderRespond = 0.5f;

	private float chanceLoseLeader = 0.1f;

	private TechType myTechType;

	private EcoTargetType targetType;

	private Creature _leader;

	public float schoolSize = 2f;

	public float swimVelocity = 4f;

	public float swimInterval = 1f;

	private float timeNextSwim;

	private EcoRegion.TargetFilter isTargetValidFilter;

	private void Start()
	{
		base.gameObject.GetComponent<Creature>();
		isTargetValidFilter = IsValidLeader;
		InvokeRepeating("FindLeader", Random.value, 2f);
		InvokeRepeating("LoseLeader", Random.value, 2f);
		myTechType = CraftData.GetTechType(base.gameObject);
		targetType = CreatureData.GetEcoTargetType(myTechType);
	}

	private bool IsValidLeader(IEcoTarget target)
	{
		GameObject gameObject = target.GetGameObject();
		if (!gameObject)
		{
			return false;
		}
		Creature component = gameObject.GetComponent<Creature>();
		if (!component)
		{
			return false;
		}
		if (CraftData.GetTechType(gameObject) != myTechType)
		{
			return false;
		}
		float num = ((_leader != null) ? _leader.GetSize() : creature.GetSize());
		return component.GetSize() > num;
	}

	private void FindLeader()
	{
		if (base.gameObject.activeInHierarchy && Random.value < percentFindLeaderRespond)
		{
			IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(targetType, base.transform.position, isTargetValidFilter);
			if (ecoTarget != null)
			{
				_leader = ecoTarget.GetGameObject().GetComponent<Creature>();
			}
			else
			{
				_leader = null;
			}
		}
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (_leader != null)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time > timeNextSwim)
		{
			timeNextSwim = time + swimInterval;
			if (_leader != null)
			{
				Vector3 vector = Random.insideUnitSphere * schoolSize;
				Vector3 position = _leader.gameObject.transform.position;
				Vector3 forward = _leader.gameObject.transform.forward;
				Vector3 targetPosition = position + vector;
				targetPosition += _leader.GetComponent<Rigidbody>().velocity * swimInterval;
				base.swimBehaviour.SwimTo(targetPosition, forward, swimVelocity);
			}
		}
	}

	public override void StartPerform(Creature creature, float time)
	{
		creature.Happy.Add(1f);
	}

	public override void StopPerform(Creature creature, float time)
	{
		_leader = null;
	}

	private void LoseLeader()
	{
		if (base.gameObject.activeInHierarchy && _leader != null && ((_leader.gameObject.transform.position - base.gameObject.transform.position).magnitude > kBreakDistance || Random.value < chanceLoseLeader))
		{
			_leader = null;
		}
	}
}
