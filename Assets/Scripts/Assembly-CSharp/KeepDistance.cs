using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KeepDistance : MonoBehaviour
{
	public float minDistance = 1f;

	public float force = 15f;

	public float updateTargetTime = 1f;

	private TechType myTechType;

	private EcoTargetType targetType;

	private GameObject nearestTarget;

	private Rigidbody rig;

	private EcoRegion.TargetFilter isTargetValidFilter;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		InvokeRepeating("FindNearestTarget", Random.Range(0f, updateTargetTime), updateTargetTime);
		isTargetValidFilter = IsValidTarget;
	}

	private void FixedUpdate()
	{
		if (nearestTarget != null)
		{
			Vector3 vector = base.transform.position - nearestTarget.transform.position;
			if (vector.sqrMagnitude < minDistance * minDistance)
			{
				Vector3 normalized = vector.normalized;
				rig.AddForce(normalized * force);
			}
		}
	}

	private void FindNearestTarget()
	{
		nearestTarget = null;
		IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(targetType, base.transform.position, isTargetValidFilter);
		if (ecoTarget != null)
		{
			nearestTarget = ecoTarget.GetGameObject();
		}
	}

	private bool IsValidTarget(IEcoTarget target)
	{
		GameObject gameObject = target.GetGameObject();
		if (gameObject == null || gameObject == base.gameObject)
		{
			return false;
		}
		return CraftData.GetTechType(gameObject) == myTechType;
	}
}
