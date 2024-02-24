using UnityEngine;

[RequireComponent(typeof(CreatureFear))]
public class ScareWhenSee : MonoBehaviour
{
	[SerializeField]
	[AssertNotNull]
	private Creature creature;

	public EcoTargetType targetType;

	public float scarePerSecond = 1f;

	public float maxRangeScalar = 10f;

	private const float kUpdateRate = 0.33f;

	private EcoRegion.TargetFilter isTargetValidFilter;

	private void Start()
	{
		isTargetValidFilter = IsTargetValid;
		InvokeRepeating("ScanForScareTarget", 0f, 0.33f);
	}

	private void ScanForScareTarget()
	{
		if (base.gameObject.activeInHierarchy && EcoRegionManager.main != null)
		{
			IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(targetType, base.transform.position, isTargetValidFilter);
			if (ecoTarget != null)
			{
				float num = Vector3.Distance(ecoTarget.GetPosition(), base.transform.position);
				float num2 = (maxRangeScalar - num) / maxRangeScalar;
				Debug.DrawLine(ecoTarget.GetPosition(), base.transform.position, Color.white);
				creature.Scared.Add(scarePerSecond * num2 * 0.33f);
				base.gameObject.GetComponent<CreatureFear>().SetTarget(ecoTarget.GetGameObject());
			}
		}
	}

	private bool IsTargetValid(IEcoTarget target)
	{
		if ((target.GetPosition() - base.transform.position).sqrMagnitude > maxRangeScalar * maxRangeScalar)
		{
			return false;
		}
		return creature.GetCanSeeObject(target.GetGameObject());
	}
}
