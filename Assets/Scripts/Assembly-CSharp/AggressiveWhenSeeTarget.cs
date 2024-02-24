using UnityEngine;

[RequireComponent(typeof(LastTarget))]
public class AggressiveWhenSeeTarget : MonoBehaviour
{
	[AssertNotNull]
	public AnimationCurve maxRangeMultiplier;

	[AssertNotNull]
	public AnimationCurve distanceAggressionMultiplier;

	[AssertNotNull]
	public LastTarget lastTarget;

	[SerializeField]
	[AssertNotNull]
	private Creature creature;

	public EcoTargetType targetType;

	public float aggressionPerSecond = 1f;

	public float maxRangeScalar = 10f;

	public int maxSearchRings = 1;

	public bool ignoreSameKind = true;

	public bool targetShouldBeInfected;

	public float minimumVelocity;

	public float hungerThreshold;

	public FMOD_StudioEventEmitter sightedSound;

	private const float kUpdateRate = 1f;

	private TechType myTechType;

	private EcoRegion.TargetFilter isTargetValidFilter;

	private void Start()
	{
		myTechType = CreatureData.GetCreatureType(base.gameObject);
		isTargetValidFilter = IsTargetValid;
		InvokeRepeating("ScanForAggressionTarget", Random.Range(0f, 1f), 1f);
	}

	private void ScanForAggressionTarget()
	{
		if (!base.gameObject.activeInHierarchy || !base.enabled || (creature != null && creature.Hunger.Value < hungerThreshold) || EcoRegionManager.main == null)
		{
			return;
		}
		GameObject aggressionTarget = GetAggressionTarget();
		if (!(aggressionTarget != null))
		{
			return;
		}
		float num = Vector3.Distance(aggressionTarget.transform.position, base.transform.position);
		float num2 = DayNightUtils.Evaluate(maxRangeScalar, maxRangeMultiplier);
		float time = (num2 - num) / num2;
		float num3 = distanceAggressionMultiplier.Evaluate(time);
		float num4 = 1f;
		if (targetShouldBeInfected)
		{
			InfectedMixin component = aggressionTarget.GetComponent<InfectedMixin>();
			num4 = ((component != null) ? component.infectedAmount : 0f);
		}
		float num5 = aggressionPerSecond * num3 * num4 * 1f;
		if (num5 > 0f)
		{
			Debug.DrawLine(aggressionTarget.transform.position, base.transform.position, Color.white);
			creature.Aggression.Add(num5);
			lastTarget.SetTarget(aggressionTarget);
			if (sightedSound != null && !sightedSound.GetIsPlaying())
			{
				Debug.Log("Not playing sighted sound, starting " + Time.time);
				sightedSound.StartEvent();
			}
		}
	}

	protected virtual GameObject GetAggressionTarget()
	{
		return EcoRegionManager.main.FindNearestTarget(targetType, base.transform.position, isTargetValidFilter, maxSearchRings)?.GetGameObject();
	}

	private bool IsTargetValid(IEcoTarget target)
	{
		return IsTargetValid(target.GetGameObject());
	}

	protected bool IsTargetValid(GameObject target)
	{
		if (target == null)
		{
			return false;
		}
		if (creature.IsFriendlyTo(target))
		{
			return false;
		}
		if (target == Player.main.gameObject && !Player.main.CanBeAttacked())
		{
			return false;
		}
		if (GameModeUtils.IsInvisible() && target.GetComponent<Vehicle>() != null)
		{
			return false;
		}
		if (ignoreSameKind && CreatureData.GetCreatureType(target) == myTechType)
		{
			return false;
		}
		if (targetShouldBeInfected)
		{
			InfectedMixin component = target.GetComponent<InfectedMixin>();
			if (component == null || component.GetInfectedAmount() < 0.33f)
			{
				return false;
			}
		}
		if (Vector3.Distance(target.transform.position, base.transform.position) > maxRangeScalar)
		{
			return false;
		}
		if (!Mathf.Approximately(minimumVelocity, 0f))
		{
			Rigidbody componentInChildren = target.GetComponentInChildren<Rigidbody>();
			if (componentInChildren != null && componentInChildren.velocity.magnitude <= minimumVelocity)
			{
				return false;
			}
		}
		return creature.GetCanSeeObject(target);
	}
}
